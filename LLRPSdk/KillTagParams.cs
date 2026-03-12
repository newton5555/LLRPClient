

#nullable disable
namespace LLRPSdk
{
    /// <summary>
    /// Class used to carry the configuration for the
    /// <see cref="M:LLRPSdk.LLRPReader.KillTag(LLRPSdk.KillTagParams)" />
    /// operation
    /// </summary>
    public class KillTagParams : ReadTagMemoryParams
  {
    /// <summary>
    /// Hexadecimal string representing the kill password of the tag. The
    /// kill password is a 32-bit number so this string may be up to 8
    /// hexadecimal characters in length.
    /// </summary>
    public string KillPassword { get; set; }
  }
}
