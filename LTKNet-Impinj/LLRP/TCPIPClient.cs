using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;


namespace Org.LLRP.LTK.LLRPV1
{
    /// <summary>
    /// TCPIPClient, used for building LLRPClient
    /// </summary>
    internal class TCPIPClient : CommunicationInterface
    {
        private TcpClient tcp_client;
        private Stream network_stream;
        private object syn_msg = new object();
        private TCPIPClient.ENUM_INTERNAL_CONNECTION_STATUS connectionStatus;
        private const int LLRP_HEADER_SIZE = 10;
        private TCPIPClient.EMessageProcessingState message_state;
        private uint msg_cursor;
        private byte[] msg_header_storage = new byte[10];
        private byte[] msg_data;
        private const int BUFFER_SIZE = 2048;
        private int buffer_cursor;
        private int buffer_bytes_available;
        private byte[] buffer;
        private short msg_ver;
        private short msg_type;
        private int msg_len;
        private int msg_id;
        private ManualResetEvent non_block_tcp_connection_evt;

        public override void Close()
        {
            bool flag = false;
            TCPIPClient.ENUM_INTERNAL_CONNECTION_STATUS connectionStatus = this.connectionStatus;
            lock (this.syn_msg)
            {
                if (connectionStatus != TCPIPClient.ENUM_INTERNAL_CONNECTION_STATUS.DISCONNECTED)
                {
                    this.connectionStatus = TCPIPClient.ENUM_INTERNAL_CONNECTION_STATUS.DISCONNECTING;
                    flag = true;
                }
            }
            if (flag)
                new ManualResetEvent(false).WaitOne(100, false);
            lock (this.syn_msg)
            {
                if (this.network_stream != null)
                {
                    this.network_stream.Close();
                    this.network_stream = (Stream)null;
                }
                if (this.tcp_client != null)
                {
                    this.tcp_client.Close();
                    this.tcp_client = (TcpClient)null;
                }
                this.connectionStatus = TCPIPClient.ENUM_INTERNAL_CONNECTION_STATUS.DISCONNECTED;
            }
            if (connectionStatus != TCPIPClient.ENUM_INTERNAL_CONNECTION_STATUS.CONNECTED)
                return;
            this.TriggerOnClientConnectionStatusChange(ENUM_CONNECTION_STATUS.DISCONNECTED);
        }

        public override bool Open(string device_name, int port) => this.Open(device_name, port, 0);

        public override bool Open(string device_name, int port, bool useTLS)
        {
            return this.Open(device_name, port, 0, useTLS);
        }

        public override bool Open(string device_name, int port, int timeout)
        {
            return this.Open(device_name, port, timeout, false);
        }

        public override bool Open(string device_name, int port, int timeout, bool useTLS)
        {
            return this.Open(device_name, port, timeout, useTLS, LLRPClient.TlsProtocols.OsDefault);
        }

        public override bool Open(
          string device_name,
          int port,
          int timeout,
          bool useTLS,
          LLRPClient.TlsProtocols tlsProtocol)
        {
            lock (this.syn_msg)
            {
                if (this.connectionStatus != TCPIPClient.ENUM_INTERNAL_CONNECTION_STATUS.DISCONNECTED)
                    return false;
                this.tcp_client = new TcpClient(AddressFamily.InterNetworkV6);
                this.tcp_client.Client.DualMode = true;
                IPAddress[] hostAddresses;
                try
                {
                    hostAddresses = Dns.GetHostAddresses(device_name);
                }
                catch (Exception ex)
                {
                    return false;
                }
                this.non_block_tcp_connection_evt = new ManualResetEvent(false);
                try
                {
                    this.tcp_client.BeginConnect(hostAddresses, port, new AsyncCallback(this.NonBlockTCPConnectionCallback), (object)this.tcp_client);
                }
                catch (Exception ex)
                {
                    this.Close();
                    return false;
                }
                this.network_stream = (Stream)null;
                if (this.non_block_tcp_connection_evt.WaitOne(timeout, false) && this.tcp_client.Connected)
                {
                    if (useTLS)
                        this.InitializeSslStream(device_name, tlsProtocol);
                    else
                        this.network_stream = (Stream)this.tcp_client.GetStream();
                    try
                    {
                        this.InitializeMessageProcessing();
                        this.StartNewBufferReceive();
                    }
                    catch (InvalidOperationException ex)
                    {
                        this.Close();
                        return false;
                    }
                    catch (IOException ex)
                    {
                        this.Close();
                        return false;
                    }
                    this.connectionStatus = TCPIPClient.ENUM_INTERNAL_CONNECTION_STATUS.CONNECTED;
                }
                else
                {
                    this.Close();
                    return false;
                }
            }
            this.TriggerOnClientConnectionStatusChange(ENUM_CONNECTION_STATUS.CONNECTED);
            return true;
        }

