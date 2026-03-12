// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_C1G2Write
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_C1G2Write : Parameter
  {
    public ushort OpSpecID;
    private short OpSpecID_len;
    public uint AccessPassword;
    private short AccessPassword_len;
    public TwoBits MB = new TwoBits((ushort) 0);
    private short MB_len;
    private const ushort param_reserved_len5 = 6;
    public ushort WordPointer;
    private short WordPointer_len;
    public UInt16Array WriteData = new UInt16Array();
    private short WriteData_len;

    public PARAM_C1G2Write() => this.typeID = (ushort) 342;

    public static PARAM_C1G2Write FromBitArray(ref BitArray bit_array, ref int cursor, int length)
    {
      if (cursor >= length)
        return (PARAM_C1G2Write) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_C1G2Write paramC1G2Write = new PARAM_C1G2Write();
      paramC1G2Write.tvCoding = bit_array[cursor];
      int val;
      if (paramC1G2Write.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        paramC1G2Write.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) paramC1G2Write.length * 8;
      }
      if (val != (int) paramC1G2Write.TypeID)
      {
        cursor = num1;
        return (PARAM_C1G2Write) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 16;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len1);
      paramC1G2Write.OpSpecID = (ushort) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 32;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len2);
      paramC1G2Write.AccessPassword = (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len3 = 2;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (TwoBits), field_len3);
      paramC1G2Write.MB = (TwoBits) obj;
      cursor += 6;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len4 = 16;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len4);
      paramC1G2Write.WordPointer = (ushort) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int fieldLength = Util.DetermineFieldLength(ref bit_array, ref cursor);
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (UInt16Array), fieldLength);
      paramC1G2Write.WriteData = (UInt16Array) obj;
      return paramC1G2Write;
    }

    public override string ToString()
    {
      string str = "<C1G2Write>" + "\r\n";
      int opSpecId = (int) this.OpSpecID;
      try
      {
        str = str + "  <OpSpecID>" + Util.ConvertValueTypeToString((object) this.OpSpecID, "u16", "") + "</OpSpecID>";
        str += "\r\n";
      }
      catch
      {
      }
      int accessPassword = (int) this.AccessPassword;
      try
      {
        str = str + "  <AccessPassword>" + Util.ConvertValueTypeToString((object) this.AccessPassword, "u32", "") + "</AccessPassword>";
        str += "\r\n";
      }
      catch
      {
      }
      if (this.MB != null)
      {
        try
        {
          str = str + "  <MB>" + this.MB.ToString() + "</MB>";
          str += "\r\n";
        }
        catch
        {
        }
      }
      int wordPointer = (int) this.WordPointer;
      try
      {
        str = str + "  <WordPointer>" + Util.ConvertValueTypeToString((object) this.WordPointer, "u16", "") + "</WordPointer>";
        str += "\r\n";
      }
      catch
      {
      }
      if (this.WriteData != null)
      {
        try
        {
          str = str + "  <WriteData>" + Util.ConvertArrayTypeToString((object) this.WriteData, "u16v", "Hex") + "</WriteData>";
          str += "\r\n";
        }
        catch
        {
        }
      }
      return str + "</C1G2Write>" + "\r\n";
    }

    public static PARAM_C1G2Write FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      namespaceManager.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      namespaceManager.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      return new PARAM_C1G2Write()
      {
        OpSpecID = (ushort) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "OpSpecID"), "u16", ""),
        AccessPassword = (uint) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "AccessPassword"), "u32", ""),
        MB = TwoBits.FromString(XmlUtil.GetNodeValue(node, "MB")),
        WordPointer = (ushort) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "WordPointer"), "u16", ""),
        WriteData = (UInt16Array) Util.ParseArrayTypeFromString(XmlUtil.GetNodeValue(node, "WriteData"), "u16v", "Hex")
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
      int opSpecId = (int) this.OpSpecID;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.OpSpecID, (int) this.OpSpecID_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int accessPassword = (int) this.AccessPassword;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.AccessPassword, (int) this.AccessPassword_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      if (this.MB != null)
      {
        try
        {
          BitArray bitArray = Util.ConvertObjToBitArray((object) this.MB, (int) this.MB_len);
          bitArray.CopyTo((Array) bit_array, cursor);
          cursor += bitArray.Length;
        }
        catch
        {
        }
      }
      cursor += 6;
      int wordPointer = (int) this.WordPointer;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.WordPointer, (int) this.WordPointer_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      if (this.WriteData != null)
      {
        try
        {
          Util.ConvertIntToBitArray((uint) this.WriteData.Count, 16).CopyTo((Array) bit_array, cursor);
          cursor += 16;
          BitArray bitArray = Util.ConvertObjToBitArray((object) this.WriteData, (int) this.WriteData_len);
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
      int opSpecId = (int) this.OpSpecID;
      try
      {
        Util.AppendObjToBitArray((object) this.OpSpecID, (int) this.OpSpecID_len, bArr);
      }
      catch
      {
      }
      int accessPassword = (int) this.AccessPassword;
      try
      {
        Util.AppendObjToBitArray((object) this.AccessPassword, (int) this.AccessPassword_len, bArr);
      }
      catch
      {
      }
      if (this.MB != null)
      {
        try
        {
          Util.AppendObjToBitArray((object) this.MB, (int) this.MB_len, bArr);
        }
        catch
        {
        }
      }
      bArr.Length += 6;
      int wordPointer = (int) this.WordPointer;
      try
      {
        Util.AppendObjToBitArray((object) this.WordPointer, (int) this.WordPointer_len, bArr);
      }
      catch
      {
      }
      if (this.WriteData != null)
      {
        try
        {
          Util.AppendIntToBitArray((uint) this.WriteData.Count, 16, bArr);
          Util.AppendObjToBitArray((object) this.WriteData, (int) this.WriteData_len, bArr);
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
