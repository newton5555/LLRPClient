
#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Specialization of <see cref="T:LLRPSdk.TagOpResult" /> class that contains the
  /// results of an individual tag write operation, as reported in the
  /// <see cref="T:LLRPSdk.TagOpReport" /> parameter of a
  /// <see cref="E:LLRPSdk.LLRPReader.TagOpComplete" /> event.
  /// </summary>
  public class TagWriteOpResult : TagOpResult
  {
    /// <summary>
    /// The number of 16-bit words written by the tag write operation.
    /// </summary>
    public ushort NumWordsWritten;
    /// <summary>The result of the tag write operation.</summary>
    public WriteResultStatus Result;
    /// <summary>
    /// Indicates whether the tag write was a standard or BlockWrite
    /// operation.
    /// </summary>
    public bool IsBlockWrite;
  }
}
