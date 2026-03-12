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


