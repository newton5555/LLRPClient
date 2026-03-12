
#nullable disable
namespace LLRPSdk
{
  /// <summary>Parent class for tag operation definitions</summary>
  public class TagOp
  {
    private static ushort LastId;
    /// <summary>The tag operation unique identifier.</summary>
    public ushort Id;

    /// <summary>
    /// Constructor; automatically assigns a unique ID number.
    /// </summary>
    public TagOp() => this.SetDefaults();

    private void AutoAssignId()
    {
      lock (this)
      {
        if (TagOp.LastId == ushort.MaxValue)
          TagOp.LastId = (ushort) 1;
        else
          ++TagOp.LastId;
        this.Id = TagOp.LastId;
      }
    }

    private void SetDefaults() => this.AutoAssignId();
  }
}
