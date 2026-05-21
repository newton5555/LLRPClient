# LLRPReaderManagement 技术说明

`LLRPReaderManagement` 是一个 .NET MAUI Blazor Hybrid 项目，用 Razor Component 实现前端界面，通过 ViewModel/State/Service/Repository 分层调用 `LLRPSdk`，用于管理 LLRP RFID Reader 的连接、配置、盘点、ROSpec 和标签访问操作。

## 技术栈

- .NET MAUI Blazor Hybrid
- Razor Component
- Microsoft DI
- `LLRPSdk` 项目引用
- 多目标框架：
  - 默认：`net10.0-android`
  - Windows 下追加：`net10.0-windows10.0.19041.0`
  - iOS / MacCatalyst 仅在显式传入 `EnableAppleTargets=true` 时启用

## 项目结构

```text
LLRPReaderManagement/
  Components/
    Pages/              Razor 页面
    Shared/AppShell     应用外壳、顶部栏、侧边栏
    Routes.razor        Blazor 路由入口
  ViewModels/           页面 ViewModel
  State/AppState.cs     全局运行状态
  Services/             业务服务
  Repositories/         LLRPSdk 适配层
  Models/               UI 使用的数据模型
  wwwroot/css/app.css   原型样式整合后的应用样式
  Prototype/            HTML 原型参考文件
  MauiProgram.cs        DI 注册和 MAUI 启动配置
```

## 架构分层

### Component

Razor 页面只负责渲染 UI 和接收用户交互，主要文件在 `Components/Pages`：

- `Home.razor`：仪表盘
- `Readers.razor`：Reader 连接和多设备列表
- `Inventory.razor`：多设备盘点和标签列表
- `Config.razor`：Reader 参数配置
- `Rospec.razor`：ROSpec 运行控制
- `Access.razor`：标签读写访问操作
- `History.razor`：标签和日志历史视图

页面通过注入 ViewModel 调用业务能力，不直接调用 `LLRPSdk`。

### ViewModel / State

`ViewModels` 负责把页面操作转换成 Service 调用，并暴露页面绑定字段。

`AppState` 是应用级状态容器，维护：

- 已连接 Reader 列表
- 当前 Active Reader
- Reader 能力信息和配置快照
- 盘点运行状态
- 标签缓存
- 应用日志
- 忙碌状态和连接状态文本

状态变更通过 `Changed` 事件通知 Razor 页面刷新。

### Service

`Services` 承担业务流程编排：

- `ReaderManagementService`
  - 连接 / 断开 Reader
  - 切换 Active Reader
  - 查询和应用 Reader 配置
  - 处理 SDK 无 ROSPEC / 配置缺失场景
  - 处理 Keepalive timeout
- `InventoryService`
  - 启动 / 停止所有 Reader 盘点
  - 拉取缓存标签
  - 清空标签结果
- `AccessOperationService`
  - 对选中标签执行 Read / Write
  - 创建并清理 TagOpSequence
- `AppLogService`
  - 写入 UI 日志和调试日志

### Repository

`Repositories` 是 `LLRPSdk` 的适配层。上层业务只依赖 `ILlrpReaderRepository`，不直接持有 SDK Reader 对象。

当前 Repository 支持多 Reader：

- 内部按 endpoint 维护多个 `LlrpReader`
- `ActiveEndpoint` 表示当前操作对象
- `StartAll()` / `StopAll()` 可批量控制所有已连接设备
- 标签事件带上 endpoint，避免多设备标签来源丢失
- 单个 Reader 连接失败时只清理该 Reader，不影响已经连接的设备

## DI 注册

DI 配置在 `MauiProgram.cs`：

```csharp
builder.Services.AddSingleton<AppState>();
builder.Services.AddSingleton<ILlrpReaderRepository, LlrpReaderRepository>();
builder.Services.AddSingleton<IAppLogService, AppLogService>();
builder.Services.AddSingleton<ReaderManagementService>();
builder.Services.AddSingleton<InventoryService>();
builder.Services.AddSingleton<AccessOperationService>();

builder.Services.AddTransient<DashboardViewModel>();
builder.Services.AddTransient<ReadersViewModel>();
builder.Services.AddTransient<InventoryViewModel>();
builder.Services.AddTransient<AccessViewModel>();
builder.Services.AddTransient<ConfigViewModel>();
builder.Services.AddTransient<RospecViewModel>();
```

