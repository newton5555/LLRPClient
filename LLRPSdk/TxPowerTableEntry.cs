
#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Structure used to map a dBm value for transmit power to the appropriate
  /// index in the transmit power level table used internally to the reader.
  /// </summary>
  public struct TxPowerTableEntry
  {
    /// <summary>Reader internal transmit power table index.</summary>
    public ushort Index;
    /// <summary>Transmit power in dBm.</summary>
    public double Dbm;
  }
}
