$ErrorActionPreference = 'Stop'

$baseDir = 'E:\文档\软著-LLRP客户端'
$originalDoc = Join-Path $baseDir 'LLRP客户端.doc'
$backupDoc = Join-Path $baseDir ('LLRP客户端_原始备份_' + (Get-Date -Format 'yyyyMMdd_HHmmss') + '.doc')
$updatedDocx = Join-Path $baseDir 'LLRP客户端_更新版.docx'
$screenshotDir = Join-Path $baseDir '界面UI'

Copy-Item -LiteralPath $originalDoc -Destination $backupDoc -Force

$images = @{
    DeviceConnection = Join-Path $screenshotDir '设备连接.bmp'
    Settings1        = Join-Path $screenshotDir '参数配置1.bmp'
    Settings2        = Join-Path $screenshotDir '参数配置2.bmp'
    InventoryCfg1    = Join-Path $screenshotDir '盘点配置1.bmp'
    InventoryCfg2    = Join-Path $screenshotDir '盘点配置2.bmp'
    InventoryCfg3    = Join-Path $screenshotDir '盘点配置3.bmp'
    Inventory        = Join-Path $screenshotDir '盘点操作.bmp'
    ReadWrite        = Join-Path $screenshotDir '读写操作.bmp'
    Adv1             = Join-Path $screenshotDir '标签高级操作1.bmp'
    Adv2             = Join-Path $screenshotDir '标签高级操作2.bmp'
    Log              = Join-Path $screenshotDir '日志.bmp'
    LlrpHistory      = Join-Path $screenshotDir 'LLRP历史消息.bmp'
}

$word = $null
$doc = $null
$sel = $null

function Set-Font {
    param(
        [string]$Name = '宋体',
        [int]$Size = 12,
        [bool]$Bold = $false
    )

    $script:sel.Font.Name = $Name
    $script:sel.Font.Size = $Size
    $script:sel.Font.Bold = [int][bool]$Bold
}

function Add-Paragraph {
    param(
        [string]$Text,
        [string]$Style = $null,
        [string]$Font = '宋体',
        [int]$Size = 12,
        [bool]$Bold = $false
    )

    if ($Style) {
        $script:sel.Style = $script:doc.Styles.Item($Style)
    }

    Set-Font -Name $Font -Size $Size -Bold $Bold
    $script:sel.TypeText($Text)
    $script:sel.TypeParagraph()
}

function Add-BlankLine {
    $script:sel.TypeParagraph()
}

function Add-Bullets {
    param([string[]]$Items)

    $range = $script:sel.Range
    foreach ($item in $Items) {
        $script:sel.TypeText($item)
        $script:sel.TypeParagraph()
    }

    $range.ListFormat.ApplyBulletDefault()
    $script:sel.Collapse(0)
    $script:sel.TypeParagraph()
}

function Add-Numbered {
    param([string[]]$Items)

    $range = $script:sel.Range
    foreach ($item in $Items) {
        $script:sel.TypeText($item)
        $script:sel.TypeParagraph()
    }

    $range.ListFormat.ApplyNumberDefault()
    $script:sel.Collapse(0)
    $script:sel.TypeParagraph()
}

function Add-Image {
    param(
        [string]$Path,
        [string]$Caption,
        [double]$Width = 430
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        return
    }

    $script:sel.ParagraphFormat.Alignment = 1
    $pic = $script:sel.InlineShapes.AddPicture($Path)
    $pic.LockAspectRatio = -1
    $pic.Width = $Width
    $script:sel.TypeParagraph()
    Set-Font -Name '宋体' -Size 10 -Bold $false
    $script:sel.TypeText($Caption)
    $script:sel.TypeParagraph()
    $script:sel.ParagraphFormat.Alignment = 0
    $script:sel.TypeParagraph()
}

