# LLRPReaderUI_WPF UI User Manual
## 1. Overview
### 1.1 Software Introduction
LLRPReaderUI_WPF is a professional RFID reader control and testing tool based on the LLRP protocol. It communicates with readers through LLRPSdk and supports device configuration, inventory, memory read/write, and advanced security operations under standard LLRP.

### 1.2 Runtime Environment
- OS: Windows 10 / Windows 11
- Runtime: .NET 9.0 Runtime
- Network: Reader must be reachable on the same subnet or via routing

### 1.3 Feature Summary
| Module | Description |
| --- | --- |
| Device Connection | IP:Port connection, auto-parses device capabilities (model, firmware, antennas, RF params) |
| Settings | Keepalive, Session, Tag Population Estimate, RF Mode, hop tables, state-aware inventory, hold reports on disconnect, per-antenna config, factory reset |
| GPIO | GPI enable & live state, GPO target level set/apply |
| Inventory Config | Auto start/stop, dual filters, tag select filters, report mode (incl. BatchAfterStop) & fields, attached data |
| Inventory | Real-time inventory, duration, stats (reports/tags/unique), column toggle, manual buffered pull |
| Read/Write | EPC/TID target matching, 4 memory banks, optional BlockWrite, 5s timeout |
| Advanced Ops | Lock/Unlock/Permalock/Permaunlock, Kill, BlockErase, 5s timeout |
| Logs | Operation/LLRP/Raw logs, batched UI refresh |
| LLRP Messages | SQLite history, multi-filter query, message tree parse & export |

## 2. Quick Start
### 2.1 Connect Device
1. In **Device Connection**, enter reader IP or IP:Port (supports 3 recent endpoints).
2. Click **Connect**. On success, the status bar shows connection state and MAC. Capabilities load below.
3. If the reader is inventorying, the app stops it before initialization.

### 2.2 One-Click Inventory
1. Navigate to **Inventory**.
2. Click **Start Inventory**.
3. Tags appear in real time; totals show report count, total tags, and unique tags.

## 3. Module Details
### 3.1 Device Connection & Capabilities
After connection, the full FeatureSet is shown:
- Core info: ModelNumber, ReaderModel, manufacturer ID (with name), firmware, antenna/GPI/GPO counts.
- Protocol: CommunicationsStandard, country code (with name), max sequences/ops, tag access and filtering capabilities.
- Memory ops: IsMultiwordBlockWriteAvailable, IsMultiwordBlockEraseAvailable.
- RF: IsHoppingRegion, power levels, sensitivity levels, frequency points, RF modes.

Keepalive timeout protection: if the reader stops responding to keepalive, the app disconnects and shows timeout.

### 3.2 Settings
Core actions: **Get**, **Save**, **Factory Reset**.
- Keepalive: enable/disable and interval (ms).
- RF mode & frequency:
  - RF Mode uses Pn prefix and mode id (e.g., P0-0, P1-2) with detailed params.
  - Hop tables: select HopTable ID; in hopping regions (FCC/ETSI) the reader cycles table frequencies; a single channel index is usually ignored.
  - Channel index: set Channel Index and show current frequency (fixed frequency in non-hopping region).
- Inventory strategy:
  - Session: 0/1/2/3.
  - TagPopulationEstimate affects inventory efficiency.
  - State-Aware: if supported, configure Target (A/B) and Search Mode (SL/Not_SL).
- Disconnect strategy: Hold Reports On Disconnect.
- Antennas: per-port enable, Tx Power (dBm), Rx Sensitivity (dBm).
- Events: displays ReaderEventNotificationSpec enablement.

### 3.3 GPIO
- GPI: per-port enable and current level (High/Low/Unknown), save to apply.
- GPO: per-port target level; apply to set and refresh.

### 3.4 Inventory Config
Core actions: **Read**, **Save**.
- Auto start:
  - None, Immediate, GPI Trigger, Periodic; periodic supports first delay / UTC time / period.
