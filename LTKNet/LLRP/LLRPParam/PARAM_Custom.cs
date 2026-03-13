// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_Custom
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_Custom : Parameter, ICustom_Parameter, IParameter
  {
    protected uint VendorIdentifier;
    protected uint ParameterSubtype;
    protected ByteArray Data;
    private short VendorIdentifier_len;
    private short ParameterSubtype_len;
    private short Data_len;

    public PARAM_Custom() => this.typeID = (ushort) 1023;

    public uint VendorID => this.VendorIdentifier;

    public uint SubType => this.ParameterSubtype;

    public override string ToString()
    {
      return "<Custom>\r\n" + "  <VendorIdentifier>" + this.VendorIdentifier.ToString() + "</VendorIdentifier>\r\n" + "  <ParameterSubtype>" + this.ParameterSubtype.ToString() + "</ParameterSubtype>\r\n" + "  <Data>" + this.Data.ToHexString() + "</Data>\r\n" + "</Custom>\r\n";
    }

    public override void AppendToBitArray(AutoGrowingBitArray bArr)
    {
      int length = bArr.Length;
      bArr.Length += 6;
      Util.AppendIntToBitArray((uint) this.typeID, 10, bArr);
      bArr.Length += 16;
      try
      {
        Util.AppendObjToBitArray((object) this.VendorIdentifier, 32, bArr);
      }
      catch
      {
      }
      try
      {
        Util.AppendObjToBitArray((object) this.ParameterSubtype, 32, bArr);
      }
      catch
      {
      }
      try
      {
        Util.AppendObjToBitArray((object) this.Data, this.Data.Count * 8, bArr);
      }
      catch
      {
      }
      BitArray bitArray = Util.ConvertIntToBitArray((uint) (bArr.Length - length) / 8U, 16);
      for (int index = 0; index < bitArray.Length; ++index)
        bArr[length + 16 + index] = bitArray[index];
    }

    public override void ToBitArray(ref bool[] bit_array, ref int cursor)
    {
      int num = cursor;
      cursor += 6;
      Util.ConvertIntToBitArray((uint) this.typeID, 10).CopyTo((Array) bit_array, cursor);
      cursor += 26;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.VendorIdentifier, 32);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.ParameterSubtype, 32);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.Data, this.Data.Count * 8);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      Util.ConvertIntToBitArray((uint) (cursor - num) / 8U, 16).CopyTo((Array) bit_array, num + 16);
    }

    public static PARAM_Custom FromBitArray(ref BitArray bit_array, ref int cursor, int length)
    {
      if (cursor >= length)
        return (PARAM_Custom) null;
      PARAM_Custom paramCustom = new PARAM_Custom();
      int num = cursor;
      cursor += 6;
      if ((int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10) != (int) paramCustom.TypeID)
      {
        cursor = num;
        return (PARAM_Custom) null;
      }
      paramCustom.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
      if (cursor > length)
        throw new Exception("Input data is not complete message");
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), 32);
      paramCustom.VendorIdentifier = (uint) obj;
      if (cursor > length)
        throw new Exception("Input data is not complete message");
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), 32);
      paramCustom.ParameterSubtype = (uint) obj;
      if (cursor > length)
        throw new Exception("Input data is not complete message");
      int field_len = ((int) paramCustom.length * 8 - (cursor - num)) / 8;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ByteArray), field_len);
      paramCustom.Data = (ByteArray) obj;
      return paramCustom;
    }

    public static PARAM_Custom FromXmlNode(XmlNode node)
    {
      return new PARAM_Custom()
      {
        VendorIdentifier = Convert.ToUInt32(XmlUtil.GetNodeValue(node, "VendorIdentifier")),
        ParameterSubtype = Convert.ToUInt32(XmlUtil.GetNodeValue(node, "ParameterSubtype")),
        Data = (ByteArray) Util.ParseArrayTypeFromString(XmlUtil.GetNodeValue(node, "Data"), "bytesToEnd", "Hex")
      };
    }
  }
}