        private void InitializeSslStream(string device_name, LLRPClient.TlsProtocols tlsProtocolToUse)
        {
            SslProtocols sslProtocol = this.GetSslProtocol(tlsProtocolToUse);
            NetworkStream stream = this.tcp_client.GetStream();
            SslProtocols sslProtocolsToUse = sslProtocol == SslProtocols.None ? sslProtocol : sslProtocol | SslProtocols.Tls12;
            RemoteCertificateValidationCallback userCertificateValidationCallback = new RemoteCertificateValidationCallback(TCPIPClient.ValidateServerCertificateCallback);
            this.network_stream = (Stream)new TlsStream(stream, false, userCertificateValidationCallback, sslProtocolsToUse);
            try
            {
                if (sslProtocol == SslProtocols.Tls12)
                    ((SslStream)this.network_stream).AuthenticateAsClient(device_name, (X509CertificateCollection)null, SslProtocols.Tls12, false);
                else if (sslProtocol == SslProtocols.Tls13)
                    ((SslStream)this.network_stream).AuthenticateAsClient(device_name);
                else
                    ((SslStream)this.network_stream).AuthenticateAsClient(device_name);
            }
            catch (AuthenticationException ex)
            {
                this.tcp_client.Close();
            }
            catch (Exception ex)
            {
                this.tcp_client.Close();
            }
        }

        private SslProtocols GetSslProtocol(LLRPClient.TlsProtocols tlsProtocol)
        {
            SslProtocols sslProtocol = SslProtocols.None;
            switch (tlsProtocol)
            {
                case LLRPClient.TlsProtocols.OsDefault:
                    sslProtocol = SslProtocols.None;
                    break;
                case LLRPClient.TlsProtocols.Tls12:
                    sslProtocol = SslProtocols.Tls12;
                    break;
                case LLRPClient.TlsProtocols.Tls13:
                    sslProtocol = SslProtocols.Tls13;
                    break;
            }
            return sslProtocol;
        }

        public static bool ValidateServerCertificateCallback(
          object sender,
          X509Certificate certificate,
          X509Chain chain,
          SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private void NonBlockTCPConnectionCallback(IAsyncResult ar)
        {
            this.non_block_tcp_connection_evt.Set();
            if (!(ar.AsyncState is TcpClient asyncState) || asyncState.Client == null || !asyncState.Connected)
                return;
            asyncState.EndConnect(ar);
        }

        private void InitializeMessageProcessing()
        {
            this.ReInitializeMessageProcessing();
            this.message_state = TCPIPClient.EMessageProcessingState.MESSAGE_HEADER;
        }

        private void ReInitializeMessageProcessing()
        {
            Array.Clear((Array)this.msg_header_storage, 0, 10);
            this.msg_cursor = 0U;
            this.msg_data = (byte[])null;
            this.msg_ver = (short)0;
            this.msg_type = (short)0;
            this.msg_len = 0;
            this.msg_id = 0;
        }

        private void StartNewBufferReceive()
        {
            this.buffer_cursor = 0;
            this.buffer_bytes_available = 0;
            this.buffer = new byte[2048];
            this.network_stream.Flush();
            this.network_stream.BeginRead(this.buffer, this.buffer_cursor, 2048 - this.buffer_cursor, new AsyncCallback(this.OnDataRead), (object)this.message_state);
        }

        private void importAndQualifyHeader()
        {
            int num = ((int)this.msg_header_storage[0] << 8) + (int)this.msg_header_storage[1];
            this.msg_type = (short)(num & 1023);
            this.msg_ver = (short)(num >> 10 & 7);
            this.msg_len = ((int)this.msg_header_storage[2] << 24) + ((int)this.msg_header_storage[3] << 16) + ((int)this.msg_header_storage[4] << 8) + (int)this.msg_header_storage[5];
            this.msg_id = ((int)this.msg_header_storage[6] << 24) + ((int)this.msg_header_storage[7] << 16) + ((int)this.msg_header_storage[8] << 8) + (int)this.msg_header_storage[9];
        }

        private void OnDataRead(IAsyncResult ar)
        {
            if (ar.CompletedSynchronously)
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.ProcessReceivedData), (object)ar);
            else
                this.ProcessReceivedData((object)ar);
        }

