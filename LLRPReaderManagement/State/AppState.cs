using LLRPReaderManagement.Models;
using LLRPSdk;

namespace LLRPReaderManagement.State;

public sealed class AppState
{
    private sealed class ReaderRuntime
    {
        public required string Endpoint { get; init; }
        public string Name { get; set; } = "Reader";
        public string Status { get; set; } = "Connected";
        public bool IsConnected { get; set; }
        public bool IsInventoryRunning { get; set; }
        public int TotalReports { get; set; }
        public FeatureSet? FeatureSet { get; set; }
        public Settings? Settings { get; set; }
    }

    private readonly object gate = new();
    private readonly Dictionary<string, ReaderRuntime> readers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, InventoryTagItem> tags = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<LogEntry> logs = new();

    public event Action? Changed;

    public string Endpoint { get; set; } = "192.168.40.233";
    public string ActiveEndpoint { get; private set; } = "192.168.40.233";
    public string ConnectionStatus { get; private set; } = "Disconnected";
    public bool IsBusy { get; private set; }
    public bool IsConnected => Readers.Any(x => x.IsConnected);
    public bool IsInventoryRunning => Readers.Any(x => x.IsInventoryRunning);
    public int TotalReports => Readers.Sum(x => x.TotalReports);
    public string ReaderModel => ActiveRuntime?.FeatureSet?.ReaderModel ?? "-";
    public string Firmware => ActiveRuntime?.FeatureSet?.FirmwareVersion ?? "-";
    public uint AntennaCount => ActiveRuntime?.FeatureSet?.AntennaCount ?? 0;
    public ushort GpiCount => ActiveRuntime?.FeatureSet?.GpiCount ?? 0;
    public ushort GpoCount => ActiveRuntime?.FeatureSet?.GpoCount ?? 0;
    public FeatureSet? FeatureSet => ActiveRuntime?.FeatureSet;
    public Settings? Settings => ActiveRuntime?.Settings;

    public IReadOnlyList<ReaderSummary> Readers
    {
        get
        {
            lock (gate)
            {
                return readers.Values
                    .OrderByDescending(x => string.Equals(x.Endpoint, ActiveEndpoint, StringComparison.OrdinalIgnoreCase))
                    .ThenBy(x => x.Endpoint)
                    .Select(ToSummary)
                    .ToList();
            }
        }
    }

    public IReadOnlyList<InventoryTagItem> Tags
    {
        get
        {
            lock (gate)
            {
                return tags.Values.OrderByDescending(x => x.LastSeen).ToList();
            }
        }
    }

    public IReadOnlyList<LogEntry> Logs
    {
        get
        {
            lock (gate)
            {
                return logs.OrderByDescending(x => x.Timestamp).Take(500).ToList();
            }
        }
    }

    public ReaderSummary Reader => Readers.FirstOrDefault(x => string.Equals(x.Endpoint, ActiveEndpoint, StringComparison.OrdinalIgnoreCase))
        ?? new ReaderSummary("Primary Reader", Endpoint, "-", "-", 0, 0, 0, false, false, Tags.Count, TotalReports);

    private ReaderRuntime? ActiveRuntime
    {
        get
        {
            lock (gate)
            {
                if (readers.TryGetValue(ActiveEndpoint, out var reader))
                {
                    return reader;
                }

                return readers.Values.FirstOrDefault();
            }
        }
    }

    public void SetBusy(bool value, string? status = null)
    {
        IsBusy = value;
        if (!string.IsNullOrWhiteSpace(status))
        {
            ConnectionStatus = status;
        }
        Notify();
    }

    public void SetActiveReader(string endpoint)
    {
        ActiveEndpoint = endpoint;
        Endpoint = endpoint;
        Notify();
    }

    public void SetConnected(string endpoint, FeatureSet featureSet, Settings? settings)
    {
        lock (gate)
        {
            ActiveEndpoint = endpoint;
            Endpoint = endpoint;
            readers[endpoint] = new ReaderRuntime
            {
                Endpoint = endpoint,
                Name = $"Reader {readers.Count + 1}",
                Status = $"Connected to {endpoint}",
                IsConnected = true,
                FeatureSet = featureSet,
                Settings = settings
            };
            ConnectionStatus = $"Connected to {endpoint}";
        }

        Notify();
    }

