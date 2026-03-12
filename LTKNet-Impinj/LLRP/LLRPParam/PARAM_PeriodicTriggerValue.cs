// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_PeriodicTriggerValue
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_PeriodicTriggerValue : Parameter
  {
    public uint Offset;
    private short Offset_len;
    public uint Period;
    private short Period_len;
    public PARAM_UTCTimestamp UTCTimestamp;

    public PARAM_PeriodicTriggerValue() => this.typeID = (ushort) 180;

    public static PARAM_PeriodicTriggerValue FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_PeriodicTriggerValue) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_PeriodicTriggerValue periodicTriggerValue = new PARAM_PeriodicTriggerValue();
      periodicTriggerValue.tvCoding = bit_array[cursor];
      int val;
      if (periodicTriggerValue.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        periodicTriggerValue.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) periodicTriggerValue.length * 8;
      }
      if (val != (int) periodicTriggerValue.TypeID)
      {
        cursor = num1;
        return (PARAM_PeriodicTriggerValue) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 32;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len1);
      periodicTriggerValue.Offset = (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 32;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len2);
      periodicTriggerValue.Period = (uint) obj;
      periodicTriggerValue.UTCTimestamp = PARAM_UTCTimestamp.FromBitArray(ref bit_array, ref cursor, length);
      return periodicTriggerValue;
    }

    public override string ToString()
    {
      string str = "<PeriodicTriggerValue>" + "\r\n";
      int offset = (int) this.Offset;
      try
      {
        str = str + "  <Offset>" + Util.ConvertValueTypeToString((object) this.Offset, "u32", "") + "</Offset>";
        str += "\r\n";
      }
      catch
      {
      }
      int period = (int) this.Period;
      try
      {
        str = str + "  <Period>" + Util.ConvertValueTypeToString((object) this.Period, "u32", "") + "</Period>";
        str += "\r\n";
      }
      catch
      {
      }
      if (this.UTCTimestamp != null)
        str += Util.Indent(this.UTCTimestamp.ToString());
      return str + "</PeriodicTriggerValue>" + "\r\n";
    }

    public static PARAM_PeriodicTriggerValue FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      nsmgr.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      nsmgr.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      PARAM_PeriodicTriggerValue periodicTriggerValue = new PARAM_PeriodicTriggerValue();
      string nodeValue1 = XmlUtil.GetNodeValue(node, "Offset");
      periodicTriggerValue.Offset = (uint) Util.ParseValueTypeFromString(nodeValue1, "u32", "");
      string nodeValue2 = XmlUtil.GetNodeValue(node, "Period");
      periodicTriggerValue.Period = (uint) Util.ParseValueTypeFromString(nodeValue2, "u32", "");
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "UTCTimestamp", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            periodicTriggerValue.UTCTimestamp = PARAM_UTCTimestamp.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      return periodicTriggerValue;
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
      int offset = (int) this.Offset;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.Offset, (int) this.Offset_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int period = (int) this.Period;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.Period, (int) this.Period_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      if (this.UTCTimestamp != null)
        this.UTCTimestamp.ToBitArray(ref bit_array, ref cursor);
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
      int offset = (int) this.Offset;
      try
      {
        Util.AppendObjToBitArray((object) this.Offset, (int) this.Offset_len, bArr);
      }
      catch
      {
      }
      int period = (int) this.Period;
      try
      {
        Util.AppendObjToBitArray((object) this.Period, (int) this.Period_len, bArr);
      }
      catch
      {
      }
      if (this.UTCTimestamp != null)
        this.UTCTimestamp.AppendToBitArray(bArr);
      if (this.tvCoding)
        return;
      BitArray bitArray = Util.ConvertIntToBitArray((uint) (bArr.Length - length1) / 8U, 16);
      for (int index = 0; index < bitArray.Length; ++index)
        bArr[length1 + 16 + index] = bitArray[index];
    }
  }
}
