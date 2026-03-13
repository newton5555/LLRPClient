// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_FrequencyInformation
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_FrequencyInformation : Parameter
  {
    public bool Hopping;
    private short Hopping_len;
    private const ushort param_reserved_len3 = 7;
    public PARAM_FrequencyHopTable[] FrequencyHopTable;
    public PARAM_FixedFrequencyTable FixedFrequencyTable;

    public PARAM_FrequencyInformation() => this.typeID = (ushort) 146;

    public static PARAM_FrequencyInformation FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_FrequencyInformation) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList1 = new ArrayList();
      PARAM_FrequencyInformation frequencyInformation = new PARAM_FrequencyInformation();
      frequencyInformation.tvCoding = bit_array[cursor];
      int val;
      if (frequencyInformation.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        frequencyInformation.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) frequencyInformation.length * 8;
      }
      if (val != (int) frequencyInformation.TypeID)
      {
        cursor = num1;
        return (PARAM_FrequencyInformation) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len = 1;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (bool), field_len);
      frequencyInformation.Hopping = (bool) obj;
      cursor += 7;
      ArrayList arrayList2 = new ArrayList();
      PARAM_FrequencyHopTable frequencyHopTable;
      while ((frequencyHopTable = PARAM_FrequencyHopTable.FromBitArray(ref bit_array, ref cursor, length)) != null)
        arrayList2.Add((object) frequencyHopTable);
      if (arrayList2.Count > 0)
      {
        frequencyInformation.FrequencyHopTable = new PARAM_FrequencyHopTable[arrayList2.Count];
        for (int index = 0; index < arrayList2.Count; ++index)
          frequencyInformation.FrequencyHopTable[index] = (PARAM_FrequencyHopTable) arrayList2[index];
      }
      frequencyInformation.FixedFrequencyTable = PARAM_FixedFrequencyTable.FromBitArray(ref bit_array, ref cursor, length);
      return frequencyInformation;
    }

    public override string ToString()
    {
      string str = "<FrequencyInformation>" + "\r\n";
      int num = this.Hopping ? 1 : 0;
      try
      {
        str = str + "  <Hopping>" + Util.ConvertValueTypeToString((object) this.Hopping, "u1", "") + "</Hopping>";
        str += "\r\n";
      }
      catch
      {
      }
      if (this.FrequencyHopTable != null)
      {
        int length = this.FrequencyHopTable.Length;
        for (int index = 0; index < length; ++index)
          str += Util.Indent(this.FrequencyHopTable[index].ToString());
      }
      if (this.FixedFrequencyTable != null)
        str += Util.Indent(this.FixedFrequencyTable.ToString());
      return str + "</FrequencyInformation>" + "\r\n";
    }

    public static PARAM_FrequencyInformation FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      nsmgr.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      nsmgr.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      PARAM_FrequencyInformation frequencyInformation = new PARAM_FrequencyInformation();
      string nodeValue = XmlUtil.GetNodeValue(node, "Hopping");
      frequencyInformation.Hopping = (bool) Util.ParseValueTypeFromString(nodeValue, "u1", "");
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "FrequencyHopTable", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
          {
            frequencyInformation.FrequencyHopTable = new PARAM_FrequencyHopTable[xmlNodes.Count];
            for (int i = 0; i < xmlNodes.Count; ++i)
              frequencyInformation.FrequencyHopTable[i] = PARAM_FrequencyHopTable.FromXmlNode(xmlNodes[i]);
          }
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "FixedFrequencyTable", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            frequencyInformation.FixedFrequencyTable = PARAM_FixedFrequencyTable.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      return frequencyInformation;
    }

    public override void ToBitArray(ref bool[] bit_array, ref int cursor)
    {
      int num1 = cursor;
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
      int num2 = this.Hopping ? 1 : 0;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.Hopping, (int) this.Hopping_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      cursor += 7;
      if (this.FrequencyHopTable != null)
      {
        int length = this.FrequencyHopTable.Length;
        for (int index = 0; index < length; ++index)
          this.FrequencyHopTable[index].ToBitArray(ref bit_array, ref cursor);
      }
      if (this.FixedFrequencyTable != null)
        this.FixedFrequencyTable.ToBitArray(ref bit_array, ref cursor);
      if (this.tvCoding)
        return;
      Util.ConvertIntToBitArray((uint) (cursor - num1) / 8U, 16).CopyTo((Array) bit_array, num1 + 16);
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
      int num = this.Hopping ? 1 : 0;
      try
      {
        Util.AppendObjToBitArray((object) this.Hopping, (int) this.Hopping_len, bArr);
      }
      catch
      {
      }
      bArr.Length += 7;
      if (this.FrequencyHopTable != null)
      {
        int length3 = this.FrequencyHopTable.Length;
        for (int index = 0; index < length3; ++index)
          this.FrequencyHopTable[index].AppendToBitArray(bArr);
      }
      if (this.FixedFrequencyTable != null)
        this.FixedFrequencyTable.AppendToBitArray(bArr);
      if (this.tvCoding)
        return;
      BitArray bitArray = Util.ConvertIntToBitArray((uint) (bArr.Length - length1) / 8U, 16);
      for (int index = 0; index < bitArray.Length; ++index)
        bArr[length1 + 16 + index] = bitArray[index];
    }
  }
}
