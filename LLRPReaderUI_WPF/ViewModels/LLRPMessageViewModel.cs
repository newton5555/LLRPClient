using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LLRPReaderUI_WPF.Data;
using LLRPReaderUI_WPF.Logging;
using LLRPReaderUI_WPF.Models;
using LLRPSdk;
using Org.LLRP.LTK.LLRPV1;
using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml.Linq;

namespace LLRPReaderUI_WPF.ViewModels
{
    public partial class LLRPMessageViewModel : ObservableObject
    {
        private readonly LlrpReader _reader;
        private readonly IAppLogService _logs;
        private readonly IRawFrameRepository _rawFrameRepository;

        [ObservableProperty]
        private ObservableCollection<RawFrameEntity> _rawFrames = new ObservableCollection<RawFrameEntity>();

        [ObservableProperty]
        private RawFrameEntity? _selectedRawFrame;

        [ObservableProperty]
        private ObservableCollection<LLRPMessageNode> _messageTree = new ObservableCollection<LLRPMessageNode>();

        [ObservableProperty]
        private LLRPMessageNode? _selectedMessageNode;

        [ObservableProperty]
        private string _rawHexString = string.Empty;

        [ObservableProperty]
        private string _statusText = "就绪";

        [ObservableProperty]
        private bool _isLoading;



        public LLRPMessageViewModel(
            LlrpReader reader,
            IAppLogService logs,
            IRawFrameRepository rawFrameRepository)
        {
            _reader = reader;
            _logs = logs;
            _rawFrameRepository = rawFrameRepository;

            BindingOperations.EnableCollectionSynchronization(RawFrames, new object());
            BindingOperations.EnableCollectionSynchronization(MessageTree, new object());

            _ = LoadRawFrames();
        }

