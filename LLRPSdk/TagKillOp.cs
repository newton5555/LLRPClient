
#nullable disable
namespace LLRPSdk
{
  /// <summary>
  ///  Class used to specify a tag kill operation.
  ///  WARNING:- once killed, a tag will permanently cease functioning.
  /// </summary>
  public class TagKillOp : TagOp
  {
    /// <summary>
    /// Value of the kill password; this MUST match the kill password
    /// already written to the reserved memory bank, or the operation
    /// will fail.
    /// </summary>
    public TagData KillPassword = new TagData();
  }
}
