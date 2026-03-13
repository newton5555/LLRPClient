// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_FieldError
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_FieldError : Parameter
  {
    public ushort FieldNum;
    private short FieldNum_len;
    public ENUM_StatusCode ErrorCode;
    private short ErrorCode_len = 16;

    public PARAM_FieldError() => this.typeID = (ushort) 288;

    public static PARAM_FieldError FromBitArray(ref BitArray bit_array, ref int cursor, int length)
    {
      if (cursor >= length)
        return (PARAM_FieldError) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_FieldError paramFieldError = new PARAM_FieldError();
      paramFieldError.tvCoding = bit_array[cursor];
      int val;
      if (paramFieldError.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        paramFieldError.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) paramFieldError.length * 8;
      }
      if (val != (int) paramFieldError.TypeID)
      {
        cursor = num1;
        return (PARAM_FieldError) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 16;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len1);
      paramFieldError.FieldNum = (ushort) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 16;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len2);
      paramFieldError.ErrorCode = (ENUM_StatusCode) (uint) obj;
      return paramFieldError;
    }

    public override string ToString()
    {
      string str = "<FieldError>" + "\r\n";
      int fieldNum = (int) this.FieldNum;
      try
      {
        str = str + "  <FieldNum>" + Util.ConvertValueTypeToString((object) this.FieldNum, "u16", "") + "</FieldNum>";
        str += "\r\n";
      }
      catch
      {
      }
      int errorCode = (int) this.ErrorCode;
      try
      {
        str = str + "  <ErrorCode>" + this.ErrorCode.ToString() + "</ErrorCode>";
        str += "\r\n";
      }
      catch
      {
      }
      return str + "</FieldError>" + "\r\n";
    }

    public static PARAM_FieldError FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      namespaceManager.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      namespaceManager.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      return new PARAM_FieldError()
      {
        FieldNum = (ushort) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "FieldNum"), "u16", ""),
        ErrorCode = (ENUM_StatusCode) Enum.Parse(typeof (ENUM_StatusCode), XmlUtil.GetNodeValue(node, "ErrorCode"))
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
      int fieldNum = (int) this.FieldNum;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.FieldNum, (int) this.FieldNum_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int errorCode = (int) this.ErrorCode;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.ErrorCode, (int) this.ErrorCode_len);
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
      int fieldNum = (int) this.FieldNum;
      try
      {
        Util.AppendObjToBitArray((object) this.FieldNum, (int) this.FieldNum_len, bArr);
      }
      catch
      {
      }
      int errorCode = (int) this.ErrorCode;
      try
      {
        Util.AppendObjToBitArray((object) this.ErrorCode, (int) this.ErrorCode_len, bArr);
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
