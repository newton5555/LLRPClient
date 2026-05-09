# WPF 后续优化计划

## 简介

本文档面向 `LLRPReaderUI_WPF` 及其依赖的共享 SDK 解析层，用于整理后续优化工作的实施方案。目标不是记录版本变更，而是给后续实现者提供一份可直接落地的开发规格，尽量减少二次确认和实现分歧。

本文档只覆盖 WPF 项目，不包含 Avalonia 端的设计与实现要求。

## 优化项 1：标签上报来源识别与展示策略

### 问题现状

当前盘点页通过 `InventoryViewModel.OnTagsReported(...)` 统一接收 `LlrpReader.TagsReported` 事件中的标签数据。SDK 层在 `LlrpReader.OnTagReportAvailableInternal(...)` 中解析 `MSG_RO_ACCESS_REPORT`，并将同一批原始报文拆成两条消费链路：

- `TagsReported`
- `TagOpComplete`

当前代码同时存在以下入口：

- `InventoryViewModel.OnTagsReported(...)`
- `ReadWriteViewModel.OnTagOpComplete(...)`
- `AdvancedTagOpsViewModel.OnTagOpComplete(...)`
- `LlrpReader.OnTagReportAvailableInternal(...)`

结合当前日志与代码行为，可以确认标签操作期间至少会出现两类标签数据：

- 普通寻卡 / 标签可见性数据
  - 仍然走 `TagsReported`
  - 常见特征是 `AccessSpecID = 0` 或未携带有效 AccessSpec 标识
  - 不带 `AccessCommandOpSpecResult`
- 真正的标签操作结果数据
  - 同时会参与 `TagsReported` 解析，并进入 `TagOpComplete`
  - 常见特征是带 `AccessCommandOpSpecResult`
  - 或 `AccessSpecID` 能命中当前用户发起的标签操作 `AccessSpec`

这意味着“当前正在做标签操作”并不等于“此时收到的每一条 `TagsReported` 都属于标签操作来源”。同一时间窗口内，读写器可能同时上报普通寻卡数据和标签操作结果数据。

### 目标行为

当前第一版实现目标不再定义为“按单条 `TagsReported` 完全过滤掉所有标签操作期间数据”，而是调整为以下更贴近设备行为的目标：

- SDK 对每条 `TagReportData` 做来源标记，便于调试和后续策略演进。
- 真正的标签操作结果继续以 `TagOpComplete` 作为主消费入口，由读写页和高级标签操作页展示。
- 盘点页对 `TagsReported` 的处理策略保持保守，不能基于不可靠特征误伤正常寻卡数据。

需要明确的限制如下：

- 对于带 `AccessCommandOpSpecResult` 或可明确关联到用户标签操作 `AccessSpec` 的数据，可以可靠识别为标签操作结果。
- 对于标签操作执行窗口内那些 `AccessSpecID = 0`、`HasAccessResults = false` 的标签上报，从单条报文结构上看与普通寻卡数据没有本质区别，不能仅靠当前报文内容 100% 区分其是否属于“标签操作期间顺带出现的普通盘点数据”。
- 盘点流程中的“附加数据（Attached Data）AO”虽然同样通过 `AccessCommandOpSpecResult` 回报，但其用途属于盘点结果增强，不应简单等同于用户标签操作结果。

### 实施改动

1. 在共享 SDK 解析层增加“标签上报来源”的显式标记能力。
2. 来源判定逻辑放在 `LlrpReader.OnTagReportAvailableInternal(...)` 中逐条 `TagReportData` 执行，不能按整包 `RO_ACCESS_REPORT` 一刀切。
3. 当前判定策略按单条 `TagReportData` 的结构特征执行，而不是按“当前 UI 正在执行什么操作”执行。也就是说：
   - 用户正在标签操作，并不意味着当前窗口内所有 `TagsReported` 都属于 `TagOperation`
   - 来源判定只反映“这条报文本身更像什么”，不直接等同于“当前业务上下文”
4. 当前实现中，来源类型至少包含：
   - `Inventory`
   - `AttachedData`
   - `TagOperation`
   - `Unknown`
5. 当前来源判定规则应记录为：
   - 若未携带 `AccessSpecID`
     - 带 `AccessCommandOpSpecResult` 时，判定为 `Unknown`
     - 否则判定为 `Inventory`
   - 若 `AccessSpecID == ATTACHED_DATA_ACCESS_SPEC_ID`
     - 判定为 `AttachedData`
   - 若 `AccessSpecID` 命中当前用户发起的标签操作 `AccessSpec`
     - 判定为 `TagOperation`
   - 若带 `AccessCommandOpSpecResult`
     - 判定为 `TagOperation`
   - 其他情况
     - 判定为 `Inventory`
