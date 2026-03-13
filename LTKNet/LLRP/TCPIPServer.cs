using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;


namespace Org.LLRP.LTK.LLRPV1
{
    /// <summary>
    /// TCPIPServer. Used for building LLRPServer, for example: LLRP reader simulator
    /// </summary>
    public class TCPIPServer : CommunicationInterface
    {
        private TcpListener server;
        private NetworkStream ns;
        private TCPIPServer.ENUM_INTERNAL_CONNECTION_STATUS connectionStatus;
        private object syn_msg = new object();
        private const int LLRP_HEADER_SIZE = 10;
        private TCPIPServer.EMessageProcessingState message_state;
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

        private void DoAcceptTCPClientCallBack(IAsyncResult ar)
        {
            lock (this.syn_msg)
            {
                try
                {
                    this.ns = ((TcpListener)ar.AsyncState).EndAcceptTcpClient(ar).GetStream();
                    this.InitializeMessageProcessing();
                    this.StartNewBufferReceive();
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
                this.connectionStatus = TCPIPServer.ENUM_INTERNAL_CONNECTION_STATUS.CONNECTED;
            }
            //new delegateConnectionStatusChange(((CommunicationInterface) this).TriggerOnClientConnectionStatusChange).BeginInvoke(ENUM_CONNECTION_STATUS.CONNECTED, (AsyncCallback) null, (object) null);
            _ = Task.Run(() => TriggerOnClientConnectionStatusChange(ENUM_CONNECTION_STATUS.CONNECTED));

        }

        public override void Close()
        {
            TCPIPServer.ENUM_INTERNAL_CONNECTION_STATUS connectionStatus = this.connectionStatus;
            if (connectionStatus != TCPIPServer.ENUM_INTERNAL_CONNECTION_STATUS.DISCONNECTED)
            {
                this.connectionStatus = TCPIPServer.ENUM_INTERNAL_CONNECTION_STATUS.DISCONNECTING;
                new ManualResetEvent(false).WaitOne(100, false);
            }
            lock (this.syn_msg)
            {
                if (this.ns != null)
                {
                    this.ns.Close();
                    this.ns = (NetworkStream)null;
                }
                if (this.server != null)
                {
                    this.server.Stop();
                    this.server = (TcpListener)null;
                }
                this.connectionStatus = TCPIPServer.ENUM_INTERNAL_CONNECTION_STATUS.DISCONNECTED;
            }
            if (connectionStatus != TCPIPServer.ENUM_INTERNAL_CONNECTION_STATUS.CONNECTED)
                return;
            this.TriggerOnClientConnectionStatusChange(ENUM_CONNECTION_STATUS.DISCONNECTED);
        }

        public override bool Open(string device_name, int port)
        {
            lock (this.syn_msg)
            {
                if (this.connectionStatus != TCPIPServer.ENUM_INTERNAL_CONNECTION_STATUS.DISCONNECTED)
                    return false;
                try
                {
                    this.server = new TcpListener(IPAddress.IPv6Any, port);
                    this.server.Server.DualMode = true;
                    this.server.Start();
                    this.server.BeginAcceptTcpClient(new AsyncCallback(this.DoAcceptTCPClientCallBack), (object)this.server);
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }


     

        public override bool Open(string device_name, int port, bool useTLS)
        {
            return this.Open(device_name, port);
        }

        public override bool Open(string device_name, int port, int timeout)
        {
            return this.Open(device_name, port);
        }

        public override bool Open(string device_name, int port, int timeout, bool useTLS)
        {
            return this.Open(device_name, port);
        }

        public override bool Open(
     string device_name,
     int port,
     int timeout,
     bool useTLS,
     LLRPClient.TlsProtocols tlsProtocol)
        {
            if (useTLS)
                throw new NotSupportedException("TLS is not supported in TCPIPServer");
            return this.Open(device_name, port);
        }




        public override int Receive(out byte[] buffer)
        {
            throw new Exception("Syncrhonous receive not supported");
        }

        public override int Send(byte[] data)
        {
            try { TriggerRawSent(data); } catch { }


            lock (this.syn_msg)
            {
                if (this.connectionStatus != TCPIPServer.ENUM_INTERNAL_CONNECTION_STATUS.CONNECTED)
                    return 0;
                try
                {
                    this.ns.Flush();
                    this.ns.Write(data, 0, data.Length);
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
                if (this.connectionStatus != TCPIPServer.ENUM_INTERNAL_CONNECTION_STATUS.CONNECTED)
                    return;
                try
                {
                    this.buffer_bytes_available += this.ns.EndRead(asyncResult);
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
                        case TCPIPServer.EMessageProcessingState.MESSAGE_UNKNOWN:
                            throw new Exception("Unexpected receive message state");
                        case TCPIPServer.EMessageProcessingState.MESSAGE_HEADER:
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
                                    this.message_state = TCPIPServer.EMessageProcessingState.MESSAGE_HEADER;
                                    continue;
                                }
                                this.message_state = TCPIPServer.EMessageProcessingState.MESSAGE_BODY;
                                continue;
                            }
                            continue;
                        case TCPIPServer.EMessageProcessingState.MESSAGE_BODY:
                            int length2 = (int)Math.Min((long)this.msg_len - (long)this.msg_cursor, (long)this.buffer_bytes_available);
                            Array.Copy((Array)this.buffer, (long)this.buffer_cursor, (Array)this.msg_data, (long)this.msg_cursor, (long)length2);
                            this.msg_cursor += (uint)length2;
                            this.buffer_cursor += length2;
                            this.buffer_bytes_available -= length2;
                            if ((long)this.msg_cursor == (long)this.msg_len)
                            {
                                this.TriggerMessageEvent(this.msg_ver, this.msg_type, this.msg_id, this.msg_data);
                                this.ReInitializeMessageProcessing();
                                this.message_state = TCPIPServer.EMessageProcessingState.MESSAGE_HEADER;
                                continue;
                            }
                            continue;
                        default:
                            continue;
                    }
                }
                try
                {
                    if (this.connectionStatus != TCPIPServer.ENUM_INTERNAL_CONNECTION_STATUS.CONNECTED)
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

        private void InitializeMessageProcessing()
        {
            this.ReInitializeMessageProcessing();
            this.message_state = TCPIPServer.EMessageProcessingState.MESSAGE_HEADER;
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
            this.ns.Flush();
            this.ns.BeginRead(this.buffer, this.buffer_cursor, 2048 - this.buffer_cursor, new AsyncCallback(this.OnDataRead), (object)this.message_state);
        }

        private void importAndQualifyHeader()
        {
            int num = ((int)this.msg_header_storage[0] << 8) + (int)this.msg_header_storage[1];
            this.msg_type = (short)(num & 1023);
            this.msg_ver = (short)(num >> 10 & 7);
            this.msg_len = ((int)this.msg_header_storage[2] << 24) + ((int)this.msg_header_storage[3] << 16) + ((int)this.msg_header_storage[4] << 8) + (int)this.msg_header_storage[5];
            this.msg_id = ((int)this.msg_header_storage[6] << 24) + ((int)this.msg_header_storage[7] << 16) + ((int)this.msg_header_storage[8] << 8) + (int)this.msg_header_storage[9];
        }

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
