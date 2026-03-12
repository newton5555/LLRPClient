

#nullable disable
namespace LLRPSdk
{
    /// <summary>
    /// Class used to encapsulate the details of a
    /// <see cref="E:LLRPSdk.LLRPReader.GpiChanged" /> reader event.
    /// </summary>
    public class GpiEvent
  {
    /// <summary>Parameter defining GPI port number for event</summary>
    public ushort PortNumber;
    /// <summary>Parameter defining GPI state for event</summary>
    public bool State;
  }
}
