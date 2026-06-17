using LLRPSdk;

namespace LLRPReaderManagement.Repositories;

public sealed class LlrpReaderRepository : ILlrpReaderRepository, IDisposable
{
    private readonly object gate = new();
    private readonly Dictionary<string, LlrpReader> readers = new(StringComparer.OrdinalIgnoreCase);

    public event Action<string, IReadOnlyList<Tag>>? TagsReported;
    public event Action<TagOpReport>? TagOpCompleted;
    public event Action<string>? ReaderStopped;
    public event Action<string>? KeepaliveTimeout;

    public string ActiveEndpoint { get; private set; } = string.Empty;

    public IReadOnlyList<string> ConnectedEndpoints
    {
        get
        {
            lock (gate)
            {
                return readers.Where(x => x.Value.IsConnected).Select(x => x.Key).ToList();
            }
        }
    }

    public bool IsConnected => TryGetActiveReader(out var reader) && reader.IsConnected;
    public string Address => TryGetActiveReader(out var reader) ? reader.Address : string.Empty;
    public FeatureSet ReaderCapabilities => ActiveReader.ReaderCapabilities;

    public void Connect(string address, int? port = null)
    {
        var endpoint = FormatEndpoint(address, port);
        LlrpReader reader;

        lock (gate)
        {
            if (readers.TryGetValue(endpoint, out var existing))
            {
                if (existing.IsConnected)
                {
                    ActiveEndpoint = endpoint;
                    return;
                }

                try { existing.ForceDisconnect(); } catch { }
                readers.Remove(endpoint);
            }

            reader = new LlrpReader();
            WireReader(endpoint, reader);
            readers[endpoint] = reader;
            ActiveEndpoint = endpoint;
        }

        try
        {
            if (port.HasValue)
            {
                reader.Connect(address, port.Value, false);
            }
            else
            {
                reader.Connect(address);
            }
        }
        catch
        {
            lock (gate)
            {
                readers.Remove(endpoint);
                if (string.Equals(ActiveEndpoint, endpoint, StringComparison.OrdinalIgnoreCase))
                {
                    ActiveEndpoint = readers.FirstOrDefault(x => x.Value.IsConnected).Key ?? string.Empty;
                }
            }

            try { reader.ForceDisconnect(); } catch { }
            throw;
        }
    }

