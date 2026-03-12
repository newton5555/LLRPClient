// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.MSG_GET_ROSPECS_RESPONSE
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
  public class MSG_GET_ROSPECS_RESPONSE : Message
  {
    public PARAM_LLRPStatus LLRPStatus;
    public PARAM_ROSpec[] ROSpec;

    public MSG_GET_ROSPECS_RESPONSE()
    {
      this.msgType = (ushort) 36;
      this.MSG_ID = MessageID.getNewMessageID();
    }

    public override string ToString()
    {
      string str = "<GET_ROSPECS_RESPONSE" + string.Format(" xmlns=\"{0}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0") + string.Format(" xmlns:llrp=\"{0}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0") + " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n" + string.Format(" xsi:schemaLocation=\"{0} {1}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0/llrp.xsd") + " Version=\"" + this.version.ToString() + "\" MessageID=\"" + this.MSG_ID.ToString() + "\">\r\n";
      if (this.LLRPStatus != null)
        str += Util.Indent(this.LLRPStatus.ToString());
      if (this.ROSpec != null)
      {
        int length = this.ROSpec.Length;
        for (int index = 0; index < length; ++index)
          str += Util.Indent(this.ROSpec[index].ToString());
      }
      return str + "</GET_ROSPECS_RESPONSE>";
    }

    public static MSG_GET_ROSPECS_RESPONSE FromString(string str)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.LoadXml(str);
      XmlNode documentElement = (XmlNode) xmlDocument.DocumentElement;
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(documentElement.OwnerDocument.NameTable);
      nsmgr.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      nsmgr.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      MSG_GET_ROSPECS_RESPONSE getRospecsResponse = new MSG_GET_ROSPECS_RESPONSE();
      try
      {
        getRospecsResponse.MSG_ID = Convert.ToUInt32(XmlUtil.GetNodeAttrValue(documentElement, "MessageID"));
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
            getRospecsResponse.LLRPStatus = PARAM_LLRPStatus.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(documentElement, "ROSpec", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
          {
            getRospecsResponse.ROSpec = new PARAM_ROSpec[xmlNodes.Count];
            for (int i = 0; i < xmlNodes.Count; ++i)
              getRospecsResponse.ROSpec[i] = PARAM_ROSpec.FromXmlNode(xmlNodes[i]);
          }
        }
      }
      catch
      {
      }
      return getRospecsResponse;
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
      if (this.ROSpec != null)
      {
        int length = this.ROSpec.Length;
        for (int index = 0; index < length; ++index)
          this.ROSpec[index].AppendToBitArray(autoGrowingBitArray);
      }
      int val = autoGrowingBitArray.Length / 8;
      bool[] bitArray = new bool[autoGrowingBitArray.Length];
      autoGrowingBitArray.CopyTo((Array) bitArray, 0);
      Util.ConvertIntToBitArray((uint) val, 32).CopyTo((Array) bitArray, 16);
      return bitArray;
    }

    public static MSG_GET_ROSPECS_RESPONSE FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor > length)
        return (MSG_GET_ROSPECS_RESPONSE) null;
      ArrayList arrayList1 = new ArrayList();
      MSG_GET_ROSPECS_RESPONSE getRospecsResponse = new MSG_GET_ROSPECS_RESPONSE();
      cursor += 6;
      if ((int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10) != (int) getRospecsResponse.msgType)
      {
        cursor -= 16;
        return (MSG_GET_ROSPECS_RESPONSE) null;
      }
      getRospecsResponse.msgLen = (uint) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 32);
      getRospecsResponse.msgID = (uint) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 32);
      getRospecsResponse.LLRPStatus = PARAM_LLRPStatus.FromBitArray(ref bit_array, ref cursor, length);
      ArrayList arrayList2 = new ArrayList();
      PARAM_ROSpec paramRoSpec;
      while ((paramRoSpec = PARAM_ROSpec.FromBitArray(ref bit_array, ref cursor, length)) != null)
        arrayList2.Add((object) paramRoSpec);
      if (arrayList2.Count > 0)
      {
        getRospecsResponse.ROSpec = new PARAM_ROSpec[arrayList2.Count];
        for (int index = 0; index < arrayList2.Count; ++index)
          getRospecsResponse.ROSpec[index] = (PARAM_ROSpec) arrayList2[index];
      }
      return getRospecsResponse;
    }
  }
}
