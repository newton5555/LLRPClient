
using System;

#nullable disable
namespace LLRPSdk
{
  /// <summary></summary>
  [Serializable]
  public enum TagFilterOp
  {
    /// <summary>Only select tags that match the filter.</summary>
    Match,
    /// <summary>Only select tags that do not match the filter.</summary>
    NotMatch,
  }
}
