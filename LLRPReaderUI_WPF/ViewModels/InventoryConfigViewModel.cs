using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LLRPSdk;
using LLRPReaderUI_WPF.Logging;
using LLRPReaderUI_WPF.Messages;
using LLRPReaderUI_WPF.Models;
using System.Collections.ObjectModel;

namespace LLRPReaderUI_WPF.ViewModels;

public partial class InventoryConfigViewModel : ObservableObject
{
    public InventoryConfigViewModel()
    {
        this.reader = null!;
        this.logs = null!;
        this.settingsStore = null!;
    }



    private readonly LlrpReader reader;
    private readonly IAppLogService logs;
    private readonly ReaderSettingsStore settingsStore;

    public InventoryConfigViewModel(LlrpReader reader, IAppLogService logs, ReaderSettingsStore settingsStore)
    {
        this.reader = reader;
        this.logs = logs;
        this.settingsStore = settingsStore;
        WeakReferenceMessenger.Default.Register<InventoryConfigViewModel, ConnectionStateChangedMessage>(this, static (r, m) =>
        {
            r.OnConnectionStateChanged(m.Value);
        });
    }

    public IReadOnlyList<AutoStartMode> AutoStartModes { get; } = Enum.GetValues<AutoStartMode>();
    public IReadOnlyList<AutoStopMode> AutoStopModes { get; } = Enum.GetValues<AutoStopMode>();
    public IReadOnlyList<TagFilterMode> TagFilterModes { get; } = Enum.GetValues<TagFilterMode>();
    public IReadOnlyList<MemoryBank> MemoryBanks { get; } = Enum.GetValues<MemoryBank>();
    public IReadOnlyList<TagFilterOp> TagFilterOps { get; } = Enum.GetValues<TagFilterOp>();
    public IReadOnlyList<StateUnawareAction> StateUnawareActions { get; } = Enum.GetValues<StateUnawareAction>();
    public IReadOnlyList<ReportMode> ReportModes { get; } = Enum.GetValues<ReportMode>();

    [ObservableProperty]
    private string operationResult = "未操作";

    [ObservableProperty]
    private AutoStartMode autoStartMode;

    [ObservableProperty]
    private ushort autoStartGpiPortNumber = 1;

    [ObservableProperty]
    private bool autoStartGpiLevel;

    [ObservableProperty]
    private uint autoStartFirstDelayInMs;

    [ObservableProperty]
    private uint autoStartPeriodInMs;

    [ObservableProperty]
    private ulong autoStartUtcTimestamp;

    [ObservableProperty]
    private AutoStopMode autoStopMode;

    [ObservableProperty]
    private uint autoStopDurationInMs;

    [ObservableProperty]
    private ushort autoStopGpiPortNumber = 1;

    [ObservableProperty]
    private bool autoStopGpiLevel;

    [ObservableProperty]
    private uint autoStopTimeout;

    [ObservableProperty]
    private TagFilterMode filterMode;

    [ObservableProperty]
    private MemoryBank filter1MemoryBank = MemoryBank.Epc;

    [ObservableProperty]
    private ushort filter1BitPointer;

    [ObservableProperty]
    private int filter1BitCount;

    [ObservableProperty]
    private string filter1TagMask = string.Empty;

    [ObservableProperty]
    private TagFilterOp filter1FilterOp = TagFilterOp.Match;

    [ObservableProperty]
    private MemoryBank filter2MemoryBank = MemoryBank.Epc;

    [ObservableProperty]
    private ushort filter2BitPointer;

    [ObservableProperty]
    private int filter2BitCount;

    [ObservableProperty]
    private string filter2TagMask = string.Empty;

    [ObservableProperty]
    private TagFilterOp filter2FilterOp = TagFilterOp.Match;

    [ObservableProperty]
    private ObservableCollection<TagSelectFilterItemViewModel> tagSelectFilters = [];

    [ObservableProperty]
    private ReportMode reportMode = ReportMode.Individual;

    [ObservableProperty]
    private bool includeAntennaPortNumber;

    [ObservableProperty]
    private bool includeChannel;

    [ObservableProperty]
    private bool includeFirstSeenTime;

    [ObservableProperty]
    private bool includeLastSeenTime;

    [ObservableProperty]
    private bool includeSeenCount;

    [ObservableProperty]
    private bool includePeakRssi;

    [ObservableProperty]
    private bool includePcBits;

    [ObservableProperty]
    private bool includeCrc;

    [ObservableProperty]
    private bool attachedDataEnabled;

    [ObservableProperty]
    private MemoryBank attachedDataMemoryBank = MemoryBank.Tid;

    [ObservableProperty]
    private ushort attachedDataWordPointer;

    [ObservableProperty]
    private ushort attachedDataWordCount = 6;

    [ObservableProperty]
    private string attachedDataAccessPassword = "00000000";

