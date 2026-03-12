
using System;

#nullable disable
namespace LLRPSdk
{
  /// <summary>Enum for defining the operation state for a sequence.</summary>
  [Serializable]
  public enum SequenceState
  {
    /// <summary>Indicates that all sequence operations are disabled.</summary>
    Disabled,
    /// <summary>Indicates that all sequence operations are active.</summary>
    Active,
  }
}
