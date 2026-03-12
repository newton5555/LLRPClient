

#nullable disable
namespace LLRPSdk
{
  /// <summary>Helper class used to encode and decode the PC word.</summary>
  public static class PcBits
  {
    /// <summary>
    /// Returns new PC bits, based on the current
    /// PC bits and the new EPC length.
    /// </summary>
    /// <param name="currentPcBits"></param>
    /// <param name="newEpcLengthInWords"></param>
    /// <returns></returns>
    public static ushort AdjustPcBits(ushort currentPcBits, ushort newEpcLengthInWords)
    {
      return (ushort) ((int) currentPcBits & 2047 | (int) newEpcLengthInWords << 11);
    }

    /// <summary>
    /// Determines the current length of the EPC (in words)
    /// based on the tag's PC bits.
    /// </summary>
    /// <param name="pcBits">The current PC bits of the tag.</param>
    /// <returns>The length of the EPC is 16-bit words.</returns>
    public static ushort EpcLengthInWords(ushort pcBits) => (ushort) ((uint) pcBits >> 11);
  }
}
