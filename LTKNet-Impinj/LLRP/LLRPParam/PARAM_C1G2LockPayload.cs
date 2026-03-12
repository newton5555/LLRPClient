// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_C1G2LockPayload
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_C1G2LockPayload : Parameter
  {
    public ENUM_C1G2LockPrivilege Privilege;
    private short Privilege_len = 8;
    public ENUM_C1G2LockDataField DataField;
    private short DataField_len = 8;

    public PARAM_C1G2LockPayload() => this.typeID = (ushort) 345;

    public static PARAM_C1G2LockPayload FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_C1G2LockPayload) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_C1G2LockPayload paramC1G2LockPayload = new PARAM_C1G2LockPayload();
      paramC1G2LockPayload.tvCoding = bit_array[cursor];
      int val;
      if (paramC1G2LockPayload.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        paramC1G2LockPayload.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) paramC1G2LockPayload.length * 8;
      }
      if (val != (int) paramC1G2LockPayload.TypeID)
      {
        cursor = num1;
        return (PARAM_C1G2LockPayload) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 8;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len1);
      paramC1G2LockPayload.Privilege = (ENUM_C1G2LockPrivilege) (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 8;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len2);
      paramC1G2LockPayload.DataField = (ENUM_C1G2LockDataField) (uint) obj;
      return paramC1G2LockPayload;
    }

    public override string ToString()
    {
      string str = "<C1G2LockPayload>" + "\r\n";
      int privilege = (int) this.Privilege;
      try
      {
        str = str + "  <Privilege>" + this.Privilege.ToString() + "</Privilege>";
        str += "\r\n";
      }
      catch
      {
      }
      int dataField = (int) this.DataField;
      try
      {
        str = str + "  <DataField>" + this.DataField.ToString() + "</DataField>";
        str += "\r\n";
      }
      catch
      {
      }
      return str + "</C1G2LockPayload>" + "\r\n";
    }

    public static PARAM_C1G2LockPayload FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      namespaceManager.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      namespaceManager.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      return new PARAM_C1G2LockPayload()
      {
        Privilege = (ENUM_C1G2LockPrivilege) Enum.Parse(typeof (ENUM_C1G2LockPrivilege), XmlUtil.GetNodeValue(node, "Privilege")),
        DataField = (ENUM_C1G2LockDataField) Enum.Parse(typeof (ENUM_C1G2LockDataField), XmlUtil.GetNodeValue(node, "DataField"))
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
      int privilege = (int) this.Privilege;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.Privilege, (int) this.Privilege_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int dataField = (int) this.DataField;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.DataField, (int) this.DataField_len);
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
      int privilege = (int) this.Privilege;
      try
      {
        Util.AppendObjToBitArray((object) this.Privilege, (int) this.Privilege_len, bArr);
      }
      catch
      {
      }
      int dataField = (int) this.DataField;
      try
      {
        Util.AppendObjToBitArray((object) this.DataField, (int) this.DataField_len, bArr);
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