        private void ProcessReceivedData(object stateInfo)
        {
            IAsyncResult asyncResult = (IAsyncResult)stateInfo;
            lock (this.syn_msg)
            {
                if (this.connectionStatus != TCPIPClient.ENUM_INTERNAL_CONNECTION_STATUS.CONNECTED)
                    return;
                try
                {
                    this.buffer_bytes_available += this.network_stream.EndRead(asyncResult);
                }
                catch (InvalidOperationException ex)
                {
                    this.Close();
                    return;
                }
                catch (IOException ex)
                {
                    this.Close();
                    return;
                }
                while (this.buffer_bytes_available > 0)
                {
                    switch (this.message_state)
                    {
                        case TCPIPClient.EMessageProcessingState.MESSAGE_UNKNOWN:
                            throw new Exception("Unexpected receive message state");
                        case TCPIPClient.EMessageProcessingState.MESSAGE_HEADER:
                            int length1 = (int)Math.Min((long)(10U - this.msg_cursor), (long)this.buffer_bytes_available);
                            Array.Copy((Array)this.buffer, (long)this.buffer_cursor, (Array)this.msg_header_storage, (long)this.msg_cursor, (long)length1);
                            this.msg_cursor += (uint)length1;
                            this.buffer_cursor += length1;
                            this.buffer_bytes_available -= length1;
                            if (this.msg_cursor == 10U)
                            {
                                this.importAndQualifyHeader();
                                this.msg_data = new byte[this.msg_len];
                                Array.Copy((Array)this.msg_header_storage, (Array)this.msg_data, 10);
                                if ((long)this.msg_cursor == (long)this.msg_len)
                                {
                                    this.TriggerMessageEvent(this.msg_ver, this.msg_type, this.msg_id, this.msg_data);
                                    this.ReInitializeMessageProcessing();
                                    this.message_state = TCPIPClient.EMessageProcessingState.MESSAGE_HEADER;
                                    continue;
                                }
                                this.message_state = TCPIPClient.EMessageProcessingState.MESSAGE_BODY;
                                continue;
                            }
                            continue;
                        case TCPIPClient.EMessageProcessingState.MESSAGE_BODY:
                            int length2 = (int)Math.Min((long)this.msg_len - (long)this.msg_cursor, (long)this.buffer_bytes_available);
                            Array.Copy((Array)this.buffer, (long)this.buffer_cursor, (Array)this.msg_data, (long)this.msg_cursor, (long)length2);
                            this.msg_cursor += (uint)length2;
                            this.buffer_cursor += length2;
                            this.buffer_bytes_available -= length2;
                            if ((long)this.msg_cursor == (long)this.msg_len)
                            {
                                this.TriggerMessageEvent(this.msg_ver, this.msg_type, this.msg_id, this.msg_data);
                                this.ReInitializeMessageProcessing();
                                this.message_state = TCPIPClient.EMessageProcessingState.MESSAGE_HEADER;
                                continue;
                            }
                            continue;
                        default:
                            continue;
                    }
                }
                try
                {
                    if (this.connectionStatus != TCPIPClient.ENUM_INTERNAL_CONNECTION_STATUS.CONNECTED)
                        return;
                    this.StartNewBufferReceive();
                }
                catch (InvalidOperationException ex)
                {
                    this.Close();
                }
                catch (IOException ex)
                {
                    this.Close();
                }
            }
        }

        public override int Send(byte[] data)
        {
            try { TriggerRawSent(data); } catch { }


            lock (this.syn_msg)
            {
                if (this.connectionStatus != TCPIPClient.ENUM_INTERNAL_CONNECTION_STATUS.CONNECTED)
                    return 0;
                try
                {
                    this.network_stream.Flush();
                    this.network_stream.Write(data, 0, data.Length);
                    return data.Length;
                }
                catch (InvalidOperationException ex)
                {
                    this.Close();
                    return -1;
                }
                catch (IOException ex)
                {
                    this.Close();
                    return -1;
                }
            }
        }

        public override int Receive(out byte[] buffer)
        {
            throw new Exception("Synchronous receive unsupported");
        }

        public override void Dispose() => this.Close();

        private enum EMessageProcessingState
        {
            MESSAGE_UNKNOWN,
            MESSAGE_HEADER,
            MESSAGE_BODY,
        }

        private enum ENUM_INTERNAL_CONNECTION_STATUS
        {
            DISCONNECTED,
            DISCONNECTING,
            CONNECTED,
        }
    }
}
