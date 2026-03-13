
using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_AccessSpecState
  {
    [XmlEnum(Name = "Disabled")] Disabled,
    [XmlEnum(Name = "Active")] Active,
  }
}
