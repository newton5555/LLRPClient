using System;
using System.Collections.Generic;
using System.Globalization;


namespace Org.LLRP.LTK.LLRPV1.DataType
{
  [Serializable]
  public class ByteArray
  {
    private List<byte> data = new List<byte>();

    public void Add(byte val) => this.data.Add(val);

    public byte this[int index]
    {
      get => this.data[index];
      set => this.data[index] = value;
    }

    public int Count => this.data.Count;

    public string ToHexWordString() => Util.ConvertByteArrayToHexWordString(this.data.ToArray());

    public string ToHexString() => Util.ConvertByteArrayToHexString(this.data.ToArray());

    public static ByteArray FromHexString(string str)
    {
      ByteArray byteArray = new ByteArray();
      str = str.Trim();
      string[] strArray = Util.SplitString(str, new char[5]
      {
        ',',
        ' ',
        '\t',
        '\n',
        '\r'
      }, (ushort) 2);
      byte[] numArray = new byte[strArray.Length];
      for (int index = 0; index < strArray.Length; ++index)
      {
        try
        {
          byteArray.Add(byte.Parse(strArray[index], NumberStyles.HexNumber));
        }
        catch
        {
          byteArray.Add((byte) 0);
        }
      }
      return byteArray;
    }

    public static ByteArray FromString(string str)
    {
      ByteArray byteArray = new ByteArray();
      str = str.Trim();
      string[] strArray = str.Split(new char[5]
      {
        ',',
        ' ',
        '\t',
        '\n',
        '\r'
      }, StringSplitOptions.RemoveEmptyEntries);
      byte[] numArray = new byte[strArray.Length];
      for (int index = 0; index < strArray.Length; ++index)
      {
        try
        {
          byteArray.Add(byte.Parse(strArray[index], NumberStyles.Integer));
        }
        catch
        {
          byteArray.Add((byte) 0);
        }
      }
      return byteArray;
    }

    public static ByteArray FromString(string str, Type t)
    {
      ByteArray byteArray = new ByteArray();
      str = str.Trim();
      string[] strArray = str.Split(new char[5]
      {
        ',',
        ' ',
        '\t',
        '\n',
        '\r'
      }, StringSplitOptions.RemoveEmptyEntries);
      byte[] numArray = new byte[strArray.Length];
      for (int index = 0; index < strArray.Length; ++index)
      {
        try
        {
          byteArray.Add((byte) (int) Enum.Parse(t, strArray[index], true));
        }
        catch
        {
          byteArray.Add((byte) 0);
        }
      }
      return byteArray;
    }

    public override string ToString()
    {
      try
      {
        byte[] array = this.data.ToArray();
        string str = string.Empty;
        for (int index = 0; index < array.Length; ++index)
          str = str + Convert.ToInt16(array[index]).ToString() + " ";
        return str;
      }
      catch
      {
        return "code error!";
      }
    }

    public string ToString(Type t)
    {
      string str = string.Empty;
      foreach (int num in this.data)
        str = str + Enum.GetName(t, (object) 1) + " ";
      return str;
    }

    public byte[] ToArray() => this.data.ToArray();
  }
}
