// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_InventoryParameterSpec
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_InventoryParameterSpec : Parameter
  {
    public ushort InventoryParameterSpecID;
    private short InventoryParameterSpecID_len;
    public ENUM_AirProtocols ProtocolID;
    private short ProtocolID_len = 8;
    public PARAM_AntennaConfiguration[] AntennaConfiguration;
    public readonly CustomParameterArrayList Custom = new CustomParameterArrayList();

    public PARAM_InventoryParameterSpec() => this.typeID = (ushort) 186;

    public bool AddCustomParameter(ICustom_Parameter param)
    {
      if (param is IInventoryParameterSpec_Custom_Param)
      {
        this.Custom.Add(param);
        return true;
      }
      if (!(param.GetType() == typeof (PARAM_Custom)))
        return false;
      this.Custom.Add(param);
      return true;
    }

    public static PARAM_InventoryParameterSpec FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_InventoryParameterSpec) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList1 = new ArrayList();
      PARAM_InventoryParameterSpec inventoryParameterSpec = new PARAM_InventoryParameterSpec();
      inventoryParameterSpec.tvCoding = bit_array[cursor];
      int val;
      if (inventoryParameterSpec.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        inventoryParameterSpec.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) inventoryParameterSpec.length * 8;
      }
      if (val != (int) inventoryParameterSpec.TypeID)
      {
        cursor = num1;
        return (PARAM_InventoryParameterSpec) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 16;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len1);
      inventoryParameterSpec.InventoryParameterSpecID = (ushort) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 8;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len2);
      inventoryParameterSpec.ProtocolID = (ENUM_AirProtocols) (uint) obj;
      ArrayList arrayList2 = new ArrayList();
      PARAM_AntennaConfiguration antennaConfiguration;
      while ((antennaConfiguration = PARAM_AntennaConfiguration.FromBitArray(ref bit_array, ref cursor, length)) != null)
        arrayList2.Add((object) antennaConfiguration);
      if (arrayList2.Count > 0)
      {
        inventoryParameterSpec.AntennaConfiguration = new PARAM_AntennaConfiguration[arrayList2.Count];
        for (int index = 0; index < arrayList2.Count; ++index)
          inventoryParameterSpec.AntennaConfiguration[index] = (PARAM_AntennaConfiguration) arrayList2[index];
      }
      while (cursor < num2)
      {
        int num3 = cursor;
        ICustom_Parameter customParameter = CustomParamDecodeFactory.DecodeCustomParameter(ref bit_array, ref cursor, length);
        if (customParameter != null && cursor <= num2 && !inventoryParameterSpec.AddCustomParameter(customParameter))
        {
          cursor = num3;
          break;
        }
      }
      return inventoryParameterSpec;
    }

    public override string ToString()
    {
      string str = "<InventoryParameterSpec>" + "\r\n";
      int inventoryParameterSpecId = (int) this.InventoryParameterSpecID;
      try
      {
        str = str + "  <InventoryParameterSpecID>" + Util.ConvertValueTypeToString((object) this.InventoryParameterSpecID, "u16", "") + "</InventoryParameterSpecID>";
        str += "\r\n";
      }
      catch
      {
      }
      int protocolId = (int) this.ProtocolID;
      try
      {
        str = str + "  <ProtocolID>" + this.ProtocolID.ToString() + "</ProtocolID>";
        str += "\r\n";
      }
      catch
      {
      }
      if (this.AntennaConfiguration != null)
      {
        int length = this.AntennaConfiguration.Length;
        for (int index = 0; index < length; ++index)
          str += Util.Indent(this.AntennaConfiguration[index].ToString());
      }
      if (this.Custom != null)
      {
        int length = this.Custom.Length;
        for (int index = 0; index < length; ++index)
          str += Util.Indent(this.Custom[index].ToString());
      }
      return str + "</InventoryParameterSpec>" + "\r\n";
    }

    public static PARAM_InventoryParameterSpec FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      nsmgr.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      nsmgr.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      PARAM_InventoryParameterSpec inventoryParameterSpec = new PARAM_InventoryParameterSpec();
      string nodeValue1 = XmlUtil.GetNodeValue(node, "InventoryParameterSpecID");
      inventoryParameterSpec.InventoryParameterSpecID = (ushort) Util.ParseValueTypeFromString(nodeValue1, "u16", "");
      string nodeValue2 = XmlUtil.GetNodeValue(node, "ProtocolID");
      inventoryParameterSpec.ProtocolID = (ENUM_AirProtocols) Enum.Parse(typeof (ENUM_AirProtocols), nodeValue2);
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "AntennaConfiguration", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
          {
            inventoryParameterSpec.AntennaConfiguration = new PARAM_AntennaConfiguration[xmlNodes.Count];
            for (int i = 0; i < xmlNodes.Count; ++i)
              inventoryParameterSpec.AntennaConfiguration[i] = PARAM_AntennaConfiguration.FromXmlNode(xmlNodes[i]);
          }
        }
      }
      catch
      {
      }
      try
      {
        ArrayList nodeCustomChildren = XmlUtil.GetXmlNodeCustomChildren(node, nsmgr);
        if (nodeCustomChildren != null)
        {
          for (int index = 0; index < nodeCustomChildren.Count; ++index)
          {
            if (!arrayList.Contains(nodeCustomChildren[index]))
            {
              ICustom_Parameter customParameter = CustomParamDecodeFactory.DecodeXmlNodeToCustomParameter((XmlNode) nodeCustomChildren[index]);
              if (customParameter != null && inventoryParameterSpec.AddCustomParameter(customParameter))
                arrayList.Add(nodeCustomChildren[index]);
            }
          }
        }
      }
      catch
      {
      }
      return inventoryParameterSpec;
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
      int inventoryParameterSpecId = (int) this.InventoryParameterSpecID;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.InventoryParameterSpecID, (int) this.InventoryParameterSpecID_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int protocolId = (int) this.ProtocolID;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.ProtocolID, (int) this.ProtocolID_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      if (this.AntennaConfiguration != null)
      {
        int length = this.AntennaConfiguration.Length;
        for (int index = 0; index < length; ++index)
          this.AntennaConfiguration[index].ToBitArray(ref bit_array, ref cursor);
      }
      if (this.Custom != null)
      {
        int length = this.Custom.Length;
        for (int index = 0; index < length; ++index)
          this.Custom[index].ToBitArray(ref bit_array, ref cursor);
      }
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
      int inventoryParameterSpecId = (int) this.InventoryParameterSpecID;
      try
      {
        Util.AppendObjToBitArray((object) this.InventoryParameterSpecID, (int) this.InventoryParameterSpecID_len, bArr);
      }
      catch
      {
      }
      int protocolId = (int) this.ProtocolID;
      try
      {
        Util.AppendObjToBitArray((object) this.ProtocolID, (int) this.ProtocolID_len, bArr);
      }
      catch
      {
      }
      if (this.AntennaConfiguration != null)
      {
        int length3 = this.AntennaConfiguration.Length;
        for (int index = 0; index < length3; ++index)
          this.AntennaConfiguration[index].AppendToBitArray(bArr);
      }
      if (this.Custom != null)
      {
        int length4 = this.Custom.Length;
        for (int index = 0; index < length4; ++index)
          this.Custom[index].AppendToBitArray(bArr);
      }
      if (this.tvCoding)
        return;
      BitArray bitArray = Util.ConvertIntToBitArray((uint) (bArr.Length - length1) / 8U, 16);
      for (int index = 0; index < bitArray.Length; ++index)
        bArr[length1 + 16 + index] = bitArray[index];
    }
  }
}
