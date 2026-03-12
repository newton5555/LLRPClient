

#nullable disable
namespace LLRPSdk
{
    /// <summary>
    /// Class used to encapsulate the details of a
    /// <see cref="E:LLRPSdk.LLRPReader.AntennaChanged" /> reader event.
    /// </summary>
    public class AntennaEvent
  {
    /// <summary>Port number of the affected antenna.</summary>
    public ushort PortNumber;
    /// <summary>State to which antenna changed, prompting the event.</summary>
    public AntennaEventType State;
  }
}
