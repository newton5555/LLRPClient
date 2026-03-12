

using System;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Enum defining the possible llrp reader autostart modes.
  /// </summary>
  [Serializable]
  public enum AutoStartMode
  {
    /// <summary>No autostart mode specified</summary>
    None,
    /// <summary>Start immediately after the reader is configured.</summary>
    Immediate,
    /// <summary>Periodic autostart mode specified.</summary>
    Periodic,
    /// <summary>GPI trigger autostart mode specified.</summary>
    GpiTrigger,
  }
}
