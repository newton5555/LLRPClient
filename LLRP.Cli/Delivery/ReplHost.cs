using System.Diagnostics;
using System.Net.Sockets;
using LLRPSdk;
using Spectre.Console;

namespace LLRP.Cli.Delivery;

internal static class ReplHost
{
    public static int Run(CancellationToken cancellationToken) => new ReplApplication(cancellationToken).Run();
}

internal sealed class ReplApplication
{
    private readonly CancellationToken _applicationToken;
    private readonly ReaderContext _context = new();
    private readonly TerminalLineEditor _editor = new();
    private ReaderSession? _session;
    private string _lastHost = "127.0.0.1";
    private int _lastPort = 5084;
    private int _lastTimeout = 10_000;

    public ReplApplication(CancellationToken applicationToken) => _applicationToken = applicationToken;

    public int Run()
    {
        PrintBanner();
        try
        {
            while (!_applicationToken.IsCancellationRequested)
            {
                SynchronizeConnectionState();
                var input = _editor.ReadLine(_context.Prompt + " > ", (text, cursor) => CommandCatalog.Assist(text, cursor, _context));
                if (input.Text is null) return 0;
                if (input.Cancelled || string.IsNullOrWhiteSpace(input.Text)) continue;

                var parsed = CommandLineTokenizer.Tokenize(input.Text);
                if (!parsed.Success)
                {
                    PrintError("Syntax", parsed.Error!, "Close the quoted value and retry.");
                    continue;
                }
                if (parsed.Tokens.Count == 0) continue;

                try
                {
                    if (!Dispatch(parsed.Tokens)) return 0;
                }
                catch (OperationCanceledException) when (!_applicationToken.IsCancellationRequested)
                {
                    Info("Cancelled.");
                }
                catch (Exception ex)
                {
                    PrintException(ex, "Check `help` and retry.");
                }
            }
            return 0;
        }
        finally
        {
            _session?.Dispose();
            _editor.Dispose();
        }
    }

    private bool Dispatch(IReadOnlyList<string> tokens)
    {
        var command = CommandCatalog.FindCommand(tokens[0]);
        if (command is null)
        {
            var guesses = CommandCatalog.Commands.Select(item => item.Name)
                .OrderBy(item => EditDistance(item, tokens[0])).Take(2).ToArray();
            PrintError("Unknown command", tokens[0], $"Try: {string.Join(" or ", guesses)}. Use `help` for the full list.");
            return true;
        }
        if (command.RequiresConnection && !IsSessionConnected())
        {
            PrintError("Not connected", $"`{command.Name}` requires a reader session.", "Run `connect <host>` first.");
            ShowNextActions();
            return true;
        }

        switch (command.Name)
        {
            case "connect": Connect(tokens.Skip(1).ToArray()); break;
            case "disconnect": Disconnect(); break;
            case "status": PrintStatus(); break;
            case "send": Send(tokens.Skip(1).ToArray()); break;
            case "monitor": Monitor(tokens.Skip(1).ToArray()); break;
            case "frames": PrintRecentFrames(tokens.Skip(1).ToArray()); break;
            case "clear": Console.Clear(); PrintBanner(); break;
            case "help": PrintHelp(tokens.ElementAtOrDefault(1)); break;
            case "quit": return false;
        }
        return true;
    }

    private void Connect(IReadOnlyList<string> args)
    {
        var options = args.Count == 0 ? PromptConnectionOptions() : ParseConnectionOptions(args);
        _lastHost = options.Host;
        _lastPort = options.Port;
        _lastTimeout = options.TimeoutMilliseconds;

        _session?.Dispose();
        _session = new ReaderSession(options.TimeoutMilliseconds);
        IReadOnlyList<LlrpFrame> frames = [];
        Exception? failure = null;
        RunWithStatus($"Connecting to {options.Host}:{options.Port}…", () =>
        {
            try { frames = _session.Connect(options.Host, options.Port, options.Tls); }
            catch (Exception ex) { failure = ex; }
        });

        if (failure is not null)
        {
            _session.Dispose();
            _session = null;
            _context.ConnectionFailed(options.Host, options.Port, options.Tls, failure.Message);
            PrintException(failure, $"Verify address/port and retry `connect {options.Host} {options.Port}`.");
            return;
        }

        _context.Connected(options.Host, options.Port, options.Tls);
        _context.Observe(frames);
        if (frames.Any(frame => frame.MessageType == "GET_READER_CAPABILITIES_RESPONSE" && frame.IsSuccess != false))
            _context.OperationSucceeded(ReaderOperation.Capabilities, 0);
        Success($"Connected to {options.Host}:{options.Port} over {(options.Tls ? "TLS" : "TCP")}.");
        FrameOutput.Write(frames, OutputFormat.Text);
        ShowNextActions();
    }

