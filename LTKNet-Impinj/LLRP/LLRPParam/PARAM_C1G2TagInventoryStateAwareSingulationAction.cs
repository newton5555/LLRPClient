// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_C1G2TagInventoryStateAwareSingulationAction
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_C1G2TagInventoryStateAwareSingulationAction : Parameter
  {
    public ENUM_C1G2TagInventoryStateAwareI I;
    private short I_len = 1;
    public ENUM_C1G2TagInventoryStateAwareS S;
    private short S_len = 1;
    private const ushort param_reserved_len4 = 6;

    public PARAM_C1G2TagInventoryStateAwareSingulationAction() => this.typeID = (ushort) 337;

    public static PARAM_C1G2TagInventoryStateAwareSingulationAction FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_C1G2TagInventoryStateAwareSingulationAction) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_C1G2TagInventoryStateAwareSingulationAction singulationAction = new PARAM_C1G2TagInventoryStateAwareSingulationAction();
      singulationAction.tvCoding = bit_array[cursor];
      int val;
      if (singulationAction.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        singulationAction.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) singulationAction.length * 8;
      }
      if (val != (int) singulationAction.TypeID)
      {
        cursor = num1;
        return (PARAM_C1G2TagInventoryStateAwareSingulationAction) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 1;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len1);
      singulationAction.I = (ENUM_C1G2TagInventoryStateAwareI) (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 1;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len2);
      singulationAction.S = (ENUM_C1G2TagInventoryStateAwareS) (uint) obj;
      cursor += 6;
      return singulationAction;
    }

    public override string ToString()
    {
      string str = "<C1G2TagInventoryStateAwareSingulationAction>" + "\r\n";
      int i = (int) this.I;
      try
      {
        str = str + "  <I>" + this.I.ToString() + "</I>";
        str += "\r\n";
      }
      catch
      {
      }
      int s = (int) this.S;
      try
      {
        str = str + "  <S>" + this.S.ToString() + "</S>";
        str += "\r\n";
      }
      catch
      {
      }
      return str + "</C1G2TagInventoryStateAwareSingulationAction>" + "\r\n";
    }

    public static PARAM_C1G2TagInventoryStateAwareSingulationAction FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      namespaceManager.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      namespaceManager.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      return new PARAM_C1G2TagInventoryStateAwareSingulationAction()
      {
        I = (ENUM_C1G2TagInventoryStateAwareI) Enum.Parse(typeof (ENUM_C1G2TagInventoryStateAwareI), XmlUtil.GetNodeValue(node, "I")),
        S = (ENUM_C1G2TagInventoryStateAwareS) Enum.Parse(typeof (ENUM_C1G2TagInventoryStateAwareS), XmlUtil.GetNodeValue(node, "S"))
      };
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
      int i = (int) this.I;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.I, (int) this.I_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int s = (int) this.S;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.S, (int) this.S_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      cursor += 6;
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
      int i = (int) this.I;
      try
      {
        Util.AppendObjToBitArray((object) this.I, (int) this.I_len, bArr);
      }
      catch
      {
      }
      int s = (int) this.S;
      try
      {
        Util.AppendObjToBitArray((object) this.S, (int) this.S_len, bArr);
      }
      catch
      {
      }
      bArr.Length += 6;
      if (this.tvCoding)
        return;
      BitArray bitArray = Util.ConvertIntToBitArray((uint) (bArr.Length - length1) / 8U, 16);
      for (int index = 0; index < bitArray.Length; ++index)
        bArr[length1 + 16 + index] = bitArray[index];
    }
  }
}
