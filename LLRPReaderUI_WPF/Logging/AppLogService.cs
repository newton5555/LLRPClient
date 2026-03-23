using LLRPSdk;
using LLRPReaderUI_WPF.Data;
using LLRPReaderUI_WPF.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading;

namespace LLRPReaderUI_WPF.Logging;

public sealed class AppLogService : IAppLogService
{
    private const int MaxEntries = 5000;
    private readonly object gate = new();
    private readonly List<AppLogEntry> entries = new();
    private readonly ILogger<AppLogService> logger;
    private readonly IRawFrameRepository? repository;
    private readonly LlrpReader? reader;
    private readonly ReaderStatusStore? statusStore;
    private readonly ConcurrentQueue<(string? DeviceId, string Direction, byte[] Payload)> rawFrameQueue = new();
    private readonly CancellationTokenSource rawFrameCts = new();
    private readonly Task? rawFrameWorker;

    public AppLogService(
        ILogger<AppLogService> logger,
        IRawFrameRepository? repository = null,
        LlrpReader? reader = null,
        ReaderStatusStore? statusStore = null)
    {
        this.logger = logger;
        this.repository = repository;
        this.reader = reader;
        this.statusStore = statusStore;
        if (this.repository != null)
        {
            // 启动后台工作线程，负责从队列消费并写入持久化
            rawFrameWorker = Task.Run(async () =>
            {
                while (!rawFrameCts.IsCancellationRequested)
                {
                    try
                    {
                        if (rawFrameQueue.TryDequeue(out var item))
                        {
                            try
                            {
                                await this.repository.LogRawAsync(item.DeviceId, item.Direction, item.Payload).ConfigureAwait(false);
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
                        // 防止工作线程退出，等待一会儿再重试
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
        // 将数据先入队列，由后台任务批量/顺序写入持久化层，避免短时间大量并发 Task
        if (repository != null)
        {
            // shallow copy payload to avoid buffer reuse issues
            var copy = payload != null ? (byte[])payload.Clone() : Array.Empty<byte>();
            var deviceId = GetDeviceId();
            rawFrameQueue.Enqueue((deviceId, direction, copy));
        }
    }

    private string? GetDeviceId()
    {
        // 优先使用 ReaderIdentity（MAC 地址）
        //if (statusStore != null && statusStore.TryGetSnapshot(out var status) && status.ReaderIdentity != null)
        //{
        //    return status.ReaderIdentity.ToString();
        //}
        // 退回到 IP 地址
        if (reader != null && !string.IsNullOrEmpty(reader.Address))
        {
            return reader.Address;
        }
        return null;
    }

    private void AddEntry(AppLogEntry entry)
    {

        EntryAdded?.Invoke(entry);
    }
}
