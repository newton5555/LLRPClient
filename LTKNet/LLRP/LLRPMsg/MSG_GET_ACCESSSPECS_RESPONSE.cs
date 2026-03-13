// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.MSG_GET_ACCESSSPECS_RESPONSE
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
  public class MSG_GET_ACCESSSPECS_RESPONSE : Message
  {
    public PARAM_LLRPStatus LLRPStatus;
    public PARAM_AccessSpec[] AccessSpec;

    public MSG_GET_ACCESSSPECS_RESPONSE()
    {
      this.msgType = (ushort) 54;
      this.MSG_ID = MessageID.getNewMessageID();
    }

    public override string ToString()
    {
      string str = "<GET_ACCESSSPECS_RESPONSE" + string.Format(" xmlns=\"{0}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0") + string.Format(" xmlns:llrp=\"{0}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0") + " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n" + string.Format(" xsi:schemaLocation=\"{0} {1}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0/llrp.xsd") + " Version=\"" + this.version.ToString() + "\" MessageID=\"" + this.MSG_ID.ToString() + "\">\r\n";
      if (this.LLRPStatus != null)
        str += Util.Indent(this.LLRPStatus.ToString());
      if (this.AccessSpec != null)
      {
        int length = this.AccessSpec.Length;
        for (int index = 0; index < length; ++index)
          str += Util.Indent(this.AccessSpec[index].ToString());
      }
      return str + "</GET_ACCESSSPECS_RESPONSE>";
    }

    public static MSG_GET_ACCESSSPECS_RESPONSE FromString(string str)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.LoadXml(str);
      XmlNode documentElement = (XmlNode) xmlDocument.DocumentElement;
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(documentElement.OwnerDocument.NameTable);
      nsmgr.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      nsmgr.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      MSG_GET_ACCESSSPECS_RESPONSE accessspecsResponse = new MSG_GET_ACCESSSPECS_RESPONSE();
      try
      {
        accessspecsResponse.MSG_ID = Convert.ToUInt32(XmlUtil.GetNodeAttrValue(documentElement, "MessageID"));
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
            accessspecsResponse.LLRPStatus = PARAM_LLRPStatus.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(documentElement, "AccessSpec", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
          {
            accessspecsResponse.AccessSpec = new PARAM_AccessSpec[xmlNodes.Count];
            for (int i = 0; i < xmlNodes.Count; ++i)
              accessspecsResponse.AccessSpec[i] = PARAM_AccessSpec.FromXmlNode(xmlNodes[i]);
          }
        }
      }
      catch
      {
      }
      return accessspecsResponse;
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
      if (this.AccessSpec != null)
      {
        int length = this.AccessSpec.Length;
        for (int index = 0; index < length; ++index)
          this.AccessSpec[index].AppendToBitArray(autoGrowingBitArray);
      }
      int val = autoGrowingBitArray.Length / 8;
      bool[] bitArray = new bool[autoGrowingBitArray.Length];
      autoGrowingBitArray.CopyTo((Array) bitArray, 0);
      Util.ConvertIntToBitArray((uint) val, 32).CopyTo((Array) bitArray, 16);
      return bitArray;
    }

    public static MSG_GET_ACCESSSPECS_RESPONSE FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor > length)
        return (MSG_GET_ACCESSSPECS_RESPONSE) null;
      ArrayList arrayList1 = new ArrayList();
      MSG_GET_ACCESSSPECS_RESPONSE accessspecsResponse = new MSG_GET_ACCESSSPECS_RESPONSE();
      cursor += 6;
      if ((int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10) != (int) accessspecsResponse.msgType)
      {
        cursor -= 16;
        return (MSG_GET_ACCESSSPECS_RESPONSE) null;
      }
      accessspecsResponse.msgLen = (uint) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 32);
      accessspecsResponse.msgID = (uint) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 32);
      accessspecsResponse.LLRPStatus = PARAM_LLRPStatus.FromBitArray(ref bit_array, ref cursor, length);
      ArrayList arrayList2 = new ArrayList();
      PARAM_AccessSpec paramAccessSpec;
      while ((paramAccessSpec = PARAM_AccessSpec.FromBitArray(ref bit_array, ref cursor, length)) != null)
        arrayList2.Add((object) paramAccessSpec);
      if (arrayList2.Count > 0)
      {
        accessspecsResponse.AccessSpec = new PARAM_AccessSpec[arrayList2.Count];
        for (int index = 0; index < arrayList2.Count; ++index)
          accessspecsResponse.AccessSpec[index] = (PARAM_AccessSpec) arrayList2[index];
      }
      return accessspecsResponse;
    }
  }
}
