

using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Container class encapsulating all of the GPI status objects.
  /// This is for internal use only, and does not appear in the generated
  /// documentation.
  /// </summary>
  [XmlInclude(typeof (GpiStatus))]
  public class GpiStatusGroup : IEnumerable
  {
    private bool _GPICollectionCreatedBySerializer;

    internal List<GpiStatus> gpiStatuses { get; set; }

    internal GpiStatusGroup()
    {
      this._GPICollectionCreatedBySerializer = true;
      this.gpiStatuses = new List<GpiStatus>();
    }

    /// <summary>Returns the status for the specified GPI port.</summary>
    /// <returns>The GPI status or throws an exception if the port does not exist.</returns>
    public GpiStatus GetGpi(ushort gpiNumber)
    {
      foreach (GpiStatus gpiStatuse in this.gpiStatuses)
      {
        if ((int) gpiStatuse.PortNumber == (int) gpiNumber)
          return gpiStatuse;
      }
      throw new LLRPSdkException("Invalid GPI number specified : " + gpiNumber.ToString());
    }

    /// <summary>
    /// Adds the provided GPI status object to the collection.
    /// For internal library use only.
    /// </summary>
    /// <param name="status">The GPIStatus object to add.</param>
    public void Add(object status)
    {
      if (!this._GPICollectionCreatedBySerializer)
        throw new LLRPSdkException("Illegal operation - cannot Add to GpiStatusGroup object!");
      this.gpiStatuses.Add((GpiStatus) status);
    }

    /// <summary>
    /// Get a handle to the the IEnumerator interface of the GPI
    /// status collection.
    /// </summary>
    /// <returns>
    /// An IEnumerator interface object to the GPI status
    /// collection.</returns>
    public IEnumerator GetEnumerator() => (IEnumerator) this.gpiStatuses.GetEnumerator();
  }
}
