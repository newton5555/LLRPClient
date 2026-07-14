using Spectre.Console.Cli;

namespace LLRP.Cli.Delivery;

public sealed class ConsoleSettings : CommandSettings { }

public sealed class ConsoleCommand : Command<ConsoleSettings>
{
    protected override int Execute(CommandContext context, ConsoleSettings settings, CancellationToken cancellationToken) =>
        ReplHost.Run(cancellationToken);
}
