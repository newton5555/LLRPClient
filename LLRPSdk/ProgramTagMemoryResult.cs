
#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Parent Class to carry the results of tag memory programming
  /// operations, including not just the results of the write operation, but
  /// also any lock or verify operations, if the program operation is
  /// configured to use them.
  /// </summary>
  public class ProgramTagMemoryResult
  {
    /// <summary>
    /// Member that contains the results of the tag write operation.
    /// </summary>
    public TagWriteOpResult WriteResult { get; set; }

    /// <summary>
    /// Member that contains the results of the tag lock operation,
    /// if configured.
    /// </summary>
    public TagLockOpResult LockResult { get; set; }

    /// <summary>
    /// Member that contains the results of a tag verify operation,
    /// if configured.
    /// </summary>
    public TagReadOpResult VerifyResult { get; set; }
  }
}
