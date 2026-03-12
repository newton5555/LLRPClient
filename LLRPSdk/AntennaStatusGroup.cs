

using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Container class encapsulating all of the antenna status objects.
  /// This is for internal use only, and does not appear in the generated
  /// documentation.
  /// </summary>
  [XmlInclude(typeof (AntennaStatus))]
  public class AntennaStatusGroup : IEnumerable
  {
    private bool _AntennaCollectionCreatedBySerializer;

    internal List<AntennaStatus> antennaStatuses { get; set; }

    internal AntennaStatusGroup()
    {
      this._AntennaCollectionCreatedBySerializer = true;
      this.antennaStatuses = new List<AntennaStatus>();
    }

    /// <summary>Returns the status for the specified antenna port.</summary>
    /// <returns>
    /// The antenna status or throws an exception if the port does not exist.
    /// </returns>
    public AntennaStatus GetAntenna(ushort antennaNumber)
    {
      foreach (AntennaStatus antennaStatuse in this.antennaStatuses)
      {
        if ((int) antennaStatuse.PortNumber == (int) antennaNumber)
          return antennaStatuse;
      }
      throw new LLRPSdkException("Invalid antenna number specified : " + antennaNumber.ToString());
    }

    /// <summary>
    /// Adds the provided antenna status object to the collection.
    /// For internal library use only.
    /// </summary>
    /// <param name="status">The AntennaStatus object to add.</param>
    public void Add(object status)
    {
      if (!this._AntennaCollectionCreatedBySerializer)
        throw new LLRPSdkException("Invalid operation - cannot Add to AntennaStatusGroup object!");
      this.antennaStatuses.Add((AntennaStatus) status);
    }

    /// <summary>
    /// Get a handle to the the IEnumerator interface of the antenna
    /// status collection.
    /// </summary>
    /// <returns>
    /// An IEnumerator interface object to the antenna status
    /// collection.</returns>
    public IEnumerator GetEnumerator() => (IEnumerator) this.antennaStatuses.GetEnumerator();
  }
}
