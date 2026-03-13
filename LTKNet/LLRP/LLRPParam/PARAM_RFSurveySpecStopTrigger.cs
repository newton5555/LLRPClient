// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_RFSurveySpecStopTrigger
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_RFSurveySpecStopTrigger : Parameter
  {
    public ENUM_RFSurveySpecStopTriggerType StopTriggerType;
    private short StopTriggerType_len = 8;
    public uint DurationPeriod;
    private short DurationPeriod_len;
    public uint N;
    private short N_len;

    public PARAM_RFSurveySpecStopTrigger() => this.typeID = (ushort) 188;

    public static PARAM_RFSurveySpecStopTrigger FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_RFSurveySpecStopTrigger) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_RFSurveySpecStopTrigger surveySpecStopTrigger = new PARAM_RFSurveySpecStopTrigger();
      surveySpecStopTrigger.tvCoding = bit_array[cursor];
      int val;
      if (surveySpecStopTrigger.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        surveySpecStopTrigger.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) surveySpecStopTrigger.length * 8;
      }
      if (val != (int) surveySpecStopTrigger.TypeID)
      {
        cursor = num1;
        return (PARAM_RFSurveySpecStopTrigger) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 8;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len1);
      surveySpecStopTrigger.StopTriggerType = (ENUM_RFSurveySpecStopTriggerType) (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 32;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len2);
      surveySpecStopTrigger.DurationPeriod = (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len3 = 32;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len3);
      surveySpecStopTrigger.N = (uint) obj;
      return surveySpecStopTrigger;
    }

    public override string ToString()
    {
      string str = "<RFSurveySpecStopTrigger>" + "\r\n";
      int stopTriggerType = (int) this.StopTriggerType;
      try
      {
        str = str + "  <StopTriggerType>" + this.StopTriggerType.ToString() + "</StopTriggerType>";
        str += "\r\n";
      }
      catch
      {
      }
      int durationPeriod = (int) this.DurationPeriod;
      try
      {
        str = str + "  <DurationPeriod>" + Util.ConvertValueTypeToString((object) this.DurationPeriod, "u32", "") + "</DurationPeriod>";
        str += "\r\n";
      }
      catch
      {
      }
      int n = (int) this.N;
      try
      {
        str = str + "  <N>" + Util.ConvertValueTypeToString((object) this.N, "u32", "") + "</N>";
        str += "\r\n";
      }
      catch
      {
      }
      return str + "</RFSurveySpecStopTrigger>" + "\r\n";
    }

    public static PARAM_RFSurveySpecStopTrigger FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      namespaceManager.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      namespaceManager.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      return new PARAM_RFSurveySpecStopTrigger()
      {
        StopTriggerType = (ENUM_RFSurveySpecStopTriggerType) Enum.Parse(typeof (ENUM_RFSurveySpecStopTriggerType), XmlUtil.GetNodeValue(node, "StopTriggerType")),
        DurationPeriod = (uint) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "DurationPeriod"), "u32", ""),
        N = (uint) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "N"), "u32", "")
      };
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
      int stopTriggerType = (int) this.StopTriggerType;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.StopTriggerType, (int) this.StopTriggerType_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int durationPeriod = (int) this.DurationPeriod;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.DurationPeriod, (int) this.DurationPeriod_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int n = (int) this.N;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.N, (int) this.N_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
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
      int stopTriggerType = (int) this.StopTriggerType;
      try
      {
        Util.AppendObjToBitArray((object) this.StopTriggerType, (int) this.StopTriggerType_len, bArr);
      }
      catch
      {
      }
      int durationPeriod = (int) this.DurationPeriod;
      try
      {
        Util.AppendObjToBitArray((object) this.DurationPeriod, (int) this.DurationPeriod_len, bArr);
      }
      catch
      {
      }
      int n = (int) this.N;
      try
      {
        Util.AppendObjToBitArray((object) this.N, (int) this.N_len, bArr);
      }
      catch
      {
      }
      if (this.tvCoding)
        return;
      BitArray bitArray = Util.ConvertIntToBitArray((uint) (bArr.Length - length1) / 8U, 16);
      for (int index = 0; index < bitArray.Length; ++index)
        bArr[length1 + 16 + index] = bitArray[index];
    }
  }
}
