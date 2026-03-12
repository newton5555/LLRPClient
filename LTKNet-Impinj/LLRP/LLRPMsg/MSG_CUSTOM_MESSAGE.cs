// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.MSG_CUSTOM_MESSAGE
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;

#nullable disable
namespace Org.LLRP.LTK.LLRPV1
{
  public class MSG_CUSTOM_MESSAGE : Message
  {
    protected uint VendorIdentifier;
    protected byte MessageSubtype;
    protected ByteArray Data;
    private short VendorIdentifier_len;
    private short MessageSubtype_len;
    private short Data_len;

    public MSG_CUSTOM_MESSAGE()
    {
      this.msgType = (ushort) 1023;
      this.MSG_ID = MessageID.getNewMessageID();
    }

    public uint VendorID
    {
      get => this.VendorIdentifier;
      set => this.VendorIdentifier = value;
    }

    public byte SubType
    {
      get => this.MessageSubtype;
      set => this.MessageSubtype = value;
    }

    public override string ToString()
    {
      string str = "<CUSTOM_MESSAGE" + string.Format(" xmlns=\"{0}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0") + string.Format(" xmlns:llrp=\"{0}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0") + " xmlns:xsi= \"http://www.w3.org/2001/XMLSchema-instance\"\n" + string.Format(" xsi:schemaLocation=\"{0} {1}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0/llrp.xsd") + " Version=\"" + this.version.ToString() + "\" MessageID=\"" + this.MSG_ID.ToString() + "\">\r\n";
      try
      {
        str += "  <VendorIdentifier>";
        str += Util.ConvertValueTypeToString((object) this.VendorIdentifier, "u32", "");
        str += "</VendorIdentifier>\r\n";
      }
      catch
      {
      }
      try
      {
        str += "  <MessageSubtype>";
        str += Util.ConvertValueTypeToString((object) this.MessageSubtype, "u8", "");
        str += "</MessageSubtype>\r\n";
      }
      catch
      {
      }
      if (this.Data != null)
      {
        try
        {
          str += "  <Data>";
          str += Util.ConvertArrayTypeToString((object) this.Data, "bytesToEnd", "Hex");
          str += "</Data>\r\n";
        }
        catch
        {
        }
      }
      return str + "</CUSTOM_MESSAGE>";
    }

    public static MSG_CUSTOM_MESSAGE FromString(string str)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.LoadXml(str);
      XmlNode documentElement = (XmlNode) xmlDocument.DocumentElement;
      MSG_CUSTOM_MESSAGE msgCustomMessage = new MSG_CUSTOM_MESSAGE();
      try
      {
        msgCustomMessage.MSG_ID = Convert.ToUInt32(XmlUtil.GetNodeAttrValue(documentElement, "MessageID"));
      }
      catch
      {
      }
      string nodeValue1 = XmlUtil.GetNodeValue(documentElement, "VendorIdentifier");
      msgCustomMessage.VendorIdentifier = (uint) Util.ParseValueTypeFromString(nodeValue1, "u32", "");
      string nodeValue2 = XmlUtil.GetNodeValue(documentElement, "MessageSubtype");
      msgCustomMessage.MessageSubtype = (byte) Util.ParseValueTypeFromString(nodeValue2, "u8", "");
      string nodeValue3 = XmlUtil.GetNodeValue(documentElement, "Data");
      msgCustomMessage.Data = (ByteArray) Util.ParseArrayTypeFromString(nodeValue3, "bytesToEnd", "Hex");
      return msgCustomMessage;
    }

    public override bool[] ToBitArray()
    {
      AutoGrowingBitArray bit_arr = new AutoGrowingBitArray(10000);
      Util.AppendIntToBitArray(0U, 3, bit_arr);
      Util.AppendIntToBitArray((uint) this.version, 3, bit_arr);
      Util.AppendIntToBitArray((uint) this.msgType, 10, bit_arr);
      Util.AppendIntToBitArray(this.msgLen, 32, bit_arr);
      Util.AppendIntToBitArray(this.msgID, 32, bit_arr);
      try
      {
        Util.AppendObjToBitArray((object) this.VendorIdentifier, (int) this.VendorIdentifier_len, bit_arr);
      }
      catch
      {
      }
      try
      {
        Util.AppendObjToBitArray((object) this.MessageSubtype, (int) this.MessageSubtype_len, bit_arr);
      }
      catch
      {
      }
      if (this.Data != null)
      {
        try
        {
          Util.AppendObjToBitArray((object) this.Data, (int) this.Data_len, bit_arr);
        }
        catch
        {
        }
      }
      int val = (int) ((uint) bit_arr.Length / 8U);
      bool[] bitArray = new bool[bit_arr.Length];
      bit_arr.CopyTo((Array) bitArray, 0);
      Util.ConvertIntToBitArray((uint) val, 32).CopyTo((Array) bitArray, 16);
      return bitArray;
    }

    public static MSG_CUSTOM_MESSAGE FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor > length)
        return (MSG_CUSTOM_MESSAGE) null;
      ArrayList arrayList = new ArrayList();
      MSG_CUSTOM_MESSAGE msgCustomMessage = new MSG_CUSTOM_MESSAGE();
      cursor += 6;
      if ((int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10) != (int) msgCustomMessage.msgType)
      {
        cursor -= 16;
        return (MSG_CUSTOM_MESSAGE) null;
      }
      msgCustomMessage.msgLen = (uint) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 32);
      msgCustomMessage.msgID = (uint) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 32);
      if (cursor > length)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 32;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len1);
      msgCustomMessage.VendorIdentifier = (uint) obj;
      if (cursor > length)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 8;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (byte), field_len2);
      msgCustomMessage.MessageSubtype = (byte) obj;
      if (cursor > length)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len3 = (bit_array.Length - cursor) / 8;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ByteArray), field_len3);
      msgCustomMessage.Data = (ByteArray) obj;
      return msgCustomMessage;
    }
  }
}
