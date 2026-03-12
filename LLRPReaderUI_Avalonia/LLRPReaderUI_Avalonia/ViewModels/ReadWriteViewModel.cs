using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LLRPSdk;
using LLRPReaderUI_Avalonia.Logging;
using LLRPReaderUI_Avalonia.Messages;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using System.Collections.Generic;
using System;

namespace LLRPReaderUI_Avalonia.ViewModels;

public partial class ReadWriteViewModel : ViewModelBase
{
    private readonly LlrpReader reader;
    private readonly IAppLogService logs;
    private uint? currentOpSequenceId;
    private bool? attachedDataWasEnabled; // 保存附加数据 AO 的 enable 状态
    private CancellationTokenSource? readTimeoutCts;

    private const int ReadOperationTimeoutMs = 5000;

    public ReadWriteViewModel(LlrpReader reader, IAppLogService logs)
    {
        this.reader = reader;
        this.logs = logs;
        this.reader.TagOpComplete += OnTagOpComplete;

        WeakReferenceMessenger.Default.Register<ReadWriteViewModel, ConnectionStateChangedMessage>(this, static (r, m) =>
        {
            r.OnConnectionStateChanged(m.Value);
        });
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
    private string operationResult = "等待操作";

    [ObservableProperty]
    private bool isConnected;

    [ObservableProperty]
    private bool isBusy;

    private bool CanExecuteOperation() => IsConnected && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanExecuteOperation))]
    private void ReadMemory()
    {
        if (!reader.IsConnected)
        {
            OperationResult = "请先连接设备";
            logs.LogOperation("读内存失败：设备未连接", Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(TargetTagData))
        {
            OperationResult = $"请输入目标 {SelectedTargetTagBank}";
            logs.LogOperation($"读内存失败：目标{SelectedTargetTagBank}为空", Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }

        try
        {
            IsBusy = true;
            ReadData = string.Empty;
            OperationResult = "读取中...";

            // 保存附加数据 AO 的状态（如果存在）
            attachedDataWasEnabled = reader.IsAttachedDataAccessSpecEnabled();
            logs.LogOperation($"保存附加数据 AO 状态：{(attachedDataWasEnabled.HasValue ? (attachedDataWasEnabled.Value ? "Enable" : "Disable") : "不存在")}");

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
                    logs.LogOperation($"清理旧操作失败：{ex.Message}", Microsoft.Extensions.Logging.LogLevel.Warning);
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

            OperationResult = $"已启动读取 (OpSequence ID: {sequence.Id})";
            logs.LogOperation($"发起读内存操作，OpSequence={sequence.Id}, TargetBank={SelectedTargetTagBank}, Target={TargetTagData.Trim()}, MB={SelectedMemoryBank}, WordPointer={WordPointer}, WordCount={WordCount}");
        }
        catch (Exception ex)
        {
            IsBusy = false;
            CancelReadTimeout();
            OperationResult = $"读取失败：{ex.Message}";
            logs.LogOperation($"读内存失败：{ex.Message}", Microsoft.Extensions.Logging.LogLevel.Error, ex);
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteOperation))]
    private void WriteMemory()
    {
        if (!reader.IsConnected)
        {
            OperationResult = "请先连接设备";
            logs.LogOperation("写内存失败：设备未连接", Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(TargetTagData))
        {
            OperationResult = $"请输入目标 {SelectedTargetTagBank}";
            logs.LogOperation($"写内存失败：目标{SelectedTargetTagBank}为空", Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(WriteData))
        {
            OperationResult = "请输入写入数据(十六进制)";
            logs.LogOperation("写内存失败：写入数据为空", Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }

        var dataText = WriteData.Trim();
        if (dataText.Length % 4 != 0)
        {
            OperationResult = "写入数据长度必须是 4 的倍数(按 Word)";
            logs.LogOperation("写内存失败：写入数据长度不是 4 的倍数", Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }

        try
        {
            var writeTagData = TagData.FromHexString(dataText);

            IsBusy = true;
            OperationResult = "写入中...";

            attachedDataWasEnabled = reader.IsAttachedDataAccessSpecEnabled();
            logs.LogOperation($"保存附加数据 AO 状态：{(attachedDataWasEnabled.HasValue ? (attachedDataWasEnabled.Value ? "Enable" : "Disable") : "不存在")}");

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
                    logs.LogOperation($"清理旧操作失败：{ex.Message}", Microsoft.Extensions.Logging.LogLevel.Warning);
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

            OperationResult = $"已启动写入 (OpSequence ID: {sequence.Id})";
            logs.LogOperation($"发起写内存操作，OpSequence={sequence.Id}, TargetBank={SelectedTargetTagBank}, Target={TargetTagData.Trim()}, MB={SelectedMemoryBank}, WordPointer={WordPointer}, WordCount={dataText.Length / 4}");
        }
        catch (Exception ex)
        {
            IsBusy = false;
            CancelReadTimeout();
            OperationResult = $"写入失败：{ex.Message}";
            logs.LogOperation($"写内存失败：{ex.Message}", Microsoft.Extensions.Logging.LogLevel.Error, ex);
        }
    }

    [RelayCommand]
    private void ClearData()
    {
        ReadData = string.Empty;
        OperationResult = "已清空";
        logs.LogOperation("清空读写页面数据");
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
                            OperationResult = $"读取成功，读取到 {readResult.Data?.ToList().Count ?? 0} 个字";
                            logs.LogOperation(OperationResult);
                        }
                        else
                        {
                            OperationResult = $"读取失败：{readResult.Result}";
                            logs.LogOperation(OperationResult, Microsoft.Extensions.Logging.LogLevel.Warning);
                        }
                    }
                    else if (result is TagWriteOpResult writeResult)
                    {
                        if (writeResult.Result == WriteResultStatus.Success)
                        {
                            OperationResult = $"写入成功，写入 {writeResult.NumWordsWritten} 个字";
                            logs.LogOperation(OperationResult);
                        }
                        else
                        {
                            OperationResult = $"写入失败：{writeResult.Result}";
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

            OperationResult = $"操作超时：未匹配到目标标签（{ReadOperationTimeoutMs} ms）";
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
                    logs.LogOperation($"已恢复附加数据 AccessSpec，Enable={attachedDataWasEnabled.Value}");
                }

                currentOpSequenceId = null;
            }
            catch (Exception ex)
            {
                logs.LogOperation($"清理操作失败：{ex.Message}", Microsoft.Extensions.Logging.LogLevel.Warning);
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
        var dispatcher = Dispatcher.UIThread;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            action();
            return;
        }

        dispatcher.Post(action);
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
        }
        OperationResult = connected ? "设备已连接，可执行读写" : "请先连接设备";
    }
}


