

using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_C1G2TagInventoryStateAwareI
  {
    [XmlEnum(Name = "State_A")] State_A,
    [XmlEnum(Name = "State_B")] State_B,
  }
}
