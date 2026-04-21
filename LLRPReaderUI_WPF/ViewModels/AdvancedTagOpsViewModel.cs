using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LLRPSdk;
using LLRPReaderUI_WPF.Logging;
using LLRPReaderUI_WPF.Messages;
using LLRPReaderUI_WPF.Services;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace LLRPReaderUI_WPF.ViewModels;

public partial class AdvancedTagOpsViewModel : ObservableObject
{
    private readonly LlrpReader reader;
    private readonly IAppLogService logs;
    private readonly LanguageService _languageService;
    private uint? currentOpSequenceId;
    private bool? attachedDataWasEnabled;
    private CancellationTokenSource? operationTimeoutCts;

    private const int OperationTimeoutMs = 5000;

    public AdvancedTagOpsViewModel(LlrpReader reader, IAppLogService logs, LanguageService languageService)
    {
        this.reader = reader;
        this.logs = logs;
        _languageService = languageService;
        this.reader.TagOpComplete += OnTagOpComplete;

        WeakReferenceMessenger.Default.Register<AdvancedTagOpsViewModel, ConnectionStateChangedMessage>(this, static (r, m) =>
        {
            r.OnConnectionStateChanged(m.Value);
        });

        // Set initial state
        OperationResult = _languageService.GetLocalizedString("AdvancedTagOps.Waiting");
    }

    public IReadOnlyList<string> TargetTagBanks { get; } = new[] { "EPC", "TID" };

    public IReadOnlyList<string> MemoryBanks { get; } = new[] { "User", "TID", "Reserved", "EPC" };

    public IReadOnlyList<string> LockBanks { get; } = new[] { "KillPassword", "AccessPassword", "EPC", "TID", "User" };

    public IReadOnlyList<string> LockActions { get; } = new[] { "Lock", "Unlock", "Permalock", "Permaunlock" };

    [ObservableProperty]
    private string selectedTargetTagBank = "EPC";

    [ObservableProperty]
    private string targetTagData = string.Empty;

    [ObservableProperty]
    private string accessPassword = "00000000";

    [ObservableProperty]
    private string selectedMemoryBank = "User";

    [ObservableProperty]
    private int wordPointer;

    [ObservableProperty]
    private int wordCount = 2;

    [ObservableProperty]
    private string killPassword = "00000000";

    [ObservableProperty]
    private string selectedLockBank = "User";

    [ObservableProperty]
    private string selectedLockAction = "Lock";

