using LLRPReaderManagement.Repositories;
using LLRPReaderManagement.State;
using Microsoft.Extensions.Logging;

namespace LLRPReaderManagement.Services;

public sealed class ReaderManagementService
{
    private readonly ILlrpReaderRepository repository;
    private readonly AppState state;
    private readonly IAppLogService logs;
    private readonly EndpointHistoryService endpointHistory;

    public ReaderManagementService(
        ILlrpReaderRepository repository,
        AppState state,
        IAppLogService logs,
        EndpointHistoryService endpointHistory)
    {
        this.repository = repository;
        this.state = state;
        this.logs = logs;
        this.endpointHistory = endpointHistory;

        repository.TagsReported += (endpoint, tags) => state.AddTags(endpoint, tags);
        repository.ReaderStopped += endpoint => state.SetInventoryRunning(endpoint, false);
        repository.KeepaliveTimeout += endpoint =>
        {
            logs.Log("Reader", $"Keepalive timeout from {endpoint}, forcing disconnect.", LogLevel.Warning);
            try { repository.Disconnect(endpoint); } catch { }
            state.SetDisconnected(endpoint, $"Disconnected {endpoint} by keepalive timeout");
        };
    }

    public async Task ConnectAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        var (address, port) = ParseEndpoint(endpoint);
        var targetEndpoint = port.HasValue ? $"{address}:{port.Value}" : address;
        var previousActiveEndpoint = state.ActiveEndpoint;
        state.SetBusy(true, $"Connecting to {endpoint}");
        logs.Log("Reader", $"Connecting to {endpoint}");