6. `ReadWriteViewModel.OnTagOpComplete(...)` 与 `AdvancedTagOpsViewModel.OnTagOpComplete(...)` 继续处理标签操作结果，不改变现有职责。
7. 文档层面明确：来源标记的价值主要是帮助识别“明确的标签操作结果”，但不能保证把标签操作期间的普通寻卡样式上报完全隔离出去。

### 测试与验收

- 普通寻卡启动后，盘点页正常显示标签数据。
- 读写操作成功时，`TagOpComplete` 中能收到对应操作结果，供读写页展示。
- 高级标签操作成功时，`TagOpComplete` 中能收到对应操作结果，供高级标签操作页展示。
- 启用附加数据后，盘点页仍能正常显示 `AttachedData` 列内容。
- 混合回报场景下，来源识别按单条 `TagReportData` 生效。
- 在标签操作期间，如果同时出现：
  - `AccessSpecID = 当前操作序列 ID` 且带 `AccessCommandOpSpecResult` 的回报；
  - `AccessSpecID = 0`、`HasAccessResults = false` 的回报；
  应视为设备同时上报了“操作结果数据”和“普通寻卡样式数据”，属于当前已知且允许存在的现象。

## 优化项 2：盘点数据按 EPC 汇总

### 问题现状

当前 `InventoryViewModel.OnTagsReported(...)` 每收到一条标签数据，就直接向 `ReceivedTags` 插入一行。现有统计中：

- `TotalReports` 表示收到的 report 包数
- `TotalTags` 表示收到的标签条数
- `UniqueTagCount` 通过 `uniqueEpcs` 单独统计唯一 EPC 数

但是列表本身没有按 EPC 汇总，导致相同 EPC 会重复显示多行，不利于盘点结果查看和统计。

### 目标行为

盘点页表格改为“同 EPC 汇总为一行”的展示方式。

每个 EPC 在表格中最多只显示一行，且随着后续上报持续更新该行内容。

当前实现中的顺序策略一并明确如下：

- 新 EPC 首次出现时，按首次出现顺序追加到表格中。
- 已存在的 EPC 后续再次上报时，只更新该行内容，不改变该行在表格中的位置。
- 也就是说，在没有新 EPC 进入的前提下，表格顺序应保持稳定。

### 实施改动

1. 以 EPC 十六进制字符串作为聚合键，比较时大小写不敏感。
2. 若 EPC 为空，则使用 `"-"` 作为聚合键参与展示。
3. `InventoryViewModel` 内部维护一份 EPC 到聚合行的映射结构，`ReceivedTags` 继续作为 UI 绑定集合。
4. 聚合行在 `ReceivedTags` 中采用“首次出现顺序固定”的展示策略：
   - 新 EPC 创建新行并追加到集合末尾。
   - 已存在 EPC 不执行 `Remove + Insert` 之类的重排操作。
   - 因此更新已有 EPC 时，表格顺序不应跳动。
5. 聚合规则固定如下：
   - `SeenCount`
     - 如果上报中带 `TagSeenCount`，按该值累加。
     - 如果没有 `TagSeenCount`，默认按 `1` 累加。
   - `FirstSeenTimestampUtc`
     - 取所有上报中的最早时间。
   - `LastSeenTimestampUtc`
     - 取所有上报中的最晚时间。
   - 其他字段
     - `ReceiveTime`
     - `Antenna`
     - `ChannelMhz`
     - `Rssi`
     - `Pc`
     - `Crc`
     - `AttachedData`
     都使用最新一次标签数据覆盖。
6. `InventoryTagItemViewModel` 的语义调整为“聚合结果行”，不再视为原始单条上报行。
7. 聚合行对象需要支持属性变更通知。否则当顺序稳定、不再重排时，WPF `DataGrid` 只能看到首次插入时的值，无法实时刷新后续字段更新。
8. 统计项行为固定如下：
   - `TotalReports`：仍表示收到的 report 包数
   - `TotalTags`：仍表示收到的原始标签条数
   - `UniqueTagCount`：改为当前聚合后的 EPC 行数，并与 `ReceivedTags.Count` 一致

### 测试与验收

