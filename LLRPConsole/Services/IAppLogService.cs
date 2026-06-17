using Microsoft.Extensions.Logging;

namespace LLRPConsole.Services;

public interface IAppLogService
{
    void Log(string category, string message, LogLevel level = LogLevel.Information, Exception? exception = null);
}

