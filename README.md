# LLRPClient

## 1. LLRP 介绍

LLRP（Low Level Reader Protocol，低层读写器协议）是 GS1 EPCglobal 定义的 RFID 读写器与上位机客户端之间的标准通信接口。之所以称为“低层”，是因为它提供了对 RFID 空口协议时序、读写器行为以及标签操作参数的细粒度控制，适合需要直接管理读写器能力、天线、盘点流程和标签命令的应用场景。

本项目围绕 LLRP 协议实现读写器连接、配置与标签操作能力，可用于对接支持 LLRP 的 RFID 设备。

## 2. 标准版本

### 2.1 LLRP 2.0

GS1 当前公开的 LLRP 标准版本为 2.0，发布时间为 2021 年 1 月。该版本用于更好地匹配 Gen2v2 空口标准，并引入了版本管理、向后兼容以及与隐私和安全相关的扩展能力。

- 标准介绍页面：https://www.gs1.org/standards/epc-rfid/epc-rfid-llrp/2-0
- 当前标准 PDF：https://www.gs1.org/docs/epc/LLRP_standard_i2_r_2021-01-27.pdf

### 2.2 LLRP 1.0.1

LLRP 1.0.1 是较早期且广泛被设备厂商支持的版本，很多现有 RFID 读写器和 SDK 仍基于这一版本实现协议交互。

- 1.0.1 标准 PDF：https://gs1go2.azureedge.net/sites/gs1/files/docs/epc/llrp_1_0_1-standard-20070813.pdf

本项目采用的是 LLRP 1.0.1 版本。

## 3. LLRP ToolKit

LLRP ToolKit，通常简称 LTK，是围绕 LLRP 协议构建的开源工具库集合，主要用于帮助开发者完成 LLRP 消息定义、编解码、收发通信以及对象模型映射等基础工作。对于需要对接 RFID 读写器、开发调试工具或实现上位机控制软件的场景，LTK 可以显著降低协议接入成本。

从历史实现来看，LTK 的官方与社区资料主要围绕 LLRP 1.0.1 展开，因此很多现有项目、示例代码以及读写器厂商实现，仍然以 1.0.1 体系为基础。

### 3.1 相关资源

- 官方站点：http://llrp.org/
- SourceForge 项目页：https://sourceforge.net/projects/llrp-toolkit
- 原始 CVS 项目的 GitHub 镜像：https://github.com/opencps/llrp-toolkit

### 3.2 Impinj 维护的 LTKNet 扩展版本

Impinj 提供了基于原版 LTKNet 扩展的 .NET 版本，增加了 IPv6 与 TLS 加密通信支持，更适合在现代 .NET 环境中进行 LLRP 应用开发和设备通信集成。Impinj 官方 LTKNet 通常包含两部分：标准 LLRP 报文能力与厂商自定义报文扩展，对应常见程序集为 LLRP.dll 与 LLRP.Impinj.dll。

- NuGet 包：https://www.nuget.org/packages/libltknet-sdk/

## 4. 本项目说明

本仓库基于 Impinj 的 LTKNet 思路进行封装，聚焦标准 LLRP 协议能力，并按“协议库 + UI 示例”组织代码：

- LTKNet-Impinj：Impinj维护的 标准 LTK（LTKNet）的 copy 版本。
- LLRPSdk：参考 OctaneSdk 实现并剔除其中对 LLRP.Impinj.dll 的调用，仅保留标准 LLRP 报文能力；主要通过 LLRPSdk.LlrpReader 对外提供能力。
- LLRPReaderUI_WPF / LLRPReaderUI_Avalonia：上层界面与示例工程。


## 5. UI 项目

LLRPReaderUI_WPF 是基于标准 LLRP 报文的可视化 Demo，可用于连接读写器、盘点标签、读写标签内存等常见操作流程验证。

当前已进行过以下设备联调验证：

- Impinj R700
- Zebra FX9600