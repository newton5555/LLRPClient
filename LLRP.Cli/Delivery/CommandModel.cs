using System.Text;

namespace LLRP.Cli.Delivery;

public sealed record CommandSpec(string Name, string Usage, string Description, bool RequiresConnection = false, params string[] Aliases);

public sealed record OperationSpec(
    string Name,
    ReaderOperation Operation,
    string Description,
    bool RequiresRospecId = false,
    bool RequiresConfirmation = false);

public sealed record InputAssist(IReadOnlyList<string> Candidates, string GhostSuffix, string Hint)
{
    public static InputAssist Empty { get; } = new([], string.Empty, string.Empty);
}

public static class CommandCatalog
{
    public static IReadOnlyList<CommandSpec> Commands { get; } =
    [
        new("connect", "connect [host] [port] [--tls] [--timeout-ms <ms>]", "Connect and negotiate with an LLRP reader."),
        new("disconnect", "disconnect", "Close or clear the current reader connection."),
        new("status", "status", "Show connection, workflow, ROSpec and frame state."),
        new("send", "send <operation> [rospec-id]", "Build through LLRPSdk, send, correlate and decode a standard request.", true),
        new("rospec", "rospec edit <id> [options]", "Inspect or edit common fields of an installed ROSpec.", true),
        new("monitor", "monitor [seconds]", "Stream received frames immediately; 0 runs until Ctrl+C.", true),
        new("frames", "frames [count]", "Show the most recent captured frames."),
        new("clear", "clear", "Clear the terminal."),
        new("help", "help [command|operation]", "Show context-aware command help.", Aliases: ["?"]),
        new("quit", "quit", "Exit the console.", Aliases: ["exit", "q"])
    ];

    public static IReadOnlyList<OperationSpec> Operations { get; } =
    [
        new("capabilities", ReaderOperation.Capabilities, "Query standard reader capabilities."),
        new("configuration", ReaderOperation.Configuration, "Query the complete standard reader configuration."),
        new("rospecs", ReaderOperation.Rospecs, "Query installed ROSpecs and their actual states."),
        new("apply-default-settings", ReaderOperation.ApplyDefaultSettings, "Apply SDK-generated default settings and ROSpec.", RequiresConfirmation: true),
        new("enable-rospec", ReaderOperation.EnableRospec, "Enable one ROSpec.", RequiresRospecId: true),
        new("disable-rospec", ReaderOperation.DisableRospec, "Disable one ROSpec.", RequiresRospecId: true),
        new("start-rospec", ReaderOperation.StartRospec, "Start inventory for one enabled ROSpec.", RequiresRospecId: true, RequiresConfirmation: true),
        new("stop-rospec", ReaderOperation.StopRospec, "Stop one active ROSpec.", RequiresRospecId: true),
        new("delete-rospec", ReaderOperation.DeleteRospec, "Delete one ROSpec.", RequiresRospecId: true, RequiresConfirmation: true),
        new("delete-all-rospecs", ReaderOperation.DeleteAllRospecs, "Delete every ROSpec (standard ID 0 semantics).", RequiresConfirmation: true)
    ];

    public static CommandSpec? FindCommand(string value) => Commands.FirstOrDefault(command =>
        command.Name.Equals(value, StringComparison.OrdinalIgnoreCase) ||
        command.Aliases.Contains(value, StringComparer.OrdinalIgnoreCase));

    public static OperationSpec? FindOperation(string value) => Operations.FirstOrDefault(operation =>
        operation.Name.Equals(value, StringComparison.OrdinalIgnoreCase));

    public static InputAssist Assist(string text, int cursor, ReaderContext context)
    {
        cursor = Math.Clamp(cursor, 0, text.Length);
        var candidates = Complete(text, cursor, context);
        var prefix = text[..cursor];
        var parsed = CommandLineTokenizer.TokenizeForCompletion(prefix);
        var ghost = cursor == text.Length ? BuildGhost(prefix, parsed.CurrentToken, candidates, context) : string.Empty;
        return new(candidates, ghost, BuildHint(parsed, candidates, context));
    }

