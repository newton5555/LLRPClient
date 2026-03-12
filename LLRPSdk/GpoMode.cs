

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Defines the modes that may be used to configure the
  /// GPO ports when the Advanced GPO features are used.
  /// </summary>
  public enum GpoMode
  {
    /// <summary>Normal mode; GPO value must be set via SetGpo()</summary>
    Normal,
    /// <summary>
    /// Pulsed mode; GPO value must be set via SetGpo(), but will change to
    /// opposite state after a timeout.
    /// </summary>
    Pulsed,
    /// <summary>
    /// Reader Operational Status; GPO value of true means reader is
    /// operational, false means the reader is not operational.
    /// </summary>
    ReaderOperationalStatus,
    /// <summary>
    /// LLRP Connection Status; GPO value of true means the LLRP connection
    /// is operational, false means the LLRP connection is not operational.
    /// </summary>
    LLRPConnectionStatus,
    /// <summary>
    /// Reader Inventory Status; GPO value is true while the reader is
    /// performing a tag inventory.
    /// </summary>
    ReaderInventoryStatus,
    /// <summary>
    /// Network Connection Status; GPO value of true means that the Network
    /// connection is operational, false means that the network connection
    /// is not operational.
    /// </summary>
    NetworkConnectionStatus,
    /// <summary>
    /// Reader Inventory Tag Status; GPO value is true whenever a unique tag
    /// has been inventoried.
    /// </summary>
    ReaderInventoryTagsStatus,
  }
}
