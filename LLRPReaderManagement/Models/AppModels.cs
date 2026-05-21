using Microsoft.Extensions.Logging;

namespace LLRPReaderManagement.Models;

public sealed record ReaderSummary(
    string Name,
    string Endpoint,
    string Model,
    string Firmware,
    uint AntennaCount,
    ushort GpiCount,
    ushort GpoCount,
    bool IsConnected,
    bool IsInventoryRunning,
    uint? CurrentRoSpecId,
    int UniqueTags,
    int TotalReports);

public sealed record InventoryTagItem(
    string Epc,
    string ReaderEndpoint,
    ushort Antenna,
    string AntennaText,
    double Rssi,
    int SeenCount,
    double Channel,
    DateTime FirstSeen,
    DateTime LastSeen,
    string AttachedData,
    string ReportSource);

public sealed record LogEntry(DateTime Timestamp, LogLevel Level, string Category, string Message);

public sealed record ReaderMetric(string Label, string Value, string Hint, string Tone = "");

public sealed record FeatureItem(string Label, string Value);

public sealed record AccessOperationResult(bool Success, string Message, string? Data = null);

public sealed record ReadRatePoint(DateTime Minute, int Count);
