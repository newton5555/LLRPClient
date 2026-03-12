

using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_AntennaEventType
  {
    [XmlEnum(Name = "Antenna_Disconnected")] Antenna_Disconnected,
    [XmlEnum(Name = "Antenna_Connected")] Antenna_Connected,
  }
}
