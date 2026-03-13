

using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_GetReaderConfigRequestedData
  {
    [XmlEnum(Name = "All")] All,
    [XmlEnum(Name = "Identification")] Identification,
    [XmlEnum(Name = "AntennaProperties")] AntennaProperties,
    [XmlEnum(Name = "AntennaConfiguration")] AntennaConfiguration,
    [XmlEnum(Name = "ROReportSpec")] ROReportSpec,
    [XmlEnum(Name = "ReaderEventNotificationSpec")] ReaderEventNotificationSpec,
    [XmlEnum(Name = "AccessReportSpec")] AccessReportSpec,
    [XmlEnum(Name = "LLRPConfigurationStateValue")] LLRPConfigurationStateValue,
    [XmlEnum(Name = "KeepaliveSpec")] KeepaliveSpec,
    [XmlEnum(Name = "GPIPortCurrentState")] GPIPortCurrentState,
    [XmlEnum(Name = "GPOWriteData")] GPOWriteData,
    [XmlEnum(Name = "EventsAndReports")] EventsAndReports,
  }
}