    [RelayCommand]
    private void AddTagSelectFilter()
    {
        TagSelectFilters.Add(new TagSelectFilterItemViewModel
        {
            MemoryBank = MemoryBank.Epc,
            MatchAction = StateUnawareAction.Select,
            NonMatchAction = StateUnawareAction.Unselect
        });
    }

    [RelayCommand]
    private void RemoveTagSelectFilter(TagSelectFilterItemViewModel? item)
    {
        if (item is null)
        {
            return;
        }

        TagSelectFilters.Remove(item);
    }

    [RelayCommand]
    private void QueryInventoryConfig()
    {
        try
        {
            if (!reader.IsConnected)
            {
                OperationResult = "请先连接读写器";
                logs.LogOperation("读取寻卡配置失败：设备未连接", Microsoft.Extensions.Logging.LogLevel.Warning);
                return;
            }

            if (!settingsStore.TryGetSnapshot(out var settings) || settings is null)
            {
                OperationResult = "请先在参数配置页点击获取参数";
                return;
            }

            AutoStartMode = settings.AutoStart.Mode;
            AutoStartGpiPortNumber = settings.AutoStart.GpiPortNumber;
            AutoStartGpiLevel = settings.AutoStart.GpiLevel;
            AutoStartFirstDelayInMs = settings.AutoStart.FirstDelayInMs;
            AutoStartPeriodInMs = settings.AutoStart.PeriodInMs;
            AutoStartUtcTimestamp = settings.AutoStart.UtcTimestamp;

            AutoStopMode = settings.AutoStop.Mode;
            AutoStopDurationInMs = settings.AutoStop.DurationInMs;
            AutoStopGpiPortNumber = settings.AutoStop.GpiPortNumber;
            AutoStopGpiLevel = settings.AutoStop.GpiLevel;
            AutoStopTimeout = settings.AutoStop.Timeout;

            FilterMode = settings.Filters.Mode;
            Filter1MemoryBank = settings.Filters.TagFilter1.MemoryBank;
            Filter1BitPointer = settings.Filters.TagFilter1.BitPointer;
            Filter1BitCount = settings.Filters.TagFilter1.BitCount;
            Filter1TagMask = settings.Filters.TagFilter1.TagMask ?? string.Empty;
            Filter1FilterOp = settings.Filters.TagFilter1.FilterOp;

            Filter2MemoryBank = settings.Filters.TagFilter2.MemoryBank;
            Filter2BitPointer = settings.Filters.TagFilter2.BitPointer;
            Filter2BitCount = settings.Filters.TagFilter2.BitCount;
            Filter2TagMask = settings.Filters.TagFilter2.TagMask ?? string.Empty;
            Filter2FilterOp = settings.Filters.TagFilter2.FilterOp;

            TagSelectFilters.Clear();
            foreach (var filter in settings.Filters.TagSelectFilters)
            {
                TagSelectFilters.Add(new TagSelectFilterItemViewModel
                {
                    MemoryBank = filter.MemoryBank,
                    BitPointer = filter.BitPointer,
                    BitCount = filter.BitCount,
                    TagMask = filter.TagMask ?? string.Empty,
                    MatchAction = filter.MatchAction,
                    NonMatchAction = filter.NonMatchAction
                });
            }

            ReportMode = settings.Report.Mode;
            IncludeAntennaPortNumber = settings.Report.IncludeAntennaPortNumber;
            IncludeChannel = settings.Report.IncludeChannel;
            IncludeFirstSeenTime = settings.Report.IncludeFirstSeenTime;
            IncludeLastSeenTime = settings.Report.IncludeLastSeenTime;
            IncludeSeenCount = settings.Report.IncludeSeenCount;
            IncludePeakRssi = settings.Report.IncludePeakRssi;
            IncludePcBits = settings.Report.IncludePcBits;
            IncludeCrc = settings.Report.IncludeCrc;

            AttachedDataEnabled = settings.AttachedData?.Enabled ?? false;
            AttachedDataMemoryBank = settings.AttachedData?.MemoryBank ?? MemoryBank.Tid;
            AttachedDataWordPointer = settings.AttachedData?.WordPointer ?? (ushort)0;
            AttachedDataWordCount = settings.AttachedData?.WordCount ?? (ushort)6;
            AttachedDataAccessPassword = settings.AttachedData?.AccessPassword ?? "00000000";

            OperationResult = "已读取寻卡配置";
            logs.LogOperation("已读取寻卡配置");
        }
        catch (Exception ex)
        {
            OperationResult = $"读取失败：{ex.Message}";
            logs.LogOperation($"读取寻卡配置失败：{ex.Message}", Microsoft.Extensions.Logging.LogLevel.Error, ex);
        }
    }

