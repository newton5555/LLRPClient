
using System;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Class containing the status information for a general purpose output (GPO) port.
  /// </summary>
  public class GpoStatus
  {
    /// <summary>The current output level of the GPO port.</summary>
    public bool State { get; set; }

    /// <summary>The GPO port number.</summary>
    public ushort PortNumber { get; set; }

    /// <summary />
    [Obsolete("This property is no longer part of the reader status. It is now part of the reader settings, which can be retrieved using QuerySettings().", true)]
    public GpoMode Mode { get; set; }
  }
}
