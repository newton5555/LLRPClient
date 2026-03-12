

using System;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// An enumerations used to indicate the results of
  /// a block permalock operation.
  /// </summary>
  [Serializable]
  public enum BlockPermalockResult
  {
    /// <summary>The operation was successful.</summary>
    Success,
    /// <summary>
    /// There was insufficient power to perform the operation.
    /// </summary>
    Insufficient_Power,
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
  }
}
