using LLRPReaderManagement.Services;
using LLRPReaderManagement.State;

namespace LLRPReaderManagement.ViewModels;

public sealed class InventoryViewModel(AppState state, InventoryService inventory, ReaderManagementService readers)
{
    public AppState State => state;
    public void Start() => inventory.Start();
    public void Stop() => inventory.Stop();
    public void Pull() => inventory.PullBufferedReports();
    public void Clear() => inventory.Clear();
    public void SelectReader(string endpoint) => readers.SelectReader(endpoint);
}
