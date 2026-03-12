

using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_C1G2LockDataField
  {
    [XmlEnum(Name = "Kill_Password")] Kill_Password,
    [XmlEnum(Name = "Access_Password")] Access_Password,
    [XmlEnum(Name = "EPC_Memory")] EPC_Memory,
    [XmlEnum(Name = "TID_Memory")] TID_Memory,
    [XmlEnum(Name = "User_Memory")] User_Memory,
  }
}
