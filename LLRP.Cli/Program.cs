using LLRP.Cli.Delivery;
using Spectre.Console.Cli;

using System.Text;

namespace LLRP.Cli;

public static class Program
{
    public static int Main(string[] args)
    {
        Console.InputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        var app = new CommandApp();
        app.SetDefaultCommand<ConsoleCommand>()
            .WithDescription("Launch the interactive LLRP command REPL.");
        app.Configure(config =>
        {
            config.SetApplicationName("llrp");
            config.UseStrictParsing();
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
