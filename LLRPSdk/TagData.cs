
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Class used to hold tag data in the raw form in which it is stored
  /// on the tag (list of unsigned shorts).
  /// </summary>
  public class TagData
  {
    private List<ushort> data = new List<ushort>();

    /// <summary>The size of the tag data, in bytes.</summary>
    public int CountBytes => this.data.Count * 2;

    /// <summary>
    /// Constructor that creates a TagData object populated with data from
    /// a list of words (ushort).
    /// </summary>
    /// <param name="data">Populated list of words.</param>
    /// <returns>Populated TagData object</returns>
    public static TagData FromWordList(List<ushort> data)
    {
      return new TagData() { data = data };
    }

    /// <summary>
    /// Constructor that creates a TagData object populated with data from
    /// a byte array.
    /// </summary>
    /// <param name="data">Populated byte array.</param>
    /// <returns>Populated TagData object</returns>
    public static TagData FromByteArray(byte[] data)
    {
      return TagData.FromByteList(new List<byte>((IEnumerable<byte>) data));
    }

    /// <summary>
    /// Constructor that creates a TagData object populated with data from
    /// an unsigned int.
    /// </summary>
    /// <param name="data">Unsigned int.</param>
    /// <returns>Populated TagData object.</returns>
    public static TagData FromUnsignedInt(uint data)
    {
      List<byte> data1 = new List<byte>();
      for (int index = 0; index < 4; ++index)
      {
        data1.Insert(0, (byte) (data & (uint) byte.MaxValue));
        data >>= 8;
      }
      return TagData.FromByteList(data1);
    }

    /// <summary>
    /// Constructor that creates a TagData object populated with data from
    /// an array of words
    /// </summary>
    /// <param name="data">Populated array of words.</param>
    /// <returns>Populated TagData object.</returns>
    public static TagData FromWordArray(ushort[] data)
    {
      return TagData.FromWordList(new List<ushort>((IEnumerable<ushort>) data));
    }

    /// <summary>
    /// Constructor that creates a TagData object populated with data from a single 16-bit word
    /// </summary>
    /// <param name="data">Word to add to the TagData object.</param>
    /// <returns>Populated TagData object.</returns>
    /// <exception cref="T:LLRPSdk.LLRPSdkException">
    /// Thrown when a null word is provided.
    /// </exception>
    public static TagData FromWord(ushort data)
    {
      return TagData.FromWordList(new List<ushort>()
      {
        data
      });
    }

    /// <summary>
    /// Constructor that creates a TagData object populated with data from
    /// a list of bytes.
    /// </summary>
    /// <param name="data">Populated list of bytes.</param>
    /// <returns>Populated TagData object.</returns>
    public static TagData FromByteList(List<byte> data)
    {
      TagData tagData = new TagData();
      ushort num1 = 0;
      int index = 0;
      int num2 = 0;
      if (data.Count % 2 != 0)
        num2 = 1;
      for (; index < data.Count; ++index)
      {
        if (num2 == 0)
        {
          num1 = (ushort) data[index];
          num2 = 1;
        }
        else
        {
          num1 = (ushort) ((uint) (ushort) ((uint) num1 << 8) | (uint) data[index]);
          tagData.data.Add(num1);
          num2 = 0;
        }
      }
      return tagData;
    }

    /// <summary>
    /// Constructor that creates a TagData object populated with data from
    /// a Hex string of arbitrary length.
    /// </summary>
    /// <param name="hex">Hex number represented as a string.</param>
    /// <returns>Populated TagData object.</returns>
    /// <exception cref="T:LLRPSdk.LLRPSdkException">
    /// Thrown when a null or non-hex string is provided.
    /// </exception>
    public static TagData FromHexString(string hex)
    {
      hex = hex != null ? new Regex("[-\\s]").Replace(hex, "") : throw new LLRPSdkException("Conversion failed. The hex string cannot be null.");
      if (hex.Length <= 0)
        throw new LLRPSdkException("Conversion failed. Invalid hex string supplied. (" + hex + ")");
      if (new Regex("[0-9a-fA-F -]").Replace(hex, "").Length != 0)
        throw new LLRPSdkException("Conversion failed. Invalid hex string supplied. (" + hex + ")");
      while (hex.Length % 4 != 0)
        hex += "0";
      TagData tagData = new TagData();
      for (int startIndex = 0; startIndex < hex.Length; startIndex += 4)
      {
        string s = hex.Substring(startIndex, 4);
        tagData.data.Add(ushort.Parse(s, NumberStyles.AllowHexSpecifier));
      }
      return tagData;
    }

    /// <summary>
    /// Conversion utility that outputs the data of the TagData object as a
    /// list.
    /// </summary>
    /// <returns>Contents of TagData as a List of words (ushort).</returns>
    public List<ushort> ToList() => this.data;

    /// <summary>
    /// Conversion utility that outputs the data of the TagData object as a
    /// string in hexadecimal format, with each word separated by a space.
    /// </summary>
    /// <returns>Contents of TagData represented as a string.</returns>
    public string ToHexWordString()
    {
      string str = "";
      foreach (ushort num in this.data)
        str = str + num.ToString("X4") + " ";
      return str.Trim();
    }

    /// <summary>
    /// Conversion utility that outputs the data of the TagData object as a
    /// string in hexadecimal format, with no spaces between each word.
    /// </summary>
    /// <returns>Contents of TagData represented as a string.</returns>
    public string ToHexString() => this.ToHexWordString().Replace(" ", "");

    /// <summary>
    /// Conversion utility that outputs the data of the TagData object as a
    /// string in hexadecimal format, with each word separated by a space.
    /// </summary>
    /// <returns>Contents of TagData represented as a string.</returns>
    public override string ToString() => this.ToHexWordString();

    /// <summary>
    /// Conversion utility that outputs the data of the TagData object as an
    /// unsigned int.  This will only work for TagData objects that contain
    /// 1 or 2 16-bit words of data.
    /// </summary>
    /// <returns>Up to 2 words of data as an unsigned int.</returns>
    /// <exception cref="T:LLRPSdk.LLRPSdkException">
    /// Thrown when the TagData object contains more than 32-bits of data.
    /// </exception>
    public uint ToUnsignedInt()
    {
      if (this.data.Count == 0)
        return 0;
      if (this.data.Count == 1)
        return (uint) this.data[0];
      if (this.data.Count == 2)
        return (uint) this.data[0] << 16 | (uint) this.data[1];
      throw new LLRPSdkException("Conversion failed. Data array should only contain up to 32-bits of data.");
    }
  }
}
