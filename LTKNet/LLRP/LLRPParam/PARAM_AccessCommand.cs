// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_AccessCommand
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_AccessCommand : Parameter
  {
    public UNION_AirProtocolTagSpec AirProtocolTagSpec = new UNION_AirProtocolTagSpec();
    public UNION_AccessCommandOpSpec AccessCommandOpSpec = new UNION_AccessCommandOpSpec();
    public readonly CustomParameterArrayList Custom = new CustomParameterArrayList();

    public PARAM_AccessCommand() => this.typeID = (ushort) 209;

    public bool AddCustomParameter(ICustom_Parameter param)
    {
      if (param is IAccessCommand_Custom_Param)
      {
        this.Custom.Add(param);
        return true;
      }
      if (!(param.GetType() == typeof (PARAM_Custom)))
        return false;
      this.Custom.Add(param);
      return true;
    }

    public static PARAM_AccessCommand FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_AccessCommand) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_AccessCommand paramAccessCommand = new PARAM_AccessCommand();
      paramAccessCommand.tvCoding = bit_array[cursor];
      int val1;
      if (paramAccessCommand.tvCoding)
      {
        ++cursor;
        val1 = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val1 = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        paramAccessCommand.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) paramAccessCommand.length * 8;
      }
      if (val1 != (int) paramAccessCommand.TypeID)
      {
        cursor = num1;
        return (PARAM_AccessCommand) null;
      }
      ushort num3 = 1;
      while (num3 != (ushort) 0)
      {
        num3 = (ushort) 0;
        PARAM_C1G2TagSpec val2 = PARAM_C1G2TagSpec.FromBitArray(ref bit_array, ref cursor, length);
        if (val2 != null)
        {
          ++num3;
          paramAccessCommand.AirProtocolTagSpec.Add((IParameter) val2);
        }
      }
      ushort num4 = 1;
      while (num4 != (ushort) 0)
      {
        num4 = (ushort) 0;
        PARAM_C1G2Read val3 = PARAM_C1G2Read.FromBitArray(ref bit_array, ref cursor, length);
        if (val3 != null)
        {
          ++num4;
          paramAccessCommand.AccessCommandOpSpec.Add((IParameter) val3);
        }
        PARAM_C1G2Write val4 = PARAM_C1G2Write.FromBitArray(ref bit_array, ref cursor, length);
        if (val4 != null)
        {
          ++num4;
          paramAccessCommand.AccessCommandOpSpec.Add((IParameter) val4);
        }
        PARAM_C1G2Kill val5 = PARAM_C1G2Kill.FromBitArray(ref bit_array, ref cursor, length);
        if (val5 != null)
        {
          ++num4;
          paramAccessCommand.AccessCommandOpSpec.Add((IParameter) val5);
        }
        PARAM_C1G2Lock val6 = PARAM_C1G2Lock.FromBitArray(ref bit_array, ref cursor, length);
        if (val6 != null)
        {
          ++num4;
          paramAccessCommand.AccessCommandOpSpec.Add((IParameter) val6);
        }
        PARAM_C1G2BlockErase val7 = PARAM_C1G2BlockErase.FromBitArray(ref bit_array, ref cursor, length);
        if (val7 != null)
        {
          ++num4;
          paramAccessCommand.AccessCommandOpSpec.Add((IParameter) val7);
        }
        PARAM_C1G2BlockWrite val8 = PARAM_C1G2BlockWrite.FromBitArray(ref bit_array, ref cursor, length);
        if (val8 != null)
        {
          ++num4;
          paramAccessCommand.AccessCommandOpSpec.Add((IParameter) val8);
        }
        PARAM_ClientRequestOpSpec val9 = PARAM_ClientRequestOpSpec.FromBitArray(ref bit_array, ref cursor, length);
        if (val9 != null)
        {
          ++num4;
          paramAccessCommand.AccessCommandOpSpec.Add((IParameter) val9);
        }
        int num5 = cursor;
        ICustom_Parameter customParameter = CustomParamDecodeFactory.DecodeCustomParameter(ref bit_array, ref cursor, length);
        if (customParameter != null)
        {
          if (paramAccessCommand.AccessCommandOpSpec.AddCustomParameter(customParameter))
            ++num4;
          else
            cursor = num5;
        }
      }
      while (cursor < num2)
      {
        int num6 = cursor;
        ICustom_Parameter customParameter = CustomParamDecodeFactory.DecodeCustomParameter(ref bit_array, ref cursor, length);
        if (customParameter != null && cursor <= num2 && !paramAccessCommand.AddCustomParameter(customParameter))
        {
          cursor = num6;
          break;
        }
      }
      return paramAccessCommand;
    }

    public override string ToString()
    {
      string str = "<AccessCommand>" + "\r\n";
      if (this.AirProtocolTagSpec != null)
      {
        int count = this.AirProtocolTagSpec.Count;
        for (int index = 0; index < count; ++index)
          str += Util.Indent(this.AirProtocolTagSpec[index].ToString());
      }
      if (this.AccessCommandOpSpec != null)
      {
        int count = this.AccessCommandOpSpec.Count;
        for (int index = 0; index < count; ++index)
          str += Util.Indent(this.AccessCommandOpSpec[index].ToString());
      }
      if (this.Custom != null)
      {
        int length = this.Custom.Length;
        for (int index = 0; index < length; ++index)
          str += Util.Indent(this.Custom[index].ToString());
      }
      return str + "</AccessCommand>" + "\r\n";
    }

    public static PARAM_AccessCommand FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      nsmgr.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      nsmgr.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      PARAM_AccessCommand paramAccessCommand = new PARAM_AccessCommand();
      paramAccessCommand.AirProtocolTagSpec = new UNION_AirProtocolTagSpec();
      try
      {
        foreach (XmlNode childNode in node.ChildNodes)
        {
          if (childNode.Name == "C1G2TagSpec")
            paramAccessCommand.AirProtocolTagSpec.Add((IParameter) PARAM_C1G2TagSpec.FromXmlNode(childNode));
        }
      }
      catch
      {
      }
      paramAccessCommand.AccessCommandOpSpec = new UNION_AccessCommandOpSpec();
      try
      {
        foreach (XmlNode childNode in node.ChildNodes)
        {
          string name = childNode.Name;
          if (name != null)
          {
            switch (name.Length)
            {
              case 6:
                if (name == "Custom")
                  break;
                break;
              case 8:
                switch (name[4])
                {
                  case 'K':
                    if (name == "C1G2Kill")
                    {
                      paramAccessCommand.AccessCommandOpSpec.Add((IParameter) PARAM_C1G2Kill.FromXmlNode(childNode));
                      continue;
                    }
                    break;
                  case 'L':
                    if (name == "C1G2Lock")
                    {
                      paramAccessCommand.AccessCommandOpSpec.Add((IParameter) PARAM_C1G2Lock.FromXmlNode(childNode));
                      continue;
                    }
                    break;
                  case 'R':
                    if (name == "C1G2Read")
                    {
                      paramAccessCommand.AccessCommandOpSpec.Add((IParameter) PARAM_C1G2Read.FromXmlNode(childNode));
                      continue;
                    }
                    break;
                }
                break;
              case 9:
                if (name == "C1G2Write")
                {
                  paramAccessCommand.AccessCommandOpSpec.Add((IParameter) PARAM_C1G2Write.FromXmlNode(childNode));
                  continue;
                }
                break;
              case 14:
                switch (name[9])
                {
                  case 'E':
                    if (name == "C1G2BlockErase")
                    {
                      paramAccessCommand.AccessCommandOpSpec.Add((IParameter) PARAM_C1G2BlockErase.FromXmlNode(childNode));
                      continue;
                    }
                    break;
                  case 'W':
                    if (name == "C1G2BlockWrite")
                    {
                      paramAccessCommand.AccessCommandOpSpec.Add((IParameter) PARAM_C1G2BlockWrite.FromXmlNode(childNode));
                      continue;
                    }
                    break;
                }
                break;
              case 19:
                if (name == "ClientRequestOpSpec")
                {
                  paramAccessCommand.AccessCommandOpSpec.Add((IParameter) PARAM_ClientRequestOpSpec.FromXmlNode(childNode));
                  continue;
                }
                break;
            }
          }
          if (!arrayList.Contains((object) childNode))
          {
            ICustom_Parameter customParameter = CustomParamDecodeFactory.DecodeXmlNodeToCustomParameter(childNode);
            if (customParameter != null && paramAccessCommand.AccessCommandOpSpec.AddCustomParameter(customParameter))
              arrayList.Add((object) childNode);
          }
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
              if (customParameter != null && paramAccessCommand.AddCustomParameter(customParameter))
                arrayList.Add(nodeCustomChildren[index]);
            }
          }
        }
      }
      catch
      {
      }
      return paramAccessCommand;
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
      int count1 = this.AirProtocolTagSpec.Count;
      for (int index = 0; index < count1; ++index)
        this.AirProtocolTagSpec[index].ToBitArray(ref bit_array, ref cursor);
      int count2 = this.AccessCommandOpSpec.Count;
      for (int index = 0; index < count2; ++index)
        this.AccessCommandOpSpec[index].ToBitArray(ref bit_array, ref cursor);
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
      int count1 = this.AirProtocolTagSpec.Count;
      for (int index = 0; index < count1; ++index)
        this.AirProtocolTagSpec[index].AppendToBitArray(bArr);
      int count2 = this.AccessCommandOpSpec.Count;
      for (int index = 0; index < count2; ++index)
        this.AccessCommandOpSpec[index].AppendToBitArray(bArr);
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
