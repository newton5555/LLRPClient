using LLRPReaderManagement.Models;
using LLRPReaderManagement.State;

namespace LLRPReaderManagement.ViewModels;

public sealed class DashboardViewModel(AppState state)
{
    public IReadOnlyList<ReaderMetric> Metrics => new[]
    {
        new ReaderMetric("Active Readers", state.Readers.Count(x => x.IsConnected).ToString(), state.ConnectionStatus, state.IsConnected ? "up" : ""),
        new ReaderMetric("Tags in Range", state.Tags.Count.ToString(), $"{state.TotalReports} total reports", "up"),
        new ReaderMetric("Read Rate", state.IsInventoryRunning ? "Live" : "Idle", state.IsInventoryRunning ? "inventory running" : "not scanning"),
        new ReaderMetric("Antennas", state.AntennaCount == 0 ? "-" : state.AntennaCount.ToString(), state.Reader.Model)
    };

    public ReaderSummary Reader => state.Reader;
    public IReadOnlyList<InventoryTagItem> RecentTags => state.Tags.Take(8).ToList();
    public IReadOnlyList<LogEntry> Activity => state.Logs.Take(8).ToList();
}
