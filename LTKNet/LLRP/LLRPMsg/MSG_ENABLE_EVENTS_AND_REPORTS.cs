// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.MSG_ENABLE_EVENTS_AND_REPORTS
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
  public class MSG_ENABLE_EVENTS_AND_REPORTS : Message
  {
    public MSG_ENABLE_EVENTS_AND_REPORTS()
    {
      this.msgType = (ushort) 64;
      this.MSG_ID = MessageID.getNewMessageID();
    }

    public override string ToString()
    {
      return "<ENABLE_EVENTS_AND_REPORTS" + string.Format(" xmlns=\"{0}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0") + string.Format(" xmlns:llrp=\"{0}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0") + " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n" + string.Format(" xsi:schemaLocation=\"{0} {1}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0/llrp.xsd") + " Version=\"" + this.version.ToString() + "\" MessageID=\"" + this.MSG_ID.ToString() + "\">\r\n" + "</ENABLE_EVENTS_AND_REPORTS>";
    }

    public static MSG_ENABLE_EVENTS_AND_REPORTS FromString(string str)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.LoadXml(str);
      XmlNode documentElement = (XmlNode) xmlDocument.DocumentElement;
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(documentElement.OwnerDocument.NameTable);
      namespaceManager.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      namespaceManager.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      MSG_ENABLE_EVENTS_AND_REPORTS eventsAndReports = new MSG_ENABLE_EVENTS_AND_REPORTS();
      try
      {
        eventsAndReports.MSG_ID = Convert.ToUInt32(XmlUtil.GetNodeAttrValue(documentElement, "MessageID"));
      }
      catch
      {
      }
      return eventsAndReports;
    }

    public override bool[] ToBitArray()
    {
      AutoGrowingBitArray bit_arr = new AutoGrowingBitArray(10000);
      Util.AppendIntToBitArray(0U, 3, bit_arr);
      Util.AppendIntToBitArray((uint) this.version, 3, bit_arr);
      Util.AppendIntToBitArray((uint) this.msgType, 10, bit_arr);
      Util.AppendIntToBitArray(this.msgLen, 32, bit_arr);
      Util.AppendIntToBitArray(this.msgID, 32, bit_arr);
      int val = bit_arr.Length / 8;
      bool[] bitArray = new bool[bit_arr.Length];
      bit_arr.CopyTo((Array) bitArray, 0);
      Util.ConvertIntToBitArray((uint) val, 32).CopyTo((Array) bitArray, 16);
      return bitArray;
    }

    public static MSG_ENABLE_EVENTS_AND_REPORTS FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor > length)
        return (MSG_ENABLE_EVENTS_AND_REPORTS) null;
      ArrayList arrayList = new ArrayList();
      MSG_ENABLE_EVENTS_AND_REPORTS eventsAndReports = new MSG_ENABLE_EVENTS_AND_REPORTS();
      cursor += 6;
      if ((int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10) != (int) eventsAndReports.msgType)
      {
        cursor -= 16;
        return (MSG_ENABLE_EVENTS_AND_REPORTS) null;
      }
      eventsAndReports.msgLen = (uint) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 32);
      eventsAndReports.msgID = (uint) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 32);
      return eventsAndReports;
    }
  }
}
