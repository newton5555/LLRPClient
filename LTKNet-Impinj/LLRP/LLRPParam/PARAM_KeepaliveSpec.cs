// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_KeepaliveSpec
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_KeepaliveSpec : Parameter
  {
    public ENUM_KeepaliveTriggerType KeepaliveTriggerType;
    private short KeepaliveTriggerType_len = 8;
    public uint PeriodicTriggerValue;
    private short PeriodicTriggerValue_len;

    public PARAM_KeepaliveSpec() => this.typeID = (ushort) 220;

    public static PARAM_KeepaliveSpec FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_KeepaliveSpec) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_KeepaliveSpec paramKeepaliveSpec = new PARAM_KeepaliveSpec();
      paramKeepaliveSpec.tvCoding = bit_array[cursor];
      int val;
      if (paramKeepaliveSpec.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        paramKeepaliveSpec.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) paramKeepaliveSpec.length * 8;
      }
      if (val != (int) paramKeepaliveSpec.TypeID)
      {
        cursor = num1;
        return (PARAM_KeepaliveSpec) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 8;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len1);
      paramKeepaliveSpec.KeepaliveTriggerType = (ENUM_KeepaliveTriggerType) (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 32;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len2);
      paramKeepaliveSpec.PeriodicTriggerValue = (uint) obj;
      return paramKeepaliveSpec;
    }

    public override string ToString()
    {
      string str = "<KeepaliveSpec>" + "\r\n";
      int keepaliveTriggerType = (int) this.KeepaliveTriggerType;
      try
      {
        str = str + "  <KeepaliveTriggerType>" + this.KeepaliveTriggerType.ToString() + "</KeepaliveTriggerType>";
        str += "\r\n";
      }
      catch
      {
      }
      int periodicTriggerValue = (int) this.PeriodicTriggerValue;
      try
      {
        str = str + "  <PeriodicTriggerValue>" + Util.ConvertValueTypeToString((object) this.PeriodicTriggerValue, "u32", "") + "</PeriodicTriggerValue>";
        str += "\r\n";
      }
      catch
      {
      }
      return str + "</KeepaliveSpec>" + "\r\n";
    }

    public static PARAM_KeepaliveSpec FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      namespaceManager.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      namespaceManager.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      return new PARAM_KeepaliveSpec()
      {
        KeepaliveTriggerType = (ENUM_KeepaliveTriggerType) Enum.Parse(typeof (ENUM_KeepaliveTriggerType), XmlUtil.GetNodeValue(node, "KeepaliveTriggerType")),
        PeriodicTriggerValue = (uint) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "PeriodicTriggerValue"), "u32", "")
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
      int keepaliveTriggerType = (int) this.KeepaliveTriggerType;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.KeepaliveTriggerType, (int) this.KeepaliveTriggerType_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int periodicTriggerValue = (int) this.PeriodicTriggerValue;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.PeriodicTriggerValue, (int) this.PeriodicTriggerValue_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
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
      int keepaliveTriggerType = (int) this.KeepaliveTriggerType;
      try
      {
        Util.AppendObjToBitArray((object) this.KeepaliveTriggerType, (int) this.KeepaliveTriggerType_len, bArr);
      }
      catch
      {
      }
      int periodicTriggerValue = (int) this.PeriodicTriggerValue;
      try
      {
        Util.AppendObjToBitArray((object) this.PeriodicTriggerValue, (int) this.PeriodicTriggerValue_len, bArr);
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
