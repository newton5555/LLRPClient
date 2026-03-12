
using System.Collections;


namespace Org.LLRP.LTK.LLRPV1.DataType
{
  public abstract class Parameter : IParameter
  {
    protected ushort typeID;
    protected ushort length;
    protected bool tvCoding;

    public virtual void ToBitArray(ref bool[] bit_array, ref int cursor)
    {
    }

    public virtual void AppendToBitArray(AutoGrowingBitArray bArr)
    {
    }

    public virtual Parameter FromBitArray(ref BitArray bit_array, ref int cursor, int length)
    {
      return (Parameter) null;
    }

    public virtual Parameter FromString(string xml) => (Parameter) null;

    public ushort TypeID => this.typeID;

    public ushort Length => this.length;
  }
}
