

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Class to carry the results of a
  /// <see cref="M:LLPRSdk.LLPRReader.LockTag(LLRPSdk.LockTagParams)" />
  /// operation
  /// </summary>
  public class LockTagResult
  {
    /// <summary>The result of the lock operation.</summary>
    public TagLockOpResult LockResult { get; set; }
  }
}
