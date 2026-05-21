using LLRPReaderManagement.Services;
using LLRPReaderManagement.State;

namespace LLRPReaderManagement.ViewModels;

public sealed class ReadersViewModel(
    AppState state,
    ReaderManagementService readers,
    EndpointHistoryService endpointHistory)
{
    public string Endpoint
    {
        get => state.Endpoint;
        set => state.Endpoint = value;
    }

    public AppState State => state;
    public IReadOnlyList<string> RecentEndpoints => endpointHistory.RecentEndpoints;
    public Task ConnectAsync() => readers.ConnectAsync(Endpoint);
    public Task ConnectAsync(string endpoint) => readers.ConnectAsync(endpoint);
    public void Disconnect() => readers.Disconnect();
    public void Disconnect(string endpoint) => readers.Disconnect(endpoint);
    public void Select(string endpoint) => readers.SelectReader(endpoint);
}
