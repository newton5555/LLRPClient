
#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Parent class that contains the results of an individual tag operation,
  /// as reported in the TagOpReport parameter of the
  /// <see cref="E:LLRPSdk.LLRPReader.TagOpComplete" />
  /// event.
  /// </summary>
  public class TagOpResult
  {
    /// <summary>
    /// Details of the individual tag on which the operation was performed
    /// </summary>
    public Tag Tag = new Tag();
    /// <summary>
    /// The identifier of the tag operation that produced these results.
    /// </summary>
    public ushort OpId;
    /// <summary>
    /// The identifier of the tag operation sequence that contains this
    /// operation.
    /// </summary>
    public uint SequenceId;
  }
}
