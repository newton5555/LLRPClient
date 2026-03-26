using Microsoft.Extensions.Logging;
using Serilog;
using System.Reflection;
using System.Text.Json;
using System.IO;

namespace LLRPReaderUI_Avalonia.Logging;

/// <summary>
/// 日志配置数据模型
/// </summary>
public class LoggingConfiguration
{
    public LogFeatureConfig RawFrameLogging { get; set; } = new();
    public LogFeatureConfig LlrpMessageLogging { get; set; } = new();
    public LogFeatureConfig EventNotificationLogging { get; set; } = new();
}

/// <summary>
/// 单个日志功能的配置
/// </summary>
public class LogFeatureConfig
{
    public bool Enabled { get; set; } = true;
    public bool IsXml { get; set; } = false;
}

/// <summary>
/// 日志配置管理器 - 从 LoggingConfig.json 读取 Serilog 配置和日志功能开关
/// </summary>
public static class LoggingConfigurationManager
{
    private static LoggingConfiguration? _config;
    private const string ConfigFileName = "LoggingConfig.json";
    private const string ConfigResourceName = "LLRPReaderUI_Avalonia.LoggingConfig.json";

    /// <summary>
    /// 获取日志存储目录（跨平台适配）
    /// </summary>
    private static string GetLogDirectory()
    {
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(basePath, "LLRPReaderUI_Avalonia", "logs");
    }

