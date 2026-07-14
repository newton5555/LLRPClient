using LLRP.Cli.Delivery;
using Spectre.Console.Cli;

namespace LLRP.Cli;

public static class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandApp();
        app.SetDefaultCommand<ConsoleCommand>()
            .WithDescription("Launch the interactive LLRP command REPL.");
        app.Configure(config =>
        {
            config.SetApplicationName("llrp");
            config.UseStrictParsing();
            config.AddCommand<SendCommand>("send")
                .WithDescription("Send a standard LLRP request through LLRPSdk and display every TX/RX frame.");
            config.AddCommand<MonitorCommand>("monitor")
                .WithDescription("Connect to a reader and continuously decode received LLRP frames.");
            config.AddCommand<DecodeCommand>("decode")
                .WithDescription("Decode one captured LLRP frame from hexadecimal text or a file.");
            config.AddCommand<ConsoleCommand>("console")
                .WithDescription("Launch the interactive LLRP command REPL.");
        });
        return app.Run(args);
    }
}
