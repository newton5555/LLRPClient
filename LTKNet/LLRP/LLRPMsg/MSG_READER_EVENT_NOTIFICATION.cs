

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;

#nullable disable
namespace Org.LLRP.LTK.LLRPV1
{
  public class MSG_READER_EVENT_NOTIFICATION : Message
  {
    public PARAM_ReaderEventNotificationData ReaderEventNotificationData;

    public MSG_READER_EVENT_NOTIFICATION()
    {
      this.msgType = (ushort) 63;
      this.MSG_ID = MessageID.getNewMessageID();
    }

    public override string ToString()
    {
      string str = "<READER_EVENT_NOTIFICATION" + string.Format(" xmlns=\"{0}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0") + string.Format(" xmlns:llrp=\"{0}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0") + " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n" + string.Format(" xsi:schemaLocation=\"{0} {1}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0/llrp.xsd") + " Version=\"" + this.version.ToString() + "\" MessageID=\"" + this.MSG_ID.ToString() + "\">\r\n";
      if (this.ReaderEventNotificationData != null)
        str += Util.Indent(this.ReaderEventNotificationData.ToString());
      return str + "</READER_EVENT_NOTIFICATION>";
    }

    public static MSG_READER_EVENT_NOTIFICATION FromString(string str)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.LoadXml(str);
      XmlNode documentElement = (XmlNode) xmlDocument.DocumentElement;
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(documentElement.OwnerDocument.NameTable);
      nsmgr.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      nsmgr.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      MSG_READER_EVENT_NOTIFICATION eventNotification = new MSG_READER_EVENT_NOTIFICATION();
      try
      {
        eventNotification.MSG_ID = Convert.ToUInt32(XmlUtil.GetNodeAttrValue(documentElement, "MessageID"));
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(documentElement, "ReaderEventNotificationData", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            eventNotification.ReaderEventNotificationData = PARAM_ReaderEventNotificationData.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      return eventNotification;
    }

    public override bool[] ToBitArray()
    {
      AutoGrowingBitArray autoGrowingBitArray = new AutoGrowingBitArray(10000);
      Util.AppendIntToBitArray(0U, 3, autoGrowingBitArray);
      Util.AppendIntToBitArray((uint) this.version, 3, autoGrowingBitArray);
      Util.AppendIntToBitArray((uint) this.msgType, 10, autoGrowingBitArray);
      Util.AppendIntToBitArray(this.msgLen, 32, autoGrowingBitArray);
      Util.AppendIntToBitArray(this.msgID, 32, autoGrowingBitArray);
      if (this.ReaderEventNotificationData != null)
        this.ReaderEventNotificationData.AppendToBitArray(autoGrowingBitArray);
      int val = autoGrowingBitArray.Length / 8;
      bool[] bitArray = new bool[autoGrowingBitArray.Length];
      autoGrowingBitArray.CopyTo((Array) bitArray, 0);
      Util.ConvertIntToBitArray((uint) val, 32).CopyTo((Array) bitArray, 16);
      return bitArray;
    }

    public static MSG_READER_EVENT_NOTIFICATION FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor > length)
        return (MSG_READER_EVENT_NOTIFICATION) null;
      ArrayList arrayList = new ArrayList();
      MSG_READER_EVENT_NOTIFICATION eventNotification = new MSG_READER_EVENT_NOTIFICATION();
      cursor += 6;
      if ((int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10) != (int) eventNotification.msgType)
      {
        cursor -= 16;
        return (MSG_READER_EVENT_NOTIFICATION) null;
      }
      eventNotification.msgLen = (uint) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 32);
      eventNotification.msgID = (uint) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 32);
      eventNotification.ReaderEventNotificationData = PARAM_ReaderEventNotificationData.FromBitArray(ref bit_array, ref cursor, length);
      return eventNotification;
    }
  }
}
