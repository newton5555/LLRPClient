// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_ROSpecStartTrigger
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_ROSpecStartTrigger : Parameter
  {
    public ENUM_ROSpecStartTriggerType ROSpecStartTriggerType;
    private short ROSpecStartTriggerType_len = 8;
    public PARAM_PeriodicTriggerValue PeriodicTriggerValue;
    public PARAM_GPITriggerValue GPITriggerValue;

    public PARAM_ROSpecStartTrigger() => this.typeID = (ushort) 179;

    public static PARAM_ROSpecStartTrigger FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_ROSpecStartTrigger) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_ROSpecStartTrigger specStartTrigger = new PARAM_ROSpecStartTrigger();
      specStartTrigger.tvCoding = bit_array[cursor];
      int val;
      if (specStartTrigger.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        specStartTrigger.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) specStartTrigger.length * 8;
      }
      if (val != (int) specStartTrigger.TypeID)
      {
        cursor = num1;
        return (PARAM_ROSpecStartTrigger) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len = 8;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len);
      specStartTrigger.ROSpecStartTriggerType = (ENUM_ROSpecStartTriggerType) (uint) obj;
      specStartTrigger.PeriodicTriggerValue = PARAM_PeriodicTriggerValue.FromBitArray(ref bit_array, ref cursor, length);
      specStartTrigger.GPITriggerValue = PARAM_GPITriggerValue.FromBitArray(ref bit_array, ref cursor, length);
      return specStartTrigger;
    }

    public override string ToString()
    {
      string str = "<ROSpecStartTrigger>" + "\r\n";
      int startTriggerType = (int) this.ROSpecStartTriggerType;
      try
      {
        str = str + "  <ROSpecStartTriggerType>" + this.ROSpecStartTriggerType.ToString() + "</ROSpecStartTriggerType>";
        str += "\r\n";
      }
      catch
      {
      }
      if (this.PeriodicTriggerValue != null)
        str += Util.Indent(this.PeriodicTriggerValue.ToString());
      if (this.GPITriggerValue != null)
        str += Util.Indent(this.GPITriggerValue.ToString());
      return str + "</ROSpecStartTrigger>" + "\r\n";
    }

    public static PARAM_ROSpecStartTrigger FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      nsmgr.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      nsmgr.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      PARAM_ROSpecStartTrigger specStartTrigger = new PARAM_ROSpecStartTrigger();
      string nodeValue = XmlUtil.GetNodeValue(node, "ROSpecStartTriggerType");
      specStartTrigger.ROSpecStartTriggerType = (ENUM_ROSpecStartTriggerType) Enum.Parse(typeof (ENUM_ROSpecStartTriggerType), nodeValue);
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "PeriodicTriggerValue", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            specStartTrigger.PeriodicTriggerValue = PARAM_PeriodicTriggerValue.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "GPITriggerValue", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            specStartTrigger.GPITriggerValue = PARAM_GPITriggerValue.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      return specStartTrigger;
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
      int startTriggerType = (int) this.ROSpecStartTriggerType;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.ROSpecStartTriggerType, (int) this.ROSpecStartTriggerType_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      if (this.PeriodicTriggerValue != null)
        this.PeriodicTriggerValue.ToBitArray(ref bit_array, ref cursor);
      if (this.GPITriggerValue != null)
        this.GPITriggerValue.ToBitArray(ref bit_array, ref cursor);
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
      int startTriggerType = (int) this.ROSpecStartTriggerType;
      try
      {
        Util.AppendObjToBitArray((object) this.ROSpecStartTriggerType, (int) this.ROSpecStartTriggerType_len, bArr);
      }
      catch
      {
      }
      if (this.PeriodicTriggerValue != null)
        this.PeriodicTriggerValue.AppendToBitArray(bArr);
      if (this.GPITriggerValue != null)
        this.GPITriggerValue.AppendToBitArray(bArr);
      if (this.tvCoding)
        return;
      BitArray bitArray = Util.ConvertIntToBitArray((uint) (bArr.Length - length1) / 8U, 16);
      for (int index = 0; index < bitArray.Length; ++index)
        bArr[length1 + 16 + index] = bitArray[index];
    }
  }
}
