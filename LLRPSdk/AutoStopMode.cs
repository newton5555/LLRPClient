

using System;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Enum defining the possible llrp reader autostop modes.
  /// </summary>
  [Serializable]
  public enum AutoStopMode
  {
    /// <summary>No autostop mode specified.</summary>
    None,
    /// <summary>Autostop defined by a timeout.</summary>
    Duration,
    /// <summary>Autostop defined by a GPI trigger value.</summary>
    GpiTrigger,
  }
}
