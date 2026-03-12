
#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Parent class that is specialized to define the configuration for specific
  /// tag read operations.
  /// </summary>
  public class ReadTagMemoryParams
  {
    /// <summary>
    ///  The access password to use during the read operation. This should be specified as a hex string.
    /// </summary>
    public string AccessPassword { get; set; }

    /// <summary>The antenna port to use for the read operation.</summary>
    public ushort AntennaPortNumber { get; set; }

    /// <summary>
    /// The number of times to retry the write operation if a failure occurs.
    /// </summary>
    public ushort RetryCount { get; set; }

    /// <summary>
    /// The EPC of the tag to read. This should be specified as a hex string.
    /// </summary>
    public string TargetTag { get; set; }

    /// <summary>
    /// The amount of time (in milliseconds) to wait for the read operation to complete.
    /// </summary>
    public int TimeoutInMs { get; set; }

    /// <summary>
    ///  Creates a new instance of the ReadTagMemoryParams class that
    /// initializes all member variables to their default values.
    /// </summary>
    public ReadTagMemoryParams()
    {
      this.AccessPassword = "00000000";
      this.AntennaPortNumber = (ushort) 0;
      this.RetryCount = (ushort) 0;
      this.TargetTag = "";
      this.TimeoutInMs = 5000;
    }
  }
}
