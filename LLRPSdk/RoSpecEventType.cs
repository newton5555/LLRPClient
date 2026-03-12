
using System;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Enum identifying the type of ROSpec event that has occurred.
  /// </summary>
  [Serializable]
  public enum RoSpecEventType
  {
    /// <summary>This is an ROSpec start event.</summary>
    StartOfROSpec,
    /// <summary>This is an end of ROSpec event.</summary>
    EndOfROSpec,
    /// <summary>
    /// ROSpec has been preempted by an ROSpec of a higher priority.
    /// </summary>
    PreemptionOfROSpec,
  }
}