`AppState`、Repository 和 Service 使用单例，保证设备连接和运行状态在页面切换时不丢失。ViewModel 使用 transient，由页面按需创建。

## 主要调用链路

### 连接 Reader

```text
Readers.razor
  -> ReadersViewModel.ConnectAsync()
  -> ReaderManagementService.ConnectAsync()
  -> ILlrpReaderRepository.ConnectAsync()
  -> LLRPSdk.LlrpReader.Connect()
  -> QuerySingulatingState / QuerySettings
  -> AppState.SetConnected()
```

连接后会查询 Reader 能力和配置。如果 SDK 返回未配置或配置无效，Service 会参考 WPF 项目的处理方式，先调用 `ApplyDefaultSettings()`，再重新查询配置。

### 多设备盘点

```text
Inventory.razor
  -> InventoryViewModel.Start()
  -> InventoryService.Start()
  -> ILlrpReaderRepository.StartAll()
  -> 每个 LlrpReader.Start()
  -> TagsReported(endpoint, tags)
  -> AppState.AddTags(endpoint, tags)
```

标签缓存使用 `endpoint|epc` 作为 key，因此同一个 EPC 来自不同 Reader 时不会互相覆盖。

### 配置应用

```text
Config.razor
  -> ConfigViewModel.ApplyChanges()
  -> ReaderManagementService.ApplyCurrentSettings()
  -> 必要时 Stop 当前 Reader
  -> ILlrpReaderRepository.ApplySettings()
  -> QuerySettings()
  -> AppState.SetSettings()
```

配置只作用于当前 Active Reader。

### 标签读写

```text
Access.razor
  -> AccessViewModel.ReadAsync() / WriteAsync()
  -> AccessOperationService
  -> DeleteAllOpSequences()
  -> AddOpSequence()
  -> Start()
  -> TagOpCompleted
  -> Stop()
  -> DeleteAllOpSequences()
```

访问操作目前面向 Active Reader。页面选择标签时会根据标签来源 Reader 切换 Active Reader。

## 前端实现说明

界面样式参考 `Prototype/screens` 中的 HTML 原型，核心样式集中在 `wwwroot/css/app.css`。

当前 UI 采用工具型布局：

- 顶部栏 + 左侧导航
- 页面主区域使用卡片、表格、状态徽章和工具栏
- Reader、Inventory、Config、ROSpec、Access 页面均按原型结构对齐
- switch/toggle 使用 `.toggle` 样式，在 Blazor 中以 `button` 实现，便于点击和键盘焦点处理

## 构建命令

Windows 目标：

```powershell
dotnet build .\LLRPReaderManagement\LLRPReaderManagement.csproj -f net10.0-windows10.0.19041.0 --no-restore -p:UseAppHost=false
```

Android 目标：

```powershell
dotnet build .\LLRPReaderManagement\LLRPReaderManagement.csproj -f net10.0-android --no-restore
```

默认构建：

```powershell
dotnet build .\LLRPReaderManagement\LLRPReaderManagement.csproj -p:UseAppHost=false
```

如果 Visual Studio 报旧 iOS workload 路径错误，先确认项目是否真的启用了 Apple target。当前项目默认不启用 iOS / MacCatalyst；如果 VS 仍引用旧路径，通常是设计时构建缓存或 workload manifest 缓存问题。

## 当前限制

- Lock / Kill 操作 UI 已预留，但业务未启用。
- Access 操作当前基于 Active Reader，不是全设备广播。
- `QueryTags(double)` 来自 SDK 的同步查询接口，目前仍用于手动拉取缓存标签，SDK 会给出 obsolete warning。
- 多 Reader 的 `UniqueTags` 统计当前主要在全局标签列表中体现，Reader 卡片里更侧重连接状态和报告数。

