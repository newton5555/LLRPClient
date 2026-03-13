
using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_C1G2MValue
  {
    [XmlEnum(Name = "MV_FM0")] MV_FM0,
    [XmlEnum(Name = "MV_2")] MV_2,
    [XmlEnum(Name = "MV_4")] MV_4,
    [XmlEnum(Name = "MV_8")] MV_8,
  }
}
