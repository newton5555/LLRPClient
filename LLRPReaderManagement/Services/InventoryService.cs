using LLRPReaderManagement.Repositories;
using LLRPReaderManagement.State;
using Microsoft.Extensions.Logging;

namespace LLRPReaderManagement.Services;

public sealed class InventoryService(ILlrpReaderRepository repository, AppState state, IAppLogService logs)
{
    public void StartActive()
    {
        if (!repository.IsConnected)
        {
            logs.Log("Inventory", "Connect a reader before starting inventory.", LogLevel.Warning);
            return;
        }

        repository.Start();
        state.SetInventoryRunning(repository.ActiveEndpoint, true);
        logs.Log("Inventory", $"Inventory started on {repository.ActiveEndpoint}.");
    }

    public void Start()
    {
        if (!repository.IsConnected)
        {
            logs.Log("Inventory", "Connect a reader before starting inventory.", LogLevel.Warning);
            return;
        }

        state.ClearTags();
        var started = repository.StartAll();
        foreach (var endpoint in started)
        {
            state.SetInventoryRunning(endpoint, true);
        }
        logs.Log("Inventory", $"Inventory started on {started.Count} of {repository.ConnectedEndpoints.Count} reader(s).");
    }

    public void StopActive()
    {
        if (!repository.IsConnected)
        {
            return;
        }

        repository.Stop();
        state.SetInventoryRunning(repository.ActiveEndpoint, false);
        logs.Log("Inventory", $"Inventory stopped on {repository.ActiveEndpoint}.");
    }

    public void Stop()
    {
        if (!repository.IsConnected)
        {
            return;
        }

        var stopped = repository.StopAll();
        foreach (var endpoint in stopped)
        {
            state.SetInventoryRunning(endpoint, false);
        }
        logs.Log("Inventory", $"Inventory stopped on {stopped.Count} reader(s).");
    }

    public void PullBufferedReports()
    {
        if (!repository.IsConnected)
        {
            logs.Log("Inventory", "Connect a reader before pulling reports.", LogLevel.Warning);
            return;
        }

        var previous = repository.ActiveEndpoint;
        var total = 0;
        foreach (var endpoint in repository.ConnectedEndpoints)
        {
            try
            {
                repository.SetActive(endpoint);
                var report = repository.QueryTags();
                state.AddTags(endpoint, report.Tags);
                total += report.Tags.Count;
            }
            catch (Exception ex)
            {
                logs.Log("Inventory", $"Pull failed from {endpoint}: {ex.Message}", LogLevel.Warning, ex);
            }
        }

        if (!string.IsNullOrWhiteSpace(previous))
        {
            repository.SetActive(previous);
            state.SetActiveReader(previous);
        }

        logs.Log("Inventory", $"Pulled {total} buffered tags from {repository.ConnectedEndpoints.Count} reader(s).");
    }

    public void Clear() => state.ClearTags();
}
