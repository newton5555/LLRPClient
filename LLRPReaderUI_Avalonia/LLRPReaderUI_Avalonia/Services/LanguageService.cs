using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;

namespace LLRPReaderUI_Avalonia.Services;

public enum AppLanguage
{
    EnUS,
    ZhCN
}

public class LanguageService : INotifyPropertyChanged
{
    private static readonly string ConfigFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LLRPReaderUI_Avalonia",
        "language_config.json"
    );

    private AppLanguage _currentLanguage;

    // 语言资源定义
    private static readonly Dictionary<string, string> ZhCN_Strings = new()
    {
        ["App.Title"] = "LLRP 读写器",
        ["Menu.Navigation"] = "功能导航",
        ["Menu.DeviceConnection"] = "设备连接",
        ["Menu.Settings"] = "参数配置",
        ["Menu.GPIO"] = "GPIO 配置",
        ["Menu.InventoryConfig"] = "盘点配置",
        ["Menu.Inventory"] = "盘点操作",
        ["Menu.ReadWrite"] = "读写操作",
        ["Menu.AdvancedTagOps"] = "高级标签操作",
        ["Menu.Log"] = "日志",
        ["Menu.LLRPMessage"] = "历史LLRP消息",
        ["DeviceConnection.Connect"] = "连接",
        ["DeviceConnection.Disconnect"] = "断开",
        ["Status.Device"] = "设备",
        ["Status.Inventory"] = "盘点",
        ["Status.Connected"] = "已连接",
        ["Status.NotConnected"] = "未连接",
        ["Status.Running"] = "运行中",
        ["Status.Idle"] = "空闲",
        ["Status.Unknown"] = "未知",
        ["Status.GPI"] = "GPI",
        ["Status.GPO"] = "GPO",
        ["Status.MAC"] = "MAC",
        ["Status.High"] = "高",
        ["Status.Low"] = "低",
        ["Status.NoData"] = "无数据",
        ["Status.NoResponse"] = "设备未返回",
        ["Theme.ToggleLight"] = "切换到亮色主题",
        ["Theme.ToggleDark"] = "切换到暗色主题",
        ["Language.Toggle"] = "切换语言",
        ["Common.All"] = "全部",
        ["Common.ConnectFirst"] = "请先连接设备",
        ["Inventory.Start"] = "开始寻卡",
        ["Inventory.Stop"] = "停止寻卡",
        ["Inventory.Clear"] = "清空数据",
        ["Inventory.PullBuffer"] = "手动拉缓存",
        ["Inventory.Antenna"] = "天线",
        ["MainWindow.DeviceNotConnected"] = "设备: 未连接",
        ["MainWindow.InventoryUnknown"] = "盘点: 未知",
        ["MainWindow.AntennaDefault"] = "天线: --",
        ["MainWindow.GPIDefault"] = "GPI: --",
        ["MainWindow.GPODefault"] = "GPO: --",
        ["MainWindow.MACDefault"] = "MAC: --",
        ["GPIO.High"] = "高",
        ["GPIO.Low"] = "低",
        ["ReadWrite.Ready"] = "就绪",
        ["ReadWrite.Waiting"] = "等待操作...",
        ["ReadWrite.Reading"] = "正在读取...",
        ["ReadWrite.Writing"] = "正在写入...",
    };

    private static readonly Dictionary<string, string> EnUS_Strings = new()
    {
        ["App.Title"] = "LLRP Reader UI",
        ["Menu.Navigation"] = "Navigation",
        ["Menu.DeviceConnection"] = "Device Connection",
        ["Menu.Settings"] = "Settings",
        ["Menu.GPIO"] = "GPIO Config",
        ["Menu.InventoryConfig"] = "Inventory Config",
        ["Menu.Inventory"] = "Inventory",
        ["Menu.ReadWrite"] = "Read/Write",
        ["Menu.AdvancedTagOps"] = "Advanced Tag Ops",
        ["Menu.Log"] = "Log",
        ["Menu.LLRPMessage"] = "LLRP Messages",
        ["DeviceConnection.Connect"] = "Connect",
        ["DeviceConnection.Disconnect"] = "Disconnect",
        ["Status.Device"] = "Device",
        ["Status.Inventory"] = "Inventory",
        ["Status.Connected"] = "Connected",
        ["Status.NotConnected"] = "Not Connected",
        ["Status.Running"] = "Running",
        ["Status.Idle"] = "Idle",
        ["Status.Unknown"] = "Unknown",
        ["Status.GPI"] = "GPI",
        ["Status.GPO"] = "GPO",
        ["Status.MAC"] = "MAC",
        ["Status.High"] = "High",
        ["Status.Low"] = "Low",
        ["Status.NoData"] = "No Data",
        ["Status.NoResponse"] = "No response from device",
        ["Theme.ToggleLight"] = "Switch to Light Theme",
        ["Theme.ToggleDark"] = "Switch to Dark Theme",
        ["Language.Toggle"] = "Switch Language",
        ["Common.All"] = "All",
        ["Common.ConnectFirst"] = "Please connect device first",
        ["Inventory.Start"] = "Start Inventory",
        ["Inventory.Stop"] = "Stop Inventory",
        ["Inventory.Clear"] = "Clear Data",
        ["Inventory.PullBuffer"] = "Pull Buffer",
        ["Inventory.Antenna"] = "Antenna",
        ["MainWindow.DeviceNotConnected"] = "Device: Not connected",
        ["MainWindow.InventoryUnknown"] = "Inventory: Unknown",
        ["MainWindow.AntennaDefault"] = "Antenna: --",
        ["MainWindow.GPIDefault"] = "GPI: --",
        ["MainWindow.GPODefault"] = "GPO: --",
        ["MainWindow.MACDefault"] = "MAC: --",
        ["GPIO.High"] = "High",
        ["GPIO.Low"] = "Low",
        ["ReadWrite.Ready"] = "Ready",
        ["ReadWrite.Waiting"] = "Waiting for operation...",
        ["ReadWrite.Reading"] = "Reading...",
        ["ReadWrite.Writing"] = "Writing...",
    };

    public LanguageService()
    {
        _currentLanguage = LoadLanguageFromConfig();
    }

    public AppLanguage CurrentLanguage => _currentLanguage;

    public event Action<AppLanguage>? OnLanguageChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public void Initialize()
    {
        UpdateApplicationResources();
    }

    public void SetLanguage(AppLanguage language)
    {
        if (_currentLanguage == language) return;
        _currentLanguage = language;
        SaveLanguageToConfig(language);
        UpdateApplicationResources();
        OnLanguageChanged?.Invoke(language);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentLanguage)));
    }

    private void UpdateApplicationResources()
    {
        var app = Application.Current;
        if (app?.Resources == null) return;

        var strings = _currentLanguage == AppLanguage.ZhCN ? ZhCN_Strings : EnUS_Strings;

        foreach (var kvp in strings)
        {
            app.Resources[kvp.Key] = kvp.Value;
        }
    }

    public string GetLocalizedString(string key)
    {
        var strings = _currentLanguage == AppLanguage.ZhCN ? ZhCN_Strings : EnUS_Strings;

        if (strings.TryGetValue(key, out var value))
        {
            return value;
        }

        // Fallback: 尝试从 Application.Resources 获取
        var app = Application.Current;
        if (app?.Resources != null && app.Resources.TryGetValue(key, out var resourceValue) && resourceValue is string str)
        {
            return str;
        }

        return key;
    }

    private static AppLanguage LoadLanguageFromConfig()
    {
        try
        {
            if (File.Exists(ConfigFilePath))
            {
                var json = File.ReadAllText(ConfigFilePath);
                var config = JsonSerializer.Deserialize<LanguageConfig>(json);
                if (config != null && Enum.IsDefined(typeof(AppLanguage), config.Language))
                {
                    return config.Language;
                }
            }
        }
        catch { }

        var systemLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return systemLang == "zh" ? AppLanguage.ZhCN : AppLanguage.EnUS;
    }

    private static void SaveLanguageToConfig(AppLanguage language)
    {
        try
        {
            var dir = Path.GetDirectoryName(ConfigFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var config = new LanguageConfig { Language = language };
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFilePath, json);
        }
        catch { }
    }

    private class LanguageConfig
    {
        public AppLanguage Language { get; set; }
    }
}
