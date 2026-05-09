using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LLRPSdk;
using LLRPReaderUI_Avalonia.Logging;
using LLRPReaderUI_Avalonia.Messages;
using LLRPReaderUI_Avalonia.Models;
using LLRPReaderUI_Avalonia.Services;
using Org.LLRP.LTK.LLRPV1;
using System.Collections.ObjectModel;

namespace LLRPReaderUI_Avalonia.ViewModels;

public partial class InventoryConfigViewModel : ViewModelBase
{
    public InventoryConfigViewModel()
    {
        this.reader = null!;
        this.logs = null!;
        this.settingsStore = null!;
        _languageService = null!;
        reportMode = ReportModes[1];
    }



    private readonly LlrpReader reader;
    private readonly IAppLogService logs;
    private readonly ReaderSettingsStore settingsStore;
    private readonly LanguageService _languageService;

    public InventoryConfigViewModel(LlrpReader reader, IAppLogService logs, ReaderSettingsStore settingsStore, LanguageService languageService)
    {
        this.reader = reader;
        this.logs = logs;
        this.settingsStore = settingsStore;
        _languageService = languageService;
        reportMode = ReportModes[1];
        WeakReferenceMessenger.Default.Register<InventoryConfigViewModel, ConnectionStateChangedMessage>(this, static (r, m) =>
        {
            r.OnConnectionStateChanged(m.Value);
        });
        WeakReferenceMessenger.Default.Register<InventoryConfigViewModel, StatusUpdateRequestedMessage>(this, static (r, m) =>
        {
            r.RefreshStateAwareFlagFromSnapshot();
            r.OnStatusUpdateRequested(m.Value);
        });

        RefreshStateAwareFlagFromSnapshot();

        // Set initial state
        OperationResult = _languageService.GetLocalizedString("InventoryConfig.Waiting");
    }

    private void RefreshStateAwareFlagFromSnapshot()
    {
        if (settingsStore.TryGetSnapshot(out var settings) && settings is not null)
        {
            IsInventoryStateAwareEnabled = settings.InventoryStateAware;
        }
    }

