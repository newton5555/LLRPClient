
using System;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Enum defining the possible status outcomes for the tag write operation.
  /// </summary>
  [Serializable]
  public enum WriteResultStatus
  {
    /// <summary>The tag write operation was successful.</summary>
    Success,
    /// <summary>
    /// The tag write operation request contained more data than can fit in
    /// the target memory bank.
    /// </summary>
    TagMemoryOverrunError,
    /// <summary>The target memory bank is in the locked state.</summary>
    TagMemoryLockedError,
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
  }
}
