using Microsoft.Extensions.Logging;

namespace LLRPReaderUI_WPF.Logging;

public interface IAppLogService
{
    event Action<AppLogEntry>? EntryAdded;

    IReadOnlyList<AppLogEntry> Snapshot();

    void ClearInMemory();

    void LogOperation(string message, LogLevel level = LogLevel.Information, Exception? exception = null);

    void LogLlrpMessage(string message, LogLevel level = LogLevel.Information, Exception? exception = null);

    /// <summary>
    /// 记录 LLRP 消息（带详细信息）
    /// </summary>
    void LogLlrpMessage(string messageType, ushort msgTypeId, uint msgId, string? details = null, LogLevel level = LogLevel.Information);

    void LogRawFrame(string direction, byte[] payload, LogLevel level = LogLevel.Debug);
}