    private void OnStatusUpdateRequested(string reason)
    {
        if (!reason.Contains("AttachedData", StringComparison.OrdinalIgnoreCase)
            && !reason.Contains("Settings", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (settingsStore.TryGetSnapshot(out var settings) && settings is not null)
        {
            ApplySettingsSnapshot(settings);
        }
    }

    public IReadOnlyList<AutoStartMode> AutoStartModes { get; } = Enum.GetValues<AutoStartMode>();
    public IReadOnlyList<AutoStopMode> AutoStopModes { get; } = Enum.GetValues<AutoStopMode>();
    public IReadOnlyList<TagFilterMode> TagFilterModes { get; } = Enum.GetValues<TagFilterMode>();
    public IReadOnlyList<MemoryBank> MemoryBanks { get; } = Enum.GetValues<MemoryBank>();
    public IReadOnlyList<TagFilterOp> TagFilterOps { get; } = Enum.GetValues<TagFilterOp>();
    public IReadOnlyList<StateUnawareAction> StateUnawareActions { get; } = Enum.GetValues<StateUnawareAction>();
    public IReadOnlyList<ENUM_C1G2StateAwareTarget> StateAwareTargets { get; } = Enum.GetValues<ENUM_C1G2StateAwareTarget>();
    public IReadOnlyList<ENUM_C1G2StateAwareAction> StateAwareActions { get; } = Enum.GetValues<ENUM_C1G2StateAwareAction>();
    public IReadOnlyList<ReportModeOptionItem> ReportModes { get; } =
    [
        new(LLRPSdk.ReportMode.WaitForQuery, "WaitForQuery (Trigger=0, N=0)"),
        new(LLRPSdk.ReportMode.Individual, "Individual (Trigger=2, N=1)"),
        new(LLRPSdk.ReportMode.BatchAfterStop, "BatchAfterStop (Trigger=2, N=0)")
    ];

    [ObservableProperty]
    private string operationResult = string.Empty;

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
    private bool showTagFilter1;

    [ObservableProperty]
    private bool showTagFilter2;

    [ObservableProperty]
    private bool showTagSelectFilters;

    [ObservableProperty]
    private bool showStateUnawareTagSelectFilterActions;

    [ObservableProperty]
    private bool showStateAwareTagSelectFilterActions;

    [ObservableProperty]
    private bool isStateAwareTagSelectFiltersEnabled;

    [ObservableProperty]
    private bool isInventoryStateAwareEnabled;

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
    private ReportModeOptionItem reportMode;

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

    /// <summary>
    /// Get a localized string with format arguments.
    /// </summary>
    private string GetLocalizedString(string key, params object[] args)
    {
        var format = _languageService.GetLocalizedString(key);
        return args.Length > 0 ? string.Format(format, args) : format;
    }

    partial void OnFilterModeChanged(TagFilterMode value)
    {
        ShowTagFilter1 = value is TagFilterMode.OnlyFilter1 or TagFilterMode.Filter1AndFilter2 or TagFilterMode.Filter1OrFilter2;
        ShowTagFilter2 = value is TagFilterMode.OnlyFilter2 or TagFilterMode.Filter1AndFilter2 or TagFilterMode.Filter1OrFilter2;
        ShowTagSelectFilters = value is TagFilterMode.UseTagSelectFilters or TagFilterMode.UseStateAwareTagSelectFilters;
        ShowStateUnawareTagSelectFilterActions = value == TagFilterMode.UseTagSelectFilters;
        ShowStateAwareTagSelectFilterActions = value == TagFilterMode.UseStateAwareTagSelectFilters;
        UpdateStateAwareTagSelectFilterAvailability();
    }

    partial void OnIsInventoryStateAwareEnabledChanged(bool value)
    {
        UpdateStateAwareTagSelectFilterAvailability();
    }

    private void UpdateStateAwareTagSelectFilterAvailability()
    {
        IsStateAwareTagSelectFiltersEnabled = FilterMode != TagFilterMode.UseStateAwareTagSelectFilters || IsInventoryStateAwareEnabled;
    }

    [RelayCommand]
    private void AddTagSelectFilter()
    {
        TagSelectFilters.Add(new TagSelectFilterItemViewModel
        {
            MemoryBank = MemoryBank.Epc,
            MatchAction = StateUnawareAction.Select,
            NonMatchAction = StateUnawareAction.Unselect,
            UseStateAwareAction = FilterMode == TagFilterMode.UseStateAwareTagSelectFilters,
            StateAwareTarget = ENUM_C1G2StateAwareTarget.SL,
            StateAwareAction = ENUM_C1G2StateAwareAction.AssertSLOrA_DeassertSLOrB
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
    private void LoadInventoryConfigFromCache()
    {
        try
        {
            if (!settingsStore.TryGetSnapshot(out var settings) || settings is null)
            {
                OperationResult = _languageService.GetLocalizedString("InventoryConfig.CacheEmpty");
                logs.LogOperation(_languageService.GetLocalizedString("InventoryConfig.CacheEmpty"), Microsoft.Extensions.Logging.LogLevel.Warning);
                return;
            }

            ApplySettingsSnapshot(settings);
            OperationResult = _languageService.GetLocalizedString("InventoryConfig.LoadCacheSuccess");
            logs.LogOperation(_languageService.GetLocalizedString("InventoryConfig.LoadCacheSuccess"));
        }
        catch (Exception ex)
        {
            OperationResult = GetLocalizedString("InventoryConfig.LoadCacheFailed", ex.Message);
            logs.LogOperation(GetLocalizedString("InventoryConfig.LoadCacheFailed", ex.Message), Microsoft.Extensions.Logging.LogLevel.Error, ex);
        }
    }

    [RelayCommand]
    private void QueryInventoryConfigFromDevice()
    {
        try
        {
            if (!reader.IsConnected)
            {
                OperationResult = _languageService.GetLocalizedString("InventoryConfig.ConnectReaderFirst");
                logs.LogOperation(_languageService.GetLocalizedString("InventoryConfig.GetDeviceFailedNotConnected"), Microsoft.Extensions.Logging.LogLevel.Warning);
                return;
            }

            Settings settings;
            try
            {
                settings = reader.QuerySettings();
            }
            catch (LLRPSdkException ex) when (
                ex.Message.Contains("has not been configured", StringComparison.OrdinalIgnoreCase)
                || ex.Message.Contains("configuration is invalid", StringComparison.OrdinalIgnoreCase))
            {
                reader.ApplyDefaultSettings();
                settings = reader.QuerySettings();
            }

            settingsStore.Set(settings);
            ApplySettingsSnapshot(settings);
            OperationResult = _languageService.GetLocalizedString("InventoryConfig.GetDeviceSuccess");
            logs.LogOperation(_languageService.GetLocalizedString("InventoryConfig.GetDeviceSuccess"));
            WeakReferenceMessenger.Default.Send(new StatusUpdateRequestedMessage("InventorySettingsLoadedFromDevice"));
        }
        catch (Exception ex)
        {
            OperationResult = GetLocalizedString("InventoryConfig.GetDeviceFailed", ex.Message);
            logs.LogOperation(GetLocalizedString("InventoryConfig.GetDeviceFailed", ex.Message), Microsoft.Extensions.Logging.LogLevel.Error, ex);
        }
    }

    private void ApplySettingsSnapshot(Settings settings)
    {
        IsInventoryStateAwareEnabled = settings.InventoryStateAware;

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
                NonMatchAction = filter.NonMatchAction,
                UseStateAwareAction = filter.UseStateAwareAction,
                StateAwareTarget = filter.StateAwareTarget,
                StateAwareAction = filter.StateAwareAction
            });
        }

        ReportMode = ReportModes.FirstOrDefault(x => x.Value == settings.Report.Mode) ?? ReportModes[1];
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
    }

    [RelayCommand]
    private void SaveInventoryConfig()
    {
        try
        {
            if (!reader.IsConnected)
            {
                OperationResult = _languageService.GetLocalizedString("InventoryConfig.ConnectReaderFirst");
                logs.LogOperation(_languageService.GetLocalizedString("InventoryConfig.SaveFailedNotConnected"), Microsoft.Extensions.Logging.LogLevel.Warning);
                return;
            }

            if (!settingsStore.TryGetSnapshot(out var settings) || settings is null)
            {
                OperationResult = _languageService.GetLocalizedString("InventoryConfig.GetSettingsFirst");
                return;
            }
            settings.InventoryStateAware = IsInventoryStateAwareEnabled;

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
                NonMatchAction = x.NonMatchAction,
                UseStateAwareAction = FilterMode == TagFilterMode.UseStateAwareTagSelectFilters,
                StateAwareTarget = x.StateAwareTarget,
                StateAwareAction = x.StateAwareAction
            }).ToList();

            settings.Report.Mode = ReportMode.Value;
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
            OperationResult = _languageService.GetLocalizedString("InventoryConfig.SaveSuccess");
            logs.LogOperation(_languageService.GetLocalizedString("InventoryConfig.SaveSuccess"));
            WeakReferenceMessenger.Default.Send(new StatusUpdateRequestedMessage("AttachedDataConfigChanged"));
        }
        catch (Exception ex)
        {
            OperationResult = GetLocalizedString("InventoryConfig.SaveError", ex.Message);
            logs.LogOperation(GetLocalizedString("InventoryConfig.SaveError", ex.Message), Microsoft.Extensions.Logging.LogLevel.Error, ex);
        }
    }

