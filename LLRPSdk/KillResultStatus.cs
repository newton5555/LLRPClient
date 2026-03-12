

using System;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Enum defining the possible status outcomes for the tag kill operation.
  /// </summary>
  [Serializable]
  public enum KillResultStatus
  {
    /// <summary>The tag kill operation was successful.</summary>
    Success,
    /// <summary>
    /// The tag kill password was set to zero; a non-zero password is
    /// required to kill a tag.
    /// </summary>
    ZeroKillPasswordError,
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