    public void SetDisconnected(string status = "Disconnected")
    {
        lock (gate)
        {
            foreach (var reader in readers.Values)
            {
                reader.IsConnected = false;
                reader.IsInventoryRunning = false;
                reader.Status = status;
            }

            readers.Clear();
            ConnectionStatus = status;
        }

        IsBusy = false;
        Notify();
    }

    public void SetDisconnected(string endpoint, string status)
    {
        lock (gate)
        {
            readers.Remove(endpoint);
            if (string.Equals(ActiveEndpoint, endpoint, StringComparison.OrdinalIgnoreCase))
            {
                ActiveEndpoint = readers.Keys.FirstOrDefault() ?? Endpoint;
                Endpoint = ActiveEndpoint;
            }

            ConnectionStatus = readers.Count == 0 ? status : $"{readers.Count} reader(s) connected";
        }

        Notify();
    }

    public void SetInventoryRunning(bool value)
    {
        lock (gate)
        {
            foreach (var reader in readers.Values.Where(x => x.IsConnected))
            {
                reader.IsInventoryRunning = value;
            }
        }

        Notify();
    }

    public void SetInventoryRunning(string endpoint, bool value)
    {
        lock (gate)
        {
            if (readers.TryGetValue(endpoint, out var reader))
            {
                reader.IsInventoryRunning = value;
            }
        }

        Notify();
    }

    public void SetSettings(Settings settings)
    {
        lock (gate)
        {
            if (readers.TryGetValue(ActiveEndpoint, out var reader))
            {
                reader.Settings = settings;
            }
        }

        Notify();
    }

    public void AddTags(string endpoint, IEnumerable<Tag> reportedTags)
    {
        lock (gate)
        {
            foreach (var tag in reportedTags)
            {
                var epc = tag.Epc?.ToHexString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(epc))
                {
                    continue;
                }

                if (readers.TryGetValue(endpoint, out var reader))
                {
                    reader.TotalReports++;
                }

                tags[$"{endpoint}|{epc}"] = new InventoryTagItem(
                    epc,
                    endpoint,
                    tag.IsAntennaPortNumberPresent ? tag.AntennaPortNumber : (ushort)0,
                    tag.IsPeakRssiPresent ? tag.PeakRssi : 0,
                    tag.IsSeenCountPresent ? tag.TagSeenCount : (ushort)1,
                    tag.IsChannelInMhzPresent ? tag.ChannelInMhz : 0,
                    DateTime.Now);
            }
        }

        Notify();
    }

    public void AddTags(IEnumerable<Tag> reportedTags) => AddTags(ActiveEndpoint, reportedTags);

    public void ClearTags()
    {
        lock (gate)
        {
            tags.Clear();
            foreach (var reader in readers.Values)
            {
                reader.TotalReports = 0;
            }
        }

        Notify();
    }

    public void AddLog(LogEntry entry)
    {
        lock (gate)
        {
            logs.Add(entry);
            if (logs.Count > 1000)
            {
                logs.RemoveRange(0, logs.Count - 1000);
            }
        }

        Notify();
    }

    private static ReaderSummary ToSummary(ReaderRuntime runtime)
    {
        var featureSet = runtime.FeatureSet;
        return new ReaderSummary(
            runtime.Name,
            runtime.Endpoint,
            string.IsNullOrWhiteSpace(featureSet?.ReaderModel) ? featureSet?.ModelNumber.ToString() ?? "-" : featureSet.ReaderModel,
            string.IsNullOrWhiteSpace(featureSet?.FirmwareVersion) ? "-" : featureSet.FirmwareVersion,
            featureSet?.AntennaCount ?? 0,
            featureSet?.GpiCount ?? 0,
            featureSet?.GpoCount ?? 0,
            runtime.IsConnected,
            runtime.IsInventoryRunning,
            0,
            runtime.TotalReports);
    }

    private void Notify() => Changed?.Invoke();
}
