using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LLRPSdk;
using LLRPReaderUI_WPF.Logging;
using LLRPReaderUI_WPF.Messages;
using LLRPReaderUI_WPF.Models;
using LLRPReaderUI_WPF.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace LLRPReaderUI_WPF.ViewModels;

public partial class InventoryViewModel : ObservableObject
{
    private const int MaxRows = 500;
    private const string EmptyValue = "-";
    private static readonly TimeSpan ManualPullAcceptWindow = TimeSpan.FromSeconds(2);
    private readonly LlrpReader reader;
    private readonly IAppLogService logs;
    private readonly ReaderSettingsStore settingsStore;
    private readonly LanguageService _languageService;
    private readonly Dictionary<string, InventoryTagItemViewModel> aggregatedTags = new(StringComparer.OrdinalIgnoreCase);
    private readonly DispatcherTimer inventoryElapsedTimer;
    private DateTime manualPullAcceptUntilUtc = DateTime.MinValue;
    private DateTime? inventoryStartedAt;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartInventoryCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopInventoryCommand))]
    private bool isRunning;

    [ObservableProperty]
    private string inventoryState = string.Empty;

    [ObservableProperty]
    private int totalReports;

    [ObservableProperty]
    private int totalTags;

    [ObservableProperty]
    private int uniqueTagCount;

    [ObservableProperty]
    private string inventoryDuration = "00:00:00.000";

    [ObservableProperty]
    private ObservableCollection<InventoryTagItemViewModel> receivedTags = [];

    [ObservableProperty]
    private bool attachedDataEnabled;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ManualPullBufferedReportsCommand))]
    private bool isManualPullAvailable;

    [ObservableProperty]
    private bool showPcColumn;

    [ObservableProperty]
    private bool showCrcColumn;

    [ObservableProperty]
    private bool showFirstSeenTimestampUtcColumn;

    [ObservableProperty]
    private bool showLastSeenTimestampUtcColumn;

    [ObservableProperty]
    private bool showAntennaPortNumberColumn;

    [ObservableProperty]
    private bool showChannelColumn;

    [ObservableProperty]
    private bool showPeakRssiColumn;

    [ObservableProperty]
    private bool showSeenCountColumn;

    public InventoryViewModel(LlrpReader reader, IAppLogService logs, ReaderSettingsStore settingsStore, LanguageService languageService)
    {
        this.reader = reader;
        this.logs = logs;
        this.settingsStore = settingsStore;
        _languageService = languageService;
        this.reader.TagsReported += OnTagsReported;
        this.reader.ReaderStarted += OnReaderStarted;
        this.reader.ReaderStopped += OnReaderStopped;
        WeakReferenceMessenger.Default.Register<InventoryViewModel, ConnectionStateChangedMessage>(this, static (r, m) =>
        {
            r.OnConnectionStateChanged(m.Value);
        });
        WeakReferenceMessenger.Default.Register<InventoryViewModel, StatusUpdateRequestedMessage>(this, static (r, m) =>
        {
            r.OnStatusUpdateRequested(m.Value);
        });

        // Subscribe to language changes
        _languageService.OnLanguageChanged += OnLanguageChanged;

        // Set initial state
        InventoryState = _languageService.GetLocalizedString("Inventory.NotStarted");
        inventoryElapsedTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        inventoryElapsedTimer.Tick += (_, _) => UpdateInventoryDuration();

        RefreshReportColumnVisibility();
    }

    private void OnLanguageChanged(AppLanguage language)
    {
        // Refresh UI state text if not running
        if (!IsRunning)
        {
            if (reader.IsConnected)
            {
                InventoryState = _languageService.GetLocalizedString("Inventory.ConnectedReady");
            }
            else
            {
                InventoryState = _languageService.GetLocalizedString("Inventory.NotStarted");
            }
        }
    }

    /// <summary>
    /// Get a localized string with format arguments.
    /// </summary>
    private string GetLocalizedString(string key, params object[] args)
    {
        var format = _languageService.GetLocalizedString(key);
        return args.Length > 0 ? string.Format(format, args) : format;
    }

    private bool CanStartInventory() => !IsRunning;

    private bool CanStopInventory() => IsRunning;

    [RelayCommand(CanExecute = nameof(CanStartInventory))]
    private void StartInventory()
    {
        if (!reader.IsConnected)
        {
            InventoryState = _languageService.GetLocalizedString("Common.ConnectFirst");
            logs.LogOperation(_languageService.GetLocalizedString("Inventory.StartFailedNotConnected"), Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }

        try
        {
            RefreshAttachedDataEnabled();
            reader.Start();
            ClearReceivedData();
            inventoryStartedAt = DateTime.Now;
            UpdateInventoryDuration();
            inventoryElapsedTimer.Start();
            IsRunning = true;
            InventoryState = _languageService.GetLocalizedString("Inventory.Running");
            logs.LogOperation(_languageService.GetLocalizedString("Inventory.StartLog"));
            WeakReferenceMessenger.Default.Send(new StatusUpdateRequestedMessage("InventoryStarted"));
        }
        catch (Exception ex)
        {
            inventoryElapsedTimer.Stop();
            inventoryStartedAt = null;
            IsRunning = false;
            InventoryState = GetLocalizedString("Inventory.StartFailed", ex.Message);
            logs.LogOperation(GetLocalizedString("Inventory.StartFailedLog", ex.Message), Microsoft.Extensions.Logging.LogLevel.Error, ex);
        }
    }

    [RelayCommand(CanExecute = nameof(CanStopInventory))]
    private async void StopInventory()
    {
        try
        {
            InventoryState = _languageService.GetLocalizedString("Inventory.Stopping");
            reader.Stop();
            // 等待一段时间让阅读器发送缓存的标签数据
            // 阅读器在停止时会发送最后一批 RO_ACCESS_REPORT
            await Task.Delay(100);
            inventoryElapsedTimer.Stop();
            UpdateInventoryDuration();
            IsRunning = false;
            InventoryState = _languageService.GetLocalizedString("Inventory.Stopped");
            logs.LogOperation(_languageService.GetLocalizedString("Inventory.StopLog"));
            WeakReferenceMessenger.Default.Send(new StatusUpdateRequestedMessage("InventoryStopped"));
        }
        catch (Exception ex)
        {
            inventoryElapsedTimer.Stop();
            UpdateInventoryDuration();
            IsRunning = false;
            InventoryState = GetLocalizedString("Inventory.StopFailed", ex.Message);
            logs.LogOperation(GetLocalizedString("Inventory.StopFailedLog", ex.Message), Microsoft.Extensions.Logging.LogLevel.Error, ex);
        }
    }

    [RelayCommand]
    private void ClearReceivedData()
    {
        ReceivedTags.Clear();
        aggregatedTags.Clear();
        TotalReports = 0;
        TotalTags = 0;
        UniqueTagCount = 0;
        logs.LogOperation(_languageService.GetLocalizedString("Inventory.DataCleared"));
    }

    [RelayCommand(CanExecute = nameof(CanManualPullBufferedReports))]
    private void ManualPullBufferedReports()
    {
        if (!reader.IsConnected)
        {
            InventoryState = _languageService.GetLocalizedString("Common.ConnectFirst");
            logs.LogOperation(_languageService.GetLocalizedString("Inventory.PullFailedNotConnected"), Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }

        if (!IsManualPullAvailable)
        {
            InventoryState = _languageService.GetLocalizedString("Inventory.PullNotSupported");
            logs.LogOperation(_languageService.GetLocalizedString("Inventory.PullNotSupportedLog"), Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }

        try
        {
            RefreshAttachedDataEnabled();
            manualPullAcceptUntilUtc = DateTime.UtcNow.Add(ManualPullAcceptWindow);
            reader.QueryTags();
            InventoryState = _languageService.GetLocalizedString("Inventory.PullSent");
            logs.LogOperation(_languageService.GetLocalizedString("Inventory.PullSentLog"));
        }
        catch (Exception ex)
        {
            InventoryState = GetLocalizedString("Inventory.PullFailed", ex.Message);
            logs.LogOperation(GetLocalizedString("Inventory.PullFailedLog", ex.Message), Microsoft.Extensions.Logging.LogLevel.Error, ex);
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
            inventoryElapsedTimer.Stop();
            UpdateInventoryDuration();
            IsRunning = false;
            InventoryState = _languageService.GetLocalizedString("Inventory.Stopped");
            logs.LogOperation(_languageService.GetLocalizedString("Inventory.ReaderStopped"));
            WeakReferenceMessenger.Default.Send(new StatusUpdateRequestedMessage("InventoryStoppedByReader"));
        });
    }

    private void OnReaderStarted(LlrpReader _reader, ReaderStartedEvent _eventArgs)
    {
        RunOnUi(EnsureInventoryRunning);
    }

    private void OnTagsReported(LlrpReader _reader, TagReport report)
    {
        bool fromManualPull = DateTime.UtcNow <= manualPullAcceptUntilUtc;
        var reportSummary = string.Join(" | ", report.Tags.Select(FormatTagDebugSummary));
        var visibleTags = report.Tags
            .Where(ShouldDisplayInInventory)
            .ToList();
        var filteredTags = report.Tags
            .Where(tag => !ShouldDisplayInInventory(tag))
            .Select(FormatTagDebugSummary)
            .ToList();

        logs.LogOperation($"[InventoryDebug] Incoming Tags={report.Tags.Count} Visible={visibleTags.Count} Filtered={filteredTags.Count} IsRunning={IsRunning} ManualPull={fromManualPull}");
        if (!string.IsNullOrWhiteSpace(reportSummary))
        {
            logs.LogOperation($"[InventoryDebug] Incoming Detail: {reportSummary}");
        }
        if (filteredTags.Count > 0)
        {
            logs.LogOperation($"[InventoryDebug] Filtered Detail: {string.Join(" | ", filteredTags)}");
        }

        if (visibleTags.Count == 0)
        {
            return;
        }

        // 默认只在寻卡中处理；手动拉缓存命令触发后短时间窗口内也允许处理
        if (!IsRunning && !fromManualPull)
        {
            RunOnUi(EnsureInventoryRunning);
        }

        if (!IsRunning && !fromManualPull)
            return;

        RunOnUi(() =>
        {
            TotalReports++;
            TotalTags += visibleTags.Count;
            logs.LogOperation(GetLocalizedString("Inventory.TagReport", visibleTags.Count));

            foreach (var tag in visibleTags)
            {
                UpsertAggregatedTag(tag);
            }

            TrimReceivedTags();
            UniqueTagCount = ReceivedTags.Count;
        });
    }

    private void EnsureInventoryRunning()
    {
        if (IsRunning)
        {
            return;
        }

        inventoryStartedAt ??= DateTime.Now;
        UpdateInventoryDuration();
        inventoryElapsedTimer.Start();
        IsRunning = true;
        InventoryState = _languageService.GetLocalizedString("Inventory.Running");
    }

    private bool ShouldDisplayInInventory(Tag tag)
    {

        return !(tag.ReportSource == TagReportSource.Unknown || tag.ReportSource == TagReportSource.TagOperation);
           
        return true;
    }

    private void UpsertAggregatedTag(Tag tag)
    {
        var aggregationKey = string.IsNullOrWhiteSpace(tag.Epc?.ToHexString())
            ? EmptyValue
            : tag.Epc!.ToHexString();

        if (!aggregatedTags.TryGetValue(aggregationKey, out var row))
        {
            row = new InventoryTagItemViewModel
            {
                Epc = aggregationKey
            };
            aggregatedTags[aggregationKey] = row;
            ReceivedTags.Insert(0, row);
        }
        else
        {
            ReceivedTags.Remove(row);
            ReceivedTags.Insert(0, row);
        }

        var reportSeenCount = tag.IsSeenCountPresent ? tag.TagSeenCount : (ushort)1;
        row.ReceiveTime = DateTime.Now;
        row.Epc = aggregationKey;
        row.Antenna = tag.IsAntennaPortNumberPresent ? tag.AntennaPortNumber.ToString() : EmptyValue;
        row.ChannelMhz = tag.IsChannelInMhzPresent ? tag.ChannelInMhz.ToString("F3") : EmptyValue;
        row.Rssi = tag.IsPeakRssiPresent ? tag.PeakRssi.ToString("F1") : EmptyValue;
        row.SeenCountValue += reportSeenCount;
        row.SeenCount = row.SeenCountValue.ToString();
        row.Pc = tag.IsPcBitsPresent ? $"0x{tag.PcBits:X4}" : EmptyValue;
        row.Crc = tag.IsCrcPresent ? $"0x{tag.Crc:X4}" : EmptyValue;
        row.AttachedData = ResolveAttachedData(tag);
        row.FirstSeenTimestampUtc = ResolveFirstSeenTimestamp(row, tag);
        row.LastSeenTimestampUtc = ResolveLastSeenTimestamp(row, tag);
    }

    private static string FormatTagDebugSummary(Tag tag)
    {
        var epc = tag.Epc?.ToHexString() ?? EmptyValue;
        var accessSpecId = tag.AccessSpecId?.ToString() ?? "null";
        return $"EPC={epc},Source={tag.ReportSource},AccessSpecId={accessSpecId},HasAccessResults={tag.HasAccessOperationResults},ReadResults={tag.ReadOperationResults.Count}";
    }

    private void TrimReceivedTags()
    {
        while (ReceivedTags.Count > MaxRows)
        {
            var removed = ReceivedTags[^1];
            aggregatedTags.Remove(removed.Epc);
            ReceivedTags.RemoveAt(ReceivedTags.Count - 1);
        }
    }

    private string ResolveAttachedData(Tag tag)
    {
        if (!AttachedDataEnabled || tag.ReadOperationResults is not { Count: > 0 })
        {
            return EmptyValue;
        }

        var successRead = tag.ReadOperationResults.FirstOrDefault(x => x.Result == ReadResultStatus.Success && x.Data != null);
        return successRead?.Data?.ToHexString() ?? EmptyValue;
    }

    private static string ResolveFirstSeenTimestamp(InventoryTagItemViewModel row, Tag tag)
    {
        var candidate = FormatUtcTimestamp(tag.IsFirstSeenTimePresent, tag.FirstSeenTime);
        if (row.FirstSeenTimestampValueUtc is null)
        {
            row.FirstSeenTimestampValueUtc = GetUtcDateTime(tag.IsFirstSeenTimePresent, tag.FirstSeenTime);
            return candidate;
        }

        var reportValue = GetUtcDateTime(tag.IsFirstSeenTimePresent, tag.FirstSeenTime);
        if (reportValue is not null && reportValue < row.FirstSeenTimestampValueUtc)
        {
            row.FirstSeenTimestampValueUtc = reportValue;
            return candidate;
        }

        return row.FirstSeenTimestampUtc;
    }

    private static string ResolveLastSeenTimestamp(InventoryTagItemViewModel row, Tag tag)
    {
        var candidate = FormatUtcTimestamp(tag.IsLastSeenTimePresent, tag.LastSeenTime);
        var reportValue = GetUtcDateTime(tag.IsLastSeenTimePresent, tag.LastSeenTime);
        if (row.LastSeenTimestampValueUtc is null)
        {
            row.LastSeenTimestampValueUtc = reportValue;
            return candidate;
        }

        if (reportValue is not null && reportValue > row.LastSeenTimestampValueUtc)
        {
            row.LastSeenTimestampValueUtc = reportValue;
            return candidate;
        }

        return row.LastSeenTimestampUtc;
    }

    private static DateTime? GetUtcDateTime(bool isPresent, Timestamp? timestamp)
    {
        if (!isPresent || timestamp is null)
        {
            return null;
        }

        try
        {
            return timestamp.UTCDateTime;
        }
        catch
        {
            return null;
        }
    }

    private static void RunOnUi(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            action();
            return;
        }

        dispatcher.Invoke(action);
    }

    public void OnConnectionStateChanged(bool isConnected)
    {
        RunOnUi(() =>
        {
            if (!isConnected)
            {
                inventoryElapsedTimer.Stop();
                inventoryStartedAt = null;
                InventoryDuration = "00:00:00.000";
                IsRunning = false;
                InventoryState = _languageService.GetLocalizedString("Common.ConnectFirst");
                AttachedDataEnabled = false;
                IsManualPullAvailable = false;
                RefreshReportColumnVisibility();
                return;
            }

            RefreshAttachedDataEnabled();
            RefreshManualPullAvailability();
            RefreshReportColumnVisibility();
            if (!IsRunning)
            {
                InventoryState = _languageService.GetLocalizedString("Inventory.ConnectedReady");
            }
        });
    }

    private void OnStatusUpdateRequested(string reason)
    {
        if (!reader.IsConnected)
        {
            AttachedDataEnabled = false;
            RefreshReportColumnVisibility();
            return;
        }

        if (reason.Contains("AttachedData", StringComparison.OrdinalIgnoreCase)
            || reason.Contains("Settings", StringComparison.OrdinalIgnoreCase)
            || reason.Contains("Inventory", StringComparison.OrdinalIgnoreCase))
        {
            RefreshAttachedDataEnabled();
            RefreshManualPullAvailability();
            RefreshReportColumnVisibility();
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

    private void RefreshReportColumnVisibility()
    {
        if (settingsStore.TryGetSnapshot(out var settings) && settings?.Report is not null)
        {
            ShowPcColumn = settings.Report.IncludePcBits;
            ShowCrcColumn = settings.Report.IncludeCrc;
            ShowFirstSeenTimestampUtcColumn = settings.Report.IncludeFirstSeenTime;
            ShowLastSeenTimestampUtcColumn = settings.Report.IncludeLastSeenTime;
            ShowAntennaPortNumberColumn = settings.Report.IncludeAntennaPortNumber;
            ShowChannelColumn = settings.Report.IncludeChannel;
            ShowPeakRssiColumn=settings.Report.IncludePeakRssi;
            ShowSeenCountColumn=settings.Report.IncludeSeenCount;
            return;
        }

        ShowPcColumn = false;
        ShowCrcColumn = false;
        ShowFirstSeenTimestampUtcColumn = false;
        ShowLastSeenTimestampUtcColumn = false;

        ShowAntennaPortNumberColumn = false;
        ShowChannelColumn = false;
        ShowPeakRssiColumn = false;
        ShowSeenCountColumn = false;
    }

    private static string FormatUtcTimestamp(bool isPresent, Timestamp? timestamp)
    {
        if (!isPresent || timestamp is null)
        {
            return "-";
        }

        try
        {
            var utcDateTime = timestamp.UTCDateTime;
            return utcDateTime.ToString("yy-MM-dd HH:mm:ss.fff");
        }
        catch
        {
            return timestamp.Utc.ToString();
        }
    }

    private void UpdateInventoryDuration()
    {
        if (inventoryStartedAt is null)
        {
            InventoryDuration = "00:00:00.000";
            return;
        }

        var elapsed = DateTime.Now - inventoryStartedAt.Value;
        if (elapsed < TimeSpan.Zero)
        {
            elapsed = TimeSpan.Zero;
        }

        InventoryDuration = $"{(int)elapsed.TotalHours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}.{elapsed.Milliseconds:000}";
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
    public string Pc { get; set; } = "-";
    public string Crc { get; set; } = "-";
    public string FirstSeenTimestampUtc { get; set; } = "-";
    public string LastSeenTimestampUtc { get; set; } = "-";
    public string AttachedData { get; set; } = "-";
    public int SeenCountValue { get; set; }
    public DateTime? FirstSeenTimestampValueUtc { get; set; }
    public DateTime? LastSeenTimestampValueUtc { get; set; }
}
