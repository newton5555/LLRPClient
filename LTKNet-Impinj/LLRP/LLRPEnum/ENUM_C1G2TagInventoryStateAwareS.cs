

using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_C1G2TagInventoryStateAwareS
  {
    [XmlEnum(Name = "SL")] SL,
    [XmlEnum(Name = "Not_SL")] Not_SL,
  }
}
