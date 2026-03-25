using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LLRPReaderUI_WPF.Logging;
using LLRPReaderUI_WPF.Messages;
using LLRPReaderUI_WPF.Models;
using LLRPReaderUI_WPF.Services;
using LLRPSdk;
using Serilog;
using System.Collections.ObjectModel;
using System.Reflection;
using FontAwesome.Sharp;
using System.Windows.Media;

namespace LLRPReaderUI_WPF.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly LlrpReader reader;
    private readonly ReaderStatusStore statusStore;
    private readonly IAppLogService logs;
    private readonly LanguageService _languageService;

    public ThemeLanguageViewModel ThemeLanguageViewModel { get; }

    [ObservableProperty]
    private NavigationItem? selectedNavigationItem;

    [ObservableProperty]
    private object? currentPageViewModel;

    [ObservableProperty]
    private string statusText = "Ready";

    [ObservableProperty]
    private string deviceStatusText = "设备: 未连接";

    [ObservableProperty]
    private bool isDeviceConnected;

    [ObservableProperty]
    private bool isGlobalBusy;

    [ObservableProperty]
    private string busyText = string.Empty;

    [ObservableProperty]
    private string inventoryStatusText = "盘点: 未知";

    [ObservableProperty]
    private bool isInventoryRunning;

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
        LogViewModel logViewModel,
        LLRPMessageViewModel llrpMessageViewModel,
        ThemeLanguageViewModel themeLanguageViewModel,
        LanguageService languageService)
    {
        this.reader = reader;
        this.statusStore = statusStore;
        this.logs = logs;
        _languageService = languageService;
        ThemeLanguageViewModel = themeLanguageViewModel;

        NavigationItems =
        [
            new NavigationItem { Title = _languageService.GetLocalizedString("Menu.DeviceConnection"), TitleResourceKey = "Menu.DeviceConnection", Icon = IconChar.PlugCircleBolt, IconBrush = CreateBrush("#0EA5E9"), ViewModel = deviceConnectionViewModel },
            new NavigationItem { Title = _languageService.GetLocalizedString("Menu.Settings"), TitleResourceKey = "Menu.Settings", Icon = IconChar.Sliders, IconBrush = CreateBrush("#8B5CF6"), ViewModel = settingsViewModel },
            new NavigationItem { Title = _languageService.GetLocalizedString("Menu.GPIO"), TitleResourceKey = "Menu.GPIO", Icon = IconChar.Microchip, IconBrush = CreateBrush("#F59E0B"), ViewModel = gpioViewModel },
            new NavigationItem { Title = _languageService.GetLocalizedString("Menu.InventoryConfig"), TitleResourceKey = "Menu.InventoryConfig", Icon = IconChar.ScrewdriverWrench, IconBrush = CreateBrush("#14B8A6"), ViewModel = inventoryConfigViewModel },
            new NavigationItem { Title = _languageService.GetLocalizedString("Menu.Inventory"), TitleResourceKey = "Menu.Inventory", Icon = IconChar.Tags, IconBrush = CreateBrush("#10B981"), ViewModel = inventoryViewModel },
            new NavigationItem { Title = _languageService.GetLocalizedString("Menu.ReadWrite"), TitleResourceKey = "Menu.ReadWrite", Icon = IconChar.PenToSquare, IconBrush = CreateBrush("#F97316"), ViewModel = readWriteViewModel },
            new NavigationItem { Title = _languageService.GetLocalizedString("Menu.AdvancedTagOps"), TitleResourceKey = "Menu.AdvancedTagOps", Icon = IconChar.Flask, IconBrush = CreateBrush("#EF4444"), ViewModel = advancedTagOpsViewModel },
            new NavigationItem { Title = _languageService.GetLocalizedString("Menu.Log"), TitleResourceKey = "Menu.Log", Icon = IconChar.ClipboardList, IconBrush = CreateBrush("#6366F1"), ViewModel = logViewModel },
            new NavigationItem { Title = _languageService.GetLocalizedString("Menu.LLRPMessage"), TitleResourceKey = "Menu.LLRPMessage", Icon = IconChar.CodeBranch, IconBrush = CreateBrush("#8B5CF6"), ViewModel = llrpMessageViewModel }
        ];

        // Subscribe to language changes
        _languageService.OnLanguageChanged += OnLanguageChanged;

        WeakReferenceMessenger.Default.Register<MainWindowViewModel, ConnectionStateChangedMessage>(this, static (r, m) =>
        {
            r.OnDeviceConnectionStateChanged(m.Value);
        });
        WeakReferenceMessenger.Default.Register<MainWindowViewModel, BusyStateChangedMessage>(this, static (r, m) =>
        {
            r.IsGlobalBusy = m.Value;
            r.BusyText = m.Text ?? string.Empty;
        });
        WeakReferenceMessenger.Default.Register<MainWindowViewModel, StatusUpdateRequestedMessage>(this, static (r, m) =>
        {
            var reason = m.Value;
            if (reason.Contains("Inventory", StringComparison.OrdinalIgnoreCase))
            {
                // Use lightweight query for inventory state changes
                r.QuerySingulatingState();
            }
            else
            {
                r.QueryStatus();
            }
        });

        SelectedNavigationItem = NavigationItems[0];
        CurrentPageViewModel = SelectedNavigationItem.ViewModel;
    }

    private void OnDeviceConnectionStateChanged(bool isConnected)
    {
        QueryStatus();
    }

    [RelayCommand]
    private void QueryStatus()
    {
        if (!reader.IsConnected)
        {
            statusStore.Clear();
            UpdateStatusTexts(null);
            IsDeviceConnected = false;
            IsInventoryRunning = false;
            IdentificationStatusText = $"{_languageService.GetLocalizedString("Status.MAC")}: --";
            return;
        }

        var status = reader.QueryStatus();
        statusStore.Set(status);
        logs.LogOperation("已更新设备状态");
        UpdateStatusTexts(status);
        IsDeviceConnected = status.IsConnected;
        IsInventoryRunning = status.IsSingulating;
        IdentificationStatusText = $"{_languageService.GetLocalizedString("Status.MAC")}: {FormatIdentification(status.ReaderIdentity)}";

        var highText = _languageService.GetLocalizedString("Status.High");
        var lowText = _languageService.GetLocalizedString("Status.Low");

        var gpiParts = status.Gpis
            .Cast<GpiStatus>()
            .OrderBy(x => x.PortNumber)
            .Select(x => $"{x.PortNumber}:{(x.State ? highText : lowText)}")
            .ToList();
        GpiStatusText = gpiParts.Count > 0
            ? $"{_languageService.GetLocalizedString("Status.GPI")}: {string.Join(" ", gpiParts)}"
            : $"{_languageService.GetLocalizedString("Status.GPI")}: {_languageService.GetLocalizedString("Status.NoData")}";

        var gpoParts = status.GpoStates
            .Cast<GpoStatus>()
            .OrderBy(x => x.PortNumber)
            .Select(x => $"{x.PortNumber}:{(x.State ? highText : lowText)}")
            .ToList();
        GpoStatusText = gpoParts.Count > 0
            ? $"{_languageService.GetLocalizedString("Status.GPO")}: {string.Join(" ", gpoParts)}"
            : $"{_languageService.GetLocalizedString("Status.GPO")}: {_languageService.GetLocalizedString("Status.NoResponse")}";
    }

    private void UpdateStatusTexts(Status? status)
    {
        var deviceText = _languageService.GetLocalizedString("Status.Device");
        var inventoryText = _languageService.GetLocalizedString("Status.Inventory");

        if (status == null)
        {
            DeviceStatusText = $"{deviceText}: {_languageService.GetLocalizedString("Status.NotConnected")}";
            InventoryStatusText = $"{inventoryText}: {_languageService.GetLocalizedString("Status.Unknown")}";
            AntennaStatusText = $"{_languageService.GetLocalizedString("Inventory.Antenna")}: --";
            GpiStatusText = $"{_languageService.GetLocalizedString("Status.GPI")}: --";
            GpoStatusText = $"{_languageService.GetLocalizedString("Status.GPO")}: --";
        }
        else
        {
            DeviceStatusText = $"{deviceText}: {(status.IsConnected ? _languageService.GetLocalizedString("Status.Connected") : _languageService.GetLocalizedString("Status.NotConnected"))}";
            InventoryStatusText = $"{inventoryText}: {(status.IsSingulating ? _languageService.GetLocalizedString("Status.Running") : _languageService.GetLocalizedString("Status.Idle"))}";
        }
    }

    private static bool ShouldRefreshStatus(string reason)
    {
        return reason.Contains("Inventory", StringComparison.OrdinalIgnoreCase);
    }

    [RelayCommand]
    private void QuerySingulatingState()
    {
        var inventoryText = _languageService.GetLocalizedString("Status.Inventory");

        if (!reader.IsConnected)
        {
            InventoryStatusText = $"{inventoryText}: {_languageService.GetLocalizedString("Status.Unknown")}";
            IsInventoryRunning = false;
            return;
        }

        bool isSingulating = reader.QuerySingulatingState();
        statusStore.SetSingulating(isSingulating);
        InventoryStatusText = $"{inventoryText}: {(isSingulating ? _languageService.GetLocalizedString("Status.Running") : _languageService.GetLocalizedString("Status.Idle"))}";
        IsInventoryRunning = isSingulating;
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

    private static Brush CreateBrush(string hex)
    {
        return new BrushConverter().ConvertFromString(hex) as Brush ?? Brushes.DodgerBlue;
    }

    private void OnLanguageChanged(AppLanguage language)
    {
        // Update navigation item titles
        foreach (var item in NavigationItems)
        {
            if (!string.IsNullOrEmpty(item.TitleResourceKey))
            {
                var newTitle = _languageService.GetLocalizedString(item.TitleResourceKey);
                item.UpdateTitle(newTitle);
            }
        }

        // Refresh status display texts (no device query)
        RefreshStatusDisplay();
    }

    /// <summary>
    /// Refresh status display texts from cached state (no device query).
    /// </summary>
    private void RefreshStatusDisplay()
    {
        if (statusStore.TryGetSnapshot(out var status) && status != null)
        {
            UpdateStatusTexts(status);
            var highText = _languageService.GetLocalizedString("Status.High");
            var lowText = _languageService.GetLocalizedString("Status.Low");

            var gpiParts = status.Gpis
                .Cast<GpiStatus>()
                .OrderBy(x => x.PortNumber)
                .Select(x => $"{x.PortNumber}:{(x.State ? highText : lowText)}")
                .ToList();
            GpiStatusText = gpiParts.Count > 0
                ? $"{_languageService.GetLocalizedString("Status.GPI")}: {string.Join(" ", gpiParts)}"
                : $"{_languageService.GetLocalizedString("Status.GPI")}: {_languageService.GetLocalizedString("Status.NoData")}";

            var gpoParts = status.GpoStates
                .Cast<GpoStatus>()
                .OrderBy(x => x.PortNumber)
                .Select(x => $"{x.PortNumber}:{(x.State ? highText : lowText)}")
                .ToList();
            GpoStatusText = gpoParts.Count > 0
                ? $"{_languageService.GetLocalizedString("Status.GPO")}: {string.Join(" ", gpoParts)}"
                : $"{_languageService.GetLocalizedString("Status.GPO")}: {_languageService.GetLocalizedString("Status.NoResponse")}";

            IdentificationStatusText = $"{_languageService.GetLocalizedString("Status.MAC")}: {FormatIdentification(status.ReaderIdentity)}";
        }
        else
        {
            UpdateStatusTexts(null);
            IdentificationStatusText = $"{_languageService.GetLocalizedString("Status.MAC")}: --";
        }
    }
}
