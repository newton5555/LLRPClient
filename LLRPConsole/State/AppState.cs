using LLRPConsole.Models;
using LLRPSdk;

namespace LLRPConsole.State;

public sealed class AppState
{
    private const int ReadRateWindowSeconds = 5;
    private const int MaxRawTagRows = 2000;
    private const uint InventoryRoSpecId = 14150;

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

    private readonly record struct TagKey(string Endpoint, string Epc);

    private readonly object gate = new();
    private readonly Dictionary<string, ReaderRuntime> readers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<TagKey, InventoryTagItem> tags = new();
    private readonly List<InventoryTagItem> rawTags = new();
    private readonly Dictionary<TagKey, uint> tagAntennas = new();
    private readonly Dictionary<TagKey, ushort> tagSeenCounts = new();
    private readonly List<LogEntry> logs = new();
    
    private int notifyPending = 0;
    private long lastNotifyTicks = 0;
    private readonly SortedDictionary<DateTime, int> readRateByMinute = new();
    private readonly Queue<ReadRatePoint> readEvents = new();

    public event Action? Changed;

    private string searchQuery = string.Empty;
    public string SearchQuery
    {
        get => searchQuery;
        set
        {
            if (searchQuery != value)
            {
                searchQuery = value;
                Notify();
            }
        }
    }

    private string currentCrumb = "LLRP / Reader 连接";
    public string CurrentCrumb
    {
        get => currentCrumb;
        set
        {
            if (currentCrumb != value)
            {
                currentCrumb = value;
                Notify();
            }
        }
    }

