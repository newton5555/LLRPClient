

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Text;
using System.Text.RegularExpressions;

#nullable disable
namespace LLRPSdk
{
  /// <summary>Used to specify the mask to margin read</summary>
  public class MarginReadMask
  {
    private UInt16Array _Mask;
    private byte _BitLength;

    /// <summary>Gets the margin read mask.</summary>
    public UInt16Array Mask
    {
      get => this._Mask;
      private set
      {
        if (value == null)
          return;
        this._Mask = value;
      }
    }

    /// <summary>The number of bits to check.</summary>
    public byte BitLength
    {
      get => this._BitLength;
      set => this._BitLength = value;
    }

    /// <summary>
    /// Constructor for MarginReadMask initializes empty defaults.
    /// </summary>
    public MarginReadMask()
    {
      this.Mask = UInt16Array.FromHexString("");
      this.BitLength = (byte) 0;
    }

    /// <summary>
    /// Sets the margin read mask from the specified hex string.
    /// </summary>
    /// <param name="str">The hex string we're setting the mask with</param>
    public void SetMaskFromHexString(string str)
    {
      MarginReadMask.ValidateInputString(str, MarginReadMask.StringType.Hex);
      str = str.Trim();
      int length = str.Length;
      int totalWidth = length % 4 == 0 ? length : (length / 4 + 1) * 4;
      str = str.PadRight(totalWidth, '0');
      this.Mask = UInt16Array.FromHexString(str);
      this.BitLength = Convert.ToByte(length * 4);
    }

    /// <summary>
    /// Sets the margin read mask from the specified bit string.
    /// </summary>
    /// <param name="str">The bit string we're setting the mask with</param>
    public void SetMaskFromBitString(string str)
    {
      MarginReadMask.ValidateInputString(str, MarginReadMask.StringType.Bit);
      str = str.Trim();
      int length = str.Length;
      int totalWidth = length % 16 == 0 ? length : (length / 16 + 1) * 16;
      str = str.PadRight(totalWidth, '0');
      this.Mask = UInt16Array.FromHexString(MarginReadMask.BinaryStringToHexString(str));
      this.BitLength = Convert.ToByte(length);
    }

    /// <summary>
    /// Converts the margin read mask into a hexadecimal string.
    /// </summary>
    /// <returns>Hexadecimal string representation of the margin mask</returns>
    public string ToHexString()
    {
      string empty = string.Empty;
      foreach (ushort num in this.Mask.data)
        empty += num.ToString("X4");
      return empty;
    }

    /// <summary>Converts a binary string to a hex string</summary>
    /// <param name="binary">The binary string to convert</param>
    /// <returns>The converted hex string</returns>
    private static string BinaryStringToHexString(string binary)
    {
      StringBuilder stringBuilder = new StringBuilder(binary.Length / 4 + 1);
      for (int startIndex = 0; startIndex < binary.Length; startIndex += 8)
      {
        string str = binary.Substring(startIndex, 8);
        stringBuilder.AppendFormat("{0:X2}", (object) Convert.ToByte(str, 2));
      }
      return stringBuilder.ToString();
    }

    /// <summary>
    /// Verifies the string user input and throws the necessary exceptions
    /// </summary>
    /// <param name="str">The string we're verifying</param>
    /// <param name="stringType">The pattern we're verifying the string against</param>
    private static void ValidateInputString(string str, MarginReadMask.StringType stringType)
    {
      str = !string.IsNullOrEmpty(str) ? str.Trim() : throw new LLRPSdkException("The given string is null or empty.");
      if (str.Length == 0)
        throw new LLRPSdkException("The given string has no valid characters.");
      if (stringType == MarginReadMask.StringType.Bit && !Regex.IsMatch(str, "\\A\\b[01]+\\b\\Z"))
        throw new LLRPSdkException("The given string contains non-bit characters.");
      if (stringType == MarginReadMask.StringType.Hex && !Regex.IsMatch(str, "\\A\\b[0-9a-fA-F]+\\b\\Z"))
        throw new LLRPSdkException("The given string contains non-hex characters.");
    }

    /// <summary>Enum representing the string type</summary>
    private enum StringType
    {
      /// <summary>The string is made up of 1s and 0s</summary>
      Bit,
      /// <summary>The string is made up of hexadecimal characters.</summary>
      Hex,
    }
  }
}
