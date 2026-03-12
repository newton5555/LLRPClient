using System;
using System.Collections.Generic;


namespace Org.LLRP.LTK.LLRPV1.DataType
{
  [Serializable]
  public class LLRPBitArray
  {
    private List<bool> data = new List<bool>();

    public void Add(bool val) => this.data.Add(val);

    public bool this[int index]
    {
      get => this.data[index];
      set => this.data[index] = value;
    }

    public int Count
    {
      get => this.data.Count;
      set
      {
        if (this.data.Count <= value)
          return;
        this.data.RemoveRange(value, this.data.Count - value);
      }
    }

    public string ToHexString()
    {
      try
      {
        return Util.ConvertByteArrayToHexString(Util.ConvertBitArrayToByteArray(this.data.ToArray()));
      }
      catch
      {
        return "code error!";
      }
    }

    public string ToHexWordString()
    {
      try
      {
        return Util.ConvertByteArrayToHexWordString(Util.ConvertBitArrayToByteArray(this.data.ToArray()));
      }
      catch
      {
        return "code error!";
      }
    }

    public override string ToString()
    {
      try
      {
        byte[] byteArray = Util.ConvertBitArrayToByteArray(this.data.ToArray());
        string empty = string.Empty;
        for (int index = 0; index < byteArray.Length; ++index)
        {
          empty += Convert.ToInt16(byteArray[index]).ToString();
          if (index + 1 < byteArray.Length)
            empty += " ";
        }
        return empty;
      }
      catch
      {
        return "code error!";
      }
    }

    public static LLRPBitArray FromBinString(string str)
    {
      LLRPBitArray llrpBitArray = new LLRPBitArray();
      for (int index = 0; index < str.Length; ++index)
        llrpBitArray.Add(str[index] == '1');
      return llrpBitArray;
    }

    public static LLRPBitArray FromString(string str) => LLRPBitArray.FromHexString(str);

    public static LLRPBitArray FromHexString(string str)
    {
      return LLRPBitArray.FromBinString(Util.ConvertHexStringToBinaryString(str));
    }
  }
}
