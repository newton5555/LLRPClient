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

                string details = $"{env.msg_type}  Length={env.msg_len}";

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
                    //ParseSpecificMessage(message, msgNode);

                    ParseSpecificMessageXML(message, msgNode);
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

            switch (message)
            {
                case MSG_GET_READER_CAPABILITIES getReaderCapabilities:
                    parentNode.Children.Add(getReaderCapabilities.BuildTreeNode());
                    return;
                case MSG_GET_READER_CAPABILITIES_RESPONSE getReaderCapabilitiesResponse:
                    parentNode.Children.Add(getReaderCapabilitiesResponse.BuildTreeNode());
                    return;
                case MSG_RO_ACCESS_REPORT roAccessReport:
                    parentNode.Children.Add(roAccessReport.BuildTreeNode());
                    return;
                case MSG_READER_EVENT_NOTIFICATION readerEventNotification:
                    parentNode.Children.Add(readerEventNotification.BuildTreeNode());
                    return;
                case MSG_ERROR_MESSAGE errorMessage:
                    parentNode.Children.Add(errorMessage.BuildTreeNode());
                    return;
                case MSG_SET_READER_CONFIG setReaderConfig:
                    parentNode.Children.Add(setReaderConfig.BuildTreeNode());
                    return;
                case MSG_ADD_ROSPEC addRoSpec:
                    parentNode.Children.Add(addRoSpec.BuildTreeNode());
                    return;
                case MSG_ADD_ROSPEC_RESPONSE addRoSpecResponse:
                    parentNode.Children.Add(addRoSpecResponse.BuildTreeNode());
                    return;
                case MSG_START_ROSPEC startRoSpec:
                    parentNode.Children.Add(startRoSpec.BuildTreeNode());
                    return;
                case MSG_START_ROSPEC_RESPONSE startRoSpecResponse:
                    parentNode.Children.Add(startRoSpecResponse.BuildTreeNode());
                    return;
                case MSG_STOP_ROSPEC stopRoSpec:
                    parentNode.Children.Add(stopRoSpec.BuildTreeNode());
                    return;
                case MSG_STOP_ROSPEC_RESPONSE stopRoSpecResponse:
                    parentNode.Children.Add(stopRoSpecResponse.BuildTreeNode());
                    return;
                case MSG_DELETE_ROSPEC deleteRoSpec:
                    parentNode.Children.Add(deleteRoSpec.BuildTreeNode());
                    return;
                case MSG_DELETE_ROSPEC_RESPONSE deleteRoSpecResponse:
                    parentNode.Children.Add(deleteRoSpecResponse.BuildTreeNode());
                    return;
                case MSG_ENABLE_ROSPEC enableRoSpec:
                    parentNode.Children.Add(enableRoSpec.BuildTreeNode());
                    return;
                case MSG_ENABLE_ROSPEC_RESPONSE enableRoSpecResponse:
                    parentNode.Children.Add(enableRoSpecResponse.BuildTreeNode());
                    return;
                case MSG_DISABLE_ROSPEC disableRoSpec:
                    parentNode.Children.Add(disableRoSpec.BuildTreeNode());
                    return;
                case MSG_DISABLE_ROSPEC_RESPONSE disableRoSpecResponse:
                    parentNode.Children.Add(disableRoSpecResponse.BuildTreeNode());
                    return;
                case MSG_GET_ROSPECS getRoSpecs:
                    parentNode.Children.Add(getRoSpecs.BuildTreeNode());
                    return;
                case MSG_GET_ROSPECS_RESPONSE getRoSpecsResp:
                    parentNode.Children.Add(getRoSpecsResp.BuildTreeNode());
                    return;
                case MSG_ADD_ACCESSSPEC addAccessSpec:
                    parentNode.Children.Add(addAccessSpec.BuildTreeNode());
                    return;
                case MSG_ADD_ACCESSSPEC_RESPONSE addAccessSpecResponse:
                    parentNode.Children.Add(addAccessSpecResponse.BuildTreeNode());
                    return;
                case MSG_DISABLE_ACCESSSPEC disableAccessSpec:
                    parentNode.Children.Add(disableAccessSpec.BuildTreeNode());
                    return;
                case MSG_DISABLE_ACCESSSPEC_RESPONSE disableAccessSpecResponse:
                    parentNode.Children.Add(disableAccessSpecResponse.BuildTreeNode());
                    return;
                case MSG_DELETE_ACCESSSPEC deleteAccessSpec:
                    parentNode.Children.Add(deleteAccessSpec.BuildTreeNode());
                    return;
                case MSG_DELETE_ACCESSSPEC_RESPONSE deleteAccessSpecResponse:
                    parentNode.Children.Add(deleteAccessSpecResponse.BuildTreeNode());
                    return;
                case MSG_ENABLE_ACCESSSPEC enableAccessSpec:
                    parentNode.Children.Add(enableAccessSpec.BuildTreeNode());
                    return;
                case MSG_ENABLE_ACCESSSPEC_RESPONSE enableAccessSpecResponse:
                    parentNode.Children.Add(enableAccessSpecResponse.BuildTreeNode());
                    return;
                case MSG_GET_ACCESSSPECS getAccessSpecs:
                    parentNode.Children.Add(getAccessSpecs.BuildTreeNode());
                    return;
                case MSG_GET_ACCESSSPECS_RESPONSE getAccessSpecsResponse:
                    parentNode.Children.Add(getAccessSpecsResponse.BuildTreeNode());
                    return;
                case MSG_CUSTOM_MESSAGE customMessage:
                    parentNode.Children.Add(customMessage.BuildTreeNode());
                    return;
                case MSG_CLOSE_CONNECTION closeConnection:
                    parentNode.Children.Add(closeConnection.BuildTreeNode());
                    return;
                case MSG_CLOSE_CONNECTION_RESPONSE closeConnectionResponse:
                    parentNode.Children.Add(closeConnectionResponse.BuildTreeNode());
                    return;
                case MSG_KEEPALIVE_ACK keepAliveAck:
                    parentNode.Children.Add(keepAliveAck.BuildTreeNode());
                    return;
                case MSG_GET_READER_CONFIG getReaderConfig:
                    parentNode.Children.Add(getReaderConfig.BuildTreeNode());
                    return;
                default:
                    break;
            }
        }


        private void ParseSpecificMessageXML(Message message, LLRPMessageNode parentNode)
        {
            parentNode.AddChild("消息类型", message.GetType().Name);

            var child=message.BuildTreeFromMSG();

            parentNode.Children.Add(child); 
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
}
