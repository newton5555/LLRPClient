// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_C1G2TargetTag
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_C1G2TargetTag : Parameter
  {
    public TwoBits MB = new TwoBits((ushort) 0);
    private short MB_len;
    public bool Match;
    private short Match_len;
    private const ushort param_reserved_len4 = 5;
    public ushort Pointer;
    private short Pointer_len;
    public LLRPBitArray TagMask = new LLRPBitArray();
    private short TagMask_len;
    public LLRPBitArray TagData = new LLRPBitArray();
    private short TagData_len;

    public PARAM_C1G2TargetTag() => this.typeID = (ushort) 339;

    public static PARAM_C1G2TargetTag FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_C1G2TargetTag) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_C1G2TargetTag paramC1G2TargetTag = new PARAM_C1G2TargetTag();
      paramC1G2TargetTag.tvCoding = bit_array[cursor];
      int val;
      if (paramC1G2TargetTag.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        paramC1G2TargetTag.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) paramC1G2TargetTag.length * 8;
      }
      if (val != (int) paramC1G2TargetTag.TypeID)
      {
        cursor = num1;
        return (PARAM_C1G2TargetTag) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 2;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (TwoBits), field_len1);
      paramC1G2TargetTag.MB = (TwoBits) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 1;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (bool), field_len2);
      paramC1G2TargetTag.Match = (bool) obj;
      cursor += 5;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len3 = 16;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len3);
      paramC1G2TargetTag.Pointer = (ushort) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int fieldLength1 = Util.DetermineFieldLength(ref bit_array, ref cursor);
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (LLRPBitArray), fieldLength1);
      paramC1G2TargetTag.TagMask = (LLRPBitArray) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int fieldLength2 = Util.DetermineFieldLength(ref bit_array, ref cursor);
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (LLRPBitArray), fieldLength2);
      paramC1G2TargetTag.TagData = (LLRPBitArray) obj;
      return paramC1G2TargetTag;
    }

    public override string ToString()
    {
      string str = "<C1G2TargetTag>" + "\r\n";
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
      int num = this.Match ? 1 : 0;
      try
      {
        str = str + "  <Match>" + Util.ConvertValueTypeToString((object) this.Match, "u1", "") + "</Match>";
        str += "\r\n";
      }
      catch
      {
      }
      int pointer = (int) this.Pointer;
      try
      {
        str = str + "  <Pointer>" + Util.ConvertValueTypeToString((object) this.Pointer, "u16", "") + "</Pointer>";
        str += "\r\n";
      }
      catch
      {
      }
      if (this.TagMask != null)
      {
        try
        {
          str = str + "  <TagMask Count=\"" + this.TagMask.Count.ToString() + "\">" + Util.ConvertArrayTypeToString((object) this.TagMask, "u1v", "Hex") + "</TagMask>";
          str += "\r\n";
        }
        catch
        {
        }
      }
      if (this.TagData != null)
      {
        try
        {
          str = str + "  <TagData Count=\"" + this.TagData.Count.ToString() + "\">" + Util.ConvertArrayTypeToString((object) this.TagData, "u1v", "Hex") + "</TagData>";
          str += "\r\n";
        }
        catch
        {
        }
      }
      return str + "</C1G2TargetTag>" + "\r\n";
    }

    public static PARAM_C1G2TargetTag FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      namespaceManager.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      namespaceManager.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      PARAM_C1G2TargetTag paramC1G2TargetTag = new PARAM_C1G2TargetTag();
      string nodeValue1 = XmlUtil.GetNodeValue(node, "MB");
      paramC1G2TargetTag.MB = TwoBits.FromString(nodeValue1);
      string nodeValue2 = XmlUtil.GetNodeValue(node, "Match");
      paramC1G2TargetTag.Match = (bool) Util.ParseValueTypeFromString(nodeValue2, "u1", "");
      string nodeValue3 = XmlUtil.GetNodeValue(node, "Pointer");
      paramC1G2TargetTag.Pointer = (ushort) Util.ParseValueTypeFromString(nodeValue3, "u16", "");
      string nodeValue4 = XmlUtil.GetNodeValue(node, "TagMask");
      paramC1G2TargetTag.TagMask = (LLRPBitArray) Util.ParseArrayTypeFromString(nodeValue4, "u1v", "Hex");
      string nodeAttribute1 = XmlUtil.GetNodeAttribute(node, "TagMask", "Count");
      if (nodeAttribute1 != string.Empty)
        paramC1G2TargetTag.TagMask.Count = Convert.ToInt32(nodeAttribute1);
      string nodeValue5 = XmlUtil.GetNodeValue(node, "TagData");
      paramC1G2TargetTag.TagData = (LLRPBitArray) Util.ParseArrayTypeFromString(nodeValue5, "u1v", "Hex");
      string nodeAttribute2 = XmlUtil.GetNodeAttribute(node, "TagData", "Count");
      if (nodeAttribute2 != string.Empty)
        paramC1G2TargetTag.TagData.Count = Convert.ToInt32(nodeAttribute2);
      return paramC1G2TargetTag;
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
      int num2 = this.Match ? 1 : 0;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.Match, (int) this.Match_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      cursor += 5;
      int pointer = (int) this.Pointer;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.Pointer, (int) this.Pointer_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      if (this.TagMask != null)
      {
        try
        {
          Util.ConvertIntToBitArray((uint) this.TagMask.Count, 16).CopyTo((Array) bit_array, cursor);
          cursor += 16;
          BitArray bitArray = Util.ConvertObjToBitArray((object) this.TagMask, (int) this.TagMask_len);
          bitArray.CopyTo((Array) bit_array, cursor);
          cursor += bitArray.Length;
        }
        catch
        {
        }
      }
      if (this.TagData != null)
      {
        try
        {
          Util.ConvertIntToBitArray((uint) this.TagData.Count, 16).CopyTo((Array) bit_array, cursor);
          cursor += 16;
          BitArray bitArray = Util.ConvertObjToBitArray((object) this.TagData, (int) this.TagData_len);
          bitArray.CopyTo((Array) bit_array, cursor);
          cursor += bitArray.Length;
        }
        catch
        {
        }
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
      int num = this.Match ? 1 : 0;
      try
      {
        Util.AppendObjToBitArray((object) this.Match, (int) this.Match_len, bArr);
      }
      catch
      {
      }
      bArr.Length += 5;
      int pointer = (int) this.Pointer;
      try
      {
        Util.AppendObjToBitArray((object) this.Pointer, (int) this.Pointer_len, bArr);
      }
      catch
      {
      }
      if (this.TagMask != null)
      {
        try
        {
          Util.AppendIntToBitArray((uint) this.TagMask.Count, 16, bArr);
          Util.AppendObjToBitArray((object) this.TagMask, (int) this.TagMask_len, bArr);
        }
        catch
        {
        }
      }
      if (this.TagData != null)
      {
        try
        {
          Util.AppendIntToBitArray((uint) this.TagData.Count, 16, bArr);
          Util.AppendObjToBitArray((object) this.TagData, (int) this.TagData_len, bArr);
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