- 同一 EPC 连续上报多次时，表格中始终只保留一行。
- 同一 EPC 连续上报多次时，该行位置保持不变，仅内容更新。
- 在没有新 EPC 进入时，表格顺序保持稳定。
- `SeenCount` 按规则累加。
- `FirstSeenTimestampUtc` 为最早值。
- `LastSeenTimestampUtc` 为最新值。
- `ReceiveTime`、`Antenna`、`ChannelMhz`、`Rssi`、`Pc`、`Crc`、`AttachedData` 在后续上报后被最新数据覆盖，并能实时刷新到 UI。
- 多个 EPC 混合上报时，`UniqueTagCount` 与表格行数保持一致，`TotalTags` 仍反映原始标签条数。

## 优化项 3：寻卡参数页面缓存加载与设备获取拆分

### 问题现状

当前盘点配置页的“读取寻卡配置”按钮实际绑定的是 `InventoryConfigViewModel.QueryInventoryConfig()`。这个方法并不会直接访问设备，而是从 `settingsStore` 缓存中取出快照后调用 `ApplySettingsSnapshot(settings)`。

也就是说，当前按钮语义与实际行为不一致：界面上看像是在“从设备读取”，实际上只是“从缓存加载”。

相关现有入口如下：

- `InventoryConfigViewModel.QueryInventoryConfig()`
- `InventoryConfigViewModel.ApplySettingsSnapshot(...)`

### 目标行为

盘点配置页中明确区分两种动作：

- 从缓存加载
- 从设备获取参数

用户能够清楚知道当前看到的是缓存数据，还是刚从设备读取到的最新数据。

### 实施改动

1. 保留当前缓存回填能力，但将现有按钮和命令语义明确为“从缓存加载”。
2. 新增一个真正访问设备的动作，例如新增独立命令：
   - 直接调用 `reader.QuerySettings()`
   - 获取到最新 `Settings` 后同步更新 `settingsStore`
   - 再调用 `ApplySettingsSnapshot(...)` 刷新盘点配置页 UI
3. 后续默认按钮命名采用：
   - `从缓存加载`
   - `从设备获取参数`
4. 同步调整本地化资源与状态提示文案，至少区分以下场景：
   - 缓存加载成功
   - 缓存为空
   - 设备获取成功
   - 设备获取失败
   - 设备未连接
5. 自动连接后的现有缓存回填行为先保持不变，但文案上视为“缓存加载”，不再误导为实时设备查询。

### 测试与验收

- 点击“从缓存加载”时，不访问设备，只使用 `settingsStore` 中的快照。
- 点击“从设备获取参数”时，必须实际执行设备查询。
- 设备查询成功后，应同步刷新缓存与当前页面 UI。
- 缓存为空时，点击“从缓存加载”应给出明确提示。
- 设备未连接或设备查询失败时，应显示与“设备获取”语义一致的提示和日志。

## 实施入口

后续开发时，优先从以下位置着手：

- `LLRPSdk/LlrpReader.cs`
  - `LlrpReader.OnTagReportAvailableInternal(...)`
- `LLRPReaderUI_WPF/ViewModels/InventoryViewModel.cs`
  - `InventoryViewModel.OnTagsReported(...)`
- `LLRPReaderUI_WPF/ViewModels/ReadWriteViewModel.cs`
  - `ReadWriteViewModel.OnTagOpComplete(...)`
- `LLRPReaderUI_WPF/ViewModels/AdvancedTagOpsViewModel.cs`
  - `AdvancedTagOpsViewModel.OnTagOpComplete(...)`
- `LLRPReaderUI_WPF/ViewModels/InventoryConfigViewModel.cs`
  - `InventoryConfigViewModel.QueryInventoryConfig()`

## 假设与默认决策

- 本文档只覆盖 WPF 与其依赖的共享 SDK 解析层，不扩展到 Avalonia。
- 文档为实施计划，不是用户手册，也不是版本更新日志。
- 标签来源判定第一版默认优先依赖 `AccessCommandOpSpecResult` 与 `AccessSpecID`，不要求一开始就依赖 `ROSpecID` 或 `SpecIndex`。
- 单靠单条 `TagsReported` 的报文结构，当前无法 100% 区分“标签操作期间顺带出现的普通寻卡样式上报”与“真正的普通寻卡数据”。
- 第一版实现中，盘点附加数据专用 AccessSpec 默认视为盘点链路的一部分，不归类为用户标签操作结果。
- 盘点页采用“只显示聚合结果”的方向，不保留原始逐条标签列表的并行视图。
- 参数页面中的两类动作默认命名固定为：
  - `从缓存加载`
  - `从设备获取参数`
