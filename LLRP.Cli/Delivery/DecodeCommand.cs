using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace LLRP.Cli.Delivery;

public sealed class DecodeSettings : CommandSettings
{
    [CommandOption("--hex <HEX>")]
    [Description("Complete LLRP frame in hexadecimal; spaces, dashes and 0x prefixes are accepted.")]
    public string? Hex { get; init; }

    [CommandOption("--file <PATH>")]
    [Description("Text file containing one complete LLRP frame in hexadecimal.")]
    public string? File { get; init; }

    [CommandOption("--direction <DIRECTION>")]
    [Description("Frame direction label: tx or rx.")]
    [DefaultValue("rx")]
    public string Direction { get; init; } = "rx";

    [CommandOption("--output <FORMAT>")]
    [Description("Output format: text or json.")]
    [DefaultValue("text")]
    public string Output { get; init; } = "text";
}

public sealed class DecodeCommand : Command<DecodeSettings>
{
    protected override int Execute(CommandContext context, DecodeSettings settings, CancellationToken cancellationToken)
    {
        if ((settings.Hex is null) == (settings.File is null))
        {
            AnsiConsole.MarkupLine("[red]Error:[/] Specify exactly one of --hex or --file.");
            return 2;
        }
        if (!FrameOutput.TryParse(settings.Output, out var format) ||
            !Enum.TryParse<FrameDirection>(settings.Direction, true, out var direction))
        {
            AnsiConsole.MarkupLine("[red]Error:[/] --output must be text or json; --direction must be tx or rx.");
            return 2;
        }
        try
        {
            var input = settings.Hex ?? System.IO.File.ReadAllText(settings.File!);
            var normalized = input.Replace("0x", "", StringComparison.OrdinalIgnoreCase);
            normalized = string.Concat(normalized.Where(Uri.IsHexDigit));
            if (normalized.Length == 0 || normalized.Length % 2 != 0)
                throw new FormatException("Hexadecimal input must contain a non-zero even number of digits.");
            FrameOutput.Write([LlrpFrame.Decode(direction, Convert.FromHexString(normalized))], format);
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Decode failed:[/] {Markup.Escape(ex.Message)}");
            return 1;
        }
    }
}
