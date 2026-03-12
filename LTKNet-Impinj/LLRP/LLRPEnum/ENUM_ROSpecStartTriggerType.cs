

using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_ROSpecStartTriggerType
  {
    [XmlEnum(Name = "Null")] Null,
    [XmlEnum(Name = "Immediate")] Immediate,
    [XmlEnum(Name = "Periodic")] Periodic,
    [XmlEnum(Name = "GPI")] GPI,
  }
}
