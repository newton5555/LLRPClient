

using System;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Enum defining the possible status outcomes for the tag lock operation.
  /// </summary>
  [Serializable]
  public enum LockResultStatus
  {
    /// <summary>The tag lock operation completed successfully.</summary>
    Success,
    /// <summary>The tag did not meet the required RSSI threshold.</summary>
    InsufficientPower,
    /// <summary>An unidentified tag error occurred.</summary>
    NonspecificTagError,
    /// <summary>
    /// No ACK response was received from the tag in response to the operation.
    /// </summary>
    NoResponseFromTag,
    /// <summary>An unidentified reader error occurred.</summary>
    NonspecificReaderError,
    /// <summary>
    /// The provided password did not match the required value.
    /// </summary>
    IncorrectPasswordError,
    /// <summary>
    /// The tag lock operation exceeded the available address range.
    /// </summary>
    MemoryOverrunError,
    /// <summary>The memory location is locked.</summary>
    MemoryLockedError,
  }
}
