

using System;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Enum for defining the tag memory bank to perform lock operation on.
  /// </summary>
  [Serializable]
  public enum LockMemoryBank
  {
    /// <summary>
    /// Perform a lock operation on the Kill password memory region in
    /// the reserved memory bank.
    /// </summary>
    KillPassword,
    /// <summary>
    /// Perform a lock operation on the Access password memory region in
    /// the reserved memory bank.
    /// </summary>
    AccessPassword,
    /// <summary>Perform a lock operation on the EPC memory bank.</summary>
    Epc,
    /// <summary>Perform a lock operation on the TID memory bank.</summary>
    Tid,
    /// <summary>Perform a lock operation on the User memory bank.</summary>
    User,
  }
}
