using System;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Enum defining the possible status outcomes for the tag block erase operation.
  /// </summary>
  [Serializable]
  public enum BlockEraseResultStatus
  {
    Success,
    TagMemoryOverrunError,
    TagMemoryLockedError,
    InsufficientPower,
    NonspecificTagError,
    NoResponseFromTag,
    NonspecificReaderError,
  }
}
