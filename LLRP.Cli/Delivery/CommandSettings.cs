using System.ComponentModel;
using Spectre.Console.Cli;

namespace LLRP.Cli.Delivery;

public abstract class ConnectionSettings : CommandSettings
{
    [CommandOption("--host <HOST>")]
    [Description("Reader host name or IP address.")]
    [DefaultValue("127.0.0.1")]
    public string Host { get; init; } = "127.0.0.1";

    [CommandOption("-p|--port <PORT>")]
    [Description("LLRP TCP port.")]
    [DefaultValue(5084)]
    public int Port { get; init; } = 5084;

    [CommandOption("--tls")]
    [Description("Use the LLRP TLS port and TLS transport.")]
    public bool Tls { get; init; }

    [CommandOption("--timeout-ms <MILLISECONDS>")]
    [Description("Connection and request timeout in milliseconds.")]
    [DefaultValue(10000)]
    public int TimeoutMilliseconds { get; init; } = 10000;

    [CommandOption("--output <FORMAT>")]
    [Description("Frame output format: text or json.")]
    [DefaultValue("text")]
    public string Output { get; init; } = "text";
}

internal static class SettingValidation
{
    public static bool TryValidate(ConnectionSettings settings, out OutputFormat format, out string error)
    {
        if (string.IsNullOrWhiteSpace(settings.Host)) { format = default; error = "--host cannot be empty."; return false; }
        if (settings.Port is < 1 or > 65535) { format = default; error = "--port must be between 1 and 65535."; return false; }
        if (settings.TimeoutMilliseconds <= 0) { format = default; error = "--timeout-ms must be positive."; return false; }
        if (!FrameOutput.TryParse(settings.Output, out format)) { error = "--output must be text or json."; return false; }
        error = string.Empty;
        return true;
    }
}
