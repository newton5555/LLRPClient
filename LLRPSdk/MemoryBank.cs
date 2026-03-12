

using System;

#nullable disable
namespace LLRPSdk
{
  /// <summary>Enum for defining the tag memory bank to access.</summary>
  [Serializable]
  public enum MemoryBank
  {
    /// <summary>Tag reserved memory.</summary>
    Reserved,
    /// <summary>Tag Electronic Product Code (EPC) memory</summary>
    Epc,
    /// <summary>Tag identifier (TID) memory</summary>
    Tid,
    /// <summary>Tag user memory</summary>
    User,
  }
}
