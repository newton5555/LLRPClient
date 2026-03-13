using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class PARAM_GeneralDeviceCapabilities : Parameter
  {
    public ushort MaxNumberOfAntennaSupported;
    private short MaxNumberOfAntennaSupported_len;
    public bool CanSetAntennaProperties;
    private short CanSetAntennaProperties_len;
    public bool HasUTCClockCapability;
    private short HasUTCClockCapability_len;
    private const ushort param_reserved_len5 = 14;
    public uint DeviceManufacturerName;
    private short DeviceManufacturerName_len;
    public uint ModelName;
    private short ModelName_len;
    public string ReaderFirmwareVersion = string.Empty;
    private short ReaderFirmwareVersion_len;
    public PARAM_ReceiveSensitivityTableEntry[] ReceiveSensitivityTableEntry;
    public PARAM_PerAntennaReceiveSensitivityRange[] PerAntennaReceiveSensitivityRange;
    public PARAM_GPIOCapabilities GPIOCapabilities;
    public PARAM_PerAntennaAirProtocol[] PerAntennaAirProtocol;

    public PARAM_GeneralDeviceCapabilities() => this.typeID = (ushort) 137;

    public static PARAM_GeneralDeviceCapabilities FromBitArray(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (PARAM_GeneralDeviceCapabilities) null;
      int num1 = cursor;
      int num2 = length;
      ArrayList arrayList1 = new ArrayList();
      PARAM_GeneralDeviceCapabilities deviceCapabilities = new PARAM_GeneralDeviceCapabilities();
      deviceCapabilities.tvCoding = bit_array[cursor];
      int val;
      if (deviceCapabilities.tvCoding)
      {
        ++cursor;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 7);
      }
      else
      {
        cursor += 6;
        val = (int) (ulong) Util.CalculateVal(ref bit_array, ref cursor, 10);
        deviceCapabilities.length = (ushort) Util.DetermineFieldLength(ref bit_array, ref cursor);
        num2 = num1 + (int) deviceCapabilities.length * 8;
      }
      if (val != (int) deviceCapabilities.TypeID)
      {
        cursor = num1;
        return (PARAM_GeneralDeviceCapabilities) null;
      }
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len1 = 16;
      object obj;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (ushort), field_len1);
      deviceCapabilities.MaxNumberOfAntennaSupported = (ushort) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len2 = 1;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (bool), field_len2);
      deviceCapabilities.CanSetAntennaProperties = (bool) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len3 = 1;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (bool), field_len3);
      deviceCapabilities.HasUTCClockCapability = (bool) obj;
      cursor += 14;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len4 = 32;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len4);
      deviceCapabilities.DeviceManufacturerName = (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int field_len5 = 32;
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (uint), field_len5);
      deviceCapabilities.ModelName = (uint) obj;
      if (cursor > length || cursor > num2)
        throw new Exception("Input data is not a complete LLRP message");
      int fieldLength = Util.DetermineFieldLength(ref bit_array, ref cursor);
      Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof (string), fieldLength);
      deviceCapabilities.ReaderFirmwareVersion = (string) obj;
      ArrayList arrayList2 = new ArrayList();
      PARAM_ReceiveSensitivityTableEntry sensitivityTableEntry;
      while ((sensitivityTableEntry = PARAM_ReceiveSensitivityTableEntry.FromBitArray(ref bit_array, ref cursor, length)) != null)
        arrayList2.Add((object) sensitivityTableEntry);
      if (arrayList2.Count > 0)
      {
        deviceCapabilities.ReceiveSensitivityTableEntry = new PARAM_ReceiveSensitivityTableEntry[arrayList2.Count];
        for (int index = 0; index < arrayList2.Count; ++index)
          deviceCapabilities.ReceiveSensitivityTableEntry[index] = (PARAM_ReceiveSensitivityTableEntry) arrayList2[index];
      }
      ArrayList arrayList3 = new ArrayList();
      PARAM_PerAntennaReceiveSensitivityRange sensitivityRange;
      while ((sensitivityRange = PARAM_PerAntennaReceiveSensitivityRange.FromBitArray(ref bit_array, ref cursor, length)) != null)
        arrayList3.Add((object) sensitivityRange);
      if (arrayList3.Count > 0)
      {
        deviceCapabilities.PerAntennaReceiveSensitivityRange = new PARAM_PerAntennaReceiveSensitivityRange[arrayList3.Count];
        for (int index = 0; index < arrayList3.Count; ++index)
          deviceCapabilities.PerAntennaReceiveSensitivityRange[index] = (PARAM_PerAntennaReceiveSensitivityRange) arrayList3[index];
      }
      deviceCapabilities.GPIOCapabilities = PARAM_GPIOCapabilities.FromBitArray(ref bit_array, ref cursor, length);
      ArrayList arrayList4 = new ArrayList();
      PARAM_PerAntennaAirProtocol antennaAirProtocol;
      while ((antennaAirProtocol = PARAM_PerAntennaAirProtocol.FromBitArray(ref bit_array, ref cursor, length)) != null)
        arrayList4.Add((object) antennaAirProtocol);
      if (arrayList4.Count > 0)
      {
        deviceCapabilities.PerAntennaAirProtocol = new PARAM_PerAntennaAirProtocol[arrayList4.Count];
        for (int index = 0; index < arrayList4.Count; ++index)
          deviceCapabilities.PerAntennaAirProtocol[index] = (PARAM_PerAntennaAirProtocol) arrayList4[index];
      }
      return deviceCapabilities;
    }

    public override string ToString()
    {
      string str = "<GeneralDeviceCapabilities>" + "\r\n";
      int antennaSupported = (int) this.MaxNumberOfAntennaSupported;
      try
      {
        str = str + "  <MaxNumberOfAntennaSupported>" + Util.ConvertValueTypeToString((object) this.MaxNumberOfAntennaSupported, "u16", "") + "</MaxNumberOfAntennaSupported>";
        str += "\r\n";
      }
      catch
      {
      }
      int num1 = this.CanSetAntennaProperties ? 1 : 0;
      try
      {
        str = str + "  <CanSetAntennaProperties>" + Util.ConvertValueTypeToString((object) this.CanSetAntennaProperties, "u1", "") + "</CanSetAntennaProperties>";
        str += "\r\n";
      }
      catch
      {
      }
      int num2 = this.HasUTCClockCapability ? 1 : 0;
      try
      {
        str = str + "  <HasUTCClockCapability>" + Util.ConvertValueTypeToString((object) this.HasUTCClockCapability, "u1", "") + "</HasUTCClockCapability>";
        str += "\r\n";
      }
      catch
      {
      }
      int manufacturerName = (int) this.DeviceManufacturerName;
      try
      {
        str = str + "  <DeviceManufacturerName>" + Util.ConvertValueTypeToString((object) this.DeviceManufacturerName, "u32", "") + "</DeviceManufacturerName>";
        str += "\r\n";
      }
      catch
      {
      }
      int modelName = (int) this.ModelName;
      try
      {
        str = str + "  <ModelName>" + Util.ConvertValueTypeToString((object) this.ModelName, "u32", "") + "</ModelName>";
        str += "\r\n";
      }
      catch
      {
      }
      if (this.ReaderFirmwareVersion != null)
      {
        try
        {
          str = str + "  <ReaderFirmwareVersion>" + Util.ConvertArrayTypeToString((object) this.ReaderFirmwareVersion, "utf8v", "UTF8") + "</ReaderFirmwareVersion>";
          str += "\r\n";
        }
        catch
        {
        }
      }
      if (this.ReceiveSensitivityTableEntry != null)
      {
        int length = this.ReceiveSensitivityTableEntry.Length;
        for (int index = 0; index < length; ++index)
          str += Util.Indent(this.ReceiveSensitivityTableEntry[index].ToString());
      }
      if (this.PerAntennaReceiveSensitivityRange != null)
      {
        int length = this.PerAntennaReceiveSensitivityRange.Length;
        for (int index = 0; index < length; ++index)
          str += Util.Indent(this.PerAntennaReceiveSensitivityRange[index].ToString());
      }
      if (this.GPIOCapabilities != null)
        str += Util.Indent(this.GPIOCapabilities.ToString());
      if (this.PerAntennaAirProtocol != null)
      {
        int length = this.PerAntennaAirProtocol.Length;
        for (int index = 0; index < length; ++index)
          str += Util.Indent(this.PerAntennaAirProtocol[index].ToString());
      }
      return str + "</GeneralDeviceCapabilities>" + "\r\n";
    }

    public static PARAM_GeneralDeviceCapabilities FromXmlNode(XmlNode node)
    {
      ArrayList arrayList = new ArrayList();
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      nsmgr.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      nsmgr.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
      PARAM_GeneralDeviceCapabilities deviceCapabilities = new PARAM_GeneralDeviceCapabilities();
      string nodeValue1 = XmlUtil.GetNodeValue(node, "MaxNumberOfAntennaSupported");
      deviceCapabilities.MaxNumberOfAntennaSupported = (ushort) Util.ParseValueTypeFromString(nodeValue1, "u16", "");
      string nodeValue2 = XmlUtil.GetNodeValue(node, "CanSetAntennaProperties");
      deviceCapabilities.CanSetAntennaProperties = (bool) Util.ParseValueTypeFromString(nodeValue2, "u1", "");
      string nodeValue3 = XmlUtil.GetNodeValue(node, "HasUTCClockCapability");
      deviceCapabilities.HasUTCClockCapability = (bool) Util.ParseValueTypeFromString(nodeValue3, "u1", "");
      string nodeValue4 = XmlUtil.GetNodeValue(node, "DeviceManufacturerName");
      deviceCapabilities.DeviceManufacturerName = (uint) Util.ParseValueTypeFromString(nodeValue4, "u32", "");
      string nodeValue5 = XmlUtil.GetNodeValue(node, "ModelName");
      deviceCapabilities.ModelName = (uint) Util.ParseValueTypeFromString(nodeValue5, "u32", "");
      string nodeValue6 = XmlUtil.GetNodeValue(node, "ReaderFirmwareVersion");
      deviceCapabilities.ReaderFirmwareVersion = (string) Util.ParseArrayTypeFromString(nodeValue6, "utf8v", "UTF8");
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "ReceiveSensitivityTableEntry", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
          {
            deviceCapabilities.ReceiveSensitivityTableEntry = new PARAM_ReceiveSensitivityTableEntry[xmlNodes.Count];
            for (int i = 0; i < xmlNodes.Count; ++i)
              deviceCapabilities.ReceiveSensitivityTableEntry[i] = PARAM_ReceiveSensitivityTableEntry.FromXmlNode(xmlNodes[i]);
          }
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "PerAntennaReceiveSensitivityRange", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
          {
            deviceCapabilities.PerAntennaReceiveSensitivityRange = new PARAM_PerAntennaReceiveSensitivityRange[xmlNodes.Count];
            for (int i = 0; i < xmlNodes.Count; ++i)
              deviceCapabilities.PerAntennaReceiveSensitivityRange[i] = PARAM_PerAntennaReceiveSensitivityRange.FromXmlNode(xmlNodes[i]);
          }
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "GPIOCapabilities", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
            deviceCapabilities.GPIOCapabilities = PARAM_GPIOCapabilities.FromXmlNode(xmlNodes[0]);
        }
      }
      catch
      {
      }
      try
      {
        XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "PerAntennaAirProtocol", nsmgr);
        if (xmlNodes != null)
        {
          if (xmlNodes.Count != 0)
          {
            deviceCapabilities.PerAntennaAirProtocol = new PARAM_PerAntennaAirProtocol[xmlNodes.Count];
            for (int i = 0; i < xmlNodes.Count; ++i)
              deviceCapabilities.PerAntennaAirProtocol[i] = PARAM_PerAntennaAirProtocol.FromXmlNode(xmlNodes[i]);
          }
        }
      }
      catch
      {
      }
      return deviceCapabilities;
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
      int antennaSupported = (int) this.MaxNumberOfAntennaSupported;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.MaxNumberOfAntennaSupported, (int) this.MaxNumberOfAntennaSupported_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int num2 = this.CanSetAntennaProperties ? 1 : 0;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.CanSetAntennaProperties, (int) this.CanSetAntennaProperties_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int num3 = this.HasUTCClockCapability ? 1 : 0;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.HasUTCClockCapability, (int) this.HasUTCClockCapability_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      cursor += 14;
      int manufacturerName = (int) this.DeviceManufacturerName;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.DeviceManufacturerName, (int) this.DeviceManufacturerName_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      int modelName = (int) this.ModelName;
      try
      {
        BitArray bitArray = Util.ConvertObjToBitArray((object) this.ModelName, (int) this.ModelName_len);
        bitArray.CopyTo((Array) bit_array, cursor);
        cursor += bitArray.Length;
      }
      catch
      {
      }
      if (this.ReaderFirmwareVersion != null)
      {
        try
        {
          Util.ConvertIntToBitArray((uint) this.ReaderFirmwareVersion.Length, 16).CopyTo((Array) bit_array, cursor);
          cursor += 16;
          BitArray bitArray = Util.ConvertObjToBitArray((object) this.ReaderFirmwareVersion, (int) this.ReaderFirmwareVersion_len);
          bitArray.CopyTo((Array) bit_array, cursor);
          cursor += bitArray.Length;
        }
        catch
        {
        }
      }
      if (this.ReceiveSensitivityTableEntry != null)
      {
        int length = this.ReceiveSensitivityTableEntry.Length;
        for (int index = 0; index < length; ++index)
          this.ReceiveSensitivityTableEntry[index].ToBitArray(ref bit_array, ref cursor);
      }
      if (this.PerAntennaReceiveSensitivityRange != null)
      {
        int length = this.PerAntennaReceiveSensitivityRange.Length;
        for (int index = 0; index < length; ++index)
          this.PerAntennaReceiveSensitivityRange[index].ToBitArray(ref bit_array, ref cursor);
      }
      if (this.GPIOCapabilities != null)
        this.GPIOCapabilities.ToBitArray(ref bit_array, ref cursor);
      if (this.PerAntennaAirProtocol != null)
      {
        int length = this.PerAntennaAirProtocol.Length;
        for (int index = 0; index < length; ++index)
          this.PerAntennaAirProtocol[index].ToBitArray(ref bit_array, ref cursor);
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
      int antennaSupported = (int) this.MaxNumberOfAntennaSupported;
      try
      {
        Util.AppendObjToBitArray((object) this.MaxNumberOfAntennaSupported, (int) this.MaxNumberOfAntennaSupported_len, bArr);
      }
      catch
      {
      }
      int num1 = this.CanSetAntennaProperties ? 1 : 0;
      try
      {
        Util.AppendObjToBitArray((object) this.CanSetAntennaProperties, (int) this.CanSetAntennaProperties_len, bArr);
      }
      catch
      {
      }
      int num2 = this.HasUTCClockCapability ? 1 : 0;
      try
      {
        Util.AppendObjToBitArray((object) this.HasUTCClockCapability, (int) this.HasUTCClockCapability_len, bArr);
      }
      catch
      {
      }
      bArr.Length += 14;
      int manufacturerName = (int) this.DeviceManufacturerName;
      try
      {
        Util.AppendObjToBitArray((object) this.DeviceManufacturerName, (int) this.DeviceManufacturerName_len, bArr);
      }
      catch
      {
      }
      int modelName = (int) this.ModelName;
      try
      {
        Util.AppendObjToBitArray((object) this.ModelName, (int) this.ModelName_len, bArr);
      }
      catch
      {
      }
      if (this.ReaderFirmwareVersion != null)
      {
        try
        {
          Util.AppendIntToBitArray((uint) this.ReaderFirmwareVersion.Length, 16, bArr);
          Util.AppendObjToBitArray((object) this.ReaderFirmwareVersion, (int) this.ReaderFirmwareVersion_len, bArr);
        }
        catch
        {
        }
      }
      if (this.ReceiveSensitivityTableEntry != null)
      {
        int length3 = this.ReceiveSensitivityTableEntry.Length;
        for (int index = 0; index < length3; ++index)
          this.ReceiveSensitivityTableEntry[index].AppendToBitArray(bArr);
      }
      if (this.PerAntennaReceiveSensitivityRange != null)
      {
        int length4 = this.PerAntennaReceiveSensitivityRange.Length;
        for (int index = 0; index < length4; ++index)
          this.PerAntennaReceiveSensitivityRange[index].AppendToBitArray(bArr);
      }
      if (this.GPIOCapabilities != null)
        this.GPIOCapabilities.AppendToBitArray(bArr);
      if (this.PerAntennaAirProtocol != null)
      {
        int length5 = this.PerAntennaAirProtocol.Length;
        for (int index = 0; index < length5; ++index)
          this.PerAntennaAirProtocol[index].AppendToBitArray(bArr);
      }
      if (this.tvCoding)
        return;
      BitArray bitArray = Util.ConvertIntToBitArray((uint) (bArr.Length - length1) / 8U, 16);
      for (int index = 0; index < bitArray.Length; ++index)
        bArr[length1 + 16 + index] = bitArray[index];
    }
  }
}
