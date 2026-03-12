
using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_AirProtocols
  {
    [XmlEnum(Name = "Unspecified")] Unspecified,
    [XmlEnum(Name = "EPCGlobalClass1Gen2")] EPCGlobalClass1Gen2,
  }
}
