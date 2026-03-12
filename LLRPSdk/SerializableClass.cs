
using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Parent class that enables child classes to output contents in XML format.
  /// </summary>
  public class SerializableClass
  {
    /// <summary>Output the class contents as an XML string.</summary>
    /// <returns>The class contents in XML string format.</returns>
    public string ToXmlString()
    {
      Type type = this.GetType();
      StringWriter stringWriter = new StringWriter();
      new XmlSerializer(type).Serialize((TextWriter) stringWriter, (object) this);
      return stringWriter.ToString();
    }

    /// <summary>Output the class contents as an XML file.</summary>
    /// <param name="path">path and filename of the XML file.</param>
    public void Save(string path) => File.WriteAllText(path, this.ToXmlString(), Encoding.Default);
  }
}
