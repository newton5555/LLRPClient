using Spectre.Console;

namespace LLRP.Cli.Delivery;

/// <summary>Small ANSI-safe rendering helpers shared by the interactive screens.</summary>
internal static class TerminalVisuals
{
    public static string Paint(string value, Color foreground, Color? background = null, Decoration? decoration = null)
    {
        using var writer = new StringWriter();
        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.Yes,
            ColorSystem = ColorSystemSupport.Standard,
            Out = new AnsiConsoleOutput(writer)
        });
        console.Write(new Text(value, new Style(foreground, background, decoration)));
        return writer.ToString();
    }
}
