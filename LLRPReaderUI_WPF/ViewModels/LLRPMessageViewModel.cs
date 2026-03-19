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

        // 定义一个不希望在通用解析中显示的属性黑名单
        private static readonly HashSet<string> PropertyBlacklist = new HashSet<string>
        {
            "MSG_ID", "TypeNum", "Length", "Version", "msgLen", "msgID", "hdr"
        };

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
                foreach (var frame in frames.OrderByDescending(f => f.Timestamp))
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
        public static LLRPMessageNode BuildTreeNode(this MSG_GET_READER_CAPABILITIES msg)
        {
            var root = new LLRPMessageNode("GET_READER_CAPABILITIES", description: $"MessageID={msg.MSG_ID}");
            root.AddChild("MessageID", msg.MSG_ID.ToString());
            root.AddChild("RequestedData", LlrpDisplayHelper.FormatEnum(msg.RequestedData));
            if (msg.Custom != null && msg.Custom.Length > 0)
            {
                var customNode = root.AddChild("Custom参数", msg.Custom.Length.ToString());
                for (int i = 0; i < msg.Custom.Length; i++)
                {
                    var param = msg.Custom[i];
                    var child = customNode.AddChild($"Custom[{i}]", description: param?.GetType().Name);
                    if (param == null)
                    {
                        child.AddChild("值", "null");
                        continue;
                    }
                    // 直接按字段语义组织
                    foreach (var prop in param.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                    {
                        var val = prop.GetValue(param);
                        child.AddChild(prop.Name, val?.ToString() ?? "null");
                    }
                }
            }
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_GET_READER_CAPABILITIES_RESPONSE msg)
        {
            var root = new LLRPMessageNode("GET_READER_CAPABILITIES_RESPONSE", description: $"MessageID={msg.MSG_ID}");
            root.AddChild("MessageID", msg.MSG_ID.ToString());
            if (msg.LLRPStatus != null)
            {
                var statusNode = root.AddChild("LLRPStatus", description: msg.LLRPStatus.StatusCode.ToString());
                foreach (var prop in msg.LLRPStatus.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    var val = prop.GetValue(msg.LLRPStatus);
                    statusNode.AddChild(prop.Name, val?.ToString() ?? "null");
                }
            }
            if (msg.GeneralDeviceCapabilities != null)
            {
                var gdcNode = root.AddChild("GeneralDeviceCapabilities");
                foreach (var prop in msg.GeneralDeviceCapabilities.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    var val = prop.GetValue(msg.GeneralDeviceCapabilities);
                    gdcNode.AddChild(prop.Name, val?.ToString() ?? "null");
                }
            }
            if (msg.LLRPCapabilities != null)
            {
                var llrpNode = root.AddChild("LLRPCapabilities");
                foreach (var prop in msg.LLRPCapabilities.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    var val = prop.GetValue(msg.LLRPCapabilities);
                    llrpNode.AddChild(prop.Name, val?.ToString() ?? "null");
                }
            }
            if (msg.RegulatoryCapabilities != null)
            {
                var regNode = root.AddChild("RegulatoryCapabilities");
                foreach (var prop in msg.RegulatoryCapabilities.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    var val = prop.GetValue(msg.RegulatoryCapabilities);
                    regNode.AddChild(prop.Name, val?.ToString() ?? "null");
                }
            }
            if (msg.AirProtocolLLRPCapabilities != null)
            {
                var apNode = root.AddChild("AirProtocolLLRPCapabilities");
                for (int i = 0; i < msg.AirProtocolLLRPCapabilities.Count; i++)
                {
                    var item = msg.AirProtocolLLRPCapabilities[i];
                    var itemNode = apNode.AddChild(item.GetType().Name);
                    foreach (var prop in item.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                    {
                        var val = prop.GetValue(item);
                        itemNode.AddChild(prop.Name, val?.ToString() ?? "null");
                    }
                }
                
            }
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_RO_ACCESS_REPORT msg)
        {
            var root = new LLRPMessageNode("RO_ACCESS_REPORT", description: $"MessageID={msg.MSG_ID}");
            root.AddChild("MessageID", msg.MSG_ID.ToString());
            if (msg.TagReportData != null)
            {
                var tagsNode = root.AddChild("TagReportData", msg.TagReportData.Length.ToString());
                for (int i = 0; i < msg.TagReportData.Length; i++)
                {
                    var tag = msg.TagReportData[i];
                    var tagNode = tagsNode.AddChild($"Tag[{i}]", description: tag.GetType().Name);
                    foreach (var prop in tag.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                    {
                        var val = prop.GetValue(tag);
                        if (val is Array arr && arr.Length > 0)
                        {
                            var arrNode = tagNode.AddChild(prop.Name, $"Count={arr.Length}");
                            foreach (var elem in arr)
                            {
                                arrNode.AddChild(elem.GetType().Name, elem?.ToString() ?? "null");
                            }
                        }
                        else
                        {
                            tagNode.AddChild(prop.Name, val?.ToString() ?? "null");
                        }
                    }
                }
            }
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_READER_EVENT_NOTIFICATION msg)
        {
            var root = new LLRPMessageNode("READER_EVENT_NOTIFICATION", description: $"MessageID={msg.MSG_ID}");
            root.AddChild("MessageID", msg.MSG_ID.ToString());
            if (msg.ReaderEventNotificationData != null)
            {
                var dataNode = root.AddChild("ReaderEventNotificationData");
                foreach (var prop in msg.ReaderEventNotificationData.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    var val = prop.GetValue(msg.ReaderEventNotificationData);
                    dataNode.AddChild(prop.Name, val?.ToString() ?? "null");
                }
            }
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_ERROR_MESSAGE msg)
        {
            var root = new LLRPMessageNode("ERROR_MESSAGE", description: $"MessageID={msg.MSG_ID}");
            root.AddChild("MessageID", msg.MSG_ID.ToString());
            if (msg.LLRPStatus != null)
            {
                var statusNode = root.AddChild("LLRPStatus", description: msg.LLRPStatus.StatusCode.ToString());
                foreach (var prop in msg.LLRPStatus.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    var val = prop.GetValue(msg.LLRPStatus);
                    statusNode.AddChild(prop.Name, val?.ToString() ?? "null");
                }
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
