
#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Helper class used for accessing the tag kill or access passwords.
  /// </summary>
  public static class TagWordPointer
  {
    /// <summary>
    /// Constant used to point to Kill Password in reserved memory.
    /// </summary>
    public const ushort KillPassword = 0;
    /// <summary>
    /// Constant used to point to Access Password in reserved memory.
    /// </summary>
    public const ushort AccessPassword = 2;
  }
}