    public static IReadOnlyList<string> Complete(string text, int cursor, ReaderContext context)
    {
        var prefixText = text[..Math.Clamp(cursor, 0, text.Length)];
        var parsed = CommandLineTokenizer.TokenizeForCompletion(prefixText);
        var token = parsed.CurrentToken;
        IEnumerable<string> candidates;
        if (parsed.Tokens.Count <= 1 && !parsed.EndsWithSeparator)
        {
            candidates = PrioritizedCommands(context)
                .Where(command => !command.RequiresConnection || context.IsConnected)
                .Where(command => context.IsConnected || command.Name != "disconnect")
                .Select(command => command.Name);
        }
        else
        {
            var command = parsed.Tokens.FirstOrDefault()?.ToLowerInvariant();
            candidates = command switch
            {
                "send" => CompleteSend(parsed, context),
                "rospec" => CompleteRospec(parsed, context),
                "help" or "?" => Commands.Select(item => item.Name).Concat(Operations.Select(item => item.Name)),
                "monitor" => ["0", "10", "30", "60"],
                "frames" => ["10", "20", "50", "100"],
                _ => []
            };
        }
        return candidates.Where(candidate => candidate.StartsWith(token, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static IEnumerable<string> CompleteSend(CompletionParse parsed, ReaderContext context)
    {
        if (parsed.Tokens.Count <= 2 && !parsed.EndsWithSeparator)
            return PrioritizedOperations(context);
        if (parsed.Tokens.Count == 1 && parsed.EndsWithSeparator)
            return PrioritizedOperations(context);
        var operationName = parsed.Tokens.ElementAtOrDefault(1);
        var operation = operationName is null ? null : FindOperation(operationName);
        if (operation?.RequiresRospecId == true)
            return context.KnownRospecIds.Select(id => id.ToString()).DefaultIfEmpty("1");
        return [];
    }

    private static IEnumerable<string> CompleteRospec(CompletionParse parsed, ReaderContext context)
    {
        if (parsed.Tokens.Count == 1 && parsed.EndsWithSeparator ||
            parsed.Tokens.Count == 2 && !parsed.EndsWithSeparator)
            return ["edit"];
        if (!parsed.Tokens.ElementAtOrDefault(1).Equals("edit", StringComparison.OrdinalIgnoreCase))
            return [];
        if (parsed.Tokens.Count == 2 && parsed.EndsWithSeparator ||
            parsed.Tokens.Count == 3 && !parsed.EndsWithSeparator)
            return context.KnownRospecIds.Select(id => id.ToString()).DefaultIfEmpty("1");

        var valueOption = parsed.EndsWithSeparator
            ? parsed.Tokens.LastOrDefault()
            : parsed.Tokens.ElementAtOrDefault(parsed.Tokens.Count - 2);
        return valueOption?.ToLowerInvariant() switch
        {
            "--session" => ["0", "1", "2", "3"],
            "--population" => ["32", "64", "128", "256"],
            "--stop-ms" => ["0", "1000", "10000", "30000"],
            "--report-every" => ["0", "1", "10", "100"],
            "--include-antenna" or "--include-rssi" => ["on", "off"],
            _ => ["--priority", "--session", "--population", "--stop-ms", "--report-every", "--include-antenna", "--include-rssi"]
        };
    }

    private static IEnumerable<string> PrioritizedOperations(ReaderContext context)
    {
        var recommended = PromptChain.GetNextActions(context).Select(action => action.Command)
            .Where(command => command.StartsWith("send ", StringComparison.OrdinalIgnoreCase))
            .Select(command => command.Split(' ', StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(1))
            .OfType<string>();
        return recommended.Concat(Operations.Select(operation => operation.Name)).Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static IEnumerable<CommandSpec> PrioritizedCommands(ReaderContext context)
    {
        var recommendedNames = PromptChain.GetNextActions(context)
            .Select(action => action.Command.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault())
            .OfType<string>();
        var recommended = recommendedNames.Select(FindCommand).OfType<CommandSpec>();
        return recommended.Concat(Commands).DistinctBy(command => command.Name, StringComparer.OrdinalIgnoreCase);
    }

    private static string BuildGhost(
        string prefix,
        string currentToken,
        IReadOnlyList<string> candidates,
        ReaderContext context)
    {
        var contextualLine = PromptChain.GetNextActions(context)
            .Select(action => action.Command)
            .FirstOrDefault(command => !command.Contains('<') && command.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        if (contextualLine is not null && contextualLine.Length > prefix.Length)
            return contextualLine[prefix.Length..];

        var candidate = candidates.FirstOrDefault();
        return candidate is not null && candidate.Length > currentToken.Length
            ? candidate[currentToken.Length..]
            : string.Empty;
    }

    private static string BuildHint(CompletionParse parsed, IReadOnlyList<string> candidates, ReaderContext context)
    {
        if (parsed.Tokens.Count == 0)
        {
            var next = PromptChain.GetNextActions(context).FirstOrDefault();
            return next is null
                ? "Type `help` to list commands."
                : $"Next: {next.Command} — {next.Reason}  ·  Tab/→ accept";
        }

        var commandText = parsed.Tokens[0];
        var command = FindCommand(commandText);
        if (command is null)
        {
            var match = Commands.FirstOrDefault(item => item.Name.StartsWith(commandText, StringComparison.OrdinalIgnoreCase));
            return match is null
                ? $"Unknown command `{commandText}`. Type `help` for the command list."
                : $"{match.Usage} — {match.Description}";
        }

        if (command.Name != "send")
            return $"{command.Usage} — {command.Description}" + CandidateSuffix(candidates);

        var operationText = parsed.Tokens.ElementAtOrDefault(1);
        if (operationText is null)
        {
            var suggested = candidates.FirstOrDefault() ?? "capabilities";
            return $"send <operation> [rospec-id] — suggested now: {suggested}" + CandidateSuffix(candidates);
        }

        var operation = FindOperation(operationText) ??
                        Operations.FirstOrDefault(item => item.Name.StartsWith(operationText, StringComparison.OrdinalIgnoreCase));
        if (operation is null)
            return $"Unknown operation `{operationText}`. Type `help send` for supported operations.";

        var usage = $"send {operation.Name}" + (operation.RequiresRospecId ? " <rospec-id>" : string.Empty);
        var state = RospecStateHint(parsed, operation, context);
        return $"{usage} — {operation.Description}{state}" + CandidateSuffix(candidates);
    }

    private static string RospecStateHint(CompletionParse parsed, OperationSpec operation, ReaderContext context)
    {
        if (!operation.RequiresRospecId) return string.Empty;
        if (uint.TryParse(parsed.Tokens.ElementAtOrDefault(2), out var id) && context.RospecStates.TryGetValue(id, out var state))
            return $"  ·  ROSpec {id}: {state}";
        return context.KnownRospecIds.Count == 0
            ? "  ·  ROSpec state not discovered"
            : $"  ·  known IDs: {string.Join(", ", context.KnownRospecIds)}";
    }

    private static string CandidateSuffix(IReadOnlyList<string> candidates) => candidates.Count > 1
        ? $"  ·  options: {string.Join("  ", candidates.Take(5))}"
        : string.Empty;
}

public sealed record CommandParseResult(IReadOnlyList<string> Tokens, string? Error)
{
    public bool Success => Error is null;
}

public sealed record CompletionParse(IReadOnlyList<string> Tokens, string CurrentToken, bool EndsWithSeparator);

public static class CommandLineTokenizer
{
    public static CommandParseResult Tokenize(string input)
    {
        var tokens = new List<string>();
        var current = new StringBuilder();
        char? quote = null;
        var escaping = false;
        foreach (var character in input)
        {
            if (escaping)
            {
                current.Append(character);
                escaping = false;
                continue;
            }
            if (character == '\\')
            {
                escaping = true;
                continue;
            }
            if (quote is not null)
            {
                if (character == quote) quote = null;
                else current.Append(character);
                continue;
            }
            if (character is '\'' or '"')
            {
                quote = character;
                continue;
            }
            if (char.IsWhiteSpace(character))
            {
                Flush(tokens, current);
                continue;
            }
            current.Append(character);
        }
        if (escaping) current.Append('\\');
        if (quote is not null) return new(tokens, $"Missing closing {quote} quote.");
        Flush(tokens, current);
        return new(tokens, null);
    }

    public static CompletionParse TokenizeForCompletion(string input)
    {
        var parsed = Tokenize(input);
        var tokens = parsed.Tokens.ToList();
        var separator = input.Length > 0 && char.IsWhiteSpace(input[^1]);
        var current = separator ? string.Empty : tokens.LastOrDefault() ?? string.Empty;
        return new(tokens, current, separator);
    }

    private static void Flush(ICollection<string> tokens, StringBuilder current)
    {
        if (current.Length == 0) return;
        tokens.Add(current.ToString());
        current.Clear();
    }
}
