using LLRPReaderManagement.Models;
using LLRPReaderManagement.Repositories;
using LLRPReaderManagement.State;
using LLRPSdk;
using Microsoft.Extensions.Logging;

namespace LLRPReaderManagement.Services;

public sealed class AccessOperationService(ILlrpReaderRepository repository, AppState state, IAppLogService logs)
{
    private const int OperationTimeoutMs = 5000;

    public async Task<AccessOperationResult> ReadAsync(string epc, string memoryBank, ushort wordPointer, ushort wordCount)
    {
        if (!ValidateConnectedTarget(epc, out var validationResult))
        {
            return validationResult;
        }

        var completion = new TaskCompletionSource<AccessOperationResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        void OnTagOpCompleted(TagOpReport report)
        {
            try
            {
                foreach (var result in report.Results)
                {
                    if (result is TagReadOpResult read)
                    {
                        completion.TrySetResult(read.Result == ReadResultStatus.Success
                            ? new AccessOperationResult(true, "Read completed.", read.Data?.ToHexString())
                            : new AccessOperationResult(false, $"Read failed: {read.Result}"));
                        return;
                    }
                }

                completion.TrySetResult(new AccessOperationResult(false, "Tag operation completed without a read result."));
            }
            catch (Exception ex)
            {
                completion.TrySetException(ex);
            }
        }

        try
        {
            state.SetBusy(true, "Reading tag memory");
            repository.TagOpCompleted += OnTagOpCompleted;
            PrepareAccessOperation();

            var sequence = CreateTargetedSequence(epc);
            sequence.Ops.Add(new TagReadOp
            {
                MemoryBank = ParseMemoryBank(memoryBank),
                WordPointer = wordPointer,
                WordCount = wordCount,
                AccessPassword = TagData.FromHexString("00000000")
            });

            repository.AddOpSequence(sequence);
            repository.Start();
            logs.Log("Access", $"Read started. Sequence={sequence.Id}, EPC={epc}, Bank={memoryBank}, Ptr={wordPointer}, Words={wordCount}");

            return await AwaitOperationAsync(completion, "Read");
        }
        catch (Exception ex)
        {
            logs.Log("Access", $"Read failed: {ex.Message}", LogLevel.Error, ex);
            return new AccessOperationResult(false, $"Read failed: {ex.Message}");
        }
        finally
        {
            repository.TagOpCompleted -= OnTagOpCompleted;
            CleanupAccessOperation();
        }
    }

    public async Task<AccessOperationResult> WriteAsync(
        string epc,
        string memoryBank,
        ushort wordPointer,
        string writeData,
        bool blockWriteEnabled = false)
    {
        if (!ValidateConnectedTarget(epc, out var validationResult))
        {
            return validationResult;
        }

        var dataText = NormalizeHex(writeData);
        if (string.IsNullOrWhiteSpace(dataText))
        {
            return new AccessOperationResult(false, "Write data is required.");
        }

        if (dataText.Length % 4 != 0)
        {
            return new AccessOperationResult(false, "Write data length must be a multiple of one 16-bit word (4 hex characters).");
        }

        TagData writeTagData;
        try
        {
            writeTagData = TagData.FromHexString(dataText);
        }
        catch (Exception ex)
        {
            return new AccessOperationResult(false, $"Invalid write data: {ex.Message}");
        }

        var completion = new TaskCompletionSource<AccessOperationResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        void OnTagOpCompleted(TagOpReport report)
        {
            try
            {
                foreach (var result in report.Results)
                {
                    if (result is TagWriteOpResult write)
                    {
                        completion.TrySetResult(write.Result == WriteResultStatus.Success
                            ? new AccessOperationResult(
                                true,
                                $"{(write.IsBlockWrite ? "BlockWrite" : "Write")} completed. Words written: {write.NumWordsWritten}.",
                                dataText)
                            : new AccessOperationResult(false, $"Write failed: {write.Result}"));
                        return;
                    }
                }

                completion.TrySetResult(new AccessOperationResult(false, "Tag operation completed without a write result."));
            }
            catch (Exception ex)
            {
                completion.TrySetException(ex);
            }
        }

        try
        {
            state.SetBusy(true, "Writing tag memory");
            repository.TagOpCompleted += OnTagOpCompleted;
            PrepareAccessOperation();

            var sequence = CreateTargetedSequence(epc);
            sequence.BlockWriteEnabled = blockWriteEnabled;
            sequence.Ops.Add(new TagWriteOp
            {
                MemoryBank = ParseMemoryBank(memoryBank),
                WordPointer = wordPointer,
                Data = writeTagData,
                AccessPassword = TagData.FromHexString("00000000")
            });

            repository.AddOpSequence(sequence);
            repository.Start();
            logs.Log("Access", $"Write started. Sequence={sequence.Id}, EPC={epc}, Bank={memoryBank}, Ptr={wordPointer}, Words={dataText.Length / 4}, BlockWrite={blockWriteEnabled}");

            return await AwaitOperationAsync(completion, "Write");
        }
        catch (Exception ex)
        {
            logs.Log("Access", $"Write failed: {ex.Message}", LogLevel.Error, ex);
            return new AccessOperationResult(false, $"Write failed: {ex.Message}");
        }
        finally
        {
            repository.TagOpCompleted -= OnTagOpCompleted;
            CleanupAccessOperation();
        }
    }

    private bool ValidateConnectedTarget(string epc, out AccessOperationResult result)
    {
        if (!repository.IsConnected)
        {
            result = new AccessOperationResult(false, "Connect a reader before running access operations.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(epc))
        {
            result = new AccessOperationResult(false, "Target EPC is required.");
            return false;
        }

        result = new AccessOperationResult(true, string.Empty);
        return true;
    }

    private void PrepareAccessOperation()
    {
        try { repository.Stop(); } catch { }
        repository.DeleteAllOpSequences();
    }

    private void CleanupAccessOperation()
    {
        try { repository.Stop(); } catch { }
        try { repository.DeleteAllOpSequences(); } catch { }
        state.SetBusy(false);
    }

    private async Task<AccessOperationResult> AwaitOperationAsync(TaskCompletionSource<AccessOperationResult> completion, string operationName)
    {
        var completed = await Task.WhenAny(completion.Task, Task.Delay(OperationTimeoutMs));
        if (completed != completion.Task)
        {
            return new AccessOperationResult(false, $"{operationName} timed out after {OperationTimeoutMs} ms.");
        }

        var result = await completion.Task;
        logs.Log("Access", result.Message, result.Success ? LogLevel.Information : LogLevel.Warning);
        return result;
    }

    private static MemoryBank ParseMemoryBank(string value) => value switch
    {
        "EPC" => MemoryBank.Epc,
        "TID" => MemoryBank.Tid,
        "Reserved" => MemoryBank.Reserved,
        _ => MemoryBank.User
    };

    private static TagOpSequence CreateTargetedSequence(string epc)
    {
        return new TagOpSequence
        {
            ExecutionCount = 1,
            TargetTag = new TargetTag
            {
                MemoryBank = MemoryBank.Epc,
                Data = NormalizeHex(epc),
                BitPointer = 32
            },
            AntennaId = 0,
            State = SequenceState.Active
        };
    }

    private static string NormalizeHex(string value)
    {
        return new string((value ?? string.Empty).Where(Uri.IsHexDigit).ToArray()).ToUpperInvariant();
    }
}
