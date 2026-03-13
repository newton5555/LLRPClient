

using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_C1G2KillResultType
  {
    [XmlEnum(Name = "Success")] Success,
    [XmlEnum(Name = "Zero_Kill_Password_Error")] Zero_Kill_Password_Error,
    [XmlEnum(Name = "Insufficient_Power")] Insufficient_Power,
    [XmlEnum(Name = "Nonspecific_Tag_Error")] Nonspecific_Tag_Error,
    [XmlEnum(Name = "No_Response_From_Tag")] No_Response_From_Tag,
    [XmlEnum(Name = "Nonspecific_Reader_Error")] Nonspecific_Reader_Error,
    [XmlEnum(Name = "Incorrect_Password_Error")] Incorrect_Password_Error,
  }
}
