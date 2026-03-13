// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_RFSurveySpec
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_RFSurveySpec : Parameter
  {
    public ushort AntennaID;
    private short AntennaID_len;
    public uint StartFrequency;
    private short StartFrequency_len;
    public uint EndFrequency;
    private short EndFrequency_len;
    public PARAM_RFSurveySpecStopTrigger RFSurveySpecStopTrigger;
    public readonly CustomParameterArrayList Custom = new CustomParameterArrayList();

    public PARAM_RFSurveySpec() => this.typeID = (ushort) 187;

    public bool AddCustomParameter(ICustom_Parameter param)
    {
      if (param is IRFSurveySpec_Custom_Param)
      {
        this.Custom.Add(param);
        return true;
      }
      if (!(param.GetType() == typeof (PARAM_Custom)))
        return false;
      this.Custom.Add(param);
      return true;
    }

    public static PARAM_RFSurveySpec FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_RFSurveySpec) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_RFSurveySpec paramRfSurveySpec = new PARAM_RFSurveySpec();
      paramRfSurveySpec.tvCoding = bit_array[cursor];
      int val;
      if (paramRfSurveySpec.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        paramRfSurveySpec.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) paramRfSurveySpec.length * 8;
      }
      if (val != (int) paramRfSurveySpec.TypeID)
      {
        cursor = num1;
        return (PARAM_RFSurveySpec) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 16;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len1);
      paramRfSurveySpec.AntennaID = (ushort) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 32;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len2);
      paramRfSurveySpec.StartFrequency = (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len3 = 32;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len3);
      paramRfSurveySpec.EndFrequency = (uint) obj;
      paramRfSurveySpec.RFSurveySpecStopTrigger = PARAM_RFSurveySpecStopTrigger.FromBitArray(ref bit_array, ref cursor, length);
      while (cursor < num2)
      {
        int num3 = cursor;
        ICustom_Parameter customParameter = CustomParamDecodeFactory.DecodeCustomParameter(ref bit_array, ref cursor, length);
        if (customParameter != null && cursor <= num2 && !paramRfSurveySpec.AddCustomParameter(customParameter))
        {
          cursor = num3;
          break;
        }
      }
      return paramRfSurveySpec;
    }

    public override string ToString()
    {
      string str = "<RFSurveySpec>" + "\r\n";
      int antennaId = (int) this.AntennaID;
      try
      {
        str = str + "  <AntennaID>" + Util.ConvertValueTypeToString((object) this.AntennaID, "u16", "") + "</AntennaID>";
        str += "\r\n";
      }
      catch
      {
      }
      int startFrequency = (int) this.StartFrequency;
      try
      {
        str = str + "  <StartFrequency>" + Util.ConvertValueTypeToString((object) this.StartFrequency, "u32", "") + "</StartFrequency>";
        str += "\r\n";
      }
      catch
      {
      }
      int endFrequency = (int) this.EndFrequency;
      try
      {
        str = str + "  <EndFrequency>" + Util.ConvertValueTypeToString((object) this.EndFrequency, "u32", "") + "</EndFrequency>";
        str += "\r\n";
      }
      catch
      {
      }
      if (this.RFSurveySpecStopTrigger != null)
        str += Util.Indent(this.RFSurveySpecStopTrigger.ToString());
      if (this.Custom != null)
      {
        int length = this.Custom.Length;
        for (int index = 0; index < length; ++index)
          str += Util.Indent(this.Custom[index].ToString());
      }
      return str + "</RFSurveySpec>" + "\r\n";
    }

    public static PARAM_RFSurveySpec FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      nsmgr.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      nsmgr.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      PARAM_RFSurveySpec paramRfSurveySpec = new PARAM_RFSurveySpec();
      string nodeValue1 = XmlUtil.GetNodeValue(node, "AntennaID");
      paramRfSurveySpec.AntennaID = (ushort) Util.ParseValueTypeFromString(nodeValue1, "u16", "");
      string nodeValue2 = XmlUtil.GetNodeValue(node, "StartFrequency");
      paramRfSurveySpec.StartFrequency = (uint) Util.ParseValueTypeFromString(nodeValue2, "u32", "");
      string nodeValue3 = XmlUtil.GetNodeValue(node, "EndFrequency");
      paramRfSurveySpec.EndFrequency = (uint) Util.ParseValueTypeFromString(nodeValue3, "u32", "");
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "RFSurveySpecStopTrigger", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            paramRfSurveySpec.RFSurveySpecStopTrigger = PARAM_RFSurveySpecStopTrigger.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        ArrayList nodeCustomChildren = XmlUtil.GetXmlNodeCustomChildren(node, nsmgr);
        if (nodeCustomChildren != null)
        {
          for (int index = 0; index < nodeCustomChildren.Count; ++index)
          {
            if (!arrayList.Contains(nodeCustomChildren[index]))
            {
              ICustom_Parameter customParameter = CustomParamDecodeFactory.DecodeXmlNodeToCustomParameter((XmlNode) nodeCustomChildren[index]);
              if (customParameter != null && paramRfSurveySpec.AddCustomParameter(customParameter))
                arrayList.Add(nodeCustomChildren[index]);
            }
          }
        }
      }
      catch
      {
      }
      return paramRfSurveySpec;
    }

    public override void ToBitArray(ref bool[] bit_array, ref int cursor)
    {
      int num = cursor;
      if (this.tvCoding)
      {
        bit_array[cursor] = true;
        ++cursor;
        Util.ConvertIntToBitArray((uint) this.typeID, 7).CopyTo((Array) bit_array, cursor);
        cursor += 7;
      }
      else
      {
        cursor += 6;
        Util.ConvertIntToBitArray((uint) this.typeID, 10).CopyTo((Array) bit_array, cursor);
        cursor += 10;
        cursor += 16;
      }
      int antennaId = (int) this.AntennaID;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.AntennaID, (int) this.AntennaID_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int startFrequency = (int) this.StartFrequency;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.StartFrequency, (int) this.StartFrequency_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int endFrequency = (int) this.EndFrequency;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.EndFrequency, (int) this.EndFrequency_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      if (this.RFSurveySpecStopTrigger != null)
        this.RFSurveySpecStopTrigger.ToBitArray(ref bit_array, ref cursor);
      if (this.Custom != null)
      {
        int length = this.Custom.Length;
        for (int index = 0; index < length; ++index)
          this.Custom[index].ToBitArray(ref bit_array, ref cursor);
      }
      if (this.tvCoding)
        return;
      Util.ConvertIntToBitArray((uint) (cursor - num) / 8U, 16).CopyTo((Array) bit_array, num + 16);
    }

    public override void AppendToBitArray(AutoGrowingBitArray bArr)
    {
      int length1 = bArr.Length;
      if (this.tvCoding)
      {
        int length2 = bArr.Length;
        ++bArr.Length;
        bArr[length2] = true;
        Util.AppendIntToBitArray((uint) this.typeID, 7, bArr);
      }
      else
      {
        bArr.Length += 6;
        Util.AppendIntToBitArray((uint) this.typeID, 10, bArr);
        bArr.Length += 16;
      }
      int antennaId = (int) this.AntennaID;
      try
      {
        Util.AppendObjToBitArray((object) this.AntennaID, (int) this.AntennaID_len, bArr);
      }
      catch
      {
      }
      int startFrequency = (int) this.StartFrequency;
      try
      {
        Util.AppendObjToBitArray((object) this.StartFrequency, (int) this.StartFrequency_len, bArr);
      }
      catch
      {
      }
      int endFrequency = (int) this.EndFrequency;
      try
      {
        Util.AppendObjToBitArray((object) this.EndFrequency, (int) this.EndFrequency_len, bArr);
      }
      catch
      {
      }
      if (this.RFSurveySpecStopTrigger != null)
        this.RFSurveySpecStopTrigger.AppendToBitArray(bArr);
      if (this.Custom != null)
      {
        int length3 = this.Custom.Length;
        for (int index = 0; index < length3; ++index)
          this.Custom[index].AppendToBitArray(bArr);
      }
      if (this.tvCoding)
        return;
      BitArray bitArray = Util.ConvertIntToBitArray((uint) (bArr.Length - length1) / 8U, 16);
      for (int index = 0; index < bitArray.Length; ++index)
        bArr[length1 + 16 + index] = bitArray[index];
    }
  }
}
