# WPF 后续优化计划

## 简介

本文档面向 `LLRPReaderUI_WPF` 及其依赖的共享 SDK 解析层，用于整理后续优化工作的实施方案。目标不是记录版本变更，而是给后续实现者提供一份可直接落地的开发规格，尽量减少二次确认和实现分歧。

本文档只覆盖 WPF 项目，不包含 Avalonia 端的设计与实现要求。

## 优化项 1：标签上报来源识别与过滤

### 问题现状

当前盘点页通过 `InventoryViewModel.OnTagsReported(...)` 统一接收 `LlrpReader.TagsReported` 事件中的标签数据。SDK 层在 `LlrpReader.OnTagReportAvailableInternal(...)` 中解析 `MSG_RO_ACCESS_REPORT`，但没有显式标记该条报告是来自普通寻卡流程，还是来自读写操作 / 高级标签操作触发的标签操作报告。

当前代码同时存在以下入口：

- `InventoryViewModel.OnTagsReported(...)`
- `ReadWriteViewModel.OnTagOpComplete(...)`
- `AdvancedTagOpsViewModel.OnTagOpComplete(...)`
- `LlrpReader.OnTagReportAvailableInternal(...)`

这意味着标签操作流程中附带的标签数据，存在进入盘点 UI 的风险，造成盘点结果被污染。

### 目标行为

后续实现后，盘点页只显示普通寻卡来源的标签数据。

来自以下流程的标签操作结果，不应再显示到盘点页：

- 标签读写操作
- 高级标签操作
- 任何带 `AccessCommandOpSpecResult` 的标签操作回报

这些结果应继续只在对应的功能页面内消费和展示。

### 实施改动

1. 在共享 SDK 解析层增加“标签上报来源”的显式标记能力。
2. 来源判定逻辑放在 `LlrpReader.OnTagReportAvailableInternal(...)` 中逐条 `TagReportData` 执行，不能按整包 `RO_ACCESS_REPORT` 一刀切。
3. 默认判定规则如下：
   - 若该条 `TagReportData` 带有 `AccessCommandOpSpecResult`，判定为标签操作来源。
   - 若该条 `TagReportData` 带有可识别的 `AccessSpecID` 且属于 Access 操作链路，判定为标签操作来源。
   - 否则判定为普通寻卡来源。
4. 建议新增明确的来源类型，例如 `TagReportSource`，至少包含：
   - `Inventory`
   - `TagOperation`
5. `InventoryViewModel.OnTagsReported(...)` 后续只处理 `Inventory` 来源的标签。
6. `ReadWriteViewModel.OnTagOpComplete(...)` 与 `AdvancedTagOpsViewModel.OnTagOpComplete(...)` 继续处理标签操作结果，不改变现有职责。

### 测试与验收

- 普通寻卡启动后，盘点页正常显示标签数据。
- 读写操作成功时，结果只在读写页显示，盘点页不新增行。
- 高级标签操作成功时，结果只在高级标签操作页显示，盘点页不新增行。
- 混合回报场景下，来源识别按单条 `TagReportData` 生效，不得误过滤普通寻卡标签。

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

### 实施改动

1. 以 EPC 十六进制字符串作为聚合键，比较时大小写不敏感。
2. 若 EPC 为空，则使用 `"-"` 作为聚合键参与展示。
3. `InventoryViewModel` 内部维护一份 EPC 到聚合行的映射结构，`ReceivedTags` 继续作为 UI 绑定集合。
4. 聚合规则固定如下：
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
5. `InventoryTagItemViewModel` 的语义调整为“聚合结果行”，不再视为原始单条上报行。
6. 统计项行为固定如下：
   - `TotalReports`：仍表示收到的 report 包数
   - `TotalTags`：仍表示收到的原始标签条数
   - `UniqueTagCount`：改为当前聚合后的 EPC 行数，并与 `ReceivedTags.Count` 一致

### 测试与验收

- 同一 EPC 连续上报多次时，表格中始终只保留一行。
- `SeenCount` 按规则累加。
- `FirstSeenTimestampUtc` 为最早值。
- `LastSeenTimestampUtc` 为最新值。
- 其他字段在后续上报后被最新数据覆盖。
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
- 盘点页采用“只显示聚合结果”的方向，不保留原始逐条标签列表的并行视图。
- 参数页面中的两类动作默认命名固定为：
  - `从缓存加载`
  - `从设备获取参数`
