// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.MSG_CLIENT_REQUEST_OP
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
  public class MSG_CLIENT_REQUEST_OP : Message
  {
    public PARAM_TagReportData TagReportData;

    public MSG_CLIENT_REQUEST_OP()
    {
      this.msgType = (ushort) 45;
      this.MSG_ID = MessageID.getNewMessageID();
    }

    public override string ToString()
    {
      string str = "<CLIENT_REQUEST_OP" + string.Format(" xmlns=\"{0}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0") + string.Format(" xmlns:llrp=\"{0}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0") + " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n" + string.Format(" xsi:schemaLocation=\"{0} {1}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0/llrp.xsd") + " Version=\"" + this.version.ToString() + "\" MessageID=\"" + this.MSG_ID.ToString() + "\">\r\n";
      if (this.TagReportData != null)
        str += Util.Indent(this.TagReportData.ToString());
      return str + "</CLIENT_REQUEST_OP>";
    }

    public static MSG_CLIENT_REQUEST_OP FromString(string str)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.LoadXml(str);
      XmlNode documentElement = (XmlNode) xmlDocument.DocumentElement;
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(documentElement.OwnerDocument.NameTable);
      nsmgr.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      nsmgr.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      MSG_CLIENT_REQUEST_OP msgClientRequestOp = new MSG_CLIENT_REQUEST_OP();
      try
      {
        msgClientRequestOp.MSG_ID = Convert.ToUInt32(XmlUtil.GetNodeAttrValue(documentElement, "MessageID"));
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(documentElement, "TagReportData", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            msgClientRequestOp.TagReportData = PARAM_TagReportData.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      return msgClientRequestOp;
    }

    public override bool[] ToBitArray()
    {
      AutoGrowingBitArray autoGrowingBitArray = new AutoGrowingBitArray(10000);
      Util.AppendIntToBitArray(0U, 3, autoGrowingBitArray);
      Util.AppendIntToBitArray((uint) this.version, 3, autoGrowingBitArray);
      Util.AppendIntToBitArray((uint) this.msgType, 10, autoGrowingBitArray);
      Util.AppendIntToBitArray(this.msgLen, 32, autoGrowingBitArray);
      Util.AppendIntToBitArray(this.msgID, 32, autoGrowingBitArray);
      if (this.TagReportData != null)
        this.TagReportData.AppendToBitArray(autoGrowingBitArray);
      int val = autoGrowingBitArray.Length / 8;
      bool[] bitArray = new bool[autoGrowingBitArray.Length];
      autoGrowingBitArray.CopyTo((Array) bitArray, 0);
      Util.ConvertIntToBitArray((uint) val, 32).CopyTo((Array) bitArray, 16);
      return bitArray;
    }

    public static MSG_CLIENT_REQUEST_OP FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor > length)
        return (MSG_CLIENT_REQUEST_OP) null;
      ArrayList arrayList = new ArrayList();
      MSG_CLIENT_REQUEST_OP msgClientRequestOp = new MSG_CLIENT_REQUEST_OP();
      cursor += 6;
      if ((int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10) != (int) msgClientRequestOp.msgType)
      {
        cursor -= 16;
        return (MSG_CLIENT_REQUEST_OP) null;
      }
      msgClientRequestOp.msgLen = (uint) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 32);
      msgClientRequestOp.msgID = (uint) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 32);
      msgClientRequestOp.TagReportData = PARAM_TagReportData.FromBitArray(ref bit_array, ref cursor, length);
      return msgClientRequestOp;
    }
  }
}
