# LLRPSdk API 参考手册

## 1. 概述

### 1.1 简介

LLRPSdk 是一个基于 LLRP (Low Level Reader Protocol) 标准的 .NET SDK，用于控制符合 LLRP 协议的 RFID 读写器。SDK 提供简洁的 API 接口，支持设备连接、标签盘点、内存操作和 GPIO 控制等功能。

### 1.2 命名空间

```csharp
using LLRPSdk;
```

### 1.3 快速开始

```csharp
// 创建读写器实例
var reader = new LlrpReader();

// 连接设备
reader.Connect("192.168.1.100");

// 订阅标签报告事件
reader.TagsReported += (sender, args) =>
{
    foreach (var tag in args.TagReport.Tags)
    {
        Console.WriteLine($"EPC: {tag.Epc}");
    }
};

// 开始盘点
reader.Start();

// 停止盘点
reader.Stop();

// 断开连接
reader.Disconnect();
```

---

## 2. 核心类

### 2.1 LlrpReader 类

`LlrpReader` 类是控制 LLRP 读写器的主要入口点。

#### 构造函数

| 构造函数 | 说明 |
|----------|------|
| `LlrpReader()` | 创建默认实例 |
| `LlrpReader(string address, string name)` | 创建带有指定地址和名称的实例 |

#### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `IsConnected` | bool | 是否已连接到读写器 |
| `Address` | string | 读写器 IP 地址 |
| `Name` | string | 读写器名称 |
| `ReaderCapabilities` | FeatureSet | 读写器能力信息 |
| `MessageTimeout` | int | 消息超时时间（毫秒），默认 5000 |
| `ConnectTimeout` | int | 连接超时时间（毫秒），默认 5000 |
| `LlrpMessageLogAsXml` | bool | LLRP 消息日志是否包含 XML 详情 |

#### 方法

##### 连接管理

```csharp
// 同步连接
void Connect(string address);
void Connect(string address, int port);
void Connect(string address, bool useTLS);
void Connect(string address, int port, bool useTLS);

// 异步连接
void ConnectAsync(string address);
void ConnectAsync(string address, int port);
void ConnectAsync(string address, int port, bool useTLS);

// 断开连接
void Disconnect();
```

##### 盘点操作

```csharp
// 开始盘点
void Start();

// 停止盘点
void Stop();
```

##### 状态与配置

```csharp
// 查询状态
Status QueryStatus();

// 查询配置
Settings QuerySettings();
Settings QueryDefaultSettings();

// 应用配置
void ApplySettings(Settings settings);
void ApplySettingsWithoutFactoryReset(Settings settings);
void ApplyDefaultSettings();
```

##### 设备能力

```csharp
// 查询设备能力
FeatureSet QueryFeatureSet();
```

##### GPIO 操作

```csharp
// 设置 GPO 状态
void SetGpo(ushort portNumber, bool state);
void SetGpos(ushort[] portNumbers, bool[] states);
```

##### 标签操作序列

```csharp
// 添加操作序列
void AddOpSequence(TagOpSequence sequence);

// 删除操作序列
void DeleteOpSequence(uint sequenceId);
void DeleteAllOpSequences();

// 检查附加数据 AO 状态
bool? IsAttachedDataAccessSpecEnabled();
```

##### 事件通知配置

```csharp
// 查询事件通知配置
List<ReaderEventNotificationState> QueryReaderEventNotifications();
```

#### 事件

