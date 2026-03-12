// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_AccessSpecID
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_AccessSpecID : Parameter
  {
    public uint AccessSpecID;
    private short AccessSpecID_len;

    public PARAM_AccessSpecID()
    {
      this.typeID = (ushort) 16;
      this.tvCoding = true;
    }

    public static PARAM_AccessSpecID FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_AccessSpecID) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_AccessSpecID paramAccessSpecId = new PARAM_AccessSpecID();
      paramAccessSpecId.tvCoding = bit_array[cursor];
      int val;
      if (paramAccessSpecId.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        paramAccessSpecId.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) paramAccessSpecId.length * 8;
      }
      if (val != (int) paramAccessSpecId.TypeID)
      {
        cursor = num1;
        return (PARAM_AccessSpecID) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len = 32;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len);
      paramAccessSpecId.AccessSpecID = (uint) obj;
      return paramAccessSpecId;
    }

    public override string ToString()
    {
      string str = "<AccessSpecID>" + "\r\n";
      int accessSpecId = (int) this.AccessSpecID;
      try
      {
        str = str + "  <AccessSpecID>" + Util.ConvertValueTypeToString((object) this.AccessSpecID, "u32", "") + "</AccessSpecID>";
        str += "\r\n";
      }
      catch
      {
      }
      return str + "</AccessSpecID>" + "\r\n";
    }

    public static PARAM_AccessSpecID FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      namespaceManager.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      namespaceManager.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      return new PARAM_AccessSpecID()
      {
        AccessSpecID = (uint) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "AccessSpecID"), "u32", "")
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
      int accessSpecId = (int) this.AccessSpecID;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.AccessSpecID, (int) this.AccessSpecID_len);
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
      int accessSpecId = (int) this.AccessSpecID;
      try
      {
        Util.AppendObjToBitArray((object) this.AccessSpecID, (int) this.AccessSpecID_len, bArr);
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
