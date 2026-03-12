
using System;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// The action to perform when a tag filter matches or doesn't match
  /// </summary>
  [Serializable]
  public enum StateUnawareAction
  {
    /// <summary>Assert the selected flag</summary>
    Select,
    /// <summary>Deassert the selected flag</summary>
    Unselect,
    /// <summary>Leave the selected flag as it is</summary>
    DoNothing,
  }
}
