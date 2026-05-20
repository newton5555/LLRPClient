using LLRPReaderManagement.Models;
using LLRPReaderManagement.Services;
using LLRPReaderManagement.State;

namespace LLRPReaderManagement.ViewModels;

public sealed class ConfigViewModel(AppState state, ReaderManagementService readers)
{
    public AppState State => state;

    public IReadOnlyList<FeatureItem> Features => new[]
    {
        new FeatureItem("Reader Model", state.ReaderModel),
        new FeatureItem("Firmware", state.Firmware),
        new FeatureItem("Antennas", state.AntennaCount == 0 ? "-" : state.AntennaCount.ToString()),
        new FeatureItem("GPI / GPO", $"{state.GpiCount} / {state.GpoCount}"),
        new FeatureItem("RF Mode", state.Settings?.RfMode?.ToString() ?? "-"),
        new FeatureItem("Session", state.Settings?.Session.ToString() ?? "-"),
        new FeatureItem("Tag Population", state.Settings?.TagPopulationEstimate.ToString() ?? "-"),
        new FeatureItem("Hold Reports", state.Settings is null ? "-" : state.Settings.HoldReportsOnDisconnect ? "Enabled" : "Disabled")
    };

    public void Refresh() => readers.RefreshSettings();
    public void ApplyChanges() => readers.ApplyCurrentSettings();
    public void ApplyDefaults() => readers.ApplyDefaultSettings();

    public void SelectReader(string endpoint)
    {
        readers.SelectReader(endpoint);
        Refresh();
    }
}
