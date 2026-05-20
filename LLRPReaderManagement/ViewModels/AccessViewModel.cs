using LLRPReaderManagement.Services;

namespace LLRPReaderManagement.ViewModels;

public sealed class AccessViewModel(AccessOperationService access, ReaderManagementService readers)
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
        var result = await access.ReadAsync(TargetEpc, MemoryBank, WordPointer, WordCount);
        Result = result.Message;
        Data = result.Data;
    }

    public async Task WriteAsync()
    {
        var result = await access.WriteAsync(TargetEpc, MemoryBank, WordPointer, WriteData, BlockWriteEnabled);
        Result = result.Message;
        Data = result.Data;
    }

    public void Clear()
    {
        Result = "Waiting for operation.";
        Data = null;
    }

    public void SelectReader(string endpoint) => readers.SelectReader(endpoint);
}
