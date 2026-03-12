// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_AISpecStopTrigger
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_AISpecStopTrigger : Parameter
  {
    public ENUM_AISpecStopTriggerType AISpecStopTriggerType;
    private short AISpecStopTriggerType_len = 8;
    public uint DurationTrigger;
    private short DurationTrigger_len;
    public PARAM_GPITriggerValue GPITriggerValue;
    public PARAM_TagObservationTrigger TagObservationTrigger;

    public PARAM_AISpecStopTrigger() => this.typeID = (ushort) 184;

    public static PARAM_AISpecStopTrigger FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_AISpecStopTrigger) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_AISpecStopTrigger aiSpecStopTrigger = new PARAM_AISpecStopTrigger();
      aiSpecStopTrigger.tvCoding = bit_array[cursor];
      int val;
      if (aiSpecStopTrigger.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        aiSpecStopTrigger.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) aiSpecStopTrigger.length * 8;
      }
      if (val != (int) aiSpecStopTrigger.TypeID)
      {
        cursor = num1;
        return (PARAM_AISpecStopTrigger) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 8;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len1);
      aiSpecStopTrigger.AISpecStopTriggerType = (ENUM_AISpecStopTriggerType) (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 32;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len2);
      aiSpecStopTrigger.DurationTrigger = (uint) obj;
      aiSpecStopTrigger.GPITriggerValue = PARAM_GPITriggerValue.FromBitArray(ref bit_array, ref cursor, length);
      aiSpecStopTrigger.TagObservationTrigger = PARAM_TagObservationTrigger.FromBitArray(ref bit_array, ref cursor, length);
      return aiSpecStopTrigger;
    }

    public override string ToString()
    {
      string str = "<AISpecStopTrigger>" + "\r\n";
      int specStopTriggerType = (int) this.AISpecStopTriggerType;
      try
      {
        str = str + "  <AISpecStopTriggerType>" + this.AISpecStopTriggerType.ToString() + "</AISpecStopTriggerType>";
        str += "\r\n";
      }
      catch
      {
      }
      int durationTrigger = (int) this.DurationTrigger;
      try
      {
        str = str + "  <DurationTrigger>" + Util.ConvertValueTypeToString((object) this.DurationTrigger, "u32", "") + "</DurationTrigger>";
        str += "\r\n";
      }
      catch
      {
      }
      if (this.GPITriggerValue != null)
        str += Util.Indent(this.GPITriggerValue.ToString());
      if (this.TagObservationTrigger != null)
        str += Util.Indent(this.TagObservationTrigger.ToString());
      return str + "</AISpecStopTrigger>" + "\r\n";
    }

    public static PARAM_AISpecStopTrigger FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      nsmgr.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      nsmgr.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      PARAM_AISpecStopTrigger aiSpecStopTrigger = new PARAM_AISpecStopTrigger();
      string nodeValue1 = XmlUtil.GetNodeValue(node, "AISpecStopTriggerType");
      aiSpecStopTrigger.AISpecStopTriggerType = (ENUM_AISpecStopTriggerType) Enum.Parse(typeof (ENUM_AISpecStopTriggerType), nodeValue1);
      string nodeValue2 = XmlUtil.GetNodeValue(node, "DurationTrigger");
      aiSpecStopTrigger.DurationTrigger = (uint) Util.ParseValueTypeFromString(nodeValue2, "u32", "");
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "GPITriggerValue", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            aiSpecStopTrigger.GPITriggerValue = PARAM_GPITriggerValue.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "TagObservationTrigger", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            aiSpecStopTrigger.TagObservationTrigger = PARAM_TagObservationTrigger.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      return aiSpecStopTrigger;
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
      int specStopTriggerType = (int) this.AISpecStopTriggerType;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.AISpecStopTriggerType, (int) this.AISpecStopTriggerType_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int durationTrigger = (int) this.DurationTrigger;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.DurationTrigger, (int) this.DurationTrigger_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      if (this.GPITriggerValue != null)
        this.GPITriggerValue.ToBitArray(ref bit_array, ref cursor);
      if (this.TagObservationTrigger != null)
        this.TagObservationTrigger.ToBitArray(ref bit_array, ref cursor);
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
      int specStopTriggerType = (int) this.AISpecStopTriggerType;
      try
      {
        Util.AppendObjToBitArray((object) this.AISpecStopTriggerType, (int) this.AISpecStopTriggerType_len, bArr);
      }
      catch
      {
      }
      int durationTrigger = (int) this.DurationTrigger;
      try
      {
        Util.AppendObjToBitArray((object) this.DurationTrigger, (int) this.DurationTrigger_len, bArr);
      }
      catch
      {
      }
      if (this.GPITriggerValue != null)
        this.GPITriggerValue.AppendToBitArray(bArr);
      if (this.TagObservationTrigger != null)
        this.TagObservationTrigger.AppendToBitArray(bArr);
      if (this.tvCoding)
        return;
      BitArray bitArray = Util.ConvertIntToBitArray((uint) (bArr.Length - length1) / 8U, 16);
      for (int index = 0; index < bitArray.Length; ++index)
        bArr[length1 + 16 + index] = bitArray[index];
    }
  }
}