    private void Disconnect()
    {
        _session?.Dispose();
        _session = null;
        _context.Disconnected();
        Success("Disconnected.");
        ShowNextActions();
    }

    private void Send(IReadOnlyList<string> args)
    {
        var defaultOperation = SuggestedOperation();
        var operationName = args.ElementAtOrDefault(0) ?? Ask("Operation", defaultOperation);
        var spec = CommandCatalog.FindOperation(operationName) ??
                   throw new ArgumentException($"Unknown operation `{operationName}`. Run `help send`.");
        uint rospecId = 0;
        if (spec.RequiresRospecId)
        {
            var suggestedId = _context.CurrentRospecId ?? _context.KnownRospecIds.FirstOrDefault();
            if (suggestedId == 0) suggestedId = 1;
            var defaultId = suggestedId.ToString();
            var idText = args.ElementAtOrDefault(1) ?? Ask("ROSpec ID", defaultId);
            if (!uint.TryParse(idText, out rospecId) || rospecId == 0)
                throw new ArgumentException("ROSpec ID must be an integer between 1 and 4294967295.");
        }
        if (args.Count > (spec.RequiresRospecId ? 2 : 1))
            throw new ArgumentException($"Too many arguments. Usage: send {spec.Name}{(spec.RequiresRospecId ? " <rospec-id>" : string.Empty)}");
        var preflight = OperationRules.Validate(_context, spec.Operation, rospecId);
        if (!preflight.Allowed)
        {
            PrintError("Invalid reader state", preflight.Message!, preflight.Recovery!);
            ShowNextActions();
            return;
        }
        if (spec.RequiresConfirmation && !Confirm($"{spec.Name} changes reader state. Continue"))
        {
            Info("Cancelled; no message was sent.");
            return;
        }

        OperationExecution? execution = null;
        RunWithStatus($"Sending {spec.Name}…", () => execution = _session!.Execute(spec.Operation, rospecId));
        FrameOutput.WriteExecution(execution!);
        _context.Observe(execution!.Frames);
        if (execution.Succeeded)
        {
            _context.OperationSucceeded(spec.Operation, rospecId);
            Success($"{spec.Name} completed in {execution.Duration.TotalMilliseconds:F1} ms.");
        }
        else
        {
            var error = execution.Error ?? new LLRPSdkException("Reader returned an unsuccessful LLRP status.");
            _context.OperationFailed(error.Message, connectionLost: !_session!.IsConnected);
            PrintException(error, RecoveryFor(spec.Operation, rospecId));
        }
        ShowNextActions();
    }

