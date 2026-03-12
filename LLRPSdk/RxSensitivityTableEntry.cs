
#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Structure used to map a dBm value for receiver sensitivity to the
  /// appropriate index in the sensitivity level table used internally
  /// to the reader.
  /// </summary>
  public struct RxSensitivityTableEntry
  {
    /// <summary>Reader internal receiver sensitivity table index.</summary>
    public ushort Index;
    /// <summary>Receive sensitivity in dBm.</summary>
    public double Dbm;
  }
}
