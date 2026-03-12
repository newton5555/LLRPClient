
using System;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Specialization of <see cref="T:LLRPSdk.TagOpResult" /> class that contains the
  /// results of an individual tag read operation, as reported in the
  /// <see cref="T:LLRPSdk.TagOpReport" /> parameter of a
  /// <see cref="E:LLRPSdk.LLRPReader.TagOpComplete" /> event.
  /// </summary>
  public class TagReadOpResult : TagOpResult
  {
    /// <summary>The result of the tag read operation.</summary>
    public ReadResultStatus Result;
    /// <summary>The data read from the tag.</summary>
    public TagData Data = new TagData();
    /// <summary />
    [Obsolete("This property has been renamed Data.", true)]
    public TagData ReadData;
  }
}
