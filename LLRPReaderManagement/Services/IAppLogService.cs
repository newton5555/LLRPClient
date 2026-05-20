using Microsoft.Extensions.Logging;

namespace LLRPReaderManagement.Services;

public interface IAppLogService
{
    void Log(string category, string message, LogLevel level = LogLevel.Information, Exception? exception = null);
}
