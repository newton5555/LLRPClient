
using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Class encapsulating the reader status information, as returned by
  /// <see cref="M:LLRPSdk.LLRPReader.QueryStatus" />.
  /// </summary>
  public class Status : SerializableClass
  {
    /// <summary>Collection of antenna status information.</summary>
    [XmlArray("Antennas")]
    [XmlArrayItem("Antenna", typeof (AntennaStatus))]
    public AntennaStatusGroup Antennas { get; set; }

    /// <summary>Collection of GPI port status information.</summary>
    [XmlArray("Gpis")]
    [XmlArrayItem("Gpi", typeof (GpiStatus))]
    public GpiStatusGroup Gpis { get; set; }

    /// <summary>Collection of GPO port status information, when supported by reader.</summary>
    [XmlArray("GpoStates")]
    [XmlArrayItem("Gpo", typeof (GpoStatus))]
    public GpoStatusGroup GpoStates { get; set; }



    /// <summary />
    [Obsolete("There is no way to query the GPO state, so this has been removed.", true)]
    public object Gpos { get; set; }

    /// <summary>The reader connection status.</summary>
    public bool IsConnected { get; set; }

    /// <summary>
    /// Temperature reading from on-board reader temperature sensor, in Celsius.
    /// </summary>
    //public short TemperatureInCelsius { get; set; }



    /// <summary>The reader inventory status.</summary>
    public bool IsSingulating { get; set; }


    public object ReaderIdentity { get; set; }

    /// <summary />
    [Obsolete("This has been removed. Use IsConnected and IsSingulating instead.", true)]
    public object OperationState { get; set; }

    /// <summary>
    /// Obsolete - this has been removed. Use IsConnected instead.
    /// </summary>
    [Obsolete("This has been removed. Use IsConnected instead.", true)]
    public object Connection { get; set; }

    internal Status()
    {
      this.Antennas = new AntennaStatusGroup();
      this.Gpis = new GpiStatusGroup();
      this.GpoStates = new GpoStatusGroup();
      
    }

    /// <summary>Load reader status information from an XML string.</summary>
    /// <param name="xml">The XML string to parse.</param>
    /// <returns>A populated reader Status object.</returns>
    public static Status FromXmlString(string xml)
    {
      return (Status) new XmlSerializer(typeof (Status)).Deserialize((TextReader) new StringReader(xml));
    }

    /// <summary>Load reader status information from an XML file.</summary>
    /// <param name="path">The path and filename of the XML file.</param>
    /// <returns>A populated reader Status object.</returns>
    public static Status Load(string path)
    {
      return Status.FromXmlString(File.ReadAllText(path, Encoding.Default));
    }
  }
}
