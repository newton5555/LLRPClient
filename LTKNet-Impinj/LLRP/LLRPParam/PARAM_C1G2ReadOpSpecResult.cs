// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_C1G2ReadOpSpecResult
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_C1G2ReadOpSpecResult : Parameter
  {
    public ENUM_C1G2ReadResultType Result;
    private short Result_len = 8;
    public ushort OpSpecID;
    private short OpSpecID_len;
    public UInt16Array ReadData = new UInt16Array();
    private short ReadData_len;

    public PARAM_C1G2ReadOpSpecResult() => this.typeID = (ushort) 349;

    public static PARAM_C1G2ReadOpSpecResult FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_C1G2ReadOpSpecResult) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_C1G2ReadOpSpecResult readOpSpecResult = new PARAM_C1G2ReadOpSpecResult();
      readOpSpecResult.tvCoding = bit_array[cursor];
      int val;
      if (readOpSpecResult.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        readOpSpecResult.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) readOpSpecResult.length * 8;
      }
      if (val != (int) readOpSpecResult.TypeID)
      {
        cursor = num1;
        return (PARAM_C1G2ReadOpSpecResult) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 8;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len1);
      readOpSpecResult.Result = (ENUM_C1G2ReadResultType) (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 16;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len2);
      readOpSpecResult.OpSpecID = (ushort) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int fieldLength = Util.DetermineFieldLength(ref bit_array, ref cursor);
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (UInt16Array), fieldLength);
      readOpSpecResult.ReadData = (UInt16Array) obj;
      return readOpSpecResult;
    }

    public override string ToString()
    {
      string str = "<C1G2ReadOpSpecResult>" + "\r\n";
      int result = (int) this.Result;
      try
      {
        str = str + "  <Result>" + this.Result.ToString() + "</Result>";
        str += "\r\n";
      }
      catch
      {
      }
      int opSpecId = (int) this.OpSpecID;
      try
      {
        str = str + "  <OpSpecID>" + Util.ConvertValueTypeToString((object) this.OpSpecID, "u16", "") + "</OpSpecID>";
        str += "\r\n";
      }
      catch
      {
      }
      if (this.ReadData != null)
      {
        try
        {
          str = str + "  <ReadData>" + Util.ConvertArrayTypeToString((object) this.ReadData, "u16v", "Hex") + "</ReadData>";
          str += "\r\n";
        }
        catch
        {
        }
      }
      return str + "</C1G2ReadOpSpecResult>" + "\r\n";
    }

    public static PARAM_C1G2ReadOpSpecResult FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      namespaceManager.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      namespaceManager.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      return new PARAM_C1G2ReadOpSpecResult()
      {
        Result = (ENUM_C1G2ReadResultType) Enum.Parse(typeof (ENUM_C1G2ReadResultType), XmlUtil.GetNodeValue(node, "Result")),
        OpSpecID = (ushort) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "OpSpecID"), "u16", ""),
        ReadData = (UInt16Array) Util.ParseArrayTypeFromString(XmlUtil.GetNodeValue(node, "ReadData"), "u16v", "Hex")
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
      int result = (int) this.Result;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.Result, (int) this.Result_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
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
      if (this.ReadData != null)
      {
        try
        {
          Util.ConvertIntToBitArray((uint) this.ReadData.Count, 16).CopyTo((Array) bit_array, cursor);
          cursor += 16;
          BitArray bitArray = Util.ConvertObjToBitArray((object) this.ReadData, (int) this.ReadData_len);
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
      int result = (int) this.Result;
      try
      {
        Util.AppendObjToBitArray((object) this.Result, (int) this.Result_len, bArr);
      }
      catch
      {
      }
      int opSpecId = (int) this.OpSpecID;
      try
      {
        Util.AppendObjToBitArray((object) this.OpSpecID, (int) this.OpSpecID_len, bArr);
      }
      catch
      {
      }
      if (this.ReadData != null)
      {
        try
        {
          Util.AppendIntToBitArray((uint) this.ReadData.Count, 16, bArr);
          Util.AppendObjToBitArray((object) this.ReadData, (int) this.ReadData_len, bArr);
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
