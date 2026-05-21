using LLRPReaderManagement.Services;
using LLRPReaderManagement.State;

namespace LLRPReaderManagement.ViewModels;

public sealed class AccessViewModel(AccessOperationService access, ReaderManagementService readers, AppState state)
{
    public string TargetEpc { get; set; } = string.Empty;
    public string MemoryBank { get; set; } = "User";
    public ushort WordPointer { get; set; }
    public ushort WordCount { get; set; } = 8;
    public string WriteData { get; set; } = string.Empty;
    public bool BlockWriteEnabled { get; set; }
    public string Result { get; private set; } = "Waiting for operation.";
    public string? Data { get; private set; }

    public async Task ReadAsync()
    {
        if (state.IsInventoryRunning)
        {
            Result = "Stop inventory before running access operations.";
            Data = null;
            state.ShowNotification("Read failed", Result, false);
            return;
        }

        try
        {
            var result = await access.ReadAsync(TargetEpc, MemoryBank, WordPointer, WordCount);
            Result = result.Message;
            Data = result.Data;
            state.ShowNotification(
                result.Success ? "Read completed" : "Read failed",
                string.IsNullOrWhiteSpace(result.Data) ? result.Message : $"{result.Message}{Environment.NewLine}{result.Data}",
                result.Success);
        }
        catch (Exception ex)
        {
            Result = $"Read failed: {ex.Message}";
            Data = null;
            state.ShowNotification("Read failed", ex.Message, false);
        }
    }

    public async Task WriteAsync()
    {
        if (state.IsInventoryRunning)
        {
            Result = "Stop inventory before running access operations.";
            Data = null;
            state.ShowNotification("Write failed", Result, false);
            return;
        }

        try
        {
            var result = await access.WriteAsync(TargetEpc, MemoryBank, WordPointer, WriteData, BlockWriteEnabled);
            Result = result.Message;
            Data = result.Data;
            state.ShowNotification(result.Success ? "Write completed" : "Write failed", result.Message, result.Success);
        }
        catch (Exception ex)
        {
            Result = $"Write failed: {ex.Message}";
            Data = null;
            state.ShowNotification("Write failed", ex.Message, false);
        }
    }

    public void Clear()
    {
        Result = "Waiting for operation.";
        Data = null;
    }

    public void SelectReader(string endpoint) => readers.SelectReader(endpoint);
}
