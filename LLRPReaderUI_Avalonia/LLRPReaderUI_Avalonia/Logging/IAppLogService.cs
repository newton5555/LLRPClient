using Microsoft.Extensions.Logging;

namespace LLRPReaderUI_Avalonia.Logging;

public interface IAppLogService
{
    event Action<AppLogEntry>? EntryAdded;

    IReadOnlyList<AppLogEntry> Snapshot();

    void ClearInMemory();

    void LogOperation(string message, LogLevel level = LogLevel.Information, Exception? exception = null);

    void LogLlrpMessage(string message, LogLevel level = LogLevel.Information, Exception? exception = null);

    void LogRawFrame(string direction, byte[] payload, LogLevel level = LogLevel.Debug);
}
