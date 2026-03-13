// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_TransmitPowerLevelTableEntry
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_TransmitPowerLevelTableEntry : Parameter
  {
    public ushort Index;
    private short Index_len;
    public short TransmitPowerValue;
    private short TransmitPowerValue_len;

    public PARAM_TransmitPowerLevelTableEntry() => this.typeID = (ushort) 145;

    public static PARAM_TransmitPowerLevelTableEntry FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_TransmitPowerLevelTableEntry) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_TransmitPowerLevelTableEntry powerLevelTableEntry = new PARAM_TransmitPowerLevelTableEntry();
      powerLevelTableEntry.tvCoding = bit_array[cursor];
      int val;
      if (powerLevelTableEntry.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        powerLevelTableEntry.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) powerLevelTableEntry.length * 8;
      }
      if (val != (int) powerLevelTableEntry.TypeID)
      {
        cursor = num1;
        return (PARAM_TransmitPowerLevelTableEntry) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 16;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len1);
      powerLevelTableEntry.Index = (ushort) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 16;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (short), field_len2);
      powerLevelTableEntry.TransmitPowerValue = (short) obj;
      return powerLevelTableEntry;
    }

    public override string ToString()
    {
      string str = "<TransmitPowerLevelTableEntry>" + "\r\n";
      int index = (int) this.Index;
      try
      {
        str = str + "  <Index>" + Util.ConvertValueTypeToString((object) this.Index, "u16", "") + "</Index>";
        str += "\r\n";
      }
      catch
      {
      }
      int transmitPowerValue = (int) this.TransmitPowerValue;
      try
      {
        str = str + "  <TransmitPowerValue>" + Util.ConvertValueTypeToString((object) this.TransmitPowerValue, "s16", "") + "</TransmitPowerValue>";
        str += "\r\n";
      }
      catch
      {
      }
      return str + "</TransmitPowerLevelTableEntry>" + "\r\n";
    }

    public static PARAM_TransmitPowerLevelTableEntry FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      namespaceManager.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      namespaceManager.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      return new PARAM_TransmitPowerLevelTableEntry()
      {
        Index = (ushort) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "Index"), "u16", ""),
        TransmitPowerValue = (short) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "TransmitPowerValue"), "s16", "")
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
      int index = (int) this.Index;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.Index, (int) this.Index_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int transmitPowerValue = (int) this.TransmitPowerValue;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.TransmitPowerValue, (int) this.TransmitPowerValue_len);
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
      int index1 = (int) this.Index;
      try
      {
        Util.AppendObjToBitArray((object) this.Index, (int) this.Index_len, bArr);
      }
      catch
      {
      }
      int transmitPowerValue = (int) this.TransmitPowerValue;
      try
      {
        Util.AppendObjToBitArray((object) this.TransmitPowerValue, (int) this.TransmitPowerValue_len, bArr);
      }
      catch
      {
      }
      if (this.tvCoding)
        return;
      BitArray bitArray = Util.ConvertIntToBitArray((uint) (bArr.Length - length1) / 8U, 16);
      for (int index2 = 0; index2 < bitArray.Length; ++index2)
        bArr[length1 + 16 + index2] = bitArray[index2];
    }
  }
}
