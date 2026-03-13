// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_EventNotificationState
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_EventNotificationState : Parameter
  {
    public ENUM_NotificationEventType EventType;
    private short EventType_len = 16;
    public bool NotificationState;
    private short NotificationState_len;
    private const ushort param_reserved_len4 = 7;

    public PARAM_EventNotificationState() => this.typeID = (ushort) 245;

    public static PARAM_EventNotificationState FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_EventNotificationState) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_EventNotificationState notificationState = new PARAM_EventNotificationState();
      notificationState.tvCoding = bit_array[cursor];
      int val;
      if (notificationState.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        notificationState.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) notificationState.length * 8;
      }
      if (val != (int) notificationState.TypeID)
      {
        cursor = num1;
        return (PARAM_EventNotificationState) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 16;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len1);
      notificationState.EventType = (ENUM_NotificationEventType) (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 1;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (bool), field_len2);
      notificationState.NotificationState = (bool) obj;
      cursor += 7;
      return notificationState;
    }

    public override string ToString()
    {
      string str = "<EventNotificationState>" + "\r\n";
      int eventType = (int) this.EventType;
      try
      {
        str = str + "  <EventType>" + this.EventType.ToString() + "</EventType>";
        str += "\r\n";
      }
      catch
      {
      }
      int num = this.NotificationState ? 1 : 0;
      try
      {
        str = str + "  <NotificationState>" + Util.ConvertValueTypeToString((object) this.NotificationState, "u1", "") + "</NotificationState>";
        str += "\r\n";
      }
      catch
      {
      }
      return str + "</EventNotificationState>" + "\r\n";
    }

    public static PARAM_EventNotificationState FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      namespaceManager.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      namespaceManager.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      return new PARAM_EventNotificationState()
      {
        EventType = (ENUM_NotificationEventType) Enum.Parse(typeof (ENUM_NotificationEventType), XmlUtil.GetNodeValue(node, "EventType")),
        NotificationState = (bool) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "NotificationState"), "u1", "")
      };
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
      int num2 = this.NotificationState ? 1 : 0;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.NotificationState, (int) this.NotificationState_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      cursor += 7;
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
      int eventType = (int) this.EventType;
      try
      {
        Util.AppendObjToBitArray((object) this.EventType, (int) this.EventType_len, bArr);
      }
      catch
      {
      }
      int num = this.NotificationState ? 1 : 0;
      try
      {
        Util.AppendObjToBitArray((object) this.NotificationState, (int) this.NotificationState_len, bArr);
      }
      catch
      {
      }
      bArr.Length += 7;
      if (this.tvCoding)
        return;
      BitArray bitArray = Util.ConvertIntToBitArray((uint) (bArr.Length - length1) / 8U, 16);
      for (int index = 0; index < bitArray.Length; ++index)
        bArr[length1 + 16 + index] = bitArray[index];
    }
  }
}