    public void OnConnectionStateChanged(bool isConnected)
    {
        if (!isConnected)
        {
            OperationResult = _languageService.GetLocalizedString("InventoryConfig.ConnectReaderFirst");
            return;
        }

        OperationResult = settingsStore.HasValue
            ? _languageService.GetLocalizedString("InventoryConfig.CanReadCache")
            : _languageService.GetLocalizedString("InventoryConfig.GetSettingsFirst");

        if (settingsStore.TryGetSnapshot(out var settings) && settings is not null)
        {
            IsInventoryStateAwareEnabled = settings.InventoryStateAware;
        }

        if (settingsStore.TryGetSnapshot(out _)
            && LoadInventoryConfigFromCacheCommand.CanExecute(null))
        {
            LoadInventoryConfigFromCacheCommand.Execute(null);
        }
    }
}

public partial class TagSelectFilterItemViewModel : ViewModelBase
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

    [ObservableProperty]
    private bool useStateAwareAction;

    [ObservableProperty]
    private ENUM_C1G2StateAwareTarget stateAwareTarget = ENUM_C1G2StateAwareTarget.SL;

    [ObservableProperty]
    private ENUM_C1G2StateAwareAction stateAwareAction = ENUM_C1G2StateAwareAction.AssertSLOrA_DeassertSLOrB;
}

public sealed class ReportModeOptionItem(ReportMode value, string displayText)
{
    public ReportMode Value { get; } = value;

    public string DisplayText { get; } = displayText;
}
