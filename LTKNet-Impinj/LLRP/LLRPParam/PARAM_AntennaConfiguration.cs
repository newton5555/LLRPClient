// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_AntennaConfiguration
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_AntennaConfiguration : Parameter
  {
    public ushort AntennaID;
    private short AntennaID_len;
    public PARAM_RFReceiver RFReceiver;
    public PARAM_RFTransmitter RFTransmitter;
    public UNION_AirProtocolInventoryCommandSettings AirProtocolInventoryCommandSettings = new UNION_AirProtocolInventoryCommandSettings();

    public PARAM_AntennaConfiguration() => this.typeID = (ushort) 222;

    public static PARAM_AntennaConfiguration FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_AntennaConfiguration) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_AntennaConfiguration antennaConfiguration = new PARAM_AntennaConfiguration();
      antennaConfiguration.tvCoding = bit_array[cursor];
      int val1;
      if (antennaConfiguration.tvCoding)
      {
        ++cursor;
        val1 = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val1 = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        antennaConfiguration.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) antennaConfiguration.length * 8;
      }
      if (val1 != (int) antennaConfiguration.TypeID)
      {
        cursor = num1;
        return (PARAM_AntennaConfiguration) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len = 16;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len);
      antennaConfiguration.AntennaID = (ushort) obj;
      antennaConfiguration.RFReceiver = PARAM_RFReceiver.FromBitArray(ref bit_array, ref cursor, length);
      antennaConfiguration.RFTransmitter = PARAM_RFTransmitter.FromBitArray(ref bit_array, ref cursor, length);
      ushort num3 = 1;
      while (num3 != (ushort) 0)
      {
        num3 = (ushort) 0;
        PARAM_C1G2InventoryCommand val2 = PARAM_C1G2InventoryCommand.FromBitArray(ref bit_array, ref cursor, length);
        if (val2 != null)
        {
          ++num3;
          antennaConfiguration.AirProtocolInventoryCommandSettings.Add((IParameter) val2);
        }
      }
      return antennaConfiguration;
    }

    public override string ToString()
    {
      string str = "<AntennaConfiguration>" + "\r\n";
      int antennaId = (int) this.AntennaID;
      try
      {
        str = str + "  <AntennaID>" + Util.ConvertValueTypeToString((object) this.AntennaID, "u16", "") + "</AntennaID>";
        str += "\r\n";
      }
      catch
      {
      }
      if (this.RFReceiver != null)
        str += Util.Indent(this.RFReceiver.ToString());
      if (this.RFTransmitter != null)
        str += Util.Indent(this.RFTransmitter.ToString());
      if (this.AirProtocolInventoryCommandSettings != null)
      {
        int count = this.AirProtocolInventoryCommandSettings.Count;
        for (int index = 0; index < count; ++index)
          str += Util.Indent(this.AirProtocolInventoryCommandSettings[index].ToString());
      }
      return str + "</AntennaConfiguration>" + "\r\n";
    }

    public static PARAM_AntennaConfiguration FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      nsmgr.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      nsmgr.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      PARAM_AntennaConfiguration antennaConfiguration = new PARAM_AntennaConfiguration();
      string nodeValue = XmlUtil.GetNodeValue(node, "AntennaID");
      antennaConfiguration.AntennaID = (ushort) Util.ParseValueTypeFromString(nodeValue, "u16", "");
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "RFReceiver", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            antennaConfiguration.RFReceiver = PARAM_RFReceiver.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "RFTransmitter", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            antennaConfiguration.RFTransmitter = PARAM_RFTransmitter.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      antennaConfiguration.AirProtocolInventoryCommandSettings = new UNION_AirProtocolInventoryCommandSettings();
      try
      {
        foreach (XmlNode childNode in node.ChildNodes)
        {
          if (childNode.Name == "C1G2InventoryCommand")
            antennaConfiguration.AirProtocolInventoryCommandSettings.Add((IParameter) PARAM_C1G2InventoryCommand.FromXmlNode(childNode));
        }
      }
      catch
      {
      }
      return antennaConfiguration;
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
      int antennaId = (int) this.AntennaID;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.AntennaID, (int) this.AntennaID_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      if (this.RFReceiver != null)
        this.RFReceiver.ToBitArray(ref bit_array, ref cursor);
      if (this.RFTransmitter != null)
        this.RFTransmitter.ToBitArray(ref bit_array, ref cursor);
      int count = this.AirProtocolInventoryCommandSettings.Count;
      for (int index = 0; index < count; ++index)
        this.AirProtocolInventoryCommandSettings[index].ToBitArray(ref bit_array, ref cursor);
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
      int antennaId = (int) this.AntennaID;
      try
      {
        Util.AppendObjToBitArray((object) this.AntennaID, (int) this.AntennaID_len, bArr);
      }
      catch
      {
      }
      if (this.RFReceiver != null)
        this.RFReceiver.AppendToBitArray(bArr);
      if (this.RFTransmitter != null)
        this.RFTransmitter.AppendToBitArray(bArr);
      int count = this.AirProtocolInventoryCommandSettings.Count;
      for (int index = 0; index < count; ++index)
        this.AirProtocolInventoryCommandSettings[index].AppendToBitArray(bArr);
      if (this.tvCoding)
        return;
      BitArray bitArray = Util.ConvertIntToBitArray((uint) (bArr.Length - length1) / 8U, 16);
      for (int index = 0; index < bitArray.Length; ++index)
        bArr[length1 + 16 + index] = bitArray[index];
    }
  }
}
