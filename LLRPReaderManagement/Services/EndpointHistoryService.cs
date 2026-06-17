using Microsoft.Maui.Storage;

namespace LLRPReaderManagement.Services;

public sealed class EndpointHistoryService
{
    private const string StorageKey = "reader.endpoint.history";
    private const int MaxEndpointCount = 3;

    public IReadOnlyList<string> RecentEndpoints => Load();

    public void Remember(string endpoint)
    {
        endpoint = endpoint.Trim();
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return;
        }

        var endpoints = Load()
            .Where(x => !string.Equals(x, endpoint, StringComparison.OrdinalIgnoreCase))
            .Prepend(endpoint)
            .Take(MaxEndpointCount)
            .ToList();

        Preferences.Default.Set(StorageKey, string.Join("|", endpoints));
    }

    private static IReadOnlyList<string> Load()
    {
        var value = Preferences.Default.Get(StorageKey, string.Empty);
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(MaxEndpointCount)
            .ToList();
    }
}