    private void Monitor(IReadOnlyList<string> args)
    {
        if (args.Count > 1) throw new ArgumentException("Usage: monitor [seconds]");
        var raw = args.ElementAtOrDefault(0) ?? Ask("Duration seconds (0 = until stopped)", "30");
        if (!int.TryParse(raw, out var seconds) || seconds < 0)
            throw new ArgumentException("Monitor duration must be zero or a positive integer.");

        var received = 0L;
        var tags = 0L;
        var connectionLost = false;
        var stopwatch = Stopwatch.StartNew();
        using var stop = CancellationTokenSource.CreateLinkedTokenSource(_applicationToken);
        ConsoleCancelEventHandler cancelHandler = (_, eventArgs) => { eventArgs.Cancel = true; stop.Cancel(); };
        void OnFrame(LlrpFrame frame)
        {
            if (frame.Direction != FrameDirection.Rx) return;
            Interlocked.Increment(ref received);
            if (frame.DecodedMessage is Org.LLRP.LTK.LLRPV1.MSG_RO_ACCESS_REPORT report)
                Interlocked.Add(ref tags, report.TagReportData?.Length ?? 0);
            _context.Observe([frame]);
            FrameOutput.Write([frame], OutputFormat.Text);
        }

        _session!.FrameArrived += OnFrame;
        Console.CancelKeyPress += cancelHandler;
        Info(seconds == 0 ? "Live monitor active — press Ctrl+C or Esc to stop." : $"Live monitor active for {seconds}s — press Ctrl+C or Esc to stop early.");
        try
        {
            while (!stop.IsCancellationRequested && (seconds == 0 || stopwatch.Elapsed < TimeSpan.FromSeconds(seconds)))
            {
                if (!_session.IsConnected)
                {
                    connectionLost = true;
                    stop.Cancel();
                    continue;
                }
                if (TryReadMonitorStopKey()) stop.Cancel();
                Thread.Sleep(50);
            }
        }
        finally
        {
            Console.CancelKeyPress -= cancelHandler;
            _session.FrameArrived -= OnFrame;
            stopwatch.Stop();
        }
        if (connectionLost)
        {
            _context.OperationFailed("Reader transport disconnected during monitoring.", connectionLost: true);
            PrintError("Transport", "Reader disconnected during live monitoring.", $"Retry `connect {_context.Host ?? _lastHost} {_context.Port}`.");
        }
        else Success($"Monitor stopped after {stopwatch.Elapsed.TotalSeconds:F1}s · {received} RX frame(s) · {tags} tag report(s).");
        ShowNextActions();
    }

    private void PrintRecentFrames(IReadOnlyList<string> args)
    {
        if (args.Count > 1) throw new ArgumentException("Usage: frames [count]");
        var raw = args.ElementAtOrDefault(0) ?? "20";
        if (!int.TryParse(raw, out var count) || count is < 1 or > 500)
            throw new ArgumentException("Frame count must be between 1 and 500.");
        var frames = _session?.Frames.TakeLast(count).ToArray() ?? [];
        if (frames.Length == 0) Info("No captured frames.");
        else FrameOutput.Write(frames, OutputFormat.Text);
    }

    private void PrintStatus()
    {
        var table = new Table().Border(TableBorder.Simple).HideHeaders();
        table.AddColumn("Field");
        table.AddColumn("Value");
        table.AddRow("[grey]Transport[/]", _context.IsConnected ? "[green]connected[/]" : "[grey]offline[/]");
        table.AddRow("[grey]Endpoint[/]", _context.Host is null ? "—" : Markup.Escape($"{_context.Host}:{_context.Port} ({(_context.Tls ? "TLS" : "TCP")})"));
        table.AddRow("[grey]Workflow[/]", Markup.Escape(_context.Phase.ToString()));
        table.AddRow("[grey]Captured[/]", $"{_session?.TotalFrameCount ?? 0} frame(s)");
        table.AddRow("[grey]Received[/]", $"{_context.ReceivedFrames} frame(s), {_context.TagReports} tag report(s)");
        var rospecs = _context.RospecStates.Count == 0 ? "not discovered" :
            string.Join(", ", _context.RospecStates.Select(pair => $"{pair.Key}:{pair.Value}"));
        table.AddRow("[grey]ROSpecs[/]", Markup.Escape(rospecs));
        if (_context.LastError is not null) table.AddRow("[red]Last error[/]", Markup.Escape(_context.LastError));
        AnsiConsole.Write(table);
        ShowNextActions();
    }

    private void PrintHelp(string? topic)
    {
        if (topic is not null)
        {
            var command = CommandCatalog.FindCommand(topic);
            if (command is not null)
            {
                AnsiConsole.MarkupLine($"[bold deepskyblue1]{Markup.Escape(command.Usage)}[/]");
                AnsiConsole.MarkupLine(Markup.Escape(command.Description));
                if (command.Name == "send") PrintOperations();
                return;
            }
            var operation = CommandCatalog.FindOperation(topic);
            if (operation is not null)
            {
                AnsiConsole.MarkupLine($"[bold deepskyblue1]send {Markup.Escape(operation.Name)}{(operation.RequiresRospecId ? " <rospec-id>" : string.Empty)}[/]");
                AnsiConsole.MarkupLine(Markup.Escape(operation.Description));
                return;
            }
            PrintError("Unknown help topic", topic, "Run `help` to list all commands.");
            return;
        }

        AnsiConsole.MarkupLine("[bold]Commands[/]");
        var table = new Table().Border(TableBorder.None).AddColumn("Command").AddColumn("Purpose");
        foreach (var command in CommandCatalog.Commands)
            table.AddRow($"[deepskyblue1]{Markup.Escape(command.Usage)}[/]", $"[grey]{Markup.Escape(command.Description)}[/]");
        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("[grey]Editing: ←/→ · Ctrl+←/→ · Home/End · ↑/↓ history · Tab/→ accept suggestion · Shift+Tab cycle · Esc clear · Ctrl+C cancel line · Ctrl+D exit[/]");
        ShowNextActions();
    }

