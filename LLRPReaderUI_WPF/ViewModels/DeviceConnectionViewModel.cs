using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LLRPSdk;
using LLRPReaderUI_WPF.Logging;
using LLRPReaderUI_WPF.Messages;
using LLRPReaderUI_WPF.Models;
using Nager.Country;
using System.IO;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace LLRPReaderUI_WPF.ViewModels;

public partial class DeviceConnectionViewModel : ObservableObject
{
    private const int MaxRecentEndpoints = 3;
    private static readonly string RecentEndpointsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LLRPReaderUI_WPF",
        "recent-endpoints.json");

    private readonly LlrpReader reader;
    private readonly IAppLogService logs;
    private readonly ReaderSettingsStore settingsStore;
    private readonly ReaderStatusStore statusStore;
    private static readonly CountryProvider countryProvider = new();
    private string pendingAddress = string.Empty;
    private string pendingEndpoint = string.Empty;

    public DeviceConnectionViewModel(
        LlrpReader reader,
        IAppLogService logs,
        ReaderSettingsStore settingsStore,
        ReaderStatusStore statusStore)
    {
        this.reader = reader;
        this.logs = logs;
        this.settingsStore = settingsStore;
        this.statusStore = statusStore;

        // Subscribe to keepalive timeout event
        this.reader.KeepaliveTimeout += OnKeepaliveTimeout;

        LoadRecentEndpoints();
    }

    private void OnKeepaliveTimeout(LlrpReader _)
    {
        // Keepalive timeout - reader stopped responding
        // Use ForceDisconnect for fast close without waiting for CLOSE_CONNECTION response
        Application.Current.Dispatcher.Invoke(() =>
        {
            ConnectionState = "心跳超时，正在断开连接...";
            IsBusy = true;
            logs.LogOperation("心跳超时，阅读器无响应", Microsoft.Extensions.Logging.LogLevel.Warning);

            try
            {
                reader.ForceDisconnect();
            }
            catch { }

            settingsStore.Clear();
            statusStore.Clear();
            IsConnected = false;
            ConnectionState = "已断开（心跳超时）";
            WeakReferenceMessenger.Default.Send(new ConnectionStateChangedMessage(false));
            IsBusy = false;
        });
    }

    [ObservableProperty]
    private string readerEndpoint = "192.168.40.233";

    [ObservableProperty]
    private bool isConnected;

    [ObservableProperty]
    private string connectionState = "未连接";

    [ObservableProperty]
    private bool isBusy;

    public ObservableCollection<string> RecentReaderEndpoints { get; } = new();

    partial void OnIsConnectedChanged(bool value)
    {
        WeakReferenceMessenger.Default.Send(new ConnectionStateChangedMessage(value));

        // Refresh command CanExecute when connection state changes
        try
        {
            ConnectCommand?.NotifyCanExecuteChanged();
        }
        catch { }

        try
        {
            DisconnectCommand?.NotifyCanExecuteChanged();
        }
        catch { }
    }

    partial void OnIsBusyChanged(bool value)
    {
        WeakReferenceMessenger.Default.Send(new BusyStateChangedMessage(value, ConnectionState));
    }

    partial void OnConnectionStateChanged(string value)
    {
        // Connection state changed, but we rely on OnIsBusyChanged for the overlay
    }

    public FeatureItemCollection FeatureSetItems { get; } =
    [
        new("ModelNumber", "-"),
        new("ReaderModel", "-"),
        new("DeviceManufacturerNumber", "-"),
        new("FirmwareVersion", "-"),
        new("AntennaCount", "-"),
        new("GpiCount", "-"),
        new("GpoCount", "-"),
        new("MaxOperationSequences", "-"),
        new("MaxOperationsPerSequence", "-"),
        new("CountryCode","-"),
        new("CommunicationsStandard", "-"),
        new("IsTagAccessAvailable", "-"),
        new("IsFilteringAvailable", "-"),
        new("MaxTagSelectFiltersAllowed", "-"),
      
        new("IsMultiwordBlockWriteAvailable", "-"),
        new("IsMultiwordBlockEraseAvailable", "-"),
       

        //new("ReaderMaxSensitivityActualDbm", "-"),
        new("IsHoppingRegion", "-"),
        new("TxPowers", "-"),
        new("RxSensitivities", "-"),
        new("TxFrequencies", "-"),
        new("RfModes", "-")
    ];

    [RelayCommand(CanExecute = nameof(CanConnect))]
    private void Connect()
    {
        try
        {
            ConnectionState = "正在连接设备...";
            IsBusy = true;
            var endpoint = ReaderEndpoint.Trim();
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new LLRPSdkException("请先输入读写器地址。示例：192.168.1.10 或 192.168.1.10:5084");
            }

            var (address, port) = ParseEndpoint(endpoint);

            if (reader.IsConnected)
            {
                reader.Disconnect();
            }

            pendingAddress = address;
            pendingEndpoint = endpoint;
            logs.LogOperation($"发起连接：{endpoint}");
            reader.ConnectAsyncComplete -= OnConnectAsyncComplete;
            reader.ConnectAsyncComplete += OnConnectAsyncComplete;

            if (port.HasValue)
            {
                reader.ConnectAsync(address, port.Value, false);
            }
            else
            {
                reader.ConnectAsync(address);
            }
            ConnectionState = $"连接中：{address}";
        }
        catch (Exception ex)
        {
            reader.ConnectAsyncComplete -= OnConnectAsyncComplete;
            IsConnected = false;
            IsBusy = false;
            ConnectionState = $"连接失败：{ex.Message}";
            logs.LogOperation($"连接失败：{ex.Message}", Microsoft.Extensions.Logging.LogLevel.Error, ex);
        }
    }

    private bool CanConnect()
    {
        return !IsConnected;
    }

    private void OnConnectAsyncComplete(LlrpReader _, ConnectAsyncResult result, string errorMessage)
    {
        reader.ConnectAsyncComplete -= OnConnectAsyncComplete;

        if (result == ConnectAsyncResult.Success && reader.IsConnected)
        {
            // Run initialization on background thread
            Task.Run(() =>
            {
                try
                {
                    bool wasSingulating = EnsureStoppedIfSingulating();
                    var settings = QueryInitialSettings();
                    var featureSet = reader.ReaderCapabilities;

                    // Update UI on dispatcher
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        settingsStore.Set(settings);
                        UpdateFeatureSetItems(featureSet);
                        AddRecentEndpoint(pendingEndpoint);
                        IsConnected = true;
                        ConnectionState = wasSingulating
                            ? $"已连接：{pendingAddress}（检测到设备盘点中，已自动停止）"
                            : $"已连接：{pendingAddress}";
                        IsBusy = false; // This will hide the overlay
                        logs.LogOperation(ConnectionState);
                    });
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        IsConnected = false;
                        ConnectionState = $"连接失败：{ex.Message}";
                        IsBusy = false; // This will hide the overlay
                        logs.LogOperation($"连接后初始化失败：{ex.Message}", Microsoft.Extensions.Logging.LogLevel.Error, ex);
                    });
                }
            });
            return;
        }

        // Failed case
        Application.Current.Dispatcher.Invoke(() =>
        {
            IsConnected = false;
            ConnectionState = $"连接失败：{errorMessage}";
            IsBusy = false; // This will hide the overlay
            logs.LogOperation($"连接失败：{errorMessage}", Microsoft.Extensions.Logging.LogLevel.Warning);
        });
    }

    private bool EnsureStoppedIfSingulating()
    {
        bool isSingulating = reader.QuerySingulatingState();
        if (isSingulating)
        {
            reader.Stop();
        }

        return isSingulating;
    }

    private Settings QueryInitialSettings()
    {
        try
        {
            return reader.QuerySettings();
        }
        catch (LLRPSdkException ex) when (
            ex.Message.Contains("has not been configured", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("configuration is invalid", StringComparison.OrdinalIgnoreCase))
        {
            reader.ApplyDefaultSettings();
            return reader.QuerySettings();
        }
    }

    [RelayCommand(CanExecute = nameof(CanDisconnect))]
    private async void Disconnect()
    {
        ConnectionState = "正在断开连接...";
        IsBusy = true;

        try
        {
            reader.ConnectAsyncComplete -= OnConnectAsyncComplete;
            if (reader.IsConnected)
            {
                // Run disconnect on background thread to avoid UI freeze
                await Task.Run(() =>
                {
                    try
                    {
                        reader.Disconnect();
                    }
                    catch
                    {
                        // Ignore errors during disconnect (connection may already be lost)
                    }
                });
            }
        }
        finally
        {
            settingsStore.Clear();
            statusStore.Clear();
            IsConnected = false;
            ConnectionState = "未连接";
            IsBusy = false; // This will hide the overlay
            logs.LogOperation("设备断开连接");
        }
    }

    private bool CanDisconnect()
    {
        return IsConnected;
    }

    private static (string Address, int? Port) ParseEndpoint(string endpoint)
    {
        var value = endpoint.Trim();
        var separatorIndex = value.LastIndexOf(':');

        if (separatorIndex > 0
            && separatorIndex == value.IndexOf(':')
            && int.TryParse(value[(separatorIndex + 1)..], out var port))
        {
            var address = value[..separatorIndex].Trim();
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new LLRPSdkException("地址不能为空。示例：192.168.1.10:5084");
            }

            return (address, port);
        }

        return (value, null);
    }

    private void UpdateFeatureSetItems(FeatureSet featureSet)
    {
        FeatureSetItems["ModelNumber"] = featureSet.ModelNumber.ToString();
        FeatureSetItems["ReaderModel"] = featureSet.ReaderModel.ToString();
        FeatureSetItems["DeviceManufacturerNumber"] = FormatManufacturerNumber(featureSet.DeviceManufacturerNumber);
        FeatureSetItems["FirmwareVersion"] = featureSet.FirmwareVersion ?? "-";
        FeatureSetItems["AntennaCount"] = featureSet.AntennaCount.ToString();
        FeatureSetItems["GpiCount"] = featureSet.GpiCount.ToString();
        FeatureSetItems["GpoCount"] = featureSet.GpoCount.ToString();
        FeatureSetItems["MaxOperationSequences"] = featureSet.MaxOperationSequences.ToString();
        FeatureSetItems["MaxOperationsPerSequence"] = featureSet.MaxOperationsPerSequence.ToString();
        FeatureSetItems["CommunicationsStandard"] = featureSet.CommunicationsStandard.ToString();
        FeatureSetItems["CountryCode"] = FormatCountryCode(featureSet.CountryCode);
        FeatureSetItems["IsTagAccessAvailable"] = featureSet.IsTagAccessAvailable.ToString();
        FeatureSetItems["IsFilteringAvailable"] = featureSet.IsFilteringAvailable.ToString();
        FeatureSetItems["MaxTagSelectFiltersAllowed"] = featureSet.MaxTagSelectFiltersAllowed.ToString();
       
        FeatureSetItems["IsMultiwordBlockWriteAvailable"] = featureSet.IsMultiwordBlockWriteAvailable.ToString();
        FeatureSetItems["IsMultiwordBlockEraseAvailable"] = featureSet.IsMultiwordBlockEraseAvailable.ToString();
 
        //FeatureSetItems["ReaderMaxSensitivityActualDbm"] = featureSet.ReaderMaxSensitivityActualDbm.ToString();
        FeatureSetItems["IsHoppingRegion"] = featureSet.IsHoppingRegion.ToString();
        FeatureSetItems["TxPowers"] = featureSet.TxPowers is { Count: > 0 } ? $"Count={featureSet.TxPowers.Count}" : "Count=0";
        FeatureSetItems["RxSensitivities"] = featureSet.RxSensitivities is { Count: > 0 } ? $"Count={featureSet.RxSensitivities.Count}" : "Count=0";
        FeatureSetItems["TxFrequencies"] = featureSet.TxFrequencies is { Count: > 0 } ? $"Count={featureSet.TxFrequencies.Count}" : "Count=0";
        FeatureSetItems["RfModes"] = featureSet.RfModes is { Count: > 0 } ? $"Count={featureSet.RfModes.Count}" : "Count=0";
    }

    private static string FormatManufacturerNumber(uint manufacturerNumber)
    {
        if (manufacturerNumber <= int.MaxValue)
        {
            var enumValue = (ManufacturerNumber)(int)manufacturerNumber;
            if (Enum.IsDefined(enumValue))
            {
                return $"{manufacturerNumber}(\"{enumValue}\")";
            }
        }

        return manufacturerNumber.ToString();
    }

    private static string FormatCountryCode(ushort countryCode)
    {
        var info = countryProvider.GetCountries().FirstOrDefault(x => x.NumericCode == countryCode);
        return info is null ? countryCode.ToString() : $"{countryCode}({info.CommonName})";
    }

    private void LoadRecentEndpoints()
    {
        try
        {
            if (!File.Exists(RecentEndpointsFilePath))
            {
                return;
            }

            var json = File.ReadAllText(RecentEndpointsFilePath);
            var items = JsonSerializer.Deserialize<List<string>>(json) ?? [];

            foreach (var endpoint in items
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(MaxRecentEndpoints))
            {
                RecentReaderEndpoints.Add(endpoint);
            }

            if (RecentReaderEndpoints.Count > 0)
            {
                ReaderEndpoint = RecentReaderEndpoints[0];
            }
        }
        catch
        {
        }
    }

    private void AddRecentEndpoint(string endpoint)
    {
        var value = endpoint.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        var existingIndex = RecentReaderEndpoints
            .Select((item, index) => new { item, index })
            .FirstOrDefault(x => string.Equals(x.item, value, StringComparison.OrdinalIgnoreCase))
            ?.index;

        if (existingIndex.HasValue)
        {
            RecentReaderEndpoints.RemoveAt(existingIndex.Value);
        }

        RecentReaderEndpoints.Insert(0, value);
        while (RecentReaderEndpoints.Count > MaxRecentEndpoints)
        {
            RecentReaderEndpoints.RemoveAt(RecentReaderEndpoints.Count - 1);
        }

        ReaderEndpoint = value;
        SaveRecentEndpoints();
    }

    private void SaveRecentEndpoints()
    {
        try
        {
            var dir = Path.GetDirectoryName(RecentEndpointsFilePath);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var json = JsonSerializer.Serialize(RecentReaderEndpoints.ToList());
            File.WriteAllText(RecentEndpointsFilePath, json);
        }
        catch
        {
        }
    }
}

public sealed class FeatureItemCollection : ObservableCollection<FeatureItemViewModel>
{
    public string this[string name]
    {
        get
        {
            var item = this.FirstOrDefault(x => x.Name == name)
                ?? throw new KeyNotFoundException($"Feature item not found: {name}");
            return item.Value;
        }
        set
        {
            var item = this.FirstOrDefault(x => x.Name == name)
                ?? throw new KeyNotFoundException($"Feature item not found: {name}");
            item.Value = value;
        }
    }
}

public partial class FeatureItemViewModel : ObservableObject
{
    public FeatureItemViewModel(string name, string value)
    {
        Name = name;
        this.value = value;
    }

    public string Name { get; }

    [ObservableProperty]
    private string value;
}
