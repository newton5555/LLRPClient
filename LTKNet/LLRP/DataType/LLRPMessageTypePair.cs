
namespace Org.LLRP.LTK.LLRPV1.DataType
{
  public class LLRPMessageTypePair
  {
    public object msg;
    public ENUM_LLRP_MSG_TYPE type;

    public LLRPMessageTypePair(object msg, ENUM_LLRP_MSG_TYPE type)
    {
      this.msg = msg;
      this.type = type;
    }
  }
}
