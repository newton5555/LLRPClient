// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_FrequencyRSSILevelEntry
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_FrequencyRSSILevelEntry : Parameter
  {
    public uint Frequency;
    private short Frequency_len;
    public uint Bandwidth;
    private short Bandwidth_len;
    public sbyte AverageRSSI;
    private short AverageRSSI_len;
    public sbyte PeakRSSI;
    private short PeakRSSI_len;
    public UNION_Timestamp Timestamp = new UNION_Timestamp();

    public PARAM_FrequencyRSSILevelEntry() => this.typeID = (ushort) 243;

    public static PARAM_FrequencyRSSILevelEntry FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_FrequencyRSSILevelEntry) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_FrequencyRSSILevelEntry frequencyRssiLevelEntry = new PARAM_FrequencyRSSILevelEntry();
      frequencyRssiLevelEntry.tvCoding = bit_array[cursor];
      int val1;
      if (frequencyRssiLevelEntry.tvCoding)
      {
        ++cursor;
        val1 = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val1 = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        frequencyRssiLevelEntry.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) frequencyRssiLevelEntry.length * 8;
      }
      if (val1 != (int) frequencyRssiLevelEntry.TypeID)
      {
        cursor = num1;
        return (PARAM_FrequencyRSSILevelEntry) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 32;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len1);
      frequencyRssiLevelEntry.Frequency = (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 32;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len2);
      frequencyRssiLevelEntry.Bandwidth = (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len3 = 8;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (sbyte), field_len3);
      frequencyRssiLevelEntry.AverageRSSI = (sbyte) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len4 = 8;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (sbyte), field_len4);
      frequencyRssiLevelEntry.PeakRSSI = (sbyte) obj;
      ushort num3 = 1;
      while (num3 != (ushort) 0)
      {
        num3 = (ushort) 0;
        PARAM_UTCTimestamp val2 = PARAM_UTCTimestamp.FromBitArray(ref bit_array, ref cursor, length);
        if (val2 != null)
        {
          ++num3;
          frequencyRssiLevelEntry.Timestamp.Add((IParameter) val2);
        }
        PARAM_Uptime val3 = PARAM_Uptime.FromBitArray(ref bit_array, ref cursor, length);
        if (val3 != null)
        {
          ++num3;
          frequencyRssiLevelEntry.Timestamp.Add((IParameter) val3);
        }
      }
      return frequencyRssiLevelEntry;
    }

    public override string ToString()
    {
      string str = "<FrequencyRSSILevelEntry>" + "\r\n";
      int frequency = (int) this.Frequency;
      try
      {
        str = str + "  <Frequency>" + Util.ConvertValueTypeToString((object) this.Frequency, "u32", "") + "</Frequency>";
        str += "\r\n";
      }
      catch
      {
      }
      int bandwidth = (int) this.Bandwidth;
      try
      {
        str = str + "  <Bandwidth>" + Util.ConvertValueTypeToString((object) this.Bandwidth, "u32", "") + "</Bandwidth>";
        str += "\r\n";
      }
      catch
      {
      }
      int averageRssi = (int) this.AverageRSSI;
      try
      {
        str = str + "  <AverageRSSI>" + Util.ConvertValueTypeToString((object) this.AverageRSSI, "s8", "") + "</AverageRSSI>";
        str += "\r\n";
      }
      catch
      {
      }
      int peakRssi = (int) this.PeakRSSI;
      try
      {
        str = str + "  <PeakRSSI>" + Util.ConvertValueTypeToString((object) this.PeakRSSI, "s8", "") + "</PeakRSSI>";
        str += "\r\n";
      }
      catch
      {
      }
      if (this.Timestamp != null)
      {
        int count = this.Timestamp.Count;
        for (int index = 0; index < count; ++index)
          str += Util.Indent(this.Timestamp[index].ToString());
      }
      return str + "</FrequencyRSSILevelEntry>" + "\r\n";
    }

    public static PARAM_FrequencyRSSILevelEntry FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      namespaceManager.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      namespaceManager.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      PARAM_FrequencyRSSILevelEntry frequencyRssiLevelEntry = new PARAM_FrequencyRSSILevelEntry();
      string nodeValue1 = XmlUtil.GetNodeValue(node, "Frequency");
      frequencyRssiLevelEntry.Frequency = (uint) Util.ParseValueTypeFromString(nodeValue1, "u32", "");
      string nodeValue2 = XmlUtil.GetNodeValue(node, "Bandwidth");
      frequencyRssiLevelEntry.Bandwidth = (uint) Util.ParseValueTypeFromString(nodeValue2, "u32", "");
      string nodeValue3 = XmlUtil.GetNodeValue(node, "AverageRSSI");
      frequencyRssiLevelEntry.AverageRSSI = (sbyte) Util.ParseValueTypeFromString(nodeValue3, "s8", "");
      string nodeValue4 = XmlUtil.GetNodeValue(node, "PeakRSSI");
      frequencyRssiLevelEntry.PeakRSSI = (sbyte) Util.ParseValueTypeFromString(nodeValue4, "s8", "");
      frequencyRssiLevelEntry.Timestamp = new UNION_Timestamp();
      try
      {
        foreach (XmlNode childNode in node.ChildNodes)
        {
          switch (childNode.Name)
          {
            case "UTCTimestamp":
              frequencyRssiLevelEntry.Timestamp.Add((IParameter) PARAM_UTCTimestamp.FromXmlNode(childNode));
              continue;
            case "Uptime":
              frequencyRssiLevelEntry.Timestamp.Add((IParameter) PARAM_Uptime.FromXmlNode(childNode));
              continue;
            default:
              continue;
          }
        }
      }
      catch
      {
      }
      return frequencyRssiLevelEntry;
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
      int frequency = (int) this.Frequency;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.Frequency, (int) this.Frequency_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int bandwidth = (int) this.Bandwidth;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.Bandwidth, (int) this.Bandwidth_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int averageRssi = (int) this.AverageRSSI;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.AverageRSSI, (int) this.AverageRSSI_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int peakRssi = (int) this.PeakRSSI;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.PeakRSSI, (int) this.PeakRSSI_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int count = this.Timestamp.Count;
      for (int index = 0; index < count; ++index)
        this.Timestamp[index].ToBitArray(ref bit_array, ref cursor);
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
      int frequency = (int) this.Frequency;
      try
      {
        Util.AppendObjToBitArray((object) this.Frequency, (int) this.Frequency_len, bArr);
      }
      catch
      {
      }
      int bandwidth = (int) this.Bandwidth;
      try
      {
        Util.AppendObjToBitArray((object) this.Bandwidth, (int) this.Bandwidth_len, bArr);
      }
      catch
      {
      }
      int averageRssi = (int) this.AverageRSSI;
      try
      {
        Util.AppendObjToBitArray((object) this.AverageRSSI, (int) this.AverageRSSI_len, bArr);
      }
      catch
      {
      }
      int peakRssi = (int) this.PeakRSSI;
      try
      {
        Util.AppendObjToBitArray((object) this.PeakRSSI, (int) this.PeakRSSI_len, bArr);
      }
      catch
      {
      }
      int count = this.Timestamp.Count;
      for (int index = 0; index < count; ++index)
        this.Timestamp[index].AppendToBitArray(bArr);
      if (this.tvCoding)
        return;
      BitArray bitArray = Util.ConvertIntToBitArray((uint) (bArr.Length - length1) / 8U, 16);
      for (int index = 0; index < bitArray.Length; ++index)
        bArr[length1 + 16 + index] = bitArray[index];
    }
  }
}
