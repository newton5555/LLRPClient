
using System;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Parent class that is specialized to define the configuration for specific
  /// tag programming operations.
  /// </summary>
  public class ProgramTagMemoryParams
  {
    /// <summary>
    ///  The access password to use during the write operation. This should be specified as a hex string.
    /// </summary>
    public string AccessPassword { get; set; }

    /// <summary>The antenna port to use for the write operation.</summary>
    public ushort AntennaPortNumber { get; set; }

    /// <summary>
    /// The number of words to write per block. The chip must support bock writes.
    /// The reader is limited to 32-bit (2 word) block writes.
    /// </summary>
    public ushort BlockWriteWordCount { get; set; }

    /// <summary />
    [Obsolete("This property was removed because it is unnecessary. If BlockWriteWord count is set, a block write will be performed.", true)]
    public bool IsBlockWriteUsed { get; set; }

    /// <summary>
    /// Specifies whether or not to verify the write, by performing a separate read operation.
    /// </summary>
    public bool IsWriteVerified { get; set; }

    /// <summary>The lock type to perform.</summary>
    public TagLockState LockType { get; set; }

    /// <summary>
    /// The number of times to retry the write operation if a failure occurs.
    /// </summary>
    public ushort RetryCount { get; set; }

    /// <summary>
    /// The EPC of the tag to write to. This should be specified as a hex string.
    /// </summary>
    public string TargetTag { get; set; }

    /// <summary>
    /// The amount of time (in milliseconds) to wait for the write operation to complete.
    /// </summary>
    public int TimeoutInMs { get; set; }

    /// <summary>
    /// Creates a new instance of the ProgramTagMemoryParams class that
    /// initializes all member variables to their default values.
    /// </summary>
    public ProgramTagMemoryParams()
    {
      this.AccessPassword = "00000000";
      this.AntennaPortNumber = (ushort) 0;
      this.BlockWriteWordCount = (ushort) 0;
      this.IsWriteVerified = false;
      this.LockType = TagLockState.None;
      this.RetryCount = (ushort) 0;
      this.TargetTag = "";
      this.TimeoutInMs = 5000;
    }
  }
}
