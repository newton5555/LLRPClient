// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_GPITriggerValue
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_GPITriggerValue : Parameter
  {
    public ushort GPIPortNum;
    private short GPIPortNum_len;
    public bool GPIEvent;
    private short GPIEvent_len;
    private const ushort param_reserved_len4 = 7;
    public uint Timeout;
    private short Timeout_len;

    public PARAM_GPITriggerValue() => this.typeID = (ushort) 181;

    public static PARAM_GPITriggerValue FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_GPITriggerValue) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_GPITriggerValue paramGpiTriggerValue = new PARAM_GPITriggerValue();
      paramGpiTriggerValue.tvCoding = bit_array[cursor];
      int val;
      if (paramGpiTriggerValue.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        paramGpiTriggerValue.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) paramGpiTriggerValue.length * 8;
      }
      if (val != (int) paramGpiTriggerValue.TypeID)
      {
        cursor = num1;
        return (PARAM_GPITriggerValue) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 16;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len1);
      paramGpiTriggerValue.GPIPortNum = (ushort) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 1;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (bool), field_len2);
      paramGpiTriggerValue.GPIEvent = (bool) obj;
      cursor += 7;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len3 = 32;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len3);
      paramGpiTriggerValue.Timeout = (uint) obj;
      return paramGpiTriggerValue;
    }

    public override string ToString()
    {
      string str = "<GPITriggerValue>" + "\r\n";
      int gpiPortNum = (int) this.GPIPortNum;
      try
      {
        str = str + "  <GPIPortNum>" + Util.ConvertValueTypeToString((object) this.GPIPortNum, "u16", "") + "</GPIPortNum>";
        str += "\r\n";
      }
      catch
      {
      }
      int num = this.GPIEvent ? 1 : 0;
      try
      {
        str = str + "  <GPIEvent>" + Util.ConvertValueTypeToString((object) this.GPIEvent, "u1", "") + "</GPIEvent>";
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
      return str + "</GPITriggerValue>" + "\r\n";
    }

    public static PARAM_GPITriggerValue FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      namespaceManager.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      namespaceManager.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      return new PARAM_GPITriggerValue()
      {
        GPIPortNum = (ushort) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "GPIPortNum"), "u16", ""),
        GPIEvent = (bool) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "GPIEvent"), "u1", ""),
        Timeout = (uint) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "Timeout"), "u32", "")
      };
    }

    public override void ToBitArray(ref bool[] bit_array, ref int cursor)
    {
      int num1 = cursor;
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
      int gpiPortNum = (int) this.GPIPortNum;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.GPIPortNum, (int) this.GPIPortNum_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int num2 = this.GPIEvent ? 1 : 0;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.GPIEvent, (int) this.GPIEvent_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      cursor += 7;
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
      Util.ConvertIntToBitArray((uint) (cursor - num1) / 8U, 16).CopyTo((Array) bit_array, num1 + 16);
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
      int gpiPortNum = (int) this.GPIPortNum;
      try
      {
        Util.AppendObjToBitArray((object) this.GPIPortNum, (int) this.GPIPortNum_len, bArr);
      }
      catch
      {
      }
      int num = this.GPIEvent ? 1 : 0;
      try
      {
        Util.AppendObjToBitArray((object) this.GPIEvent, (int) this.GPIEvent_len, bArr);
      }
      catch
      {
      }
      bArr.Length += 7;
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
