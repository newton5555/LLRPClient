
#nullable disable
namespace LLRPSdk
{
  /// <summary>Used to perform a margin read operation</summary>
  public class TagMarginReadOp : TagOp
  {
    private TagData _AccessPassword = new TagData();
    private MemoryBank _MemoryBank;
    private ushort _BitPointer;
    private MarginReadMask _MarginMask;

    /// <summary>
    /// Tag access password. Required only if an access
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

    /// <summary>Tag memory bank to perform margin read.</summary>
    public MemoryBank MemoryBank
    {
      get => this._MemoryBank;
      set => this._MemoryBank = value;
    }

    /// <summary>Numeric pointer to first bit to read.</summary>
    public ushort BitPointer
    {
      get => this._BitPointer;
      set => this._BitPointer = value;
    }

    /// <summary>A mask, which specifies the mask to look for.</summary>
    public MarginReadMask MarginMask
    {
      get => this._MarginMask;
      set
      {
        if (value == null)
          return;
        this._MarginMask = value;
      }
    }
  }
}
