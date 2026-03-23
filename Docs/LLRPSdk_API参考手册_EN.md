# LLRPSdk API Reference Manual

## 1. Overview

### 1.1 Introduction

LLRPSdk is a .NET SDK based on the LLRP (Low Level Reader Protocol) standard for controlling LLRP-compliant RFID readers. The SDK provides a clean API interface supporting device connection, tag inventory, memory operations, and GPIO control.

### 1.2 Namespace

```csharp
using LLRPSdk;
```

### 1.3 Quick Start

```csharp
// Create reader instance
var reader = new LlrpReader();

// Connect to device
reader.Connect("192.168.1.100");

// Subscribe to tag reports
reader.TagsReported += (sender, args) =>
{
    foreach (var tag in args.TagReport.Tags)
    {
        Console.WriteLine($"EPC: {tag.Epc}");
    }
};

// Start inventory
reader.Start();

// Stop inventory
reader.Stop();

// Disconnect
reader.Disconnect();
```

---

## 2. Core Classes

### 2.1 LlrpReader Class

The `LlrpReader` class is the main entry point for controlling an LLRP reader.

#### Constructors

| Constructor | Description |
|-------------|-------------|
| `LlrpReader()` | Creates a default instance |
| `LlrpReader(string address, string name)` | Creates an instance with specified address and name |

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsConnected` | bool | Indicates whether connected to a reader |
| `Address` | string | Reader IP address |
| `Name` | string | Reader name |
| `ReaderCapabilities` | FeatureSet | Reader capability information |
| `MessageTimeout` | int | Message timeout in milliseconds (default: 5000) |
| `ConnectTimeout` | int | Connection timeout in milliseconds (default: 5000) |
| `LlrpMessageLogAsXml` | bool | Whether LLRP message logs include XML details |

#### Methods

##### Connection Management

```csharp
// Synchronous connection
void Connect(string address);
void Connect(string address, int port);
void Connect(string address, bool useTLS);
void Connect(string address, int port, bool useTLS);

// Asynchronous connection
void ConnectAsync(string address);
void ConnectAsync(string address, int port);
void ConnectAsync(string address, int port, bool useTLS);

// Disconnect
void Disconnect();
```

##### Inventory Operations

```csharp
// Start inventory
void Start();

// Stop inventory
void Stop();
```

##### Status and Configuration

```csharp
// Query status
Status QueryStatus();

// Query configuration
Settings QuerySettings();
Settings QueryDefaultSettings();

// Apply configuration
void ApplySettings(Settings settings);
void ApplySettingsWithoutFactoryReset(Settings settings);
void ApplyDefaultSettings();
```

##### Device Capabilities

```csharp
// Query device capabilities
FeatureSet QueryFeatureSet();
```

##### GPIO Operations

```csharp
// Set GPO state
void SetGpo(ushort portNumber, bool state);
void SetGpos(ushort[] portNumbers, bool[] states);
```

##### Tag Operation Sequences

```csharp
// Add operation sequence
void AddOpSequence(TagOpSequence sequence);

// Delete operation sequences
void DeleteOpSequence(uint sequenceId);
void DeleteAllOpSequences();

// Check attached data AO status
bool? IsAttachedDataAccessSpecEnabled();
```

##### Event Notification Configuration

```csharp
// Query event notification configuration
List<ReaderEventNotificationState> QueryReaderEventNotifications();
```

#### Events

| Event | Delegate Type | Description |
|-------|---------------|-------------|
| `TagsReported` | TagsReportedHandler | Tag report event |
| `TagOpComplete` | TagOpCompleteHandler | Tag operation complete event |
| `GpiChanged` | GpiChangedHandler | GPI state changed event |
| `AntennaChanged` | AntennaEventHandler | Antenna state changed event |
| `AntennaStarted` | AntennaStartEventHandler | Antenna started event |
| `EndOfCycle` | EndOfCycleEventHandler | Inventory cycle end event |
| `ConnectionLost` | ConnectionLostHandler | Connection lost event |
| `KeepaliveReceived` | KeepaliveHandler | Keepalive received event |
| `ReaderStarted` | ReaderStartedEventHandler | Reader started event |
| `ReaderStopped` | ReaderStoppedEventHandler | Reader stopped event |
| `ReportBufferWarning` | ReportBufferWarningEventHandler | Report buffer warning event |
| `ReportBufferOverflow` | ReportBufferOverflowEventHandler | Report buffer overflow event |
| `ConnectAsyncComplete` | ConnectAsyncCompleteHandler | Async connection complete event |
| `RawFrameReceived` | RawFrameReceivedHandler | Raw frame received event |
| `RawFrameSent` | RawFrameSentHandler | Raw frame sent event |
| `LlrpMessageLogged` | LlrpMessageLoggedHandler | LLRP message log event |
| `ErrorNotification` | ErrorNotificationHandler | Error notification event |
| `DiagnosticsReported` | DiagnosticsReportedHandler | Diagnostics report event |

---

### 2.2 Status Class

The `Status` class contains the current state information of a reader.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsConnected` | bool | Connection status |
| `IsSingulating` | bool | Whether inventory is running |
| `Antennas` | AntennaStatusGroup | Antenna status collection |
| `Gpis` | GpiStatusGroup | GPI status collection |
| `GpoStates` | GpoStatusGroup | GPO status collection |
| `ReaderIdentity` | object | Reader identifier (MAC address) |

