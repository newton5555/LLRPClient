// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_ReaderEventNotificationSpec
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_ReaderEventNotificationSpec : Parameter
  {
    public PARAM_EventNotificationState[] EventNotificationState;

    public PARAM_ReaderEventNotificationSpec() => this.typeID = (ushort) 244;

    public static PARAM_ReaderEventNotificationSpec FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_ReaderEventNotificationSpec) null;
      int num = cursor;
      ArrayList arrayList1 = new ArrayList();
      PARAM_ReaderEventNotificationSpec notificationSpec = new PARAM_ReaderEventNotificationSpec();
      notificationSpec.tvCoding = bit_array[cursor];
      int val;
      if (notificationSpec.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        notificationSpec.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        int length1 = (int) notificationSpec.length;
      }
      if (val != (int) notificationSpec.TypeID)
      {
        cursor = num;
        return (PARAM_ReaderEventNotificationSpec) null;
      }
      ArrayList arrayList2 = new ArrayList();
      PARAM_EventNotificationState notificationState;
      while ((notificationState = PARAM_EventNotificationState.FromBitArray(ref bit_array, ref cursor, length)) != null)
        arrayList2.Add((object) notificationState);
      if (arrayList2.Count > 0)
      {
        notificationSpec.EventNotificationState = new PARAM_EventNotificationState[arrayList2.Count];
        for (int index = 0; index < arrayList2.Count; ++index)
          notificationSpec.EventNotificationState[index] = (PARAM_EventNotificationState) arrayList2[index];
      }
      return notificationSpec;
    }

    public override string ToString()
    {
      string str = "<ReaderEventNotificationSpec>" + "\r\n";
      if (this.EventNotificationState != null)
      {
        int length = this.EventNotificationState.Length;
        for (int index = 0; index < length; ++index)
          str += Util.Indent(this.EventNotificationState[index].ToString());
      }
      return str + "</ReaderEventNotificationSpec>" + "\r\n";
    }

    public static PARAM_ReaderEventNotificationSpec FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      nsmgr.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      nsmgr.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      PARAM_ReaderEventNotificationSpec notificationSpec = new PARAM_ReaderEventNotificationSpec();
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "EventNotificationState", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
          {
            notificationSpec.EventNotificationState = new PARAM_EventNotificationState[xmlNodes.Count];
            for (int i = 0; i < xmlNodes.Count; ++i)
              notificationSpec.EventNotificationState[i] = PARAM_EventNotificationState.FromXmlNode(xmlNodes[i]);
          }
        }
      }
      catch
      {
      }
      return notificationSpec;
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
      if (this.EventNotificationState != null)
      {
        int length = this.EventNotificationState.Length;
        for (int index = 0; index < length; ++index)
          this.EventNotificationState[index].ToBitArray(ref bit_array, ref cursor);
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
      if (this.EventNotificationState != null)
      {
        int length3 = this.EventNotificationState.Length;
        for (int index = 0; index < length3; ++index)
          this.EventNotificationState[index].AppendToBitArray(bArr);
      }
      if (this.tvCoding)
        return;
      BitArray bitArray = Util.ConvertIntToBitArray((uint) (bArr.Length - length1) / 8U, 16);
      for (int index = 0; index < bitArray.Length; ++index)
        bArr[length1 + 16 + index] = bitArray[index];
    }
  }
}
