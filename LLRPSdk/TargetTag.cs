
#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Helper class used to identify a specific tag or set of tags on which to
  /// perform an operation.
  /// </summary>
  public class TargetTag
  {
    /// <summary>The tag memory bank to filter on.</summary>
    public MemoryBank MemoryBank { get; set; }

    /// <summary>
    /// A pointer to the bit to start the filter comparison on.
    /// </summary>
    public ushort BitPointer { get; set; }

    /// <summary>
    /// The specific data value to filter on.
    /// Should be expressed as a hex string.
    /// </summary>
    public string Data { get; set; }

    /// <summary>
    /// A mask, in hex, which specifies which bits to
    /// match on and which to ignore. If a mask is not
    /// specified, all bits will be used to match.
    /// </summary>
    public string Mask { get; set; }
  }
}
