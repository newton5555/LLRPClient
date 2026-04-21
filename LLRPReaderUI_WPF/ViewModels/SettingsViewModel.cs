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

namespace LLRPReaderUI_WPF.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private static readonly Dictionary<uint, string> RfModePrefixes = new()
    {
        { 113, "P0" },
        { 45, "P1" },
        { 203, "P2" },
        { 107, "P3" },
        { 220, "P4" },
        { 101, "P8" },
        { 111, "P9" },
        { 4185, "P10" },
        { 4146, "P11" },
        { 4148, "P12" },
        { 4124, "P13" },
        { 5185, "P18" },
        { 5146, "P19" },
        { 5148, "P20" },
        { 5124, "P21" }
    };

    private readonly LlrpReader reader;
    private readonly IAppLogService logs;
    private readonly ReaderSettingsStore settingsStore;
    private readonly LanguageService _languageService;

    public SettingsViewModel(LlrpReader reader, IAppLogService logs, ReaderSettingsStore settingsStore, LanguageService languageService)
    {
        this.reader = reader;
        this.logs = logs;
        this.settingsStore = settingsStore;
        _languageService = languageService;
        WeakReferenceMessenger.Default.Register<SettingsViewModel, ConnectionStateChangedMessage>(this, static (r, m) =>
        {
            r.OnConnectionStateChanged(m.Value);
        });

        // Set initial state
        SaveResult = _languageService.GetLocalizedString("Settings.NotSaved");
    }

    /// <summary>
    /// Get a localized string with format arguments.
    /// </summary>
    private string GetLocalizedString(string key, params object[] args)
    {
        var format = _languageService.GetLocalizedString(key);
        return args.Length > 0 ? string.Format(format, args) : format;
    }

    [ObservableProperty]
    private bool enableKeepalive = true;

    [ObservableProperty]
    private int keepaliveIntervalMs = 5000;

    [ObservableProperty]
    private int session = 2;

    [ObservableProperty]
    private int tagPopulationEstimate = 32;

    [ObservableProperty]
    private bool holdEventsAndReportsUponReconnect;

    [ObservableProperty]
    private ushort hopTableId = 1;

    [ObservableProperty]
    private ushort channelIndex = 0;

    [ObservableProperty]
    private ObservableCollection<ushort> hopTableOptions = new();

    [ObservableProperty]
    private string selectedHopTableFrequencies = string.Empty;

    [ObservableProperty]
    private string selectedChannelFrequency = string.Empty;

    [ObservableProperty]
    private uint? selectedRfMode;

    [ObservableProperty]
    private RfModeOptionItem? selectedRfModeOption;

    [ObservableProperty]
    private ObservableCollection<RfModeOptionItem> rfModeOptions = new();

    [ObservableProperty]
    private ObservableCollection<double> txPowerOptions = new();

    [ObservableProperty]
    private ObservableCollection<double> rxSensitivityOptions = new();

    [ObservableProperty]
    private ObservableCollection<string> readerEventNotifications = new();

    [ObservableProperty]
    private string saveResult = string.Empty;

    [ObservableProperty]
    private bool isStateAwareSupported;

    [ObservableProperty]
    private bool inventoryStateAware;

    [ObservableProperty]
    private InventoryTarget inventoryTarget = InventoryTarget.A;

    public List<InventoryTarget> InventoryTargetOptions { get; } = new() { InventoryTarget.A, InventoryTarget.B };

    [ObservableProperty]
    private InventorySearchMode inventorySearchMode = InventorySearchMode.Not_SL;

    public List<InventorySearchMode> InventorySearchModeOptions { get; } = new() { InventorySearchMode.SL, InventorySearchMode.Not_SL };

    [ObservableProperty]
    private ObservableCollection<AntennaItemViewModel> antennas = new();

    private void UpdateReaderEventNotifications()
    {
        ReaderEventNotifications.Clear();

        var eventStates = reader.QueryReaderEventNotifications();
        if (eventStates.Count == 0)
        {
            ReaderEventNotifications.Add("ReaderEventNotificationSpec: (empty)");
            return;
        }

        foreach (var eventState in eventStates)
        {
            ReaderEventNotifications.Add($"{eventState.EventType}: {(eventState.IsEnabled ? "Enabled" : "Disabled")}");
        }
    }

    private void RefreshFeatureOptions()
    {
        if (!reader.IsConnected)
        {
            return;
        }

        var featureSet = reader.ReaderCapabilities;
        IsStateAwareSupported = featureSet.CanDoTagInventoryStateAwareSingulation;

        var rfModes = featureSet.RfModes?
            .Where(x => x.HasValue)
            .Select(x => x.GetValueOrDefault())
            .Distinct()
            .OrderBy(x => x)
            .ToList() ?? [];

        RfModeOptions.Clear();
        foreach (var rfMode in rfModes)
        {
            var detail = featureSet.RfModeDetails.TryGetValue(rfMode, out var text) ? text : string.Empty;
            var display = string.IsNullOrWhiteSpace(detail) ? rfMode.ToString() : $"{rfMode} - {detail}";
            
            if (RfModePrefixes.TryGetValue(rfMode, out var prefix))
            {
                display = $"{prefix} - {display}";
            }

            RfModeOptions.Add(new RfModeOptionItem(rfMode, display));
        }

        var txPowers = featureSet.TxPowers?
            .Select(x => x.Dbm)
            .Distinct()
            .OrderBy(x => x)
            .ToList() ?? [];

        TxPowerOptions.Clear();
        foreach (var txPower in txPowers)
        {
            TxPowerOptions.Add(txPower);
        }

        var rxSensitivities = featureSet.RxSensitivities?
            .Select(x => x.Dbm)
            .Distinct()
            .OrderBy(x => x)
            .ToList() ?? [];

        RxSensitivityOptions.Clear();
        foreach (var rxSensitivity in rxSensitivities)
        {
            RxSensitivityOptions.Add(rxSensitivity);
        }

        HopTableOptions.Clear();
        HopTableOptions.Add(0); // Always include ID=0
        if (featureSet.HopTables != null)
        {
            foreach (var table in featureSet.HopTables)
            {
                if (table.HopTableId != 0) // Avoid duplicate 0
                    HopTableOptions.Add(table.HopTableId);
            }
        }
        UpdateSelectedHopTableFrequencies();
        UpdateSelectedChannelFrequency();
    }

    partial void OnHopTableIdChanged(ushort value)
    {
        UpdateSelectedHopTableFrequencies();
        UpdateSelectedChannelFrequency();
    }

    partial void OnChannelIndexChanged(ushort value)
    {
        UpdateSelectedChannelFrequency();
    }

    private void UpdateSelectedChannelFrequency()
    {
        if (reader.IsConnected)
        {
            if (ChannelIndex == 0)
            {
                SelectedChannelFrequency = "-";
                return;
            }

            var table = reader.ReaderCapabilities.HopTables?.FirstOrDefault(x => x.HopTableId == HopTableId);
            List<double> freqs = null;
            if (table != null)
            {
                freqs = table.Frequencies;
            }
            else if (HopTableId == 0 && !reader.ReaderCapabilities.IsHoppingRegion)
            {
                freqs = reader.ReaderCapabilities.TxFrequencies;
            }

            if (freqs != null && ChannelIndex > 0 && ChannelIndex <= freqs.Count)
            {
                SelectedChannelFrequency = $"{freqs[ChannelIndex - 1]:F1} MHz";
            }
            else
            {
                SelectedChannelFrequency = "-";
            }
        }
    }

    private void UpdateSelectedHopTableFrequencies()
    {
        if (reader.IsConnected)
        {
            var table = reader.ReaderCapabilities.HopTables?.FirstOrDefault(x => x.HopTableId == HopTableId);
            if (table != null)
            {
                SelectedHopTableFrequencies = string.Join(", ", table.Frequencies.Select(f => f.ToString("F3")));
            }
            else if (HopTableId == 0)
            {
                // If ID=0, maybe show FixedFrequencyTable if we saved it elsewhere, 
                // but for now let's just show TxFrequencies if IsHoppingRegion is false
                if (!reader.ReaderCapabilities.IsHoppingRegion)
                {
                    SelectedHopTableFrequencies = string.Join(", ", reader.ReaderCapabilities.TxFrequencies.Select(f => f.ToString("F3")));
                }
                else
                {
                    SelectedHopTableFrequencies = "-";
                }
            }
            else
            {
                SelectedHopTableFrequencies = "-";
            }
        }
    }

    [RelayCommand]
    private void SaveSettings()
    {
        try
        {
            if (!reader.IsConnected)
            {
                SaveResult = _languageService.GetLocalizedString("Settings.ConnectDeviceFirst");
                logs.LogOperation(_languageService.GetLocalizedString("Settings.SaveFailedNotConnected"), Microsoft.Extensions.Logging.LogLevel.Warning);
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
                settings = reader.QueryDefaultSettings();
            }

            settings.Keepalives.Enabled = EnableKeepalive;
            settings.Keepalives.PeriodInMs = (uint)Math.Max(1, KeepaliveIntervalMs);
            settings.Session = (ushort)Math.Clamp(Session, 0, 3);
            settings.TagPopulationEstimate = (ushort)Math.Clamp(TagPopulationEstimate, 1, ushort.MaxValue);
            settings.HoldReportsOnDisconnect = HoldEventsAndReportsUponReconnect;
            settings.RfMode = SelectedRfModeOption?.Id;
            settings.HopTableId = HopTableId;
            settings.ChannelIndex = ChannelIndex;
            settings.InventoryStateAware = InventoryStateAware;
            settings.InventoryTarget = InventoryTarget;
            settings.InventorySearchMode = InventorySearchMode;

            var configuredAntennas = settings.Antennas.AntennaConfigs;
            if (Antennas.Count > 0)
            {
                foreach (var uiAntenna in Antennas)
                {
                    var targetAntenna = configuredAntennas.FirstOrDefault(x => x.PortNumber == uiAntenna.PortNumber);
                    //if(targetAntenna is null) { continue; }
                    if (targetAntenna is null)
                    {
                        targetAntenna = new AntennaConfig(uiAntenna.PortNumber)
                        {
                            PortName = string.IsNullOrWhiteSpace(uiAntenna.PortName)
                                ? $"Antenna Port {uiAntenna.PortNumber}"
                                : uiAntenna.PortName
                        };
                        settings.Antennas.Add(targetAntenna);
                    }
                    targetAntenna.IsEnabled = uiAntenna.IsEnabled;
                    targetAntenna.MaxTxPower = false;
                    targetAntenna.TxPowerInDbm = uiAntenna.TxPowerInDbm;
                    targetAntenna.MaxRxSensitivity = false;
                    targetAntenna.RxSensitivityInDbm = uiAntenna.RxSensitivityInDbm;
                }
            }

            reader.ApplySettings(settings);
            settingsStore.Set(settings);
            SaveResult = _languageService.GetLocalizedString("Settings.SavedToDevice");
            logs.LogOperation(_languageService.GetLocalizedString("Settings.SavedLog"));
            WeakReferenceMessenger.Default.Send(new StatusUpdateRequestedMessage("AttachedDataConfigChanged"));
        }
        catch (Exception ex)
        {
            SaveResult = GetLocalizedString("Settings.SaveFailedMsg", ex.Message);
            logs.LogOperation(GetLocalizedString("Settings.SaveFailedMsg", ex.Message), Microsoft.Extensions.Logging.LogLevel.Error, ex);
        }
    }

    [RelayCommand]
    private void QueryDeviceSettings()
    {
        try
        {
            if (!reader.IsConnected)
            {
                SaveResult = _languageService.GetLocalizedString("Settings.ConnectDeviceFirst");
                logs.LogOperation(_languageService.GetLocalizedString("Settings.GetFailedNotConnected"), Microsoft.Extensions.Logging.LogLevel.Warning);
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
                //settings=reader.QueryDefaultSettings();//测试使用 假的数据
                SaveResult = _languageService.GetLocalizedString("Settings.DefaultApplied");
            }

            ApplySettingsToUi(settings);
            UpdateReaderEventNotifications();
            settingsStore.Set(settings);
            SaveResult = _languageService.GetLocalizedString("Settings.GotFromDevice");
            logs.LogOperation(_languageService.GetLocalizedString("Settings.GotLog"));
            WeakReferenceMessenger.Default.Send(new StatusUpdateRequestedMessage("AttachedDataConfigChanged"));
        }
        catch (Exception ex)
        {
            SaveResult = GetLocalizedString("Settings.GetFailed", ex.Message);
            logs.LogOperation(GetLocalizedString("Settings.GetFailed", ex.Message), Microsoft.Extensions.Logging.LogLevel.Error, ex);
        }
    }

    [RelayCommand]
    private void ResetFactoryDefaults()
    {
        try
        {
            if (!reader.IsConnected)
            {
                SaveResult = _languageService.GetLocalizedString("Settings.ConnectDeviceFirst");
                logs.LogOperation(_languageService.GetLocalizedString("Settings.ResetFailedNotConnected"), Microsoft.Extensions.Logging.LogLevel.Warning);
                return;
            }

            reader.ResetToFactoryDefaultsOnly();
            settingsStore.Clear();
            SaveResult = _languageService.GetLocalizedString("Settings.ResetSuccess");
            logs.LogOperation(_languageService.GetLocalizedString("Settings.ResetLog"));
        }
        catch (Exception ex)
        {
            SaveResult = GetLocalizedString("Settings.ResetFailed", ex.Message);
            logs.LogOperation(GetLocalizedString("Settings.ResetFailed", ex.Message), Microsoft.Extensions.Logging.LogLevel.Error, ex);
        }
    }

    public void OnConnectionStateChanged(bool isConnected)
    {
        if (!isConnected)
        {
            RfModeOptions.Clear();
            TxPowerOptions.Clear();
            RxSensitivityOptions.Clear();
            Antennas.Clear();
            ReaderEventNotifications.Clear();
            settingsStore.Clear();
            SelectedRfMode = null;
            SelectedRfModeOption = null;
            SaveResult = _languageService.GetLocalizedString("Settings.ConnectDeviceFirst");
            return;
        }

        if (settingsStore.TryGetSnapshot(out var settings) && settings is not null)
        {
            ApplySettingsToUi(settings);
            UpdateReaderEventNotifications();
            SaveResult = _languageService.GetLocalizedString("Settings.LoadedInitParams");
            return;
        }

        if (QueryDeviceSettingsCommand.CanExecute(null))
        {
            QueryDeviceSettingsCommand.Execute(null);
        }
    }

    private void ApplySettingsToUi(Settings settings)
    {
        RefreshFeatureOptions();

        EnableKeepalive = settings.Keepalives.Enabled;
        KeepaliveIntervalMs = (int)settings.Keepalives.PeriodInMs;
        SelectedRfMode = settings.RfMode;
        SelectedRfModeOption = RfModeOptions.FirstOrDefault(x => x.Id == settings.RfMode);
        TagPopulationEstimate = settings.TagPopulationEstimate;
        HoldEventsAndReportsUponReconnect = settings.HoldReportsOnDisconnect;
        HopTableId = settings.HopTableId;
        ChannelIndex = settings.ChannelIndex;
        InventoryStateAware = settings.InventoryStateAware;
        InventoryTarget = settings.InventoryTarget;
        InventorySearchMode = settings.InventorySearchMode;

        Antennas.Clear();
        var configuredByPort = settings.Antennas.AntennaConfigs
            .GroupBy(x => x.PortNumber)
            .ToDictionary(x => x.Key, x => x.First());
        var antennaCount = (int)reader.ReaderCapabilities.AntennaCount;
        var defaultTxPower = TxPowerOptions.Count > 0 ? TxPowerOptions.Max(x => x) : 0d;
        var defaultRxSensitivity = RxSensitivityOptions.Count > 0 ? RxSensitivityOptions[0] : 0d;

        for (var port = 1; port <= antennaCount; port++)
        {
            var portNumber = (ushort)port;
            configuredByPort.TryGetValue(portNumber, out var antenna);

            Antennas.Add(new AntennaItemViewModel
            {
                PortNumber = portNumber,
                PortName = antenna?.PortName ?? $"Antenna Port {portNumber}",
                IsEnabled = antenna?.IsEnabled ?? false,
                TxPowerInDbm = antenna?.TxPowerInDbm ?? defaultTxPower,
                RxSensitivityInDbm = antenna?.RxSensitivityInDbm ?? defaultRxSensitivity
            });
        }

        Session = settings.Session;
    }

}

public sealed class RfModeOptionItem
{
    public RfModeOptionItem(uint id, string displayText)
    {
        Id = id;
        DisplayText = displayText;
    }

    public uint Id { get; }

    public string DisplayText { get; }
}

public partial class AntennaItemViewModel : ObservableObject
{
    [ObservableProperty]
    private ushort portNumber;

    [ObservableProperty]
    private string portName = string.Empty;

    [ObservableProperty]
    private bool isEnabled;

    [ObservableProperty]
    private double txPowerInDbm;

    [ObservableProperty]
    private double rxSensitivityInDbm;
}
