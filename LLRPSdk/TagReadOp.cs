
#nullable disable
namespace LLRPSdk
{
  /// <summary>Class used to specify a tag read operation.</summary>
  public class TagReadOp : TagOp
  {
    private TagData _AccessPassword = new TagData();
    /// <summary>
    /// Parameter defining Memory Bank to access for operation.
    /// </summary>
    public MemoryBank MemoryBank;
    /// <summary>
    /// Parameter defining Word Pointer to access for operation.
    /// </summary>
    public ushort WordPointer;
    /// <summary>
    /// Parameter defining the number of words to access for operation.
    /// </summary>
    public ushort WordCount;

    /// <summary>
    /// Parameter defining Access Password to use for operation.
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