#### Methods

```csharp
// Load from XML string
static Status FromXmlString(string xml);

// Load from file
static Status Load(string path);

// Convert to XML string
string ToXmlString();
```

---

### 2.3 Settings Class

The `Settings` class contains reader configuration parameters.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `AutoStart` | AutoStartConfig | Auto-start configuration |
| `AutoStop` | AutoStopConfig | Auto-stop configuration |
| `RfMode` | uint? | RF mode |
| `Session` | ushort | Session number (0-3) |
| `TagPopulationEstimate` | ushort | Tag population estimate |
| `Filters` | FilterSettings | Tag filter settings |
| `Report` | ReportConfig | Report configuration |
| `Antennas` | AntennaConfigGroup | Antenna configuration collection |
| `Gpis` | GpiConfigGroup | GPI configuration collection |
| `Gpos` | GpoConfigGroup | GPO configuration collection |
| `Keepalives` | KeepaliveConfig | Keepalive configuration |
| `HoldReportsOnDisconnect` | bool | Hold reports on disconnect |
| `TxFrequenciesInMhz` | List\<double\> | Transmit frequency list |
| `StartOfAntennaEvent` | bool | Enable antenna start event |
| `EndOfCycleEvent` | bool | Enable cycle end event |
| `AttachedData` | AttachedDataConfig | Attached data configuration |

---

### 2.4 FeatureSet Class

The `FeatureSet` class contains reader capability information.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `ModelNumber` | uint | Model number |
| `ReaderModel` | ReaderModel | Reader model enumeration |
| `DeviceManufacturerNumber` | uint | Manufacturer number |
| `FirmwareVersion` | string | Firmware version |
| `AntennaCount` | uint | Number of antennas |
| `GpiCount` | uint | Number of GPI ports |
| `GpoCount` | uint | Number of GPO ports |
| `MaxOperationSequences` | uint | Maximum operation sequences |
| `MaxOperationsPerSequence` | uint | Maximum operations per sequence |
| `IsTagAccessAvailable` | bool | Tag access support |
| `IsFilteringAvailable` | bool | Filtering support |
| `MaxTagSelectFiltersAllowed` | int | Maximum tag select filters |
| `IsMultiwordBlockWriteAvailable` | bool | Multi-word block write support |
| `IsMultiwordBlockEraseAvailable` | bool | Multi-word block erase support |
| `IsHoppingRegion` | bool | Whether hopping region |
| `TxPowers` | IList\<TxPowerTableEntry\> | Transmit power table |
| `RxSensitivities` | IList\<RxSensitivityTableEntry\> | Receive sensitivity table |
| `TxFrequencies` | IList\<double\> | Transmit frequency list |
| `RfModes` | IList\<uint?\> | RF mode list |

---

### 2.5 TagData Class

The `TagData` class represents tag data.

#### Static Methods

```csharp
// Create from hex string
static TagData FromHexString(string hex);

// Create from byte array
static TagData FromByteArray(byte[] bytes);
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Epc` | string | EPC data (hex string) |
| `Tid` | string | TID data |
| `UserMemory` | string | User memory data |

---

## 3. Connection Management

### 3.1 Connect Method

**Synchronous Connection**:

```csharp
// Connect with default port
reader.Connect("192.168.1.100");

// Connect with specified port
reader.Connect("192.168.1.100", 5084);

// Connect with TLS encryption
reader.Connect("192.168.1.100", true);

// Full parameters
reader.Connect("192.168.1.100", 5085, true, TlsProtocols.Tls12);
```

**Parameters**:

| Parameter | Type | Description |
|-----------|------|-------------|
| address | string | IP address or hostname |
| port | int | Port number (default: 5084, encrypted: 5085) |
| useTLS | bool | Enable TLS encryption |
| tlsProtocol | TlsProtocols | TLS protocol version |

### 3.2 ConnectAsync Method

**Asynchronous Connection**:

```csharp
// Subscribe to connection complete event
reader.ConnectAsyncComplete += (sender, result, errorMessage) =>
{
    if (result == ConnectAsyncResult.Success)
    {
        Console.WriteLine("Connected successfully");
    }
    else
    {
        Console.WriteLine($"Connection failed: {errorMessage}");
    }
};

// Initiate async connection
reader.ConnectAsync("192.168.1.100");
```

