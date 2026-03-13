// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_ClientRequestResponse
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_ClientRequestResponse : Parameter
  {
    public uint AccessSpecID;
    private short AccessSpecID_len;
    public PARAM_EPCData EPCData;
    public UNION_AirProtocolOpSpec AirProtocolOpSpec = new UNION_AirProtocolOpSpec();

    public PARAM_ClientRequestResponse() => this.typeID = (ushort) 211;

    public static PARAM_ClientRequestResponse FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_ClientRequestResponse) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_ClientRequestResponse clientRequestResponse = new PARAM_ClientRequestResponse();
      clientRequestResponse.tvCoding = bit_array[cursor];
      int val1;
      if (clientRequestResponse.tvCoding)
      {
        ++cursor;
        val1 = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val1 = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        clientRequestResponse.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) clientRequestResponse.length * 8;
      }
      if (val1 != (int) clientRequestResponse.TypeID)
      {
        cursor = num1;
        return (PARAM_ClientRequestResponse) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len = 32;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len);
      clientRequestResponse.AccessSpecID = (uint) obj;
      clientRequestResponse.EPCData = PARAM_EPCData.FromBitArray(ref bit_array, ref cursor, length);
      ushort num3 = 1;
      while (num3 != (ushort) 0)
      {
        num3 = (ushort) 0;
        PARAM_C1G2Read val2 = PARAM_C1G2Read.FromBitArray(ref bit_array, ref cursor, length);
        if (val2 != null)
        {
          ++num3;
          clientRequestResponse.AirProtocolOpSpec.Add((IParameter) val2);
        }
        PARAM_C1G2Write val3 = PARAM_C1G2Write.FromBitArray(ref bit_array, ref cursor, length);
        if (val3 != null)
        {
          ++num3;
          clientRequestResponse.AirProtocolOpSpec.Add((IParameter) val3);
        }
        PARAM_C1G2Kill val4 = PARAM_C1G2Kill.FromBitArray(ref bit_array, ref cursor, length);
        if (val4 != null)
        {
          ++num3;
          clientRequestResponse.AirProtocolOpSpec.Add((IParameter) val4);
        }
        PARAM_C1G2Lock val5 = PARAM_C1G2Lock.FromBitArray(ref bit_array, ref cursor, length);
        if (val5 != null)
        {
          ++num3;
          clientRequestResponse.AirProtocolOpSpec.Add((IParameter) val5);
        }
        PARAM_C1G2BlockErase val6 = PARAM_C1G2BlockErase.FromBitArray(ref bit_array, ref cursor, length);
        if (val6 != null)
        {
          ++num3;
          clientRequestResponse.AirProtocolOpSpec.Add((IParameter) val6);
        }
        PARAM_C1G2BlockWrite val7 = PARAM_C1G2BlockWrite.FromBitArray(ref bit_array, ref cursor, length);
        if (val7 != null)
        {
          ++num3;
          clientRequestResponse.AirProtocolOpSpec.Add((IParameter) val7);
        }
      }
      return clientRequestResponse;
    }

    public override string ToString()
    {
      string str = "<ClientRequestResponse>" + "\r\n";
      int accessSpecId = (int) this.AccessSpecID;
      try
      {
        str = str + "  <AccessSpecID>" + Util.ConvertValueTypeToString((object) this.AccessSpecID, "u32", "") + "</AccessSpecID>";
        str += "\r\n";
      }
      catch
      {
      }
      if (this.EPCData != null)
        str += Util.Indent(this.EPCData.ToString());
      if (this.AirProtocolOpSpec != null)
      {
        int count = this.AirProtocolOpSpec.Count;
        for (int index = 0; index < count; ++index)
          str += Util.Indent(this.AirProtocolOpSpec[index].ToString());
      }
      return str + "</ClientRequestResponse>" + "\r\n";
    }

    public static PARAM_ClientRequestResponse FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      nsmgr.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      nsmgr.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      PARAM_ClientRequestResponse clientRequestResponse = new PARAM_ClientRequestResponse();
      string nodeValue = XmlUtil.GetNodeValue(node, "AccessSpecID");
      clientRequestResponse.AccessSpecID = (uint) Util.ParseValueTypeFromString(nodeValue, "u32", "");
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "EPCData", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            clientRequestResponse.EPCData = PARAM_EPCData.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      clientRequestResponse.AirProtocolOpSpec = new UNION_AirProtocolOpSpec();
      try
      {
        foreach (XmlNode childNode in node.ChildNodes)
        {
          switch (childNode.Name)
          {
            case "C1G2Read":
              clientRequestResponse.AirProtocolOpSpec.Add((IParameter) PARAM_C1G2Read.FromXmlNode(childNode));
              continue;
            case "C1G2Write":
              clientRequestResponse.AirProtocolOpSpec.Add((IParameter) PARAM_C1G2Write.FromXmlNode(childNode));
              continue;
            case "C1G2Kill":
              clientRequestResponse.AirProtocolOpSpec.Add((IParameter) PARAM_C1G2Kill.FromXmlNode(childNode));
              continue;
            case "C1G2Lock":
              clientRequestResponse.AirProtocolOpSpec.Add((IParameter) PARAM_C1G2Lock.FromXmlNode(childNode));
              continue;
            case "C1G2BlockErase":
              clientRequestResponse.AirProtocolOpSpec.Add((IParameter) PARAM_C1G2BlockErase.FromXmlNode(childNode));
              continue;
            case "C1G2BlockWrite":
              clientRequestResponse.AirProtocolOpSpec.Add((IParameter) PARAM_C1G2BlockWrite.FromXmlNode(childNode));
              continue;
            default:
              continue;
          }
        }
      }
      catch
      {
      }
      return clientRequestResponse;
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
      int accessSpecId = (int) this.AccessSpecID;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.AccessSpecID, (int) this.AccessSpecID_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      if (this.EPCData != null)
        this.EPCData.ToBitArray(ref bit_array, ref cursor);
      int count = this.AirProtocolOpSpec.Count;
      for (int index = 0; index < count; ++index)
        this.AirProtocolOpSpec[index].ToBitArray(ref bit_array, ref cursor);
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
      int accessSpecId = (int) this.AccessSpecID;
      try
      {
        Util.AppendObjToBitArray((object) this.AccessSpecID, (int) this.AccessSpecID_len, bArr);
      }
      catch
      {
      }
      if (this.EPCData != null)
        this.EPCData.AppendToBitArray(bArr);
      int count = this.AirProtocolOpSpec.Count;
      for (int index = 0; index < count; ++index)
        this.AirProtocolOpSpec[index].AppendToBitArray(bArr);
      if (this.tvCoding)
        return;
      BitArray bitArray = Util.ConvertIntToBitArray((uint) (bArr.Length - length1) / 8U, 16);
      for (int index = 0; index < bitArray.Length; ++index)
        bArr[length1 + 16 + index] = bitArray[index];
    }
  }
}
