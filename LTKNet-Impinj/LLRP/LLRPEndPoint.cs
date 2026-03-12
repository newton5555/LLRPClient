using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Threading.Tasks;


namespace Org.LLRP.LTK.LLRPV1
{
  public class LLRPEndPoint : IDisposable
  {
    private CommunicationInterface cI;
    private const int LLRP1_TCP_PORT = 5084;
    private string name;

    public event delegateConnectionStatusChange OnClientConnectionStatusChange;

    public event delegateOnMessageReceived OnMessageReceived;

    public LLRPEndPoint() => this.name = "Unknown";

    public LLRPEndPoint(string initName) => this.name = initName;

    public bool Create(string host, bool server) => this.Create(host, 5084, server);

    public string GetName() => this.name;

    public bool Create(string host, int tcpPort, bool server)
    {
      if (this.cI != null)
        return false;
      try
      {
        if (server)
        {
          this.cI = (CommunicationInterface) new TCPIPServer();
          this.cI.OnFrameReceived += new delegateFrameReceived(this.cI_OnMessageReceived);
          this.cI.OnClientConnectionStatusChange += new delegateConnectionStatusChange(this.cI_OnClientConnectionStatusChange);
          return this.cI.Open("", tcpPort);
        }
        this.cI = (CommunicationInterface) new TCPIPClient();
        this.cI.OnFrameReceived += new delegateFrameReceived(this.cI_OnMessageReceived);
        this.cI.OnClientConnectionStatusChange += new delegateConnectionStatusChange(this.cI_OnClientConnectionStatusChange);
        return this.cI.Open(host, tcpPort);
      }
      catch
      {
        if (this.cI != null)
        {
          this.cI.OnFrameReceived -= new delegateFrameReceived(this.cI_OnMessageReceived);
          this.cI.OnClientConnectionStatusChange -= new delegateConnectionStatusChange(this.cI_OnClientConnectionStatusChange);
        }
        return false;
      }
    }

    private void cI_OnClientConnectionStatusChange(ENUM_CONNECTION_STATUS status)
    {
      if (this.OnClientConnectionStatusChange == null)
        return;
      this.OnClientConnectionStatusChange(status);
    }

    private void triggerMessageReceived(Message msg)
    {
      if (this.OnMessageReceived == null)
        return;
      this.OnMessageReceived(msg);
    }

    private void cI_OnMessageReceived(short ver, short msg_type, int msg_id, byte[] msg_data)
    {
      Message msg;
      try
      {
        LLRPBinaryDecoder.Decode(ref msg_data, out msg);
      }
      catch
      {
        this.Close();
        msg = (Message) null;
      }
      if (msg == null || this.OnMessageReceived == null)
        return;
           //new delegateOnMessageReceived(this.triggerMessageReceived).BeginInvoke(msg, (AsyncCallback) null, (object) null);
            _ = Task.Run(() => triggerMessageReceived(msg));
        }

    public void Close()
    {
      this.name = "Unknown";
      if (this.cI == null)
        return;
      this.cI.Close();
      this.cI = (CommunicationInterface) null;
    }

    public void Dispose() => this.Close();

    public bool SendMessage(Message msg)
    {
      byte[] byteArray = Util.ConvertBitArrayToByteArray(msg.ToBitArray());
      try
      {
        return this.cI.Send(byteArray) > 0;
      }
      catch
      {
        return false;
      }
    }

    public Message TransactMessage(
      Message msg,
      int time_out,
      out MSG_ERROR_MESSAGE msg_err,
      ENUM_LLRP_MSG_TYPE responseType)
    {
      try
      {
        return new Transaction(this.cI, msg.MSG_ID, responseType).Transact(msg, out msg_err, time_out);
      }
      catch
      {
        msg_err = (MSG_ERROR_MESSAGE) null;
        return (Message) null;
      }
    }
  }
}
