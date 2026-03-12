using LLRPSdk;
using Microsoft.Extensions.Logging;

namespace LLRPReaderUI_WPF.Logging;

public sealed class LlrpLoggingBridge
{
    public LlrpLoggingBridge(LlrpReader reader, IAppLogService logs)
    {
        // 加载日志配置
        var config = LoggingConfigurationManager.LoadConfiguration();

        reader.LlrpMessageLogAsXml = config.LlrpMessageLogging.IsXml;
        
        // 仅在启用时注册原始帧日志（RawFrame）
        if (config.RawFrameLogging.Enabled)
        {
            reader.RawFrameReceived += (llrp, data) =>
            {
                _ = Task.Run(() =>
                {
                    try { logs.LogRawFrame("RX", data, LogLevel.Debug); }
                    catch { /* 日志失败不影响通信 */ }
                });
            };
            reader.RawFrameSent += (llrp, data) =>
            {
                _ = Task.Run(() =>
                {
                    try { logs.LogRawFrame("TX", data, LogLevel.Debug); }
                    catch { /* 日志失败不影响通信 */ }
                });
            };
        }

        // 仅在启用时注册 LLRP 消息日志
        if (config.LlrpMessageLogging.Enabled)
        {
            reader.LlrpMessageLogged += (llrp, message) =>
            {
                _ = Task.Run(() =>
                {
                    try { logs.LogLlrpMessage(message, LogLevel.Information); }
                    catch { /* 日志失败不影响通信 */ }
                });
            };
        }

        // 仅在启用时注册错误/事件通知日志
        if (config.EventNotificationLogging.Enabled)
        {
            reader.ErrorNotification += (llrp, ex) =>
            {
                _ = Task.Run(() =>
                {
                    try { logs.LogLlrpMessage($"ReaderError: {ex.Message}", LogLevel.Error, ex); }
                    catch { /* 日志失败不影响通信 */ }
                });
            };
        }
    }
}
