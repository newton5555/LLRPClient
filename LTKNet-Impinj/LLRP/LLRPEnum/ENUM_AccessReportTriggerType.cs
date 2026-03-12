
using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_AccessReportTriggerType
  {
    [XmlEnum(Name = "Whenever_ROReport_Is_Generated")] Whenever_ROReport_Is_Generated,
    [XmlEnum(Name = "End_Of_AccessSpec")] End_Of_AccessSpec,
  }
}
