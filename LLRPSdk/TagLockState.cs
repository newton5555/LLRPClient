
using System;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Enum for determining which lock operation to perform on the tag memory bank;
  /// this determines what state to leave the tag memory bank in.
  /// </summary>
  [Serializable]
  public enum TagLockState
  {
    /// <summary>
    /// Leave the tag memory bank in the 'Lock' state;
    /// the tag memory bank can subsequently be unlocked.
    /// </summary>
    Lock,
    /// <summary>
    /// Leave the tag memory bank in the 'Permalock' state;
    /// the tag memory bank can never subsequently be unlocked.
    /// </summary>
    Permalock,
    /// <summary>
    /// Leave the tag memory bank in the 'Permaunlock' state;
    /// the tag memory bank can never subsequently be locked.
    /// </summary>
    Permaunlock,
    /// <summary>
    /// Leave the tag memory bank in the 'Unlock' state;
    /// the tag memory bank can subsequently be locked.
    /// </summary>
    Unlock,
    /// <summary>Do not change the lock state of the tag memory bank.</summary>
    None,
  }
}
