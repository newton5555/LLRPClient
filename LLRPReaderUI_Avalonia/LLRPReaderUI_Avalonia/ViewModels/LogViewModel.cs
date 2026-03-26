using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LLRPReaderUI_Avalonia.Logging;
using LLRPReaderUI_Avalonia.Messages;

namespace LLRPReaderUI_Avalonia.ViewModels;

public partial class LogViewModel : ObservableObject
{
    private const int MaxRows = 500;
    private const int UIUpdateIntervalMs = 200;
    private readonly IAppLogService appLogService;
    private readonly ConcurrentQueue<AppLogEntry> pendingEntries = new();
    private readonly DispatcherTimer uiUpdateTimer;

    public LogViewModel(IAppLogService appLogService)
    {
        this.appLogService = appLogService;
        WeakReferenceMessenger.Default.Register<LogViewModel, ConnectionStateChangedMessage>(this, static (r, m) =>
        {
            r.OnConnectionStateChanged(m.Value);
        });

        foreach (var entry in appLogService.Snapshot())
        {
            AddEntryToView(entry);
        }

        appLogService.EntryAdded += OnEntryAdded;

        uiUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(UIUpdateIntervalMs)
        };
        uiUpdateTimer.Tick += OnUIUpdateTimerTick;
        uiUpdateTimer.Start();
    }

    public ObservableCollection<string> OperationEntries { get; } = [];

    public ObservableCollection<string> LlrpMessageEntries { get; } = [];

    public ObservableCollection<string> RawPacketEntries { get; } = [];

    [ObservableProperty]
    private bool showOperationLogs = true;

    [ObservableProperty]
    private bool showLlrpMessageLogs = true;

    [ObservableProperty]
    private bool showRawFrameLogs = true;

    [ObservableProperty]
    private int totalLogCount = 0;

    [RelayCommand]
    private void ClearLogs()
    {
        appLogService.ClearInMemory();
        OperationEntries.Clear();
        LlrpMessageEntries.Clear();
        RawPacketEntries.Clear();
        TotalLogCount = 0;
        OperationEntries.Add($"{DateTime.Now:HH:mm:ss.fff} [Information] 日志已清空");
    }

    [RelayCommand]
    private void ClearOperationLogs()
    {
        OperationEntries.Clear();
        TotalLogCount -= OperationEntries.Count;
    }

    [RelayCommand]
    private void ClearLlrpMessageLogs()
    {
        LlrpMessageEntries.Clear();
        TotalLogCount -= LlrpMessageEntries.Count;
    }

    [RelayCommand]
    private void ClearRawFrameLogs()
    {
        RawPacketEntries.Clear();
        TotalLogCount -= RawPacketEntries.Count;
    }

    private void OnConnectionStateChanged(bool isConnected)
    {
        appLogService.LogOperation(isConnected ? "设备连接成功" : "设备已断开");
    }

    private void OnEntryAdded(AppLogEntry entry)
    {
        pendingEntries.Enqueue(entry);
    }

    private void OnUIUpdateTimerTick(object? sender, EventArgs e)
    {
        while (pendingEntries.TryDequeue(out var entry))
        {
            AddEntryToView(entry);
        }
    }

    private void AddEntryToView(AppLogEntry entry)
    {
        var line = $"{entry.Timestamp:HH:mm:ss.fff} [{entry.Level}] {entry.Message}";
        if (!string.IsNullOrWhiteSpace(entry.Exception))
        {
            line += $" | {entry.Exception}";
        }

        switch (entry.Category)
        {
            case AppLogCategory.Operation:
                if (ShowOperationLogs)
                {
                    InsertCapped(OperationEntries, line);
                    TotalLogCount++;
                }
                break;
            case AppLogCategory.LlrpMessage:
                if (ShowLlrpMessageLogs)
                {
                    InsertCapped(LlrpMessageEntries, line);
                    TotalLogCount++;
                }
                break;
            case AppLogCategory.RawFrame:
                if (ShowRawFrameLogs)
                {
                    InsertCapped(RawPacketEntries, line);
                    TotalLogCount++;
                }
                break;
        }
    }

    private static void InsertCapped(ObservableCollection<string> collection, string line)
    {
        collection.Add(line);
        while (collection.Count > MaxRows)
        {
            collection.RemoveAt(0);
        }
    }
}
