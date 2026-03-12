

using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_KeepaliveTriggerType
  {
    [XmlEnum(Name = "Null")] Null,
    [XmlEnum(Name = "Periodic")] Periodic,
  }
}
