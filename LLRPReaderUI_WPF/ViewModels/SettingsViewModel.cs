using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LLRPSdk;
using LLRPReaderUI_WPF.Logging;
using LLRPReaderUI_WPF.Messages;
using LLRPReaderUI_WPF.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace LLRPReaderUI_WPF.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly LlrpReader reader;
    private readonly IAppLogService logs;
    private readonly ReaderSettingsStore settingsStore;

    public SettingsViewModel(LlrpReader reader, IAppLogService logs, ReaderSettingsStore settingsStore)
    {
        this.reader = reader;
        this.logs = logs;
        this.settingsStore = settingsStore;
        WeakReferenceMessenger.Default.Register<SettingsViewModel, ConnectionStateChangedMessage>(this, static (r, m) =>
        {
            r.OnConnectionStateChanged(m.Value);
        });
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
    private string saveResult = "未保存";

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
    }

    [RelayCommand]
    private void SaveSettings()
    {
        try
        {
            if (!reader.IsConnected)
            {
                SaveResult = "请先在设备连接页连接读写器";
                logs.LogOperation("保存参数失败：设备未连接", Microsoft.Extensions.Logging.LogLevel.Warning);
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
            SaveResult = "参数已下发到设备";
            logs.LogOperation("参数配置已下发到设备");
            WeakReferenceMessenger.Default.Send(new StatusUpdateRequestedMessage("AttachedDataConfigChanged"));
        }
        catch (Exception ex)
        {
            SaveResult = $"保存失败：{ex.Message}";
            logs.LogOperation($"保存参数失败：{ex.Message}", Microsoft.Extensions.Logging.LogLevel.Error, ex);
        }
    }

    [RelayCommand]
    private void QueryDeviceSettings()
    {
        try
        {
            if (!reader.IsConnected)
            {
                SaveResult = "请先在设备连接页连接读写器";
                logs.LogOperation("获取参数失败：设备未连接", Microsoft.Extensions.Logging.LogLevel.Warning);
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
                SaveResult = "设备尚未配置，已自动下发默认参数";
            }

            ApplySettingsToUi(settings);
            UpdateReaderEventNotifications();
            settingsStore.Set(settings);
            SaveResult = "已从设备获取参数";
            logs.LogOperation("已从设备获取参数");
            WeakReferenceMessenger.Default.Send(new StatusUpdateRequestedMessage("AttachedDataConfigChanged"));
        }
        catch (Exception ex)
        {
            SaveResult = $"获取失败：{ex.Message}";
            logs.LogOperation($"获取参数失败：{ex.Message}", Microsoft.Extensions.Logging.LogLevel.Error, ex);
        }
    }

    [RelayCommand]
    private void ResetFactoryDefaults()
    {
        try
        {
            if (!reader.IsConnected)
            {
                SaveResult = "请先在设备连接页连接读写器";
                logs.LogOperation("恢复出厂失败：设备未连接", Microsoft.Extensions.Logging.LogLevel.Warning);
                return;
            }

            reader.ResetToFactoryDefaultsOnly();
            settingsStore.Clear();
            SaveResult = "已恢复设备出厂默认";
            logs.LogOperation("已恢复设备出厂默认");
        }
        catch (Exception ex)
        {
            SaveResult = $"恢复失败：{ex.Message}";
            logs.LogOperation($"恢复出厂失败：{ex.Message}", Microsoft.Extensions.Logging.LogLevel.Error, ex);
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
            SaveResult = "请先在设备连接页连接读写器";
            return;
        }

        if (settingsStore.TryGetSnapshot(out var settings) && settings is not null)
        {
            ApplySettingsToUi(settings);
            UpdateReaderEventNotifications();
            SaveResult = "已加载连接初始化参数";
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