| 事件 | 委托类型 | 说明 |
|------|----------|------|
| `TagsReported` | TagsReportedHandler | 标签报告事件 |
| `TagOpComplete` | TagOpCompleteHandler | 标签操作完成事件 |
| `GpiChanged` | GpiChangedHandler | GPI 状态变化事件 |
| `AntennaChanged` | AntennaEventHandler | 天线状态变化事件 |
| `AntennaStarted` | AntennaStartEventHandler | 天线启动事件 |
| `EndOfCycle` | EndOfCycleEventHandler | 盘点周期结束事件 |
| `ConnectionLost` | ConnectionLostHandler | 连接丢失事件 |
| `KeepaliveReceived` | KeepaliveHandler | 心跳接收事件 |
| `ReaderStarted` | ReaderStartedEventHandler | 读写器启动事件 |
| `ReaderStopped` | ReaderStoppedEventHandler | 读写器停止事件 |
| `ReportBufferWarning` | ReportBufferWarningEventHandler | 报告缓冲区警告事件 |
| `ReportBufferOverflow` | ReportBufferOverflowEventHandler | 报告缓冲区溢出事件 |
| `ConnectAsyncComplete` | ConnectAsyncCompleteHandler | 异步连接完成事件 |
| `RawFrameReceived` | RawFrameReceivedHandler | 原始帧接收事件 |
| `RawFrameSent` | RawFrameSentHandler | 原始帧发送事件 |
| `LlrpMessageLogged` | LlrpMessageLoggedHandler | LLRP 消息日志事件 |
| `ErrorNotification` | ErrorNotificationHandler | 错误通知事件 |
| `DiagnosticsReported` | DiagnosticsReportedHandler | 诊断报告事件 |

---

### 2.2 Status 类

`Status` 类包含读写器的当前状态信息。

#### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `IsConnected` | bool | 连接状态 |
| `IsSingulating` | bool | 是否正在盘点 |
| `Antennas` | AntennaStatusGroup | 天线状态集合 |
| `Gpis` | GpiStatusGroup | GPI 状态集合 |
| `GpoStates` | GpoStatusGroup | GPO 状态集合 |
| `ReaderIdentity` | object | 读写器标识（MAC 地址） |

#### 方法

```csharp
// 从 XML 字符串加载
static Status FromXmlString(string xml);

// 从文件加载
static Status Load(string path);

// 转换为 XML 字符串
string ToXmlString();
```

---

### 2.3 Settings 类

`Settings` 类包含读写器配置参数。

#### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `AutoStart` | AutoStartConfig | 自动启动配置 |
| `AutoStop` | AutoStopConfig | 自动停止配置 |
| `RfMode` | uint? | 射频模式 |
| `Session` | ushort | 会话编号（0-3） |
| `TagPopulationEstimate` | ushort | 标签数量估算 |
| `Filters` | FilterSettings | 标签过滤设置 |
| `Report` | ReportConfig | 报告配置 |
| `Antennas` | AntennaConfigGroup | 天线配置集合 |
| `Gpis` | GpiConfigGroup | GPI 配置集合 |
| `Gpos` | GpoConfigGroup | GPO 配置集合 |
| `Keepalives` | KeepaliveConfig | 心跳配置 |
| `HoldReportsOnDisconnect` | bool | 断开连接时保持报告 |
| `TxFrequenciesInMhz` | List\<double\> | 发射频率列表 |
| `StartOfAntennaEvent` | bool | 启用天线启动事件 |
| `EndOfCycleEvent` | bool | 启用周期结束事件 |
| `AttachedData` | AttachedDataConfig | 附加数据配置 |

---

### 2.4 FeatureSet 类

`FeatureSet` 类包含读写器能力信息。

#### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `ModelNumber` | uint | 型号编号 |
| `ReaderModel` | ReaderModel | 读写器型号枚举 |
| `DeviceManufacturerNumber` | uint | 制造商编号 |
| `FirmwareVersion` | string | 固件版本 |
| `AntennaCount` | uint | 天线数量 |
| `GpiCount` | uint | GPI 端口数量 |
| `GpoCount` | uint | GPO 端口数量 |
| `MaxOperationSequences` | uint | 最大操作序列数 |
| `MaxOperationsPerSequence` | uint | 每个序列最大操作数 |
| `IsTagAccessAvailable` | bool | 是否支持标签访问 |
| `IsFilteringAvailable` | bool | 是否支持过滤 |
| `MaxTagSelectFiltersAllowed` | int | 最大标签选择过滤器数 |
| `IsMultiwordBlockWriteAvailable` | bool | 是否支持多字块写 |
| `IsMultiwordBlockEraseAvailable` | bool | 是否支持多字块擦除 |
| `IsHoppingRegion` | bool | 是否为跳频区域 |
| `TxPowers` | IList\<TxPowerTableEntry\> | 发射功率表 |
| `RxSensitivities` | IList\<RxSensitivityTableEntry\> | 接收灵敏度表 |
| `TxFrequencies` | IList\<double\> | 发射频率列表 |
| `RfModes` | IList\<uint?\> | 射频模式列表 |

