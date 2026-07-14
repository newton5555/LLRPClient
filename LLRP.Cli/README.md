# LLRP CLI

`llrp` is a production-oriented console for standards-compliant LLRP readers.
LLRPSdk builds and sends requests; the CLI independently captures every raw
TX/RX frame, correlates request/response pairs by Message ID, evaluates
`LLRPStatus`, and renders an LLRP message/parameter tree plus an offset Hex dump.
XML is used only as an internal decoder representation and is never shown.

## Interactive console

```powershell
dotnet run --project LLRP.Cli
```

The prompt reflects the observed workflow rather than merely the TCP state:

```text
llrp[offline] > connect 192.168.1.100
llrp[192.168.1.100|ready] > send rospecs
llrp[192.168.1.100|rospec-disabled] > send enable-rospec 1
llrp[192.168.1.100|rospec-enabled] > send start-rospec 1
llrp[192.168.1.100|inventory] > monitor 30
```

After every state-changing result, the console proposes up to two valid next
commands and explains why. The chain is driven by successful reader responses
and queried ROSpec state. Device rejection, timeout, transport loss and invalid
input produce different recovery guidance.

The input line continuously renders a dim inline suggestion and a second-line
hint containing the active command syntax, parameter meaning, candidate values
and current reader/ROSpec reason. Tab or right-arrow at the end accepts the
suggestion; Shift+Tab cycles candidates. Editing also supports Ctrl+left/right
word movement, Home/End, Delete/Backspace, persistent up/down history, Esc to
clear, Ctrl+C to cancel the current line, and Ctrl+D to exit. Long input scrolls
horizontally instead of wrapping the active input row. History is stored under
the user's local application-data directory; credentials are never collected
or stored.

Run `help` or `help send` inside the console. Core commands are:

- `connect [host] [port] [--tls] [--timeout-ms <ms>]`
- `disconnect`, `status`, `frames [count]`
- `send <operation> [rospec-id]`
- `monitor [seconds]` (`0` means until Ctrl+C/Esc)
- `clear`, `help [topic]`, `quit`

If `connect` is entered without arguments, a validated host/port/TLS/timeout
wizard is shown. Commands that can start or destructively change reader state
require confirmation in the interactive console.

## Supported standard operations

`capabilities`, `configuration`, `rospecs`, `apply-default-settings`,
`enable-rospec`, `disable-rospec`, `start-rospec`, `stop-rospec`,
`delete-rospec`, and `delete-all-rospecs`.

`configuration` issues standard `GET_READER_CONFIG(All)` and `rospecs` issues
standard `GET_ROSPECS`. The default-settings operation intentionally uses the
SDK's generated default configuration; no configuration or inventory is
started implicitly on connection.

## Automation commands

The one-shot commands use the same transport, decoder and output path:

```powershell
dotnet run --project LLRP.Cli -- send capabilities --host 192.168.1.100
dotnet run --project LLRP.Cli -- send rospecs --host 192.168.1.100 --output json
dotnet run --project LLRP.Cli -- send start-rospec --rospec-id 1 --host 192.168.1.100
dotnet run --project LLRP.Cli -- monitor --host 192.168.1.100 --duration-seconds 0
dotnet run --project LLRP.Cli -- decode --hex 04160000000E0000002A00000001
dotnet run --project LLRP.Cli -- decode --file .\captured-frame.txt --output json
```

Text output contains the semantic tree and full offset/ASCII Hex dump. JSON
output writes one object per frame and includes direction, message type/ID,
protocol version, declared/captured lengths, status, summary, Hex and decoder
warning. Invalid arguments return exit code `2`; connection, timeout and reader
operation failures return `1`.

## Build, test and package

```powershell
dotnet build LLRP.Cli\LLRP.Cli.csproj
dotnet test LLRP.Cli.Tests\LLRP.Cli.Tests.csproj
dotnet pack LLRP.Cli\LLRP.Cli.csproj -c Release
```

The tests include a simulated LLRP transport that emits raw standard request,
successful response and rejected response frames. They verify message-number
mapping, decoding, `LLRPStatus`, Message-ID correlation, workflow transitions,
prompt suggestions, completion, command tokenization and Hex formatting.

Captured frames are bounded to the latest 10,000 entries. A frame that the
bundled LTK decoder cannot fully decode is still retained and shown with its
authoritative header, declared/captured length, known fixed fields, complete
Hex and decoder warning.
