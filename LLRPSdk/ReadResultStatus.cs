
using System;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Enum defining the possible status outcomes for the tag read operation.
  /// </summary>
  [Serializable]
  public enum ReadResultStatus
  {
    /// <summary>The tag read operation was successful.</summary>
    Success,
    /// <summary>An unidentified tag error occurred.</summary>
    NonspecificTagError,
    /// <summary>
    /// No ACK response was received from the tag in response to the operation.
    /// </summary>
    NoResponseFromTag,
    /// <summary>An unidentified reader error occurred.</summary>
    NonspecificReaderError,
    /// <summary>
    /// The tag read operation exceeded the available address range.
    /// </summary>
    MemoryOverrunError,
    /// <summary>The memory location is locked.</summary>
    MemoryLockedError,
    /// <summary>
    /// The provided password did not match the required value.
    /// </summary>
    IncorrectPasswordError,
  }
}
