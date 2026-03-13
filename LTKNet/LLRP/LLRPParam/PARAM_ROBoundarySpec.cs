// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_ROBoundarySpec
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_ROBoundarySpec : Parameter
  {
    public PARAM_ROSpecStartTrigger ROSpecStartTrigger;
    public PARAM_ROSpecStopTrigger ROSpecStopTrigger;

    public PARAM_ROBoundarySpec() => this.typeID = (ushort) 178;

    public static PARAM_ROBoundarySpec FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_ROBoundarySpec) null;
      int num = cursor;
      ArrayList arrayList = new ArrayList();
      PARAM_ROBoundarySpec paramRoBoundarySpec = new PARAM_ROBoundarySpec();
      paramRoBoundarySpec.tvCoding = bit_array[cursor];
      int val;
      if (paramRoBoundarySpec.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        paramRoBoundarySpec.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        int length1 = (int) paramRoBoundarySpec.length;
      }
      if (val != (int) paramRoBoundarySpec.TypeID)
      {
        cursor = num;
        return (PARAM_ROBoundarySpec) null;
      }
      paramRoBoundarySpec.ROSpecStartTrigger = PARAM_ROSpecStartTrigger.FromBitArray(ref bit_array, ref cursor, length);
      paramRoBoundarySpec.ROSpecStopTrigger = PARAM_ROSpecStopTrigger.FromBitArray(ref bit_array, ref cursor, length);
      return paramRoBoundarySpec;
    }

    public override string ToString()
    {
      string str = "<ROBoundarySpec>" + "\r\n";
      if (this.ROSpecStartTrigger != null)
        str += Util.Indent(this.ROSpecStartTrigger.ToString());
      if (this.ROSpecStopTrigger != null)
        str += Util.Indent(this.ROSpecStopTrigger.ToString());
      return str + "</ROBoundarySpec>" + "\r\n";
    }

    public static PARAM_ROBoundarySpec FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      nsmgr.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      nsmgr.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      PARAM_ROBoundarySpec paramRoBoundarySpec = new PARAM_ROBoundarySpec();
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "ROSpecStartTrigger", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            paramRoBoundarySpec.ROSpecStartTrigger = PARAM_ROSpecStartTrigger.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "ROSpecStopTrigger", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            paramRoBoundarySpec.ROSpecStopTrigger = PARAM_ROSpecStopTrigger.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      return paramRoBoundarySpec;
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
      if (this.ROSpecStartTrigger != null)
        this.ROSpecStartTrigger.ToBitArray(ref bit_array, ref cursor);
      if (this.ROSpecStopTrigger != null)
        this.ROSpecStopTrigger.ToBitArray(ref bit_array, ref cursor);
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
      if (this.ROSpecStartTrigger != null)
        this.ROSpecStartTrigger.AppendToBitArray(bArr);
      if (this.ROSpecStopTrigger != null)
        this.ROSpecStopTrigger.AppendToBitArray(bArr);
      if (this.tvCoding)
        return;
      BitArray bitArray = Util.ConvertIntToBitArray((uint) (bArr.Length - length1) / 8U, 16);
      for (int index = 0; index < bitArray.Length; ++index)
        bArr[length1 + 16 + index] = bitArray[index];
    }
  }
}
