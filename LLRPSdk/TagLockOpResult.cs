
#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Specialization of <see cref="T:LLRPSdk.TagOpResult" /> class that contains the
  /// results of an individual tag lock operation, as reported in the
  /// <see cref="T:LLRPSdk.TagOpReport" /> parameter of a
  /// <see cref="E:LLRPSdk.LLRPReader.TagOpComplete" /> event.
  /// </summary>
  public class TagLockOpResult : TagOpResult
  {
    /// <summary>The results of the tag lock operation.</summary>
    public LockResultStatus Result;
  }
}
