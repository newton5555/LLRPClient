

using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_C1G2DRValue
  {
    [XmlEnum(Name = "DRV_8")] DRV_8,
    [XmlEnum(Name = "DRV_64_3")] DRV_64_3,
  }
}
