using System;


namespace Org.LLRP.LTK.LLRPV1
{
    public delegate void delegateRawFrame(byte[] raw);



    [Serializable]
    public class AsynReadState
    {
        public byte[] data;

        public AsynReadState(int buffer_size) => this.data = new byte[buffer_size];
    }


    [Serializable]
    public abstract class CommunicationInterface : IDisposable
    {
        protected AsynReadState state;

        public event delegateFrameReceived OnFrameReceived;//真正的LLRP消息事件，外部订阅这个事件来处理LLRP消息

        public event delegateConnectionStatusChange OnClientConnectionStatusChange;


        public event delegateRawFrame? OnRawReceived;//日志
        public event delegateRawFrame? OnRawSent;//日志

        protected void TriggerRawSent(byte[] data)
        {
            try { OnRawSent?.Invoke(data); } catch { }
        }



        protected void TriggerMessageEvent(short ver, short msg_type, int msg_id, byte[] data)
        {
            try
            {
                try { OnRawReceived?.Invoke(data); } catch { }
                if (this.OnFrameReceived == null)
                    return;
                this.OnFrameReceived(ver, msg_type, msg_id, data);
            }
            catch
            {
            }
        }






        internal void TriggerOnClientConnectionStatusChange(ENUM_CONNECTION_STATUS status)
        {
            try
            {
                if (this.OnClientConnectionStatusChange == null)
                    return;
                this.OnClientConnectionStatusChange(status);
            }
            catch
            {
            }
        }

        public virtual bool Open(string device_name, int port) => false;

        public virtual bool Open(string device_name, int port, int timeout) => false;

        public virtual bool Open(string device_name, int port, bool useTLS) => false;

        public virtual bool Open(string device_name, int port, int timeout, bool useTLS) => false;

        public virtual bool Open(
          string device_name,
          int port,
          int timeout,
          bool useTLS,
          LLRPClient.TlsProtocols sslProtocolToUse)
        {
            return false;
        }

        public virtual void Close()
        {
        }

        public virtual int Send(byte[] data) => 0;

        public virtual int Receive(out byte[] buffer)
        {
            buffer = (byte[])null;
            return 0;
        }

        public virtual bool SetBufferSize(int size) => false;

        public virtual void Dispose()
        {
        }
    }
}