    public Task ConnectAsync(string address, int? port = null, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => Connect(address, port), cancellationToken);
    }

    public void Disconnect() => Disconnect(ActiveEndpoint);

    public void Disconnect(string endpoint)
    {
        LlrpReader? reader = null;
        lock (gate)
        {
            if (!string.IsNullOrWhiteSpace(endpoint) && readers.TryGetValue(endpoint, out reader))
            {
                readers.Remove(endpoint);
            }

            if (string.Equals(ActiveEndpoint, endpoint, StringComparison.OrdinalIgnoreCase))
            {
                ActiveEndpoint = readers.Keys.FirstOrDefault() ?? string.Empty;
            }
        }

        if (reader is not null && reader.IsConnected)
        {
            reader.Disconnect();
        }
    }

    public void ForceDisconnect()
    {
        var reader = ActiveReader;
        if (reader.IsConnected)
        {
            reader.ForceDisconnect();
        }
    }

    public void Start() => ActiveReader.Start();

    public IReadOnlyList<string> StartAll()
    {
        var started = new List<string>();
        foreach (var pair in SnapshotReaderPairs())
        {
            if (!pair.Reader.IsConnected)
            {
                continue;
            }

            try
            {
                pair.Reader.Start();
                started.Add(pair.Endpoint);
            }
            catch
            {
                // Caller can infer partial success from the returned endpoint list.
            }
        }

        return started;
    }

    public void Stop() => ActiveReader.Stop();

    public IReadOnlyList<string> StopAll()
    {
        var stopped = new List<string>();
        foreach (var pair in SnapshotReaderPairs())
        {
            if (!pair.Reader.IsConnected)
            {
                continue;
            }

            try
            {
                pair.Reader.Stop();
                stopped.Add(pair.Endpoint);
            }
            catch
            {
            }
        }

        return stopped;
    }

    public void SetActive(string endpoint)
    {
        lock (gate)
        {
            if (readers.ContainsKey(endpoint))
            {
                ActiveEndpoint = endpoint;
            }
        }
    }

    public TagReport QueryTags() => ActiveReader.QueryTags(1);
    public Settings QuerySettings() => ActiveReader.QuerySettings();
    public Status QueryStatus() => ActiveReader.QueryStatus();
    public bool QuerySingulatingState() => ActiveReader.QuerySingulatingState();
    public void ApplyDefaultSettings() => ActiveReader.ApplyDefaultSettings();
    public void ApplySettings(Settings settings) => ActiveReader.ApplySettingsWithoutFactoryReset(settings);
    public void SetGpo(ushort port, bool state) => ActiveReader.SetGpo(port, state);
    public void AddOpSequence(TagOpSequence sequence) => ActiveReader.AddOpSequence(sequence);
    public void DeleteAllOpSequences() => ActiveReader.DeleteAllOpSequences();
    public string ExportAddRoSpecXml() => ActiveReader.BuildAddROSpecMessage(QuerySettings()).ToString();
    public string ExportSetReaderConfigXml() => ActiveReader.BuildSetReaderConfigMessage(QuerySettings()).ToString();
    public Settings ImportLlrpXml(string addRoSpecXml, string setReaderConfigXml)
    {
        var addRoSpec = Org.LLRP.LTK.LLRPV1.MSG_ADD_ROSPEC.FromString(addRoSpecXml);
        var setReaderConfig = Org.LLRP.LTK.LLRPV1.MSG_SET_READER_CONFIG.FromString(setReaderConfigXml);
        return ActiveReader.ParseSettingsFromLlrpMessages(addRoSpec, setReaderConfig);
    }

    public void Dispose()
    {
        foreach (var reader in SnapshotReaders())
        {
            if (reader.IsConnected)
            {
                try { reader.ForceDisconnect(); } catch { }
            }
        }
    }

    private LlrpReader ActiveReader
    {
        get
        {
            if (TryGetActiveReader(out var reader))
            {
                return reader;
            }

            throw new LLRPSdkException("No active reader is connected.");
        }
    }

    private bool TryGetActiveReader(out LlrpReader reader)
    {
        lock (gate)
        {
            if (!string.IsNullOrWhiteSpace(ActiveEndpoint) && readers.TryGetValue(ActiveEndpoint, out reader!))
            {
                return true;
            }

            var first = readers.FirstOrDefault(x => x.Value.IsConnected);
            if (!string.IsNullOrWhiteSpace(first.Key))
            {
                ActiveEndpoint = first.Key;
                reader = first.Value;
                return true;
            }
        }

        reader = null!;
        return false;
    }

    private IReadOnlyList<LlrpReader> SnapshotReaders()
    {
        lock (gate)
        {
            return readers.Values.ToList();
        }
    }

    private IReadOnlyList<(string Endpoint, LlrpReader Reader)> SnapshotReaderPairs()
    {
        lock (gate)
        {
            return readers.Select(x => (x.Key, x.Value)).ToList();
        }
    }

    private void WireReader(string endpoint, LlrpReader reader)
    {
        reader.TagsReported += (_, report) => TagsReported?.Invoke(endpoint, report.Tags);
        reader.TagOpComplete += (_, report) => TagOpCompleted?.Invoke(report);
        reader.ReaderStopped += (_, _) => ReaderStopped?.Invoke(endpoint);
        reader.KeepaliveTimeout += _ => KeepaliveTimeout?.Invoke(endpoint);
    }

    private static string FormatEndpoint(string address, int? port)
    {
        return port.HasValue ? $"{address}:{port.Value}" : address;
    }
}
