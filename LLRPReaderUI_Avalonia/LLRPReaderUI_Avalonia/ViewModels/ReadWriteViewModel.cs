using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LLRPReaderUI_Avalonia.Logging;
using LLRPReaderUI_Avalonia.Messages;
using LLRPReaderUI_Avalonia.Services;
using LLRPSdk;
using Avalonia.Threading;
using System.Threading;

namespace LLRPReaderUI_Avalonia.ViewModels;

public partial class ReadWriteViewModel : ViewModelBase
{
    private readonly LlrpReader reader;
    private readonly IAppLogService logs;
    private readonly LanguageService _languageService;
    private uint? currentOpSequenceId;
    private bool? attachedDataWasEnabled; // 保存附加数据 AO 的 enable 状态
    private CancellationTokenSource? readTimeoutCts;

    private const int ReadOperationTimeoutMs = 5000;

    public ReadWriteViewModel(LlrpReader reader, IAppLogService logs, LanguageService languageService)
    {
        this.reader = reader;
        this.logs = logs;
        _languageService = languageService;
        this.reader.TagOpComplete += OnTagOpComplete;

        WeakReferenceMessenger.Default.Register<ReadWriteViewModel, ConnectionStateChangedMessage>(this, static (r, m) =>
        {
            r.OnConnectionStateChanged(m.Value);
        });

        // Set initial state
        OperationResult = _languageService.GetLocalizedString("ReadWrite.Waiting");
    }

    /// <summary>
    /// Get a localized string with format arguments.
    /// </summary>
    private string GetLocalizedString(string key, params object[] args)
    {
        var format = _languageService.GetLocalizedString(key);
        return args.Length > 0 ? string.Format(format, args) : format;
    }

    public IReadOnlyList<string> MemoryBanks { get; } = new[] { "User", "TID", "Reserved", "EPC" };

    public IReadOnlyList<string> TargetTagBanks { get; } = new[] { "EPC", "TID" };

    [ObservableProperty]
    private string targetTagData = string.Empty;

    [ObservableProperty]
    private string selectedTargetTagBank = "EPC";

    [ObservableProperty]
    private string selectedMemoryBank = "User";

    [ObservableProperty]
    private int wordPointer = 0;

    [ObservableProperty]
    private int wordCount = 8;

    [ObservableProperty]
    private string accessPassword = "00000000";

    [ObservableProperty]
    private string readData = string.Empty;

    [ObservableProperty]
    private string writeData = string.Empty;

    [ObservableProperty]
    private string operationResult = string.Empty;

    [ObservableProperty]
    private bool isConnected;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isBlockWriteChecked;

    [ObservableProperty]
    private bool isBlockWriteSupported;

