using System;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Exception class for uniquely identifying SDK exceptions.
  /// </summary>
  public class LLRPSdkException : Exception
  {
    /// <summary>Creates an OctaneSdkException object with no message.</summary>
    public LLRPSdkException()
    {
    }

    /// <summary>Creates an OctandSdkException object with a message.</summary>
    /// <param name="message">The message to include with the exception.</param>
    public LLRPSdkException(string message)
      : base(message)
    {
    }
  }
}
