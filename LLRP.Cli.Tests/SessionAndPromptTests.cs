using LLRP.Cli.Delivery;
using Org.LLRP.LTK.LLRPV1;

namespace LLRP.Cli.Tests;

public sealed class SessionAndPromptTests
{
    [Fact]
    public void SimulatedTransportCapturesAndCorrelatesSuccessfulExchange()
    {
        using var transport = new SimulatedTransport();
        using var session = new ReaderSession(1000, transport);
        session.Connect("simulator", 5084, false);

        var execution = session.Execute(ReaderOperation.StartRospec, 1);

        Assert.True(execution.Succeeded);
        Assert.Equal(2, execution.Frames.Count);
        var transaction = Assert.Single(execution.Transactions);
        Assert.Equal("START_ROSPEC", transaction.Request.MessageType);
        Assert.Equal("START_ROSPEC_RESPONSE", transaction.Response?.MessageType);
        Assert.True(transaction.Succeeded);
    }

    [Fact]
    public void SimulatedReaderRejectionPreservesFramesAndError()
    {
        using var transport = new SimulatedTransport { RejectRequest = true };
        using var session = new ReaderSession(1000, transport);
        session.Connect("simulator", 5084, false);

        var execution = session.Execute(ReaderOperation.StartRospec, 1);

        Assert.False(execution.Succeeded);
        Assert.NotNull(execution.Error);
        Assert.Equal("R_DeviceError", execution.Frames.Last().StatusCode);
        Assert.False(execution.Transactions.Single().Succeeded);
    }

    [Fact]
    public void PromptChainFollowsReaderWorkflow()
    {
        var context = new ReaderContext();
        Assert.StartsWith("connect", PromptChain.GetNextActions(context)[0].Command);

        context.Connected("reader", 5084, false);
        Assert.Equal("send capabilities", PromptChain.GetNextActions(context)[0].Command);

        context.OperationSucceeded(ReaderOperation.Capabilities, 0);
        Assert.Equal("send rospecs", PromptChain.GetNextActions(context)[0].Command);

        context.OperationSucceeded(ReaderOperation.ApplyDefaultSettings, 0);
        Assert.Equal("send start-rospec 1", PromptChain.GetNextActions(context)[0].Command);

        context.OperationSucceeded(ReaderOperation.StartRospec, 1);
        Assert.Equal("monitor 30", PromptChain.GetNextActions(context)[0].Command);
        Assert.Contains("inventory", context.Prompt);
    }

    [Fact]
    public void CompletionIsContextAndArgumentAware()
    {
        var context = new ReaderContext();
        Assert.DoesNotContain("send", CommandCatalog.Complete("s", 1, context));

        context.Connected("reader", 5084, false);
        context.OperationSucceeded(ReaderOperation.Capabilities, 0);
        Assert.Contains("send", CommandCatalog.Complete("s", 1, context));
        Assert.Contains("start-rospec", CommandCatalog.Complete("send st", 7, context));

        context.OperationSucceeded(ReaderOperation.ApplyDefaultSettings, 0);
        Assert.Contains("1", CommandCatalog.Complete("send start-rospec ", 18, context));
    }

    [Fact]
    public void ObservedGetRospecsResponseOverridesAssumedState()
    {
        var response = new MSG_GET_ROSPECS_RESPONSE
        {
            ROSpec = [new PARAM_ROSpec { ROSpecID = 7, CurrentState = ENUM_ROSpecState.Active }],
            LLRPStatus = new PARAM_LLRPStatus { StatusCode = ENUM_StatusCode.M_Success }
        };
        var frame = new LlrpFrame(
            DateTimeOffset.UtcNow, FrameDirection.Rx, [], "GET_ROSPECS_RESPONSE", response.MSG_ID,
            "simulated", string.Empty, null, response, StatusCode: "M_Success");
        var context = new ReaderContext();
        context.Connected("reader", 5084, false);

        context.Observe([frame]);

        Assert.Equal(ReaderWorkflowPhase.InventoryActive, context.Phase);
        Assert.Equal((uint)7, context.CurrentRospecId);
        Assert.Equal("monitor 30", PromptChain.GetNextActions(context)[0].Command);
    }

    [Fact]
    public void PreflightUsesKnownRospecStateButPermitsUnknownState()
    {
        var context = new ReaderContext();
        context.Connected("reader", 5084, false);
        context.OperationSucceeded(ReaderOperation.ApplyDefaultSettings, 0);

        Assert.True(OperationRules.Validate(context, ReaderOperation.StartRospec, 1).Allowed);
        Assert.False(OperationRules.Validate(context, ReaderOperation.StopRospec, 1).Allowed);
        Assert.True(OperationRules.Validate(context, ReaderOperation.StartRospec, 99).Allowed);

        context.OperationSucceeded(ReaderOperation.DisableRospec, 1);
        var result = OperationRules.Validate(context, ReaderOperation.StartRospec, 1);
        Assert.False(result.Allowed);
        Assert.Contains("enable-rospec 1", result.Recovery);
    }

    [Fact]
    public void InputAssistProvidesInlineContextualSuggestionAndSyntaxHint()
    {
        var offline = new ReaderContext();
        var connect = CommandCatalog.Assist("con", 3, offline);
        Assert.Equal("nect", connect.GhostSuffix);
        Assert.Contains("connect [host]", connect.Hint);

        var ready = new ReaderContext();
        ready.Connected("reader", 5084, false);
        ready.OperationSucceeded(ReaderOperation.Capabilities, 0);
        var suggestedQuery = CommandCatalog.Assist(string.Empty, 0, ready);
        Assert.Equal("send rospecs", suggestedQuery.GhostSuffix);
        Assert.Contains("discover installed ROSpecs", suggestedQuery.Hint);

        ready.OperationSucceeded(ReaderOperation.ApplyDefaultSettings, 0);
        var start = CommandCatalog.Assist("send st", 7, ready);
        Assert.Equal("art-rospec 1", start.GhostSuffix);
        Assert.Contains("Start inventory", start.Hint);
    }

    [Theory]
    [InlineData("connect \"reader lab\" 5084", "reader lab")]
    [InlineData("connect reader\\ lab 5084", "reader lab")]
    public void TokenizerSupportsQuotedAndEscapedArguments(string input, string host)
    {
        var result = CommandLineTokenizer.Tokenize(input);

        Assert.True(result.Success);
        Assert.Equal(host, result.Tokens[1]);
    }

    private sealed class SimulatedTransport : IReaderTransport
    {
        public bool RejectRequest { get; init; }
        public bool IsConnected { get; private set; }
        public string? Address { get; private set; }
        public event Action<byte[]>? RawFrameSent;
        public event Action<byte[]>? RawFrameReceived;

        public void Connect(string host, int port, bool tls, int timeoutMilliseconds)
        {
            Address = host;
            IsConnected = true;
        }

        public void Execute(ReaderOperation operation, uint rospecId, int timeoutMilliseconds)
        {
            Assert.Equal(ReaderOperation.StartRospec, operation);
            RawFrameSent?.Invoke(Convert.FromHexString("04160000000E0000002A00000001"));
            RawFrameReceived?.Invoke(Convert.FromHexString(RejectRequest
                ? "0420000000120000002A011F000801910000"
                : "0420000000120000002A011F000800000000"));
            if (RejectRequest) throw new InvalidOperationException("Reader returned R_DeviceError.");
        }

        public void Disconnect() => IsConnected = false;
        public void Dispose() => IsConnected = false;
    }
}