    [ObservableProperty]
    private string operationResult = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(BlockEraseCommand))]
    [NotifyCanExecuteChangedFor(nameof(LockCommand))]
    [NotifyCanExecuteChangedFor(nameof(KillCommand))]
    private bool isConnected;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(BlockEraseCommand))]
    [NotifyCanExecuteChangedFor(nameof(LockCommand))]
    [NotifyCanExecuteChangedFor(nameof(KillCommand))]
    private bool isBusy;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(BlockEraseCommand))]
    private bool isBlockEraseSupported;

    private bool CanExecuteOperation() => IsConnected && !IsBusy;

    private bool CanExecuteBlockErase() => CanExecuteOperation() && IsBlockEraseSupported;

    /// <summary>
    /// Get a localized string with format arguments.
    /// </summary>
    private string GetLocalizedString(string key, params object[] args)
    {
        var format = _languageService.GetLocalizedString(key);
        return args.Length > 0 ? string.Format(format, args) : format;
    }

    [RelayCommand(CanExecute = nameof(CanExecuteBlockErase))]
    private void BlockErase()
    {
        if (!ValidateConnectedAndTarget(_languageService.GetLocalizedString("AdvancedTagOps.BlockErase")))
            return;

        if (WordCount <= 0)
        {
            OperationResult = _languageService.GetLocalizedString("AdvancedTagOps.WordCountRequired");
            return;
        }

        var eraseOp = new TagBlockEraseOp
        {
            MemoryBank = ParseMemoryBank(SelectedMemoryBank),
            WordPointer = (ushort)WordPointer,
            WordCount = (ushort)WordCount,
            AccessPassword = TagData.FromHexString(AccessPassword)
        };

        ExecuteSingleOp(eraseOp, $"{_languageService.GetLocalizedString("AdvancedTagOps.BlockErase")}, MB={SelectedMemoryBank}, WordPointer={WordPointer}, WordCount={WordCount}");
    }

    [RelayCommand(CanExecute = nameof(CanExecuteOperation))]
    private void Lock()
    {
        if (!ValidateConnectedAndTarget(_languageService.GetLocalizedString("AdvancedTagOps.LockOp")))
            return;

        var lockOp = new TagLockOp
        {
            AccessPassword = TagData.FromHexString(AccessPassword)
        };

        var state = ParseLockState(SelectedLockAction);
        switch (SelectedLockBank)
        {
            case "KillPassword":
                lockOp.KillPasswordLockType = state;
                break;
            case "AccessPassword":
                lockOp.AccessPasswordLockType = state;
                break;
            case "EPC":
                lockOp.EpcLockType = state;
                break;
            case "TID":
                lockOp.TidLockType = state;
                break;
            default:
                lockOp.UserLockType = state;
                break;
        }

        ExecuteSingleOp(lockOp, $"{_languageService.GetLocalizedString("AdvancedTagOps.LockOp")}, Bank={SelectedLockBank}, Action={SelectedLockAction}");
    }

    [RelayCommand(CanExecute = nameof(CanExecuteOperation))]
    private void Kill()
    {
        if (!ValidateConnectedAndTarget("Kill"))
            return;

        if (string.IsNullOrWhiteSpace(KillPassword) || KillPassword.Trim().Length != 8)
        {
            OperationResult = _languageService.GetLocalizedString("AdvancedTagOps.KillPasswordInvalid");
            return;
        }

        var killOp = new TagKillOp
        {
            KillPassword = TagData.FromHexString(KillPassword.Trim())
        };

        ExecuteSingleOp(killOp, _languageService.GetLocalizedString("AdvancedTagOps.KillOp"));
    }

    private bool ValidateConnectedAndTarget(string opName)
    {
        if (!reader.IsConnected)
        {
            OperationResult = _languageService.GetLocalizedString("Common.ConnectFirst");
            return false;
        }

        if (string.IsNullOrWhiteSpace(TargetTagData))
        {
            OperationResult = GetLocalizedString("AdvancedTagOps.TargetRequired", opName, SelectedTargetTagBank);
            return false;
        }

        return true;
    }

    private void ExecuteSingleOp(TagOp op, string opDescription)
    {
        try
        {
            IsBusy = true;
            OperationResult = _languageService.GetLocalizedString("Common.Executing");

            attachedDataWasEnabled = reader.IsAttachedDataAccessSpecEnabled();
            logs.LogOperation(GetLocalizedString("AdvancedTagOps.AOStateSaved", attachedDataWasEnabled.HasValue ? (attachedDataWasEnabled.Value ? "Enable" : "Disable") : "null"));

            if (reader.IsConnected)
            {
                reader.Stop();
                Thread.Sleep(100);
                reader.DeleteAllOpSequences();
                Thread.Sleep(50);
            }

            var sequence = new TagOpSequence
            {
                ExecutionCount = 1,
                TargetTag = new TargetTag
                {
                    MemoryBank = ParseTargetTagMemoryBank(SelectedTargetTagBank),
                    Data = TargetTagData.Trim(),
                    BitPointer = GetTargetBitPointer(SelectedTargetTagBank)
                },
                AntennaId = 0,
                State = SequenceState.Active
            };

            sequence.Ops.Add(op);
            reader.AddOpSequence(sequence);
            currentOpSequenceId = sequence.Id;

            reader.Start();
            StartOperationTimeout(sequence.Id);

            OperationResult = GetLocalizedString("AdvancedTagOps.OpStarted", opDescription, sequence.Id);
            logs.LogOperation(OperationResult);
        }
        catch (Exception ex)
        {
            IsBusy = false;
            CancelOperationTimeout();
            OperationResult = GetLocalizedString("AdvancedTagOps.OpFailed", ex.Message);
            logs.LogOperation(OperationResult, Microsoft.Extensions.Logging.LogLevel.Error, ex);
        }
    }

    private void OnTagOpComplete(LlrpReader sender, TagOpReport results)
    {
        if (!IsBusy)
            return;

        RunOnUi(() =>
        {
            try
            {
                foreach (var result in results.Results)
                {
                    if (currentOpSequenceId.HasValue && result.SequenceId != currentOpSequenceId.Value)
                        continue;

                    if (result is TagBlockEraseOpResult blockErase)
                    {
                        OperationResult = blockErase.Result == BlockEraseResultStatus.Success
                            ? _languageService.GetLocalizedString("AdvancedTagOps.BlockEraseSuccess")
                            : GetLocalizedString("AdvancedTagOps.BlockEraseFailed", blockErase.Result);
                    }
                    else if (result is TagLockOpResult lockResult)
                    {
                        OperationResult = lockResult.Result == LockResultStatus.Success
                            ? _languageService.GetLocalizedString("AdvancedTagOps.LockSuccess")
                            : GetLocalizedString("AdvancedTagOps.LockFailed", lockResult.Result);
                    }
                    else if (result is TagKillOpResult killResult)
                    {
                        OperationResult = killResult.Result == KillResultStatus.Success
                            ? _languageService.GetLocalizedString("AdvancedTagOps.KillSuccess")
                            : GetLocalizedString("AdvancedTagOps.KillFailed", killResult.Result);
                    }

                    logs.LogOperation(OperationResult, OperationResult.Contains(_languageService.GetLocalizedString("Common.Failed")) ? Microsoft.Extensions.Logging.LogLevel.Warning : Microsoft.Extensions.Logging.LogLevel.Information);
                }
            }
            finally
            {
                FinishOperationCleanup();
            }
        });
    }

    private void StartOperationTimeout(uint sequenceId)
    {
        CancelOperationTimeout();

        var cts = new CancellationTokenSource();
        operationTimeoutCts = cts;
        _ = WatchOperationTimeoutAsync(sequenceId, cts.Token);
    }

    private async Task WatchOperationTimeoutAsync(uint sequenceId, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(OperationTimeoutMs, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        RunOnUi(() =>
        {
            if (!IsBusy || currentOpSequenceId != sequenceId)
                return;

            OperationResult = GetLocalizedString("ReadWrite.Timeout", OperationTimeoutMs);
            logs.LogOperation(OperationResult, Microsoft.Extensions.Logging.LogLevel.Warning);
            FinishOperationCleanup();
        });
    }

    private void CancelOperationTimeout()
    {
        try
        {
            operationTimeoutCts?.Cancel();
            operationTimeoutCts?.Dispose();
        }
        catch
        {
        }
        finally
        {
            operationTimeoutCts = null;
        }
    }

    private void FinishOperationCleanup()
    {
        CancelOperationTimeout();
        IsBusy = false;

        if (!reader.IsConnected)
            return;

        try
        {
            reader.Stop();
            Thread.Sleep(100);
            reader.DeleteAllOpSequences();

            if (attachedDataWasEnabled.HasValue)
            {
                reader.RestoreAttachedDataAccessSpec(attachedDataWasEnabled.Value);
                logs.LogOperation(GetLocalizedString("AdvancedTagOps.AORestored", attachedDataWasEnabled.Value));
            }

            currentOpSequenceId = null;
        }
        catch (Exception ex)
        {
            logs.LogOperation(GetLocalizedString("AdvancedTagOps.CleanupFailed", ex.Message), Microsoft.Extensions.Logging.LogLevel.Warning);
        }
    }

    private static MemoryBank ParseMemoryBank(string bankName) => bankName switch
    {
        "User" => MemoryBank.User,
        "TID" => MemoryBank.Tid,
        "Reserved" => MemoryBank.Reserved,
        "EPC" => MemoryBank.Epc,
        _ => MemoryBank.User
    };

    private static MemoryBank ParseTargetTagMemoryBank(string bankName) => bankName switch
    {
        "TID" => MemoryBank.Tid,
        _ => MemoryBank.Epc
    };

    private static ushort GetTargetBitPointer(string bankName) => bankName switch
    {
        "TID" => 0,
        _ => 32
    };

    private static TagLockState ParseLockState(string action) => action switch
    {
        "Unlock" => TagLockState.Unlock,
        "Permalock" => TagLockState.Permalock,
        "Permaunlock" => TagLockState.Permaunlock,
        _ => TagLockState.Lock
    };

    private static void RunOnUi(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            action();
            return;
        }

        dispatcher.Invoke(action);
    }

    private void OnConnectionStateChanged(bool connected)
    {
        IsConnected = connected;
        if (!connected)
        {
            CancelOperationTimeout();
            IsBusy = false;
            currentOpSequenceId = null;
            attachedDataWasEnabled = null;
            IsBlockEraseSupported = false;
        }
        else
        {
            try
            {
                IsBlockEraseSupported = reader.ReaderCapabilities.IsMultiwordBlockEraseAvailable;
            }
            catch
            {
                IsBlockEraseSupported = false;
            }
        }

        OperationResult = connected
            ? _languageService.GetLocalizedString("AdvancedTagOps.Ready")
            : _languageService.GetLocalizedString("Common.ConnectFirst");
    }
}
