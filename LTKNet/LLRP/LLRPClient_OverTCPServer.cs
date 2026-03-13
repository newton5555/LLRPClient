using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Threading;


namespace Org.LLRP.LTK.LLRPV1
{
    //上位机作为TCP服务器，阅读器作为TCPClient   与LLRP阅读器通信,测试可以工作单不保证没问题,后面主要还是用LLRPClient
    [Serializable]
    public class LLRPClient_OverTCPServer : IDisposable
    {
        private CommunicationInterface cI;
        private int LLRP_TCP_PORT = 5084;
        private int MSG_TIME_OUT = 10000;
        private Thread notificationThread;
        private Thread keepalivesThread;
        private BlockingQueue notificationQueue;
        private BlockingQueue keepalivesQueue;
        private ManualResetEvent conn_evt;
        private ENUM_ConnectionAttemptStatusType conn_status_type;
        private string reader_name;
        private bool connected;

        public event delegateReaderEventNotification OnReaderEventNotification;

        public event delegateRoAccessReport OnRoAccessReportReceived;

        public event delegateKeepAlive OnKeepAlive;

        public event delegateEncapReaderEventNotification OnEncapedReaderEventNotification;

        public event delegateEncapRoAccessReport OnEncapedRoAccessReportReceived;

        public event delegateEncapKeepAlive OnEncapedKeepAlive;

        public event delegateErrorNotification OnErrorNotification;

        protected void TriggerReaderEventNotification(MSG_READER_EVENT_NOTIFICATION msg)
        {
            try
            {
                if (this.OnReaderEventNotification != null)
                    this.OnReaderEventNotification(msg);
                if (this.OnEncapedReaderEventNotification == null)
                    return;
                ENCAPED_READER_EVENT_NOTIFICATION msg1 = new ENCAPED_READER_EVENT_NOTIFICATION();
                msg1.reader = this.reader_name;
                msg1.ntf = msg;
                this.OnEncapedReaderEventNotification(msg1);
            }
            catch
            {
            }
        }

        protected void TriggerRoAccessReport(MSG_RO_ACCESS_REPORT msg)
        {
            try
            {
                if (this.OnRoAccessReportReceived != null)
                    this.OnRoAccessReportReceived(msg);
                if (this.OnEncapedRoAccessReportReceived == null)
                    return;
                this.OnEncapedRoAccessReportReceived(new ENCAPED_RO_ACCESS_REPORT()
                {
                    reader = this.reader_name,
                    report = msg
                });
            }
            catch
            {
            }
        }

        protected void TriggerKeepAlive(MSG_KEEPALIVE msg)
        {
            try
            {
                if (this.OnKeepAlive != null)
                    this.OnKeepAlive(msg);
                if (this.OnEncapedKeepAlive == null)
                    return;
                this.OnEncapedKeepAlive(new ENCAPED_KEEP_ALIVE()
                {
                    reader = this.reader_name,
                    keep_alive = msg
                });
            }
            catch
            {
            }
        }

        public string ReaderName => this.reader_name;

        public bool IsConnected => this.connected;

        public void SetMessageTimeOut(int time_out) => this.MSG_TIME_OUT = time_out;

        public int GetMessageTimeOut() => this.MSG_TIME_OUT;

        public LLRPClient_OverTCPServer()
        {
            this.cI = (CommunicationInterface)new TCPIPServer();
            this.notificationQueue = new BlockingQueue();
            this.keepalivesQueue = new BlockingQueue();
            this.CheckNotificationAndKeepaliveThreads();
        }

        public LLRPClient_OverTCPServer(int port)
        {
            this.LLRP_TCP_PORT = port;
            this.cI = (CommunicationInterface)new TCPIPServer();
            this.notificationQueue = new BlockingQueue();
            this.keepalivesQueue = new BlockingQueue();
            this.CheckNotificationAndKeepaliveThreads();
        }

        private void CheckNotificationAndKeepaliveThreads()
        {
            if (this.notificationThread == null)
            {
                this.notificationThread = new Thread(new ThreadStart(this.ProcessNotificationQueue))
                {
                    IsBackground = true
                };
                this.notificationThread.Start();
            }
            if (this.keepalivesThread != null)
                return;
            this.keepalivesThread = new Thread(new ThreadStart(this.ProcessKeepalivesQueue))
            {
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal
            };
            this.keepalivesThread.Start();
        }

