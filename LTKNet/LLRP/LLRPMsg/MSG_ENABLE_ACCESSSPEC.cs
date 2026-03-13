// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.MSG_ENABLE_ACCESSSPEC
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
  public class MSG_ENABLE_ACCESSSPEC : Message
  {
    public uint AccessSpecID;
    private short AccessSpecID_len;

    public MSG_ENABLE_ACCESSSPEC()
    {
      this.msgType = (ushort) 42;
      this.MSG_ID = MessageID.getNewMessageID();
    }

    public override string ToString()
    {
      string str = "<ENABLE_ACCESSSPEC" + string.Format(" xmlns=\"{0}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0") + string.Format(" xmlns:llrp=\"{0}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0") + " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n" + string.Format(" xsi:schemaLocation=\"{0} {1}\"\n", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0", (object) "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0/llrp.xsd") + " Version=\"" + this.version.ToString() + "\" MessageID=\"" + this.MSG_ID.ToString() + "\">\r\n";
      int accessSpecId = (int) this.AccessSpecID;
      try
      {
        str = str + "  <AccessSpecID>" + Util.ConvertValueTypeToString((object) this.AccessSpecID, "u32", "") + "</AccessSpecID>";
        str += "\r\n";
      }
      catch
      {
      }
      return str + "</ENABLE_ACCESSSPEC>";
    }

    public static MSG_ENABLE_ACCESSSPEC FromString(string str)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.LoadXml(str);
      XmlNode documentElement = (XmlNode) xmlDocument.DocumentElement;
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(documentElement.OwnerDocument.NameTable);
      namespaceManager.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      namespaceManager.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      MSG_ENABLE_ACCESSSPEC enableAccessspec = new MSG_ENABLE_ACCESSSPEC();
      try
      {
        enableAccessspec.MSG_ID = Convert.ToUInt32(XmlUtil.GetNodeAttrValue(documentElement, "MessageID"));
      }
      catch
      {
      }
      string nodeValue = XmlUtil.GetNodeValue(documentElement, "AccessSpecID");
      enableAccessspec.AccessSpecID = (uint) Util.ParseValueTypeFromString(nodeValue, "u32", "");
      return enableAccessspec;
    }

    public override bool[] ToBitArray()
    {
      AutoGrowingBitArray bit_arr = new AutoGrowingBitArray(10000);
      Util.AppendIntToBitArray(0U, 3, bit_arr);
      Util.AppendIntToBitArray((uint) this.version, 3, bit_arr);
      Util.AppendIntToBitArray((uint) this.msgType, 10, bit_arr);
      Util.AppendIntToBitArray(this.msgLen, 32, bit_arr);
      Util.AppendIntToBitArray(this.msgID, 32, bit_arr);
      int accessSpecId = (int) this.AccessSpecID;
      try
      {
        Util.AppendObjToBitArray((object) this.AccessSpecID, (int) this.AccessSpecID_len, bit_arr);
      }
      catch
      {
      }
      int val = bit_arr.Length / 8;
      bool[] bitArray = new bool[bit_arr.Length];
      bit_arr.CopyTo((Array) bitArray, 0);
      Util.ConvertIntToBitArray((uint) val, 32).CopyTo((Array) bitArray, 16);
      return bitArray;
    }

    public static MSG_ENABLE_ACCESSSPEC FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor > length)
        return (MSG_ENABLE_ACCESSSPEC) null;
      ArrayList arrayList = new ArrayList();
      MSG_ENABLE_ACCESSSPEC enableAccessspec = new MSG_ENABLE_ACCESSSPEC();
      cursor += 6;
      if ((int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10) != (int) enableAccessspec.msgType)
      {
        cursor -= 16;
        return (MSG_ENABLE_ACCESSSPEC) null;
      }
      enableAccessspec.msgLen = (uint) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 32);
      enableAccessspec.msgID = (uint) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 32);
      if (cursor > length)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len = 32;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len);
      enableAccessspec.AccessSpecID = (uint) obj;
      return enableAccessspec;
    }
  }
}
