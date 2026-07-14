using System.Text;
using System.Text.Json;
using Spectre.Console;

namespace LLRP.Cli.Delivery;

public enum OutputFormat { Text, Json }

public static class FrameOutput
{
    private static readonly object OutputLock = new();

    public static bool TryParse(string value, out OutputFormat format) => Enum.TryParse(value, true, out format);

    public static void Write(IEnumerable<LlrpFrame> frames, OutputFormat format)
    {
        lock (OutputLock)
        {
            foreach (var frame in frames)
            {
                if (format == OutputFormat.Json) WriteJson(frame);
                else WriteText(frame);
            }
        }
    }

    public static void WriteExecution(OperationExecution execution, OutputFormat format = OutputFormat.Text)
    {
        Write(execution.Frames, format);
        if (format == OutputFormat.Json) return;
        WriteTransactions(execution.Transactions);
    }

    public static void WriteExecution(RospecEditExecution execution, OutputFormat format = OutputFormat.Text)
    {
        Write(execution.Frames, format);
        if (format == OutputFormat.Json) return;
        WriteTransactions(execution.Transactions);
    }

    private static void WriteTransactions(IEnumerable<LlrpTransaction> transactions)
    {
        lock (OutputLock)
        {
            foreach (var transaction in transactions)
            {
                var state = transaction.Response is null ? "[yellow]NO RESPONSE[/]" :
                    transaction.Response.IsSuccess == false ? "[red]REJECTED[/]" : "[green]SUCCESS[/]";
                var elapsed = transaction.Duration is null ? "—" : $"{transaction.Duration.Value.TotalMilliseconds:F1} ms";
                AnsiConsole.MarkupLine($"  [grey]↳ transaction[/] {state}  [grey]ID {transaction.Request.MessageId} · {Markup.Escape(elapsed)}[/]");
            }
        }
    }

    public static string HexDump(byte[] bytes)
    {
        var builder = new StringBuilder();
        for (var offset = 0; offset < bytes.Length; offset += 16)
        {
            var count = Math.Min(16, bytes.Length - offset);
            builder.Append($"{offset:X4}  ");
            for (var index = 0; index < 16; index++)
            {
                if (index < count) builder.Append($"{bytes[offset + index]:X2} ");
                else builder.Append("   ");
                if (index == 7) builder.Append(' ');
            }
            builder.Append(" | ");
            for (var index = 0; index < count; index++)
            {
                var value = bytes[offset + index];
                builder.Append(value is >= 32 and <= 126 ? (char)value : '.');
            }
            builder.AppendLine();
        }
        return builder.ToString().TrimEnd();
    }

    private static void WriteText(LlrpFrame frame)
    {
        var tx = frame.Direction == FrameDirection.Tx;
        var color = tx ? "deepskyblue1" : "springgreen2";
        var arrow = tx ? "→ TX" : "← RX";
        var status = frame.IsSuccess switch
        {
            true => " [green]SUCCESS[/]",
            false => " [red]REJECTED[/]",
            _ => string.Empty
        };
        AnsiConsole.MarkupLine($"[{color} bold]{arrow}[/]  [bold]{Markup.Escape(frame.MessageType)}[/]{status}  [grey]ID {frame.MessageId?.ToString() ?? "—"} · {frame.Bytes.Length} bytes · {frame.Timestamp:HH:mm:ss.fff} UTC[/]");

        if (frame.SemanticTree is not null)
        {
            var tree = new Spectre.Console.Tree($"[bold]{Markup.Escape(frame.SemanticTree.Name)}[/]")
                .Style(new Style(Color.Grey70))
                .Guide(TreeGuide.Line);
            var remaining = 350;
            foreach (var child in frame.SemanticTree.Items)
                AddNode(tree, child, ref remaining);
            AnsiConsole.Write(tree);
        }
        else
        {
            AnsiConsole.MarkupLine($"  [grey]{Markup.Escape(frame.Summary)}[/]");
        }

        var hexPanel = new Panel(new Text(HexDump(frame.Bytes), new Style(Color.Grey85)))
            .Header("[grey] RAW HEX [/]")
            .Border(BoxBorder.Rounded)
            .BorderStyle(new Style(tx ? Color.Cyan1 : Color.SpringGreen2))
            .Padding(1, 0);
        AnsiConsole.Write(hexPanel);
        if (frame.DecodeError is not null)
            AnsiConsole.MarkupLine($"[yellow]Decoder warning:[/] {Markup.Escape(frame.DecodeError)}");
        Console.WriteLine();
    }

    private static void AddNode(IHasTreeNodes parent, LlrpSemanticNode node, ref int remaining)
    {
        if (remaining-- <= 0) return;
        var label = node.Value is null
            ? $"[deepskyblue1]{Markup.Escape(node.Name)}[/]"
            : $"[grey70]{Markup.Escape(node.Name)}:[/] [white]{Markup.Escape(node.Value)}[/]";
        var branch = parent.AddNode(label);
        foreach (var child in node.Items)
        {
            if (remaining <= 0)
            {
                branch.AddNode("[grey]… additional fields omitted from terminal view[/]");
                break;
            }
            AddNode(branch, child, ref remaining);
        }
    }

    private static void WriteJson(LlrpFrame frame)
    {
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            timestamp = frame.Timestamp,
            direction = frame.Direction.ToString().ToUpperInvariant(),
            type = frame.MessageType,
            messageId = frame.MessageId,
            version = frame.ProtocolVersion,
            declaredLength = frame.DeclaredLength,
            capturedLength = frame.Bytes.Length,
            statusCode = frame.StatusCode,
            statusDescription = frame.StatusDescription,
            hex = frame.Hex,
            summary = frame.Summary,
            decodeError = frame.DecodeError
        }));
    }
}