try {
    $word = New-Object -ComObject Word.Application
    $word.Visible = $false
    $word.DisplayAlerts = 0
    $doc = $word.Documents.Add()
    $sel = $word.Selection

    $sel.ParagraphFormat.Alignment = 1
    Add-BlankLine
    Add-BlankLine
    Add-Paragraph 'LLRP客户端软件操作手册' $null '黑体' 22 $true
    Add-BlankLine
    Add-Paragraph '基于当前 LLRPClient 项目与最新界面截图整理' $null '宋体' 14 $false
    Add-BlankLine
    Add-BlankLine
    Add-Paragraph '版本：V2.0' $null '宋体' 14 $false
    Add-Paragraph ('更新日期：' + (Get-Date -Format 'yyyy年MM月dd日')) $null '宋体' 14 $false
    Add-Paragraph '适用对象：支持 LLRP 协议的 RFID 读写器用户' $null '宋体' 14 $false
    Add-BlankLine
    Add-BlankLine
    Add-Paragraph '项目名称：LLRPClient' $null '宋体' 14 $false
    Add-Paragraph '界面版本：Avalonia 桌面客户端' $null '宋体' 14 $false
    $sel.InsertBreak(7)
    $sel.ParagraphFormat.Alignment = 0

    Add-Paragraph '目录' 'Heading 1' '黑体' 16 $true
    $range = $sel.Range
    $null = $doc.TablesOfContents.Add($range, $true, 1, 3)
    $sel.TypeParagraph()
    $sel.InsertBreak(7)

    Add-Paragraph '1 软件简介' 'Heading 1' '黑体' 16 $true
    Add-Paragraph 'LLRP客户端是一个基于标准 LLRP 1.0.1 协议开发的 RFID 读写器上位机软件，用于连接支持 LLRP 的读写器，并完成参数配置、盘点、标签读写、高级标签操作、日志查看以及 LLRP 历史消息分析等工作。'
    Add-Paragraph '当前项目采用“协议库 + 可视化界面”的结构：底层由 LTKNet 与 LLRPSdk 提供标准 LLRP 报文编解码和读写器控制能力，上层采用 Avalonia 实现跨平台桌面图形界面。'
    Add-Paragraph '当前已在以下设备上进行过联调验证：'
    Add-Bullets @(
        'Impinj R700',
        'Zebra FX9600'
    )

    Add-Paragraph '2 运行环境' 'Heading 1' '黑体' 16 $true
    Add-Paragraph '建议在 Windows 10 或 Windows 11 64 位操作系统下运行本软件。程序通过网络与读写器建立 LLRP 连接，默认使用读写器 IP 地址进行访问，必要时可附带端口号。'
    Add-Paragraph '使用前建议确认以下条件：'
    Add-Numbered @(
        '读写器已正确上电，并与当前电脑网络互通。',
        '已知读写器 IP 地址，例如 192.168.41.116。',
        '读写器支持标准 LLRP 协议。',
        '如设备启用了访问控制、射频限制或特定区域参数，应由管理员提前完成基础网络与设备准备。'
    )

    Add-Paragraph '3 功能概览' 'Heading 1' '黑体' 16 $true
    Add-Paragraph '软件左侧为功能导航区，右侧为当前功能页，底部状态栏会持续显示设备连接状态、盘点状态、天线状态、GPI/GPO 状态以及设备 MAC 信息。'
    Add-Paragraph '当前版本主要包括以下模块：'
    Add-Bullets @(
        '设备连接：连接或断开读写器，并查看 FeatureSet 能力信息。',
        '参数配置：读取和下发设备基础参数、射频参数、盘点协议参数、GPIO 与天线参数。',
        '盘点配置：配置自动开始/停止策略、标签筛选、数据上报策略以及附加数据读取。',
        '盘点操作：启动或停止盘点，查看 EPC、RSSI、次数、时间等标签结果。',
        '读写操作：对指定标签执行内存读取、写入和块写操作。',
        '高级标签操作：执行 BlockErase、Lock、Kill 等高风险标签操作。',
        '日志：查看操作日志、LLRP 消息日志和原始报文收发日志。',
        '历史LLRP消息：按条件筛选历史原始帧，并查看消息树与十六进制报文。'
    )

    Add-Paragraph '4 操作说明' 'Heading 1' '黑体' 16 $true

    Add-Paragraph '4.1 设备连接' 'Heading 2' '黑体' 14 $true
    Add-Paragraph '设备连接页用于建立客户端与读写器之间的 LLRP 连接。输入框支持直接填写 IP 地址，也支持填写“IP:端口”的形式。连接成功后，界面会自动显示当前设备的 FeatureSet 能力信息。'
    Add-Numbered @(
        '在“设备连接”页输入读写器地址，例如 192.168.41.116。',
        '单击“连接”按钮，等待软件完成连接与初始化。',
        '连接成功后，可在下方表格中查看设备型号、固件版本、天线数量、GPI/GPO 数量、最大操作序列数、频率列表、功率列表等能力信息。',
        '如需结束通信，单击“断开”按钮。'
    )
    Add-Paragraph '说明：软件支持 Keepalive 心跳监控。当读写器长时间无响应时，客户端会自动断开连接并更新状态。'
    Add-Image $images.DeviceConnection '图4-1 设备连接页面'

    Add-Paragraph '4.2 参数配置' 'Heading 2' '黑体' 14 $true
    Add-Paragraph '参数配置页用于读取与设置设备运行参数，主要包括系统管理、射频物理层参数、空口协议参数、高级特性、GPIO 配置和天线参数。'
    Add-Paragraph '主要配置项说明如下：'
    Add-Bullets @(
        '系统管理：可启用或关闭 Keepalive，并设置 Keepalive 间隔。',
        '射频物理层参数：包括 RF Mode、跳频表 ID、信道索引等。',
        '空口协议参数：包括 Session 与 TagPopulationEstimate。',
        '高级特性配置：可开启状态感知盘点，并配置 Inventory Target 与 Search Mode。',
        'GPIO 配置：在本页中同时提供 GPI 配置与 GPO 输出下发功能。',
        '天线配置：可逐天线设置启用状态、发射功率以及接收灵敏度。'
    )
    Add-Paragraph '页底操作按钮说明：'
    Add-Bullets @(
        '获取参数：从设备读取当前配置。',
        '保存参数：将当前界面参数下发到设备。',
        '获取设备状态：读取当前设备连接与运行状态。',
        '恢复出厂：将设备参数恢复到默认状态。'
    )
    Add-Image $images.Settings1 '图4-2 参数配置页面（上半部分）'
    Add-Image $images.Settings2 '图4-3 参数配置页面（下半部分）'

    Add-Paragraph '4.3 盘点配置' 'Heading 2' '黑体' 14 $true
    Add-Paragraph '盘点配置页用于设置标签盘点过程中的触发方式、筛选条件和数据上报方式。该页适合在正式盘点前进行策略预设。'
    Add-Paragraph '本页包括以下配置区域：'
    Add-Bullets @(
        '自动开始配置：可设置盘点开始模式、GPI 端口号、GPI 电平、首次延时、周期和 UTC 时间戳。',
        '自动停止配置：可设置停止模式、持续时长、GPI 端口号、GPI 电平以及超时参数。',
        '筛选配置：支持 TagFilter1、TagFilter2 以及 TagSelectFilters 等筛选方式。',
        '数据上报策略：可设置上报模式，并选择是否附带天线号、信道、首次时间、末次时间、次数、峰值 RSSI、PC 与 CRC。',
        '附加数据配置：可配置盘点时附加读取的内存区、起始字地址、读取字数和访问密码。'
    )
    Add-Paragraph '页底按钮支持从缓存加载、从设备获取参数以及保存盘点配置。'
    Add-Image $images.InventoryCfg1 '图4-4 盘点配置页面（触发策略）'
    Add-Image $images.InventoryCfg2 '图4-5 盘点配置页面（筛选配置）'
    Add-Image $images.InventoryCfg3 '图4-6 盘点配置页面（上报与附加数据配置）'

    Add-Paragraph '4.4 盘点操作' 'Heading 2' '黑体' 14 $true
    Add-Paragraph '盘点操作页用于执行实际标签盘点。页面顶部提供“开始寻卡”“停止寻卡”“手动拉缓存”“清空数据”等操作按钮，并实时显示报告包数量、标签总数、唯一 EPC 数量和盘点耗时。'
    Add-Paragraph '盘点结果表格可显示接收时间、EPC、天线号、频率、RSSI、出现次数、PC、CRC 以及附加数据等信息。用户还可以通过右键菜单复制 EPC、附加数据或选中行内容。'
    Add-Numbered @(
        '确认读写器已连接。',
        '如有需要，先在“盘点配置”页设置盘点策略。',
        '进入“盘点操作”页，单击“开始寻卡”。',
        '观察统计指标和标签结果列表。',
        '结束时单击“停止寻卡”；如采用 WaitForQuery 模式，可使用“手动拉缓存”获取缓存报告。'
    )
    Add-Image $images.Inventory '图4-7 盘点操作页面'

    Add-Paragraph '4.5 读写操作' 'Heading 2' '黑体' 14 $true
    Add-Paragraph '读写操作页用于对指定标签执行标准内存读写。页面采用颜色提示区分读操作必填项、写操作必填项以及同时必填项。'
    Add-Paragraph '常用字段说明：'
    Add-Bullets @(
        '目标标签类型：指定目标标签匹配所使用的内存区类型。',
        '目标标签：输入要匹配的标签值，通常使用十六进制格式。',
        '内存区：选择需要读取或写入的标签内存区，例如 EPC、TID、USER。',
        'Word 起始地址：指定操作起始 Word 地址。',
        'Word 数量：读取操作需要填写的读取字数。',
        '写入数据：写操作需要填写的十六进制数据，长度需满足 Word 对齐要求。',
        '访问密码：默认可为 00000000，若标签设置了访问密码则需填写正确值。'
    )
    Add-Paragraph '操作区支持“读取内存”“写入内存”“块写入”和“清空数据”。执行后，结果会显示在页面下方的十六进制数据区域与状态提示信息中。'
    Add-Image $images.ReadWrite '图4-8 读写操作页面'

    Add-Paragraph '4.6 高级标签操作' 'Heading 2' '黑体' 14 $true
    Add-Paragraph '高级标签操作页提供 BlockErase、Lock 和 Kill 等高风险操作，用于高级调试或生产控制场景。由于相关操作可能导致标签数据不可恢复或标签永久失效，建议仅在确认标签、参数和业务后执行。'
    Add-Bullets @(
        'BlockErase：按指定内存区、起始地址和字数执行块擦除。',
        'Lock：对指定标签的特定数据区执行锁定或解锁动作。',
        'Kill：使用 Kill 密码使标签永久失效。'
    )
    Add-Paragraph '使用建议：'
    Add-Numbered @(
        '先在测试标签上验证参数。',
        '确认目标标签值、访问密码和 Kill 密码正确无误。',
        '仅在业务确有需要时执行 Lock 或 Kill 操作。'
    )
    Add-Image $images.Adv1 '图4-9 高级标签操作页面（公共参数与 BlockErase）'
    Add-Image $images.Adv2 '图4-10 高级标签操作页面（Lock、Kill 与结果）'

    Add-Paragraph '4.7 日志' 'Heading 2' '黑体' 14 $true
    Add-Paragraph '日志页用于查看软件运行过程中的日志信息。页面支持分别查看操作日志、LLRP 消息日志和原始报文收发日志，也可以单独或全部清空。'
    Add-Bullets @(
        '操作日志：记录连接、配置下发、盘点、读写等业务行为。',
        'LLRP 消息日志：记录协议层消息处理过程。',
        '原始报文收发：记录收发的原始数据帧，便于联调分析。'
    )
    Add-Image $images.Log '图4-11 日志页面'

    Add-Paragraph '4.8 历史LLRP消息' 'Heading 2' '黑体' 14 $true
    Add-Paragraph '历史LLRP消息页用于离线查看历史原始帧记录。页面支持按开始日期、结束日期、方向、设备和消息类型进行筛选，并可将结果导出为文本。'
    Add-Paragraph '页面主要由三部分组成：'
    Add-Bullets @(
        '原始帧列表：按时间顺序展示已采集的 LLRP 原始帧。',
        'LLRP 消息树：将选中帧解析为结构化消息树，便于查看各层字段。',
        '原始十六进制数据：显示当前帧的原始 HEX 数据。'
    )
    Add-Image $images.LlrpHistory '图4-12 历史LLRP消息页面'

    Add-Paragraph '5 注意事项' 'Heading 1' '黑体' 16 $true
    Add-Bullets @(
        '连接设备前，请确认读写器网络可达，且 IP 地址与端口设置正确。',
        '执行盘点、读写和高级标签操作前，建议先完成参数配置并确认天线、功率与协议参数。',
        '高级标签操作具有风险，尤其是 Kill 与 BlockErase，应先在测试环境验证。',
        '若需要分析协议问题，优先结合“日志”和“历史LLRP消息”页面排查。',
        '如设备在连接前已处于盘点状态，软件连接后可能自动停止盘点，以便接管控制流程。'
    )

    Add-Paragraph '6 常见操作流程示例' 'Heading 1' '黑体' 16 $true
    Add-Numbered @(
        '连接读写器：进入“设备连接”页，填写 IP 地址并单击“连接”。',
        '读取参数：进入“参数配置”页，单击“获取参数”。',
        '设置盘点策略：进入“盘点配置”页，根据需要设置触发、筛选和上报方式后单击“保存盘点配置”。',
        '执行盘点：进入“盘点操作”页，单击“开始寻卡”并查看标签结果。',
        '读写标签：进入“读写操作”页，填写目标标签和内存参数后执行读取或写入。',
        '分析问题：进入“日志”或“历史LLRP消息”页查看日志、原始帧和消息树。'
    )

    foreach ($toc in $doc.TablesOfContents) {
        $toc.Update()
    }

    $doc.SaveAs2([ref]$updatedDocx, [ref]16)
    $doc.SaveAs2([ref]$originalDoc, [ref]0)
}
finally {
    if ($doc -ne $null) {
        $doc.Close()
    }
    if ($word -ne $null) {
        $word.Quit()
    }

    if ($sel -ne $null) {
        [void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($sel)
    }
    if ($doc -ne $null) {
        [void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($doc)
    }
    if ($word -ne $null) {
        [void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($word)
    }

    [GC]::Collect()
    [GC]::WaitForPendingFinalizers()
}

Write-Output ('Backup: ' + $backupDoc)
Write-Output ('Updated DOCX: ' + $updatedDocx)
Write-Output ('Updated DOC: ' + $originalDoc)
