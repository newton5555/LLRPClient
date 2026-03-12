
using System.Collections;
using System.Collections.Generic;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Container class used to encapsulate all tag operation results,
  /// as reported in the <see cref="T:LLRPSdk.TagOpReport" /> parameter of the
  /// <see cref="E:LLRPSdk.LLRPReader.TagOpComplete" />
  /// event.
  /// </summary>
  public class TagOpReport : IEnumerable
  {
    /// <summary>
    /// A list of detailed tag operation results; each must be cast to the
    /// appropriate tag operation result type to extract all of the contents.
    /// </summary>
    public List<TagOpResult> Results = new List<TagOpResult>();

    /// <summary>
    /// Get access to the C# IEnumerator interface of the tag operation
    /// results list.
    /// </summary>
    /// <returns>
    /// The IEnumerator interface to the list of tag operation results.
    /// </returns>
    public IEnumerator GetEnumerator() => (IEnumerator) this.Results.GetEnumerator();
  }
}
