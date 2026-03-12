using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LLRPSdk;
using LLRPReaderUI_WPF.Logging;
using LLRPReaderUI_WPF.Messages;
using System.Threading;
using System.Windows;

namespace LLRPReaderUI_WPF.ViewModels;

public partial class AdvancedTagOpsViewModel : ObservableObject
{
    private readonly LlrpReader reader;
    private readonly IAppLogService logs;
    private uint? currentOpSequenceId;
    private bool? attachedDataWasEnabled;

    public AdvancedTagOpsViewModel(LlrpReader reader, IAppLogService logs)
    {
        this.reader = reader;
        this.logs = logs;
        this.reader.TagOpComplete += OnTagOpComplete;

        WeakReferenceMessenger.Default.Register<AdvancedTagOpsViewModel, ConnectionStateChangedMessage>(this, static (r, m) =>
        {
            r.OnConnectionStateChanged(m.Value);
        });
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
    private string operationResult = "等待操作";

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

    [RelayCommand(CanExecute = nameof(CanExecuteBlockErase))]
    private void BlockErase()
    {
        if (!ValidateConnectedAndTarget("块擦除"))
            return;

        if (WordCount <= 0)
        {
            OperationResult = "块擦除失败：Word 数量必须大于 0";
            return;
        }

        var eraseOp = new TagBlockEraseOp
        {
            MemoryBank = ParseMemoryBank(SelectedMemoryBank),
            WordPointer = (ushort)WordPointer,
            WordCount = (ushort)WordCount,
            AccessPassword = TagData.FromHexString(AccessPassword)
        };

        ExecuteSingleOp(eraseOp, $"块擦除，MB={SelectedMemoryBank}, WordPointer={WordPointer}, WordCount={WordCount}");
    }

    [RelayCommand(CanExecute = nameof(CanExecuteOperation))]
    private void Lock()
    {
        if (!ValidateConnectedAndTarget("锁操作"))
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

        ExecuteSingleOp(lockOp, $"锁操作，Bank={SelectedLockBank}, Action={SelectedLockAction}");
    }

    [RelayCommand(CanExecute = nameof(CanExecuteOperation))]
    private void Kill()
    {
        if (!ValidateConnectedAndTarget("Kill"))
            return;

        if (string.IsNullOrWhiteSpace(KillPassword) || KillPassword.Trim().Length != 8)
        {
            OperationResult = "Kill 失败：Kill 密码需为 8 位十六进制";
            return;
        }

        var killOp = new TagKillOp
        {
            KillPassword = TagData.FromHexString(KillPassword.Trim())
        };

        ExecuteSingleOp(killOp, "Kill 操作");
    }

    private bool ValidateConnectedAndTarget(string opName)
    {
        if (!reader.IsConnected)
        {
            OperationResult = "请先连接设备";
            return false;
        }

        if (string.IsNullOrWhiteSpace(TargetTagData))
        {
            OperationResult = $"{opName}失败：请输入目标 {SelectedTargetTagBank}";
            return false;
        }

        return true;
    }

    private void ExecuteSingleOp(TagOp op, string opDescription)
    {
        try
        {
            IsBusy = true;
            OperationResult = "执行中...";

            attachedDataWasEnabled = reader.IsAttachedDataAccessSpecEnabled();

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
            OperationResult = $"已启动 {opDescription} (OpSequence ID: {sequence.Id})";
            logs.LogOperation(OperationResult);
        }
        catch (Exception ex)
        {
            IsBusy = false;
            OperationResult = $"执行失败：{ex.Message}";
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
                            ? "块擦除成功"
                            : $"块擦除失败：{blockErase.Result}";
                    }
                    else if (result is TagLockOpResult lockResult)
                    {
                        OperationResult = lockResult.Result == LockResultStatus.Success
                            ? "锁操作成功"
                            : $"锁操作失败：{lockResult.Result}";
                    }
                    else if (result is TagKillOpResult killResult)
                    {
                        OperationResult = killResult.Result == KillResultStatus.Success
                            ? "Kill 成功"
                            : $"Kill 失败：{killResult.Result}";
                    }

                    logs.LogOperation(OperationResult, OperationResult.Contains("失败") ? Microsoft.Extensions.Logging.LogLevel.Warning : Microsoft.Extensions.Logging.LogLevel.Information);
                }
            }
            finally
            {
                FinishOperationCleanup();
            }
        });
    }

    private void FinishOperationCleanup()
    {
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
            }

            currentOpSequenceId = null;
        }
        catch (Exception ex)
        {
            logs.LogOperation($"清理操作失败：{ex.Message}", Microsoft.Extensions.Logging.LogLevel.Warning);
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

        OperationResult = connected ? "设备已连接，可执行高级标签操作" : "请先连接设备";
    }
}
