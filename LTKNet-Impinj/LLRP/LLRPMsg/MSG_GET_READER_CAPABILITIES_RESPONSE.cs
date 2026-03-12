// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.MSG_GET_READER_CAPABILITIES_RESPONSE
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
  public class MSG_GET_READER_CAPABILITIES_RESPONSE : Message
  {
    public PARAM_LLRPStatus LLRPStatus;
    public PARAM_GeneralDeviceCapabilities GeneralDeviceCapabilities;
    public PARAM_LLRPCapabilities LLRPCapabilities;
    public PARAM_RegulatoryCapabilities RegulatoryCapabilities;
    public UNION_AirProtocolLLRPCapabilities AirProtocolLLRPCapabilities = new UNION_AirProtocolLLRPCapabilities();
    public readonly CustomParameterArrayList Custom = new CustomParameterArrayList();

    public bool AddCustomParameter(ICustom_Parameter param)
    {
      if (param is IGET_READER_CAPABILITIES_RESPONSE_Custom_Param)
      {
        this.Custom.Add(param);
        return true;
      }
      if (!(param.GetType() == typeof (PARAM_Custom)))
        return false;
      this.Custom.Add(param);
      return true;
    }

    public MSG_GET_READER_CAPABILITIES_RESPONSE()
    {
      this.msgType = (ushort) 11;
      this.MSG_ID = MessageID.getNewMessageID();
    }

    public override string ToString()
    {
      string str = "<GET_READER_CAPABILITIES_RESPONSE" + string.Format(" xmlns=\"{0}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0") + string.Format(" xmlns:llrp=\"{0}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0") + " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n" + string.Format(" xsi:schemaLocation=\"{0} {1}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0/llrp.xsd") + " Version=\"" + this.version.ToString() + "\" MessageID=\"" + this.MSG_ID.ToString() + "\">\r\n";
      if (this.LLRPStatus != null)
        str += Util.Indent(this.LLRPStatus.ToString());
      if (this.GeneralDeviceCapabilities != null)
        str += Util.Indent(this.GeneralDeviceCapabilities.ToString());
      if (this.LLRPCapabilities != null)
        str += Util.Indent(this.LLRPCapabilities.ToString());
      if (this.RegulatoryCapabilities != null)
        str += Util.Indent(this.RegulatoryCapabilities.ToString());
      if (this.AirProtocolLLRPCapabilities != null)
      {
        int count = this.AirProtocolLLRPCapabilities.Count;
        for (int index = 0; index < count; ++index)
          str += Util.Indent(this.AirProtocolLLRPCapabilities[index].ToString());
      }
      if (this.Custom != null)
      {
        int length = this.Custom.Length;
        for (int index = 0; index < length; ++index)
          str += Util.Indent(this.Custom[index].ToString());
      }
      return str + "</GET_READER_CAPABILITIES_RESPONSE>";
    }

    public static MSG_GET_READER_CAPABILITIES_RESPONSE FromString(string str)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.LoadXml(str);
      XmlNode documentElement = (XmlNode) xmlDocument.DocumentElement;
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(documentElement.OwnerDocument.NameTable);
      nsmgr.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      nsmgr.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      MSG_GET_READER_CAPABILITIES_RESPONSE capabilitiesResponse = new MSG_GET_READER_CAPABILITIES_RESPONSE();
      try
      {
        capabilitiesResponse.MSG_ID = Convert.ToUInt32(XmlUtil.GetNodeAttrValue(documentElement, "MessageID"));
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(documentElement, "LLRPStatus", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            capabilitiesResponse.LLRPStatus = PARAM_LLRPStatus.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(documentElement, "GeneralDeviceCapabilities", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            capabilitiesResponse.GeneralDeviceCapabilities = PARAM_GeneralDeviceCapabilities.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(documentElement, "LLRPCapabilities", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            capabilitiesResponse.LLRPCapabilities = PARAM_LLRPCapabilities.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(documentElement, "RegulatoryCapabilities", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            capabilitiesResponse.RegulatoryCapabilities = PARAM_RegulatoryCapabilities.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      capabilitiesResponse.AirProtocolLLRPCapabilities = new UNION_AirProtocolLLRPCapabilities();
      try
      {
        foreach (XmlNode childNode in documentElement.ChildNodes)
        {
          if (childNode.Name == "C1G2LLRPCapabilities")
            capabilitiesResponse.AirProtocolLLRPCapabilities.Add((IParameter) PARAM_C1G2LLRPCapabilities.FromXmlNode(childNode));
        }
      }
      catch
      {
      }
      try
      {
        ArrayList nodeCustomChildren = XmlUtil.GetXmlNodeCustomChildren(documentElement, nsmgr);
        if (nodeCustomChildren != null)
        {
          for (int index = 0; index < nodeCustomChildren.Count; ++index)
          {
            ICustom_Parameter customParameter = CustomParamDecodeFactory.DecodeXmlNodeToCustomParameter((XmlNode) nodeCustomChildren[index]);
            if (customParameter != null)
              capabilitiesResponse.AddCustomParameter(customParameter);
          }
        }
      }
      catch
      {
      }
      return capabilitiesResponse;
    }

    public override bool[] ToBitArray()
    {
      AutoGrowingBitArray autoGrowingBitArray = new AutoGrowingBitArray(10000);
      Util.AppendIntToBitArray(0U, 3, autoGrowingBitArray);
      Util.AppendIntToBitArray((uint) this.version, 3, autoGrowingBitArray);
      Util.AppendIntToBitArray((uint) this.msgType, 10, autoGrowingBitArray);
      Util.AppendIntToBitArray(this.msgLen, 32, autoGrowingBitArray);
      Util.AppendIntToBitArray(this.msgID, 32, autoGrowingBitArray);
      if (this.LLRPStatus != null)
        this.LLRPStatus.AppendToBitArray(autoGrowingBitArray);
      if (this.GeneralDeviceCapabilities != null)
        this.GeneralDeviceCapabilities.AppendToBitArray(autoGrowingBitArray);
      if (this.LLRPCapabilities != null)
        this.LLRPCapabilities.AppendToBitArray(autoGrowingBitArray);
      if (this.RegulatoryCapabilities != null)
        this.RegulatoryCapabilities.AppendToBitArray(autoGrowingBitArray);
      int count = this.AirProtocolLLRPCapabilities.Count;
      for (int index = 0; index < count; ++index)
        this.AirProtocolLLRPCapabilities[index].AppendToBitArray(autoGrowingBitArray);
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

    public static MSG_GET_READER_CAPABILITIES_RESPONSE FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor > length)
        return (MSG_GET_READER_CAPABILITIES_RESPONSE) null;
      ArrayList arrayList = new ArrayList();
      MSG_GET_READER_CAPABILITIES_RESPONSE capabilitiesResponse = new MSG_GET_READER_CAPABILITIES_RESPONSE();
      cursor += 6;
      if ((int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10) != (int) capabilitiesResponse.msgType)
      {
        cursor -= 16;
        return (MSG_GET_READER_CAPABILITIES_RESPONSE) null;
      }
      capabilitiesResponse.msgLen = (uint) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 32);
      capabilitiesResponse.msgID = (uint) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 32);
      capabilitiesResponse.LLRPStatus = PARAM_LLRPStatus.FromBitArray(ref bit_array, ref cursor, length);
      capabilitiesResponse.GeneralDeviceCapabilities = PARAM_GeneralDeviceCapabilities.FromBitArray(ref bit_array, ref cursor, length);
      capabilitiesResponse.LLRPCapabilities = PARAM_LLRPCapabilities.FromBitArray(ref bit_array, ref cursor, length);
      capabilitiesResponse.RegulatoryCapabilities = PARAM_RegulatoryCapabilities.FromBitArray(ref bit_array, ref cursor, length);
      ushort num1 = 1;
      while (num1 != (ushort) 0)
      {
        num1 = (ushort) 0;
        PARAM_C1G2LLRPCapabilities val1 = PARAM_C1G2LLRPCapabilities.FromBitArray(ref bit_array, ref cursor, length);
        if (val1 != null)
        {
          ++num1;
          capabilitiesResponse.AirProtocolLLRPCapabilities.Add((IParameter) val1);
        }
        PARAM_C1G2LLRPCapabilities val2;
        while ((val2 = PARAM_C1G2LLRPCapabilities.FromBitArray(ref bit_array, ref cursor, length)) != null)
          capabilitiesResponse.AirProtocolLLRPCapabilities.Add((IParameter) val2);
      }
      int num2;
      bool flag;
      do
      {
        num2 = cursor;
        flag = false;
        ICustom_Parameter customParameter = CustomParamDecodeFactory.DecodeCustomParameter(ref bit_array, ref cursor, length);
        if (customParameter != null && capabilitiesResponse.AddCustomParameter(customParameter))
          flag = true;
      }
      while (flag);
      cursor = num2;
      return capabilitiesResponse;
    }
  }
}
