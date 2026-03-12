// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.MSG_GET_READER_CONFIG
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
  public class MSG_GET_READER_CONFIG : Message
  {
    public ushort AntennaID;
    private short AntennaID_len;
    public ENUM_GetReaderConfigRequestedData RequestedData;
    private short RequestedData_len = 8;
    public ushort GPIPortNum;
    private short GPIPortNum_len;
    public ushort GPOPortNum;
    private short GPOPortNum_len;
    public readonly CustomParameterArrayList Custom = new CustomParameterArrayList();

    public bool AddCustomParameter(ICustom_Parameter param)
    {
      if (param is IGET_READER_CONFIG_Custom_Param)
      {
        this.Custom.Add(param);
        return true;
      }
      if (!(param.GetType() == typeof (PARAM_Custom)))
        return false;
      this.Custom.Add(param);
      return true;
    }

    public MSG_GET_READER_CONFIG()
    {
      this.msgType = (ushort) 2;
      this.MSG_ID = MessageID.getNewMessageID();
    }

    public override string ToString()
    {
      string str = "<GET_READER_CONFIG" + string.Format(" xmlns=\"{0}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0") + string.Format(" xmlns:llrp=\"{0}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0") + " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n" + string.Format(" xsi:schemaLocation=\"{0} {1}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0/llrp.xsd") + " Version=\"" + this.version.ToString() + "\" MessageID=\"" + this.MSG_ID.ToString() + "\">\r\n";
      int antennaId = (int) this.AntennaID;
      try
      {
        str = str + "  <AntennaID>" + Util.ConvertValueTypeToString((object) this.AntennaID, "u16", "") + "</AntennaID>";
        str += "\r\n";
      }
      catch
      {
      }
      int requestedData = (int) this.RequestedData;
      try
      {
        str = str + "  <RequestedData>" + this.RequestedData.ToString() + "</RequestedData>";
        str += "\r\n";
      }
      catch
      {
      }
      int gpiPortNum = (int) this.GPIPortNum;
      try
      {
        str = str + "  <GPIPortNum>" + Util.ConvertValueTypeToString((object) this.GPIPortNum, "u16", "") + "</GPIPortNum>";
        str += "\r\n";
      }
      catch
      {
      }
      int gpoPortNum = (int) this.GPOPortNum;
      try
      {
        str = str + "  <GPOPortNum>" + Util.ConvertValueTypeToString((object) this.GPOPortNum, "u16", "") + "</GPOPortNum>";
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
      return str + "</GET_READER_CONFIG>";
    }

    public static MSG_GET_READER_CONFIG FromString(string str)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.LoadXml(str);
      XmlNode documentElement = (XmlNode) xmlDocument.DocumentElement;
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(documentElement.OwnerDocument.NameTable);
      nsmgr.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      nsmgr.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      MSG_GET_READER_CONFIG msgGetReaderConfig = new MSG_GET_READER_CONFIG();
      try
      {
        msgGetReaderConfig.MSG_ID = Convert.ToUInt32(XmlUtil.GetNodeAttrValue(documentElement, "MessageID"));
      }
      catch
      {
      }
      string nodeValue1 = XmlUtil.GetNodeValue(documentElement, "AntennaID");
      msgGetReaderConfig.AntennaID = (ushort) Util.ParseValueTypeFromString(nodeValue1, "u16", "");
      string nodeValue2 = XmlUtil.GetNodeValue(documentElement, "RequestedData");
      msgGetReaderConfig.RequestedData = (ENUM_GetReaderConfigRequestedData) Enum.Parse(typeof (ENUM_GetReaderConfigRequestedData), nodeValue2);
      string nodeValue3 = XmlUtil.GetNodeValue(documentElement, "GPIPortNum");
      msgGetReaderConfig.GPIPortNum = (ushort) Util.ParseValueTypeFromString(nodeValue3, "u16", "");
      string nodeValue4 = XmlUtil.GetNodeValue(documentElement, "GPOPortNum");
      msgGetReaderConfig.GPOPortNum = (ushort) Util.ParseValueTypeFromString(nodeValue4, "u16", "");
      try
      {
        ArrayList nodeCustomChildren = XmlUtil.GetXmlNodeCustomChildren(documentElement, nsmgr);
        if (nodeCustomChildren != null)
        {
          for (int index = 0; index < nodeCustomChildren.Count; ++index)
          {
            ICustom_Parameter customParameter = CustomParamDecodeFactory.DecodeXmlNodeToCustomParameter((XmlNode) nodeCustomChildren[index]);
            if (customParameter != null)
              msgGetReaderConfig.AddCustomParameter(customParameter);
          }
        }
      }
      catch
      {
      }
      return msgGetReaderConfig;
    }

    public override bool[] ToBitArray()
    {
      AutoGrowingBitArray autoGrowingBitArray = new AutoGrowingBitArray(10000);
      Util.AppendIntToBitArray(0U, 3, autoGrowingBitArray);
      Util.AppendIntToBitArray((uint) this.version, 3, autoGrowingBitArray);
      Util.AppendIntToBitArray((uint) this.msgType, 10, autoGrowingBitArray);
      Util.AppendIntToBitArray(this.msgLen, 32, autoGrowingBitArray);
      Util.AppendIntToBitArray(this.msgID, 32, autoGrowingBitArray);
      int antennaId = (int) this.AntennaID;
      try
      {
        Util.AppendObjToBitArray((object) this.AntennaID, (int) this.AntennaID_len, autoGrowingBitArray);
      }
      catch
      {
      }
      int requestedData = (int) this.RequestedData;
      try
      {
        Util.AppendObjToBitArray((object) this.RequestedData, (int) this.RequestedData_len, autoGrowingBitArray);
      }
      catch
      {
      }
      int gpiPortNum = (int) this.GPIPortNum;
      try
      {
        Util.AppendObjToBitArray((object) this.GPIPortNum, (int) this.GPIPortNum_len, autoGrowingBitArray);
      }
      catch
      {
      }
      int gpoPortNum = (int) this.GPOPortNum;
      try
      {
        Util.AppendObjToBitArray((object) this.GPOPortNum, (int) this.GPOPortNum_len, autoGrowingBitArray);
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

    public static MSG_GET_READER_CONFIG FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor > length)
        return (MSG_GET_READER_CONFIG) null;
      ArrayList arrayList = new ArrayList();
      MSG_GET_READER_CONFIG msgGetReaderConfig = new MSG_GET_READER_CONFIG();
      cursor += 6;
      if ((int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10) != (int) msgGetReaderConfig.msgType)
      {
        cursor -= 16;
        return (MSG_GET_READER_CONFIG) null;
      }
      msgGetReaderConfig.msgLen = (uint) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 32);
      msgGetReaderConfig.msgID = (uint) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 32);
      if (cursor > length)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 16;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len1);
      msgGetReaderConfig.AntennaID = (ushort) obj;
      if (cursor > length)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 8;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len2);
      msgGetReaderConfig.RequestedData = (ENUM_GetReaderConfigRequestedData) (uint) obj;
      if (cursor > length)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len3 = 16;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len3);
      msgGetReaderConfig.GPIPortNum = (ushort) obj;
      if (cursor > length)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len4 = 16;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len4);
      msgGetReaderConfig.GPOPortNum = (ushort) obj;
      int num;
      bool flag;
      do
      {
        num = cursor;
        flag = false;
        ICustom_Parameter customParameter = CustomParamDecodeFactory.DecodeCustomParameter(ref bit_array, ref cursor, length);
        if (customParameter != null && msgGetReaderConfig.AddCustomParameter(customParameter))
          flag = true;
      }
      while (flag);
      cursor = num;
      return msgGetReaderConfig;
    }
  }
}
