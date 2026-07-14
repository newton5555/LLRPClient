using System.Collections.Concurrent;
using LLRPSdk;
using Org.LLRP.LTK.LLRPV1;

namespace LLRP.Cli.Delivery;

public enum ReaderOperation
{
    Capabilities,
    Configuration,
    Rospecs,
    ApplyDefaultSettings,
    EnableRospec,
    DisableRospec,
    StartRospec,
    StopRospec,
    DeleteRospec,
    DeleteAllRospecs
}

public sealed record OperationExecution(
    ReaderOperation Operation,
    uint RospecId,
    IReadOnlyList<LlrpFrame> Frames,
    IReadOnlyList<LlrpTransaction> Transactions,
    TimeSpan Duration,
    Exception? Error)
{
    public bool Succeeded => Error is null && Transactions.All(transaction => transaction.Succeeded);
}

public sealed record RospecEditExecution(
    uint RospecId,
    IReadOnlyList<LlrpFrame> Frames,
    IReadOnlyList<LlrpTransaction> Transactions,
    TimeSpan Duration,
    RospecEditResult? Result,
    Exception? Error)
{
    public bool Succeeded => Error is null && Result is not null && Transactions.All(transaction => transaction.Succeeded);
}

public interface IReaderTransport : IDisposable
{
    bool IsConnected { get; }
    string? Address { get; }
    event Action<byte[]>? RawFrameSent;
    event Action<byte[]>? RawFrameReceived;
    void Connect(string host, int port, bool tls, int timeoutMilliseconds);
    void Execute(ReaderOperation operation, uint rospecId, int timeoutMilliseconds);
    RospecEditResult EditRospec(uint rospecId, RospecEditPatch patch, int timeoutMilliseconds);
    void Disconnect();
}

public sealed class ReaderSession : IDisposable
{
    private const int MaximumCapturedFrames = 10_000;
    private readonly ConcurrentQueue<LlrpFrame> _frames = new();
    private readonly IReaderTransport _transport;
    private readonly int _timeoutMilliseconds;
    private long _totalFrameCount;

    public ReaderSession(int timeoutMilliseconds) : this(timeoutMilliseconds, new SdkReaderTransport()) { }

    public ReaderSession(int timeoutMilliseconds, IReaderTransport transport)
    {
        if (timeoutMilliseconds <= 0) throw new ArgumentOutOfRangeException(nameof(timeoutMilliseconds));
        _timeoutMilliseconds = timeoutMilliseconds;
        _transport = transport;
        _transport.RawFrameSent += raw => Add(LlrpFrame.Decode(FrameDirection.Tx, raw));
        _transport.RawFrameReceived += raw => Add(LlrpFrame.Decode(FrameDirection.Rx, raw));
    }

    public event Action<LlrpFrame>? FrameArrived;
    public IReadOnlyList<LlrpFrame> Frames => _frames.ToArray();
    public bool IsConnected => _transport.IsConnected;
    public string? Address => _transport.Address;
    public long TotalFrameCount => Interlocked.Read(ref _totalFrameCount);

    public IReadOnlyList<LlrpFrame> Connect(string host, int port, bool tls)
    {
        var before = TotalFrameCount;
        _transport.Connect(host, port, tls, _timeoutMilliseconds);
        return FramesAfter(before);
    }

    public OperationExecution Execute(ReaderOperation operation, uint rospecId)
    {
        if (!IsConnected) throw new InvalidOperationException("Not connected to an LLRP reader.");
        var before = TotalFrameCount;
        var started = DateTimeOffset.UtcNow;
        Exception? error = null;
        try
        {
            _transport.Execute(operation, rospecId, _timeoutMilliseconds);
        }
        catch (Exception ex)
        {
            error = ex;
        }
        var duration = DateTimeOffset.UtcNow - started;
        var frames = FramesAfter(before);
        return new(operation, rospecId, frames, LlrpTransactionAnalyzer.Correlate(frames), duration, error);
    }

    public RospecEditExecution EditRospec(uint rospecId, RospecEditPatch patch)
    {
        if (!IsConnected) throw new InvalidOperationException("Not connected to an LLRP reader.");
        var before = TotalFrameCount;
        var started = DateTimeOffset.UtcNow;
        RospecEditResult? result = null;
        Exception? error = null;
        try
        {
            result = _transport.EditRospec(rospecId, patch, _timeoutMilliseconds);
        }
        catch (Exception ex)
        {
            error = ex;
        }
        var duration = DateTimeOffset.UtcNow - started;
        var frames = FramesAfter(before);
        return new(rospecId, frames, LlrpTransactionAnalyzer.Correlate(frames), duration, result, error);
    }

    public void Disconnect() => _transport.Disconnect();

    public void Dispose()
    {
        try
        {
            if (_transport.IsConnected) _transport.Disconnect();
        }
        finally
        {
            _transport.Dispose();
        }
    }

