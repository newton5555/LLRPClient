using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace LLRP.Cli.Delivery;

public sealed class SendSettings : ConnectionSettings
{
    [CommandArgument(0, "<OPERATION>")]
    [Description("capabilities, configuration, rospecs, apply-default-settings, enable-rospec, disable-rospec, start-rospec, stop-rospec, delete-rospec, or delete-all-rospecs")]
    public required string Operation { get; init; }

    [CommandOption("--rospec-id <ID>")]
    [Description("ROSpec ID for ROSpec lifecycle requests (0 has the standard delete-all meaning).")]
    [DefaultValue(1u)]
    public uint RospecId { get; init; } = 1;
}

public sealed class SendCommand : AsyncCommand<SendSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, SendSettings settings, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        if (!SettingValidation.TryValidate(settings, out var format, out var error) ||
            !Enum.TryParse<ReaderOperation>(settings.Operation.Replace("-", string.Empty), true, out var operation))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(error.Length > 0 ? error : "Unknown operation. Use `llrp send --help`.")}");
            return 2;
        }

        try
        {
            using var session = new ReaderSession(settings.TimeoutMilliseconds);
            var connectionFrames = session.Connect(settings.Host, settings.Port, settings.Tls);
            FrameOutput.Write(connectionFrames, format);
            var execution = session.Execute(operation, settings.RospecId);
            FrameOutput.WriteExecution(execution, format);
            if (execution.Succeeded) return 0;
            var failure = execution.Error?.Message ?? "Reader returned an unsuccessful LLRP status.";
            AnsiConsole.MarkupLine($"[red]LLRP operation failed:[/] {Markup.Escape(failure)}");
            return 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]LLRP operation failed:[/] {Markup.Escape(ex.Message)}");
            return 1;
        }
    }
}
