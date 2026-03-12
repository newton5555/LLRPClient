
using System;

#nullable disable
namespace LLRPSdk
{
  /// <summary>The possible inventory tag filter combinations.</summary>
  [Serializable]
  public enum TagFilterMode
  {
    /// <summary>No inventory tag filter.</summary>
    None,
    /// <summary>Filter only on conditions defined in Filter 1.</summary>
    OnlyFilter1,
    /// <summary>Filter only on conditions defined in Filter 2.</summary>
    OnlyFilter2,
    /// <summary>Filter on conditions defined in Filters 1 AND 2.</summary>
    Filter1AndFilter2,
    /// <summary>Filter on conditions defined in Filters 2 OR 2.</summary>
    Filter1OrFilter2,
    /// <summary>
    /// Filter on conditions defined in the tag select filter list.
    /// </summary>
    UseTagSelectFilters,
  }
}
