// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_C1G2BlockWriteOpSpecResult
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_C1G2BlockWriteOpSpecResult : Parameter
  {
    public ENUM_C1G2BlockWriteResultType Result;
    private short Result_len = 8;
    public ushort OpSpecID;
    private short OpSpecID_len;
    public ushort NumWordsWritten;
    private short NumWordsWritten_len;

    public PARAM_C1G2BlockWriteOpSpecResult() => this.typeID = (ushort) 354;

    public static PARAM_C1G2BlockWriteOpSpecResult FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_C1G2BlockWriteOpSpecResult) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_C1G2BlockWriteOpSpecResult writeOpSpecResult = new PARAM_C1G2BlockWriteOpSpecResult();
      writeOpSpecResult.tvCoding = bit_array[cursor];
      int val;
      if (writeOpSpecResult.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        writeOpSpecResult.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) writeOpSpecResult.length * 8;
      }
      if (val != (int) writeOpSpecResult.TypeID)
      {
        cursor = num1;
        return (PARAM_C1G2BlockWriteOpSpecResult) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 8;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len1);
      writeOpSpecResult.Result = (ENUM_C1G2BlockWriteResultType) (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 16;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len2);
      writeOpSpecResult.OpSpecID = (ushort) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len3 = 16;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len3);
      writeOpSpecResult.NumWordsWritten = (ushort) obj;
      return writeOpSpecResult;
    }

    public override string ToString()
    {
      string str = "<C1G2BlockWriteOpSpecResult>" + "\r\n";
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
      int numWordsWritten = (int) this.NumWordsWritten;
      try
      {
        str = str + "  <NumWordsWritten>" + Util.ConvertValueTypeToString((object) this.NumWordsWritten, "u16", "") + "</NumWordsWritten>";
        str += "\r\n";
      }
      catch
      {
      }
      return str + "</C1G2BlockWriteOpSpecResult>" + "\r\n";
    }

    public static PARAM_C1G2BlockWriteOpSpecResult FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      namespaceManager.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      namespaceManager.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      return new PARAM_C1G2BlockWriteOpSpecResult()
      {
        Result = (ENUM_C1G2BlockWriteResultType) Enum.Parse(typeof (ENUM_C1G2BlockWriteResultType), XmlUtil.GetNodeValue(node, "Result")),
        OpSpecID = (ushort) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "OpSpecID"), "u16", ""),
        NumWordsWritten = (ushort) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "NumWordsWritten"), "u16", "")
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
      int numWordsWritten = (int) this.NumWordsWritten;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.NumWordsWritten, (int) this.NumWordsWritten_len);
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
      int numWordsWritten = (int) this.NumWordsWritten;
      try
      {
        Util.AppendObjToBitArray((object) this.NumWordsWritten, (int) this.NumWordsWritten_len, bArr);
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