        private void ProcessNotificationQueue()
        {
            try
            {
                if (this.notificationQueue == null)
                    return;
                while (true)
                {
                    Message msg;
                    ENUM_LLRP_MSG_TYPE msgType;
                    do
                    {
                        msg = (Message)this.notificationQueue.Dequeue();
                        msgType = (ENUM_LLRP_MSG_TYPE)msg.MSG_TYPE;
                        if (msgType == ENUM_LLRP_MSG_TYPE.RO_ACCESS_REPORT)
                            goto label_3;
                    }
                    while (msgType != ENUM_LLRP_MSG_TYPE.READER_EVENT_NOTIFICATION);
                    goto label_4;
                label_3:
                    this.TriggerRoAccessReport((MSG_RO_ACCESS_REPORT)msg);
                    continue;
                label_4:
                    this.TriggerReaderEventNotification((MSG_READER_EVENT_NOTIFICATION)msg);
                }
            }
            catch (ThreadInterruptedException ex)
            {
            }
            catch (Exception ex)
            {
                this.TriggerErrorNotification(ex);
            }
        }

        private void ProcessKeepalivesQueue()
        {
            try
            {
                if (this.keepalivesQueue == null)
                    return;
                while (true)
                {
                    Message msg;
                    do
                    {
                        msg = (Message)this.keepalivesQueue.Dequeue();
                    }
                    while (msg.MSG_TYPE != (ushort)62);
                    this.TriggerKeepAlive((MSG_KEEPALIVE)msg);
                }
            }
            catch (ThreadInterruptedException ex)
            {
            }
            catch (Exception ex)
            {
                this.TriggerErrorNotification(ex);
            }
        }

        private void TriggerErrorNotification(Exception error)
        {
            if (this.OnErrorNotification == null)
                return;
            this.OnErrorNotification(this.reader_name, error);
        }

        public bool Open(
          string llrp_reader_name,
          int timeout,
          out ENUM_ConnectionAttemptStatusType status)
        {
            return this.Open(llrp_reader_name, timeout, false, out status);
        }

        public bool Open(
          string llrp_reader_name,
          int timeout,
          bool useTLS,
          out ENUM_ConnectionAttemptStatusType status)
        {
            return this.Open(llrp_reader_name, timeout, useTLS, LLRPClient.TlsProtocols.OsDefault, out status);
        }

        public bool Open(
          string llrp_reader_name,
          int timeout,
          bool useTLS,
          LLRPClient.TlsProtocols tlsProtocol,
          out ENUM_ConnectionAttemptStatusType status)
        {
            this.reader_name = llrp_reader_name;
            status = ~ENUM_ConnectionAttemptStatusType.Success;
            this.cI.OnFrameReceived += new delegateFrameReceived(this.ProcessFrame);
            this.CheckNotificationAndKeepaliveThreads();
            bool flag;
            try
            {
                flag = this.cI.Open(llrp_reader_name, this.LLRP_TCP_PORT, timeout, useTLS, tlsProtocol);
            }
            catch (LLRPNetworkException ex)
            {
                this.cI.OnFrameReceived -= new delegateFrameReceived(this.ProcessFrame);
                throw;
            }
            this.conn_evt = new ManualResetEvent(false);
            if (flag && this.conn_evt.WaitOne(timeout, false))
            {
                status = this.conn_status_type;
                if (status == ENUM_ConnectionAttemptStatusType.Success)
                {
                    this.connected = true;
                    return this.connected;
                }
            }
            this.reader_name = llrp_reader_name;
            this.cI.Close();
            this.cI.OnFrameReceived -= new delegateFrameReceived(this.ProcessFrame);
            this.connected = false;
            return this.connected;
        }

