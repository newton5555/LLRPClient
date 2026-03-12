

using System;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// An enumeration used to indicate the results of
  /// a margin read operation.
  /// </summary>
  [Serializable]
  public enum MarginReadResult
  {
    /// <summary>The operation was successful.</summary>
    Success,
    /// <summary>The operation failed.</summary>
    Failure,
    /// <summary>
    /// There was insufficient power to perform the operation.
    /// </summary>
    InsufficientPower,
    /// <summary>The operation failed for an unknown reason.</summary>
    NonspecificTagError,
    /// <summary>
    /// The operation failed because the reader did not receive a response from the tag.
    /// </summary>
    NoResponseFromTag,
    /// <summary>The operation failed for an unknown reason.</summary>
    NonspecificReaderError,
    /// <summary>
    /// The operation failed because an incorrect password was supplied.
    /// </summary>
    IncorrectPasswordError,
    /// <summary>
    /// The operation failed because it attempted to access memory outside of the valid range.
    /// </summary>
    TagMemoryOverrunError,
    /// <summary>The target memory bank is in the locked state.</summary>
    TagMemoryLockedError,
  }
}
