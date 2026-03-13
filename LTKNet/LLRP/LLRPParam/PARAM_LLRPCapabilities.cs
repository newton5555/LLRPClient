// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_LLRPCapabilities
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{

    //功能开关（bool）
    //CanDoRFSurvey：是否支持 RF Survey。
    //CanReportBufferFillWarning：是否支持上报缓冲区接近满的告警。
    //SupportsClientRequestOpSpec：是否支持 ClientRequest OpSpec。
    //CanDoTagInventoryStateAwareSingulation：是否支持状态感知盘点（State-Aware Singulation）。
    //SupportsEventAndReportHolding：是否支持事件/报告保持（先缓存再释放）。

    //能力参数
    //MaxNumPriorityLevelsSupported：支持的优先级层级数。
    //ClientRequestOpSpecTimeout：ClientRequest OpSpec 的超时能力参数（单位按协议定义）。

    //数量上限
    //MaxNumROSpecs：ROSpec 最大数量。
    //MaxNumAccessSpecs：AccessSpec(AO) 最大数量。
    //MaxNumSpecsPerROSpec：每个 ROSpec 可挂的 Spec（如 AISpec）最大数量。
    //MaxNumInventoryParameterSpecsPerAISpec：每个 AISpec 下 InventoryParameterSpec 最大数量（你说的 AI 相关上限）。
    //MaxNumOpSpecsPerAccessSpec：每个 AccessSpec 下 OpSpec 最大数量。
  public class PARAM_LLRPCapabilities : Parameter
  {
    public bool CanDoRFSurvey;
    private short CanDoRFSurvey_len;
    public bool CanReportBufferFillWarning;
    private short CanReportBufferFillWarning_len;
    public bool SupportsClientRequestOpSpec;
    private short SupportsClientRequestOpSpec_len;
    public bool CanDoTagInventoryStateAwareSingulation;
    private short CanDoTagInventoryStateAwareSingulation_len;
    public bool SupportsEventAndReportHolding;
    private short SupportsEventAndReportHolding_len;
    private const ushort param_reserved_len7 = 3;
    public byte MaxNumPriorityLevelsSupported;
    private short MaxNumPriorityLevelsSupported_len;
    public ushort ClientRequestOpSpecTimeout;
    private short ClientRequestOpSpecTimeout_len;
    public uint MaxNumROSpecs;
    private short MaxNumROSpecs_len;
    public uint MaxNumSpecsPerROSpec;
    private short MaxNumSpecsPerROSpec_len;
    public uint MaxNumInventoryParameterSpecsPerAISpec;
    private short MaxNumInventoryParameterSpecsPerAISpec_len;
    public uint MaxNumAccessSpecs;
    private short MaxNumAccessSpecs_len;
    public uint MaxNumOpSpecsPerAccessSpec;
    private short MaxNumOpSpecsPerAccessSpec_len;

    public PARAM_LLRPCapabilities() => this.typeID = (ushort) 142;

    public static PARAM_LLRPCapabilities FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_LLRPCapabilities) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_LLRPCapabilities llrpCapabilities = new PARAM_LLRPCapabilities();
      llrpCapabilities.tvCoding = bit_array[cursor];
      int val;
      if (llrpCapabilities.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        llrpCapabilities.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) llrpCapabilities.length * 8;
      }
      if (val != (int) llrpCapabilities.TypeID)
      {
        cursor = num1;
        return (PARAM_LLRPCapabilities) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 1;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (bool), field_len1);
      llrpCapabilities.CanDoRFSurvey = (bool) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 1;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (bool), field_len2);
      llrpCapabilities.CanReportBufferFillWarning = (bool) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len3 = 1;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (bool), field_len3);
      llrpCapabilities.SupportsClientRequestOpSpec = (bool) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len4 = 1;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (bool), field_len4);
      llrpCapabilities.CanDoTagInventoryStateAwareSingulation = (bool) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len5 = 1;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (bool), field_len5);
      llrpCapabilities.SupportsEventAndReportHolding = (bool) obj;
      cursor += 3;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len6 = 8;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (byte), field_len6);
      llrpCapabilities.MaxNumPriorityLevelsSupported = (byte) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len7 = 16;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len7);
      llrpCapabilities.ClientRequestOpSpecTimeout = (ushort) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len8 = 32;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len8);
      llrpCapabilities.MaxNumROSpecs = (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len9 = 32;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len9);
      llrpCapabilities.MaxNumSpecsPerROSpec = (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len10 = 32;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len10);
      llrpCapabilities.MaxNumInventoryParameterSpecsPerAISpec = (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len11 = 32;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len11);
      llrpCapabilities.MaxNumAccessSpecs = (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len12 = 32;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len12);
      llrpCapabilities.MaxNumOpSpecsPerAccessSpec = (uint) obj;
      return llrpCapabilities;
    }

    public override string ToString()
    {
      string str = "<LLRPCapabilities>" + "\r\n";
      int num1 = this.CanDoRFSurvey ? 1 : 0;
      try
      {
        str = str + "  <CanDoRFSurvey>" + Util.ConvertValueTypeToString((object) this.CanDoRFSurvey, "u1", "") + "</CanDoRFSurvey>";
        str += "\r\n";
      }
      catch
      {
      }
      int num2 = this.CanReportBufferFillWarning ? 1 : 0;
      try
      {
        str = str + "  <CanReportBufferFillWarning>" + Util.ConvertValueTypeToString((object) this.CanReportBufferFillWarning, "u1", "") + "</CanReportBufferFillWarning>";
        str += "\r\n";
      }
      catch
      {
      }
      int num3 = this.SupportsClientRequestOpSpec ? 1 : 0;
      try
      {
        str = str + "  <SupportsClientRequestOpSpec>" + Util.ConvertValueTypeToString((object) this.SupportsClientRequestOpSpec, "u1", "") + "</SupportsClientRequestOpSpec>";
        str += "\r\n";
      }
      catch
      {
      }
      int num4 = this.CanDoTagInventoryStateAwareSingulation ? 1 : 0;
      try
      {
        str = str + "  <CanDoTagInventoryStateAwareSingulation>" + Util.ConvertValueTypeToString((object) this.CanDoTagInventoryStateAwareSingulation, "u1", "") + "</CanDoTagInventoryStateAwareSingulation>";
        str += "\r\n";
      }
      catch
      {
      }
      int num5 = this.SupportsEventAndReportHolding ? 1 : 0;
      try
      {
        str = str + "  <SupportsEventAndReportHolding>" + Util.ConvertValueTypeToString((object) this.SupportsEventAndReportHolding, "u1", "") + "</SupportsEventAndReportHolding>";
        str += "\r\n";
      }
      catch
      {
      }
      int priorityLevelsSupported = (int) this.MaxNumPriorityLevelsSupported;
      try
      {
        str = str + "  <MaxNumPriorityLevelsSupported>" + Util.ConvertValueTypeToString((object) this.MaxNumPriorityLevelsSupported, "u8", "") + "</MaxNumPriorityLevelsSupported>";
        str += "\r\n";
      }
      catch
      {
      }
      int requestOpSpecTimeout = (int) this.ClientRequestOpSpecTimeout;
      try
      {
        str = str + "  <ClientRequestOpSpecTimeout>" + Util.ConvertValueTypeToString((object) this.ClientRequestOpSpecTimeout, "u16", "") + "</ClientRequestOpSpecTimeout>";
        str += "\r\n";
      }
      catch
      {
      }
      int maxNumRoSpecs = (int) this.MaxNumROSpecs;
      try
      {
        str = str + "  <MaxNumROSpecs>" + Util.ConvertValueTypeToString((object) this.MaxNumROSpecs, "u32", "") + "</MaxNumROSpecs>";
        str += "\r\n";
      }
      catch
      {
      }
      int numSpecsPerRoSpec = (int) this.MaxNumSpecsPerROSpec;
      try
      {
        str = str + "  <MaxNumSpecsPerROSpec>" + Util.ConvertValueTypeToString((object) this.MaxNumSpecsPerROSpec, "u32", "") + "</MaxNumSpecsPerROSpec>";
        str += "\r\n";
      }
      catch
      {
      }
      int parameterSpecsPerAiSpec = (int) this.MaxNumInventoryParameterSpecsPerAISpec;
      try
      {
        str = str + "  <MaxNumInventoryParameterSpecsPerAISpec>" + Util.ConvertValueTypeToString((object) this.MaxNumInventoryParameterSpecsPerAISpec, "u32", "") + "</MaxNumInventoryParameterSpecsPerAISpec>";
        str += "\r\n";
      }
      catch
      {
      }
      int maxNumAccessSpecs = (int) this.MaxNumAccessSpecs;
      try
      {
        str = str + "  <MaxNumAccessSpecs>" + Util.ConvertValueTypeToString((object) this.MaxNumAccessSpecs, "u32", "") + "</MaxNumAccessSpecs>";
        str += "\r\n";
      }
      catch
      {
      }
      int specsPerAccessSpec = (int) this.MaxNumOpSpecsPerAccessSpec;
      try
      {
        str = str + "  <MaxNumOpSpecsPerAccessSpec>" + Util.ConvertValueTypeToString((object) this.MaxNumOpSpecsPerAccessSpec, "u32", "") + "</MaxNumOpSpecsPerAccessSpec>";
        str += "\r\n";
      }
      catch
      {
      }
      return str + "</LLRPCapabilities>" + "\r\n";
    }

    public static PARAM_LLRPCapabilities FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      namespaceManager.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      namespaceManager.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      return new PARAM_LLRPCapabilities()
      {
        CanDoRFSurvey = (bool) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "CanDoRFSurvey"), "u1", ""),
        CanReportBufferFillWarning = (bool) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "CanReportBufferFillWarning"), "u1", ""),
        SupportsClientRequestOpSpec = (bool) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "SupportsClientRequestOpSpec"), "u1", ""),
        CanDoTagInventoryStateAwareSingulation = (bool) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "CanDoTagInventoryStateAwareSingulation"), "u1", ""),
        SupportsEventAndReportHolding = (bool) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "SupportsEventAndReportHolding"), "u1", ""),
        MaxNumPriorityLevelsSupported = (byte) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "MaxNumPriorityLevelsSupported"), "u8", ""),
        ClientRequestOpSpecTimeout = (ushort) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "ClientRequestOpSpecTimeout"), "u16", ""),
        MaxNumROSpecs = (uint) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "MaxNumROSpecs"), "u32", ""),
        MaxNumSpecsPerROSpec = (uint) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "MaxNumSpecsPerROSpec"), "u32", ""),
        MaxNumInventoryParameterSpecsPerAISpec = (uint) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "MaxNumInventoryParameterSpecsPerAISpec"), "u32", ""),
        MaxNumAccessSpecs = (uint) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "MaxNumAccessSpecs"), "u32", ""),
        MaxNumOpSpecsPerAccessSpec = (uint) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "MaxNumOpSpecsPerAccessSpec"), "u32", "")
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
      int num2 = this.CanDoRFSurvey ? 1 : 0;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.CanDoRFSurvey, (int) this.CanDoRFSurvey_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int num3 = this.CanReportBufferFillWarning ? 1 : 0;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.CanReportBufferFillWarning, (int) this.CanReportBufferFillWarning_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int num4 = this.SupportsClientRequestOpSpec ? 1 : 0;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.SupportsClientRequestOpSpec, (int) this.SupportsClientRequestOpSpec_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int num5 = this.CanDoTagInventoryStateAwareSingulation ? 1 : 0;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.CanDoTagInventoryStateAwareSingulation, (int) this.CanDoTagInventoryStateAwareSingulation_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int num6 = this.SupportsEventAndReportHolding ? 1 : 0;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.SupportsEventAndReportHolding, (int) this.SupportsEventAndReportHolding_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      cursor += 3;
      int priorityLevelsSupported = (int) this.MaxNumPriorityLevelsSupported;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.MaxNumPriorityLevelsSupported, (int) this.MaxNumPriorityLevelsSupported_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int requestOpSpecTimeout = (int) this.ClientRequestOpSpecTimeout;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.ClientRequestOpSpecTimeout, (int) this.ClientRequestOpSpecTimeout_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int maxNumRoSpecs = (int) this.MaxNumROSpecs;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.MaxNumROSpecs, (int) this.MaxNumROSpecs_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int numSpecsPerRoSpec = (int) this.MaxNumSpecsPerROSpec;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.MaxNumSpecsPerROSpec, (int) this.MaxNumSpecsPerROSpec_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int parameterSpecsPerAiSpec = (int) this.MaxNumInventoryParameterSpecsPerAISpec;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.MaxNumInventoryParameterSpecsPerAISpec, (int) this.MaxNumInventoryParameterSpecsPerAISpec_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int maxNumAccessSpecs = (int) this.MaxNumAccessSpecs;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.MaxNumAccessSpecs, (int) this.MaxNumAccessSpecs_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int specsPerAccessSpec = (int) this.MaxNumOpSpecsPerAccessSpec;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.MaxNumOpSpecsPerAccessSpec, (int) this.MaxNumOpSpecsPerAccessSpec_len);
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
      int num1 = this.CanDoRFSurvey ? 1 : 0;
      try
      {
        Util.AppendObjToBitArray((object) this.CanDoRFSurvey, (int) this.CanDoRFSurvey_len, bArr);
      }
      catch
      {
      }
      int num2 = this.CanReportBufferFillWarning ? 1 : 0;
      try
      {
        Util.AppendObjToBitArray((object) this.CanReportBufferFillWarning, (int) this.CanReportBufferFillWarning_len, bArr);
      }
      catch
      {
      }
      int num3 = this.SupportsClientRequestOpSpec ? 1 : 0;
      try
      {
        Util.AppendObjToBitArray((object) this.SupportsClientRequestOpSpec, (int) this.SupportsClientRequestOpSpec_len, bArr);
      }
      catch
      {
      }
      int num4 = this.CanDoTagInventoryStateAwareSingulation ? 1 : 0;
      try
      {
        Util.AppendObjToBitArray((object) this.CanDoTagInventoryStateAwareSingulation, (int) this.CanDoTagInventoryStateAwareSingulation_len, bArr);
      }
      catch
      {
      }
      int num5 = this.SupportsEventAndReportHolding ? 1 : 0;
      try
      {
        Util.AppendObjToBitArray((object) this.SupportsEventAndReportHolding, (int) this.SupportsEventAndReportHolding_len, bArr);
      }
      catch
      {
      }
      bArr.Length += 3;
      int priorityLevelsSupported = (int) this.MaxNumPriorityLevelsSupported;
      try
      {
        Util.AppendObjToBitArray((object) this.MaxNumPriorityLevelsSupported, (int) this.MaxNumPriorityLevelsSupported_len, bArr);
      }
      catch
      {
      }
      int requestOpSpecTimeout = (int) this.ClientRequestOpSpecTimeout;
      try
      {
        Util.AppendObjToBitArray((object) this.ClientRequestOpSpecTimeout, (int) this.ClientRequestOpSpecTimeout_len, bArr);
      }
      catch
      {
      }
      int maxNumRoSpecs = (int) this.MaxNumROSpecs;
      try
      {
        Util.AppendObjToBitArray((object) this.MaxNumROSpecs, (int) this.MaxNumROSpecs_len, bArr);
      }
      catch
      {
      }
      int numSpecsPerRoSpec = (int) this.MaxNumSpecsPerROSpec;
      try
      {
        Util.AppendObjToBitArray((object) this.MaxNumSpecsPerROSpec, (int) this.MaxNumSpecsPerROSpec_len, bArr);
      }
      catch
      {
      }
      int parameterSpecsPerAiSpec = (int) this.MaxNumInventoryParameterSpecsPerAISpec;
      try
      {
        Util.AppendObjToBitArray((object) this.MaxNumInventoryParameterSpecsPerAISpec, (int) this.MaxNumInventoryParameterSpecsPerAISpec_len, bArr);
      }
      catch
      {
      }
      int maxNumAccessSpecs = (int) this.MaxNumAccessSpecs;
      try
      {
        Util.AppendObjToBitArray((object) this.MaxNumAccessSpecs, (int) this.MaxNumAccessSpecs_len, bArr);
      }
      catch
      {
      }
      int specsPerAccessSpec = (int) this.MaxNumOpSpecsPerAccessSpec;
      try
      {
        Util.AppendObjToBitArray((object) this.MaxNumOpSpecsPerAccessSpec, (int) this.MaxNumOpSpecsPerAccessSpec_len, bArr);
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
