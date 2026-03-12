
using System;
using System.Linq;
using System.Text.RegularExpressions;

#nullable disable
namespace LLRPSdk
{
  internal static class ReaderUtilities
  {
    internal static ushort GetETSITxFreqIndex(double freq, FeatureSet readerCapabilities)
    {
      return (ushort) (readerCapabilities.TxFrequencies.IndexOf(freq) + 1);
    }

    internal static double GetETSITxFrequency(ushort index, FeatureSet readerCapabilities)
    {
      return index > (ushort) 0 && (int) index <= readerCapabilities.TxFrequencies.Count ? readerCapabilities.TxFrequencies[(int) index - 1] : 0.0;
    }

    internal static ushort GetFCCTxFreqIndex(double freq, FeatureSet readerCapabilities)
    {
      return (ushort) (readerCapabilities.TxFrequencies.OrderBy<double, double>((Func<double, double>) (d => d)).ToList<double>().IndexOf(freq) + 1);
    }

    internal static double GetFCCTxFrequency(ushort index, FeatureSet readerCapabilities)
    {
      return index > (ushort) 0 && (int) index <= readerCapabilities.TxFrequencies.Count ? readerCapabilities.TxFrequencies.OrderBy<double, double>((Func<double, double>) (d => d)).ToList<double>()[(int) index - 1] : 0.0;
    }

    internal static bool CheckIfSupportedFirmware(
      string firmwareVersion,
      string minimumFirmwareVersion)
    {
      return ReaderUtilities.CheckIfSupportedFirmware(new Version(Regex.Match(firmwareVersion, "((?:\\d+\\.){2,3}\\d+)(?:-[-\\w\\.]+)?(?:\\+[-\\w\\.]+)?").Groups[1].ToString()), new Version(minimumFirmwareVersion));
    }

    internal static bool CheckIfSupportedFirmware(
      Version firmwareVersion,
      Version minimumFirmwareVersion)
    {
      return firmwareVersion.CompareTo(minimumFirmwareVersion) >= 0;
    }
  }
}
