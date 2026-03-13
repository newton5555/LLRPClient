// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_ReaderExceptionEvent
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_ReaderExceptionEvent : Parameter
  {
    public string Message = string.Empty;
    private short Message_len;
    public PARAM_ROSpecID ROSpecID;
    public PARAM_SpecIndex SpecIndex;
    public PARAM_InventoryParameterSpecID InventoryParameterSpecID;
    public PARAM_AntennaID AntennaID;
    public PARAM_AccessSpecID AccessSpecID;
    public PARAM_OpSpecID OpSpecID;
    public readonly CustomParameterArrayList Custom = new CustomParameterArrayList();

    public PARAM_ReaderExceptionEvent() => this.typeID = (ushort) 252;

    public bool AddCustomParameter(ICustom_Parameter param)
    {
      if (param is IReaderExceptionEvent_Custom_Param)
      {
        this.Custom.Add(param);
        return true;
      }
      if (!(param.GetType() == typeof (PARAM_Custom)))
        return false;
      this.Custom.Add(param);
      return true;
    }

    public static PARAM_ReaderExceptionEvent FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_ReaderExceptionEvent) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_ReaderExceptionEvent readerExceptionEvent = new PARAM_ReaderExceptionEvent();
      readerExceptionEvent.tvCoding = bit_array[cursor];
      int val;
      if (readerExceptionEvent.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        readerExceptionEvent.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) readerExceptionEvent.length * 8;
      }
      if (val != (int) readerExceptionEvent.TypeID)
      {
        cursor = num1;
        return (PARAM_ReaderExceptionEvent) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int fieldLength = Util.DetermineFieldLength(ref bit_array, ref cursor);
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (string), fieldLength);
      readerExceptionEvent.Message = (string) obj;
      readerExceptionEvent.ROSpecID = PARAM_ROSpecID.FromBitArray(ref bit_array, ref cursor, length);
      readerExceptionEvent.SpecIndex = PARAM_SpecIndex.FromBitArray(ref bit_array, ref cursor, length);
      readerExceptionEvent.InventoryParameterSpecID = PARAM_InventoryParameterSpecID.FromBitArray(ref bit_array, ref cursor, length);
      readerExceptionEvent.AntennaID = PARAM_AntennaID.FromBitArray(ref bit_array, ref cursor, length);
      readerExceptionEvent.AccessSpecID = PARAM_AccessSpecID.FromBitArray(ref bit_array, ref cursor, length);
      readerExceptionEvent.OpSpecID = PARAM_OpSpecID.FromBitArray(ref bit_array, ref cursor, length);
      while (cursor < num2)
      {
        int num3 = cursor;
        ICustom_Parameter customParameter = CustomParamDecodeFactory.DecodeCustomParameter(ref bit_array, ref cursor, length);
        if (customParameter != null && cursor <= num2 && !readerExceptionEvent.AddCustomParameter(customParameter))
        {
          cursor = num3;
          break;
        }
      }
      return readerExceptionEvent;
    }

    public override string ToString()
    {
      string str = "<ReaderExceptionEvent>" + "\r\n";
      if (this.Message != null)
      {
        try
        {
          str = str + "  <Message>" + Util.ConvertArrayTypeToString((object) this.Message, "utf8v", "UTF8") + "</Message>";
          str += "\r\n";
        }
        catch
        {
        }
      }
      if (this.ROSpecID != null)
        str += Util.Indent(this.ROSpecID.ToString());
      if (this.SpecIndex != null)
        str += Util.Indent(this.SpecIndex.ToString());
      if (this.InventoryParameterSpecID != null)
        str += Util.Indent(this.InventoryParameterSpecID.ToString());
      if (this.AntennaID != null)
        str += Util.Indent(this.AntennaID.ToString());
      if (this.AccessSpecID != null)
        str += Util.Indent(this.AccessSpecID.ToString());
      if (this.OpSpecID != null)
        str += Util.Indent(this.OpSpecID.ToString());
      if (this.Custom != null)
      {
        int length = this.Custom.Length;
        for (int index = 0; index < length; ++index)
          str += Util.Indent(this.Custom[index].ToString());
      }
      return str + "</ReaderExceptionEvent>" + "\r\n";
    }

    public static PARAM_ReaderExceptionEvent FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      nsmgr.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      nsmgr.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      PARAM_ReaderExceptionEvent readerExceptionEvent = new PARAM_ReaderExceptionEvent();
      string nodeValue = XmlUtil.GetNodeValue(node, "Message");
      readerExceptionEvent.Message = (string) Util.ParseArrayTypeFromString(nodeValue, "utf8v", "UTF8");
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "ROSpecID", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            readerExceptionEvent.ROSpecID = PARAM_ROSpecID.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "SpecIndex", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            readerExceptionEvent.SpecIndex = PARAM_SpecIndex.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "InventoryParameterSpecID", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            readerExceptionEvent.InventoryParameterSpecID = PARAM_InventoryParameterSpecID.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "AntennaID", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            readerExceptionEvent.AntennaID = PARAM_AntennaID.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "AccessSpecID", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            readerExceptionEvent.AccessSpecID = PARAM_AccessSpecID.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "OpSpecID", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            readerExceptionEvent.OpSpecID = PARAM_OpSpecID.FromXmlNode(xmlNodes[0]);
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
              if (customParameter != null && readerExceptionEvent.AddCustomParameter(customParameter))
                arrayList.Add(nodeCustomChildren[index]);
            }
          }
        }
      }
      catch
      {
      }
      return readerExceptionEvent;
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
      if (this.Message != null)
      {
        try
        {
          Util.ConvertIntToBitArray((uint) this.Message.Length, 16).CopyTo((Array) bit_array, cursor);
          cursor += 16;
          BitArray bitArray = Util.ConvertObjToBitArray((object) this.Message, (int) this.Message_len);
          bitArray.CopyTo((Array) bit_array, cursor);
          cursor += bitArray.Length;
        }
        catch
        {
        }
      }
      if (this.ROSpecID != null)
        this.ROSpecID.ToBitArray(ref bit_array, ref cursor);
      if (this.SpecIndex != null)
        this.SpecIndex.ToBitArray(ref bit_array, ref cursor);
      if (this.InventoryParameterSpecID != null)
        this.InventoryParameterSpecID.ToBitArray(ref bit_array, ref cursor);
      if (this.AntennaID != null)
        this.AntennaID.ToBitArray(ref bit_array, ref cursor);
      if (this.AccessSpecID != null)
        this.AccessSpecID.ToBitArray(ref bit_array, ref cursor);
      if (this.OpSpecID != null)
        this.OpSpecID.ToBitArray(ref bit_array, ref cursor);
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
      if (this.Message != null)
      {
        try
        {
          Util.AppendIntToBitArray((uint) this.Message.Length, 16, bArr);
          Util.AppendObjToBitArray((object) this.Message, (int) this.Message_len, bArr);
        }
        catch
        {
        }
      }
      if (this.ROSpecID != null)
        this.ROSpecID.AppendToBitArray(bArr);
      if (this.SpecIndex != null)
        this.SpecIndex.AppendToBitArray(bArr);
      if (this.InventoryParameterSpecID != null)
        this.InventoryParameterSpecID.AppendToBitArray(bArr);
      if (this.AntennaID != null)
        this.AntennaID.AppendToBitArray(bArr);
      if (this.AccessSpecID != null)
        this.AccessSpecID.AppendToBitArray(bArr);
      if (this.OpSpecID != null)
        this.OpSpecID.AppendToBitArray(bArr);
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
