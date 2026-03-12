
using System;

#nullable disable
namespace LLRPSdk
{
  /// <summary>Enum used to identify the sequence stop trigger.</summary>
  [Serializable]
  public enum SequenceTriggerType
  {
    /// <summary>
    /// Indicates that there is no stop trigger; the sequence runs
    /// indefinitely.
    /// </summary>
    None,
    /// <summary>
    /// Indicates that the sequence will run for the number of
    /// times specified in the ExecutionCount parameter.
    /// </summary>
    ExecutionCount,
  }
}
