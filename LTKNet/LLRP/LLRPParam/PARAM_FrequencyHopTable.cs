// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_FrequencyHopTable
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_FrequencyHopTable : Parameter
  {
    public byte HopTableID;
    private short HopTableID_len;
    private const ushort param_reserved_len3 = 8;
    public UInt32Array Frequency = new UInt32Array();
    private short Frequency_len;

    public PARAM_FrequencyHopTable() => this.typeID = (ushort) 147;

    public static PARAM_FrequencyHopTable FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_FrequencyHopTable) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_FrequencyHopTable frequencyHopTable = new PARAM_FrequencyHopTable();
      frequencyHopTable.tvCoding = bit_array[cursor];
      int val;
      if (frequencyHopTable.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        frequencyHopTable.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) frequencyHopTable.length * 8;
      }
      if (val != (int) frequencyHopTable.TypeID)
      {
        cursor = num1;
        return (PARAM_FrequencyHopTable) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len = 8;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (byte), field_len);
      frequencyHopTable.HopTableID = (byte) obj;
      cursor += 8;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int fieldLength = Util.DetermineFieldLength(ref bit_array, ref cursor);
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (UInt32Array), fieldLength);
      frequencyHopTable.Frequency = (UInt32Array) obj;
      return frequencyHopTable;
    }

    public override string ToString()
    {
      string str = "<FrequencyHopTable>" + "\r\n";
      int hopTableId = (int) this.HopTableID;
      try
      {
        str = str + "  <HopTableID>" + Util.ConvertValueTypeToString((object) this.HopTableID, "u8", "") + "</HopTableID>";
        str += "\r\n";
      }
      catch
      {
      }
      if (this.Frequency != null)
      {
        try
        {
          str = str + "  <Frequency>" + Util.ConvertArrayTypeToString((object) this.Frequency, "u32v", "") + "</Frequency>";
          str += "\r\n";
        }
        catch
        {
        }
      }
      return str + "</FrequencyHopTable>" + "\r\n";
    }

    public static PARAM_FrequencyHopTable FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      namespaceManager.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      namespaceManager.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      return new PARAM_FrequencyHopTable()
      {
        HopTableID = (byte) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "HopTableID"), "u8", ""),
        Frequency = (UInt32Array) Util.ParseArrayTypeFromString(XmlUtil.GetNodeValue(node, "Frequency"), "u32v", "")
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
      int hopTableId = (int) this.HopTableID;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.HopTableID, (int) this.HopTableID_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      cursor += 8;
      if (this.Frequency != null)
      {
        try
        {
          Util.ConvertIntToBitArray((uint) this.Frequency.Count, 16).CopyTo((Array) bit_array, cursor);
          cursor += 16;
          BitArray bitArray = Util.ConvertObjToBitArray((object) this.Frequency, (int) this.Frequency_len);
          bitArray.CopyTo((Array) bit_array, cursor);
          cursor += bitArray.Length;
        }
        catch
        {
        }
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
      int hopTableId = (int) this.HopTableID;
      try
      {
        Util.AppendObjToBitArray((object) this.HopTableID, (int) this.HopTableID_len, bArr);
      }
      catch
      {
      }
      bArr.Length += 8;
      if (this.Frequency != null)
      {
        try
        {
          Util.AppendIntToBitArray((uint) this.Frequency.Count, 16, bArr);
          Util.AppendObjToBitArray((object) this.Frequency, (int) this.Frequency_len, bArr);
        }
        catch
        {
        }
      }
      if (this.tvCoding)
        return;
      BitArray bitArray = Util.ConvertIntToBitArray((uint) (bArr.Length - length1) / 8U, 16);
      for (int index = 0; index < bitArray.Length; ++index)
        bArr[length1 + 16 + index] = bitArray[index];
    }
  }
}
