

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Specialization of ProgramTagMemoryParams class used to carry the
  /// configuration for a
  /// <see cref="M:LLRPSdk.LLRPReader.ProgramKillPassword(LLRPSdk.ProgramKillPasswordParams)" />
  /// operation.
  /// </summary>
  public class ProgramKillPasswordParams : ProgramTagMemoryParams
  {
    /// <summary>
    /// The kill password to write to memory. This should be specified as a hex string.
    /// </summary>
    public string NewKillPassword { get; set; }

    /// <summary>
    /// Creates a new instance of the ProgramKillPasswordParams class that
    /// initializes NewKillPassword to "00000000".
    /// </summary>
    public ProgramKillPasswordParams() => this.NewKillPassword = "00000000";
  }
}