---

### 2.5 TagData 类

`TagData` 类表示标签数据。

#### 静态方法

```csharp
// 从十六进制字符串创建
static TagData FromHexString(string hex);

// 从字节数组创建
static TagData FromByteArray(byte[] bytes);
```

#### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `Epc` | string | EPC 数据（十六进制字符串） |
| `Tid` | string | TID 数据 |
| `UserMemory` | string | 用户内存数据 |

---

## 3. 连接管理

### 3.1 Connect 方法

**同步连接**：

```csharp
// 使用默认端口连接
reader.Connect("192.168.1.100");

// 指定端口连接
reader.Connect("192.168.1.100", 5084);

// 使用 TLS 加密连接
reader.Connect("192.168.1.100", true);

// 完整参数
reader.Connect("192.168.1.100", 5085, true, TlsProtocols.Tls12);
```

**参数说明**：

| 参数 | 类型 | 说明 |
|------|------|------|
| address | string | IP 地址或主机名 |
| port | int | 端口号（默认 5084，加密 5085） |
| useTLS | bool | 是否启用 TLS 加密 |
| tlsProtocol | TlsProtocols | TLS 协议版本 |

### 3.2 ConnectAsync 方法

**异步连接**：

```csharp
// 订阅连接完成事件
reader.ConnectAsyncComplete += (sender, result, errorMessage) =>
{
    if (result == ConnectAsyncResult.Success)
    {
        Console.WriteLine("连接成功");
    }
    else
    {
        Console.WriteLine($"连接失败: {errorMessage}");
    }
};

// 发起异步连接
reader.ConnectAsync("192.168.1.100");
```

### 3.3 Disconnect 方法

```csharp
// 断开连接
if (reader.IsConnected)
{
    reader.Disconnect();
}
```

### 3.4 连接事件

```csharp
// 连接丢失事件
reader.ConnectionLost += (sender) =>
{
    Console.WriteLine("连接已断开");
};
```

---

## 4. 盘点操作

### 4.1 Start 方法

```csharp
// 开始盘点
try
{
    reader.Start();
    Console.WriteLine("盘点已开始");
}
catch (LLRPSdkException ex)
{
    Console.WriteLine($"启动失败: {ex.Message}");
}
```

### 4.2 Stop 方法

```csharp
// 停止盘点
reader.Stop();
```

### 4.3 TagsReported 事件

```csharp
reader.TagsReported += (sender, args) =>
{
    var report = args.TagReport;

    foreach (var tag in report.Tags)
    {
        Console.WriteLine($"EPC: {tag.Epc}");
        Console.WriteLine($"天线: {tag.AntennaPortNumber}");
        Console.WriteLine($"RSSI: {tag.PeakRssiInDbm} dBm");
        Console.WriteLine($"读取次数: {tag.TagSeenCount}");
        Console.WriteLine($"频率: {tag.ChannelInMhz} MHz");
    }
};
```

### 4.4 TagReport 类

| 属性 | 类型 | 说明 |
|------|------|------|
| `Tags` | IList\<Tag\> | 标签列表 |

**Tag 类属性**：

