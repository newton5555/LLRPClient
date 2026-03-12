
#nullable disable
namespace LLRPSdk
{
  /// <summary>Contains the results of a margin read operation</summary>
  public class TagMarginReadOpResult : TagOpResult
  {
    private MarginReadResult _Result;

    /// <summary>The results of the margin read operation.</summary>
    public MarginReadResult Result
    {
      get => this._Result;
      set => this._Result = value;
    }
  }
}
