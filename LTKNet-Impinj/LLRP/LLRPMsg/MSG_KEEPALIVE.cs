// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.MSG_KEEPALIVE
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
  public class MSG_KEEPALIVE : Message
  {
    public MSG_KEEPALIVE()
    {
      this.msgType = (ushort) 62;
      this.MSG_ID = MessageID.getNewMessageID();
    }

    public override string ToString()
    {
      return "<KEEPALIVE" + string.Format(" xmlns=\"{0}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0") + string.Format(" xmlns:llrp=\"{0}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0") + " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n" + string.Format(" xsi:schemaLocation=\"{0} {1}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0/llrp.xsd") + " Version=\"" + this.version.ToString() + "\" MessageID=\"" + this.MSG_ID.ToString() + "\">\r\n" + "</KEEPALIVE>";
    }

    public static MSG_KEEPALIVE FromString(string str)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.LoadXml(str);
      XmlNode documentElement = (XmlNode) xmlDocument.DocumentElement;
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(documentElement.OwnerDocument.NameTable);
      namespaceManager.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      namespaceManager.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      MSG_KEEPALIVE msgKeepalive = new MSG_KEEPALIVE();
      try
      {
        msgKeepalive.MSG_ID = Convert.ToUInt32(XmlUtil.GetNodeAttrValue(documentElement, "MessageID"));
      }
      catch
      {
      }
      return msgKeepalive;
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

    public static MSG_KEEPALIVE FromBitArray(ref BitArray bit_array, ref int cursor, int length)
    {
      if (cursor > length)
        return (MSG_KEEPALIVE) null;
      ArrayList arrayList = new ArrayList();
      MSG_KEEPALIVE msgKeepalive = new MSG_KEEPALIVE();
      cursor += 6;
      if ((int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10) != (int) msgKeepalive.msgType)
      {
        cursor -= 16;
        return (MSG_KEEPALIVE) null;
      }
      msgKeepalive.msgLen = (uint) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 32);
      msgKeepalive.msgID = (uint) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 32);
      return msgKeepalive;
    }
  }
}
