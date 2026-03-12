
#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Parent class used to carry the results of any tag read
  /// operations, including not just the results of the read operation, but
  /// also any lock or verify operations, if the program operation is
  /// configured to use them.
  /// </summary>
  public class ReadTagMemoryResult
  {
    /// <summary>
    /// Member that contains the results of the tag read operation.
    /// </summary>
    public TagReadOpResult ReadResult { get; set; }

    /// <summary>
    /// Member that contains the results of the tag lock operation,
    /// if configured.
    /// </summary>
    public TagLockOpResult LockResult { get; set; }

    /// <summary>
    /// Member that contains the results of a tag verify operation,
    /// if configured.
    /// </summary>
    public TagKillOpResult KillResult { get; set; }
  }
}
