

using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_AISpecEventType
  {
    [XmlEnum(Name = "End_Of_AISpec")] End_Of_AISpec,
  }
}
