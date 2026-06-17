using LLRPReaderManagement.Services;
using LLRPReaderManagement.State;
using LLRPSdk;
using Org.LLRP.LTK.LLRPV1;

namespace LLRPReaderManagement.ViewModels;

public sealed class InventoryConfigViewModel(AppState state, ReaderManagementService readers)
{
    public AppState State => state;

    public IReadOnlyList<AutoStartMode> AutoStartModes { get; } = Enum.GetValues<AutoStartMode>();
    public IReadOnlyList<AutoStopMode> AutoStopModes { get; } = Enum.GetValues<AutoStopMode>();
    public IReadOnlyList<TagFilterMode> TagFilterModes { get; } = Enum.GetValues<TagFilterMode>();
    public IReadOnlyList<MemoryBank> MemoryBanks { get; } = Enum.GetValues<MemoryBank>();
    public IReadOnlyList<TagFilterOp> TagFilterOps { get; } = Enum.GetValues<TagFilterOp>();
    public IReadOnlyList<StateUnawareAction> StateUnawareActions { get; } = Enum.GetValues<StateUnawareAction>();
    public IReadOnlyList<ENUM_C1G2StateAwareTarget> StateAwareTargets { get; } = Enum.GetValues<ENUM_C1G2StateAwareTarget>();
    public IReadOnlyList<ENUM_C1G2StateAwareAction> StateAwareActions { get; } = Enum.GetValues<ENUM_C1G2StateAwareAction>();
    public IReadOnlyList<ReportMode> ReportModes { get; } = Enum.GetValues<ReportMode>()
        .Where(x => x.ToString() != "IndividualUnbuffered")
        .ToList();

    public bool ShowTagFilter1 => Settings?.Filters.Mode is TagFilterMode.OnlyFilter1 or TagFilterMode.Filter1AndFilter2 or TagFilterMode.Filter1OrFilter2;
    public bool ShowTagFilter2 => Settings?.Filters.Mode is TagFilterMode.OnlyFilter2 or TagFilterMode.Filter1AndFilter2 or TagFilterMode.Filter1OrFilter2;
    public bool ShowTagSelectFilters => Settings?.Filters.Mode is TagFilterMode.UseTagSelectFilters or TagFilterMode.UseStateAwareTagSelectFilters;
    public bool ShowStateUnawareActions => Settings?.Filters.Mode == TagFilterMode.UseTagSelectFilters;
    public bool ShowStateAwareActions => Settings?.Filters.Mode == TagFilterMode.UseStateAwareTagSelectFilters;
    public bool CanUseStateAwareFilters => Settings?.Filters.Mode != TagFilterMode.UseStateAwareTagSelectFilters || Settings.InventoryStateAware;
    public Settings? Settings => state.Settings;

    public void Refresh() => readers.RefreshSettings();

    public void Apply()
    {
        try
        {
            readers.ApplyCurrentSettings();
            state.ShowNotification("Config saved", "Inventory parameters were saved successfully.", true);
        }
        catch (Exception ex)
        {
            state.ShowNotification("Save failed", ex.Message, false);
        }
    }

    public void SelectReader(string endpoint)
    {
        readers.SelectReader(endpoint);
        Refresh();
    }

    public void AddTagSelectFilter()
    {
        if (Settings is null)
        {
            return;
        }

        Settings.Filters.TagSelectFilters.Add(new TagSelectFilter
        {
            MemoryBank = MemoryBank.Epc,
            BitPointer = 32,
            BitCount = 0,
            TagMask = string.Empty,
            MatchAction = StateUnawareAction.Select,
            NonMatchAction = StateUnawareAction.Unselect,
            UseStateAwareAction = Settings.Filters.Mode == TagFilterMode.UseStateAwareTagSelectFilters,
            StateAwareTarget = ENUM_C1G2StateAwareTarget.SL,
            StateAwareAction = ENUM_C1G2StateAwareAction.AssertSLOrA_DeassertSLOrB
        });
    }

    public void RemoveTagSelectFilter(TagSelectFilter filter)
    {
        Settings?.Filters.TagSelectFilters.Remove(filter);
    }
}
