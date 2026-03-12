

using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_C1G2ReadResultType
  {
    [XmlEnum(Name = "Success")] Success,
    [XmlEnum(Name = "Nonspecific_Tag_Error")] Nonspecific_Tag_Error,
    [XmlEnum(Name = "No_Response_From_Tag")] No_Response_From_Tag,
    [XmlEnum(Name = "Nonspecific_Reader_Error")] Nonspecific_Reader_Error,
    [XmlEnum(Name = "Memory_Overrun_Error")] Memory_Overrun_Error,
    [XmlEnum(Name = "Memory_Locked_Error")] Memory_Locked_Error,
    [XmlEnum(Name = "Incorrect_Password_Error")] Incorrect_Password_Error,
  }
}
