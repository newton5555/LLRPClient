using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace LLRPSdk
{
    /// <summary>
    /// Encapsulates reader settings with metadata for safe XML import/export.
    /// </summary>
    public class ReaderConfigurationPack : SerializableClass
    {
        public string Brand { get; set; } = "LLRP";
        public string Model { get; set; } = "Generic";
        public uint AntennaCount { get; set; }
        public ushort GpiCount { get; set; }
        public ushort GpoCount { get; set; }
        public DateTime ExportedAt { get; set; } = DateTime.Now;
        public string AddRoSpecXml { get; set; }
        public string SetReaderConfigXml { get; set; }

        public static ReaderConfigurationPack FromXmlString(string xml)
        {
            return (ReaderConfigurationPack)new XmlSerializer(typeof(ReaderConfigurationPack)).Deserialize(new StringReader(xml));
        }
    }
}
