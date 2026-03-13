

using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_RFSurveySpecStopTriggerType
  {
    [XmlEnum(Name = "Null")] Null,
    [XmlEnum(Name = "Duration")] Duration,
    [XmlEnum(Name = "N_Iterations_Through_Frequency_Range")] N_Iterations_Through_Frequency_Range,
  }
}
