

using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_C1G2SpectralMaskIndicator
  {
    [XmlEnum(Name = "Unknown")] Unknown,
    [XmlEnum(Name = "SI")] SI,
    [XmlEnum(Name = "MI")] MI,
    [XmlEnum(Name = "DI")] DI,
  }
}
