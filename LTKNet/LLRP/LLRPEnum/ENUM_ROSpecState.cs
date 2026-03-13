
using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_ROSpecState
  {
    [XmlEnum(Name = "Disabled")] Disabled,
    [XmlEnum(Name = "Inactive")] Inactive,
    [XmlEnum(Name = "Active")] Active,
  }
}
