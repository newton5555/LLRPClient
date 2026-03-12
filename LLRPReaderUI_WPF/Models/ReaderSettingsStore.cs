using LLRPSdk;
using System.Linq;

namespace LLRPReaderUI_WPF.Models;

public class ReaderSettingsStore
{
    private readonly object syncRoot = new();
    private Settings? current;

    public bool HasValue
    {
        get
        {
            lock (syncRoot)
            {
                return current is not null;
            }
        }
    }

    public void Set(Settings settings)
    {
        lock (syncRoot)
        {
            current = Clone(settings);
        }
    }

    public bool TryGetSnapshot(out Settings? snapshot)
    {
        lock (syncRoot)
        {
            if (current is null)
            {
                snapshot = null;
                return false;
            }

            snapshot = Clone(current);
            return true;
        }
    }

    public void Clear()
    {
        lock (syncRoot)
        {
            current = null;
        }
    }

    private static Settings Clone(Settings source)
    {
        var clone = Settings.FromXmlString(source.ToXmlString());
        CopyAntennaConfigs(source, clone);
        return clone;
    }

    private static void CopyAntennaConfigs(Settings source, Settings clone)
    {
        if (source.Antennas?.AntennaConfigs is null || clone.Antennas?.AntennaConfigs is null)
        {
            return;
        }

        foreach (var sourceAntenna in source.Antennas.AntennaConfigs)
        {
            var targetAntenna = clone.Antennas.AntennaConfigs
                .FirstOrDefault(x => x.PortNumber == sourceAntenna.PortNumber);

            if (targetAntenna is null)
            {
                targetAntenna = new AntennaConfig(sourceAntenna.PortNumber);
                clone.Antennas.AntennaConfigs.Add(targetAntenna);
            }

            targetAntenna.PortName = sourceAntenna.PortName;
            targetAntenna.IsEnabled = sourceAntenna.IsEnabled;
            targetAntenna.MaxTxPower = sourceAntenna.MaxTxPower;
            targetAntenna.TxPowerInDbm = sourceAntenna.TxPowerInDbm;
            targetAntenna.MaxRxSensitivity = sourceAntenna.MaxRxSensitivity;
            targetAntenna.RxSensitivityInDbm = sourceAntenna.RxSensitivityInDbm;
        }
    }
}