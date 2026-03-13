

using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_GPIPortState
  {
    [XmlEnum(Name = "Low")] Low,
    [XmlEnum(Name = "High")] High,
    [XmlEnum(Name = "Unknown")] Unknown,
  }
}
