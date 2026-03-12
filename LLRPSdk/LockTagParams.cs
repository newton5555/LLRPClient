

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Class to carry the configuration for a
  /// <see cref="M:LLRPSdk.LLRPReader.LockTag(LLRPSdk.LockTagParams)" />
  /// operation, which is flagged for obsolescence.
  /// </summary>
  public class LockTagParams : ReadTagMemoryParams
  {
    /// <summary>The type of lock operation to perform.</summary>
    public LockType LockUserMemory { get; set; }

    /// <summary>
    /// Default constructor; initializes LockUserMemory to LockType.None.
    /// </summary>
    public LockTagParams() => this.LockUserMemory = LockType.None;
  }
}
