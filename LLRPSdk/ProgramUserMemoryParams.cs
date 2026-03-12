
#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Class to carry the configuration for a
  /// <see cref="M:LLRPSdk.LLRPReader.ProgramUserMemory(LLRPSdk.ProgramUserMemoryParams)" />
  /// operation
  /// </summary>
  public class ProgramUserMemoryParams : ProgramTagMemoryParams
  {
    /// <summary>
    /// A hexadecimal string representing the data to program.
    /// </summary>
    public string NewUserBlock { get; set; }

    /// <summary>The word to start writing from.</summary>
    public ushort WordPointer { get; set; }

    /// <summary>
    /// Initializes a new instance of the ProgramUserMemoryParams class,
    /// setting NewUserBlock to "" and WordPointer to 0.
    /// </summary>
    public ProgramUserMemoryParams()
    {
      this.NewUserBlock = "";
      this.WordPointer = (ushort) 0;
    }
  }
}
