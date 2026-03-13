

using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_ROSpecStopTriggerType
  {
    [XmlEnum(Name = "Null")] Null,
    [XmlEnum(Name = "Duration")] Duration,
    [XmlEnum(Name = "GPI_With_Timeout")] GPI_With_Timeout,
  }
}
