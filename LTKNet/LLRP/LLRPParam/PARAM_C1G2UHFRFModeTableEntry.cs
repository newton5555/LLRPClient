// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_C1G2UHFRFModeTableEntry
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_C1G2UHFRFModeTableEntry : Parameter
  {
    public uint ModeIdentifier;
    private short ModeIdentifier_len;
    public ENUM_C1G2DRValue DRValue;
    private short DRValue_len = 1;
    public bool EPCHAGTCConformance;
    private short EPCHAGTCConformance_len;
    private const ushort param_reserved_len5 = 6;
    public ENUM_C1G2MValue MValue;
    private short MValue_len = 8;
    public ENUM_C1G2ForwardLinkModulation ForwardLinkModulation;
    private short ForwardLinkModulation_len = 8;
    public ENUM_C1G2SpectralMaskIndicator SpectralMaskIndicator;
    private short SpectralMaskIndicator_len = 8;
    public uint BDRValue;
    private short BDRValue_len;
    public uint PIEValue;
    private short PIEValue_len;
    public uint MinTariValue;
    private short MinTariValue_len;
    public uint MaxTariValue;
    private short MaxTariValue_len;
    public uint StepTariValue;
    private short StepTariValue_len;

    public PARAM_C1G2UHFRFModeTableEntry() => this.typeID = (ushort) 329;

    public static PARAM_C1G2UHFRFModeTableEntry FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_C1G2UHFRFModeTableEntry) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_C1G2UHFRFModeTableEntry uhfrfModeTableEntry = new PARAM_C1G2UHFRFModeTableEntry();
      uhfrfModeTableEntry.tvCoding = bit_array[cursor];
      int val;
      if (uhfrfModeTableEntry.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        uhfrfModeTableEntry.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) uhfrfModeTableEntry.length * 8;
      }
      if (val != (int) uhfrfModeTableEntry.TypeID)
      {
        cursor = num1;
        return (PARAM_C1G2UHFRFModeTableEntry) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 32;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len1);
      uhfrfModeTableEntry.ModeIdentifier = (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 1;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len2);
      uhfrfModeTableEntry.DRValue = (ENUM_C1G2DRValue) (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len3 = 1;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (bool), field_len3);
      uhfrfModeTableEntry.EPCHAGTCConformance = (bool) obj;
      cursor += 6;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len4 = 8;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len4);
      uhfrfModeTableEntry.MValue = (ENUM_C1G2MValue) (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len5 = 8;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len5);
      uhfrfModeTableEntry.ForwardLinkModulation = (ENUM_C1G2ForwardLinkModulation) (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len6 = 8;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len6);
      uhfrfModeTableEntry.SpectralMaskIndicator = (ENUM_C1G2SpectralMaskIndicator) (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len7 = 32;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len7);
      uhfrfModeTableEntry.BDRValue = (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len8 = 32;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len8);
      uhfrfModeTableEntry.PIEValue = (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len9 = 32;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len9);
      uhfrfModeTableEntry.MinTariValue = (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len10 = 32;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len10);
      uhfrfModeTableEntry.MaxTariValue = (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len11 = 32;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len11);
      uhfrfModeTableEntry.StepTariValue = (uint) obj;
      return uhfrfModeTableEntry;
    }

    public override string ToString()
    {
      string str = "<C1G2UHFRFModeTableEntry>" + "\r\n";
      int modeIdentifier = (int) this.ModeIdentifier;
      try
      {
        str = str + "  <ModeIdentifier>" + Util.ConvertValueTypeToString((object) this.ModeIdentifier, "u32", "") + "</ModeIdentifier>";
        str += "\r\n";
      }
      catch
      {
      }
      int drValue = (int) this.DRValue;
      try
      {
        str = str + "  <DRValue>" + this.DRValue.ToString() + "</DRValue>";
        str += "\r\n";
      }
      catch
      {
      }
      int num = this.EPCHAGTCConformance ? 1 : 0;
      try
      {
        str = str + "  <EPCHAGTCConformance>" + Util.ConvertValueTypeToString((object) this.EPCHAGTCConformance, "u1", "") + "</EPCHAGTCConformance>";
        str += "\r\n";
      }
      catch
      {
      }
      int mvalue = (int) this.MValue;
      try
      {
        str = str + "  <MValue>" + this.MValue.ToString() + "</MValue>";
        str += "\r\n";
      }
      catch
      {
      }
      int forwardLinkModulation = (int) this.ForwardLinkModulation;
      try
      {
        str = str + "  <ForwardLinkModulation>" + this.ForwardLinkModulation.ToString() + "</ForwardLinkModulation>";
        str += "\r\n";
      }
      catch
      {
      }
      int spectralMaskIndicator = (int) this.SpectralMaskIndicator;
      try
      {
        str = str + "  <SpectralMaskIndicator>" + this.SpectralMaskIndicator.ToString() + "</SpectralMaskIndicator>";
        str += "\r\n";
      }
      catch
      {
      }
      int bdrValue = (int) this.BDRValue;
      try
      {
        str = str + "  <BDRValue>" + Util.ConvertValueTypeToString((object) this.BDRValue, "u32", "") + "</BDRValue>";
        str += "\r\n";
      }
      catch
      {
      }
      int pieValue = (int) this.PIEValue;
      try
      {
        str = str + "  <PIEValue>" + Util.ConvertValueTypeToString((object) this.PIEValue, "u32", "") + "</PIEValue>";
        str += "\r\n";
      }
      catch
      {
      }
      int minTariValue = (int) this.MinTariValue;
      try
      {
        str = str + "  <MinTariValue>" + Util.ConvertValueTypeToString((object) this.MinTariValue, "u32", "") + "</MinTariValue>";
        str += "\r\n";
      }
      catch
      {
      }
      int maxTariValue = (int) this.MaxTariValue;
      try
      {
        str = str + "  <MaxTariValue>" + Util.ConvertValueTypeToString((object) this.MaxTariValue, "u32", "") + "</MaxTariValue>";
        str += "\r\n";
      }
      catch
      {
      }
      int stepTariValue = (int) this.StepTariValue;
      try
      {
        str = str + "  <StepTariValue>" + Util.ConvertValueTypeToString((object) this.StepTariValue, "u32", "") + "</StepTariValue>";
        str += "\r\n";
      }
      catch
      {
      }
      return str + "</C1G2UHFRFModeTableEntry>" + "\r\n";
    }

    public static PARAM_C1G2UHFRFModeTableEntry FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      namespaceManager.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      namespaceManager.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      return new PARAM_C1G2UHFRFModeTableEntry()
      {
        ModeIdentifier = (uint) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "ModeIdentifier"), "u32", ""),
        DRValue = (ENUM_C1G2DRValue) Enum.Parse(typeof (ENUM_C1G2DRValue), XmlUtil.GetNodeValue(node, "DRValue")),
        EPCHAGTCConformance = (bool) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "EPCHAGTCConformance"), "u1", ""),
        MValue = (ENUM_C1G2MValue) Enum.Parse(typeof (ENUM_C1G2MValue), XmlUtil.GetNodeValue(node, "MValue")),
        ForwardLinkModulation = (ENUM_C1G2ForwardLinkModulation) Enum.Parse(typeof (ENUM_C1G2ForwardLinkModulation), XmlUtil.GetNodeValue(node, "ForwardLinkModulation")),
        SpectralMaskIndicator = (ENUM_C1G2SpectralMaskIndicator) Enum.Parse(typeof (ENUM_C1G2SpectralMaskIndicator), XmlUtil.GetNodeValue(node, "SpectralMaskIndicator")),
        BDRValue = (uint) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "BDRValue"), "u32", ""),
        PIEValue = (uint) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "PIEValue"), "u32", ""),
        MinTariValue = (uint) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "MinTariValue"), "u32", ""),
        MaxTariValue = (uint) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "MaxTariValue"), "u32", ""),
        StepTariValue = (uint) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "StepTariValue"), "u32", "")
      };
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
      int modeIdentifier = (int) this.ModeIdentifier;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.ModeIdentifier, (int) this.ModeIdentifier_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int drValue = (int) this.DRValue;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.DRValue, (int) this.DRValue_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int num2 = this.EPCHAGTCConformance ? 1 : 0;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.EPCHAGTCConformance, (int) this.EPCHAGTCConformance_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      cursor += 6;
      int mvalue = (int) this.MValue;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.MValue, (int) this.MValue_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int forwardLinkModulation = (int) this.ForwardLinkModulation;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.ForwardLinkModulation, (int) this.ForwardLinkModulation_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int spectralMaskIndicator = (int) this.SpectralMaskIndicator;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.SpectralMaskIndicator, (int) this.SpectralMaskIndicator_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int bdrValue = (int) this.BDRValue;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.BDRValue, (int) this.BDRValue_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int pieValue = (int) this.PIEValue;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.PIEValue, (int) this.PIEValue_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int minTariValue = (int) this.MinTariValue;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.MinTariValue, (int) this.MinTariValue_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int maxTariValue = (int) this.MaxTariValue;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.MaxTariValue, (int) this.MaxTariValue_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int stepTariValue = (int) this.StepTariValue;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.StepTariValue, (int) this.StepTariValue_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
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
      int modeIdentifier = (int) this.ModeIdentifier;
      try
      {
        Util.AppendObjToBitArray((object) this.ModeIdentifier, (int) this.ModeIdentifier_len, bArr);
      }
      catch
      {
      }
      int drValue = (int) this.DRValue;
      try
      {
        Util.AppendObjToBitArray((object) this.DRValue, (int) this.DRValue_len, bArr);
      }
      catch
      {
      }
      int num = this.EPCHAGTCConformance ? 1 : 0;
      try
      {
        Util.AppendObjToBitArray((object) this.EPCHAGTCConformance, (int) this.EPCHAGTCConformance_len, bArr);
      }
      catch
      {
      }
      bArr.Length += 6;
      int mvalue = (int) this.MValue;
      try
      {
        Util.AppendObjToBitArray((object) this.MValue, (int) this.MValue_len, bArr);
      }
      catch
      {
      }
      int forwardLinkModulation = (int) this.ForwardLinkModulation;
      try
      {
        Util.AppendObjToBitArray((object) this.ForwardLinkModulation, (int) this.ForwardLinkModulation_len, bArr);
      }
      catch
      {
      }
      int spectralMaskIndicator = (int) this.SpectralMaskIndicator;
      try
      {
        Util.AppendObjToBitArray((object) this.SpectralMaskIndicator, (int) this.SpectralMaskIndicator_len, bArr);
      }
      catch
      {
      }
      int bdrValue = (int) this.BDRValue;
      try
      {
        Util.AppendObjToBitArray((object) this.BDRValue, (int) this.BDRValue_len, bArr);
      }
      catch
      {
      }
      int pieValue = (int) this.PIEValue;
      try
      {
        Util.AppendObjToBitArray((object) this.PIEValue, (int) this.PIEValue_len, bArr);
      }
      catch
      {
      }
      int minTariValue = (int) this.MinTariValue;
      try
      {
        Util.AppendObjToBitArray((object) this.MinTariValue, (int) this.MinTariValue_len, bArr);
      }
      catch
      {
      }
      int maxTariValue = (int) this.MaxTariValue;
      try
      {
        Util.AppendObjToBitArray((object) this.MaxTariValue, (int) this.MaxTariValue_len, bArr);
      }
      catch
      {
      }
      int stepTariValue = (int) this.StepTariValue;
      try
      {
        Util.AppendObjToBitArray((object) this.StepTariValue, (int) this.StepTariValue_len, bArr);
      }
      catch
      {
      }
      if (this.tvCoding)
        return;
      BitArray bitArray = Util.ConvertIntToBitArray((uint) (bArr.Length - length1) / 8U, 16);
      for (int index = 0; index < bitArray.Length; ++index)
        bArr[length1 + 16 + index] = bitArray[index];
    }
  }
}
