
#nullable disable
namespace LLRPSdk
{
  /// <summary>Helper class used when defining tag memory accesses.</summary>
  public static class WordPointers
  {
    /// <summary>
    /// Constant used to point to Kill Password in reserved memory.
    /// </summary>
    public const ushort KillPassword = 0;
    /// <summary>
    /// Constant used to point to Access Password in reserved memory.
    /// </summary>
    public const ushort AccessPassword = 2;
    /// <summary>Constant used to point to EPC memory bank CRC word.</summary>
    public const ushort Crc = 0;
    /// <summary>
    /// Constant used to point to EPC memory bank PC Bits word.
    /// </summary>
    public const ushort PcBits = 1;
    /// <summary>Constant used to point to EPC memory bank EPC Data.</summary>
    public const ushort Epc = 2;
  }
}
