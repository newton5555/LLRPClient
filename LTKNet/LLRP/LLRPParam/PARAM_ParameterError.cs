// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_ParameterError
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_ParameterError : Parameter
  {
    public ushort ParameterType;
    private short ParameterType_len;
    public ENUM_StatusCode ErrorCode;
    private short ErrorCode_len = 16;
    public PARAM_FieldError FieldError;
    public PARAM_ParameterError ParameterError;

    public PARAM_ParameterError() => this.typeID = (ushort) 289;

    public static PARAM_ParameterError FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_ParameterError) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_ParameterError paramParameterError = new PARAM_ParameterError();
      paramParameterError.tvCoding = bit_array[cursor];
      int val;
      if (paramParameterError.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        paramParameterError.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) paramParameterError.length * 8;
      }
      if (val != (int) paramParameterError.TypeID)
      {
        cursor = num1;
        return (PARAM_ParameterError) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 16;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len1);
      paramParameterError.ParameterType = (ushort) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 16;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len2);
      paramParameterError.ErrorCode = (ENUM_StatusCode) (uint) obj;
      paramParameterError.FieldError = PARAM_FieldError.FromBitArray(ref bit_array, ref cursor, length);
      paramParameterError.ParameterError = PARAM_ParameterError.FromBitArray(ref bit_array, ref cursor, length);
      return paramParameterError;
    }

    public override string ToString()
    {
      string str = "<ParameterError>" + "\r\n";
      int parameterType = (int) this.ParameterType;
      try
      {
        str = str + "  <ParameterType>" + Util.ConvertValueTypeToString((object) this.ParameterType, "u16", "") + "</ParameterType>";
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
      if (this.FieldError != null)
        str += Util.Indent(this.FieldError.ToString());
      if (this.ParameterError != null)
        str += Util.Indent(this.ParameterError.ToString());
      return str + "</ParameterError>" + "\r\n";
    }

    public static PARAM_ParameterError FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      nsmgr.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      nsmgr.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      PARAM_ParameterError paramParameterError = new PARAM_ParameterError();
      string nodeValue1 = XmlUtil.GetNodeValue(node, "ParameterType");
      paramParameterError.ParameterType = (ushort) Util.ParseValueTypeFromString(nodeValue1, "u16", "");
      string nodeValue2 = XmlUtil.GetNodeValue(node, "ErrorCode");
      paramParameterError.ErrorCode = (ENUM_StatusCode) Enum.Parse(typeof (ENUM_StatusCode), nodeValue2);
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "FieldError", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            paramParameterError.FieldError = PARAM_FieldError.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "ParameterError", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            paramParameterError.ParameterError = PARAM_ParameterError.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      return paramParameterError;
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
      int parameterType = (int) this.ParameterType;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.ParameterType, (int) this.ParameterType_len);
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
      if (this.FieldError != null)
        this.FieldError.ToBitArray(ref bit_array, ref cursor);
      if (this.ParameterError != null)
        this.ParameterError.ToBitArray(ref bit_array, ref cursor);
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
      int parameterType = (int) this.ParameterType;
      try
      {
        Util.AppendObjToBitArray((object) this.ParameterType, (int) this.ParameterType_len, bArr);
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
      if (this.FieldError != null)
        this.FieldError.AppendToBitArray(bArr);
      if (this.ParameterError != null)
        this.ParameterError.AppendToBitArray(bArr);
      if (this.tvCoding)
        return;
      BitArray bitArray = Util.ConvertIntToBitArray((uint) (bArr.Length - length1) / 8U, 16);
      for (int index = 0; index < bitArray.Length; ++index)
        bArr[length1 + 16 + index] = bitArray[index];
    }
  }
}
