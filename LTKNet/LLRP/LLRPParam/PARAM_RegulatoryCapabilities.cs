// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_RegulatoryCapabilities
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_RegulatoryCapabilities : Parameter
  {
    public ushort CountryCode;
    private short CountryCode_len;
    public ENUM_CommunicationsStandard CommunicationsStandard;
    private short CommunicationsStandard_len = 16;
    public PARAM_UHFBandCapabilities UHFBandCapabilities;
    public readonly CustomParameterArrayList Custom = new CustomParameterArrayList();

    public PARAM_RegulatoryCapabilities() => this.typeID = (ushort) 143;

    public bool AddCustomParameter(ICustom_Parameter param)
    {
      if (param is IRegulatoryCapabilities_Custom_Param)
      {
        this.Custom.Add(param);
        return true;
      }
      if (!(param.GetType() == typeof (PARAM_Custom)))
        return false;
      this.Custom.Add(param);
      return true;
    }

    public static PARAM_RegulatoryCapabilities FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_RegulatoryCapabilities) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_RegulatoryCapabilities regulatoryCapabilities = new PARAM_RegulatoryCapabilities();
      regulatoryCapabilities.tvCoding = bit_array[cursor];
      int val;
      if (regulatoryCapabilities.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        regulatoryCapabilities.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) regulatoryCapabilities.length * 8;
      }
      if (val != (int) regulatoryCapabilities.TypeID)
      {
        cursor = num1;
        return (PARAM_RegulatoryCapabilities) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 16;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len1);
      regulatoryCapabilities.CountryCode = (ushort) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 16;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len2);
      regulatoryCapabilities.CommunicationsStandard = (ENUM_CommunicationsStandard) (uint) obj;
      regulatoryCapabilities.UHFBandCapabilities = PARAM_UHFBandCapabilities.FromBitArray(ref bit_array, ref cursor, length);
      while (cursor < num2)
      {
        int num3 = cursor;
        ICustom_Parameter customParameter = CustomParamDecodeFactory.DecodeCustomParameter(ref bit_array, ref cursor, length);
        if (customParameter != null && cursor <= num2 && !regulatoryCapabilities.AddCustomParameter(customParameter))
        {
          cursor = num3;
          break;
        }
      }
      return regulatoryCapabilities;
    }

    public override string ToString()
    {
      string str = "<RegulatoryCapabilities>" + "\r\n";
      int countryCode = (int) this.CountryCode;
      try
      {
        str = str + "  <CountryCode>" + Util.ConvertValueTypeToString((object) this.CountryCode, "u16", "") + "</CountryCode>";
        str += "\r\n";
      }
      catch
      {
      }
      int communicationsStandard = (int) this.CommunicationsStandard;
      try
      {
        str = str + "  <CommunicationsStandard>" + this.CommunicationsStandard.ToString() + "</CommunicationsStandard>";
        str += "\r\n";
      }
      catch
      {
      }
      if (this.UHFBandCapabilities != null)
        str += Util.Indent(this.UHFBandCapabilities.ToString());
      if (this.Custom != null)
      {
        int length = this.Custom.Length;
        for (int index = 0; index < length; ++index)
          str += Util.Indent(this.Custom[index].ToString());
      }
      return str + "</RegulatoryCapabilities>" + "\r\n";
    }

    public static PARAM_RegulatoryCapabilities FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      nsmgr.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      nsmgr.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      PARAM_RegulatoryCapabilities regulatoryCapabilities = new PARAM_RegulatoryCapabilities();
      string nodeValue1 = XmlUtil.GetNodeValue(node, "CountryCode");
      regulatoryCapabilities.CountryCode = (ushort) Util.ParseValueTypeFromString(nodeValue1, "u16", "");
      string nodeValue2 = XmlUtil.GetNodeValue(node, "CommunicationsStandard");
      regulatoryCapabilities.CommunicationsStandard = (ENUM_CommunicationsStandard) Enum.Parse(typeof (ENUM_CommunicationsStandard), nodeValue2);
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "UHFBandCapabilities", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            regulatoryCapabilities.UHFBandCapabilities = PARAM_UHFBandCapabilities.FromXmlNode(xmlNodes[0]);
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
              if (customParameter != null && regulatoryCapabilities.AddCustomParameter(customParameter))
                arrayList.Add(nodeCustomChildren[index]);
            }
          }
        }
      }
      catch
      {
      }
      return regulatoryCapabilities;
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
      int countryCode = (int) this.CountryCode;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.CountryCode, (int) this.CountryCode_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int communicationsStandard = (int) this.CommunicationsStandard;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.CommunicationsStandard, (int) this.CommunicationsStandard_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      if (this.UHFBandCapabilities != null)
        this.UHFBandCapabilities.ToBitArray(ref bit_array, ref cursor);
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
      int countryCode = (int) this.CountryCode;
      try
      {
        Util.AppendObjToBitArray((object) this.CountryCode, (int) this.CountryCode_len, bArr);
      }
      catch
      {
      }
      int communicationsStandard = (int) this.CommunicationsStandard;
      try
      {
        Util.AppendObjToBitArray((object) this.CommunicationsStandard, (int) this.CommunicationsStandard_len, bArr);
      }
      catch
      {
      }
      if (this.UHFBandCapabilities != null)
        this.UHFBandCapabilities.AppendToBitArray(bArr);
      if (this.Custom != null)
      {
        int length3 = this.Custom.Length;
        for (int index = 0; index < length3; ++index)
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