### 3.3 Disconnect Method

```csharp
// Disconnect
if (reader.IsConnected)
{
    reader.Disconnect();
}
```

### 3.4 Connection Events

```csharp
// Connection lost event
reader.ConnectionLost += (sender) =>
{
    Console.WriteLine("Connection lost");
};
```

---

## 4. Inventory Operations

### 4.1 Start Method

```csharp
// Start inventory
try
{
    reader.Start();
    Console.WriteLine("Inventory started");
}
catch (LLRPSdkException ex)
{
    Console.WriteLine($"Start failed: {ex.Message}");
}
```

### 4.2 Stop Method

```csharp
// Stop inventory
reader.Stop();
```

### 4.3 TagsReported Event

```csharp
reader.TagsReported += (sender, args) =>
{
    var report = args.TagReport;

    foreach (var tag in report.Tags)
    {
        Console.WriteLine($"EPC: {tag.Epc}");
        Console.WriteLine($"Antenna: {tag.AntennaPortNumber}");
        Console.WriteLine($"RSSI: {tag.PeakRssiInDbm} dBm");
        Console.WriteLine($"Read Count: {tag.TagSeenCount}");
        Console.WriteLine($"Frequency: {tag.ChannelInMhz} MHz");
    }
};
```

### 4.4 TagReport Class

| Property | Type | Description |
|----------|------|-------------|
| `Tags` | IList\<Tag\> | Tag list |

**Tag Class Properties**:

| Property | Type | Description |
|----------|------|-------------|
| `Epc` | string | EPC data |
| `AntennaPortNumber` | ushort | Antenna port |
| `PeakRssiInDbm` | double | RSSI value |
| `ChannelInMhz` | double | Frequency |
| `TagSeenCount` | uint | Read count |
| `FirstSeenTimestampUtc` | DateTime | First read time |
| `LastSeenTimestampUtc` | DateTime | Last read time |
| `PcBits` | ushort | PC bits |
| `Crc` | ushort | CRC value |
| `AccessSpecId` | uint? | AccessSpec ID |
| `Tid` | string | TID data |
| `UserMemory` | string | User memory data |

---

## 5. Tag Operations

### 5.1 TagOpSequence Class

```csharp
var sequence = new TagOpSequence
{
    ExecutionCount = 1,              // Execution count
    State = SequenceState.Active,    // State

    // Target tag (optional, matches all tags if not set)
    TargetTag = new TargetTag
    {
        MemoryBank = MemoryBank.Epc,
        Data = "E20034120123456789012345",
        BitPointer = 32
    },

    // Antenna ID (0 = all antennas)
    AntennaId = 0
};

// Add operation
sequence.Ops.Add(new TagReadOp { ... });

// Submit sequence
reader.AddOpSequence(sequence);
```

### 5.2 TagReadOp Class

```csharp
var readOp = new TagReadOp
{
    MemoryBank = MemoryBank.User,    // Memory bank
    WordPointer = 0,                 // Word pointer
    WordCount = 8,                   // Word count
    AccessPassword = TagData.FromHexString("00000000")
};
```

### 5.3 TagWriteOp Class

```csharp
var writeOp = new TagWriteOp
{
    MemoryBank = MemoryBank.Epc,
    WordPointer = 2,
    Data = TagData.FromHexString("1234567890ABCDEF"),
    AccessPassword = TagData.FromHexString("00000000")
};
```

### 5.4 TagLockOp Class

```csharp
var lockOp = new TagLockOp
{
    LockMemoryBank = LockMemoryBank.User,
    LockType = LockType.Lock,
    AccessPassword = TagData.FromHexString("00000000")
};
```

**LockMemoryBank Enumeration**:

| Value | Description |
|-------|-------------|
| KillPassword | Kill password |
| AccessPassword | Access password |
| Epc | EPC memory bank |
| Tid | TID memory bank |
| User | User memory bank |

**LockType Enumeration**:

| Value | Description |
|-------|-------------|
| Unlock | Unlock |
| Lock | Lock |
| Permaunlock | Permanent unlock |
| Permalock | Permanent lock |

### 5.5 TagKillOp Class

```csharp
var killOp = new TagKillOp
{
    KillPassword = TagData.FromHexString("12345678")
};
```

> **Warning**: Kill operation is irreversible!

### 5.6 TagOpComplete Event

```csharp
reader.TagOpComplete += (sender, args) =>
{
    var report = args.TagOpReport;

    foreach (var result in report.Results)
    {
        Console.WriteLine($"EPC: {result.Epc}");
        Console.WriteLine($"Result: {result.Result}");

        if (result is TagReadOpResult readResult)
        {
            Console.WriteLine($"Read Data: {readResult.Data}");
        }
        else if (result is TagWriteOpResult writeResult)
        {
            Console.WriteLine($"Write Status: {writeResult.Status}");
        }
    }
};
```

