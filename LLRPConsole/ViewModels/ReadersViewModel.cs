using LLRPConsole.Services;
using LLRPConsole.State;

namespace LLRPConsole.ViewModels;

public sealed class ReadersViewModel(
    AppState state,
    ReaderManagementService readers,
    EndpointHistoryService endpointHistory,
    LlrpSimulator simulator)
{
    public string Endpoint
    {
        get => state.Endpoint;
        set => state.Endpoint = value;
    }

    public AppState State => state;
    public LlrpSimulator Simulator => simulator;
    public IReadOnlyList<string> RecentEndpoints => endpointHistory.RecentEndpoints;
    public Task ConnectAsync() => readers.ConnectAsync(Endpoint);
    public Task ConnectAsync(string endpoint) => readers.ConnectAsync(endpoint);
    public void Disconnect() => readers.Disconnect();
    
    public void Disconnect(string endpoint)
    {
        readers.Disconnect(endpoint);
    }

    public void Select(string endpoint) => readers.SelectReader(endpoint);

    public async Task ToggleSimulatorAsync()
    {
        if (simulator.IsRunning)
        {
            readers.Disconnect("127.0.0.1:50840");
            simulator.Stop();
            state.ShowNotification("Simulator Stopped", "The local LLRP virtual reader was stopped.", true);
        }
        else
        {
            try
            {
                simulator.Start(50840);
                Endpoint = "127.0.0.1:50840";
                await readers.ConnectAsync(Endpoint);
                state.ShowNotification("Simulator Started", "The local LLRP virtual reader was started and connected successfully.", true);
            }
            catch (Exception ex)
            {
                simulator.Stop();
                state.ShowNotification("Simulator Failed", ex.Message, false);
            }
        }
    }
}

