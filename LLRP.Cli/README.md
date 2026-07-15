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
llrp[192.168.1.100|ready] > rospec list
llrp[192.168.1.100|rospec-disabled] > rospec enable 1
llrp[192.168.1.100|rospec-enabled] > rospec start 1
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

Run `help` or `help rospec` inside the console. Core commands are:

- `connect [host] [port] [--tls] [--timeout-ms <ms>]`
- `disconnect`, `status`, `frames [count]`
- `caps`, `config`
- `rospec list|show|create|edit|enable|disable|start|stop|delete`
- `rospec edit <id> [options]`
- `monitor [seconds]` (`0` means until Ctrl+C/Esc)
- `clear`, `help [topic]`, `quit`

If `connect` is entered without arguments, a validated host/port/TLS/timeout
wizard is shown. Commands that can start or destructively change reader state
require confirmation in the interactive console.

## Reader and ROSpec operations

`caps` issues standard `GET_READER_CAPABILITIES`, and `config` issues standard
`GET_READER_CONFIG(All)`. ROSpec operations are grouped under `rospec`:
`list`, `show`, `create default`, `edit`, `enable`, `disable`, `start`, `stop`,
and `delete`.

`rospec list` issues standard `GET_ROSPECS`. If the list is empty,
`rospec create default` uses the SDK-generated default ROSpec and sends only
`ADD_ROSPEC` followed by `ENABLE_ROSPEC`; it does not reset the reader or
delete other configuration.

## Edit an installed ROSpec

Read the editable values without changing the reader:

```text
rospec edit 1
```

Apply one or more common standard fields:

```text
rospec edit 1 --session 2 --population 64 --stop-ms 30000
rospec edit 1 --report-every 1 --include-antenna on --include-rssi on
```

Supported fields are priority, C1G2 session and tag-population estimate,
ROSpec duration stop trigger, report interval, antenna ID and peak RSSI report
selectors. The editor reads the original ROSpec, preserves unedited standard
and custom parameters, and only replaces it when a value actually changes. An
active ROSpec is stopped and disabled first; after `DELETE_ROSPEC` and
`ADD_ROSPEC`, its previous Disabled, Inactive or Active state is restored. If
the new `ADD_ROSPEC` is rejected, the CLI attempts to restore the original.
Every request and response in this sequence is shown as a semantic tree and raw
Hex.

## Automation commands

The one-shot commands use the same transport, decoder and output path:

```powershell
dotnet run --project LLRP.Cli -- monitor --host 192.168.1.100 --duration-seconds 0
dotnet run --project LLRP.Cli -- decode --hex 04160000000E0000002A00000001
dotnet run --project LLRP.Cli -- decode --file .\captured-frame.txt --output json
```

Text output contains the semantic tree and full offset/ASCII Hex dump. JSON
output writes one object per frame and includes direction, message type/ID,
protocol version, declared/captured lengths, status, summary, Hex and decoder
warning. Invalid arguments return exit code `2`; connection, timeout and reader
operation failures return `1`.

## Build, test and publish

```powershell
dotnet build LLRP.Cli\LLRP.Cli.csproj
dotnet test LLRP.Cli.Tests\LLRP.Cli.Tests.csproj
dotnet publish LLRP.Cli\LLRP.Cli.csproj -c Release
```

The framework-dependent executable is produced as `LLRP.Cli.exe` and can be
run directly on a machine with the matching .NET runtime when kept with its
published DLLs. `dotnet pack` remains optional and produces a .NET Tool package
that must be installed with `dotnet tool install`; the `.nupkg` itself is not an
executable.

The tests include a simulated LLRP transport that emits raw standard request,
successful response and rejected response frames. They verify message-number
mapping, decoding, `LLRPStatus`, Message-ID correlation, workflow transitions,
prompt suggestions, completion, command tokenization and Hex formatting.

Captured frames are bounded to the latest 10,000 entries. A frame that the
bundled LTK decoder cannot fully decode is still retained and shown with its
authoritative header, declared/captured length, known fixed fields, complete
Hex and decoder warning.
