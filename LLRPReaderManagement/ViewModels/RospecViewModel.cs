using LLRPReaderManagement.Services;
using LLRPReaderManagement.State;

namespace LLRPReaderManagement.ViewModels;

public sealed class RospecViewModel(AppState state, InventoryService inventory, ReaderManagementService readers)
{
    public AppState State => state;
    public void Start() => inventory.StartActive();
    public void Stop() => inventory.StopActive();
    public void SelectReader(string endpoint) => readers.SelectReader(endpoint);
}
