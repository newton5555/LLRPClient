

using System;

#nullable disable
namespace LLRPSdk
{
  /// <summary>Enum defining the possible types of antenna event.</summary>
  [Serializable]
  public enum AntennaEventType
  {
    /// <summary>Indicates an antenna disconnected event.</summary>
    AntennaDisconnected,
    /// <summary>Indicates an antenna connected event.</summary>
    AntennaConnected,
    /// <summary>Indicates an antenna started event</summary>
    AntennaStarted,
  }
}
