// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_Identification
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_Identification : Parameter
  {
    public ENUM_IdentificationType IDType;
    private short IDType_len = 8;
    public ByteArray ReaderID = new ByteArray();
    private short ReaderID_len;

    public PARAM_Identification() => this.typeID = (ushort) 218;

    public static PARAM_Identification FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_Identification) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_Identification paramIdentification = new PARAM_Identification();
      paramIdentification.tvCoding = bit_array[cursor];
      int val;
      if (paramIdentification.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        paramIdentification.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) paramIdentification.length * 8;
      }
      if (val != (int) paramIdentification.TypeID)
      {
        cursor = num1;
        return (PARAM_Identification) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len = 8;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len);
      paramIdentification.IDType = (ENUM_IdentificationType) (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int fieldLength = Util.DetermineFieldLength(ref bit_array, ref cursor);
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ByteArray), fieldLength);
      paramIdentification.ReaderID = (ByteArray) obj;
      return paramIdentification;
    }

    public override string ToString()
    {
      string str = "<Identification>" + "\r\n";
      int idType = (int) this.IDType;
      try
      {
        str = str + "  <IDType>" + this.IDType.ToString() + "</IDType>";
        str += "\r\n";
      }
      catch
      {
      }
      if (this.ReaderID != null)
      {
        try
        {
          str = str + "  <ReaderID>" + Util.ConvertArrayTypeToString((object) this.ReaderID, "u8v", "Hex") + "</ReaderID>";
          str += "\r\n";
        }
        catch
        {
        }
      }
      return str + "</Identification>" + "\r\n";
    }

    public static PARAM_Identification FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      namespaceManager.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      namespaceManager.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      return new PARAM_Identification()
      {
        IDType = (ENUM_IdentificationType) Enum.Parse(typeof (ENUM_IdentificationType), XmlUtil.GetNodeValue(node, "IDType")),
        ReaderID = (ByteArray) Util.ParseArrayTypeFromString(XmlUtil.GetNodeValue(node, "ReaderID"), "u8v", "Hex")
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
      int idType = (int) this.IDType;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.IDType, (int) this.IDType_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      if (this.ReaderID != null)
      {
        try
        {
          Util.ConvertIntToBitArray((uint) this.ReaderID.Count, 16).CopyTo((Array) bit_array, cursor);
          cursor += 16;
          BitArray bitArray = Util.ConvertObjToBitArray((object) this.ReaderID, (int) this.ReaderID_len);
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
      int idType = (int) this.IDType;
      try
      {
        Util.AppendObjToBitArray((object) this.IDType, (int) this.IDType_len, bArr);
      }
      catch
      {
      }
      if (this.ReaderID != null)
      {
        try
        {
          Util.AppendIntToBitArray((uint) this.ReaderID.Count, 16, bArr);
          Util.AppendObjToBitArray((object) this.ReaderID, (int) this.ReaderID_len, bArr);
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
