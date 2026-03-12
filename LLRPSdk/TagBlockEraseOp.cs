#nullable disable
namespace LLRPSdk
{
  /// <summary>Used to perform a block erase operation.</summary>
  public class TagBlockEraseOp : TagOp
  {
    private TagData _AccessPassword = new TagData();

    /// <summary>
    /// Tag access password. Required only if an access password has been set.
    /// </summary>
    public TagData AccessPassword
    {
      get => this._AccessPassword;
      set
      {
        if (value == null)
          return;
        this._AccessPassword = value;
      }
    }

    /// <summary>The memory bank on which to perform block erase.</summary>
    public MemoryBank MemoryBank { get; set; } = MemoryBank.User;

    /// <summary>The starting word pointer for the erase operation.</summary>
    public ushort WordPointer { get; set; }

    /// <summary>The number of words to erase.</summary>
    public ushort WordCount { get; set; }
  }
}
