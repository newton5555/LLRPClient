

using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_C1G2StateAwareTarget
  {
    [XmlEnum(Name = "SL")] SL,
    [XmlEnum(Name = "Inventoried_State_For_Session_S0")] Inventoried_State_For_Session_S0,
    [XmlEnum(Name = "Inventoried_State_For_Session_S1")] Inventoried_State_For_Session_S1,
    [XmlEnum(Name = "Inventoried_State_For_Session_S2")] Inventoried_State_For_Session_S2,
    [XmlEnum(Name = "Inventoried_State_For_Session_S3")] Inventoried_State_For_Session_S3,
  }
}
