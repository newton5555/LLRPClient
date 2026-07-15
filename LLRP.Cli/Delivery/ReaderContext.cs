using Org.LLRP.LTK.LLRPV1;

namespace LLRP.Cli.Delivery;

public enum ReaderWorkflowPhase
{
    Offline,
    Connected,
    Ready,
    Configured,
    RospecDisabled,
    RospecEnabled,
    InventoryActive,
    Faulted
}

public sealed class ReaderContext
{
    private readonly Dictionary<uint, string> _rospecStates = new();

    public ReaderWorkflowPhase Phase { get; private set; } = ReaderWorkflowPhase.Offline;
    public string? Host { get; private set; }
    public int Port { get; private set; }
    public bool Tls { get; private set; }
    public string? LastError { get; private set; }
    public uint? CurrentRospecId { get; private set; }
    public long ReceivedFrames { get; private set; }
    public long TagReports { get; private set; }
    public bool IsConnected => Phase is not ReaderWorkflowPhase.Offline and not ReaderWorkflowPhase.Faulted;
    public IReadOnlyDictionary<uint, string> RospecStates => _rospecStates;
    public IReadOnlyCollection<uint> KnownRospecIds => _rospecStates.Keys.Order().ToArray();

    public string Prompt
    {
        get
        {
            if (Phase == ReaderWorkflowPhase.Offline) return "llrp[offline]";
            var target = Host ?? "reader";
            return $"llrp[{target}|{PhaseLabel(Phase)}]";
        }
    }

    public void Connected(string host, int port, bool tls)
    {
        Host = host;
        Port = port;
        Tls = tls;
        Phase = ReaderWorkflowPhase.Connected;
        LastError = null;
        CurrentRospecId = null;
        _rospecStates.Clear();
    }

    public void ConnectionFailed(string host, int port, bool tls, string message)
    {
        Host = host;
        Port = port;
        Tls = tls;
        Phase = ReaderWorkflowPhase.Faulted;
        LastError = message;
        CurrentRospecId = null;
        _rospecStates.Clear();
    }

    public void Disconnected()
    {
        Host = null;
        Port = 0;
        Tls = false;
        Phase = ReaderWorkflowPhase.Offline;
        LastError = null;
        CurrentRospecId = null;
        _rospecStates.Clear();
    }

    public void Observe(IEnumerable<LlrpFrame> frames)
    {
        foreach (var frame in frames)
        {
            if (frame.Direction == FrameDirection.Rx)
            {
                ReceivedFrames++;
                if (frame.DecodedMessage is MSG_RO_ACCESS_REPORT report)
                    TagReports += report.TagReportData?.Length ?? 0;
            }
            if (frame.DecodedMessage is MSG_GET_ROSPECS_RESPONSE rospecs && frame.IsSuccess != false)
            {
                _rospecStates.Clear();
                foreach (var rospec in rospecs.ROSpec ?? [])
                    _rospecStates[rospec.ROSpecID] = rospec.CurrentState.ToString();
                DeriveRospecPhase();
            }
        }
    }

    public void OperationSucceeded(ReaderOperation operation, uint rospecId)
    {
        LastError = null;
        switch (operation)
        {
            case ReaderOperation.Capabilities:
            case ReaderOperation.Configuration:
                if (Phase is ReaderWorkflowPhase.Connected or ReaderWorkflowPhase.Faulted) Phase = ReaderWorkflowPhase.Ready;
                break;
            case ReaderOperation.Rospecs:
                if (_rospecStates.Count == 0) Phase = ReaderWorkflowPhase.Configured;
                break;
            case ReaderOperation.CreateDefaultRospec:
            case ReaderOperation.ApplyDefaultSettings:
                _rospecStates[1] = "Inactive";
                CurrentRospecId = 1;
                Phase = ReaderWorkflowPhase.RospecEnabled;
                break;
            case ReaderOperation.EnableRospec:
                _rospecStates[rospecId] = "Inactive";
                CurrentRospecId = rospecId;
                Phase = ReaderWorkflowPhase.RospecEnabled;
                break;
            case ReaderOperation.StartRospec:
                _rospecStates[rospecId] = "Active";
                CurrentRospecId = rospecId;
                Phase = ReaderWorkflowPhase.InventoryActive;
                break;
            case ReaderOperation.StopRospec:
                _rospecStates[rospecId] = "Inactive";
                CurrentRospecId = rospecId;
                Phase = ReaderWorkflowPhase.RospecEnabled;
                break;
            case ReaderOperation.DisableRospec:
                _rospecStates[rospecId] = "Disabled";
                CurrentRospecId = rospecId;
                Phase = ReaderWorkflowPhase.RospecDisabled;
                break;
            case ReaderOperation.DeleteRospec:
                _rospecStates.Remove(rospecId);
                CurrentRospecId = null;
                Phase = ReaderWorkflowPhase.Configured;
                break;
            case ReaderOperation.DeleteAllRospecs:
                _rospecStates.Clear();
                CurrentRospecId = null;
                Phase = ReaderWorkflowPhase.Configured;
                break;
        }
    }

    public void OperationFailed(string message, bool connectionLost)
    {
        LastError = message;
        if (connectionLost) Phase = ReaderWorkflowPhase.Faulted;
    }

