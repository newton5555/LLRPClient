
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Container class encapsulating all of the GPO status objects.
  /// </summary>
  [XmlInclude(typeof (GpoStatus))]
  public class GpoStatusGroup : IEnumerable
  {
    private bool _GPOCollectionCreatedBySerializer;

    internal List<GpoStatus> gpoStatuses { get; set; }

    internal GpoStatusGroup()
    {
      this._GPOCollectionCreatedBySerializer = true;
      this.gpoStatuses = new List<GpoStatus>();
    }

    /// <summary>Returns the status for the specified GPO port.</summary>
    /// <returns>The GPO status or throws an exception if the port does not exist.</returns>
    public GpoStatus GetGpo(ushort gpoNumber)
    {
      foreach (GpoStatus gpoStatus in this.gpoStatuses)
      {
        if ((int) gpoStatus.PortNumber == (int) gpoNumber)
          return gpoStatus;
      }
      throw new LLRPSdkException("Invalid GPO number specified : " + gpoNumber.ToString());
    }

    /// <summary>
    /// Adds the provided GPO status object to the collection.
    /// For internal library use only.
    /// </summary>
    /// <param name="status">The GPOStatus object to add.</param>
    public void Add(object status)
    {
      if (!this._GPOCollectionCreatedBySerializer)
        throw new LLRPSdkException("Illegal operation - cannot Add to GpoStatusGroup object!");
      this.gpoStatuses.Add((GpoStatus) status);
    }

    /// <summary>
    /// Get a handle to the the IEnumerator interface of the GPO
    /// status collection.
    /// </summary>
    /// <returns>
    /// An IEnumerator interface object to the GPO status
    /// collection.</returns>
    public IEnumerator GetEnumerator() => (IEnumerator) this.gpoStatuses.GetEnumerator();
  }
}
