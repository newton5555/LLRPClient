
using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_AccessSpecStopTriggerType
  {
    [XmlEnum(Name = "Null")] Null,
    [XmlEnum(Name = "Operation_Count")] Operation_Count,
  }
}
