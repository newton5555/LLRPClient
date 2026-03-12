

using System;
using System.Xml.Serialization;


namespace Org.LLRP.LTK.LLRPV1
{
  [Serializable]
  public enum ENUM_C1G2BlockWriteResultType
  {
    [XmlEnum(Name = "Success")] Success,
    [XmlEnum(Name = "Tag_Memory_Overrun_Error")] Tag_Memory_Overrun_Error,
    [XmlEnum(Name = "Tag_Memory_Locked_Error")] Tag_Memory_Locked_Error,
    [XmlEnum(Name = "Insufficient_Power")] Insufficient_Power,
    [XmlEnum(Name = "Nonspecific_Tag_Error")] Nonspecific_Tag_Error,
    [XmlEnum(Name = "No_Response_From_Tag")] No_Response_From_Tag,
    [XmlEnum(Name = "Nonspecific_Reader_Error")] Nonspecific_Reader_Error,
    [XmlEnum(Name = "Incorrect_Password_Error")] Incorrect_Password_Error,
  }
}
