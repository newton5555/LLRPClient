using LLRPSdk;

namespace LLRPReaderUI_WPF.Models;

public class ReaderStatusStore
{
    private readonly object syncRoot = new();
    private Status? current;

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

    public void Set(Status status)
    {
        lock (syncRoot)
        {
            current = Clone(status);
        }
    }

    public bool TryGetSnapshot(out Status? snapshot)
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

    private static Status Clone(Status source)
    {
        return Status.FromXmlString(source.ToXmlString());
    }
}