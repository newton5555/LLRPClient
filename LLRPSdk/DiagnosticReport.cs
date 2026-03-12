

using System.Collections.Generic;

#nullable disable
namespace LLRPSdk
{
  /// <summary>xArray diagnostic report</summary>
  public class DiagnosticReport
  {
    internal DiagnosticReport() => this.Metrics = new List<uint>();

    /// <summary>Diagnostic data. Internal use only.</summary>
    public List<uint> Metrics { get; set; }
  }
}