- Auto stop:
  - None, Duration, GPI Trigger, GPI Timeout (Timeout=0 means unlimited).
- Filters:
  - Modes: None, Single, Dual.
  - Standard filters: Filter1/Filter2 by memory bank (EPC/TID/User/Reserved), bit offset/count, mask, Match/NonMatch.
  - Tag Select Filters: dynamic list of Select rules with memory bank, bit offset/count, mask, match/non-match action.
- Report:
  - Modes: Individual, WaitForQuery, BatchAfterStop.
  - Fields: PC, CRC, first/last seen time, antenna port, channel, Peak RSSI, seen count.
- Attached data: read a memory bank (default TID) with word pointer/count (default 6) and access password.

### 3.5 Inventory
- Actions: Start, Stop, Clear, Manual Pull.
- Stats: total reports, total tags (with duplicates), unique tags, and duration.
- Manual Pull: only available when report mode is WaitForQuery.
- Column visibility mirrors report field options.
- Tag fields: receive time, EPC, antenna port, channel frequency (MHz), Peak RSSI (dBm), seen count, PC, CRC, first/last seen time (UTC), attached data.
- Capacity: list shows up to 500 rows (oldest removed).

### 3.6 Read/Write
- Target: match by EPC (bit 32) or TID (bit 0).
- Memory banks: User, TID, Reserved, EPC.
- Read: specify word pointer/count.
- Write: hex data length must be a multiple of 4 (word aligned).
- BlockWrite: available if the reader supports it.
- Access password: 32-bit hex (default 00000000).
- 5s timeout protection.
- Attached data restore: inventory/attached state is saved before ops and restored after.

### 3.7 Advanced Tag Ops
- Target: same as read/write (EPC/TID).
- Functions:
  - Lock: Lock/Unlock/Permalock/Permaunlock for KillPassword, AccessPassword, EPC, TID, User.
  - Kill: 32-bit kill password, 8 hex chars.
  - Block Erase: only if supported.
- Automation: 5s timeout; attached data state saved/restored; clears op sequences.

### 3.8 Logs
- Types: operation, LLRP message, raw frame.
- Toggle display per type.
- Batched UI refresh every 200ms.
- Capacity: 500 rows per log type.

### 3.9 LLRP Message Browser
Loads history from SQLite (`llrp_rawframes.db`).
- Filters: time range, direction (All/RX/TX), message type, device ID, text search, clear filters.
- Parsing: select a record to show message tree and raw hex.
- Actions: Refresh (last 5000), Export (tree to `.txt`, saved to app working directory), Clear (delete all records).

### 3.10 Status Bar
Shows device state, inventory state, antenna state, GPI/GPO state, and MAC. **Refresh Status** triggers manual query.

### 3.11 Theme & Language
- Chinese/English UI switching.
- Light/Dark theme switching.
- Navigation and prompts update immediately after switching.

## 4. FAQ
**Q: Why is Channel Index ineffective in hopping mode?**
A: In hopping regions the reader cycles hop tables; single channel index is usually ignored.

**Q: Advanced or Read/Write shows ¡°Timeout¡±?**
A: Operations have a 5s timeout. Ensure tag is close and filters are not too strict.

**Q: Difference between Receive Time and FirstSeenTime?**
A: Receive time is the PC local time when the report arrived; FirstSeenTime is the reader UTC timestamp.

**Q: Manual Pull button disabled?**
A: Only available when report mode is WaitForQuery.

**Q: BlockWrite / BlockErase disabled?**
A: Depends on reader capability; unsupported devices disable those buttons automatically.

## Appendix
- Local storage:
  - LLRP raw frame DB: `%LocalAppData%\LLRPReaderUI_WPF\llrp_rawframes.db`
  - Recent endpoints: `%LocalAppData%\LLRPReaderUI_WPF\recent-endpoints.json`
  - Log files: written by Serilog
  - Raw frame retention: older than 7 days is purged on startup

- Version:
  - App version: 1.0.15-Alpha-260513
  - Document version: 1.4
  - Last updated: 2026-05-13
