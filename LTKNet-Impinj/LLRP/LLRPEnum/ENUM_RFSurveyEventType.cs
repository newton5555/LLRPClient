
using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_RFSurveyEventType
  {
    [XmlEnum(Name = "Start_Of_RFSurvey")] Start_Of_RFSurvey,
    [XmlEnum(Name = "End_Of_RFSurvey")] End_Of_RFSurvey,
  }
}
