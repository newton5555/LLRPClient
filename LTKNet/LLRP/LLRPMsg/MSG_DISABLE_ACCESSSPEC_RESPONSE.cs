// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.MSG_DISABLE_ACCESSSPEC_RESPONSE
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
  public class MSG_DISABLE_ACCESSSPEC_RESPONSE : Message
  {
    public PARAM_LLRPStatus LLRPStatus;

    public MSG_DISABLE_ACCESSSPEC_RESPONSE()
    {
      this.msgType = (ushort) 53;
      this.MSG_ID = MessageID.getNewMessageID();
    }

    public override string ToString()
    {
      string str = "<DISABLE_ACCESSSPEC_RESPONSE" + string.Format(" xmlns=\"{0}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0") + string.Format(" xmlns:llrp=\"{0}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0") + " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n" + string.Format(" xsi:schemaLocation=\"{0} {1}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0/llrp.xsd") + " Version=\"" + this.version.ToString() + "\" MessageID=\"" + this.MSG_ID.ToString() + "\">\r\n";
      if (this.LLRPStatus != null)
        str += Util.Indent(this.LLRPStatus.ToString());
      return str + "</DISABLE_ACCESSSPEC_RESPONSE>";
    }

    public static MSG_DISABLE_ACCESSSPEC_RESPONSE FromString(string str)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.LoadXml(str);
      XmlNode documentElement = (XmlNode) xmlDocument.DocumentElement;
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(documentElement.OwnerDocument.NameTable);
      nsmgr.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      nsmgr.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      MSG_DISABLE_ACCESSSPEC_RESPONSE accessspecResponse = new MSG_DISABLE_ACCESSSPEC_RESPONSE();
      try
      {
        accessspecResponse.MSG_ID = Convert.ToUInt32(XmlUtil.GetNodeAttrValue(documentElement, "MessageID"));
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(documentElement, "LLRPStatus", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            accessspecResponse.LLRPStatus = PARAM_LLRPStatus.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      return accessspecResponse;
    }

    public override bool[] ToBitArray()
    {
      AutoGrowingBitArray autoGrowingBitArray = new AutoGrowingBitArray(10000);
      Util.AppendIntToBitArray(0U, 3, autoGrowingBitArray);
      Util.AppendIntToBitArray((uint) this.version, 3, autoGrowingBitArray);
      Util.AppendIntToBitArray((uint) this.msgType, 10, autoGrowingBitArray);
      Util.AppendIntToBitArray(this.msgLen, 32, autoGrowingBitArray);
      Util.AppendIntToBitArray(this.msgID, 32, autoGrowingBitArray);
      if (this.LLRPStatus != null)
        this.LLRPStatus.AppendToBitArray(autoGrowingBitArray);
      int val = autoGrowingBitArray.Length / 8;
      bool[] bitArray = new bool[autoGrowingBitArray.Length];
      autoGrowingBitArray.CopyTo((Array) bitArray, 0);
      Util.ConvertIntToBitArray((uint) val, 32).CopyTo((Array) bitArray, 16);
      return bitArray;
    }

    public static MSG_DISABLE_ACCESSSPEC_RESPONSE FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor > length)
        return (MSG_DISABLE_ACCESSSPEC_RESPONSE) null;
      ArrayList arrayList = new ArrayList();
      MSG_DISABLE_ACCESSSPEC_RESPONSE accessspecResponse = new MSG_DISABLE_ACCESSSPEC_RESPONSE();
      cursor += 6;
      if ((int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10) != (int) accessspecResponse.msgType)
      {
        cursor -= 16;
        return (MSG_DISABLE_ACCESSSPEC_RESPONSE) null;
      }
      accessspecResponse.msgLen = (uint) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 32);
      accessspecResponse.msgID = (uint) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 32);
      accessspecResponse.LLRPStatus = PARAM_LLRPStatus.FromBitArray(ref bit_array, ref cursor, length);
      return accessspecResponse;
    }
  }
}