| 属性 | 类型 | 说明 |
|------|------|------|
| `Epc` | string | EPC 数据 |
| `AntennaPortNumber` | ushort | 天线端口 |
| `PeakRssiInDbm` | double | RSSI 值 |
| `ChannelInMhz` | double | 频率 |
| `TagSeenCount` | uint | 读取次数 |
| `FirstSeenTimestampUtc` | DateTime | 首次读取时间 |
| `LastSeenTimestampUtc` | DateTime | 最后读取时间 |
| `PcBits` | ushort | PC 位 |
| `Crc` | ushort | CRC 值 |
| `AccessSpecId` | uint? | AccessSpec ID |
| `Tid` | string | TID 数据 |
| `UserMemory` | string | 用户内存数据 |

---

## 5. 标签操作

### 5.1 TagOpSequence 类

```csharp
var sequence = new TagOpSequence
{
    ExecutionCount = 1,              // 执行次数
    State = SequenceState.Active,    // 状态

    // 目标标签（可选，不设置则匹配所有标签）
    TargetTag = new TargetTag
    {
        MemoryBank = MemoryBank.Epc,
        Data = "E20034120123456789012345",
        BitPointer = 32
    },

    // 天线 ID（0 = 所有天线）
    AntennaId = 0
};

// 添加操作
sequence.Ops.Add(new TagReadOp { ... });

// 提交序列
reader.AddOpSequence(sequence);
```

### 5.2 TagReadOp 类

```csharp
var readOp = new TagReadOp
{
    MemoryBank = MemoryBank.User,    // 内存区
    WordPointer = 0,                 // 字指针
    WordCount = 8,                   // 字数
    AccessPassword = TagData.FromHexString("00000000")
};
```

### 5.3 TagWriteOp 类

```csharp
var writeOp = new TagWriteOp
{
    MemoryBank = MemoryBank.Epc,
    WordPointer = 2,
    Data = TagData.FromHexString("1234567890ABCDEF"),
    AccessPassword = TagData.FromHexString("00000000")
};
```

### 5.4 TagLockOp 类

```csharp
var lockOp = new TagLockOp
{
    LockMemoryBank = LockMemoryBank.User,
    LockType = LockType.Lock,
    AccessPassword = TagData.FromHexString("00000000")
};
```

**LockMemoryBank 枚举**：

| 值 | 说明 |
|------|------|
| KillPassword | 销毁密码 |
| AccessPassword | 访问密码 |
| Epc | EPC 内存区 |
| Tid | TID 内存区 |
| User | 用户内存区 |

**LockType 枚举**：

| 值 | 说明 |
|------|------|
| Unlock | 解锁 |
| Lock | 锁定 |
| Permaunlock | 永久解锁 |
| Permalock | 永久锁定 |

### 5.5 TagKillOp 类

```csharp
var killOp = new TagKillOp
{
    KillPassword = TagData.FromHexString("12345678")
};
```

> **警告**：销毁操作不可逆，请谨慎使用！

### 5.6 TagOpComplete 事件

```csharp
reader.TagOpComplete += (sender, args) =>
{
    var report = args.TagOpReport;

    foreach (var result in report.Results)
    {
        Console.WriteLine($"EPC: {result.Epc}");
        Console.WriteLine($"结果: {result.Result}");

        if (result is TagReadOpResult readResult)
        {
            Console.WriteLine($"读取数据: {readResult.Data}");
        }
        else if (result is TagWriteOpResult writeResult)
        {
            Console.WriteLine($"写入状态: {writeResult.Status}");
        }
    }
};
```

---

## 6. 配置管理

### 6.1 QuerySettings 方法

```csharp
var settings = reader.QuerySettings();
Console.WriteLine($"Session: {settings.Session}");
Console.WriteLine($"功率: {settings.Antennas[1].TxPower}");
```

### 6.2 ApplySettings 方法

```csharp
var settings = reader.QuerySettings();

// 修改配置
settings.Session = 2;
settings.TagPopulationEstimate = 32;

// 设置天线功率
foreach (var antenna in settings.Antennas)
{
    antenna.MaxTxPower = true;  // 使用最大功率
}

// 应用配置
reader.ApplySettings(settings);
```

### 6.3 天线配置

