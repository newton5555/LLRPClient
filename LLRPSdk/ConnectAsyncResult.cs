

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Enumeration used to specify the result of an asynchronous connection attempt
  /// </summary>
  public enum ConnectAsyncResult
  {
    /// <summary>Successfully connected to the reader</summary>
    Success,
    /// <summary>Failed to connect to the reader</summary>
    Failure,
  }
}
