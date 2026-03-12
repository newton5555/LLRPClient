

using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_ROSpecEventType
  {
    [XmlEnum(Name = "Start_Of_ROSpec")] Start_Of_ROSpec,
    [XmlEnum(Name = "End_Of_ROSpec")] End_Of_ROSpec,
    [XmlEnum(Name = "Preemption_Of_ROSpec")] Preemption_Of_ROSpec,
  }
}