    private static void PrintOperations()
    {
        AnsiConsole.MarkupLine("\n[bold]Operations[/]");
        var table = new Table().Border(TableBorder.None).AddColumn("Name").AddColumn("Purpose");
        foreach (var operation in CommandCatalog.Operations)
            table.AddRow($"[deepskyblue1]{Markup.Escape(operation.Name)}[/]", $"[grey]{Markup.Escape(operation.Description)}[/]");
        AnsiConsole.Write(table);
    }

    private void ShowNextActions()
    {
        var actions = PromptChain.GetNextActions(_context);
        if (actions.Count == 0) return;
        AnsiConsole.MarkupLine("[bold grey70]Next[/]");
        foreach (var action in actions.Take(2))
            AnsiConsole.MarkupLine($"  [deepskyblue1]{Markup.Escape(action.Command)}[/]  [grey]— {Markup.Escape(action.Reason)}[/]");
    }

    private ConnectionOptions PromptConnectionOptions()
    {
        var host = Ask("Host", _lastHost);
        var portText = Ask("Port", _lastPort.ToString());
        if (!int.TryParse(portText, out var port) || port is < 1 or > 65535)
            throw new ArgumentException("Port must be between 1 and 65535.");
        var tls = Confirm("Use TLS", defaultValue: false);
        var timeoutText = Ask("Timeout ms", _lastTimeout.ToString());
        if (!int.TryParse(timeoutText, out var timeout) || timeout <= 0)
            throw new ArgumentException("Timeout must be a positive integer.");
        return new(host, port, tls, timeout);
    }

    private ConnectionOptions ParseConnectionOptions(IReadOnlyList<string> args)
    {
        string? host = null;
        var port = 5084;
        var tls = false;
        var timeout = 10_000;
        for (var index = 0; index < args.Count; index++)
        {
            var value = args[index];
            switch (value.ToLowerInvariant())
            {
                case "--tls": tls = true; break;
                case "--timeout-ms":
                    if (++index >= args.Count || !int.TryParse(args[index], out timeout) || timeout <= 0)
                        throw new ArgumentException("--timeout-ms requires a positive integer.");
                    break;
                default:
                    if (value.StartsWith('-')) throw new ArgumentException($"Unknown connect option `{value}`.");
                    if (host is null) host = value;
                    else if (port == 5084 && int.TryParse(value, out var parsedPort)) port = parsedPort;
                    else throw new ArgumentException("Usage: connect [host] [port] [--tls] [--timeout-ms <ms>]");
                    break;
            }
        }
        if (string.IsNullOrWhiteSpace(host)) throw new ArgumentException("Reader host cannot be empty.");
        if (port is < 1 or > 65535) throw new ArgumentException("Port must be between 1 and 65535.");
        return new(host, port, tls, timeout);
    }

    private string Ask(string label, string fallback)
    {
        var result = _editor.ReadLine($"  {label} [{fallback}] > ");
        if (result.Text is null || result.Cancelled) throw new OperationCanceledException();
        return string.IsNullOrWhiteSpace(result.Text) ? fallback : result.Text.Trim();
    }

    private bool Confirm(string label, bool defaultValue = false)
    {
        var hint = defaultValue ? "Y/n" : "y/N";
        var result = _editor.ReadLine($"  {label} [{hint}] > ");
        if (result.Text is null || result.Cancelled) return false;
        if (string.IsNullOrWhiteSpace(result.Text)) return defaultValue;
        return result.Text.Equals("y", StringComparison.OrdinalIgnoreCase) || result.Text.Equals("yes", StringComparison.OrdinalIgnoreCase);
    }

