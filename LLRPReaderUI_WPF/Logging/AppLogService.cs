using Microsoft.Extensions.Logging;

namespace LLRPReaderUI_WPF.Logging;

public sealed class AppLogService : IAppLogService
{
    private const int MaxEntries = 5000;
    private readonly object gate = new();
    private readonly List<AppLogEntry> entries = new();
    private readonly ILogger<AppLogService> logger;

    public AppLogService(ILogger<AppLogService> logger)
    {
        this.logger = logger;
    }

    public event Action<AppLogEntry>? EntryAdded;

    public IReadOnlyList<AppLogEntry> Snapshot()
    {
        lock (gate)
        {
            return entries.ToList();
        }
    }

    public void ClearInMemory()
    {
        lock (gate)
        {
            entries.Clear();
        }
    }

    public void LogOperation(string message, LogLevel level = LogLevel.Information, Exception? exception = null)
    {
        AddEntry(new AppLogEntry
        {
            Category = AppLogCategory.Operation,
            Level = level,
            Message = message,
            Exception = exception?.ToString()
        });

        logger.Log(level, exception, "[Operation] {Message}", message);
    }

    public void LogLlrpMessage(string message, LogLevel level = LogLevel.Information, Exception? exception = null)
    {
        // 格式化输出，显示完整的 LLRP 消息内容
        AddEntry(new AppLogEntry
        {
            Category = AppLogCategory.LlrpMessage,
            Level = level,
            Message = message,
            Exception = exception?.ToString()
        });

        logger.Log(level, exception, "[LLRP-MSG] {Message}", message);
    }

    public void LogLlrpMessage(string messageType, ushort msgTypeId, uint msgId, string? details = null, LogLevel level = LogLevel.Information)
    {
        // 格式：RX RO_ACCESS_REPORT (TypeId=1 MsgId=123456 len=X) details
        var msg = $"{messageType} (TypeId={msgTypeId} MsgId={msgId}){(string.IsNullOrEmpty(details) ? "" : $" {details}")}";
        
        AddEntry(new AppLogEntry
        {
            Category = AppLogCategory.LlrpMessage,
            Level = level,
            Message = msg
        });

        logger.Log(level, "[LLRP] {MessageType} TypeId={MessageTypeId} MsgId={MessageId} Details={Details}", messageType, msgTypeId, msgId, details);
    }

    public void LogRawFrame(string direction, byte[] payload, LogLevel level = LogLevel.Debug)
    {
        AddEntry(new AppLogEntry
        {
            Category = AppLogCategory.RawFrame,
            Level = level,
            Message = $"{direction} len={payload?.Length ?? 0} \r\n {BitConverter.ToString(payload)}"
        });

        logger.Log(level, "[Raw] {Direction} len={Length}", direction, payload?.Length ?? 0);
    }

    private void AddEntry(AppLogEntry entry)
    {

        EntryAdded?.Invoke(entry);
    }
}
