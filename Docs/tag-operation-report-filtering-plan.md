# 区分标签操作触发的寻卡数据，避免进入 Inventory DataGrid

## Summary

根因在 SDK：`LlrpReader.OnTagReportAvailableInternal` 收到同一个 `RO_ACCESS_REPORT` 后，只要有 EPC 就加入 `TagsReported`，同时如果有 `AccessCommandOpSpecResult` 又会加入 `TagOpComplete`。读写/高级标签操作调用 `reader.Start()` 后产生的 report 因此会同时进入 Inventory 的 `ReceivedTags`。

采用 SDK 级标记 + UI 过滤：保留正常盘点和附加数据 AO，过滤临时标签操作 AccessSpec 产生的数据。

## Key Changes

- 在 SDK 的 `Tag` 增加只读/可 set 元数据：
  - `uint? AccessSpecId`
  - `bool HasAccessOperationResults`
  - `TagReportSource ReportSource`
- 新增 `TagReportSource` enum：
  - `Inventory`
  - `AttachedData`
  - `TagOperation`
  - `Unknown`
- 在 `LlrpReader.OnTagReportAvailableInternal` 解析 `MSG_RO_ACCESS_REPORT` 时设置来源：
  - 无 `AccessCommandOpSpecResult`：`Inventory`
  - 有 access 结果且 `AccessSpecID == 65534`：`AttachedData`
  - 有 access 结果且 `AccessSpecID != 65534`：`TagOperation`
  - 缺少必要字段但有 access 结果：`Unknown`
- `InventoryViewModel.OnTagsReported` 只接收：
  - `Inventory`
  - `AttachedData`
  - `Unknown` 中没有 access 操作结果的普通报告
- `TagOperation` 来源的 tag 不计入：
  - `TotalReports`
  - `TotalTags`
  - `UniqueTagCount`
  - `ReceivedTags`
- 保持 `TagOpComplete` 逻辑不变，读写/高级标签操作仍按 `SequenceId` 处理结果。

## Test Plan

- 构建：
  - `dotnet build LLRPReaderUI_Avalonia/LLRPReaderUI_Avalonia.Desktop/LLRPReaderUI_Avalonia.Desktop.csproj`
  - `git diff --check`
- 手动验证：
  - 正常 Inventory 寻卡：DataGrid 正常新增数据。
  - 开启 AttachedData 后正常 Inventory：DataGrid 仍显示 EPC 和 AttachedData。
  - 执行 ReadWrite 读/写：操作结果正常显示，但 Inventory DataGrid 不新增这次操作触发的 EPC。
  - 执行 AdvancedTagOps 的 BlockErase/Lock/Kill：操作结果正常显示，Inventory DataGrid 不新增这次操作触发的 EPC。
  - `WaitForQuery` 手动拉取：仍按原来的短窗口接收普通盘点数据。

## Assumptions

- `65534` 继续作为 SDK 内部附加数据 AccessSpec ID，不改现有设备配置语义。
- 这次不改变 LLRP 交互流程，不暂停/重启 Inventory，不改读写/高级操作的业务流程。
- 只增加向后兼容的 SDK 元数据字段，不移除现有 `TagsReported` / `TagOpComplete` 事件。
