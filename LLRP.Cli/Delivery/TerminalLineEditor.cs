using System.Text;

namespace LLRP.Cli.Delivery;

public sealed record LineReadResult(string? Text, bool Cancelled = false);

public sealed class TerminalLineEditor : IDisposable
{
    private const int MaximumHistoryEntries = 500;
    private readonly List<string> _history;
    private readonly string _historyPath;

    public TerminalLineEditor(string? historyPath = null)
    {
        _historyPath = historyPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LLRP.Cli", "history.txt");
        _history = LoadHistory(_historyPath)
            .Select(SanitizeInput)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .TakeLast(MaximumHistoryEntries)
            .ToList();
    }

    public LineReadResult ReadLine(string prompt, Func<string, int, InputAssist>? assistProvider = null)
    {
        if (Console.IsInputRedirected || Console.IsOutputRedirected)
        {
            Console.Write(prompt);
            return new(Console.ReadLine());
        }

        var previousControlMode = Console.TreatControlCAsInput;
        var hasAssistLine = assistProvider is not null;
        var assistRendered = false;
        try
        {
            Console.TreatControlCAsInput = true;
            var buffer = new StringBuilder();
            var cursor = 0;
            var historyIndex = _history.Count;
            string? pendingLine = null;
            CompletionState? completionState = null;
            var assist = GetAssist(assistProvider, buffer, cursor);
            Redraw(prompt, buffer, cursor, assist, assistRendered, hasAssistLine);
            assistRendered = hasAssistLine;

            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.Key != ConsoleKey.Tab) completionState = null;

                if (key.Key == ConsoleKey.Enter)
                {
                    CommitLine(prompt, buffer, assistRendered);
                    var text = buffer.ToString();
                    Remember(text);
                    return new(text);
                }
                if (key.Key == ConsoleKey.C && key.Modifiers.HasFlag(ConsoleModifiers.Control))
                {
                    ClearEditor(assistRendered);
                    Console.Write(SafeDisplay(prompt));
                    Console.WriteLine("^C");
                    return new(string.Empty, Cancelled: true);
                }
                if (key.Key == ConsoleKey.D && key.Modifiers.HasFlag(ConsoleModifiers.Control))
                {
                    if (buffer.Length == 0)
                    {
                        ClearEditor(assistRendered);
                        Console.WriteLine();
                        return new(null);
                    }
                    DeleteAt(buffer, cursor);
                }
                else if (key.Key == ConsoleKey.Escape)
                {
                    buffer.Clear();
                    cursor = 0;
                }
                else if (key.Key == ConsoleKey.Backspace && cursor > 0)
                {
                    buffer.Remove(--cursor, 1);
                }
                else if (key.Key == ConsoleKey.Delete)
                {
                    DeleteAt(buffer, cursor);
                }
                else if (key.Key == ConsoleKey.LeftArrow)
                {
                    cursor = key.Modifiers.HasFlag(ConsoleModifiers.Control)
                        ? PreviousWord(buffer, cursor)
                        : Math.Max(0, cursor - 1);
                }
                else if (key.Key == ConsoleKey.RightArrow)
                {
                    if (!key.Modifiers.HasFlag(ConsoleModifiers.Control) && cursor == buffer.Length && assist.GhostSuffix.Length > 0)
                        AcceptGhost(buffer, ref cursor, assist);
                    else
                        cursor = key.Modifiers.HasFlag(ConsoleModifiers.Control)
                            ? NextWord(buffer, cursor)
                            : Math.Min(buffer.Length, cursor + 1);
                }
                else if (key.Key == ConsoleKey.Home)
                {
                    cursor = 0;
                }
                else if (key.Key == ConsoleKey.End)
                {
                    if (cursor == buffer.Length && assist.GhostSuffix.Length > 0) AcceptGhost(buffer, ref cursor, assist);
                    else cursor = buffer.Length;
                }
                else if (key.Key == ConsoleKey.UpArrow && _history.Count > 0)
                {
                    if (historyIndex == _history.Count) pendingLine = buffer.ToString();
                    historyIndex = Math.Max(0, historyIndex - 1);
                    ReplaceAll(buffer, _history[historyIndex]);
                    cursor = buffer.Length;
                }
                else if (key.Key == ConsoleKey.DownArrow && _history.Count > 0)
                {
                    historyIndex = Math.Min(_history.Count, historyIndex + 1);
                    ReplaceAll(buffer, historyIndex == _history.Count ? pendingLine ?? string.Empty : _history[historyIndex]);
                    cursor = buffer.Length;
                }
                else if (key.Key == ConsoleKey.Tab && assistProvider is not null)
                {
                    var reverse = key.Modifiers.HasFlag(ConsoleModifiers.Shift);
                    if (!reverse && cursor == buffer.Length && assist.GhostSuffix.Length > 0)
                    {
                        AcceptGhost(buffer, ref cursor, assist);
                        completionState = null;
                    }
                    else
                    {
                        completionState = Complete(buffer, ref cursor, assist.Candidates, completionState, reverse);
                    }
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    buffer.Insert(cursor++, key.KeyChar);
                }

                assist = GetAssist(assistProvider, buffer, cursor);
                Redraw(prompt, buffer, cursor, assist, assistRendered, hasAssistLine);
            }
        }
        finally
        {
            Console.TreatControlCAsInput = previousControlMode;
        }
    }

    public void Dispose() => SaveHistory();

    private static InputAssist GetAssist(Func<string, int, InputAssist>? provider, StringBuilder buffer, int cursor)
    {
        if (provider is null) return InputAssist.Empty;
        try { return provider(buffer.ToString(), cursor); }
        catch { return InputAssist.Empty; }
    }

    private static void AcceptGhost(StringBuilder buffer, ref int cursor, InputAssist assist)
    {
        var suffix = SanitizeInput(assist.GhostSuffix);
        buffer.Insert(cursor, suffix);
        cursor += suffix.Length;
    }

    private static CompletionState? Complete(
        StringBuilder buffer,
        ref int cursor,
        IReadOnlyList<string> candidates,
        CompletionState? state,
        bool reverse)
    {
        if (candidates.Count == 0) return null;
        if (state is not null)
        {
            var delta = reverse ? -1 : 1;
            var index = (state.Index + delta + state.Candidates.Count) % state.Candidates.Count;
            ReplaceRange(buffer, state.TokenStart, cursor, state.Candidates[index]);
            cursor = state.TokenStart + state.Candidates[index].Length;
            return state with { Index = index };
        }

        var tokenStart = cursor;
        while (tokenStart > 0 && !char.IsWhiteSpace(buffer[tokenStart - 1])) tokenStart--;
        if (candidates.Count == 1)
        {
            ReplaceRange(buffer, tokenStart, cursor, candidates[0]);
            cursor = tokenStart + candidates[0].Length;
            return null;
        }

        var common = LongestCommonPrefix(candidates);
        var currentLength = cursor - tokenStart;
        if (!reverse && common.Length > currentLength)
        {
            ReplaceRange(buffer, tokenStart, cursor, common);
            cursor = tokenStart + common.Length;
            return new(candidates, tokenStart, -1);
        }

        var selected = reverse ? candidates.Count - 1 : 0;
        ReplaceRange(buffer, tokenStart, cursor, candidates[selected]);
        cursor = tokenStart + candidates[selected].Length;
        return new(candidates, tokenStart, selected);
    }

    private static string LongestCommonPrefix(IReadOnlyList<string> values)
    {
        var prefix = values[0];
        foreach (var value in values.Skip(1))
        {
            var length = 0;
            while (length < prefix.Length && length < value.Length &&
                   char.ToUpperInvariant(prefix[length]) == char.ToUpperInvariant(value[length])) length++;
            prefix = prefix[..length];
            if (prefix.Length == 0) break;
        }
        return prefix;
    }

    private void Remember(string text)
    {
        text = SanitizeInput(text);
        if (string.IsNullOrWhiteSpace(text)) return;
        if (_history.Count == 0 || !_history[^1].Equals(text, StringComparison.Ordinal)) _history.Add(text);
        if (_history.Count > MaximumHistoryEntries) _history.RemoveRange(0, _history.Count - MaximumHistoryEntries);
    }

    private void SaveHistory()
    {
        try
        {
            var directory = Path.GetDirectoryName(_historyPath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            File.WriteAllLines(_historyPath, _history.TakeLast(MaximumHistoryEntries));
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    private static IEnumerable<string> LoadHistory(string path)
    {
        try { return File.Exists(path) ? File.ReadLines(path).ToArray() : []; }
        catch (IOException) { return []; }
        catch (UnauthorizedAccessException) { return []; }
    }

    private static void Redraw(
        string prompt,
        StringBuilder buffer,
        int cursor,
        InputAssist assist,
        bool clearAssistLine,
        bool renderAssistLine)
    {
        ClearEditor(clearAssistLine);
        var width = TerminalWidth();
        var safePrompt = FitMiddle(SafeDisplay(prompt), Math.Max(10, width / 2));
        var available = Math.Max(8, width - safePrompt.Length - 3);
        var viewport = BuildViewport(buffer.ToString(), cursor, available);
        var ghost = cursor == buffer.Length ? SafeDisplay(assist.GhostSuffix) : string.Empty;
        ghost = FitEnd(ghost, Math.Max(0, available - viewport.Text.Length));

        const string inputMarker = "> ";
        Console.Write(TerminalVisuals.Paint(inputMarker, Spectre.Console.Color.SpringGreen2, decoration: Spectre.Console.Decoration.Bold));
        Console.Write(TerminalVisuals.Paint(safePrompt, Spectre.Console.Color.Aqua, decoration: Spectre.Console.Decoration.Bold));
        Console.Write(viewport.Text);
        if (ghost.Length > 0) Console.Write(TerminalVisuals.Paint(ghost, Spectre.Console.Color.Grey));

        if (renderAssistLine)
        {
            var hint = string.IsNullOrWhiteSpace(assist.Hint)
                ? "Tab/→ accept suggestion · Shift+Tab cycles candidates · Esc clears"
                : SafeDisplay(assist.Hint);
            hint = FitEnd(hint, Math.Max(1, width - 6));
            Console.Write("\n\r\u001b[2K" + TerminalVisuals.Paint("  └─ " + hint, Spectre.Console.Color.Grey));
            Console.Write("\u001b[1A\r");
        }

        var column = inputMarker.Length + safePrompt.Length + viewport.CursorColumn;
        if (column > 0) Console.Write($"\u001b[{column}C");
    }

    private static Viewport BuildViewport(string value, int cursor, int available)
    {
        if (value.Length <= available) return new(value, cursor);
        var contentWidth = Math.Max(1, available - 2);
        var start = Math.Clamp(cursor - contentWidth / 2, 0, Math.Max(0, value.Length - contentWidth));
        if (cursor == value.Length) start = Math.Max(0, value.Length - contentWidth);
        var end = Math.Min(value.Length, start + contentWidth);
        var left = start > 0;
        var right = end < value.Length;
        var text = (left ? "‹" : string.Empty) + value[start..end] + (right ? "›" : string.Empty);
        var cursorColumn = (left ? 1 : 0) + Math.Clamp(cursor - start, 0, end - start);
        return new(text, cursorColumn);
    }

    private static void CommitLine(string prompt, StringBuilder buffer, bool hasAssistLine)
    {
        ClearEditor(hasAssistLine);
        Console.Write("> ");
        Console.Write(SafeDisplay(prompt));
        Console.Write(buffer);
        Console.WriteLine();
    }

    private static void ClearEditor(bool hasAssistLine)
    {
        Console.Write("\r\u001b[2K");
        if (hasAssistLine) Console.Write("\u001b[1B\r\u001b[2K\u001b[1A\r");
    }

    private static string SafeDisplay(string value) => new(value.Where(character => !char.IsControl(character)).ToArray());
    private static string SanitizeInput(string value) => SafeDisplay(value);
    private static string FitEnd(string value, int width) => width <= 0 ? string.Empty : value.Length <= width ? value : width == 1 ? "…" : value[..(width - 1)] + "…";
    private static string FitMiddle(string value, int width)
    {
        if (value.Length <= width) return value;
        if (width <= 3) return value[..width];
        var left = (width - 1) / 2;
        return value[..left] + "…" + value[^(width - left - 1)..];
    }

    private static int TerminalWidth()
    {
        try { return Math.Max(30, Console.WindowWidth); }
        catch (IOException) { return 80; }
        catch (InvalidOperationException) { return 80; }
    }

    private static void DeleteAt(StringBuilder buffer, int cursor) { if (cursor < buffer.Length) buffer.Remove(cursor, 1); }
    private static void ReplaceAll(StringBuilder buffer, string value) { buffer.Clear(); buffer.Append(value); }
    private static void ReplaceRange(StringBuilder buffer, int start, int end, string value) { buffer.Remove(start, end - start); buffer.Insert(start, value); }

    private static int PreviousWord(StringBuilder buffer, int cursor)
    {
        while (cursor > 0 && char.IsWhiteSpace(buffer[cursor - 1])) cursor--;
        while (cursor > 0 && !char.IsWhiteSpace(buffer[cursor - 1])) cursor--;
        return cursor;
    }

    private static int NextWord(StringBuilder buffer, int cursor)
    {
        while (cursor < buffer.Length && !char.IsWhiteSpace(buffer[cursor])) cursor++;
        while (cursor < buffer.Length && char.IsWhiteSpace(buffer[cursor])) cursor++;
        return cursor;
    }

    private sealed record CompletionState(IReadOnlyList<string> Candidates, int TokenStart, int Index);
    private sealed record Viewport(string Text, int CursorColumn);
}
