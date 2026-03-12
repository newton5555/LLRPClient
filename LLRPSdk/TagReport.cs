
using System.Collections;
using System.Collections.Generic;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Container class used to encapsulate individual tag details returned
  /// from the reader.
  /// </summary>
  public class TagReport : IEnumerable
  {
    /// <summary>A list of tag details.</summary>
    public List<Tag> Tags = new List<Tag>();

    internal TagReport()
    {
    }

    /// <summary>
    /// Get access to the C# IEnumerator interface of the tag reports
    /// list.
    /// </summary>
    /// <returns>The IEnumerator interface to the list of tag reports.</returns>
    public IEnumerator GetEnumerator() => (IEnumerator) this.Tags.GetEnumerator();
  }
}
