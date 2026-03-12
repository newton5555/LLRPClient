// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.PARAM_ReaderEventNotificationData
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_ReaderEventNotificationData : Parameter
  {
    public UNION_Timestamp Timestamp = new UNION_Timestamp();
    public PARAM_HoppingEvent HoppingEvent;
    public PARAM_GPIEvent GPIEvent;
    public PARAM_ROSpecEvent ROSpecEvent;
    public PARAM_ReportBufferLevelWarningEvent ReportBufferLevelWarningEvent;
    public PARAM_ReportBufferOverflowErrorEvent ReportBufferOverflowErrorEvent;
    public PARAM_ReaderExceptionEvent ReaderExceptionEvent;
    public PARAM_RFSurveyEvent RFSurveyEvent;
    public PARAM_AISpecEvent AISpecEvent;
    public PARAM_AntennaEvent AntennaEvent;
    public PARAM_ConnectionAttemptEvent ConnectionAttemptEvent;
    public PARAM_ConnectionCloseEvent ConnectionCloseEvent;
    public readonly CustomParameterArrayList Custom = new CustomParameterArrayList();

    public PARAM_ReaderEventNotificationData() => this.typeID = (ushort) 246;

    public bool AddCustomParameter(ICustom_Parameter param)
    {
      if (param is IReaderEventNotificationData_Custom_Param)
      {
        this.Custom.Add(param);
        return true;
      }
      if (!(param.GetType() == typeof (PARAM_Custom)))
        return false;
      this.Custom.Add(param);
      return true;
    }

    public static PARAM_ReaderEventNotificationData FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_ReaderEventNotificationData) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList = new ArrayList();
      PARAM_ReaderEventNotificationData notificationData = new PARAM_ReaderEventNotificationData();
      notificationData.tvCoding = bit_array[cursor];
      int val1;
      if (notificationData.tvCoding)
      {
        ++cursor;
        val1 = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val1 = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        notificationData.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) notificationData.length * 8;
      }
      if (val1 != (int) notificationData.TypeID)
      {
        cursor = num1;
        return (PARAM_ReaderEventNotificationData) null;
      }
      ushort num3 = 1;
      while (num3 != (ushort) 0)
      {
        num3 = (ushort) 0;
        PARAM_UTCTimestamp val2 = PARAM_UTCTimestamp.FromBitArray(ref bit_array, ref cursor, length);
        if (val2 != null)
        {
          ++num3;
          notificationData.Timestamp.Add((IParameter) val2);
        }
        PARAM_Uptime val3 = PARAM_Uptime.FromBitArray(ref bit_array, ref cursor, length);
        if (val3 != null)
        {
          ++num3;
          notificationData.Timestamp.Add((IParameter) val3);
        }
      }
      notificationData.HoppingEvent = PARAM_HoppingEvent.FromBitArray(ref bit_array, ref cursor, length);
      notificationData.GPIEvent = PARAM_GPIEvent.FromBitArray(ref bit_array, ref cursor, length);
      notificationData.ROSpecEvent = PARAM_ROSpecEvent.FromBitArray(ref bit_array, ref cursor, length);
      notificationData.ReportBufferLevelWarningEvent = PARAM_ReportBufferLevelWarningEvent.FromBitArray(ref bit_array, ref cursor, length);
      notificationData.ReportBufferOverflowErrorEvent = PARAM_ReportBufferOverflowErrorEvent.FromBitArray(ref bit_array, ref cursor, length);
      notificationData.ReaderExceptionEvent = PARAM_ReaderExceptionEvent.FromBitArray(ref bit_array, ref cursor, length);
      notificationData.RFSurveyEvent = PARAM_RFSurveyEvent.FromBitArray(ref bit_array, ref cursor, length);
      notificationData.AISpecEvent = PARAM_AISpecEvent.FromBitArray(ref bit_array, ref cursor, length);
      notificationData.AntennaEvent = PARAM_AntennaEvent.FromBitArray(ref bit_array, ref cursor, length);
      notificationData.ConnectionAttemptEvent = PARAM_ConnectionAttemptEvent.FromBitArray(ref bit_array, ref cursor, length);
      notificationData.ConnectionCloseEvent = PARAM_ConnectionCloseEvent.FromBitArray(ref bit_array, ref cursor, length);
      while (cursor < num2)
      {
        int num4 = cursor;
        ICustom_Parameter customParameter = CustomParamDecodeFactory.DecodeCustomParameter(ref bit_array, ref cursor, length);
        if (customParameter != null && cursor <= num2 && !notificationData.AddCustomParameter(customParameter))
        {
          cursor = num4;
          break;
        }
      }
      return notificationData;
    }

    public override string ToString()
    {
      string str = "<ReaderEventNotificationData>" + "\r\n";
      if (this.Timestamp != null)
      {
        int count = this.Timestamp.Count;
        for (int index = 0; index < count; ++index)
          str += Util.Indent(this.Timestamp[index].ToString());
      }
      if (this.HoppingEvent != null)
        str += Util.Indent(this.HoppingEvent.ToString());
      if (this.GPIEvent != null)
        str += Util.Indent(this.GPIEvent.ToString());
      if (this.ROSpecEvent != null)
        str += Util.Indent(this.ROSpecEvent.ToString());
      if (this.ReportBufferLevelWarningEvent != null)
        str += Util.Indent(this.ReportBufferLevelWarningEvent.ToString());
      if (this.ReportBufferOverflowErrorEvent != null)
        str += Util.Indent(this.ReportBufferOverflowErrorEvent.ToString());
      if (this.ReaderExceptionEvent != null)
        str += Util.Indent(this.ReaderExceptionEvent.ToString());
      if (this.RFSurveyEvent != null)
        str += Util.Indent(this.RFSurveyEvent.ToString());
      if (this.AISpecEvent != null)
        str += Util.Indent(this.AISpecEvent.ToString());
      if (this.AntennaEvent != null)
        str += Util.Indent(this.AntennaEvent.ToString());
      if (this.ConnectionAttemptEvent != null)
        str += Util.Indent(this.ConnectionAttemptEvent.ToString());
      if (this.ConnectionCloseEvent != null)
        str += Util.Indent(this.ConnectionCloseEvent.ToString());
      if (this.Custom != null)
      {
        int length = this.Custom.Length;
        for (int index = 0; index < length; ++index)
          str += Util.Indent(this.Custom[index].ToString());
      }
      return str + "</ReaderEventNotificationData>" + "\r\n";
    }

    public static PARAM_ReaderEventNotificationData FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      nsmgr.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      nsmgr.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      PARAM_ReaderEventNotificationData notificationData = new PARAM_ReaderEventNotificationData();
      notificationData.Timestamp = new UNION_Timestamp();
      try
      {
        foreach (XmlNode childNode in node.ChildNodes)
        {
          switch (childNode.Name)
          {
            case "UTCTimestamp":
              notificationData.Timestamp.Add((IParameter) PARAM_UTCTimestamp.FromXmlNode(childNode));
              continue;
            case "Uptime":
              notificationData.Timestamp.Add((IParameter) PARAM_Uptime.FromXmlNode(childNode));
              continue;
            default:
              continue;
          }
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "HoppingEvent", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            notificationData.HoppingEvent = PARAM_HoppingEvent.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "GPIEvent", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            notificationData.GPIEvent = PARAM_GPIEvent.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "ROSpecEvent", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            notificationData.ROSpecEvent = PARAM_ROSpecEvent.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "ReportBufferLevelWarningEvent", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            notificationData.ReportBufferLevelWarningEvent = PARAM_ReportBufferLevelWarningEvent.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "ReportBufferOverflowErrorEvent", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            notificationData.ReportBufferOverflowErrorEvent = PARAM_ReportBufferOverflowErrorEvent.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "ReaderExceptionEvent", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            notificationData.ReaderExceptionEvent = PARAM_ReaderExceptionEvent.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "RFSurveyEvent", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            notificationData.RFSurveyEvent = PARAM_RFSurveyEvent.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "AISpecEvent", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            notificationData.AISpecEvent = PARAM_AISpecEvent.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "AntennaEvent", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            notificationData.AntennaEvent = PARAM_AntennaEvent.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "ConnectionAttemptEvent", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            notificationData.ConnectionAttemptEvent = PARAM_ConnectionAttemptEvent.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "ConnectionCloseEvent", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            notificationData.ConnectionCloseEvent = PARAM_ConnectionCloseEvent.FromXmlNode(xmlNodes[0]);
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
              if (customParameter != null && notificationData.AddCustomParameter(customParameter))
                arrayList.Add(nodeCustomChildren[index]);
            }
          }
        }
      }
      catch
      {
      }
      return notificationData;
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
      int count = this.Timestamp.Count;
      for (int index = 0; index < count; ++index)
        this.Timestamp[index].ToBitArray(ref bit_array, ref cursor);
      if (this.HoppingEvent != null)
        this.HoppingEvent.ToBitArray(ref bit_array, ref cursor);
      if (this.GPIEvent != null)
        this.GPIEvent.ToBitArray(ref bit_array, ref cursor);
      if (this.ROSpecEvent != null)
        this.ROSpecEvent.ToBitArray(ref bit_array, ref cursor);
      if (this.ReportBufferLevelWarningEvent != null)
        this.ReportBufferLevelWarningEvent.ToBitArray(ref bit_array, ref cursor);
      if (this.ReportBufferOverflowErrorEvent != null)
        this.ReportBufferOverflowErrorEvent.ToBitArray(ref bit_array, ref cursor);
      if (this.ReaderExceptionEvent != null)
        this.ReaderExceptionEvent.ToBitArray(ref bit_array, ref cursor);
      if (this.RFSurveyEvent != null)
        this.RFSurveyEvent.ToBitArray(ref bit_array, ref cursor);
      if (this.AISpecEvent != null)
        this.AISpecEvent.ToBitArray(ref bit_array, ref cursor);
      if (this.AntennaEvent != null)
        this.AntennaEvent.ToBitArray(ref bit_array, ref cursor);
      if (this.ConnectionAttemptEvent != null)
        this.ConnectionAttemptEvent.ToBitArray(ref bit_array, ref cursor);
      if (this.ConnectionCloseEvent != null)
        this.ConnectionCloseEvent.ToBitArray(ref bit_array, ref cursor);
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
      int count = this.Timestamp.Count;
      for (int index = 0; index < count; ++index)
        this.Timestamp[index].AppendToBitArray(bArr);
      if (this.HoppingEvent != null)
        this.HoppingEvent.AppendToBitArray(bArr);
      if (this.GPIEvent != null)
        this.GPIEvent.AppendToBitArray(bArr);
      if (this.ROSpecEvent != null)
        this.ROSpecEvent.AppendToBitArray(bArr);
      if (this.ReportBufferLevelWarningEvent != null)
        this.ReportBufferLevelWarningEvent.AppendToBitArray(bArr);
      if (this.ReportBufferOverflowErrorEvent != null)
        this.ReportBufferOverflowErrorEvent.AppendToBitArray(bArr);
      if (this.ReaderExceptionEvent != null)
        this.ReaderExceptionEvent.AppendToBitArray(bArr);
      if (this.RFSurveyEvent != null)
        this.RFSurveyEvent.AppendToBitArray(bArr);
      if (this.AISpecEvent != null)
        this.AISpecEvent.AppendToBitArray(bArr);
      if (this.AntennaEvent != null)
        this.AntennaEvent.AppendToBitArray(bArr);
      if (this.ConnectionAttemptEvent != null)
        this.ConnectionAttemptEvent.AppendToBitArray(bArr);
      if (this.ConnectionCloseEvent != null)
        this.ConnectionCloseEvent.AppendToBitArray(bArr);
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
