
namespace Org.LLRP.LTK.LLRPV1.DataType
{
  public class MessageID
  {
    protected static object mutex = new object();
    protected static uint sequence_num = 0;

    public static uint getNewMessageID()
    {
      uint sequenceNum;
      lock (MessageID.mutex)
      {
        sequenceNum = MessageID.sequence_num;
        ++MessageID.sequence_num;
      }
      return sequenceNum;
    }
  }
}
