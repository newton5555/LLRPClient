
namespace Org.LLRP.LTK.LLRPV1.DataType
{
  public interface IParameter
  {
    void ToBitArray(ref bool[] bit_array, ref int cursor);

    void AppendToBitArray(AutoGrowingBitArray bArr);
  }
}