        [RelayCommand]
        private async Task LoadRawFrames()
        {
            IsLoading = true;
            StatusText = "正在加载原始帧数据...";
            try
            {
                var frames = await _rawFrameRepository.GetRecentAsync(1000);
                RawFrames.Clear();
                foreach (var frame in frames.OrderBy(f => f.Timestamp))
                {
                    RawFrames.Add(frame);
                }
                StatusText = $"已加载 {RawFrames.Count} 条原始帧记录";
            }
            catch (Exception ex)
            {
                StatusText = $"加载数据失败: {ex.Message}";
                _logs.LogOperation($"加载原始帧数据失败: {ex.Message}", Microsoft.Extensions.Logging.LogLevel.Error, ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnSelectedRawFrameChanged(RawFrameEntity? value)
        {
            MessageTree.Clear();
            RawHexString = string.Empty;
            if (value?.Payload == null) return;

            RawHexString = LlrpDisplayHelper.FormatHex(value.Payload);
            ParseLLRPMessage(value.Payload);
        }

        private void ParseLLRPMessage(byte[] payload)
        {
            MessageTree.Clear();
            try
            {
                //根节点直接显示Decode_Envelope解析出来的内容
                LLRPBinaryDecoder.Decode_Envelope(payload, out var env);

                string details = $"{((ENUM_LLRP_MSG_TYPE)env.msg_type)}  Length={env.msg_len}";

                var root = new LLRPMessageNode($"MSG V{env.ver.ToString()} {SelectedRawFrame?.Direction} ID={env.msg_id}", details);
                
                MessageTree.Add(root);


                // 使用LLRPBinaryDecoder进行完整解析
                TryParseWithLLRPBinaryDecoder(payload, root);
            }
            catch (Exception ex)
            {
                StatusText = $"解析LLRP消息时出错: {ex.Message}";
                var errorNode = new LLRPMessageNode("LLRP Message", description: "解析失败");
                errorNode.AddChild("错误信息", ex.Message);
                errorNode.AddChild("原始数据 (Hex)", BitConverter.ToString(payload).Replace("-", " "));
                MessageTree.Add(errorNode);
            }
        }

        private void TryParseWithLLRPBinaryDecoder(byte[] payload, LLRPMessageNode root)
        {
            try
            {
                //  尝试解析完整消息
                LLRPBinaryDecoder.Decode(ref payload, out var message);
                if (message != null)
                {
                    var msgNode = root.AddChild("消息内容 (Message Body)", description: message.GetType().Name);
                    ParseSpecificMessage(message, msgNode);
                }
                else
                {
                    root.AddChild("解析结果", "消息体为空", "LLRPBinaryDecoder未能解析出有效的消息对象");
                }
            }
            catch (Exception ex)
            {
                root.AddChild("LLRP解析错误", ex.Message, "可能由于数据包不完整或格式错误导致");
            }
        }

        private void ParseSpecificMessage(Message message, LLRPMessageNode parentNode)
        {
            parentNode.AddChild("消息类型", message.GetType().Name);

            if (message is MSG_GET_READER_CAPABILITIES getReaderCapabilities)
            {
                parentNode.Children.Add(getReaderCapabilities.BuildTreeNode());
                return;
            }
            else if (message is MSG_GET_READER_CAPABILITIES_RESPONSE getReaderCapabilitiesResponse)
            {
                parentNode.Children.Add(getReaderCapabilitiesResponse.BuildTreeNode());
                return;
            }
            else if (message is MSG_RO_ACCESS_REPORT roAccessReport)
            {
                parentNode.Children.Add(roAccessReport.BuildTreeNode());
                return;
            }
            else if (message is MSG_READER_EVENT_NOTIFICATION readerEventNotification)
            {
                parentNode.Children.Add(readerEventNotification.BuildTreeNode());
                return;
            }
            else if (message is MSG_ERROR_MESSAGE errorMessage)
            {
                parentNode.Children.Add(errorMessage.BuildTreeNode());
                return;
            }



        }

 
       

      


   


     

       

        [RelayCommand] private void ClearSelection() { SelectedRawFrame = null; }
        [RelayCommand] private async Task RefreshData() => await LoadRawFrames();

        [RelayCommand]
        private void ExportToText()
        {
            if (SelectedRawFrame == null || MessageTree.Count == 0)
            {
                StatusText = "请先选择一条消息";
                return;
            }
            try
            {
                var treeText = MessageTree[0].BuildTreeString();
                var fileName = $"LLRP_Message_{SelectedRawFrame.Timestamp:yyyyMMdd_HHmmss}_{SelectedRawFrame.Direction}.txt";
                System.IO.File.WriteAllText(fileName, treeText);
                StatusText = $"已导出到文件: {fileName}";
            }
            catch (Exception ex)
            {
                StatusText = $"导出失败: {ex.Message}";
            }
        }
       
    }

  


    public static class LLRPMessageExtensions
    {
        // -----------------------------
        // PARAM_* 手写树（参考各自 ToString() 的 XML 层级与顺序）
        // -----------------------------

        public static LLRPMessageNode BuildTreeNode(this PARAM_LLRPStatus p)
        {
            var root = new LLRPMessageNode("LLRPStatus");
            // ToString(): StatusCode -> ErrorDescription -> FieldError -> ParameterError
            root.AddChild("StatusCode", p.StatusCode.ToString());
            if (!string.IsNullOrEmpty(p.ErrorDescription))
                root.AddChild("ErrorDescription", p.ErrorDescription);

            if (p.FieldError != null) root.AddChild("FieldError").AddChild("ToString()", p.FieldError.ToString());
            if (p.ParameterError != null) root.AddChild("ParameterError").AddChild("ToString()", p.ParameterError.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_LLRPCapabilities p)
        {
            var root = new LLRPMessageNode("LLRPCapabilities");
            // ToString() 顺序
            root.AddChild("CanDoRFSurvey", p.CanDoRFSurvey.ToString());
            root.AddChild("CanReportBufferFillWarning", p.CanReportBufferFillWarning.ToString());
            root.AddChild("SupportsClientRequestOpSpec", p.SupportsClientRequestOpSpec.ToString());
            root.AddChild("CanDoTagInventoryStateAwareSingulation", p.CanDoTagInventoryStateAwareSingulation.ToString());
            root.AddChild("SupportsEventAndReportHolding", p.SupportsEventAndReportHolding.ToString());
            root.AddChild("MaxNumPriorityLevelsSupported", p.MaxNumPriorityLevelsSupported.ToString());
            root.AddChild("ClientRequestOpSpecTimeout", p.ClientRequestOpSpecTimeout.ToString());
            root.AddChild("MaxNumROSpecs", p.MaxNumROSpecs.ToString());
            root.AddChild("MaxNumSpecsPerROSpec", p.MaxNumSpecsPerROSpec.ToString());
            root.AddChild("MaxNumInventoryParameterSpecsPerAISpec", p.MaxNumInventoryParameterSpecsPerAISpec.ToString());
            root.AddChild("MaxNumAccessSpecs", p.MaxNumAccessSpecs.ToString());
            root.AddChild("MaxNumOpSpecsPerAccessSpec", p.MaxNumOpSpecsPerAccessSpec.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_C1G2LLRPCapabilities p)
        {
            var root = new LLRPMessageNode("C1G2LLRPCapabilities");
            root.AddChild("CanSupportBlockErase", p.CanSupportBlockErase.ToString());
            root.AddChild("CanSupportBlockWrite", p.CanSupportBlockWrite.ToString());
            root.AddChild("MaxNumSelectFiltersPerQuery", p.MaxNumSelectFiltersPerQuery.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_RegulatoryCapabilities p)
        {
            var root = new LLRPMessageNode("RegulatoryCapabilities");
            // ToString(): CountryCode -> CommunicationsStandard -> UHFBandCapabilities -> Custom*
            root.AddChild("CountryCode", p.CountryCode.ToString());
            root.AddChild("CommunicationsStandard", p.CommunicationsStandard.ToString());

            if (p.UHFBandCapabilities != null)
                root.AddChild("UHFBandCapabilities").AddChild("ToString()", p.UHFBandCapabilities.ToString());

            if (p.Custom != null && p.Custom.Length > 0)
            {
                var customNode = root.AddChild("Custom", $"Count={p.Custom.Length}");
                for (int i = 0; i < p.Custom.Length; i++)
                {
                    var c = p.Custom[i];
                    customNode.AddChild($"Custom[{i}]", description: c?.GetType().Name)
                        .AddChild("ToString()", c?.ToString() ?? "null");
                }
            }

            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_GeneralDeviceCapabilities p)
        {
            var root = new LLRPMessageNode("GeneralDeviceCapabilities");
            // ToString(): MaxNumberOfAntennaSupported -> CanSetAntennaProperties -> HasUTCClockCapability ->
            //            DeviceManufacturerName -> ModelName -> ReaderFirmwareVersion ->
            //            ReceiveSensitivityTableEntry* -> PerAntennaReceiveSensitivityRange* -> GPIOCapabilities ->
            //            PerAntennaAirProtocol*
            root.AddChild("MaxNumberOfAntennaSupported", p.MaxNumberOfAntennaSupported.ToString());
            root.AddChild("CanSetAntennaProperties", p.CanSetAntennaProperties.ToString());
            root.AddChild("HasUTCClockCapability", p.HasUTCClockCapability.ToString());
            root.AddChild("DeviceManufacturerName", p.DeviceManufacturerName.ToString());
            root.AddChild("ModelName", p.ModelName.ToString());
            if (!string.IsNullOrEmpty(p.ReaderFirmwareVersion))
                root.AddChild("ReaderFirmwareVersion", p.ReaderFirmwareVersion);

            if (p.ReceiveSensitivityTableEntry != null && p.ReceiveSensitivityTableEntry.Length > 0)
            {
                var node = root.AddChild("ReceiveSensitivityTableEntry", $"Count={p.ReceiveSensitivityTableEntry.Length}");
                for (int i = 0; i < p.ReceiveSensitivityTableEntry.Length; i++)
                {
                    var item = p.ReceiveSensitivityTableEntry[i];
                    node.AddChild($"ReceiveSensitivityTableEntry[{i}]", description: item?.GetType().Name)
                        .AddChild("ToString()", item?.ToString() ?? "null");
                }
            }

            if (p.PerAntennaReceiveSensitivityRange != null && p.PerAntennaReceiveSensitivityRange.Length > 0)
            {
                var node = root.AddChild("PerAntennaReceiveSensitivityRange", $"Count={p.PerAntennaReceiveSensitivityRange.Length}");
                for (int i = 0; i < p.PerAntennaReceiveSensitivityRange.Length; i++)
                {
                    var item = p.PerAntennaReceiveSensitivityRange[i];
                    node.AddChild($"PerAntennaReceiveSensitivityRange[{i}]", description: item?.GetType().Name)
                        .AddChild("ToString()", item?.ToString() ?? "null");
                }
            }

            if (p.GPIOCapabilities != null)
                root.AddChild("GPIOCapabilities").AddChild("ToString()", p.GPIOCapabilities.ToString());

            if (p.PerAntennaAirProtocol != null && p.PerAntennaAirProtocol.Length > 0)
            {
                var node = root.AddChild("PerAntennaAirProtocol", $"Count={p.PerAntennaAirProtocol.Length}");
                for (int i = 0; i < p.PerAntennaAirProtocol.Length; i++)
                {
                    var item = p.PerAntennaAirProtocol[i];
                    node.AddChild($"PerAntennaAirProtocol[{i}]", description: item?.GetType().Name)
                        .AddChild("ToString()", item?.ToString() ?? "null");
                }
            }

            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_RFSurveyReportData p)
        {
            var root = new LLRPMessageNode("RFSurveyReportData");
            // ToString(): ROSpecID -> SpecIndex -> FrequencyRSSILevelEntry* -> Custom*
            if (p.ROSpecID != null) root.AddChild("ROSpecID").AddChild("ToString()", p.ROSpecID.ToString());
            if (p.SpecIndex != null) root.AddChild("SpecIndex").AddChild("ToString()", p.SpecIndex.ToString());

            if (p.FrequencyRSSILevelEntry != null && p.FrequencyRSSILevelEntry.Length > 0)
            {
                var node = root.AddChild("FrequencyRSSILevelEntry", $"Count={p.FrequencyRSSILevelEntry.Length}");
                for (int i = 0; i < p.FrequencyRSSILevelEntry.Length; i++)
                {
                    var item = p.FrequencyRSSILevelEntry[i];
                    node.AddChild($"FrequencyRSSILevelEntry[{i}]", description: item?.GetType().Name)
                        .AddChild("ToString()", item?.ToString() ?? "null");
                }
            }

            if (p.Custom != null && p.Custom.Length > 0)
            {
                var customNode = root.AddChild("Custom", $"Count={p.Custom.Length}");
                for (int i = 0; i < p.Custom.Length; i++)
                {
                    var c = p.Custom[i];
                    customNode.AddChild($"Custom[{i}]", description: c?.GetType().Name)
                        .AddChild("ToString()", c?.ToString() ?? "null");
                }
            }

            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_TagReportData p)
        {
            var root = new LLRPMessageNode("TagReportData");
            // ToString() 顺序：EPCParameter* -> ROSpecID -> SpecIndex -> InventoryParameterSpecID ->
            //             AntennaID -> PeakRSSI -> ChannelIndex ->
            //             FirstSeenTimestampUTC -> FirstSeenTimestampUptime ->
            //             LastSeenTimestampUTC -> LastSeenTimestampUptime ->
            //             TagSeenCount ->
            //             AirProtocolTagData* ->
            //             AccessSpecID ->
            //             AccessCommandOpSpecResult* ->
            //             Custom*
            if (p.EPCParameter != null)
            {
                var node = root.AddChild("EPCParameter", $"Count={p.EPCParameter.Count}");
                for (int i = 0; i < p.EPCParameter.Count; i++)
                {
                    var item = p.EPCParameter[i];
                    node.AddChild($"[{i}] {item?.GetType().Name}", description: item?.GetType().Name)
                        .AddChild("ToString()", item?.ToString() ?? "null");
                }
            }

            if (p.ROSpecID != null) root.AddChild("ROSpecID").AddChild("ToString()", p.ROSpecID.ToString());
            if (p.SpecIndex != null) root.AddChild("SpecIndex").AddChild("ToString()", p.SpecIndex.ToString());
            if (p.InventoryParameterSpecID != null) root.AddChild("InventoryParameterSpecID").AddChild("ToString()", p.InventoryParameterSpecID.ToString());
            if (p.AntennaID != null) root.AddChild("AntennaID").AddChild("ToString()", p.AntennaID.ToString());
            if (p.PeakRSSI != null) root.AddChild("PeakRSSI").AddChild("ToString()", p.PeakRSSI.ToString());
            if (p.ChannelIndex != null) root.AddChild("ChannelIndex").AddChild("ToString()", p.ChannelIndex.ToString());

            if (p.FirstSeenTimestampUTC != null) root.AddChild("FirstSeenTimestampUTC").AddChild("ToString()", p.FirstSeenTimestampUTC.ToString());
            if (p.FirstSeenTimestampUptime != null) root.AddChild("FirstSeenTimestampUptime").AddChild("ToString()", p.FirstSeenTimestampUptime.ToString());
            if (p.LastSeenTimestampUTC != null) root.AddChild("LastSeenTimestampUTC").AddChild("ToString()", p.LastSeenTimestampUTC.ToString());
            if (p.LastSeenTimestampUptime != null) root.AddChild("LastSeenTimestampUptime").AddChild("ToString()", p.LastSeenTimestampUptime.ToString());
            if (p.TagSeenCount != null) root.AddChild("TagSeenCount").AddChild("ToString()", p.TagSeenCount.ToString());

            if (p.AirProtocolTagData != null)
            {
                var node = root.AddChild("AirProtocolTagData", $"Count={p.AirProtocolTagData.Count}");
                for (int i = 0; i < p.AirProtocolTagData.Count; i++)
                {
                    var item = p.AirProtocolTagData[i];
                    node.AddChild($"[{i}] {item?.GetType().Name}", description: item?.GetType().Name)
                        .AddChild("ToString()", item?.ToString() ?? "null");
                }
            }

            if (p.AccessSpecID != null) root.AddChild("AccessSpecID").AddChild("ToString()", p.AccessSpecID.ToString());

            if (p.AccessCommandOpSpecResult != null)
            {
                var node = root.AddChild("AccessCommandOpSpecResult", $"Count={p.AccessCommandOpSpecResult.Count}");
                for (int i = 0; i < p.AccessCommandOpSpecResult.Count; i++)
                {
                    var item = p.AccessCommandOpSpecResult[i];
                    node.AddChild($"[{i}] {item?.GetType().Name}", description: item?.GetType().Name)
                        .AddChild("ToString()", item?.ToString() ?? "null");
                }
            }

            if (p.Custom != null && p.Custom.Length > 0)
            {
                var customNode = root.AddChild("Custom", $"Count={p.Custom.Length}");
                for (int i = 0; i < p.Custom.Length; i++)
                {
                    var c = p.Custom[i];
                    customNode.AddChild($"Custom[{i}]", description: c?.GetType().Name)
                        .AddChild("ToString()", c?.ToString() ?? "null");
                }
            }

            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_ReaderEventNotificationData p)
        {
            var root = new LLRPMessageNode("ReaderEventNotificationData");
            // ToString(): Timestamp* -> HoppingEvent -> GPIEvent -> ROSpecEvent -> ReportBufferLevelWarningEvent ->
            //            ReportBufferOverflowErrorEvent -> ReaderExceptionEvent -> RFSurveyEvent -> AISpecEvent ->
            //            AntennaEvent -> ConnectionAttemptEvent -> ConnectionCloseEvent -> Custom*
            if (p.Timestamp != null)
            {
                var node = root.AddChild("Timestamp", $"Count={p.Timestamp.Count}");
                for (int i = 0; i < p.Timestamp.Count; i++)
                {
                    var item = p.Timestamp[i];
                    node.AddChild($"[{i}] {item?.GetType().Name}", description: item?.GetType().Name)
                        .AddChild("ToString()", item?.ToString() ?? "null");
                }
            }

            if (p.HoppingEvent != null) root.AddChild("HoppingEvent").AddChild("ToString()", p.HoppingEvent.ToString());
            if (p.GPIEvent != null) root.AddChild("GPIEvent").AddChild("ToString()", p.GPIEvent.ToString());
            if (p.ROSpecEvent != null) root.AddChild("ROSpecEvent").AddChild("ToString()", p.ROSpecEvent.ToString());
            if (p.ReportBufferLevelWarningEvent != null) root.AddChild("ReportBufferLevelWarningEvent").AddChild("ToString()", p.ReportBufferLevelWarningEvent.ToString());
            if (p.ReportBufferOverflowErrorEvent != null) root.AddChild("ReportBufferOverflowErrorEvent").AddChild("ToString()", p.ReportBufferOverflowErrorEvent.ToString());
            if (p.ReaderExceptionEvent != null) root.AddChild("ReaderExceptionEvent").AddChild("ToString()", p.ReaderExceptionEvent.ToString());
            if (p.RFSurveyEvent != null) root.AddChild("RFSurveyEvent").AddChild("ToString()", p.RFSurveyEvent.ToString());
            if (p.AISpecEvent != null) root.AddChild("AISpecEvent").AddChild("ToString()", p.AISpecEvent.ToString());
            if (p.AntennaEvent != null) root.AddChild("AntennaEvent").AddChild("ToString()", p.AntennaEvent.ToString());
            if (p.ConnectionAttemptEvent != null) root.AddChild("ConnectionAttemptEvent").AddChild("ToString()", p.ConnectionAttemptEvent.ToString());
            if (p.ConnectionCloseEvent != null) root.AddChild("ConnectionCloseEvent").AddChild("ToString()", p.ConnectionCloseEvent.ToString());

            if (p.Custom != null && p.Custom.Length > 0)
            {
                var customNode = root.AddChild("Custom", $"Count={p.Custom.Length}");
                for (int i = 0; i < p.Custom.Length; i++)
                {
                    var c = p.Custom[i];
                    customNode.AddChild($"Custom[{i}]", description: c?.GetType().Name)
                        .AddChild("ToString()", c?.ToString() ?? "null");
                }
            }

            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_GET_READER_CAPABILITIES msg)
        {
            var root = new LLRPMessageNode("GET_READER_CAPABILITIES", description: $"MessageID={msg.MSG_ID}");
            // 参考 SDK 的 ToString()：根元素属性 Version / MessageID
            root.AddChild("Version", msg.VERSION.ToString());
            root.AddChild("MessageID", msg.MSG_ID.ToString());
            root.AddChild("RequestedData", LlrpDisplayHelper.FormatEnum(msg.RequestedData));
            if (msg.Custom != null && msg.Custom.Length > 0)
            {
                // ToString() 里是把每个 Custom 参数作为子元素直接拼出来；这里用容器仅用于 UI 归类
                var customNode = root.AddChild("Custom", $"Count={msg.Custom.Length}");
                for (int i = 0; i < msg.Custom.Length; i++)
                {
                    var param = msg.Custom[i];
                    var child = customNode.AddChild($"Custom[{i}]", description: param?.GetType().Name);
                    if (param == null)
                    {
                        child.AddChild("值", "null");
                        continue;
                    }
                    child.AddChild("ToString()", param.ToString());
                }
            }
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_GET_READER_CAPABILITIES_RESPONSE msg)
        {
            var root = new LLRPMessageNode("GET_READER_CAPABILITIES_RESPONSE", description: $"MessageID={msg.MSG_ID}");
            root.AddChild("Version", msg.VERSION.ToString());
            root.AddChild("MessageID", msg.MSG_ID.ToString());
            // 参考 ToString() 的层级与顺序：LLRPStatus → GeneralDeviceCapabilities → LLRPCapabilities → RegulatoryCapabilities → AirProtocolLLRPCapabilities(items...) → Custom(items...)
            if (msg.LLRPStatus != null)
            {
                root.Children.Add(msg.LLRPStatus.BuildTreeNode());
            }
            if (msg.GeneralDeviceCapabilities != null)
            {
                root.Children.Add(msg.GeneralDeviceCapabilities.BuildTreeNode());
            }
            if (msg.LLRPCapabilities != null)
            {
                root.Children.Add(msg.LLRPCapabilities.BuildTreeNode());

            }
            if (msg.RegulatoryCapabilities != null)
            {
                root.Children.Add(msg.RegulatoryCapabilities.BuildTreeNode());
            }
            if (msg.AirProtocolLLRPCapabilities != null)
            {
                // ToString() 中每个 AirProtocolLLRPCapabilities 项是直接作为根的子元素输出（例如 C1G2LLRPCapabilities）
                for (int i = 0; i < msg.AirProtocolLLRPCapabilities.Count; i++)
                {
                    var item = msg.AirProtocolLLRPCapabilities[i];
                    // 目前先覆盖常见的 C1G2LLRPCapabilities
                    if (item is PARAM_C1G2LLRPCapabilities c1g2)
                        root.Children.Add(c1g2.BuildTreeNode());
                    else
                        root.AddChild(item.GetType().Name).AddChild("ToString()", item?.ToString() ?? "null");
                }
                
            }
            if (msg.Custom != null && msg.Custom.Length > 0)
            {
                var customNode = root.AddChild("Custom", $"Count={msg.Custom.Length}");
                for (int i = 0; i < msg.Custom.Length; i++)
                {
                    var param = msg.Custom[i];
                    var child = customNode.AddChild($"Custom[{i}]", description: param?.GetType().Name);
                    if (param == null)
                    {
                        child.AddChild("值", "null");
                        continue;
                    }
                    child.AddChild("ToString()", param.ToString());
                }
            }
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_RO_ACCESS_REPORT msg)
        {
            var root = new LLRPMessageNode("RO_ACCESS_REPORT", description: $"MessageID={msg.MSG_ID}");
            root.AddChild("Version", msg.VERSION.ToString());
            root.AddChild("MessageID", msg.MSG_ID.ToString());
            // 参考 ToString()：TagReportData、RFSurveyReportData、Custom 都是根的直接子元素（重复多次）
            if (msg.TagReportData != null)
            {
                for (int i = 0; i < msg.TagReportData.Length; i++)
                {
                    var tag = msg.TagReportData[i];
                    if (tag != null)
                        root.Children.Add(tag.BuildTreeNode());
                }
            }
            if (msg.RFSurveyReportData != null)
            {
                for (int i = 0; i < msg.RFSurveyReportData.Length; i++)
                {
                    var item = msg.RFSurveyReportData[i];
                    if (item != null)
                        root.Children.Add(item.BuildTreeNode());
                }
            }
            if (msg.Custom != null && msg.Custom.Length > 0)
            {
                var customNode = root.AddChild("Custom", $"Count={msg.Custom.Length}");
                for (int i = 0; i < msg.Custom.Length; i++)
                {
                    var param = msg.Custom[i];
                    var child = customNode.AddChild($"Custom[{i}]", description: param?.GetType().Name);
                    if (param == null)
                    {
                        child.AddChild("值", "null");
                        continue;
                    }
                    child.AddChild("ToString()", param.ToString());
                }
            }
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_READER_EVENT_NOTIFICATION msg)
        {
            var root = new LLRPMessageNode("READER_EVENT_NOTIFICATION", description: $"MessageID={msg.MSG_ID}");
            root.AddChild("Version", msg.VERSION.ToString());
            root.AddChild("MessageID", msg.MSG_ID.ToString());
            if (msg.ReaderEventNotificationData != null)
            {
                root.Children.Add(msg.ReaderEventNotificationData.BuildTreeNode());
            }
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_ERROR_MESSAGE msg)
        {
            var root = new LLRPMessageNode("ERROR_MESSAGE", description: $"MessageID={msg.MSG_ID}");
            root.AddChild("Version", msg.VERSION.ToString());
            root.AddChild("MessageID", msg.MSG_ID.ToString());
            if (msg.LLRPStatus != null)
            {
                root.Children.Add(msg.LLRPStatus.BuildTreeNode());
            }
            return root;
        }
    

    }

    /// <summary>
    /// 提供用于在UI中清晰显示LLRP数据的静态辅助方法。
    /// </summary>
    public static class LlrpDisplayHelper
    {
        /// <summary>
        /// 将LLRP UTC微秒时间戳转换为本地化的、人类可读的字符串。
        /// </summary>
        public static string FormatUtcMicroseconds(ulong microseconds)
        {
            try
            {
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                // 1微秒 = 10 Ticks
                var dateTime = epoch.AddTicks((long)microseconds * 10);
                // 转换为本地时间以便查看
                return dateTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.ffffff");
            }
            catch
            {
                return $"{microseconds} µs (转换失败)";
            }
        }

       

        /// <summary>
        /// 获取枚举值的名称，如果失败则返回其数字值。
        /// </summary>
        public static string FormatEnum(object? enumValue)
        {
            if (enumValue == null) return "N/A";
            var type = enumValue.GetType();
            if (!type.IsEnum) return enumValue.ToString() ?? "N/A";

            try
            {
                // 显示名称和数字值，更清晰
                return $"{Enum.GetName(type, enumValue)} ({(int)enumValue})";
            }
            catch
            {
                return enumValue.ToString() ?? "N/A";
            }
        }


        public static string FormatHex(byte[] payload, int bytesPerLine = 16)
        {
            if (payload == null || payload.Length == 0) return string.Empty;
            var sb = new StringBuilder();
            for (int i = 0; i < payload.Length; i++)
            {
                if (i % bytesPerLine == 0)
                {
                    if (i > 0) sb.AppendLine();
                    sb.Append(i.ToString("X4")).Append(": ");
                }
                sb.Append(payload[i].ToString("X2")).Append(" ");
            }
            return sb.ToString();
        }


    }
}
