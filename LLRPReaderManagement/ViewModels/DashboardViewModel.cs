using LLRPReaderManagement.Models;
using LLRPReaderManagement.State;

namespace LLRPReaderManagement.ViewModels;

public sealed class DashboardViewModel(AppState state)
{
    public IReadOnlyList<ReaderMetric> Metrics => new[]
    {
        new ReaderMetric("Active Readers", $"{state.Readers.Count(x => x.IsConnected)} of {state.Readers.Count}", state.ConnectionStatus, state.IsConnected ? "up" : ""),
        new ReaderMetric("Tags in Range", state.Tags.Count.ToString("N0"), $"{state.TotalReports:N0} total reports", "up"),
        new ReaderMetric("Read Rate", state.IsInventoryRunning ? "Live" : "Idle", state.IsInventoryRunning ? "inventory running" : "not scanning", state.IsInventoryRunning ? "up" : ""),
        new ReaderMetric("System Uptime", GetUptimeValue(), $"since {state.StartupTime:HH:mm:ss}", "up")
    };

    private string GetUptimeValue()
    {
        var diff = DateTime.Now - state.StartupTime;
        if (diff.TotalHours >= 1)
        {
            return $"{diff.TotalHours:0.0}h";
        }
        return $"{diff.TotalMinutes:0}m";
    }

    public ReaderSummary Reader => state.Reader;
    public IReadOnlyList<InventoryTagItem> RecentTags => state.Tags.Take(8).ToList();
    public IReadOnlyList<LogEntry> Activity => state.Logs.Take(8).ToList();
}
