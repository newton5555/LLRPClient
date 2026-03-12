// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_RFSurveyEvent
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_RFSurveyEvent : Parameter
  {
    public ENUM_RFSurveyEventType EventType;
    private short EventType_len = 8;
    public uint ROSpecID;
    private short ROSpecID_len;
    public ushort SpecIndex;
    private short SpecIndex_len;

    public PARAM_RFSurveyEvent() => this.typeID = (ushort) 253;

    public static PARAM_RFSurveyEvent FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_RFSurveyEvent) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_RFSurveyEvent paramRfSurveyEvent = new PARAM_RFSurveyEvent();
      paramRfSurveyEvent.tvCoding = bit_array[cursor];
      int val;
      if (paramRfSurveyEvent.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        paramRfSurveyEvent.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) paramRfSurveyEvent.length * 8;
      }
      if (val != (int) paramRfSurveyEvent.TypeID)
      {
        cursor = num1;
        return (PARAM_RFSurveyEvent) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 8;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len1);
      paramRfSurveyEvent.EventType = (ENUM_RFSurveyEventType) (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 32;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len2);
      paramRfSurveyEvent.ROSpecID = (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len3 = 16;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len3);
      paramRfSurveyEvent.SpecIndex = (ushort) obj;
      return paramRfSurveyEvent;
    }

    public override string ToString()
    {
      string str = "<RFSurveyEvent>" + "\r\n";
      int eventType = (int) this.EventType;
      try
      {
        str = str + "  <EventType>" + this.EventType.ToString() + "</EventType>";
        str += "\r\n";
      }
      catch
      {
      }
      int roSpecId = (int) this.ROSpecID;
      try
      {
        str = str + "  <ROSpecID>" + Util.ConvertValueTypeToString((object) this.ROSpecID, "u32", "") + "</ROSpecID>";
        str += "\r\n";
      }
      catch
      {
      }
      int specIndex = (int) this.SpecIndex;
      try
      {
        str = str + "  <SpecIndex>" + Util.ConvertValueTypeToString((object) this.SpecIndex, "u16", "") + "</SpecIndex>";
        str += "\r\n";
      }
      catch
      {
      }
      return str + "</RFSurveyEvent>" + "\r\n";
    }

    public static PARAM_RFSurveyEvent FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      namespaceManager.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      namespaceManager.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      return new PARAM_RFSurveyEvent()
      {
        EventType = (ENUM_RFSurveyEventType) Enum.Parse(typeof (ENUM_RFSurveyEventType), XmlUtil.GetNodeValue(node, "EventType")),
        ROSpecID = (uint) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "ROSpecID"), "u32", ""),
        SpecIndex = (ushort) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "SpecIndex"), "u16", "")
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
      int eventType = (int) this.EventType;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.EventType, (int) this.EventType_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int roSpecId = (int) this.ROSpecID;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.ROSpecID, (int) this.ROSpecID_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int specIndex = (int) this.SpecIndex;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.SpecIndex, (int) this.SpecIndex_len);
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
      int eventType = (int) this.EventType;
      try
      {
        Util.AppendObjToBitArray((object) this.EventType, (int) this.EventType_len, bArr);
      }
      catch
      {
      }
      int roSpecId = (int) this.ROSpecID;
      try
      {
        Util.AppendObjToBitArray((object) this.ROSpecID, (int) this.ROSpecID_len, bArr);
      }
      catch
      {
      }
      int specIndex = (int) this.SpecIndex;
      try
      {
        Util.AppendObjToBitArray((object) this.SpecIndex, (int) this.SpecIndex_len, bArr);
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