```csharp
var settings = reader.QuerySettings();

// 获取指定天线配置
var antenna1 = settings.Antennas[1];  // 端口号从 1 开始

antenna1.IsEnabled = true;           // 启用天线
antenna1.MaxTxPower = true;          // 最大发射功率
antenna1.MaxRxSensitivity = true;    // 最大接收灵敏度
antenna1.TxPower = 30.0;             // 指定发射功率 (dBm)
antenna1.RxSensitivity = -70.0;      // 指定接收灵敏度 (dBm)
```

---

## 7. GPIO 操作

### 7.1 GpiStatus 类

```csharp
var status = reader.QueryStatus();

foreach (GpiStatus gpi in status.Gpis)
{
    Console.WriteLine($"GPI {gpi.PortNumber}: {(gpi.State ? "高电平" : "低电平")}");
}
```

### 7.2 GpoStatus 类

```csharp
// 设置 GPO 状态
reader.SetGpo(1, true);  // 设置端口 1 为高电平

// 批量设置
reader.SetGpos(
    new ushort[] { 1, 2 },
    new bool[] { true, false }
);
```

### 7.3 GpiChanged 事件

```csharp
reader.GpiChanged += (sender, args) =>
{
    Console.WriteLine($"GPI {args.PortNumber} 状态变化: {args.State}");
};
```

---

## 8. 枚举类型

### 8.1 MemoryBank

| 值 | 说明 |
|------|------|
| Reserved | 保留区（密码） |
| Epc | EPC 内存区 |
| Tid | TID 内存区 |
| User | 用户内存区 |

### 8.2 LockType

| 值 | 说明 |
|------|------|
| Unlock | 解锁 |
| Lock | 锁定 |
| Permaunlock | 永久解锁 |
| Permalock | 永久锁定 |

### 8.3 AutoStartMode

| 值 | 说明 |
|------|------|
| None | 手动启动 |
| GpiTrigger | GPI 触发 |
| Periodic | 周期启动 |
| UtcTimestamp | 定时启动 |

### 8.4 AutoStopMode

| 值 | 说明 |
|------|------|
| None | 手动停止 |
| Duration | 定时停止 |
| GpiTrigger | GPI 触发停止 |

### 8.5 TagFilterMode

| 值 | 说明 |
|------|------|
| None | 无过滤 |
| OnlyFilter1 | 仅过滤器 1 |
| Filter1AndFilter2 | 过滤器 1 和 2 |
| Filter1OrFilter2 | 过滤器 1 或 2 |
| UseTagSelectFilters | 使用标签选择过滤器 |

### 8.6 ReportMode

| 值 | 说明 |
|------|------|
| Individual | 单次报告 |
| Continuous | 连续报告 |

---

## 9. 异常处理

### 9.1 LLRPSdkException 类

```csharp
try
{
    reader.Start();
}
catch (LLRPSdkException ex)
{
    Console.WriteLine($"LLRP 错误: {ex.Message}");
}
```

**常见异常**：

| 消息 | 原因 |
|------|------|
| You must connect to the reader before starting it. | 未连接即调用 Start |
| Timeout. | 连接超时 |
| A reader initiated connection already exists. | 已存在其他连接 |
| No ROSpec found on reader. | 设备上无 ROSpec |

---

## 附录

### A. 端口参考

| 端口 | 说明 |
|------|------|
| 5084 | LLRP 默认端口（非加密） |
| 5085 | LLRP 加密端口（TLS） |

### B. 制造商编号

| 编号 | 制造商 |
|------|--------|
| 25882 | Impinj |
| 161 | Motorola |
| 10642 | Zebra |
| 47706 | Silion |
| 57690 | Seuic |

### C. 参考资料

- LLRP 规范: [EPCglobal LLRP 1.1](https://www.gs1.org/standards/epcrfid-epcglobal/llrp-1-1)
- Impinj SDK 文档: [Impinj Developer Hub](https://developer.impinj.com/)

---

*文档版本：1.0*
*最后更新：2024年*
