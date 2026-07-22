# LLRPClient

[English](README_EN.md) | 简体中文

LLRPClient 是一组面向 LLRP RFID Reader 的 .NET 客户端、SDK 封装和示例 UI 项目。仓库基于 LTKNet / LLRPSdk 组织代码，聚焦标准 LLRP 报文能力，可用于连接、配置、盘点和访问支持 LLRP 的 RFID 读写器。

当前项目采用 LLRP 1.0.1 协议体系，主要参考 Impinj LTKNet 的 .NET 实现思路，并保留标准 LLRP 能力。更详细的协议资料、SDK 文档和 UI 操作手册可在 `Refs`、`Docs` 以及各子项目目录中查看。

## 项目导航 / Project Links

| 项目 | 说明 |
| --- | --- |
| [LLRPReaderManagement](LLRPReaderManagement/README.md) | .NET MAUI Blazor Hybrid 管理端，支持 Reader 连接、配置、盘点、ROSpec 与标签访问操作。 |
| [LLRP.Cli](LLRP.Cli/README.md) | 面向 LLRP 读写器的命令行工具，提供交互式控制台、ROSpec 管理、报文监控与原始帧解码。 |
| [LLRPSdk](LLRPSdk) | 标准 LLRP 能力封装，主要通过 `LLRPSdk.LlrpReader` 对外提供 Reader 操作接口。 |
| [LTKNet/LLRP](LTKNet/LLRP) | LLRP-LTKNet 协议库代码。 |
| [LLRPReaderUI_WPF](LLRPReaderUI_WPF) | WPF 示例客户端，可用于连接 Reader、盘点标签、读写标签内存和查看 LLRP 报文。 |
| [LLRPReaderUI_Avalonia](LLRPReaderUI_Avalonia) | Avalonia 示例客户端。 |
| [Docs](Docs) | SDK 开发指南、UI 用户手册、配置对比和测试文档。 |
| [Refs](Refs) | LLRP 标准文档和协议定义文件。 |

## LLRP CLI

`LLRP.Cli` 是用于调试和自动化标准 LLRP 读写器的跨平台命令行工具。它既可作为交互式 REPL 使用，也可在脚本或 CI 中执行单次报文监控与帧解码。除命令结果外，工具会显示 LLRP 报文树、Message ID、状态码以及完整的十六进制原始帧，便于定位设备返回的协议问题。

```powershell
# 启动交互式控制台
dotnet run --project LLRP.Cli

# 连接读写器后，可执行：caps、config、rospec list、monitor 30 等命令
# 不连接设备也可离线解码已捕获的 LLRP 帧
dotnet run --project LLRP.Cli -- decode --hex 04160000000E0000002A00000001
```

### 解码输出示例

下图为 CLI 对一条 `START_ROSPEC` 原始帧的实际解码输出：它展示了语义树和保留的完整 Hex 数据。

![LLRP CLI 解码 START_ROSPEC 帧](Docs/images/llrp-cli-decode-example.svg)

CLI 支持 TLS 连接、Reader 能力与配置查询、ROSpec 创建/编辑/启停/删除、持续帧监控和离线帧解码。需要执行会改变读写器状态的操作时，交互式控制台会要求确认。完整命令说明与发布方式请见 [LLRP.Cli/README.md](LLRP.Cli/README.md)。

## LLRP 简介

LLRP（Low Level Reader Protocol，低层读写器协议）是 GS1 EPCglobal 定义的 RFID 读写器与上位机客户端之间的标准通信接口。之所以称为“低层”，是因为它提供了对 RFID 空口协议时序、读写器行为以及标签操作参数的细粒度控制，适合需要直接管理读写器能力、天线、盘点流程和标签命令的应用场景。

本项目围绕 LLRP 协议实现读写器连接、配置与标签操作能力，可用于对接支持 LLRP 的 RFID 设备。

## 标准版本

### LLRP 2.0

GS1 当前公开的 LLRP 标准版本为 2.0，发布时间为 2021 年 1 月。该版本用于更好地匹配 Gen2v2 空口标准，并引入了版本管理、向后兼容以及与隐私和安全相关的扩展能力。

- LLRP 2.0 标准介绍：https://www.gs1.org/standards/epc-rfid/epc-rfid-llrp/2-0
- LLRP 2.0 标准 PDF：https://www.gs1.org/docs/epc/LLRP_standard_i2_r_2021-01-27.pdf

### LLRP 1.0.1

LLRP 1.0.1 是较早期且广泛被设备厂商支持的版本，很多现有 RFID 读写器和 SDK 仍基于这一版本实现协议交互。本项目采用的是 LLRP 1.0.1 版本。

- LLRP 1.0.1 标准 PDF：https://gs1go2.azureedge.net/sites/gs1/files/docs/epc/llrp_1_0_1-standard-20070813.pdf

## LLRP ToolKit

LLRP ToolKit，通常简称 LTK，是围绕 LLRP 协议构建的开源工具库集合，主要用于帮助开发者完成 LLRP 消息定义、编解码、收发通信以及对象模型映射等基础工作。对于需要对接 RFID 读写器、开发调试工具或实现上位机控制软件的场景，LTK 可以显著降低协议接入成本。

从历史实现来看，LTK 的官方与社区资料主要围绕 LLRP 1.0.1 展开，因此很多现有项目、示例代码以及读写器厂商实现，仍然以 1.0.1 体系为基础。

相关资源：

- 官方站点：http://llrp.org/
- SourceForge 项目页：https://sourceforge.net/projects/llrp-toolkit
- 原始 CVS 项目的 GitHub 镜像：https://github.com/opencps/llrp-toolkit

### Impinj 维护的 LTKNet 扩展版本

Impinj 提供了基于原版 LTKNet 扩展的 .NET 版本，增加了 IPv6 与 TLS 加密通信支持，更适合在现代 .NET 环境中进行 LLRP 应用开发和设备通信集成。Impinj 官方 LTKNet 通常包含两部分：标准 LLRP 报文能力与厂商自定义报文扩展，对应常见程序集为 `LLRP.dll` 与 `LLRP.Impinj.dll`。

- NuGet 包：https://www.nuget.org/packages/libltknet-sdk/

## 设备验证

当前已进行过以下设备联调验证：

- Impinj R700
- Zebra FX9600

## 快速入口

- 查看 MAUI Blazor 管理端说明：[LLRPReaderManagement/README.md](LLRPReaderManagement/README.md)
- 查看 CLI 说明：[LLRP.Cli/README.md](LLRP.Cli/README.md)
- 查看 LLRPSdk 开发指南：[LLRPSdk/Docs/LLRPSdk_Developer_Guide.md](LLRPSdk/Docs/LLRPSdk_Developer_Guide.md)
- 查看 WPF 中文操作手册：[LLRPReaderUI_WPF/LLRPReaderUI_WPF_UI操作手册.md](LLRPReaderUI_WPF/LLRPReaderUI_WPF_UI操作手册.md)
