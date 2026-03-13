// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_TagObservationTrigger
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_TagObservationTrigger : Parameter
  {
    public ENUM_TagObservationTriggerType TriggerType;
    private short TriggerType_len = 8;
    private const ushort param_reserved_len3 = 8;
    public ushort NumberOfTags;
    private short NumberOfTags_len;
    public ushort NumberOfAttempts;
    private short NumberOfAttempts_len;
    public ushort T;
    private short T_len;
    public uint Timeout;
    private short Timeout_len;

    public PARAM_TagObservationTrigger() => this.typeID = (ushort) 185;

    public static PARAM_TagObservationTrigger FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_TagObservationTrigger) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_TagObservationTrigger observationTrigger = new PARAM_TagObservationTrigger();
      observationTrigger.tvCoding = bit_array[cursor];
      int val;
      if (observationTrigger.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        observationTrigger.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) observationTrigger.length * 8;
      }
      if (val != (int) observationTrigger.TypeID)
      {
        cursor = num1;
        return (PARAM_TagObservationTrigger) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 8;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len1);
      observationTrigger.TriggerType = (ENUM_TagObservationTriggerType) (uint) obj;
      cursor += 8;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 16;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len2);
      observationTrigger.NumberOfTags = (ushort) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len3 = 16;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len3);
      observationTrigger.NumberOfAttempts = (ushort) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len4 = 16;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len4);
      observationTrigger.T = (ushort) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len5 = 32;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len5);
      observationTrigger.Timeout = (uint) obj;
      return observationTrigger;
    }

    public override string ToString()
    {
      string str = "<TagObservationTrigger>" + "\r\n";
      int triggerType = (int) this.TriggerType;
      try
      {
        str = str + "  <TriggerType>" + this.TriggerType.ToString() + "</TriggerType>";
        str += "\r\n";
      }
      catch
      {
      }
      int numberOfTags = (int) this.NumberOfTags;
      try
      {
        str = str + "  <NumberOfTags>" + Util.ConvertValueTypeToString((object) this.NumberOfTags, "u16", "") + "</NumberOfTags>";
        str += "\r\n";
      }
      catch
      {
      }
      int numberOfAttempts = (int) this.NumberOfAttempts;
      try
      {
        str = str + "  <NumberOfAttempts>" + Util.ConvertValueTypeToString((object) this.NumberOfAttempts, "u16", "") + "</NumberOfAttempts>";
        str += "\r\n";
      }
      catch
      {
      }
      int t = (int) this.T;
      try
      {
        str = str + "  <T>" + Util.ConvertValueTypeToString((object) this.T, "u16", "") + "</T>";
        str += "\r\n";
      }
      catch
      {
      }
      int timeout = (int) this.Timeout;
      try
      {
        str = str + "  <Timeout>" + Util.ConvertValueTypeToString((object) this.Timeout, "u32", "") + "</Timeout>";
        str += "\r\n";
      }
      catch
      {
      }
      return str + "</TagObservationTrigger>" + "\r\n";
    }

    public static PARAM_TagObservationTrigger FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      namespaceManager.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      namespaceManager.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      return new PARAM_TagObservationTrigger()
      {
        TriggerType = (ENUM_TagObservationTriggerType) Enum.Parse(typeof (ENUM_TagObservationTriggerType), XmlUtil.GetNodeValue(node, "TriggerType")),
        NumberOfTags = (ushort) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "NumberOfTags"), "u16", ""),
        NumberOfAttempts = (ushort) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "NumberOfAttempts"), "u16", ""),
        T = (ushort) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "T"), "u16", ""),
        Timeout = (uint) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "Timeout"), "u32", "")
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
      int triggerType = (int) this.TriggerType;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.TriggerType, (int) this.TriggerType_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      cursor += 8;
      int numberOfTags = (int) this.NumberOfTags;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.NumberOfTags, (int) this.NumberOfTags_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int numberOfAttempts = (int) this.NumberOfAttempts;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.NumberOfAttempts, (int) this.NumberOfAttempts_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int t = (int) this.T;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.T, (int) this.T_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int timeout = (int) this.Timeout;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.Timeout, (int) this.Timeout_len);
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
      int triggerType = (int) this.TriggerType;
      try
      {
        Util.AppendObjToBitArray((object) this.TriggerType, (int) this.TriggerType_len, bArr);
      }
      catch
      {
      }
      bArr.Length += 8;
      int numberOfTags = (int) this.NumberOfTags;
      try
      {
        Util.AppendObjToBitArray((object) this.NumberOfTags, (int) this.NumberOfTags_len, bArr);
      }
      catch
      {
      }
      int numberOfAttempts = (int) this.NumberOfAttempts;
      try
      {
        Util.AppendObjToBitArray((object) this.NumberOfAttempts, (int) this.NumberOfAttempts_len, bArr);
      }
      catch
      {
      }
      int t = (int) this.T;
      try
      {
        Util.AppendObjToBitArray((object) this.T, (int) this.T_len, bArr);
      }
      catch
      {
      }
      int timeout = (int) this.Timeout;
      try
      {
        Util.AppendObjToBitArray((object) this.Timeout, (int) this.Timeout_len, bArr);
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
