
#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Specialization of the <see cref="T:LLRPSdk.ReadTagMemoryParams" /> class used to carry the
  /// configuration for a
  /// <see cref="M:LLRPSdk.LLRPReader.ReadUserMemory(LLRPSdk.ReadUserMemoryParams)" />
  /// operation.
  /// </summary>
  public class ReadUserMemoryParams : ReadTagMemoryParams
  {
    /// <summary>The number of words to read.</summary>
    public ushort WordCount { get; set; }

    /// <summary>The word to start writing from.</summary>
    public ushort WordPointer { get; set; }

    /// <summary>
    /// Creates a new instance of the ReadUserMemoryParams class that
    /// initializes WordCount and WordPointer to 0.
    /// </summary>
    public ReadUserMemoryParams()
    {
      this.WordCount = (ushort) 0;
      this.WordPointer = (ushort) 0;
    }
  }
}