---

## 6. Configuration Management

### 6.1 QuerySettings Method

```csharp
var settings = reader.QuerySettings();
Console.WriteLine($"Session: {settings.Session}");
Console.WriteLine($"Power: {settings.Antennas[1].TxPower}");
```

### 6.2 ApplySettings Method

```csharp
var settings = reader.QuerySettings();

// Modify configuration
settings.Session = 2;
settings.TagPopulationEstimate = 32;

// Set antenna power
foreach (var antenna in settings.Antennas)
{
    antenna.MaxTxPower = true;  // Use max power
}

// Apply configuration
reader.ApplySettings(settings);
```

### 6.3 Antenna Configuration

```csharp
var settings = reader.QuerySettings();

// Get specific antenna configuration
var antenna1 = settings.Antennas[1];  // Port numbers start at 1

antenna1.IsEnabled = true;           // Enable antenna
antenna1.MaxTxPower = true;          // Max transmit power
antenna1.MaxRxSensitivity = true;    // Max receive sensitivity
antenna1.TxPower = 30.0;             // Specific TX power (dBm)
antenna1.RxSensitivity = -70.0;      // Specific RX sensitivity (dBm)
```

---

## 7. GPIO Operations

### 7.1 GpiStatus Class

```csharp
var status = reader.QueryStatus();

foreach (GpiStatus gpi in status.Gpis)
{
    Console.WriteLine($"GPI {gpi.PortNumber}: {(gpi.State ? "High" : "Low")}");
}
```

### 7.2 GpoStatus Class

```csharp
// Set GPO state
reader.SetGpo(1, true);  // Set port 1 to high

// Batch set
reader.SetGpos(
    new ushort[] { 1, 2 },
    new bool[] { true, false }
);
```

### 7.3 GpiChanged Event

```csharp
reader.GpiChanged += (sender, args) =>
{
    Console.WriteLine($"GPI {args.PortNumber} state changed: {args.State}");
};
```

---

## 8. Enumerations

### 8.1 MemoryBank

| Value | Description |
|-------|-------------|
| Reserved | Reserved bank (passwords) |
| Epc | EPC memory bank |
| Tid | TID memory bank |
| User | User memory bank |

### 8.2 LockType

| Value | Description |
|-------|-------------|
| Unlock | Unlock |
| Lock | Lock |
| Permaunlock | Permanent unlock |
| Permalock | Permanent lock |

### 8.3 AutoStartMode

| Value | Description |
|-------|-------------|
| None | Manual start |
| GpiTrigger | GPI triggered |
| Periodic | Periodic start |
| UtcTimestamp | Scheduled start |

### 8.4 AutoStopMode

| Value | Description |
|-------|-------------|
| None | Manual stop |
| Duration | Duration-based stop |
| GpiTrigger | GPI triggered stop |

### 8.5 TagFilterMode

| Value | Description |
|-------|-------------|
| None | No filtering |
| OnlyFilter1 | Filter 1 only |
| Filter1AndFilter2 | Filter 1 AND 2 |
| Filter1OrFilter2 | Filter 1 OR 2 |
| UseTagSelectFilters | Use tag select filters |

### 8.6 ReportMode

| Value | Description |
|-------|-------------|
| Individual | Individual reports |
| Continuous | Continuous reports |

---

## 9. Exception Handling

### 9.1 LLRPSdkException Class

```csharp
try
{
    reader.Start();
}
catch (LLRPSdkException ex)
{
    Console.WriteLine($"LLRP Error: {ex.Message}");
}
```

**Common Exceptions**:

| Message | Cause |
|---------|-------|
| You must connect to the reader before starting it. | Start called without connection |
| Timeout. | Connection timeout |
| A reader initiated connection already exists. | Another connection exists |
| No ROSpec found on reader. | No ROSpec on device |

---

## Appendix

### A. Port Reference

| Port | Description |
|------|-------------|
| 5084 | LLRP default port (unencrypted) |
| 5085 | LLRP encrypted port (TLS) |

### B. Manufacturer Numbers

| Number | Manufacturer |
|--------|--------------|
| 25882 | Impinj |
| 161 | Motorola |
| 10642 | Zebra |
| 47706 | Silion |
| 57690 | Seuic |

### C. References

- LLRP Specification: [EPCglobal LLRP 1.1](https://www.gs1.org/standards/epcrfid-epcglobal/llrp-1-1)
- Impinj SDK Documentation: [Impinj Developer Hub](https://developer.impinj.com/)

---

*Document Version: 1.0*
*Last Updated: 2024*
