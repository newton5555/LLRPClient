// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_AntennaEvent
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_AntennaEvent : Parameter
  {
    public ENUM_AntennaEventType EventType;
    private short EventType_len = 8;
    public ushort AntennaID;
    private short AntennaID_len;

    public PARAM_AntennaEvent() => this.typeID = (ushort) byte.MaxValue;

    public static PARAM_AntennaEvent FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_AntennaEvent) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_AntennaEvent paramAntennaEvent = new PARAM_AntennaEvent();
      paramAntennaEvent.tvCoding = bit_array[cursor];
      int val;
      if (paramAntennaEvent.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        paramAntennaEvent.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) paramAntennaEvent.length * 8;
      }
      if (val != (int) paramAntennaEvent.TypeID)
      {
        cursor = num1;
        return (PARAM_AntennaEvent) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 8;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len1);
      paramAntennaEvent.EventType = (ENUM_AntennaEventType) (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 16;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len2);
      paramAntennaEvent.AntennaID = (ushort) obj;
      return paramAntennaEvent;
    }

    public override string ToString()
    {
      string str = "<AntennaEvent>" + "\r\n";
      int eventType = (int) this.EventType;
      try
      {
        str = str + "  <EventType>" + this.EventType.ToString() + "</EventType>";
        str += "\r\n";
      }
      catch
      {
      }
      int antennaId = (int) this.AntennaID;
      try
      {
        str = str + "  <AntennaID>" + Util.ConvertValueTypeToString((object) this.AntennaID, "u16", "") + "</AntennaID>";
        str += "\r\n";
      }
      catch
      {
      }
      return str + "</AntennaEvent>" + "\r\n";
    }

    public static PARAM_AntennaEvent FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      namespaceManager.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      namespaceManager.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      return new PARAM_AntennaEvent()
      {
        EventType = (ENUM_AntennaEventType) Enum.Parse(typeof (ENUM_AntennaEventType), XmlUtil.GetNodeValue(node, "EventType")),
        AntennaID = (ushort) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "AntennaID"), "u16", "")
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
      int eventType = (int) this.EventType;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.EventType, (int) this.EventType_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int antennaId = (int) this.AntennaID;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.AntennaID, (int) this.AntennaID_len);
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
      int eventType = (int) this.EventType;
      try
      {
        Util.AppendObjToBitArray((object) this.EventType, (int) this.EventType_len, bArr);
      }
      catch
      {
      }
      int antennaId = (int) this.AntennaID;
      try
      {
        Util.AppendObjToBitArray((object) this.AntennaID, (int) this.AntennaID_len, bArr);
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
