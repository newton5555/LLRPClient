

using System;


namespace Org.LLRP.LTK.LLRPV1
{
  public class MalformedPacket : Exception
  {
    public MalformedPacket()
    {
    }

    public MalformedPacket(string message)
      : base(message)
    {
    }

    public MalformedPacket(string message, Exception innerException)
      : base(message, innerException)
    {
    }
  }
}
