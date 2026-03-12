

#nullable disable
namespace LLRPSdk
{
    /// <summary>
    /// Specialization of the <see cref="T:LLRPSdk.ProgramTagMemoryParams" /> class used to carry the
    /// configuration for a
    /// <see cref="M:LLRPSdk.LLRPReader.ProgramAccessPassword(LLRPSdk.ProgramAccessPasswordParams)" />
    /// operation
    /// </summary>
    public class ProgramAccessPasswordParams : ProgramTagMemoryParams
  {
    /// <summary>
    /// The access password to write to memory. This should be specified as a hex string.
    /// </summary>
    public string NewAccessPassword { get; set; }

    /// <summary>
    /// Creates a new instance of the ReadKillPasswordParams class that
    /// initializes NewAccessPassword to "00000000".
    /// </summary>
    public ProgramAccessPasswordParams() => this.NewAccessPassword = "00000000";
  }
}
