

using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_C1G2LockPrivilege
  {
    [XmlEnum(Name = "Read_Write")] Read_Write,
    [XmlEnum(Name = "Perma_Lock")] Perma_Lock,
    [XmlEnum(Name = "Perma_Unlock")] Perma_Unlock,
    [XmlEnum(Name = "Unlock")] Unlock,
  }
}
