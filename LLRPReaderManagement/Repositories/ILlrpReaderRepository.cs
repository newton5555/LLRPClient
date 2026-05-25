using LLRPSdk;

namespace LLRPReaderManagement.Repositories;

public interface ILlrpReaderRepository
{
    event Action<string, IReadOnlyList<Tag>>? TagsReported;
    event Action<TagOpReport>? TagOpCompleted;
    event Action<string>? ReaderStopped;
    event Action<string>? KeepaliveTimeout;
    bool IsConnected { get; }
    string Address { get; }
    string ActiveEndpoint { get; }
    IReadOnlyList<string> ConnectedEndpoints { get; }
    FeatureSet ReaderCapabilities { get; }
    void Connect(string address, int? port = null);
    Task ConnectAsync(string address, int? port = null, CancellationToken cancellationToken = default);
    void Disconnect();
    void Disconnect(string endpoint);
    void ForceDisconnect();
    void Start();
    IReadOnlyList<string> StartAll();
    void Stop();
    IReadOnlyList<string> StopAll();
    void SetActive(string endpoint);
    TagReport QueryTags();
    Settings QuerySettings();
    Status QueryStatus();
    bool QuerySingulatingState();
    void ApplyDefaultSettings();
    void ApplySettings(Settings settings);
    void SetGpo(ushort port, bool state);
    void AddOpSequence(TagOpSequence sequence);
    void DeleteAllOpSequences();
    string ExportAddRoSpecXml();
    string ExportSetReaderConfigXml();
    Settings ImportLlrpXml(string addRoSpecXml, string setReaderConfigXml);
}
