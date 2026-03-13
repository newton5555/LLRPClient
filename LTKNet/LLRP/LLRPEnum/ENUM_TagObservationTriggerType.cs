

using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_TagObservationTriggerType
  {
    [XmlEnum(Name = "Upon_Seeing_N_Tags_Or_Timeout")] Upon_Seeing_N_Tags_Or_Timeout,
    [XmlEnum(Name = "Upon_Seeing_No_More_New_Tags_For_Tms_Or_Timeout")] Upon_Seeing_No_More_New_Tags_For_Tms_Or_Timeout,
    [XmlEnum(Name = "N_Attempts_To_See_All_Tags_In_FOV_Or_Timeout")] N_Attempts_To_See_All_Tags_In_FOV_Or_Timeout,
  }
}
