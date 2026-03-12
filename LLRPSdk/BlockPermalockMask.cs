

using System;

#nullable disable
namespace LLRPSdk
{
  /// <summary>Uses to specify which blocks to permalock.</summary>
  public class BlockPermalockMask
  {
    /// <summary>Gets the block permalock mask.</summary>
    public ushort[] Mask { get; private set; }

    /// <summary>
    /// Creates a block permalock mask from the specified block.
    /// </summary>
    /// <param name="block"></param>
    /// <returns>A block permalock mask</returns>
    public static BlockPermalockMask FromBlockNumber(ushort block)
    {
      return BlockPermalockMask.FromBlockNumberArray(new ushort[1]
      {
        block
      });
    }

    /// <summary>
    /// Creates a block permalock mask from an array of block numbers.
    /// </summary>
    /// <param name="blocks"></param>
    /// <returns>A block permalock mask</returns>
    public static BlockPermalockMask FromBlockNumberArray(ushort[] blocks)
    {
      BlockPermalockMask blockPermalockMask = new BlockPermalockMask();
      ushort val2 = 0;
      foreach (ushort block in blocks)
        val2 = Math.Max(block, val2);
      ushort[] numArray = new ushort[(int) val2 / 16 + 1];
      foreach (ushort block in blocks)
      {
        ushort index = (ushort) ((uint) block / 16U);
        numArray[(int) index] |= (ushort) (32768 >> (int) block - (int) index * 16);
      }
      blockPermalockMask.Mask = numArray;
      return blockPermalockMask;
    }

    /// <summary>
    /// Converts the block permalock mask into a hexadecimal string.
    /// </summary>
    /// <returns></returns>
    public string ToHexString()
    {
      string hexString = "";
      foreach (ushort num in this.Mask)
        hexString += num.ToString("X4");
      return hexString;
    }
  }
}
