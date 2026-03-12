// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_C1G2LLRPCapabilities
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_C1G2LLRPCapabilities : Parameter
  {
    public bool CanSupportBlockErase;
    private short CanSupportBlockErase_len;
    public bool CanSupportBlockWrite;
    private short CanSupportBlockWrite_len;
    private const ushort param_reserved_len4 = 6;
    public ushort MaxNumSelectFiltersPerQuery;
    private short MaxNumSelectFiltersPerQuery_len;

    public PARAM_C1G2LLRPCapabilities() => this.typeID = (ushort) 327;

    public static PARAM_C1G2LLRPCapabilities FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_C1G2LLRPCapabilities) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_C1G2LLRPCapabilities llrpCapabilities = new PARAM_C1G2LLRPCapabilities();
      llrpCapabilities.tvCoding = bit_array[cursor];
      int val;
      if (llrpCapabilities.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        llrpCapabilities.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) llrpCapabilities.length * 8;
      }
      if (val != (int) llrpCapabilities.TypeID)
      {
        cursor = num1;
        return (PARAM_C1G2LLRPCapabilities) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 1;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (bool), field_len1);
      llrpCapabilities.CanSupportBlockErase = (bool) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 1;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (bool), field_len2);
      llrpCapabilities.CanSupportBlockWrite = (bool) obj;
      cursor += 6;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len3 = 16;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len3);
      llrpCapabilities.MaxNumSelectFiltersPerQuery = (ushort) obj;
      return llrpCapabilities;
    }

    public override string ToString()
    {
      string str = "<C1G2LLRPCapabilities>" + "\r\n";
      int num1 = this.CanSupportBlockErase ? 1 : 0;
      try
      {
        str = str + "  <CanSupportBlockErase>" + Util.ConvertValueTypeToString((object) this.CanSupportBlockErase, "u1", "") + "</CanSupportBlockErase>";
        str += "\r\n";
      }
      catch
      {
      }
      int num2 = this.CanSupportBlockWrite ? 1 : 0;
      try
      {
        str = str + "  <CanSupportBlockWrite>" + Util.ConvertValueTypeToString((object) this.CanSupportBlockWrite, "u1", "") + "</CanSupportBlockWrite>";
        str += "\r\n";
      }
      catch
      {
      }
      int selectFiltersPerQuery = (int) this.MaxNumSelectFiltersPerQuery;
      try
      {
        str = str + "  <MaxNumSelectFiltersPerQuery>" + Util.ConvertValueTypeToString((object) this.MaxNumSelectFiltersPerQuery, "u16", "") + "</MaxNumSelectFiltersPerQuery>";
        str += "\r\n";
      }
      catch
      {
      }
      return str + "</C1G2LLRPCapabilities>" + "\r\n";
    }

    public static PARAM_C1G2LLRPCapabilities FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      namespaceManager.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      namespaceManager.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      return new PARAM_C1G2LLRPCapabilities()
      {
        CanSupportBlockErase = (bool) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "CanSupportBlockErase"), "u1", ""),
        CanSupportBlockWrite = (bool) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "CanSupportBlockWrite"), "u1", ""),
        MaxNumSelectFiltersPerQuery = (ushort) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "MaxNumSelectFiltersPerQuery"), "u16", "")
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
      int num2 = this.CanSupportBlockErase ? 1 : 0;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.CanSupportBlockErase, (int) this.CanSupportBlockErase_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int num3 = this.CanSupportBlockWrite ? 1 : 0;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.CanSupportBlockWrite, (int) this.CanSupportBlockWrite_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      cursor += 6;
      int selectFiltersPerQuery = (int) this.MaxNumSelectFiltersPerQuery;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.MaxNumSelectFiltersPerQuery, (int) this.MaxNumSelectFiltersPerQuery_len);
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
      int num1 = this.CanSupportBlockErase ? 1 : 0;
      try
      {
        Util.AppendObjToBitArray((object) this.CanSupportBlockErase, (int) this.CanSupportBlockErase_len, bArr);
      }
      catch
      {
      }
      int num2 = this.CanSupportBlockWrite ? 1 : 0;
      try
      {
        Util.AppendObjToBitArray((object) this.CanSupportBlockWrite, (int) this.CanSupportBlockWrite_len, bArr);
      }
      catch
      {
      }
      bArr.Length += 6;
      int selectFiltersPerQuery = (int) this.MaxNumSelectFiltersPerQuery;
      try
      {
        Util.AppendObjToBitArray((object) this.MaxNumSelectFiltersPerQuery, (int) this.MaxNumSelectFiltersPerQuery_len, bArr);
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
