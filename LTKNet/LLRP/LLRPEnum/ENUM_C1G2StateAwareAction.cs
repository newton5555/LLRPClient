

using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_C1G2StateAwareAction
  {
    [XmlEnum(Name = "AssertSLOrA_DeassertSLOrB")] AssertSLOrA_DeassertSLOrB,
    [XmlEnum(Name = "AssertSLOrA_Noop")] AssertSLOrA_Noop,
    [XmlEnum(Name = "Noop_DeassertSLOrB")] Noop_DeassertSLOrB,
    [XmlEnum(Name = "NegateSLOrABBA_Noop")] NegateSLOrABBA_Noop,
    [XmlEnum(Name = "DeassertSLOrB_AssertSLOrA")] DeassertSLOrB_AssertSLOrA,
    [XmlEnum(Name = "DeassertSLOrB_Noop")] DeassertSLOrB_Noop,
    [XmlEnum(Name = "Noop_AssertSLOrA")] Noop_AssertSLOrA,
    [XmlEnum(Name = "Noop_NegateSLOrABBA")] Noop_NegateSLOrABBA,
  }
}
