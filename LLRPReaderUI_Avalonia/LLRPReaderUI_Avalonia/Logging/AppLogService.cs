using LLRPSdk;
using LLRPReaderUI_Avalonia.Data;
using LLRPReaderUI_Avalonia.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading;

namespace LLRPReaderUI_Avalonia.Logging;

public sealed class AppLogService : IAppLogService
{
    private const int MaxEntries = 5000;
    private readonly object gate = new();
    private readonly List<AppLogEntry> entries = new();
    private readonly ILogger<AppLogService> logger;
    private readonly IServiceScopeFactory? scopeFactory;
    private readonly bool rawFrameLoggingEnabled;
    private readonly LlrpReader? reader;
    private readonly ReaderStatusStore? statusStore;
    private readonly ConcurrentQueue<(string? DeviceId, string Direction, byte[] Payload)> rawFrameQueue = new();
    private readonly CancellationTokenSource rawFrameCts = new();
    private readonly Task? rawFrameWorker;

    public AppLogService(
        ILogger<AppLogService> logger,
        IServiceScopeFactory? scopeFactory = null,
        LlrpReader? reader = null,
        ReaderStatusStore? statusStore = null)
    {
        this.logger = logger;
        this.scopeFactory = scopeFactory;
        this.reader = reader;
        this.statusStore = statusStore;
        rawFrameLoggingEnabled = IsRawFrameRepositoryRegistered();

        if (rawFrameLoggingEnabled)
        {
            rawFrameWorker = Task.Run(async () =>
            {
                while (!rawFrameCts.IsCancellationRequested)
                {
                    try
                    {
                        var batch = new List<(string? DeviceId, string Direction, byte[] Payload)>();
                        while (batch.Count < 500 && rawFrameQueue.TryDequeue(out var item))
                        {
                            batch.Add(item);
                        }

                        if (batch.Count > 0)
                        {
                            try
                            {
                                await PersistRawBatchAsync(batch).ConfigureAwait(false);
                            }
                            catch
                            {
                                /* swallow */
                            }
                        }
                        else
                        {
                            await Task.Delay(200, rawFrameCts.Token).ConfigureAwait(false);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch
                    {
                        await Task.Delay(500).ConfigureAwait(false);
                    }
                }
            }, rawFrameCts.Token);
        }
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
        var safePayload = payload ?? Array.Empty<byte>();

        AddEntry(new AppLogEntry
        {
            Category = AppLogCategory.RawFrame,
            Level = level,
            Message = $"{direction} len={safePayload.Length} \r\n {BitConverter.ToString(safePayload)}"
        });

        logger.Log(level, "[Raw] {Direction} len={Length}", direction, safePayload.Length);

        if (rawFrameLoggingEnabled)
        {
            var copy = safePayload.Length > 0 ? (byte[])safePayload.Clone() : Array.Empty<byte>();
            var deviceId = GetDeviceId();
            rawFrameQueue.Enqueue((deviceId, direction, copy));
        }
    }

    private string? GetDeviceId()
    {
        if (reader != null && !string.IsNullOrEmpty(reader.Address))
        {
            return reader.Address;
        }

        return null;
    }

    private void AddEntry(AppLogEntry entry)
    {
        lock (gate)
        {
            entries.Add(entry);
            if (entries.Count > MaxEntries)
            {
                entries.RemoveRange(0, entries.Count - MaxEntries);
            }
        }

        EntryAdded?.Invoke(entry);
    }

    private bool IsRawFrameRepositoryRegistered()
    {
        if (scopeFactory == null)
        {
            return false;
        }

        try
        {
            using var scope = scopeFactory.CreateScope();
            return scope.ServiceProvider.GetService<IRawFrameRepository>() != null;
        }
        catch
        {
            return false;
        }
    }

    private async Task PersistRawBatchAsync(IReadOnlyCollection<(string? DeviceId, string Direction, byte[] Payload)> batch)
    {
        if (scopeFactory == null || batch.Count == 0)
        {
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRawFrameRepository>();
        await repository.LogRawBatchAsync(batch.Select(item => (item.DeviceId, item.Direction, item.Payload))).ConfigureAwait(false);
    }
}