        try
        {
            await repository.ConnectAsync(address, port, cancellationToken);

            var wasSingulating = EnsureStoppedIfSingulating();
            var settings = QueryInitialSettings();

            endpointHistory.Remember(targetEndpoint);
            state.SetConnected(targetEndpoint, repository.ReaderCapabilities, settings);
            logs.Log("Reader", wasSingulating
                ? $"Connected to {targetEndpoint}; active inventory was stopped during initialization."
                : $"Connected to {targetEndpoint}");
        }
        catch (Exception ex)
        {
            try
            {
                repository.Disconnect(targetEndpoint);
            }
            catch
            {
            }

            if (!string.IsNullOrWhiteSpace(previousActiveEndpoint))
            {
                repository.SetActive(previousActiveEndpoint);
                state.SetActiveReader(previousActiveEndpoint);
            }

            state.SetBusy(false, $"Connection failed: {ex.Message}");
            logs.Log("Reader", $"Connection failed: {ex.Message}", LogLevel.Error, ex);
        }
        finally
        {
            state.SetBusy(false);
        }
    }

    public void Disconnect()
    {
        Disconnect(state.ActiveEndpoint);
    }

    public void Disconnect(string endpoint)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(endpoint))
            {
                repository.Disconnect(endpoint);
            }

            state.SetDisconnected(endpoint, $"Disconnected {endpoint}");
            logs.Log("Reader", $"Disconnected {endpoint}");
        }
        catch (Exception ex)
        {
            state.SetDisconnected(endpoint, $"Disconnect failed: {ex.Message}");
            logs.Log("Reader", $"Disconnect failed for {endpoint}: {ex.Message}", LogLevel.Warning, ex);
        }
    }

    public void SelectReader(string endpoint)
    {
        repository.SetActive(endpoint);
        state.SetActiveReader(endpoint);
    }

    public void RefreshSettings()
    {
        if (!repository.IsConnected)
        {
            logs.Log("Config", "Connect a reader before refreshing settings.", LogLevel.Warning);
            return;
        }

        var settings = repository.QuerySettings();
        state.SetSettings(settings);
        TryRefreshStatus();
        logs.Log("Config", "Reader settings refreshed.");
    }

    public void ApplyDefaultSettings()
    {
        if (!repository.IsConnected)
        {
            logs.Log("Config", "Connect a reader before applying defaults.", LogLevel.Warning);
            return;
        }

        repository.ApplyDefaultSettings();
        var settings = repository.QuerySettings();
       
        state.SetSettings(settings);
        TryRefreshStatus();
        logs.Log("Config", $"SDK default settings applied to {repository.ActiveEndpoint}.");
    }

    public void ApplyCurrentSettings()
    {
        if (!repository.IsConnected)
        {
            logs.Log("Config", "Connect a reader before applying settings.", LogLevel.Warning);
            return;
        }

        if (state.Settings is null)
        {
            logs.Log("Config", "No settings snapshot is available to apply.", LogLevel.Warning);
            return;
        }

        var wasRunning = false;
        try
        {
            wasRunning = repository.QuerySingulatingState();
            if (wasRunning)
            {
                repository.Stop();
                state.SetInventoryRunning(repository.ActiveEndpoint, false);
            }
        }
        catch (Exception ex)
        {
            logs.Log("Config", $"Could not stop reader before applying settings: {ex.Message}", LogLevel.Warning, ex);
        }

        repository.ApplySettings(state.Settings);
        var settings = repository.QuerySettings();
        state.SetSettings(settings);
        TryRefreshStatus();
        logs.Log("Config", wasRunning
            ? $"Settings applied to {repository.ActiveEndpoint}; inventory was stopped."
            : $"Settings applied to {repository.ActiveEndpoint}.");
    }

    public void RefreshStatus()
    {
        if (!repository.IsConnected)
        {
            logs.Log("Config", "Connect a reader before querying status.", LogLevel.Warning);
            return;
        }

        var status = repository.QueryStatus();
        state.SetStatus(status);
        logs.Log("GPIO", $"Status refreshed from {repository.ActiveEndpoint}.");
    }

    public void SetGpo(ushort port, bool value)
    {
        if (!repository.IsConnected)
        {
            logs.Log("GPIO", "Connect a reader before setting GPO.", LogLevel.Warning);
            return;
        }

        repository.SetGpo(port, value);
        TryRefreshStatus();
        logs.Log("GPIO", $"GPO {port} set to {(value ? "HIGH" : "LOW")} on {repository.ActiveEndpoint}.");
    }

    private static (string Address, int? Port) ParseEndpoint(string endpoint)
    {
        var value = endpoint.Trim();
        var parts = value.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 2 && int.TryParse(parts[1], out var port))
        {
            return (parts[0], port);
        }

        return (value, null);
    }

    private bool EnsureStoppedIfSingulating()
    {
        try
        {
            var isSingulating = repository.QuerySingulatingState();
            if (isSingulating)
            {
                repository.Stop();
            }

            return isSingulating;
        }
        catch (LLRPSdk.LLRPSdkException ex) when (IsMissingOrInvalidConfiguration(ex))
        {
            logs.Log("Reader", "Reader has no SDK ROSPEC/configuration yet; applying default settings before status query.", LogLevel.Warning);
            repository.ApplyDefaultSettings();
            return false;
        }
    }

    private LLRPSdk.Settings QueryInitialSettings()
    {
        try
        {
            return repository.QuerySettings();
        }
        catch (LLRPSdk.LLRPSdkException ex) when (IsMissingOrInvalidConfiguration(ex))
        {
            logs.Log("Reader", "Reader configuration missing or invalid; applying SDK default settings.", LogLevel.Warning);
            repository.ApplyDefaultSettings();
            return repository.QuerySettings();
        }
    }

    // Workaround for LLRPSdkException not providing strongly-typed error codes.
    private const string ErrMsgNotConfigured = "has not been configured";
    private const string ErrMsgInvalidConfig = "configuration is invalid";

    private static bool IsMissingOrInvalidConfiguration(LLRPSdk.LLRPSdkException ex)
    {
        return ex.Message.Contains(ErrMsgNotConfigured, StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains(ErrMsgInvalidConfig, StringComparison.OrdinalIgnoreCase);
    }

    private void TryRefreshStatus()
    {
        try
        {
            if (repository.IsConnected)
            {
                state.SetStatus(repository.QueryStatus());
            }
        }
        catch (Exception ex)
        {
            logs.Log("GPIO", $"Status refresh failed: {ex.Message}", LogLevel.Warning, ex);
        }
    }
}
