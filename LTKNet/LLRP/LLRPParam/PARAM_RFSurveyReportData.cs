// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_RFSurveyReportData
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_RFSurveyReportData : Parameter
  {
    public PARAM_ROSpecID ROSpecID;
    public PARAM_SpecIndex SpecIndex;
    public PARAM_FrequencyRSSILevelEntry[] FrequencyRSSILevelEntry;
    public readonly CustomParameterArrayList Custom = new CustomParameterArrayList();

    public PARAM_RFSurveyReportData() => this.typeID = (ushort) 242;

    public bool AddCustomParameter(ICustom_Parameter param)
    {
      if (param is IRFSurveyReportData_Custom_Param)
      {
        this.Custom.Add(param);
        return true;
      }
      if (!(param.GetType() == typeof (PARAM_Custom)))
        return false;
      this.Custom.Add(param);
      return true;
    }

    public static PARAM_RFSurveyReportData FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_RFSurveyReportData) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList1 = new ArrayList();
      PARAM_RFSurveyReportData surveyReportData = new PARAM_RFSurveyReportData();
      surveyReportData.tvCoding = bit_array[cursor];
      int val;
      if (surveyReportData.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        surveyReportData.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) surveyReportData.length * 8;
      }
      if (val != (int) surveyReportData.TypeID)
      {
        cursor = num1;
        return (PARAM_RFSurveyReportData) null;
      }
      surveyReportData.ROSpecID = PARAM_ROSpecID.FromBitArray(ref bit_array, ref cursor, length);
      surveyReportData.SpecIndex = PARAM_SpecIndex.FromBitArray(ref bit_array, ref cursor, length);
      ArrayList arrayList2 = new ArrayList();
      PARAM_FrequencyRSSILevelEntry frequencyRssiLevelEntry;
      while ((frequencyRssiLevelEntry = PARAM_FrequencyRSSILevelEntry.FromBitArray(ref bit_array, ref cursor, length)) != null)
        arrayList2.Add((object) frequencyRssiLevelEntry);
      if (arrayList2.Count > 0)
      {
        surveyReportData.FrequencyRSSILevelEntry = new PARAM_FrequencyRSSILevelEntry[arrayList2.Count];
        for (int index = 0; index < arrayList2.Count; ++index)
          surveyReportData.FrequencyRSSILevelEntry[index] = (PARAM_FrequencyRSSILevelEntry) arrayList2[index];
      }
      while (cursor < num2)
      {
        int num3 = cursor;
        ICustom_Parameter customParameter = CustomParamDecodeFactory.DecodeCustomParameter(ref bit_array, ref cursor, length);
        if (customParameter != null && cursor <= num2 && !surveyReportData.AddCustomParameter(customParameter))
        {
          cursor = num3;
          break;
        }
      }
      return surveyReportData;
    }

    public override string ToString()
    {
      string str = "<RFSurveyReportData>" + "\r\n";
      if (this.ROSpecID != null)
        str += Util.Indent(this.ROSpecID.ToString());
      if (this.SpecIndex != null)
        str += Util.Indent(this.SpecIndex.ToString());
      if (this.FrequencyRSSILevelEntry != null)
      {
        int length = this.FrequencyRSSILevelEntry.Length;
        for (int index = 0; index < length; ++index)
          str += Util.Indent(this.FrequencyRSSILevelEntry[index].ToString());
      }
      if (this.Custom != null)
      {
        int length = this.Custom.Length;
        for (int index = 0; index < length; ++index)
          str += Util.Indent(this.Custom[index].ToString());
      }
      return str + "</RFSurveyReportData>" + "\r\n";
    }

    public static PARAM_RFSurveyReportData FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      nsmgr.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      nsmgr.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      PARAM_RFSurveyReportData surveyReportData = new PARAM_RFSurveyReportData();
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "ROSpecID", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            surveyReportData.ROSpecID = PARAM_ROSpecID.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "SpecIndex", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            surveyReportData.SpecIndex = PARAM_SpecIndex.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "FrequencyRSSILevelEntry", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
          {
            surveyReportData.FrequencyRSSILevelEntry = new PARAM_FrequencyRSSILevelEntry[xmlNodes.Count];
            for (int i = 0; i < xmlNodes.Count; ++i)
              surveyReportData.FrequencyRSSILevelEntry[i] = PARAM_FrequencyRSSILevelEntry.FromXmlNode(xmlNodes[i]);
          }
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
              if (customParameter != null && surveyReportData.AddCustomParameter(customParameter))
                arrayList.Add(nodeCustomChildren[index]);
            }
          }
        }
      }
      catch
      {
      }
      return surveyReportData;
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
      if (this.ROSpecID != null)
        this.ROSpecID.ToBitArray(ref bit_array, ref cursor);
      if (this.SpecIndex != null)
        this.SpecIndex.ToBitArray(ref bit_array, ref cursor);
      if (this.FrequencyRSSILevelEntry != null)
      {
        int length = this.FrequencyRSSILevelEntry.Length;
        for (int index = 0; index < length; ++index)
          this.FrequencyRSSILevelEntry[index].ToBitArray(ref bit_array, ref cursor);
      }
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
      if (this.ROSpecID != null)
        this.ROSpecID.AppendToBitArray(bArr);
      if (this.SpecIndex != null)
        this.SpecIndex.AppendToBitArray(bArr);
      if (this.FrequencyRSSILevelEntry != null)
      {
        int length3 = this.FrequencyRSSILevelEntry.Length;
        for (int index = 0; index < length3; ++index)
          this.FrequencyRSSILevelEntry[index].AppendToBitArray(bArr);
      }
      if (this.Custom != null)
      {
        int length4 = this.Custom.Length;
        for (int index = 0; index < length4; ++index)
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
