

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Enum specifying the type of lock operation to perform.
  /// </summary>
  public enum LockType
  {
    /// <summary>
    /// Perform no lock operation on the tag memory bank;
    /// the tag memory bank lock status remains unaffected.
    /// </summary>
    None,
    /// <summary>
    /// Perform an 'Unlock' operation on the tag memory bank;
    /// the tag memory bank can never subsequently be unlocked.
    /// </summary>
    Unlocked,
    /// <summary>
    /// Perform a 'Lock' operation on the tag memory bank;
    /// the tag memory bank can subsequently be unlocked.
    /// </summary>
    Locked,
    /// <summary>
    /// Perform a 'Permaunlock' operation on the tag memory bank;
    /// the tag memory bank can never subsequently be locked.
    /// </summary>
    PermaUnlocked,
    /// <summary>
    /// Perform a 'Permalock' operation on the tag memory bank;
    /// the tag memory bank can never subsequently be unlocked.
    /// </summary>
    PermaLocked,
  }
}
