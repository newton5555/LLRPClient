using LLRPReaderManagement.Services;
using LLRPReaderManagement.State;

namespace LLRPReaderManagement.ViewModels;

public sealed class InventoryViewModel(AppState state, InventoryService inventory)
{
    public AppState State => state;
    public void Start() => inventory.Start();
    public void Stop() => inventory.Stop();
    public void Pull() => inventory.PullBufferedReports();
    public void Clear() => inventory.Clear();
}