    /// <summary>
    /// 从嵌入资源读取配置文件内容
    /// </summary>
    private static JsonDocument? LoadEmbeddedConfig()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(ConfigResourceName);
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                var json = reader.ReadToEnd();
                return JsonDocument.Parse(json);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load embedded logging configuration: {ex.Message}");
        }
        return null;
    }

    /// <summary>
    /// 加载配置（优先从文件读取，失败则从嵌入资源读取）
    /// </summary>
    private static JsonDocument? LoadConfig()
    {
        // 桌面端：优先从文件读取（用户可修改）
        var configPath = Path.Combine(AppContext.BaseDirectory, ConfigFileName);
        if (File.Exists(configPath))
        {
            try
            {
                var json = File.ReadAllText(configPath);
                return JsonDocument.Parse(json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load logging config file: {ex.Message}");
            }
        }

        // 安卓/移动端：从嵌入资源读取
        return LoadEmbeddedConfig();
    }

    /// <summary>
    /// 从配置文件构建 Serilog ILogger
    /// </summary>
    public static Serilog.ILogger BuildLogger()
    {
        var logDir = GetLogDirectory();
        Directory.CreateDirectory(logDir);

        try
        {
            using var root = LoadConfig();
            if (root != null)
            {
                // 尝试从配置文件读取 Serilog 配置
                if (root.RootElement.TryGetProperty("serilog", out var serilogSection))
                {
                    return BuildLoggerFromConfig(serilogSection, logDir);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load logging configuration: {ex.Message}. Using defaults.");
        }

        // 使用默认配置
        return new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Async(a => a.File(
                path: Path.Combine(logDir, "app-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"))
            .CreateLogger();
    }

    /// <summary>
    /// 从 JSON 配置构建 Serilog ILogger
    /// </summary>
    private static Serilog.ILogger BuildLoggerFromConfig(JsonElement serilogSection, string logDir)
    {
        var config = new LoggerConfiguration();

        // 读取最小日志级别
        if (serilogSection.TryGetProperty("minimumLevel", out var levelElement))
        {
            var levelStr = levelElement.GetString() ?? "Debug";
            var level = levelStr switch
            {
                "Verbose" => Serilog.Events.LogEventLevel.Verbose,
                "Debug" => Serilog.Events.LogEventLevel.Debug,
                "Information" => Serilog.Events.LogEventLevel.Information,
                "Warning" => Serilog.Events.LogEventLevel.Warning,
                "Error" => Serilog.Events.LogEventLevel.Error,
                "Fatal" => Serilog.Events.LogEventLevel.Fatal,
                _ => Serilog.Events.LogEventLevel.Debug
            };
            config.MinimumLevel.Is(level);
        }
        else
        {
            config.MinimumLevel.Debug();
        }

        // 读取写入目标（WriteTo）
        if (serilogSection.TryGetProperty("writeTo", out var writeToArray))
        {
            if (writeToArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var sink in writeToArray.EnumerateArray())
                {
                    if (sink.TryGetProperty("name", out var nameElement))
                    {
                        var sinkName = nameElement.GetString();
                        
                        if (sinkName == "Async" && sink.TryGetProperty("args", out var asyncArgs))
                        {
                            // 异步文件写入
                            config.WriteTo.Async(a =>
                            {
                                if (asyncArgs.TryGetProperty("configure", out var configureArray) && 
                                    configureArray.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (var fileSink in configureArray.EnumerateArray())
                                    {
                                        if (fileSink.TryGetProperty("name", out var fileSinkName) &&
                                            fileSinkName.GetString() == "File" &&
                                            fileSink.TryGetProperty("args", out var fileArgs))
                                        {
                                            // 使用平台适配的日志目录
                                            var fileName = fileArgs.TryGetProperty("path", out var pathElement)
                                                ? Path.GetFileName(pathElement.GetString() ?? "app-.log")
                                                : "app-.log";
                                            var path = Path.Combine(logDir, fileName);
                                            
                                            var rollingIntervalStr = fileArgs.TryGetProperty("rollingInterval", out var intervalElement)
                                                ? intervalElement.GetString() ?? "Day"
                                                : "Day";
                                            
                                            var retainedCount = fileArgs.TryGetProperty("retainedFileCountLimit", out var retainedElement)
                                                ? retainedElement.GetInt32()
                                                : 14;
                                            
                                            var template = fileArgs.TryGetProperty("outputTemplate", out var templateElement)
                                                ? templateElement.GetString() ?? "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"
                                                : "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}";

                                            var interval = rollingIntervalStr switch
                                            {
                                                "Day" => RollingInterval.Day,
                                                "Month" => RollingInterval.Month,
                                                "Year" => RollingInterval.Year,
                                                "Hour" => RollingInterval.Hour,
                                                _ => RollingInterval.Day
                                            };

                                            a.File(path, rollingInterval: interval, retainedFileCountLimit: retainedCount, outputTemplate: template);
                                        }
                                    }
                                }
                            });
                        }
                    }
                }
            }
        }
        else
        {
            // 默认使用异步文件写入
            config.WriteTo.Async(a => a.File(
                path: Path.Combine(logDir, "app-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"));
        }

        return config.CreateLogger();
    }

    /// <summary>
    /// 加载日志功能开关配置
    /// </summary>
    public static LoggingConfiguration LoadConfiguration()
    {
        if (_config != null)
            return _config;

        try
        {
            using var root = LoadConfig();
            if (root != null)
            {
                _config = new LoggingConfiguration
                {
                    RawFrameLogging = ParseFeatureConfig(root, "rawFrameLogging"),
                    LlrpMessageLogging = ParseFeatureConfig(root, "llrpMessageLogging"),
                    EventNotificationLogging = ParseFeatureConfig(root, "eventNotificationLogging")
                };
            }
            else
            {
                // 使用默认配置（全部启用）
                _config = new LoggingConfiguration();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load logging configuration: {ex.Message}. Using defaults.");
            _config = new LoggingConfiguration();
        }

        return _config;
    }

    /// <summary>
    /// 解析单个功能的配置
    /// </summary>
    private static LogFeatureConfig ParseFeatureConfig(JsonDocument doc, string featureName)
    {
        try
        {
            if (doc.RootElement.TryGetProperty("logging", out var loggingElement) &&
                loggingElement.TryGetProperty(featureName, out var featureElement))
            {
                var enabled = featureElement.TryGetProperty("enabled", out var enabledProp) 
                    ? enabledProp.GetBoolean() 
                    : true;

                var isXml = false;
                if (featureElement.TryGetProperty("isXml", out var isXmlProp) &&
                    (isXmlProp.ValueKind == JsonValueKind.True || isXmlProp.ValueKind == JsonValueKind.False))
                {
                    isXml = isXmlProp.GetBoolean();
                }
                else if (featureElement.TryGetProperty("isxml", out var isxmlProp) &&
                         (isxmlProp.ValueKind == JsonValueKind.True || isxmlProp.ValueKind == JsonValueKind.False))
                {
                    isXml = isxmlProp.GetBoolean();
                }

                return new LogFeatureConfig { Enabled = enabled, IsXml = isXml };
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error parsing {featureName}: {ex.Message}");
        }

        // 默认返回启用状态
        return new LogFeatureConfig { Enabled = true };
    }
}