    private IReadOnlyList<LlrpFrame> FramesAfter(long totalBefore)
    {
        var currentTotal = TotalFrameCount;
        var requested = (int)Math.Min(int.MaxValue, Math.Max(0, currentTotal - totalBefore));
        var snapshot = Frames;
        return snapshot.Skip(Math.Max(0, snapshot.Count - requested)).ToArray();
    }

    private void Add(LlrpFrame frame)
    {
        _frames.Enqueue(frame);
        Interlocked.Increment(ref _totalFrameCount);
        while (_frames.Count > MaximumCapturedFrames) _frames.TryDequeue(out _);
        FrameArrived?.Invoke(frame);
    }
}

internal sealed class SdkReaderTransport : IReaderTransport
{
    private readonly LlrpReader _reader = new();

    public SdkReaderTransport()
    {
        _reader.RawFrameSent += (_, raw) => RawFrameSent?.Invoke(raw);
        _reader.RawFrameReceived += (_, raw) => RawFrameReceived?.Invoke(raw);
    }

    public bool IsConnected => _reader.IsConnected;
    public string? Address => _reader.Address;
    public event Action<byte[]>? RawFrameSent;
    public event Action<byte[]>? RawFrameReceived;

    public void Connect(string host, int port, bool tls, int timeoutMilliseconds)
    {
        _reader.ConnectTimeout = timeoutMilliseconds;
        _reader.MessageTimeout = timeoutMilliseconds;
        _reader.Connect(host, port, tls);
    }

    public void Execute(ReaderOperation operation, uint rospecId, int timeoutMilliseconds)
    {
        _reader.MessageTimeout = timeoutMilliseconds;
        switch (operation)
        {
            case ReaderOperation.Capabilities: _ = _reader.QueryFeatureSet(); break;
            case ReaderOperation.Configuration: _ = _reader.QueryReaderConfiguration(ENUM_GetReaderConfigRequestedData.All); break;
            case ReaderOperation.Rospecs: _ = _reader.QueryRoSpecs(); break;
            case ReaderOperation.ApplyDefaultSettings: _reader.ApplyDefaultSettings(); break;
            case ReaderOperation.EnableRospec: _reader.EnableRoSpec(rospecId); break;
            case ReaderOperation.DisableRospec: _reader.DisableRoSpec(rospecId); break;
            case ReaderOperation.StartRospec: _reader.StartRoSpec(rospecId); break;
            case ReaderOperation.StopRospec: _reader.StopRoSpec(rospecId); break;
            case ReaderOperation.DeleteRospec: _reader.DeleteRoSpec(rospecId); break;
            case ReaderOperation.DeleteAllRospecs: _reader.DeleteRoSpec(0); break;
            default: throw new ArgumentOutOfRangeException(nameof(operation));
        }
    }

    public RospecEditResult EditRospec(uint rospecId, RospecEditPatch patch, int timeoutMilliseconds)
    {
        _reader.MessageTimeout = timeoutMilliseconds;
        var response = _reader.QueryRoSpecs();
        var source = response.ROSpec?.SingleOrDefault(item => item.ROSpecID == rospecId) ??
                     throw new InvalidOperationException($"ROSpec {rospecId} was not found on the reader.");
        var originalState = source.CurrentState;
        var original = RospecEditor.Clone(source);
        var edited = RospecEditor.Clone(source);
        var before = RospecEditor.Read(original);
        var after = RospecEditor.Apply(edited, patch);
        if (before == after)
            return new(rospecId, originalState, before, after, Applied: false);

        if (originalState == ENUM_ROSpecState.Active) _reader.StopRoSpec(rospecId);
        if (originalState != ENUM_ROSpecState.Disabled) _reader.DisableRoSpec(rospecId);
        _reader.DeleteRoSpec(rospecId);

        original.CurrentState = ENUM_ROSpecState.Disabled;
        edited.CurrentState = ENUM_ROSpecState.Disabled;
        try
        {
            _reader.AddRoSpec(edited);
        }
        catch (Exception editError)
        {
            try
            {
                _reader.AddRoSpec(original);
                RestoreState(rospecId, originalState);
            }
            catch (Exception rollbackError)
            {
                throw new InvalidOperationException(
                    $"Adding edited ROSpec {rospecId} failed and restoring the original ROSpec also failed.",
                    new AggregateException(editError, rollbackError));
            }

            throw new InvalidOperationException(
                $"Adding edited ROSpec {rospecId} failed; the original ROSpec was restored.", editError);
        }

        RestoreState(rospecId, originalState);
        return new(rospecId, originalState, before, after, Applied: true);
    }

    private void RestoreState(uint rospecId, ENUM_ROSpecState state)
    {
        if (state is ENUM_ROSpecState.Inactive or ENUM_ROSpecState.Active)
            _reader.EnableRoSpec(rospecId);
        if (state == ENUM_ROSpecState.Active)
            _reader.StartRoSpec(rospecId);
    }

    public void Disconnect() => _reader.Disconnect();
    public void Dispose()
    {
        if (_reader.IsConnected) _reader.Disconnect();
    }
}
