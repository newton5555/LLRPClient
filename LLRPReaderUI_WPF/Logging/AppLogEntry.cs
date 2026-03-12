using Microsoft.Extensions.Logging;

namespace LLRPReaderUI_WPF.Logging;

public sealed class AppLogEntry
{
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.Now;
    public AppLogCategory Category { get; init; }
    public LogLevel Level { get; init; } = LogLevel.Information;
    public string Message { get; init; } = string.Empty;
    public string? Exception { get; init; }
}