        public bool Close()
        {
            try
            {
                bool flag = true;
                if (this.IsConnected)
                {
                    MSG_CLOSE_CONNECTION_RESPONSE connectionResponse = this.CLOSE_CONNECTION(new MSG_CLOSE_CONNECTION(), out MSG_ERROR_MESSAGE _, this.MSG_TIME_OUT);
                    if (connectionResponse == null || connectionResponse.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                        flag = false;
                }
                try
                {
                    this.cI.Close();
                }
                catch
                {
                }
                this.cI.OnFrameReceived -= new delegateFrameReceived(this.ProcessFrame);
                this.connected = false;
                if (this.notificationThread != null)
                {
                    this.notificationThread.Interrupt();
                    if (!this.notificationThread.Join(new TimeSpan(0, 0, 1)))
                        Console.WriteLine("Error: Join timed out.");
                    this.notificationThread = (Thread)null;
                }
                if (this.keepalivesThread != null)
                {
                    this.keepalivesThread.Interrupt();
                    if (!this.keepalivesThread.Join(new TimeSpan(0, 0, 1)))
                        Console.WriteLine("Error: Join timed out.");
                    this.keepalivesThread = (Thread)null;
                }
                return flag;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose() => this.Close();

        private void ProcessFrame(short ver, short msg_type, int msg_id, byte[] data)
        {
            int cursor = 0;
            switch (msg_type)
            {
                case 61:
                    try
                    {
                        BitArray bitArray = Util.ConvertByteArrayToBitArray(data);
                        int count = bitArray.Count;
                        MSG_RO_ACCESS_REPORT msgRoAccessReport = MSG_RO_ACCESS_REPORT.FromBitArray(ref bitArray, ref cursor, count);
                        if (this.notificationQueue == null)
                            break;
                        this.notificationQueue.Enqueue((object)msgRoAccessReport);
                        break;
                    }
                    catch
                    {
                        break;
                    }
                case 62:
                    try
                    {
                        BitArray bitArray = Util.ConvertByteArrayToBitArray(data);
                        int count = bitArray.Count;
                        MSG_KEEPALIVE msgKeepalive = MSG_KEEPALIVE.FromBitArray(ref bitArray, ref cursor, count);
                        if (this.keepalivesQueue == null)
                            break;
                        this.keepalivesQueue.Enqueue((object)msgKeepalive);
                        break;
                    }
                    catch
                    {
                        break;
                    }
                case 63:
                    try
                    {
                        BitArray bitArray = Util.ConvertByteArrayToBitArray(data);
                        int count = bitArray.Count;
                        MSG_READER_EVENT_NOTIFICATION eventNotification = MSG_READER_EVENT_NOTIFICATION.FromBitArray(ref bitArray, ref cursor, count);
                        if (this.conn_evt != null && eventNotification.ReaderEventNotificationData.ConnectionAttemptEvent != null)
                        {
                            this.conn_status_type = eventNotification.ReaderEventNotificationData.ConnectionAttemptEvent.Status;
                            this.conn_evt.Set();
                            break;
                        }
                        if (this.notificationQueue == null)
                            break;
                        this.notificationQueue.Enqueue((object)eventNotification);
                        break;
                    }
                    catch
                    {
                        break;
                    }
            }
        }

        public MSG_CUSTOM_MESSAGE CUSTOM_MESSAGE(
          MSG_CUSTOM_MESSAGE msg,
          out MSG_ERROR_MESSAGE msg_err,
          int time_out)
        {
            return (MSG_CUSTOM_MESSAGE)new Transaction(this.cI, msg.MSG_ID, ENUM_LLRP_MSG_TYPE.CUSTOM_MESSAGE).Transact((Message)msg, out msg_err, time_out);
        }

        public MSG_GET_READER_CAPABILITIES_RESPONSE GET_READER_CAPABILITIES(
          MSG_GET_READER_CAPABILITIES msg,
          out MSG_ERROR_MESSAGE msg_err,
          int time_out)
        {
            return (MSG_GET_READER_CAPABILITIES_RESPONSE)new Transaction(this.cI, msg.MSG_ID, ENUM_LLRP_MSG_TYPE.GET_READER_CAPABILITIES_RESPONSE).Transact((Message)msg, out msg_err, time_out);
        }

        public MSG_ADD_ROSPEC_RESPONSE ADD_ROSPEC(
          MSG_ADD_ROSPEC msg,
          out MSG_ERROR_MESSAGE msg_err,
          int time_out)
        {
            return (MSG_ADD_ROSPEC_RESPONSE)new Transaction(this.cI, msg.MSG_ID, ENUM_LLRP_MSG_TYPE.ADD_ROSPEC_RESPONSE).Transact((Message)msg, out msg_err, time_out);
        }

        public MSG_DELETE_ROSPEC_RESPONSE DELETE_ROSPEC(
          MSG_DELETE_ROSPEC msg,
          out MSG_ERROR_MESSAGE msg_err,
          int time_out)
        {
            return (MSG_DELETE_ROSPEC_RESPONSE)new Transaction(this.cI, msg.MSG_ID, ENUM_LLRP_MSG_TYPE.DELETE_ROSPEC_RESPONSE).Transact((Message)msg, out msg_err, time_out);
        }

        public MSG_START_ROSPEC_RESPONSE START_ROSPEC(
          MSG_START_ROSPEC msg,
          out MSG_ERROR_MESSAGE msg_err,
          int time_out)
        {
            return (MSG_START_ROSPEC_RESPONSE)new Transaction(this.cI, msg.MSG_ID, ENUM_LLRP_MSG_TYPE.START_ROSPEC_RESPONSE).Transact((Message)msg, out msg_err, time_out);
        }

        public MSG_STOP_ROSPEC_RESPONSE STOP_ROSPEC(
          MSG_STOP_ROSPEC msg,
          out MSG_ERROR_MESSAGE msg_err,
          int time_out)
        {
            return (MSG_STOP_ROSPEC_RESPONSE)new Transaction(this.cI, msg.MSG_ID, ENUM_LLRP_MSG_TYPE.STOP_ROSPEC_RESPONSE).Transact((Message)msg, out msg_err, time_out);
        }

        public MSG_ENABLE_ROSPEC_RESPONSE ENABLE_ROSPEC(
          MSG_ENABLE_ROSPEC msg,
          out MSG_ERROR_MESSAGE msg_err,
          int time_out)
        {
            return (MSG_ENABLE_ROSPEC_RESPONSE)new Transaction(this.cI, msg.MSG_ID, ENUM_LLRP_MSG_TYPE.ENABLE_ROSPEC_RESPONSE).Transact((Message)msg, out msg_err, time_out);
        }

        public MSG_DISABLE_ROSPEC_RESPONSE DISABLE_ROSPEC(
          MSG_DISABLE_ROSPEC msg,
          out MSG_ERROR_MESSAGE msg_err,
          int time_out)
        {
            return (MSG_DISABLE_ROSPEC_RESPONSE)new Transaction(this.cI, msg.MSG_ID, ENUM_LLRP_MSG_TYPE.DISABLE_ROSPEC_RESPONSE).Transact((Message)msg, out msg_err, time_out);
        }

        public MSG_GET_ROSPECS_RESPONSE GET_ROSPECS(
          MSG_GET_ROSPECS msg,
          out MSG_ERROR_MESSAGE msg_err,
          int time_out)
        {
            return (MSG_GET_ROSPECS_RESPONSE)new Transaction(this.cI, msg.MSG_ID, ENUM_LLRP_MSG_TYPE.GET_ROSPECS_RESPONSE).Transact((Message)msg, out msg_err, time_out);
        }

        public MSG_ADD_ACCESSSPEC_RESPONSE ADD_ACCESSSPEC(
          MSG_ADD_ACCESSSPEC msg,
          out MSG_ERROR_MESSAGE msg_err,
          int time_out)
        {
            return (MSG_ADD_ACCESSSPEC_RESPONSE)new Transaction(this.cI, msg.MSG_ID, ENUM_LLRP_MSG_TYPE.ADD_ACCESSSPEC_RESPONSE).Transact((Message)msg, out msg_err, time_out);
        }

        public MSG_DELETE_ACCESSSPEC_RESPONSE DELETE_ACCESSSPEC(
          MSG_DELETE_ACCESSSPEC msg,
          out MSG_ERROR_MESSAGE msg_err,
          int time_out)
        {
            return (MSG_DELETE_ACCESSSPEC_RESPONSE)new Transaction(this.cI, msg.MSG_ID, ENUM_LLRP_MSG_TYPE.DELETE_ACCESSSPEC_RESPONSE).Transact((Message)msg, out msg_err, time_out);
        }

        public MSG_ENABLE_ACCESSSPEC_RESPONSE ENABLE_ACCESSSPEC(
          MSG_ENABLE_ACCESSSPEC msg,
          out MSG_ERROR_MESSAGE msg_err,
          int time_out)
        {
            return (MSG_ENABLE_ACCESSSPEC_RESPONSE)new Transaction(this.cI, msg.MSG_ID, ENUM_LLRP_MSG_TYPE.ENABLE_ACCESSSPEC_RESPONSE).Transact((Message)msg, out msg_err, time_out);
        }

        public MSG_DISABLE_ACCESSSPEC_RESPONSE DISABLE_ACCESSSPEC(
          MSG_DISABLE_ACCESSSPEC msg,
          out MSG_ERROR_MESSAGE msg_err,
          int time_out)
        {
            return (MSG_DISABLE_ACCESSSPEC_RESPONSE)new Transaction(this.cI, msg.MSG_ID, ENUM_LLRP_MSG_TYPE.DISABLE_ACCESSSPEC_RESPONSE).Transact((Message)msg, out msg_err, time_out);
        }

        public MSG_GET_ACCESSSPECS_RESPONSE GET_ACCESSSPECS(
          MSG_GET_ACCESSSPECS msg,
          out MSG_ERROR_MESSAGE msg_err,
          int time_out)
        {
            return (MSG_GET_ACCESSSPECS_RESPONSE)new Transaction(this.cI, msg.MSG_ID, ENUM_LLRP_MSG_TYPE.GET_ACCESSSPECS_RESPONSE).Transact((Message)msg, out msg_err, time_out);
        }

        public MSG_CLIENT_REQUEST_OP_RESPONSE CLIENT_REQUEST_OP(
          MSG_CLIENT_REQUEST_OP msg,
          out MSG_ERROR_MESSAGE msg_err,
          int time_out)
        {
            return (MSG_CLIENT_REQUEST_OP_RESPONSE)new Transaction(this.cI, msg.MSG_ID, ENUM_LLRP_MSG_TYPE.CLIENT_REQUEST_OP_RESPONSE).Transact((Message)msg, out msg_err, time_out);
        }

        public MSG_GET_READER_CONFIG_RESPONSE GET_READER_CONFIG(
          MSG_GET_READER_CONFIG msg,
          out MSG_ERROR_MESSAGE msg_err,
          int time_out)
        {
            return (MSG_GET_READER_CONFIG_RESPONSE)new Transaction(this.cI, msg.MSG_ID, ENUM_LLRP_MSG_TYPE.GET_READER_CONFIG_RESPONSE).Transact((Message)msg, out msg_err, time_out);
        }

        public MSG_SET_READER_CONFIG_RESPONSE SET_READER_CONFIG(
          MSG_SET_READER_CONFIG msg,
          out MSG_ERROR_MESSAGE msg_err,
          int time_out)
        {
            return (MSG_SET_READER_CONFIG_RESPONSE)new Transaction(this.cI, msg.MSG_ID, ENUM_LLRP_MSG_TYPE.SET_READER_CONFIG_RESPONSE).Transact((Message)msg, out msg_err, time_out);
        }

        public MSG_CLOSE_CONNECTION_RESPONSE CLOSE_CONNECTION(
          MSG_CLOSE_CONNECTION msg,
          out MSG_ERROR_MESSAGE msg_err,
          int time_out)
        {
            return (MSG_CLOSE_CONNECTION_RESPONSE)new Transaction(this.cI, msg.MSG_ID, ENUM_LLRP_MSG_TYPE.CLOSE_CONNECTION_RESPONSE).Transact((Message)msg, out msg_err, time_out);
        }

        public void GET_REPORT(MSG_GET_REPORT msg, out MSG_ERROR_MESSAGE msg_err, int time_out)
        {
            msg_err = (MSG_ERROR_MESSAGE)null;
            new Transaction(this.cI, msg.MSG_ID, ENUM_LLRP_MSG_TYPE.GET_REPORT).Send((Message)msg);
        }

        public void KEEPALIVE_ACK(MSG_KEEPALIVE_ACK msg, out MSG_ERROR_MESSAGE msg_err, int time_out)
        {
            msg_err = (MSG_ERROR_MESSAGE)null;
            new Transaction(this.cI, msg.MSG_ID, ENUM_LLRP_MSG_TYPE.KEEPALIVE_ACK).Send((Message)msg);
        }

        public void ENABLE_EVENTS_AND_REPORTS(
          MSG_ENABLE_EVENTS_AND_REPORTS msg,
          out MSG_ERROR_MESSAGE msg_err,
          int time_out)
        {
            msg_err = (MSG_ERROR_MESSAGE)null;
            new Transaction(this.cI, msg.MSG_ID, ENUM_LLRP_MSG_TYPE.ENABLE_EVENTS_AND_REPORTS).Send((Message)msg);
        }

        public enum TlsProtocols
        {
            OsDefault,
            Tls12,
            Tls13,
        }
    }
}
