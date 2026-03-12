using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LLRPSdk;
using LLRPReaderUI_Avalonia.Messages;
using LLRPReaderUI_Avalonia.Models;
using Material.Icons;
using System.Linq;

namespace LLRPReaderUI_Avalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly LlrpReader reader;



    [ObservableProperty]
    private NavigationItem? selectedNavigationItem;

    [ObservableProperty]
    private object? currentPageViewModel;

    [ObservableProperty]
    private string statusText = "Ready";

    [ObservableProperty]
    private string deviceStatusText = "设备: 未连接";

    [ObservableProperty]
    private string inventoryStatusText = "盘点: 未知";

    //[ObservableProperty]
    //private string temperatureStatusText = "温度: --°C";

    [ObservableProperty]
    private string antennaStatusText = "天线: --";

    [ObservableProperty]
    private string gpiStatusText = "GPI: --";

    [ObservableProperty]
    private string gpoStatusText = "GPO: --";

    [ObservableProperty]
    private string identificationStatusText = "MAC: --";

    [ObservableProperty]
    private string windowTitle = BuildWindowTitle();

    public ObservableCollection<NavigationItem> NavigationItems { get; } = new();

    public MainViewModel(
        LlrpReader reader,
        DeviceConnectionViewModel deviceConnectionViewModel,
        SettingsViewModel settingsViewModel,
        GpioViewModel gpioViewModel,
        InventoryConfigViewModel inventoryConfigViewModel,
        InventoryViewModel inventoryViewModel,
        ReadWriteViewModel readWriteViewModel,
        LogViewModel logViewModel)
    {
        this.reader = reader;

        NavigationItems =
        [
            new NavigationItem { Title = "设备连接", IconKind = MaterialIconKind.LanConnect, ViewModel = deviceConnectionViewModel },
            new NavigationItem { Title = "参数配置", IconKind = MaterialIconKind.Cog, ViewModel = settingsViewModel },
            new NavigationItem { Title = "GPIO 配置", IconKind = MaterialIconKind.PowerPlug, ViewModel = gpioViewModel },
            new NavigationItem { Title = "寻卡配置", IconKind = MaterialIconKind.Tune, ViewModel = inventoryConfigViewModel },
            new NavigationItem { Title = "盘点操作", IconKind = MaterialIconKind.Radar, ViewModel = inventoryViewModel },
            new NavigationItem { Title = "读写操作", IconKind = MaterialIconKind.PencilBoxOutline, ViewModel = readWriteViewModel },
            new NavigationItem { Title = "日志", IconKind = MaterialIconKind.TextBoxSearchOutline, ViewModel = logViewModel }
        ];

        WeakReferenceMessenger.Default.Register<MainViewModel, ConnectionStateChangedMessage>(this, static (r, m) =>
        {
            r.OnDeviceConnectionStateChanged(m.Value);
        });
        WeakReferenceMessenger.Default.Register<MainViewModel, StatusUpdateRequestedMessage>(this, static (r, m) =>
        {
            r.QueryStatus();
        });

        SelectedNavigationItem = NavigationItems[0];
        CurrentPageViewModel = SelectedNavigationItem.ViewModel;
    }

    private void OnDeviceConnectionStateChanged(bool isConnected)
    {
        //if (isConnected)
        {
            QueryStatus();
        }
    }

    [RelayCommand]
    private void QueryStatus()
    {
        if (!reader.IsConnected)
        {
            DeviceStatusText = "设备: 未连接";
            InventoryStatusText = "盘点: 未知";
            //TemperatureStatusText = "温度: --°C";
            AntennaStatusText = "天线: --";
            GpiStatusText = "GPI: --";
            GpoStatusText = "GPO: --";
            IdentificationStatusText = "MAC: --";
            return;
        }

        var status = reader.QueryStatus();

        DeviceStatusText = $"设备: {(status.IsConnected ? "已连接" : "未连接")}";
        InventoryStatusText = $"盘点: {(status.IsSingulating ? "进行中" : "空闲")}";
        //TemperatureStatusText = $"温度: {status.TemperatureInCelsius}°C";
        IdentificationStatusText = $"MAC: {FormatIdentification(status.ReaderIdentity)}";

        var antennaParts = status.Antennas
            .Cast<AntennaStatus>()
            .OrderBy(x => x.PortNumber)
            .Select(x => $"{x.PortNumber}:{(x.IsConnected ? "连" : "断")}")
            .ToList();
        //AntennaStatusText = antennaParts.Count > 0
        //    ? $"天线: {string.Join(" ", antennaParts)}"
        //    : "天线: 无数据";

        var gpiParts = status.Gpis
            .Cast<GpiStatus>()
            .OrderBy(x => x.PortNumber)
            .Select(x => $"{x.PortNumber}:{(x.State ? "高" : "低")}")
            .ToList();
        GpiStatusText = gpiParts.Count > 0
            ? $"GPI: {string.Join(" ", gpiParts)}"
            : "GPI: 无数据";

        var gpoParts = status.GpoStates
            .Cast<GpoStatus>()
            .OrderBy(x => x.PortNumber)
            .Select(x => $"{x.PortNumber}:{(x.State ? "高" : "低")}")
            .ToList();
        GpoStatusText = gpoParts.Count > 0
            ? $"GPO: {string.Join(" ", gpoParts)}"
            : "GPO: 当前设备未返回";
    }

    private static string FormatIdentification(object? readerIdentity)
    {
        if (readerIdentity is null)
        {
            return "--";
        }

        var raw = readerIdentity.ToString();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "--";
        }

        var normalized = raw.Replace(":", string.Empty).Replace("-", string.Empty).Trim();
        if (normalized.Length == 12)
        {
            return string.Join(":", Enumerable.Range(0, 6).Select(i => normalized.Substring(i * 2, 2))).ToUpperInvariant();
        }

        return raw;
    }

    partial void OnSelectedNavigationItemChanged(NavigationItem? value)
    {
        if (value is null)
        {
            return;
        }

        CurrentPageViewModel = value.ViewModel;
        StatusText = $"当前页面：{value.Title}";
    }

    private static string BuildWindowTitle()
    {
        const string appName = "LLRP Reader UI";
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

        var informational = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informational))
        {
            return $"{appName} v{informational}";
        }

        var assemblyVersion = assembly.GetName().Version?.ToString();
        return string.IsNullOrWhiteSpace(assemblyVersion)
            ? appName
            : $"{appName} v{assemblyVersion}";
    }
}


