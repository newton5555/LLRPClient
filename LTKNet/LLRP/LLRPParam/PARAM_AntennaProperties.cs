// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_AntennaProperties
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_AntennaProperties : Parameter
  {
    public bool AntennaConnected;
    private short AntennaConnected_len;
    private const ushort param_reserved_len3 = 7;
    public ushort AntennaID;
    private short AntennaID_len;
    public short AntennaGain;
    private short AntennaGain_len;

    public PARAM_AntennaProperties() => this.typeID = (ushort) 221;

    public static PARAM_AntennaProperties FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_AntennaProperties) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_AntennaProperties antennaProperties = new PARAM_AntennaProperties();
      antennaProperties.tvCoding = bit_array[cursor];
      int val;
      if (antennaProperties.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        antennaProperties.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) antennaProperties.length * 8;
      }
      if (val != (int) antennaProperties.TypeID)
      {
        cursor = num1;
        return (PARAM_AntennaProperties) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 1;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (bool), field_len1);
      antennaProperties.AntennaConnected = (bool) obj;
      cursor += 7;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 16;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len2);
      antennaProperties.AntennaID = (ushort) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len3 = 16;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (short), field_len3);
      antennaProperties.AntennaGain = (short) obj;
      return antennaProperties;
    }

    public override string ToString()
    {
      string str = "<AntennaProperties>" + "\r\n";
      int num = this.AntennaConnected ? 1 : 0;
      try
      {
        str = str + "  <AntennaConnected>" + Util.ConvertValueTypeToString((object) this.AntennaConnected, "u1", "") + "</AntennaConnected>";
        str += "\r\n";
      }
      catch
      {
      }
      int antennaId = (int) this.AntennaID;
      try
      {
        str = str + "  <AntennaID>" + Util.ConvertValueTypeToString((object) this.AntennaID, "u16", "") + "</AntennaID>";
        str += "\r\n";
      }
      catch
      {
      }
      int antennaGain = (int) this.AntennaGain;
      try
      {
        str = str + "  <AntennaGain>" + Util.ConvertValueTypeToString((object) this.AntennaGain, "s16", "") + "</AntennaGain>";
        str += "\r\n";
      }
      catch
      {
      }
      return str + "</AntennaProperties>" + "\r\n";
    }

    public static PARAM_AntennaProperties FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      namespaceManager.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      namespaceManager.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      return new PARAM_AntennaProperties()
      {
        AntennaConnected = (bool) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "AntennaConnected"), "u1", ""),
        AntennaID = (ushort) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "AntennaID"), "u16", ""),
        AntennaGain = (short) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "AntennaGain"), "s16", "")
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
      int num2 = this.AntennaConnected ? 1 : 0;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.AntennaConnected, (int) this.AntennaConnected_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      cursor += 7;
      int antennaId = (int) this.AntennaID;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.AntennaID, (int) this.AntennaID_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int antennaGain = (int) this.AntennaGain;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.AntennaGain, (int) this.AntennaGain_len);
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
      int num = this.AntennaConnected ? 1 : 0;
      try
      {
        Util.AppendObjToBitArray((object) this.AntennaConnected, (int) this.AntennaConnected_len, bArr);
      }
      catch
      {
      }
      bArr.Length += 7;
      int antennaId = (int) this.AntennaID;
      try
      {
        Util.AppendObjToBitArray((object) this.AntennaID, (int) this.AntennaID_len, bArr);
      }
      catch
      {
      }
      int antennaGain = (int) this.AntennaGain;
      try
      {
        Util.AppendObjToBitArray((object) this.AntennaGain, (int) this.AntennaGain_len, bArr);
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
