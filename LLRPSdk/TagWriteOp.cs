
#nullable disable
namespace LLRPSdk
{
  /// <summary>Class used to specify a tag write operation.</summary>
  public class TagWriteOp : TagOp
  {
    private TagData _AccessPassword = new TagData();
    /// <summary>Tag memory bank to write to.</summary>
    public MemoryBank MemoryBank;
    /// <summary>
    /// Numeric pointer to first word to write to (e.g. the first word would be word 0).
    /// </summary>
    public ushort WordPointer;
    /// <summary>
    /// Data to write into the tag memory bank, starting at the word pointed to by WordPointer.
    /// </summary>
    public TagData Data = new TagData();

    /// <summary>
    /// Tag access password.  Required only if the tag target memory bank
    /// is locked and an access password has been set.
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
  }
}
