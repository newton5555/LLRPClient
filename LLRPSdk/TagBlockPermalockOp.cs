
#nullable disable
namespace LLRPSdk
{
  /// <summary>Used to perform a block permalock operation.</summary>
  public class TagBlockPermalockOp : TagOp
  {
    private TagData _AccessPassword = new TagData();

    /// <summary>
    /// Tag access password.  Required only if an access
    /// password has been set for the target memory bank.
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

    /// <summary>The default MemoryBank is EPC (i.e. Bank 1)</summary>
    public TagBlockPermalockOp() => this.MemoryBank = MemoryBank.Epc;

    /// <summary>A mask, which specifies the blocks to permalock.</summary>
    public BlockPermalockMask BlockMask { get; set; }

    /// <summary>
    /// The index pointer to the block of UserMemory where BlockPermalocking should start
    /// </summary>
    public ushort BlockPointer { get; set; }

    /// <summary>
    /// The memory bank on which to perform the TagBlockPermalock operation.
    /// Note: If unset, the value defaults to the EPC memory bank.
    /// </summary>
    public MemoryBank MemoryBank { get; set; }
  }
}
