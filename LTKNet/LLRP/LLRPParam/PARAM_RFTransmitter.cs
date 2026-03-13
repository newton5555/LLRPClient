// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_RFTransmitter
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_RFTransmitter : Parameter
  {
    public ushort HopTableID;
    private short HopTableID_len;
    public ushort ChannelIndex;
    private short ChannelIndex_len;
    public ushort TransmitPower;
    private short TransmitPower_len;

    public PARAM_RFTransmitter() => this.typeID = (ushort) 224;

    public static PARAM_RFTransmitter FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_RFTransmitter) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_RFTransmitter paramRfTransmitter = new PARAM_RFTransmitter();
      paramRfTransmitter.tvCoding = bit_array[cursor];
      int val;
      if (paramRfTransmitter.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        paramRfTransmitter.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) paramRfTransmitter.length * 8;
      }
      if (val != (int) paramRfTransmitter.TypeID)
      {
        cursor = num1;
        return (PARAM_RFTransmitter) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 16;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len1);
      paramRfTransmitter.HopTableID = (ushort) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 16;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len2);
      paramRfTransmitter.ChannelIndex = (ushort) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len3 = 16;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len3);
      paramRfTransmitter.TransmitPower = (ushort) obj;
      return paramRfTransmitter;
    }

    public override string ToString()
    {
      string str = "<RFTransmitter>" + "\r\n";
      int hopTableId = (int) this.HopTableID;
      try
      {
        str = str + "  <HopTableID>" + Util.ConvertValueTypeToString((object) this.HopTableID, "u16", "") + "</HopTableID>";
        str += "\r\n";
      }
      catch
      {
      }
      int channelIndex = (int) this.ChannelIndex;
      try
      {
        str = str + "  <ChannelIndex>" + Util.ConvertValueTypeToString((object) this.ChannelIndex, "u16", "") + "</ChannelIndex>";
        str += "\r\n";
      }
      catch
      {
      }
      int transmitPower = (int) this.TransmitPower;
      try
      {
        str = str + "  <TransmitPower>" + Util.ConvertValueTypeToString((object) this.TransmitPower, "u16", "") + "</TransmitPower>";
        str += "\r\n";
      }
      catch
      {
      }
      return str + "</RFTransmitter>" + "\r\n";
    }

    public static PARAM_RFTransmitter FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      namespaceManager.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      namespaceManager.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      return new PARAM_RFTransmitter()
      {
        HopTableID = (ushort) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "HopTableID"), "u16", ""),
        ChannelIndex = (ushort) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "ChannelIndex"), "u16", ""),
        TransmitPower = (ushort) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "TransmitPower"), "u16", "")
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
      int hopTableId = (int) this.HopTableID;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.HopTableID, (int) this.HopTableID_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int channelIndex = (int) this.ChannelIndex;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.ChannelIndex, (int) this.ChannelIndex_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int transmitPower = (int) this.TransmitPower;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.TransmitPower, (int) this.TransmitPower_len);
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
      int hopTableId = (int) this.HopTableID;
      try
      {
        Util.AppendObjToBitArray((object) this.HopTableID, (int) this.HopTableID_len, bArr);
      }
      catch
      {
      }
      int channelIndex = (int) this.ChannelIndex;
      try
      {
        Util.AppendObjToBitArray((object) this.ChannelIndex, (int) this.ChannelIndex_len, bArr);
      }
      catch
      {
      }
      int transmitPower = (int) this.TransmitPower;
      try
      {
        Util.AppendObjToBitArray((object) this.TransmitPower, (int) this.TransmitPower_len, bArr);
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
