

using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_GetReaderCapabilitiesRequestedData
  {
    [XmlEnum(Name = "All")] All,
    [XmlEnum(Name = "General_Device_Capabilities")] General_Device_Capabilities,
    [XmlEnum(Name = "LLRP_Capabilities")] LLRP_Capabilities,
    [XmlEnum(Name = "Regulatory_Capabilities")] Regulatory_Capabilities,
    [XmlEnum(Name = "LLRP_Air_Protocol_Capabilities")] LLRP_Air_Protocol_Capabilities,
  }
}
