// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_RFReceiver
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_RFReceiver : Parameter
  {
    public ushort ReceiverSensitivity;
    private short ReceiverSensitivity_len;

    public PARAM_RFReceiver() => this.typeID = (ushort) 223;

    public static PARAM_RFReceiver FromBitArray(ref BitArray bit_array, ref int cursor, int length)
    {
      if (cursor >= length)
        return (PARAM_RFReceiver) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_RFReceiver paramRfReceiver = new PARAM_RFReceiver();
      paramRfReceiver.tvCoding = bit_array[cursor];
      int val;
      if (paramRfReceiver.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        paramRfReceiver.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) paramRfReceiver.length * 8;
      }
      if (val != (int) paramRfReceiver.TypeID)
      {
        cursor = num1;
        return (PARAM_RFReceiver) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len = 16;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len);
      paramRfReceiver.ReceiverSensitivity = (ushort) obj;
      return paramRfReceiver;
    }

    public override string ToString()
    {
      string str = "<RFReceiver>" + "\r\n";
      int receiverSensitivity = (int) this.ReceiverSensitivity;
      try
      {
        str = str + "  <ReceiverSensitivity>" + Util.ConvertValueTypeToString((object) this.ReceiverSensitivity, "u16", "") + "</ReceiverSensitivity>";
        str += "\r\n";
      }
      catch
      {
      }
      return str + "</RFReceiver>" + "\r\n";
    }

    public static PARAM_RFReceiver FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      namespaceManager.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      namespaceManager.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      return new PARAM_RFReceiver()
      {
        ReceiverSensitivity = (ushort) Util.ParseValueTypeFromString(XmlUtil.GetNodeValue(node, "ReceiverSensitivity"), "u16", "")
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
      int receiverSensitivity = (int) this.ReceiverSensitivity;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.ReceiverSensitivity, (int) this.ReceiverSensitivity_len);
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
      int receiverSensitivity = (int) this.ReceiverSensitivity;
      try
      {
        Util.AppendObjToBitArray((object) this.ReceiverSensitivity, (int) this.ReceiverSensitivity_len, bArr);
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
