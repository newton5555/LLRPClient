

using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_C1G2TruncateAction
  {
    [XmlEnum(Name = "Unspecified")] Unspecified,
    [XmlEnum(Name = "Do_Not_Truncate")] Do_Not_Truncate,
    [XmlEnum(Name = "Truncate")] Truncate,
  }
}
