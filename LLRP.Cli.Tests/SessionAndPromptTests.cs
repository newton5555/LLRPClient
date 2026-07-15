using LLRP.Cli.Delivery;
using Org.LLRP.LTK.LLRPV1;
using Org.LLRP.LTK.LLRPV1.DataType;

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
        Assert.Equal("caps", PromptChain.GetNextActions(context)[0].Command);

        context.OperationSucceeded(ReaderOperation.Capabilities, 0);
        Assert.Equal("rospec list", PromptChain.GetNextActions(context)[0].Command);

        context.OperationSucceeded(ReaderOperation.Rospecs, 0);
        Assert.Equal("rospec create default", PromptChain.GetNextActions(context)[0].Command);

        context.OperationSucceeded(ReaderOperation.CreateDefaultRospec, 0);
        Assert.Equal("rospec start 1", PromptChain.GetNextActions(context)[0].Command);

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
        Assert.Contains("caps", CommandCatalog.Complete("ca", 2, context));
        Assert.Contains("start", CommandCatalog.Complete("rospec st", 9, context));

        context.OperationSucceeded(ReaderOperation.CreateDefaultRospec, 0);
        Assert.Contains("1", CommandCatalog.Complete("rospec start ", 13, context));
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
        Assert.Contains("rospec enable 1", result.Recovery);
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
        Assert.Equal("rospec list", suggestedQuery.GhostSuffix);
        Assert.Contains("discover installed ROSpecs", suggestedQuery.Hint);

        ready.OperationSucceeded(ReaderOperation.ApplyDefaultSettings, 0);
        var start = CommandCatalog.Assist("rospec st", 9, ready);
        Assert.Equal("art 1", start.GhostSuffix);
        Assert.Contains("rospec", start.Hint);
    }

    [Fact]
    public void RospecCompletionSuggestsSubcommandKnownIdOptionsAndValues()
    {
        var context = new ReaderContext();
        context.Connected("reader", 5084, false);
        context.Observe([RospecResponseFrame(CreateRospec(7))]);

        Assert.Contains("edit", CommandCatalog.Complete("rospec ", 7, context));
        Assert.Contains("default", CommandCatalog.Complete("rospec create ", 15, context));
        Assert.Contains("7", CommandCatalog.Complete("rospec edit ", 12, context));
        Assert.Contains("--session", CommandCatalog.Complete("rospec edit 7 --s", 18, context));
        Assert.Contains("2", CommandCatalog.Complete("rospec edit 7 --session 2", 25, context));
    }

    [Fact]
    public void RospecEditorClonesAndAppliesCommonFieldsWithoutChangingOriginal()
    {
        var original = CreateRospec(7);
        var edited = RospecEditor.Clone(original);

        var after = RospecEditor.Apply(edited, new(
            Priority: 3,
            Session: 2,
            TagPopulation: 64,
            StopAfterMilliseconds: 30_000,
            ReportEvery: 10,
            IncludeAntennaId: false,
            IncludePeakRssi: true));

        Assert.Equal((byte)3, after.Priority);
        Assert.Equal((ushort)2, after.Session);
        Assert.Equal((ushort)64, after.TagPopulation);
        Assert.Equal(ENUM_ROSpecStopTriggerType.Duration, after.StopTrigger);
        Assert.Equal((uint)30_000, after.StopAfterMilliseconds);
        Assert.Equal((ushort)10, after.ReportEvery);
        Assert.False(after.IncludeAntennaId);
        Assert.True(after.IncludePeakRssi);

        var unchanged = RospecEditor.Read(original);
        Assert.Equal((ushort)1, unchanged.Session);
        Assert.Equal((ushort)32, unchanged.TagPopulation);
        Assert.Equal(ENUM_ROSpecStopTriggerType.Null, unchanged.StopTrigger);
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

    private static LlrpFrame RospecResponseFrame(PARAM_ROSpec roSpec)
    {
        var response = new MSG_GET_ROSPECS_RESPONSE
        {
            ROSpec = [roSpec],
            LLRPStatus = new PARAM_LLRPStatus { StatusCode = ENUM_StatusCode.M_Success }
        };
        return new(DateTimeOffset.UtcNow, FrameDirection.Rx, [], "GET_ROSPECS_RESPONSE", response.MSG_ID,
            "simulated", string.Empty, null, response, StatusCode: "M_Success");
    }

    private static PARAM_ROSpec CreateRospec(uint id)
    {
        var inventoryCommand = new PARAM_C1G2InventoryCommand
        {
            C1G2SingulationControl = new PARAM_C1G2SingulationControl
            {
                Session = new TwoBits(1),
                TagPopulation = 32
            }
        };
        var antenna = new PARAM_AntennaConfiguration { AntennaID = 1 };
        antenna.AirProtocolInventoryCommandSettings.Add(inventoryCommand);
        var aiSpec = new PARAM_AISpec
        {
            AISpecStopTrigger = new PARAM_AISpecStopTrigger
            {
                AISpecStopTriggerType = ENUM_AISpecStopTriggerType.Null
            },
            InventoryParameterSpec =
            [
                new PARAM_InventoryParameterSpec
                {
                    InventoryParameterSpecID = 1,
                    ProtocolID = ENUM_AirProtocols.EPCGlobalClass1Gen2,
                    AntennaConfiguration = [antenna]
                }
            ]
        };
        var roSpec = new PARAM_ROSpec
        {
            ROSpecID = id,
            CurrentState = ENUM_ROSpecState.Inactive,
            ROBoundarySpec = new PARAM_ROBoundarySpec
            {
                ROSpecStartTrigger = new PARAM_ROSpecStartTrigger
                {
                    ROSpecStartTriggerType = ENUM_ROSpecStartTriggerType.Null
                },
                ROSpecStopTrigger = new PARAM_ROSpecStopTrigger
                {
                    ROSpecStopTriggerType = ENUM_ROSpecStopTriggerType.Null
                }
            },
            ROReportSpec = new PARAM_ROReportSpec
            {
                ROReportTrigger = ENUM_ROReportTriggerType.Upon_N_Tags_Or_End_Of_ROSpec,
                N = 1,
                TagReportContentSelector = new PARAM_TagReportContentSelector
                {
                    EnableAntennaID = true,
                    EnablePeakRSSI = false
                }
            }
        };
        roSpec.SpecParameter.Add(aiSpec);
        return roSpec;
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

        public RospecEditResult EditRospec(uint rospecId, RospecEditPatch patch, int timeoutMilliseconds) =>
            throw new NotSupportedException();

        public void Disconnect() => IsConnected = false;
        public void Dispose() => IsConnected = false;
    }
}
