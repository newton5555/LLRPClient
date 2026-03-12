

using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_ROReportTriggerType
  {
    [XmlEnum(Name = "None")] None,
    [XmlEnum(Name = "Upon_N_Tags_Or_End_Of_AISpec")] Upon_N_Tags_Or_End_Of_AISpec,
    [XmlEnum(Name = "Upon_N_Tags_Or_End_Of_ROSpec")] Upon_N_Tags_Or_End_Of_ROSpec,
  }
}
