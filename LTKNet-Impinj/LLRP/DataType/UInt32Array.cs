
using System;
using System.Collections.Generic;


namespace Org.LLRP.LTK.LLRPV1.DataType
{
  [Serializable]
  public class UInt32Array
  {
    public List<uint> data = new List<uint>();

    public void Add(uint val) => this.data.Add(val);

    public uint this[int index]
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
        int num1 = (int) (ushort) (this.data[index] >> 16);
        ushort num2 = (ushort) (this.data[index] & (uint) ushort.MaxValue);
        ushort num3 = (ushort) (num1 >> 8);
        ushort num4 = (ushort) (num1 & (int) byte.MaxValue);
        ushort num5 = (ushort) ((uint) num2 >> 8);
        ushort num6 = (ushort) ((uint) num2 & (uint) byte.MaxValue);
        hexString = hexString + num3.ToString("X2") + num4.ToString("X2") + num5.ToString("X2") + num6.ToString("X2");
      }
      return hexString;
    }

    public override string ToString()
    {
      try
      {
        uint[] array = this.data.ToArray();
        string empty = string.Empty;
        for (int index = 0; index < array.Length; ++index)
        {
          empty += Convert.ToUInt32(array[index]).ToString();
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

    public static UInt32Array FromHexString(string str)
    {
      str = str.Trim();
      UInt32Array uint32Array = new UInt32Array();
      string str1 = str;
      char[] seperator = new char[5]
      {
        ',',
        ' ',
        '\t',
        '\n',
        '\r'
      };
      foreach (string str2 in Util.SplitString(str1, seperator, (ushort) 8))
      {
        try
        {
          uint32Array.Add(Convert.ToUInt32(str2, 16));
        }
        catch
        {
        }
      }
      return uint32Array;
    }

    public static UInt32Array FromString(string str)
    {
      str = str.Trim();
      UInt32Array uint32Array = new UInt32Array();
      string str1 = str;
      char[] separator = new char[5]
      {
        ',',
        ' ',
        '\t',
        '\n',
        '\r'
      };
      foreach (string str2 in str1.Split(separator, StringSplitOptions.RemoveEmptyEntries))
      {
        try
        {
          uint32Array.Add(Convert.ToUInt32(str2, 10));
        }
        catch
        {
        }
      }
      return uint32Array;
    }
  }
}
