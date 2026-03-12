

#nullable disable
namespace LLRPSdk
{
    /// <summary>
    /// Class that contains the result of
    /// <see cref="M:LLRPSdk.LLRPReader.KillTag(LLRPSdk.KillTagParams)" />
    /// operation
    /// </summary>
    public class KillTagResult
  {
    /// <summary>Details of the tag kill operation result.</summary>
    public TagKillOpResult KillResult { get; set; }
  }
}
