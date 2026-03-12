
#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Specialization of <see cref="T:LLRPSdk.TagOpResult" /> class that contains the
  /// results of an individual tag kill operation, as reported in the
  /// <see cref="T:LLRPSdk.TagOpReport" /> parameter of a
  /// <see cref="E:LLRPSdk.LLRPReader.TagOpComplete" /> event.
  /// </summary>
  public class TagKillOpResult : TagOpResult
  {
    /// <summary>The result of the tag kill operation.</summary>
    public KillResultStatus Result;
  }
}
