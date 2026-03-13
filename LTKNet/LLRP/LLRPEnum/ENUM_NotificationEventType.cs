

using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_NotificationEventType
  {
    [XmlEnum(Name = "Upon_Hopping_To_Next_Channel")] Upon_Hopping_To_Next_Channel,
    [XmlEnum(Name = "GPI_Event")] GPI_Event,
    [XmlEnum(Name = "ROSpec_Event")] ROSpec_Event,
    [XmlEnum(Name = "Report_Buffer_Fill_Warning")] Report_Buffer_Fill_Warning,
    [XmlEnum(Name = "Reader_Exception_Event")] Reader_Exception_Event,
    [XmlEnum(Name = "RFSurvey_Event")] RFSurvey_Event,
    [XmlEnum(Name = "AISpec_Event")] AISpec_Event,
    [XmlEnum(Name = "AISpec_Event_With_Details")] AISpec_Event_With_Details,
    [XmlEnum(Name = "Antenna_Event")] Antenna_Event,
  }
}