    [RelayCommand]
    private void SaveInventoryConfig()
    {
        try
        {
            if (!reader.IsConnected)
            {
                OperationResult = "请先连接读写器";
                logs.LogOperation("保存寻卡配置失败：设备未连接", Microsoft.Extensions.Logging.LogLevel.Warning);
                return;
            }

            if (!settingsStore.TryGetSnapshot(out var settings) || settings is null)
            {
                OperationResult = "请先在参数配置页点击获取参数";
                return;
            }

            settings.AutoStart.Mode = AutoStartMode;
            settings.AutoStart.GpiPortNumber = AutoStartGpiPortNumber;
            settings.AutoStart.GpiLevel = AutoStartGpiLevel;
            settings.AutoStart.FirstDelayInMs = AutoStartFirstDelayInMs;
            settings.AutoStart.PeriodInMs = AutoStartPeriodInMs;
            settings.AutoStart.UtcTimestamp = AutoStartUtcTimestamp;

            settings.AutoStop.Mode = AutoStopMode;
            settings.AutoStop.DurationInMs = AutoStopDurationInMs;
            settings.AutoStop.GpiPortNumber = AutoStopGpiPortNumber;
            settings.AutoStop.GpiLevel = AutoStopGpiLevel;
            settings.AutoStop.Timeout = AutoStopTimeout;

            settings.Filters.Mode = FilterMode;
            settings.Filters.TagFilter1.MemoryBank = Filter1MemoryBank;
            settings.Filters.TagFilter1.BitPointer = Filter1BitPointer;
            settings.Filters.TagFilter1.BitCount = Filter1BitCount;
            settings.Filters.TagFilter1.TagMask = Filter1TagMask?.Trim() ?? string.Empty;
            settings.Filters.TagFilter1.FilterOp = Filter1FilterOp;

            settings.Filters.TagFilter2.MemoryBank = Filter2MemoryBank;
            settings.Filters.TagFilter2.BitPointer = Filter2BitPointer;
            settings.Filters.TagFilter2.BitCount = Filter2BitCount;
            settings.Filters.TagFilter2.TagMask = Filter2TagMask?.Trim() ?? string.Empty;
            settings.Filters.TagFilter2.FilterOp = Filter2FilterOp;

            settings.Filters.TagSelectFilters = TagSelectFilters.Select(x => new TagSelectFilter
            {
                MemoryBank = x.MemoryBank,
                BitPointer = x.BitPointer,
                BitCount = x.BitCount,
                TagMask = x.TagMask?.Trim() ?? string.Empty,
                MatchAction = x.MatchAction,
                NonMatchAction = x.NonMatchAction
            }).ToList();

            settings.Report.Mode = ReportMode;
            settings.Report.IncludeAntennaPortNumber = IncludeAntennaPortNumber;
            settings.Report.IncludeChannel = IncludeChannel;
            settings.Report.IncludeFirstSeenTime = IncludeFirstSeenTime;
            settings.Report.IncludeLastSeenTime = IncludeLastSeenTime;
            settings.Report.IncludeSeenCount = IncludeSeenCount;
            settings.Report.IncludePeakRssi = IncludePeakRssi;
            settings.Report.IncludePcBits = IncludePcBits;
            settings.Report.IncludeCrc = IncludeCrc;

            settings.AttachedData.Enabled = AttachedDataEnabled;
            settings.AttachedData.MemoryBank = AttachedDataMemoryBank;
            settings.AttachedData.WordPointer = AttachedDataWordPointer;
            settings.AttachedData.WordCount = AttachedDataWordCount;
            settings.AttachedData.AccessPassword = string.IsNullOrWhiteSpace(AttachedDataAccessPassword)
                ? "00000000"
                : AttachedDataAccessPassword.Trim();

            reader.ApplySettings(settings);
            settingsStore.Set(settings);
            OperationResult = "寻卡配置已下发";
            logs.LogOperation("寻卡配置已下发");
            WeakReferenceMessenger.Default.Send(new StatusUpdateRequestedMessage("AttachedDataConfigChanged"));
        }
        catch (Exception ex)
        {
            OperationResult = $"保存失败：{ex.Message}";
            logs.LogOperation($"保存寻卡配置失败：{ex.Message}", Microsoft.Extensions.Logging.LogLevel.Error, ex);
        }
    }

    public void OnConnectionStateChanged(bool isConnected)
    {
        if (!isConnected)
        {
            OperationResult = "请先连接读写器";
            return;
        }

        OperationResult = settingsStore.HasValue ? "可读取缓存参数" : "请先在参数配置页点击获取参数";

        if (settingsStore.TryGetSnapshot(out _)
            && QueryInventoryConfigCommand.CanExecute(null))
        {
            QueryInventoryConfigCommand.Execute(null);
        }
    }
}

public partial class TagSelectFilterItemViewModel : ObservableObject
{
    [ObservableProperty]
    private MemoryBank memoryBank = MemoryBank.Epc;

    [ObservableProperty]
    private ushort bitPointer;

    [ObservableProperty]
    private int bitCount;

    [ObservableProperty]
    private string tagMask = string.Empty;

    [ObservableProperty]
    private StateUnawareAction matchAction = StateUnawareAction.Select;

    [ObservableProperty]
    private StateUnawareAction nonMatchAction = StateUnawareAction.Unselect;
}