    private void SynchronizeConnectionState()
    {
        if (_context.IsConnected && _session?.IsConnected != true)
            _context.OperationFailed("The transport connection is no longer active.", connectionLost: true);
    }

    private bool IsSessionConnected() => _session?.IsConnected == true;

    private string SuggestedOperation()
    {
        var command = PromptChain.GetNextActions(_context).Select(action => action.Command)
            .FirstOrDefault(value => value.StartsWith("send ", StringComparison.OrdinalIgnoreCase));
        return command?.Split(' ', StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(1) ?? "capabilities";
    }

    private static string RecoveryFor(ReaderOperation operation, uint id) => operation switch
    {
        ReaderOperation.StartRospec => $"Query `send rospecs`, then enable ROSpec {id} before retrying.",
        ReaderOperation.EnableRospec or ReaderOperation.DisableRospec or ReaderOperation.StopRospec => "Run `send rospecs` to refresh actual reader state.",
        _ => "Run `status`, verify reader state, and retry."
    };

    private static void RunWithStatus(string text, Action action)
    {
        if (Console.IsOutputRedirected) action();
        else AnsiConsole.Status().Spinner(Spinner.Known.Dots).Start(text, _ => action());
    }

    private static bool TryReadMonitorStopKey()
    {
        if (Console.IsInputRedirected) return false;
        try
        {
            if (!Console.KeyAvailable) return false;
            var key = Console.ReadKey(intercept: true);
            return key.Key is ConsoleKey.Escape || key.Key == ConsoleKey.C && key.Modifiers.HasFlag(ConsoleModifiers.Control);
        }
        catch (InvalidOperationException) { return false; }
        catch (IOException) { return false; }
    }

    private static void PrintBanner()
    {
        AnsiConsole.MarkupLine("[bold deepskyblue1]◆ LLRP CLI[/] [grey]2.0.0[/]");
        AnsiConsole.MarkupLine("[grey]  Standard reader console · semantic message tree · raw TX/RX hex[/]");
        AnsiConsole.MarkupLine("[grey]  Type `help`; Tab/→ accepts the live suggestion; Ctrl+C cancels input or monitor.[/]");
        AnsiConsole.Write(new Rule().RuleStyle("grey35"));
    }

    private static void Success(string text) => AnsiConsole.MarkupLine($"[green]✓[/] {Markup.Escape(text)}");
    private static void Info(string text) => AnsiConsole.MarkupLine($"[grey]{Markup.Escape(text)}[/]");

    private static void PrintException(Exception error, string recovery)
    {
        var category = Classify(error);
        PrintError(category, error.Message, recovery);
    }

    private static void PrintError(string category, string message, string recovery)
    {
        AnsiConsole.MarkupLine($"[red]✗ {Markup.Escape(category)}:[/] {Markup.Escape(message)}");
        AnsiConsole.MarkupLine($"  [grey]Recovery: {Markup.Escape(recovery)}[/]");
    }

    private static string Classify(Exception error)
    {
        if (error is TimeoutException || error.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase)) return "Timeout";
        if (error is SocketException or IOException || error.Message.Contains("connect", StringComparison.OrdinalIgnoreCase)) return "Transport";
        if (error is LLRPSdkException && (error.Message.Contains("status", StringComparison.OrdinalIgnoreCase) || error.Message.Contains("error", StringComparison.OrdinalIgnoreCase))) return "Reader rejected request";
        if (error is ArgumentException or FormatException) return "Invalid input";
        return "Operation failed";
    }

    private static int EditDistance(string left, string right)
    {
        var costs = Enumerable.Range(0, right.Length + 1).ToArray();
        for (var i = 1; i <= left.Length; i++)
        {
            var previous = costs[0];
            costs[0] = i;
            for (var j = 1; j <= right.Length; j++)
            {
                var current = costs[j];
                costs[j] = Math.Min(Math.Min(costs[j] + 1, costs[j - 1] + 1), previous + (char.ToUpperInvariant(left[i - 1]) == char.ToUpperInvariant(right[j - 1]) ? 0 : 1));
                previous = current;
            }
        }
        return costs[^1];
    }

    private sealed record ConnectionOptions(string Host, int Port, bool Tls, int TimeoutMilliseconds);
}
