using System.Text.Json;
using Avalonia;

namespace LLRPReaderUI_Avalonia.Services;

public enum AppLanguage
{
    EnUS,
    ZhCN
}

public class LanguageService
{
    private static readonly string ConfigFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LLRPReaderUI_Avalonia",
        "language_config.json"
    );

    private AppLanguage _currentLanguage;
    private Dictionary<string, string> _strings = new();

    public LanguageService()
    {
        _currentLanguage = LoadLanguageFromConfig();
        LoadStrings(_currentLanguage);
    }

    public AppLanguage CurrentLanguage => _currentLanguage;

    public event Action<AppLanguage>? OnLanguageChanged;

    public void SetLanguage(AppLanguage language)
    {
        if (_currentLanguage == language) return;
        _currentLanguage = language;
        SaveLanguageToConfig(language);
        LoadStrings(language);
        OnLanguageChanged?.Invoke(language);
    }

    private void LoadStrings(AppLanguage language)
    {
        // TODO: Load from embedded resources or XAML
        // For now, use hardcoded strings
        _strings = language switch
        {
            AppLanguage.ZhCN => new Dictionary<string, string>
            {
                ["Menu.DeviceConnection"] = "设备连接",
                ["Menu.Settings"] = "设置",
                ["Menu.GPIO"] = "GPIO",
                ["Menu.InventoryConfig"] = "盘点配置",
                ["Menu.Inventory"] = "盘点",
                ["Menu.ReadWrite"] = "读写",
                ["Menu.AdvancedTagOps"] = "高级操作",
                ["Menu.Log"] = "日志",
                ["Menu.LLRPMessage"] = "LLRP消息",
                ["Status.Device"] = "设备",
                ["Status.Connected"] = "已连接",
                ["Status.NotConnected"] = "未连接",
                ["Status.Inventory"] = "盘点",
                ["Status.Running"] = "运行中",
                ["Status.Idle"] = "空闲",
                ["Status.Unknown"] = "未知",
                ["Status.High"] = "高",
                ["Status.Low"] = "低",
                ["Status.GPI"] = "GPI",
                ["Status.GPO"] = "GPO",
                ["Status.MAC"] = "MAC",
                ["Status.NoData"] = "无数据",
                ["Status.NoResponse"] = "无响应",
                ["Common.All"] = "全部",
                ["Common.ConnectFirst"] = "请先连接设备",
                ["MainWindow.DeviceNotConnected"] = "设备未连接",
                ["MainWindow.InventoryUnknown"] = "盘点状态未知",
                ["MainWindow.AntennaDefault"] = "天线: --",
                ["MainWindow.GPIDefault"] = "GPI: --",
                ["MainWindow.GPODefault"] = "GPO: --",
                ["MainWindow.MACDefault"] = "MAC: --",
                ["MainWindow.StatusUpdated"] = "状态已更新",
                ["MainWindow.CurrentPage"] = "当前页面: {0}",
                ["Theme.ToggleDark"] = "切换到暗色主题",
                ["Theme.ToggleLight"] = "切换到亮色主题",
                ["Language.Toggle"] = "切换语言",
                ["Inventory.Start"] = "开始盘点",
                ["Inventory.Stop"] = "停止盘点",
                ["Inventory.PullBuffer"] = "拉取缓冲",
                ["Inventory.Clear"] = "清空",
                ["Inventory.Antenna"] = "天线",
                ["DeviceConnection.Connect"] = "连接",
                ["DeviceConnection.Disconnect"] = "断开",
                ["ReadWrite.Ready"] = "就绪",
                ["ReadWrite.Waiting"] = "等待操作...",
                ["ReadWrite.Reading"] = "正在读取...",
                ["ReadWrite.Writing"] = "正在写入...",
                ["ReadWrite.EnterTarget"] = "请输入目标{0}",
                ["ReadWrite.EnterWriteData"] = "请输入写入数据",
            },
            _ => new Dictionary<string, string>
            {
                ["Menu.DeviceConnection"] = "Device Connection",
                ["Menu.Settings"] = "Settings",
                ["Menu.GPIO"] = "GPIO",
                ["Menu.InventoryConfig"] = "Inventory Config",
                ["Menu.Inventory"] = "Inventory",
                ["Menu.ReadWrite"] = "Read/Write",
                ["Menu.AdvancedTagOps"] = "Advanced Ops",
                ["Menu.Log"] = "Log",
                ["Menu.LLRPMessage"] = "LLRP Message",
                ["Status.Device"] = "Device",
                ["Status.Connected"] = "Connected",
                ["Status.NotConnected"] = "Not Connected",
                ["Status.Inventory"] = "Inventory",
                ["Status.Running"] = "Running",
                ["Status.Idle"] = "Idle",
                ["Status.Unknown"] = "Unknown",
                ["Status.High"] = "High",
                ["Status.Low"] = "Low",
                ["Status.GPI"] = "GPI",
                ["Status.GPO"] = "GPO",
                ["Status.MAC"] = "MAC",
                ["Status.NoData"] = "No Data",
                ["Status.NoResponse"] = "No Response",
                ["Common.All"] = "All",
                ["Common.ConnectFirst"] = "Please connect device first",
                ["MainWindow.DeviceNotConnected"] = "Device not connected",
                ["MainWindow.InventoryUnknown"] = "Inventory status unknown",
                ["MainWindow.AntennaDefault"] = "Antenna: --",
                ["MainWindow.GPIDefault"] = "GPI: --",
                ["MainWindow.GPODefault"] = "GPO: --",
                ["MainWindow.MACDefault"] = "MAC: --",
                ["MainWindow.StatusUpdated"] = "Status updated",
                ["MainWindow.CurrentPage"] = "Current page: {0}",
                ["Theme.ToggleDark"] = "Switch to dark theme",
                ["Theme.ToggleLight"] = "Switch to light theme",
                ["Language.Toggle"] = "Toggle Language",
                ["Inventory.Start"] = "Start Inventory",
                ["Inventory.Stop"] = "Stop Inventory",
                ["Inventory.PullBuffer"] = "Pull Buffer",
                ["Inventory.Clear"] = "Clear",
                ["Inventory.Antenna"] = "Antenna",
                ["DeviceConnection.Connect"] = "Connect",
                ["DeviceConnection.Disconnect"] = "Disconnect",
                ["ReadWrite.Ready"] = "Ready",
                ["ReadWrite.Waiting"] = "Waiting for operation...",
                ["ReadWrite.Reading"] = "Reading...",
                ["ReadWrite.Writing"] = "Writing...",
                ["ReadWrite.EnterTarget"] = "Please enter target {0}",
                ["ReadWrite.EnterWriteData"] = "Please enter write data",
            }
        };
    }

    public string GetLocalizedString(string key)
    {
        return _strings.TryGetValue(key, out var value) ? value : key;
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
        catch
        {
        }

        var systemLang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return systemLang == "zh" ? AppLanguage.ZhCN : AppLanguage.EnUS;
    }

    private static void SaveLanguageToConfig(AppLanguage language)
    {
        try
        {
            var directory = Path.GetDirectoryName(ConfigFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var config = new LanguageConfig { Language = language };
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFilePath, json);
        }
        catch
        {
        }
    }

    private class LanguageConfig
    {
        public AppLanguage Language { get; set; }
    }
}
