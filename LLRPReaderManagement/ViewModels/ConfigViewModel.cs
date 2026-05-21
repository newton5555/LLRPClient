using LLRPReaderManagement.Models;
using LLRPReaderManagement.Services;
using LLRPReaderManagement.State;
using LLRPSdk;

namespace LLRPReaderManagement.ViewModels;

public sealed class ConfigViewModel(AppState state, ReaderManagementService readers)
{
    private static readonly Dictionary<uint, string> RfModePrefixes = new()
    {
        { 113, "P0" }, { 45, "P1" }, { 203, "P2" }, { 107, "P3" }, { 220, "P4" },
        { 101, "P8" }, { 111, "P9" }, { 4185, "P10" }, { 4146, "P11" },
        { 4148, "P12" }, { 4124, "P13" }, { 5185, "P18" }, { 5146, "P19" },
        { 5148, "P20" }, { 5124, "P21" }
    };

    public AppState State => state;

    public IReadOnlyList<FeatureItem> Features => new[]
    {
        new FeatureItem("Reader Model", state.ReaderModel),
        new FeatureItem("Firmware", state.Firmware),
        new FeatureItem("Antennas", state.AntennaCount == 0 ? "-" : state.AntennaCount.ToString()),
        new FeatureItem("GPI / GPO", $"{state.GpiCount} / {state.GpoCount}"),
        new FeatureItem("RF Mode", state.Settings?.RfMode?.ToString() ?? "-"),
        new FeatureItem("Session", state.Settings?.Session.ToString() ?? "-"),
        new FeatureItem("Tag Population", state.Settings?.TagPopulationEstimate.ToString() ?? "-")
    };

    public IReadOnlyList<RfModeOption> RfModeOptions
    {
        get
        {
            var featureSet = state.FeatureSet;
            var ids = featureSet?.RfModes?
                .Where(x => x.HasValue)
                .Select(x => x.GetValueOrDefault())
                .Distinct()
                .OrderBy(x => x)
                .ToList() ?? [];

            return ids.Select(id =>
            {
                var detail = featureSet?.RfModeDetails.TryGetValue(id, out var text) == true ? text : string.Empty;
                var display = string.IsNullOrWhiteSpace(detail) ? id.ToString() : $"{id} - {detail}";
                if (RfModePrefixes.TryGetValue(id, out var prefix))
                {
                    display = $"{prefix} - {display}";
                }

                return new RfModeOption(id, display);
            }).ToList();
        }
    }

    public IReadOnlyList<double> TxPowerOptions => state.FeatureSet?.TxPowers?
        .Select(x => x.Dbm)
        .Distinct()
        .OrderBy(x => x)
        .ToList() ?? [];

    public IReadOnlyList<double> RxSensitivityOptions => state.FeatureSet?.RxSensitivities?
        .Select(x => x.Dbm)
        .Distinct()
        .OrderBy(x => x)
        .ToList() ?? [];

    public IReadOnlyList<ushort> HopTableOptions
    {
        get
        {
            var ids = new List<ushort> { 0 };
            if (state.FeatureSet?.HopTables is not null)
            {
                ids.AddRange(state.FeatureSet.HopTables.Select(x => x.HopTableId).Where(x => x != 0));
            }

            return ids.Distinct().OrderBy(x => x).ToList();
        }
    }

    public ushort ChannelIndex => state.Settings?.ChannelIndex ?? 0;

    public void SetChannelIndex(ushort value)
    {
        if (state.Settings is not null)
        {
            state.Settings.ChannelIndex = value;
        }
    }

    public string SelectedHopTableFrequencies
    {
        get
        {
            var hopTableId = state.Settings?.HopTableId ?? 0;
            var featureSet = state.FeatureSet;
            var table = featureSet?.HopTables?.FirstOrDefault(x => x.HopTableId == hopTableId);
            if (table is not null)
            {
                return string.Join(", ", table.Frequencies.Select(x => x.ToString("F3")));
            }

            if (hopTableId == 0 && featureSet?.IsHoppingRegion == false)
            {
                return string.Join(", ", featureSet.TxFrequencies.Select(x => x.ToString("F3")));
            }

            return "-";
        }
    }

    public string SelectedChannelFrequency
    {
        get
        {
            if (state.Settings is null || state.Settings.ChannelIndex == 0)
            {
                return "-";
            }

            var featureSet = state.FeatureSet;
            var table = featureSet?.HopTables?.FirstOrDefault(x => x.HopTableId == state.Settings.HopTableId);
            var frequencies = table?.Frequencies
                ?? (state.Settings.HopTableId == 0 && featureSet?.IsHoppingRegion == false ? featureSet.TxFrequencies : null);

            return frequencies is not null && state.Settings.ChannelIndex <= frequencies.Count
                ? $"{frequencies[state.Settings.ChannelIndex - 1]:F3} MHz"
                : "-";
        }
    }

    public IReadOnlyList<GpiUiItem> Gpis
    {
        get
        {
            var statusByPort = state.Status?.Gpis.Cast<GpiStatus>().ToDictionary(x => x.PortNumber, x => x.State) ?? [];
            return Enumerable.Range(1, state.GpiCount)
                .Select(i =>
                {
                    var port = (ushort)i;
                    var hasStatus = statusByPort.TryGetValue(port, out var status);
                    return new GpiUiItem(port, hasStatus ? status : null, hasStatus ? (status ? "HIGH" : "LOW") : "Unknown");
                })
                .ToList();
        }
    }

    public IReadOnlyList<GpoUiItem> Gpos
    {
        get
        {
            var statusByPort = state.Status?.GpoStates.Cast<GpoStatus>().ToDictionary(x => x.PortNumber, x => x.State) ?? [];
            return Enumerable.Range(1, state.GpoCount)
                .Select(i =>
                {
                    var port = (ushort)i;
                    var hasStatus = statusByPort.TryGetValue(port, out var status);
                    return new GpoUiItem(port, hasStatus && status, hasStatus ? (status ? "HIGH" : "LOW") : "No Response");
                })
                .ToList();
        }
    }

    public bool IsStateAwareOptionsEnabled => state.FeatureSet?.CanDoTagInventoryStateAwareSingulation == true
        && state.Settings?.InventoryStateAware == true;

    public void Refresh() => readers.RefreshSettings();
    public void ApplyChanges() => readers.ApplyCurrentSettings();
    public void ApplyDefaults() => readers.ApplyDefaultSettings();
    public void RefreshStatus() => readers.RefreshStatus();
    public void SetGpo(ushort port, bool value) => readers.SetGpo(port, value);

    public void SelectReader(string endpoint)
    {
        readers.SelectReader(endpoint);
        Refresh();
    }
}

public sealed record RfModeOption(uint Id, string DisplayText);
public sealed record GpiUiItem(ushort PortNumber, bool? CurrentState, string CurrentStateText);
public sealed record GpoUiItem(ushort PortNumber, bool DesiredState, string CurrentStateText);
