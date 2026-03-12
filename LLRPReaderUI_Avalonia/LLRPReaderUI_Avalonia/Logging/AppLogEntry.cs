using Microsoft.Extensions.Logging;
using System;

namespace LLRPReaderUI_Avalonia.Logging;

public sealed class AppLogEntry
{
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.Now;
    public AppLogCategory Category { get; init; }
    public LogLevel Level { get; init; } = LogLevel.Information;
    public string Message { get; init; } = string.Empty;
    public string? Exception { get; init; }
}