    public DateTime StartupTime { get; } = DateTime.Now;
    public string Endpoint { get; set; } = "192.168.40.233";
    public string ActiveEndpoint { get; private set; } = "192.168.40.233";
    public string ConnectionStatus { get; private set; } = "Disconnected";
    public bool IsBusy { get; private set; }
    public bool IsConnected => Readers.Any(x => x.IsConnected);
    public bool IsInventoryRunning => Readers.Any(x => x.IsInventoryRunning);
    public int TotalReports => Readers.Sum(x => x.TotalReports);
    public double RollingReadRatePerSecond
    {
        get
        {
            lock (gate)
            {
                TrimReadEvents(DateTime.Now);
                return readEvents.Sum(x => x.Count) / (double)ReadRateWindowSeconds;
            }
        }
    }
    public string ReaderModel => ActiveRuntime?.FeatureSet?.ReaderModel ?? "-";
    public string Firmware => ActiveRuntime?.FeatureSet?.FirmwareVersion ?? "-";
    public uint AntennaCount => ActiveRuntime?.FeatureSet?.AntennaCount ?? 0;
    public ushort GpiCount => ActiveRuntime?.FeatureSet?.GpiCount ?? 0;
    public ushort GpoCount => ActiveRuntime?.FeatureSet?.GpoCount ?? 0;
    public FeatureSet? FeatureSet => ActiveRuntime?.FeatureSet;
    public Settings? Settings => ActiveRuntime?.Settings;
    public Status? Status { get; private set; }
    public AppNotification? Notification { get; private set; }
    public uint? CurrentRoSpecId => ActiveRuntime?.IsConnected == true ? InventoryRoSpecId : null;

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
                return tags.Values.ToList();
            }
        }
    }

    public IReadOnlyList<InventoryTagItem> RawTags
    {
        get
        {
            lock (gate)
            {
                return rawTags.AsEnumerable().Reverse().ToList();
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

    public IReadOnlyList<ReadRatePoint> ReadRateLast60Minutes
    {
        get
        {
            lock (gate)
            {
                var nowMinute = TruncateToMinute(DateTime.Now);
                var start = nowMinute.AddMinutes(-59);
                return Enumerable.Range(0, 60)
                    .Select(i =>
                    {
                        var minute = start.AddMinutes(i);
                        return new ReadRatePoint(minute, readRateByMinute.TryGetValue(minute, out var count) ? count : 0);
                    })
                    .ToList();
            }
        }
    }

    public ReaderSummary Reader => Readers.FirstOrDefault(x => string.Equals(x.Endpoint, ActiveEndpoint, StringComparison.OrdinalIgnoreCase))
        ?? new ReaderSummary("Primary Reader", Endpoint, "-", "-", 0, 0, 0, false, false, null, Tags.Count, TotalReports);

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

    public void SetStatus(Status status)
    {
        Status = status;
        Notify();
    }

    public void AddTags(string endpoint, IEnumerable<Tag> reportedTags)
    {
        var reportCount = 0;
        var now = DateTime.Now;
        lock (gate)
        {
            foreach (var tag in reportedTags)
            {
                var epc = tag.Epc?.ToHexString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(epc))
                {
                    continue;
                }

                var tagKey = new TagKey(endpoint, epc);
                var seenCount = tag.IsSeenCountPresent ? tag.TagSeenCount : (ushort)1;
                var delta = 1;
                if (tag.IsSeenCountPresent)
                {
                    delta = tagSeenCounts.TryGetValue(tagKey, out var previous) && seenCount >= previous
                        ? Math.Max(0, seenCount - previous)
                        : Math.Max(1, (int)seenCount);
                    tagSeenCounts[tagKey] = seenCount;
                }

                var cumulativeSeenCount = tags.TryGetValue(tagKey, out var existing)
                    ? existing.SeenCount + delta
                    : Math.Max(1, delta);

                if (existing == null && tags.Count >= 100000)
                {
                    continue; // Reached hard limit, stop adding unique EPCs
                }

                var antenna = tag.IsAntennaPortNumberPresent ? tag.AntennaPortNumber : (ushort)0;
                var rssi = tag.IsPeakRssiPresent ? tag.PeakRssi : 0;
                var channel = tag.IsChannelInMhzPresent ? tag.ChannelInMhz : 0;
                var attachedData = GetAttachedData(tag);
                var reportSource = tag.ReportSource.ToString();

                reportCount += delta;
                if (readers.TryGetValue(endpoint, out var reader))
                {
                    reader.TotalReports += delta;
                }

                if (!tagAntennas.TryGetValue(tagKey, out var antennaMask))
                {
                    antennaMask = 0;
                }

                if (antenna > 0 && antenna <= 32)
                {
                    antennaMask |= (1u << (antenna - 1));
                    tagAntennas[tagKey] = antennaMask;
                }

                rawTags.Add(new InventoryTagItem(
                    epc,
                    endpoint,
                    antenna,
                    FormatAntennaText(antennaMask),
                    rssi,
                    seenCount,
                    channel,
                    now,
                    now,
                    attachedData,
                    reportSource));
                if (rawTags.Count > MaxRawTagRows)
                {
                    rawTags.RemoveRange(0, rawTags.Count - MaxRawTagRows);
                }

                var firstSeen = existing?.FirstSeen ?? now;

                tags[tagKey] = new InventoryTagItem(
                    epc,
                    endpoint,
                    antenna,
                    FormatAntennaText(antennaMask),
                    rssi,
                    cumulativeSeenCount,
                    channel,
                    firstSeen,
                    now,
                    attachedData,
                    reportSource);
            }

            if (reportCount > 0)
            {
                var minute = TruncateToMinute(now);
                readRateByMinute[minute] = readRateByMinute.TryGetValue(minute, out var existing)
                    ? existing + reportCount
                    : reportCount;
                readEvents.Enqueue(new ReadRatePoint(now, reportCount));
                TrimReadEvents(now);

                var cutoff = minute.AddMinutes(-120);
                foreach (var key in readRateByMinute.Keys.Where(x => x < cutoff).ToList())
                {
                    readRateByMinute.Remove(key);
                }
            }
        }

        ThrottleNotify();
    }

    public void AddTags(IEnumerable<Tag> reportedTags) => AddTags(ActiveEndpoint, reportedTags);

    public void ClearTags()
    {
        lock (gate)
        {
            tags.Clear();
            rawTags.Clear();
            tagAntennas.Clear();
            tagSeenCounts.Clear();
            readRateByMinute.Clear();
            readEvents.Clear();
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

        ThrottleNotify();
    }

    public void ClearLogs()
    {
        lock (gate)
        {
            logs.Clear();
        }

        ThrottleNotify();
    }

    public void ShowNotification(string title, string message, bool isSuccess)
    {
        Notification = new AppNotification(title, message, isSuccess);
        Notify();
    }

    public void ClearNotification()
    {
        Notification = null;
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
            runtime.IsConnected ? InventoryRoSpecId : null,
            0,
            runtime.TotalReports);
    }

    private void Notify() => Changed?.Invoke();

    private void ThrottleNotify()
    {
        var now = Environment.TickCount64;
        var last = Interlocked.Read(ref lastNotifyTicks);
        if (now - last > 250)
        {
            Interlocked.Exchange(ref lastNotifyTicks, now);
            Notify();
        }
        else
        {
            if (Interlocked.CompareExchange(ref notifyPending, 1, 0) == 0)
            {
                _ = Task.Delay(250).ContinueWith(_ =>
                {
                    Interlocked.Exchange(ref notifyPending, 0);
                    Interlocked.Exchange(ref lastNotifyTicks, Environment.TickCount64);
                    Notify();
                });
            }
        }
    }

    private void TrimReadEvents(DateTime now)
    {
        var cutoff = now.AddSeconds(-ReadRateWindowSeconds);
        while (readEvents.Count > 0 && readEvents.Peek().Minute < cutoff)
        {
            readEvents.Dequeue();
        }
    }

    private static DateTime TruncateToMinute(DateTime value) =>
        new(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0, value.Kind);

    private static string FormatAntennaText(ushort antenna) => antenna > 0 ? $"Ant {antenna}" : "-";

    private static string FormatAntennaText(uint antennaMask)
    {
        if (antennaMask == 0) return "-";
        var list = new List<int>();
        for (int i = 0; i < 32; i++)
        {
            if ((antennaMask & (1u << i)) != 0)
            {
                list.Add(i + 1);
            }
        }
        return $"Ant {string.Join(", ", list)}";
    }

    private static string GetAttachedData(Tag tag)
    {
        var read = tag.ReadOperationResults?
            .LastOrDefault(x => x.Result == ReadResultStatus.Success && x.Data is not null);

        return read?.Data?.ToHexWordString() ?? string.Empty;
    }
}

