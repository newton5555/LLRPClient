
using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_IdentificationType
  {
    [XmlEnum(Name = "MAC_Address")] MAC_Address,
    [XmlEnum(Name = "EPC")] EPC,
  }
}
