// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.MSG_GET_READER_CAPABILITIES
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;

#nullable disable
namespace Org.LLRP.LTK.LLRPV1
{
  public class MSG_GET_READER_CAPABILITIES : Message
  {
    public ENUM_GetReaderCapabilitiesRequestedData RequestedData;
    private short RequestedData_len = 8;
    public readonly CustomParameterArrayList Custom = new CustomParameterArrayList();

    public bool AddCustomParameter(ICustom_Parameter param)
    {
      if (param is IGET_READER_CAPABILITIES_Custom_Param)
      {
        this.Custom.Add(param);
        return true;
      }
      if (!(param.GetType() == typeof (PARAM_Custom)))
        return false;
      this.Custom.Add(param);
      return true;
    }

    public MSG_GET_READER_CAPABILITIES()
    {
      this.msgType = (ushort) 1;
      this.MSG_ID = MessageID.getNewMessageID();
    }

    public override string ToString()
    {
      string str = "<GET_READER_CAPABILITIES" + string.Format(" xmlns=\"{0}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0") + string.Format(" xmlns:llrp=\"{0}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0") + " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n" + string.Format(" xsi:schemaLocation=\"{0} {1}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0/llrp.xsd") + " Version=\"" + this.version.ToString() + "\" MessageID=\"" + this.MSG_ID.ToString() + "\">\r\n";
      int requestedData = (int) this.RequestedData;
      try
      {
        str = str + "  <RequestedData>" + this.RequestedData.ToString() + "</RequestedData>";
        str += "\r\n";
      }
      catch
      {
      }
      if (this.Custom != null)
      {
        int length = this.Custom.Length;
        for (int index = 0; index < length; ++index)
          str += Util.Indent(this.Custom[index].ToString());
      }
      return str + "</GET_READER_CAPABILITIES>";
    }

    public static MSG_GET_READER_CAPABILITIES FromString(string str)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.LoadXml(str);
      XmlNode documentElement = (XmlNode) xmlDocument.DocumentElement;
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(documentElement.OwnerDocument.NameTable);
      nsmgr.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      nsmgr.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      MSG_GET_READER_CAPABILITIES readerCapabilities = new MSG_GET_READER_CAPABILITIES();
      try
      {
        readerCapabilities.MSG_ID = Convert.ToUInt32(XmlUtil.GetNodeAttrValue(documentElement, "MessageID"));
      }
      catch
      {
      }
      string nodeValue = XmlUtil.GetNodeValue(documentElement, "RequestedData");
      readerCapabilities.RequestedData = (ENUM_GetReaderCapabilitiesRequestedData) Enum.Parse(typeof (ENUM_GetReaderCapabilitiesRequestedData), nodeValue);
      try
      {
        ArrayList nodeCustomChildren = XmlUtil.GetXmlNodeCustomChildren(documentElement, nsmgr);
        if (nodeCustomChildren != null)
        {
          for (int index = 0; index < nodeCustomChildren.Count; ++index)
          {
            ICustom_Parameter customParameter = CustomParamDecodeFactory.DecodeXmlNodeToCustomParameter((XmlNode) nodeCustomChildren[index]);
            if (customParameter != null)
              readerCapabilities.AddCustomParameter(customParameter);
          }
        }
      }
      catch
      {
      }
      return readerCapabilities;
    }

    public override bool[] ToBitArray()
    {
      AutoGrowingBitArray autoGrowingBitArray = new AutoGrowingBitArray(10000);
      Util.AppendIntToBitArray(0U, 3, autoGrowingBitArray);
      Util.AppendIntToBitArray((uint) this.version, 3, autoGrowingBitArray);
      Util.AppendIntToBitArray((uint) this.msgType, 10, autoGrowingBitArray);
      Util.AppendIntToBitArray(this.msgLen, 32, autoGrowingBitArray);
      Util.AppendIntToBitArray(this.msgID, 32, autoGrowingBitArray);
      int requestedData = (int) this.RequestedData;
      try
      {
        Util.AppendObjToBitArray((object) this.RequestedData, (int) this.RequestedData_len, autoGrowingBitArray);
      }
      catch
      {
      }
      if (this.Custom != null)
      {
        int length = this.Custom.Length;
        for (int index = 0; index < length; ++index)
          this.Custom[index].AppendToBitArray(autoGrowingBitArray);
      }
      int val = autoGrowingBitArray.Length / 8;
      bool[] bitArray = new bool[autoGrowingBitArray.Length];
      autoGrowingBitArray.CopyTo((Array) bitArray, 0);
      Util.ConvertIntToBitArray((uint) val, 32).CopyTo((Array) bitArray, 16);
      return bitArray;
    }

    public static MSG_GET_READER_CAPABILITIES FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor > length)
        return (MSG_GET_READER_CAPABILITIES) null;
      ArrayList arrayList = new ArrayList();
      MSG_GET_READER_CAPABILITIES readerCapabilities = new MSG_GET_READER_CAPABILITIES();
      cursor += 6;
      if ((int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10) != (int) readerCapabilities.msgType)
      {
        cursor -= 16;
        return (MSG_GET_READER_CAPABILITIES) null;
      }
      readerCapabilities.msgLen = (uint) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 32);
      readerCapabilities.msgID = (uint) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 32);
      if (cursor > length)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len = 8;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len);
      readerCapabilities.RequestedData = (ENUM_GetReaderCapabilitiesRequestedData) (uint) obj;
      int num;
      bool flag;
      do
      {
        num = cursor;
        flag = false;
        ICustom_Parameter customParameter = CustomParamDecodeFactory.DecodeCustomParameter(ref bit_array, ref cursor, length);
        if (customParameter != null && readerCapabilities.AddCustomParameter(customParameter))
          flag = true;
      }
      while (flag);
      cursor = num;
      return readerCapabilities;
    }
  }
}
