

using System;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Class containing The status information for an individual antenna port.
  /// </summary>
  public class AntennaStatus
  {


    /// <summary>
    /// Indicates whether the antenna port is connected to an antenna.
    /// </summary>
    public bool IsConnected { get; set; }

    /// <summary>The antenna port number.</summary>
    public ushort PortNumber { get; set; }


  }
}
