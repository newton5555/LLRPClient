using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LLRPSdk;
using LLRPReaderUI_Avalonia.Logging;
using LLRPReaderUI_Avalonia.Messages;
using LLRPReaderUI_Avalonia.Models;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using System;
using Avalonia;
using System.Collections.Generic;

namespace LLRPReaderUI_Avalonia.ViewModels;

public partial class InventoryViewModel : ViewModelBase
{
    private const int MaxRows = 500;
    private static readonly TimeSpan ManualPullAcceptWindow = TimeSpan.FromSeconds(2);
    private readonly LlrpReader reader;
    private readonly IAppLogService logs;
    private readonly ReaderSettingsStore settingsStore;
    private readonly HashSet<string> uniqueEpcs = new(StringComparer.OrdinalIgnoreCase);
    private DateTime manualPullAcceptUntilUtc = DateTime.MinValue;

    public InventoryViewModel(LlrpReader reader, IAppLogService logs, ReaderSettingsStore settingsStore)
    {
        this.reader = reader;
        this.logs = logs;
        this.settingsStore = settingsStore;
        this.reader.TagsReported += OnTagsReported;
        this.reader.ReaderStopped += OnReaderStopped;
        WeakReferenceMessenger.Default.Register<InventoryViewModel, ConnectionStateChangedMessage>(this, static (r, m) =>
        {
            r.OnConnectionStateChanged(m.Value);
        });
        WeakReferenceMessenger.Default.Register<InventoryViewModel, StatusUpdateRequestedMessage>(this, static (r, m) =>
        {
            r.OnStatusUpdateRequested(m.Value);
        });
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartInventoryCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopInventoryCommand))]
    private bool isRunning;

    [ObservableProperty]
    private string inventoryState = "未开始";

    [ObservableProperty]
    private int totalReports;

    [ObservableProperty]
    private int totalTags;

    [ObservableProperty]
    private int uniqueTagCount;

    [ObservableProperty]
    private ObservableCollection<InventoryTagItemViewModel> receivedTags = [];

    [ObservableProperty]
    private bool attachedDataEnabled;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ManualPullBufferedReportsCommand))]
    private bool isManualPullAvailable;

    private bool CanStartInventory() => !IsRunning;

    private bool CanStopInventory() => IsRunning;

    [RelayCommand(CanExecute = nameof(CanStartInventory))]
    private void StartInventory()
    {
        if (!reader.IsConnected)
        {
            InventoryState = "请先连接设备";
            logs.LogOperation("盘点开始失败：设备未连接", Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }

        try
        {
            RefreshAttachedDataEnabled();
            reader.Start();
            ClearReceivedData();
            IsRunning = true;
            InventoryState = "寻卡中";
            logs.LogOperation("开始寻卡");
            WeakReferenceMessenger.Default.Send(new StatusUpdateRequestedMessage("InventoryStarted"));
        }
        catch (Exception ex)
        {
            IsRunning = false;
            InventoryState = $"开始失败：{ex.Message}";
            logs.LogOperation($"开始寻卡失败：{ex.Message}", Microsoft.Extensions.Logging.LogLevel.Error, ex);
        }
    }

    [RelayCommand(CanExecute = nameof(CanStopInventory))]
    private void StopInventory()
    {
        try
        {
            reader.Stop();
            IsRunning = false;
            InventoryState = "已停止";
            logs.LogOperation("停止寻卡");
            WeakReferenceMessenger.Default.Send(new StatusUpdateRequestedMessage("InventoryStopped"));
        }
        catch (Exception ex)
        {
            InventoryState = $"停止失败：{ex.Message}";
            logs.LogOperation($"停止寻卡失败：{ex.Message}", Microsoft.Extensions.Logging.LogLevel.Error, ex);
        }
    }

    [RelayCommand]
    private void ClearReceivedData()
    {
        ReceivedTags.Clear();
        uniqueEpcs.Clear();
        TotalReports = 0;
        TotalTags = 0;
        UniqueTagCount = 0;
        logs.LogOperation("清空盘点数据");
    }

    [RelayCommand(CanExecute = nameof(CanManualPullBufferedReports))]
    private void ManualPullBufferedReports()
    {
        if (!reader.IsConnected)
        {
            InventoryState = "请先连接设备";
            logs.LogOperation("手动拉缓存失败：设备未连接", Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }

        if (!IsManualPullAvailable)
        {
            InventoryState = "仅 WaitForQuery 模式支持手动拉缓存";
            logs.LogOperation("手动拉缓存失败：当前上报模式不是 WaitForQuery", Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }

        try
        {
            RefreshAttachedDataEnabled();
            manualPullAcceptUntilUtc = DateTime.UtcNow.Add(ManualPullAcceptWindow);
            reader.QueryTags();
            InventoryState = "已发送拉取缓存请求";
            logs.LogOperation("已发送手动拉取缓存请求(GET_REPORT)");
        }
        catch (Exception ex)
        {
            InventoryState = $"拉取失败：{ex.Message}";
            logs.LogOperation($"手动拉缓存失败：{ex.Message}", Microsoft.Extensions.Logging.LogLevel.Error, ex);
        }
    }

    private bool CanManualPullBufferedReports() => IsManualPullAvailable;

    private void OnReaderStopped(LlrpReader _reader, ReaderStoppedEvent _eventArgs)
    {
        // 只有在寻卡运行时才处理停止事件，避免读写操作中的 reader.Stop() 影响此处
        if (!IsRunning)
            return;

        RunOnUi(() =>
        {
            IsRunning = false;
            InventoryState = "已停止";
            logs.LogOperation("读写器事件：盘点已停止");
            WeakReferenceMessenger.Default.Send(new StatusUpdateRequestedMessage("InventoryStoppedByReader"));
        });
    }

    private void OnTagsReported(LlrpReader _reader, TagReport report)
    {
        bool fromManualPull = DateTime.UtcNow <= manualPullAcceptUntilUtc;

        // 默认只在寻卡中处理；手动拉缓存命令触发后短时间窗口内也允许处理
        if (!IsRunning && !fromManualPull)
            return;

        RunOnUi(() =>
        {
            TotalReports++;
            TotalTags += report.Tags.Count;
            logs.LogOperation($"收到标签上报：{report.Tags.Count} 个");

            foreach (var tag in report.Tags)
            {
                var epc = tag.Epc?.ToHexString() ?? string.Empty;
                var attachedData = "-";
                if (AttachedDataEnabled && tag.ReadOperationResults is { Count: > 0 })
                {
                    var successRead = tag.ReadOperationResults.FirstOrDefault(x => x.Result == ReadResultStatus.Success && x.Data != null);
                    attachedData = successRead?.Data?.ToHexString() ?? "-";
                }
                if (!string.IsNullOrWhiteSpace(epc))
                {
                    uniqueEpcs.Add(epc);
                }

                ReceivedTags.Insert(0, new InventoryTagItemViewModel
                {
                    ReceiveTime = DateTime.Now,
                    Epc = epc,
                    Antenna = tag.IsAntennaPortNumberPresent ? tag.AntennaPortNumber.ToString() : "-",
                    ChannelMhz = tag.IsChannelInMhzPresent ? tag.ChannelInMhz.ToString("F3") : "-",
                    Rssi = tag.IsPeakRssiPresent ? tag.PeakRssi.ToString("F1") : "-",
                    SeenCount = tag.IsSeenCountPresent ? tag.TagSeenCount.ToString() : "-",
                    AttachedData = attachedData
                });
            }

            while (ReceivedTags.Count > MaxRows)
            {
                ReceivedTags.RemoveAt(ReceivedTags.Count - 1);
            }

            UniqueTagCount = uniqueEpcs.Count;
        });
    }

    private static void RunOnUi(Action action)
    {
        var dispatcher = Dispatcher.UIThread;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            action();
            return;
        }

        dispatcher.Post(action);
    }

    public void OnConnectionStateChanged(bool isConnected)
    {
        RunOnUi(() =>
        {
            if (!isConnected)
            {
                IsRunning = false;
                InventoryState = "请先连接设备";
                AttachedDataEnabled = false;
                IsManualPullAvailable = false;
                return;
            }

            RefreshAttachedDataEnabled();
            RefreshManualPullAvailability();
            if (!IsRunning)
            {
                InventoryState = "已连接，待开始";
            }
        });
    }

    private void OnStatusUpdateRequested(string reason)
    {
        if (!reader.IsConnected)
        {
            AttachedDataEnabled = false;
            return;
        }

        if (reason.Contains("AttachedData", StringComparison.OrdinalIgnoreCase)
            || reason.Contains("Settings", StringComparison.OrdinalIgnoreCase)
            || reason.Contains("Inventory", StringComparison.OrdinalIgnoreCase))
        {
            RefreshAttachedDataEnabled();
            RefreshManualPullAvailability();
        }
    }

    private void RefreshAttachedDataEnabled()
    {
        if (settingsStore.TryGetSnapshot(out var settings) && settings is not null)
        {
            AttachedDataEnabled = settings.AttachedData?.Enabled ?? false;
            return;
        }

        AttachedDataEnabled = false;
    }

    private void RefreshManualPullAvailability()
    {
        if (!reader.IsConnected)
        {
            IsManualPullAvailable = false;
            return;
        }

        if (settingsStore.TryGetSnapshot(out var settings) && settings?.Report != null)
        {
            IsManualPullAvailable = settings.Report.Mode == ReportMode.WaitForQuery;
            return;
        }

        IsManualPullAvailable = false;
    }
}

public class InventoryTagItemViewModel
{
    public DateTime ReceiveTime { get; set; }
    public string Epc { get; set; } = string.Empty;
    public string Antenna { get; set; } = "-";
    public string ChannelMhz { get; set; } = "-";
    public string Rssi { get; set; } = "-";
    public string SeenCount { get; set; } = "-";
    public string AttachedData { get; set; } = "-";
}


