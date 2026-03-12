

#nullable disable
namespace LLRPSdk
{
    /// <summary>
    /// Specialization of the <see cref="T:LLRPSdk.ProgramTagMemoryParams" /> class used to
    /// carry the configuration for a
    /// <see cref="M:LLRPSdk.LLRPReader.ProgramEpc(LLRPSdk.ProgramEpcParams)" />
    /// operation.
    /// </summary>
    public class ProgramEpcParams : ProgramTagMemoryParams
  {
    /// <summary>
    /// The EPC to write to the tag. This should be specified as a hex string.
    /// </summary>
    public string NewEpc { get; set; }

    /// <summary>
    /// Creates a new instance of the ProgramEpcParams class that
    /// initializes NewEpc to "".
    /// </summary>
    public ProgramEpcParams() => this.NewEpc = "";
  }
}
