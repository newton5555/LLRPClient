

namespace Org.LLRP.LTK.LLRPV1.DataType
{
  public class U96
  {
    private ushort[] data;

    public override string ToString()
    {
      return string.Format("{0:4X}{1:4X}{2:4X}{3:4X}{4:4X}{5:4X}", (object) this.data[0], (object) this.data[1], (object) this.data[2], (object) this.data[3], (object) this.data[4], (object) this.data[5]);
    }

    public U96() => this.data = new ushort[6];
  }
}
