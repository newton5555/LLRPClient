

using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_C1G2ForwardLinkModulation
  {
    [XmlEnum(Name = "PR_ASK")] PR_ASK,
    [XmlEnum(Name = "SSB_ASK")] SSB_ASK,
    [XmlEnum(Name = "DSB_ASK")] DSB_ASK,
  }
}
