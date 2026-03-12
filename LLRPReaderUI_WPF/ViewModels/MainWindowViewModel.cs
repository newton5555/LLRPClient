using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LLRPReaderUI_WPF.Logging;
using LLRPReaderUI_WPF.Messages;
using LLRPReaderUI_WPF.Models;
using LLRPSdk;
using Serilog;
using System.Collections.ObjectModel;
using System.Reflection;

namespace LLRPReaderUI_WPF.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly LlrpReader reader;
    private readonly ReaderStatusStore statusStore;
    private readonly IAppLogService logs;


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

    public MainWindowViewModel(
        LlrpReader reader,
        ReaderStatusStore statusStore,
        DeviceConnectionViewModel deviceConnectionViewModel,
        SettingsViewModel settingsViewModel,
        GpioViewModel gpioViewModel,
        InventoryConfigViewModel inventoryConfigViewModel,
        InventoryViewModel inventoryViewModel,
        ReadWriteViewModel readWriteViewModel,
        AdvancedTagOpsViewModel advancedTagOpsViewModel,
        IAppLogService logs,
        LogViewModel logViewModel)
    {
        this.reader = reader;
        this.statusStore = statusStore;
        this.logs = logs;

        NavigationItems =
        [
            new NavigationItem { Title = "设备连接", Glyph = "\uE13D", ViewModel = deviceConnectionViewModel },
            new NavigationItem { Title = "参数配置", Glyph = "\uE115", ViewModel = settingsViewModel },
            new NavigationItem { Title = "GPIO 配置", Glyph = "\uE129", ViewModel = gpioViewModel },
            new NavigationItem { Title = "寻卡配置", Glyph = "\uE1D3", ViewModel = inventoryConfigViewModel },
            new NavigationItem { Title = "盘点操作", Glyph = "\uE140", ViewModel = inventoryViewModel },
            new NavigationItem { Title = "读写操作", Glyph = "\uE1DF", ViewModel = readWriteViewModel },
            new NavigationItem { Title = "高级标签操作", Glyph = "\uE1FC", ViewModel = advancedTagOpsViewModel },
            new NavigationItem { Title = "日志", Glyph = "\uE121", ViewModel = logViewModel }
        ];

        WeakReferenceMessenger.Default.Register<MainWindowViewModel, ConnectionStateChangedMessage>(this, static (r, m) =>
        {
            r.OnDeviceConnectionStateChanged(m.Value);
        });
        WeakReferenceMessenger.Default.Register<MainWindowViewModel, StatusUpdateRequestedMessage>(this, static (r, m) =>
        {
            if (ShouldRefreshStatus(m.Value))
            {
                r.QueryStatus();
            }
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
            statusStore.Clear();
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
        statusStore.Set(status);
        logs.LogOperation("已更新设备状态");
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

    private static bool ShouldRefreshStatus(string reason)
    {
        return reason.Contains("Inventory", StringComparison.OrdinalIgnoreCase);
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
