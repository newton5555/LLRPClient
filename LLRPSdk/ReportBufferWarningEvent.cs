
#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Class used to encapsulate the details of a
  /// <see cref="E:LLRPSdk.LLRPReader.ReportBufferWarning" />
  /// reader event.
  /// </summary>
  public class ReportBufferWarningEvent
  {
    /// <summary>
    /// Parameter defining how full the reader report buffer is in percent.
    /// </summary>
    public byte PercentFull;
  }
}
