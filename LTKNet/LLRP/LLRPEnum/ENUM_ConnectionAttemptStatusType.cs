

using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_ConnectionAttemptStatusType
  {
    [XmlEnum(Name = "Success")] Success,
    [XmlEnum(Name = "Failed_A_Reader_Initiated_Connection_Already_Exists")] Failed_A_Reader_Initiated_Connection_Already_Exists,
    [XmlEnum(Name = "Failed_A_Client_Initiated_Connection_Already_Exists")] Failed_A_Client_Initiated_Connection_Already_Exists,
    [XmlEnum(Name = "Failed_Reason_Other_Than_A_Connection_Already_Exists")] Failed_Reason_Other_Than_A_Connection_Already_Exists,
    [XmlEnum(Name = "Another_Connection_Attempted")] Another_Connection_Attempted,
  }
}
