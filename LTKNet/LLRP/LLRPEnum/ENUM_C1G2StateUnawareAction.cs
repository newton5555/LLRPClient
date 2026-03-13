

using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_C1G2StateUnawareAction
  {
    [XmlEnum(Name = "Select_Unselect")] Select_Unselect,
    [XmlEnum(Name = "Select_DoNothing")] Select_DoNothing,
    [XmlEnum(Name = "DoNothing_Unselect")] DoNothing_Unselect,
    [XmlEnum(Name = "Unselect_DoNothing")] Unselect_DoNothing,
    [XmlEnum(Name = "Unselect_Select")] Unselect_Select,
    [XmlEnum(Name = "DoNothing_Select")] DoNothing_Select,
  }
}
