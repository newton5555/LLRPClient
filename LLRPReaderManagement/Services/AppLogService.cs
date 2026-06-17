using LLRPReaderManagement.Models;
using LLRPReaderManagement.State;
using Microsoft.Extensions.Logging;

namespace LLRPReaderManagement.Services;

public sealed class AppLogService(AppState state, ILogger<AppLogService> logger) : IAppLogService
{
    public void Log(string category, string message, LogLevel level = LogLevel.Information, Exception? exception = null)
    {
        state.AddLog(new LogEntry(DateTime.Now, level, category, message));
        logger.Log(level, exception, "[{Category}] {Message}", category, message);
    }
}
