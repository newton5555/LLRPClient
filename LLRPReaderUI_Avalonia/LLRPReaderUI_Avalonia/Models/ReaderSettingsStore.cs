using LLRPSdk;

namespace LLRPReaderUI_Avalonia.Models;

public class ReaderSettingsStore
{
    private readonly object syncRoot = new();
    private Settings? current;

    public bool HasValue
    {
        get
        {
            lock (syncRoot)
            {
                return current is not null;
            }
        }
    }

    public void Set(Settings settings)
    {
        lock (syncRoot)
        {
            current = Clone(settings);
        }
    }

    public bool TryGetSnapshot(out Settings? snapshot)
    {
        lock (syncRoot)
        {
            if (current is null)
            {
                snapshot = null;
                return false;
            }

            snapshot = Clone(current);
            return true;
        }
    }

    public void Clear()
    {
        lock (syncRoot)
        {
            current = null;
        }
    }

    private static Settings Clone(Settings source)
    {
        return Settings.FromXmlString(source.ToXmlString());
    }
}
