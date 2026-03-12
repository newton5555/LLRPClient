

using System;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Class containing the status information for a general purpose input (GPI) port.
  /// </summary>
  public class GpiStatus
  {
    /// <summary>The current input level of the GPI port.</summary>
    public bool State { get; set; }

    /// <summary>The GPI port number.</summary>
    public ushort PortNumber { get; set; }

    /// <summary />
    [Obsolete("This property is no longer part of the reader status. It is now part of the reader settings, which can be retrieved using QuerySettings().", true)]
    public bool IsEnabled { get; set; }
  }
}