    private void DeriveRospecPhase()
    {
        var active = _rospecStates.FirstOrDefault(pair => pair.Value.Equals("Active", StringComparison.OrdinalIgnoreCase));
        if (active.Key != 0)
        {
            CurrentRospecId = active.Key;
            Phase = ReaderWorkflowPhase.InventoryActive;
            return;
        }
        var enabled = _rospecStates.FirstOrDefault(pair => pair.Value.Equals("Inactive", StringComparison.OrdinalIgnoreCase));
        if (enabled.Key != 0)
        {
            CurrentRospecId = enabled.Key;
            Phase = ReaderWorkflowPhase.RospecEnabled;
            return;
        }
        if (_rospecStates.Count > 0)
        {
            CurrentRospecId = _rospecStates.Keys.Min();
            Phase = ReaderWorkflowPhase.RospecDisabled;
            return;
        }
        CurrentRospecId = null;
        Phase = ReaderWorkflowPhase.Configured;
    }

    private static string PhaseLabel(ReaderWorkflowPhase phase) => phase switch
    {
        ReaderWorkflowPhase.InventoryActive => "inventory",
        ReaderWorkflowPhase.RospecDisabled => "rospec-disabled",
        ReaderWorkflowPhase.RospecEnabled => "rospec-enabled",
        _ => phase.ToString().ToLowerInvariant()
    };
}

public sealed record NextAction(string Command, string Reason);

public static class PromptChain
{
    public static IReadOnlyList<NextAction> GetNextActions(ReaderContext context) => context.Phase switch
    {
        ReaderWorkflowPhase.Offline => [new("connect <host>", "establish an LLRP session")],
        ReaderWorkflowPhase.Faulted => [new($"connect {context.Host ?? "<host>"}{(context.Port > 0 ? $" {context.Port}" : string.Empty)}", "recover the lost connection"), new("status", "inspect the last failure")],
        ReaderWorkflowPhase.Connected => [new("caps", "verify protocol compatibility and discover reader features")],
        ReaderWorkflowPhase.Ready => [new("rospec list", "discover installed ROSpecs and their actual states"), new("config", "inspect the current reader configuration")],
        ReaderWorkflowPhase.Configured => [new("rospec create default", "create a usable default ROSpec"), new("rospec list", "refresh ROSpec state from the reader")],
        ReaderWorkflowPhase.RospecDisabled => [new($"rospec enable {context.CurrentRospecId ?? 1}", "enable the selected ROSpec")],
        ReaderWorkflowPhase.RospecEnabled => [new($"rospec start {context.CurrentRospecId ?? 1}", "start inventory"), new("rospec list", "refresh ROSpec state")],
        ReaderWorkflowPhase.InventoryActive => [new("monitor 30", "stream tag reports and reader events"), new($"rospec stop {context.CurrentRospecId ?? 1}", "stop inventory")],
        _ => []
    };
}

public sealed record OperationPreflight(bool Allowed, string? Message = null, string? Recovery = null)
{
    public static OperationPreflight Permit { get; } = new(true);
}

public static class OperationRules
{
    public static OperationPreflight Validate(ReaderContext context, ReaderOperation operation, uint rospecId)
    {
        if (operation == ReaderOperation.DeleteAllRospecs)
        {
            var active = context.RospecStates.FirstOrDefault(item => Is(item.Value, "Active"));
            return active.Key == 0
                ? OperationPreflight.Permit
                : new(false, $"ROSpec {active.Key} is active.", $"Run `rospec stop {active.Key}` before deleting all ROSpecs.");
        }
        if (rospecId == 0 || !context.RospecStates.TryGetValue(rospecId, out var state))
            return OperationPreflight.Permit;

        return operation switch
        {
            ReaderOperation.EnableRospec when Is(state, "Active") =>
                new(false, $"ROSpec {rospecId} is already active.", $"Run `rospec stop {rospecId}` before changing its enabled state."),
            ReaderOperation.EnableRospec when Is(state, "Inactive") =>
                new(false, $"ROSpec {rospecId} is already enabled.", $"Run `rospec start {rospecId}` to begin inventory."),
            ReaderOperation.StartRospec when Is(state, "Disabled") =>
                new(false, $"ROSpec {rospecId} is disabled.", $"Run `rospec enable {rospecId}` first."),
            ReaderOperation.StartRospec when Is(state, "Active") =>
                new(false, $"ROSpec {rospecId} is already active.", "Run `monitor 30` to observe reports."),
            ReaderOperation.StopRospec when !Is(state, "Active") =>
                new(false, $"ROSpec {rospecId} is not active (reader state: {state}).", $"Run `rospec list` to refresh state before stopping it."),
            ReaderOperation.DisableRospec when Is(state, "Active") =>
                new(false, $"ROSpec {rospecId} is active.", $"Run `rospec stop {rospecId}` before disabling it."),
            ReaderOperation.DisableRospec when Is(state, "Disabled") =>
                new(false, $"ROSpec {rospecId} is already disabled.", $"Run `rospec enable {rospecId}` when ready."),
            ReaderOperation.DeleteRospec when Is(state, "Active") =>
                new(false, $"ROSpec {rospecId} is active.", $"Run `rospec stop {rospecId}` before deleting it."),
            _ => OperationPreflight.Permit
        };
    }

    private static bool Is(string value, string expected) => value.Equals(expected, StringComparison.OrdinalIgnoreCase);
}
