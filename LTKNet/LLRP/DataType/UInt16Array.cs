
using System;
using System.Collections.Generic;


namespace Org.LLRP.LTK.LLRPV1.DataType
{
  [Serializable]
  public class UInt16Array
  {
    public List<ushort> data = new List<ushort>();

    public void Add(ushort val) => this.data.Add(val);

    public ushort this[int index]
    {
      get => this.data[index];
      set => this.data[index] = value;
    }

    public int Count => this.data.Count;

    public string ToHexString()
    {
      string hexString = string.Empty;
      for (int index = 0; index < this.data.Count; ++index)
      {
        ushort num1 = (ushort) ((uint) this.data[index] >> 8);
        ushort num2 = (ushort) ((uint) this.data[index] & (uint) byte.MaxValue);
        hexString = hexString + num1.ToString("X2") + num2.ToString("X2");
        if (index + 1 < this.data.Count)
          hexString += " ";
      }
      return hexString;
    }

    public string ToHexWordString()
    {
      string hexWordString = string.Empty;
      for (int index = 0; index < this.data.Count; ++index)
      {
        ushort num1 = (ushort) ((uint) this.data[index] >> 8);
        ushort num2 = (ushort) ((uint) this.data[index] & (uint) byte.MaxValue);
        hexWordString = hexWordString + num1.ToString("X2") + num2.ToString("X2");
        if (index + 1 < this.data.Count)
          hexWordString += " ";
      }
      return hexWordString;
    }

    public override string ToString()
    {
      try
      {
        ushort[] array = this.data.ToArray();
        string empty = string.Empty;
        for (int index = 0; index < array.Length; ++index)
        {
          empty += Convert.ToUInt16(array[index]).ToString();
          if (index + 1 < array.Length)
            empty += " ";
        }
        return empty;
      }
      catch
      {
        return "code error!";
      }
    }

    public static UInt16Array FromHexString(string str)
    {
      str = str.Trim();
      UInt16Array uint16Array = new UInt16Array();
      if (str != string.Empty)
      {
        string[] strArray = Util.SplitString(str, new char[5]
        {
          ',',
          ' ',
          '\t',
          '\n',
          '\r'
        }, (ushort) 4);
        for (int index = 0; index < strArray.Length; ++index)
        {
          try
          {
            if (strArray[index] != string.Empty)
              uint16Array.Add(Convert.ToUInt16(strArray[index], 16));
          }
          catch
          {
          }
        }
      }
      return uint16Array;
    }

    public static UInt16Array FromString(string str)
    {
      str = str.Trim();
      UInt16Array uint16Array = new UInt16Array();
      if (str != string.Empty)
      {
        string[] strArray = str.Split(new char[5]
        {
          ',',
          ' ',
          '\t',
          '\n',
          '\r'
        }, StringSplitOptions.RemoveEmptyEntries);
        for (int index = 0; index < strArray.Length; ++index)
        {
          try
          {
            if (strArray[index] != string.Empty)
              uint16Array.Add(Convert.ToUInt16(strArray[index], 10));
          }
          catch
          {
          }
        }
      }
      return uint16Array;
    }
  }
}