    private bool CanExecuteOperation() => IsConnected && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanExecuteOperation))]
    private void ReadMemory()
    {
        if (!reader.IsConnected)
        {
            OperationResult = _languageService.GetLocalizedString("Common.ConnectFirst");
            logs.LogOperation(_languageService.GetLocalizedString("ReadWrite.ReadFailedNotConnected"), Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(TargetTagData))
        {
            OperationResult = GetLocalizedString("ReadWrite.EnterTarget", SelectedTargetTagBank);
            logs.LogOperation(GetLocalizedString("ReadWrite.ReadFailedNoTarget", SelectedTargetTagBank), Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }

        try
        {
            IsBusy = true;
            WeakReferenceMessenger.Default.Send(new StatusUpdateRequestedMessage("TagOperationStarted"));
            ReadData = string.Empty;
            OperationResult = _languageService.GetLocalizedString("ReadWrite.Reading");

            // 保存附加数据 AO 的状态（如果存在）
            attachedDataWasEnabled = reader.IsAttachedDataAccessSpecEnabled();
            logs.LogOperation(GetLocalizedString("ReadWrite.AOStateSaved", attachedDataWasEnabled.HasValue ? (attachedDataWasEnabled.Value ? "Enable" : "Disable") : "null"));

            // 先停止并清空所有现有的 OpSequence（包括附加数据的）
            if (reader.IsConnected)
            {
                try
                {
                    reader.Stop();
                    System.Threading.Thread.Sleep(100); // 等待停止完成
                    reader.DeleteAllOpSequences(); // 删除所有 AccessSpec，包括附加数据 AO
                    System.Threading.Thread.Sleep(50);
                }
                catch (Exception ex)
                {
                    logs.LogOperation(GetLocalizedString("ReadWrite.CleanupFailed", ex.Message), Microsoft.Extensions.Logging.LogLevel.Warning);
                }
            }
            currentOpSequenceId = null;

            // 创建 TagOpSequence（Access Operation 必须依赖 RO Spec）
            TagOpSequence sequence = new TagOpSequence()
            {
                ExecutionCount = 1,
                TargetTag = new TargetTag()
                {
                    MemoryBank = ParseTargetTagMemoryBank(SelectedTargetTagBank),
                    Data = TargetTagData.Trim(),
                    BitPointer = GetTargetBitPointer(SelectedTargetTagBank)
                },
                AntennaId = 0, // 0 表示所有天线
                State = SequenceState.Active
            };



            // 创建 TagReadOp
            TagReadOp readOp = new TagReadOp()
            {
                MemoryBank = ParseMemoryBank(SelectedMemoryBank),
                WordPointer = (ushort)WordPointer,
                WordCount = (ushort)WordCount,
                AccessPassword = TagData.FromHexString(AccessPassword)
            };

            sequence.Ops.Add(readOp);

            // 添加 OpSequence（不需要再调 EnableOpSequence，AddOpSequence 已经启用了）
            reader.AddOpSequence(sequence);
            currentOpSequenceId = sequence.Id;

            // 启动读写操作（必须调用 Start）
            reader.Start();

            StartReadTimeout(sequence.Id);

            OperationResult = GetLocalizedString("ReadWrite.ReadStarted", sequence.Id);
            logs.LogOperation(GetLocalizedString("ReadWrite.ReadLog", sequence.Id, SelectedTargetTagBank, TargetTagData.Trim(), SelectedMemoryBank, WordPointer, WordCount));
        }
        catch (Exception ex)
        {
            IsBusy = false;
            WeakReferenceMessenger.Default.Send(new StatusUpdateRequestedMessage("TagOperationFinished"));
            CancelReadTimeout();
            OperationResult = GetLocalizedString("ReadWrite.ReadFailed", ex.Message);
            logs.LogOperation(GetLocalizedString("ReadWrite.ReadFailed", ex.Message), Microsoft.Extensions.Logging.LogLevel.Error, ex);
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteOperation))]
    private void WriteMemory()
    {
        if (!reader.IsConnected)
        {
            OperationResult = _languageService.GetLocalizedString("Common.ConnectFirst");
            logs.LogOperation(_languageService.GetLocalizedString("ReadWrite.WriteFailedNotConnected"), Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(TargetTagData))
        {
            OperationResult = GetLocalizedString("ReadWrite.EnterTarget", SelectedTargetTagBank);
            logs.LogOperation(GetLocalizedString("ReadWrite.WriteFailedNoTarget", SelectedTargetTagBank), Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(WriteData))
        {
            OperationResult = _languageService.GetLocalizedString("ReadWrite.EnterWriteData");
            logs.LogOperation(_languageService.GetLocalizedString("ReadWrite.WriteFailedNoData"), Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }

        var dataText = WriteData.Trim();
        if (dataText.Length % 4 != 0)
        {
            OperationResult = _languageService.GetLocalizedString("ReadWrite.WriteDataLengthError");
            logs.LogOperation(_languageService.GetLocalizedString("ReadWrite.WriteFailedInvalidLength"), Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }

        try
        {
            var writeTagData = TagData.FromHexString(dataText);

            IsBusy = true;
            WeakReferenceMessenger.Default.Send(new StatusUpdateRequestedMessage("TagOperationStarted"));
            OperationResult = _languageService.GetLocalizedString("ReadWrite.Writing");

            attachedDataWasEnabled = reader.IsAttachedDataAccessSpecEnabled();
            logs.LogOperation(GetLocalizedString("ReadWrite.AOStateSaved", attachedDataWasEnabled.HasValue ? (attachedDataWasEnabled.Value ? "Enable" : "Disable") : "null"));

            if (reader.IsConnected)
            {
                try
                {
                    reader.Stop();
                    Thread.Sleep(100);
                    reader.DeleteAllOpSequences();
                    Thread.Sleep(50);
                }
                catch (Exception ex)
                {
                    logs.LogOperation(GetLocalizedString("ReadWrite.CleanupFailed", ex.Message), Microsoft.Extensions.Logging.LogLevel.Warning);
                }
            }
            currentOpSequenceId = null;

            TagOpSequence sequence = new TagOpSequence()
            {
                ExecutionCount = 1,
                TargetTag = new TargetTag()
                {
                    MemoryBank = ParseTargetTagMemoryBank(SelectedTargetTagBank),
                    Data = TargetTagData.Trim(),
                    BitPointer = GetTargetBitPointer(SelectedTargetTagBank)
                },
                AntennaId = 0,
                State = SequenceState.Active
            };

            sequence.BlockWriteEnabled = IsBlockWriteChecked && IsBlockWriteSupported;

            TagWriteOp writeOp = new TagWriteOp()
            {
                MemoryBank = ParseMemoryBank(SelectedMemoryBank),
                WordPointer = (ushort)WordPointer,
                Data = writeTagData,
                AccessPassword = TagData.FromHexString(AccessPassword)
            };



            sequence.Ops.Add(writeOp);
            reader.AddOpSequence(sequence);
            currentOpSequenceId = sequence.Id;

            reader.Start();
            StartReadTimeout(sequence.Id);

            OperationResult = GetLocalizedString("ReadWrite.WriteStarted", sequence.Id);
            logs.LogOperation(GetLocalizedString("ReadWrite.WriteLog", sequence.Id, SelectedTargetTagBank, TargetTagData.Trim(), SelectedMemoryBank, WordPointer, dataText.Length / 4, sequence.BlockWriteEnabled));
        }
        catch (Exception ex)
        {
            IsBusy = false;
            WeakReferenceMessenger.Default.Send(new StatusUpdateRequestedMessage("TagOperationFinished"));
            CancelReadTimeout();
            OperationResult = GetLocalizedString("ReadWrite.WriteFailed", ex.Message);
            logs.LogOperation(GetLocalizedString("ReadWrite.WriteFailed", ex.Message), Microsoft.Extensions.Logging.LogLevel.Error, ex);
        }
    }

    [RelayCommand]
    private void ClearData()
    {
        ReadData = string.Empty;
        OperationResult = _languageService.GetLocalizedString("ReadWrite.Cleared");
        logs.LogOperation(_languageService.GetLocalizedString("ReadWrite.ClearDataLog"));
    }

    private void OnTagOpComplete(LlrpReader sender, TagOpReport results)
    {
        // 只有在执行读写操作时才处理结果，避免与寻卡的 TagsReported 冲突
        if (!IsBusy)
            return;

        RunOnUi(() =>
        {
            try
            {
                foreach (var result in results.Results)
                {
                    if (result is TagReadOpResult readResult)
                    {
                        if (readResult.Result == ReadResultStatus.Success)
                        {
                            ReadData = readResult.Data?.ToHexString() ?? "(empty)";
                            OperationResult = GetLocalizedString("ReadWrite.ReadSuccess", readResult.Data?.ToList().Count ?? 0);
                            logs.LogOperation(OperationResult);
                        }
                        else
                        {
                            OperationResult = GetLocalizedString("ReadWrite.ReadResultFailed", readResult.Result);
                            logs.LogOperation(OperationResult, Microsoft.Extensions.Logging.LogLevel.Warning);
                        }
                    }
                    else if (result is TagWriteOpResult writeResult)
                    {
                        var writeModeText = writeResult.IsBlockWrite
                            ? _languageService.GetLocalizedString("ReadWrite.BlockWriteMode")
                            : _languageService.GetLocalizedString("ReadWrite.NormalWrite");
                        if (writeResult.Result == WriteResultStatus.Success)
                        {
                            OperationResult = GetLocalizedString("ReadWrite.WriteSuccess", writeModeText, writeResult.NumWordsWritten);
                            logs.LogOperation(OperationResult);
                        }
                        else
                        {
                            OperationResult = GetLocalizedString("ReadWrite.WriteResultFailed", writeModeText, writeResult.Result);
                            logs.LogOperation(OperationResult, Microsoft.Extensions.Logging.LogLevel.Warning);
                        }
                    }
                }
            }
            finally
            {
               FinishOperationCleanup();
            }
        });
    }

    private void StartReadTimeout(uint sequenceId)
    {
        CancelReadTimeout();

        var cts = new CancellationTokenSource();
        readTimeoutCts = cts;
        _ = WatchReadTimeoutAsync(sequenceId, cts.Token);
    }

    private async Task WatchReadTimeoutAsync(uint sequenceId, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(ReadOperationTimeoutMs, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        RunOnUi(() =>
        {
            if (!IsBusy || currentOpSequenceId != sequenceId)
                return;

            OperationResult = GetLocalizedString("ReadWrite.Timeout", ReadOperationTimeoutMs);
            logs.LogOperation(OperationResult, Microsoft.Extensions.Logging.LogLevel.Warning);
            FinishOperationCleanup();
        });
    }

    private void CancelReadTimeout()
    {
        try
        {
            readTimeoutCts?.Cancel();
            readTimeoutCts?.Dispose();
        }
        catch
        {
        }
        finally
        {
            readTimeoutCts = null;
        }
    }

    private void FinishOperationCleanup()
    {
        CancelReadTimeout();
        IsBusy = false;
        WeakReferenceMessenger.Default.Send(new StatusUpdateRequestedMessage("TagOperationFinished"));

        // 操作完成后清空本次 OpSequence，并恢复附加数据 AO 的状态
        if (reader.IsConnected)
        {
            try
            {
                reader.Stop();
                Thread.Sleep(100);
                reader.DeleteAllOpSequences();

                // 恢复附加数据 AO 及其状态
                if (attachedDataWasEnabled.HasValue)
                {
                    reader.RestoreAttachedDataAccessSpec(attachedDataWasEnabled.Value);
                    logs.LogOperation(GetLocalizedString("ReadWrite.AORestored", attachedDataWasEnabled.Value));
                }

                currentOpSequenceId = null;
            }
            catch (Exception ex)
            {
                logs.LogOperation(GetLocalizedString("ReadWrite.CleanupFailed", ex.Message), Microsoft.Extensions.Logging.LogLevel.Warning);
            }
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

    private static void RunOnUi(Action action)
    {
        Dispatcher.UIThread.Post(action);
    }

    private void OnConnectionStateChanged(bool connected)
    {
        IsConnected = connected;
        if (!connected)
        {
            CancelReadTimeout();
            IsBusy = false;
            currentOpSequenceId = null;
            attachedDataWasEnabled = null;
            IsBlockWriteSupported = false;
            IsBlockWriteChecked = false;
        }
        else
        {
            try
            {
                IsBlockWriteSupported = reader.ReaderCapabilities.IsMultiwordBlockWriteAvailable;
                if (!IsBlockWriteSupported)
                {
                    IsBlockWriteChecked = false;
                }
            }
            catch
            {
                IsBlockWriteSupported = false;
                IsBlockWriteChecked = false;
            }
        }
        OperationResult = connected
            ? _languageService.GetLocalizedString("ReadWrite.Ready")
            : _languageService.GetLocalizedString("Common.ConnectFirst");
    }
}
