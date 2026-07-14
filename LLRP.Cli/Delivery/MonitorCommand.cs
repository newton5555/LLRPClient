using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace LLRP.Cli.Delivery;

public sealed class MonitorSettings : ConnectionSettings
{
    [CommandOption("--duration-seconds <SECONDS>")]
    [Description("How long to monitor. 0 runs until Ctrl+C.")]
    [DefaultValue(30)]
    public int DurationSeconds { get; init; } = 30;
}

public sealed class MonitorCommand : AsyncCommand<MonitorSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, MonitorSettings settings, CancellationToken cancellationToken)
    {
        if (!SettingValidation.TryValidate(settings, out var format, out var error) || settings.DurationSeconds < 0)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(error.Length > 0 ? error : "--duration-seconds cannot be negative.")}");
            return 2;
        }

        try
        {
            using var session = new ReaderSession(settings.TimeoutMilliseconds);
            session.FrameArrived += frame => FrameOutput.Write([frame], format);
            session.Connect(settings.Host, settings.Port, settings.Tls);
            using var ctrlC = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            ConsoleCancelEventHandler handler = (_, e) => { e.Cancel = true; ctrlC.Cancel(); };
            Console.CancelKeyPress += handler;
            try
            {
                var started = DateTimeOffset.UtcNow;
                while (!ctrlC.IsCancellationRequested &&
                       (settings.DurationSeconds == 0 || DateTimeOffset.UtcNow - started < TimeSpan.FromSeconds(settings.DurationSeconds)))
                {
                    if (!session.IsConnected) throw new IOException("Reader transport disconnected during monitoring.");
                    await Task.Delay(100, ctrlC.Token);
                }
            }
            catch (OperationCanceledException) when (ctrlC.IsCancellationRequested) { }
            finally { Console.CancelKeyPress -= handler; }
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]LLRP monitor failed:[/] {Markup.Escape(ex.Message)}");
            return 1;
        }
    }
}
