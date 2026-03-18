using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LLRPReaderUI_WPF.Data;
using LLRPReaderUI_WPF.Logging;
using LLRPReaderUI_WPF.Models;
using LLRPSdk;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Org.LLRP.LTK.LLRPV1;
using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LLRPReaderUI_WPF.ViewModels
{
    public partial class LLRPMessageViewModel : ObservableObject
    {
        private readonly LlrpReader _reader;
        private readonly IAppLogService _logs;
        private readonly IServiceProvider _serviceProvider;
        private RawFrameDbContext _dbContext = null!;

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
            IServiceProvider serviceProvider)
        {
            _reader = reader;
            _logs = logs;
            _serviceProvider = serviceProvider;

            // 初始化数据库上下文
            InitializeDbContext();

            // 设置集合的自动刷新
            BindingOperations.EnableCollectionSynchronization(RawFrames, new object());
            BindingOperations.EnableCollectionSynchronization(MessageTree, new object());

            // 加载数据
            _ = LoadRawFrames();
        }

        private void InitializeDbContext()
        {
            try
            {
                var options = new DbContextOptionsBuilder<RawFrameDbContext>()
                    .UseSqlite("Data Source=llrp_raw_frames.db")
                    .Options;

                _dbContext = new RawFrameDbContext(options);
                _dbContext.Database.EnsureCreated();
                StatusText = "数据库连接成功";
            }
            catch (Exception ex)
            {
                StatusText = $"数据库连接失败: {ex.Message}";
                _logs.LogOperation($"初始化数据库失败: {ex.Message}", Microsoft.Extensions.Logging.LogLevel.Error, ex);
            }
        }

        [RelayCommand]
        private async Task LoadRawFrames()
        {
            if (_dbContext == null)
            {
                StatusText = "数据库未初始化";
                return;
            }

            IsLoading = true;
            StatusText = "正在加载原始帧数据...";

            try
            {
                await Task.Run(() =>
                {
                    var frames = _dbContext.RawFrames
                        .OrderByDescending(f => f.Timestamp)
                        .Take(1000)
                        .ToList();

                    RawFrames.Clear();
                    foreach (var frame in frames)
                    {
                        RawFrames.Add(frame);
                    }
                });
                StatusText = $"已加载 {RawFrames.Count} 条原始帧记录";
                _logs.LogOperation($"加载了 {RawFrames.Count} 条原始帧记录");
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
            if (value == null || value.Payload == null)
            {
                MessageTree.Clear();
                RawHexString = string.Empty;
                return;
            }

            // 显示原始十六进制字符串
            RawHexString = BitConverter.ToString(value.Payload).Replace("-", " ");

            // 解析LLRP消息
            ParseLLRPMessage(value.Payload);
        }

        private void ParseLLRPMessage(byte[] payload)
        {
            MessageTree.Clear();

            try
            {
                // 创建根节点
                var root = new LLRPMessageNode("LLRP Message", description: $"Length: {payload.Length} bytes");

                // 添加基本信息
                var basicInfo = root.AddChild("基本信息");
                basicInfo.AddChild("方向", SelectedRawFrame?.Direction);
                basicInfo.AddChild("时间戳", SelectedRawFrame?.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                basicInfo.AddChild("长度", $"{payload.Length} bytes");

                // 解析消息头 (前10字节)
                if (payload.Length >= 10)
                {
                    var header = root.AddChild("消息头");
                    
                    // 消息类型 (2字节)
                    ushort messageType = (ushort)((payload[0] << 8) | payload[1]);
                    header.AddChild("消息类型", $"0x{messageType:X4}", GetMessageTypeDescription(messageType));
                    
                    // 消息长度 (4字节)
                    uint messageLength = (uint)((payload[2] << 24) | (payload[3] << 16) | (payload[4] << 8) | payload[5]);
                    header.AddChild("消息长度", $"{messageLength} bytes");
                    
                    // 消息ID (4字节)
                    uint messageId = (uint)((payload[6] << 24) | (payload[7] << 16) | (payload[8] << 8) | payload[9]);
                    header.AddChild("消息ID", $"0x{messageId:X8}");
                }

                // 添加原始数据节点
                var rawData = root.AddChild("原始数据");
                rawData.AddChild("十六进制", RawHexString);
                rawData.AddChild("字节数", $"{payload.Length}");

                // 尝试使用LLRPBinaryDecoder进行完整解析
                TryParseWithLLRPBinaryDecoder(payload, root);

                MessageTree.Add(root);
                StatusText = "LLRP消息解析完成";
            }
            catch (Exception ex)
            {
                StatusText = $"解析LLRP消息时出错: {ex.Message}";
                _logs.LogOperation($"解析LLRP消息失败: {ex.Message}", Microsoft.Extensions.Logging.LogLevel.Error, ex);
                
                // 即使解析失败，也显示基本信息
                var errorNode = new LLRPMessageNode("LLRP Message", description: "解析失败");
                errorNode.AddChild("错误信息", ex.Message);
                errorNode.AddChild("原始数据", BitConverter.ToString(payload).Replace("-", " "));
                MessageTree.Add(errorNode);
            }
        }

        // 暂时注释掉LTK解析相关代码，确保编译通过
        // 如果需要完整解析，请确保引用了正确的LTK库并取消注释以下代码
        /*
        private void TryParseWithLTK(byte[] payload, LLRPMessageNode root)
        {
            try
            {
                // 尝试使用LTK解析消息
                Message msg = LLRPMessageFactory.DecodeMessage(payload);
                if (msg != null)
                {
                    var ltkNode = root.AddChild("LTK解析结果", description: msg.GetType().Name);
                    
                    // 添加消息基本信息
                    var msgInfo = ltkNode.AddChild("消息信息");
                    msgInfo.AddChild("类型", msg.GetType().Name);
                    msgInfo.AddChild("消息ID", msg.MSG_ID.ToString());
                    
                    // 尝试解析参数
                    ParseParameters(msg, ltkNode);
                }
                else
                {
                    root.AddChild("LTK解析", "无法解析消息", "LTK库无法识别此消息格式");
                }
            }
            catch (Exception ex)
            {
                root.AddChild("LTK解析", "解析失败", ex.Message);
            }
        }

        private void ParseParameters(Message msg, LLRPMessageNode parentNode)
        {
            try
            {
                // 使用反射获取消息的参数
                var parameters = msg.GetType().GetProperties()
                    .Where(p => p.PropertyType.IsClass && p.PropertyType != typeof(string))
                    .ToList();

                if (parameters.Any())
                {
                    var paramsNode = parentNode.AddChild("参数列表");
                    
                    foreach (var param in parameters)
                    {
                        try
                        {
                            var value = param.GetValue(msg);
                            if (value != null)
                            {
                                var paramNode = paramsNode.AddChild(param.Name, description: param.PropertyType.Name);
                                
                                // 尝试解析嵌套属性
                                ParseObjectProperties(value, paramNode);
                            }
                        }
                        catch
                        {
                            // 忽略单个参数解析失败
                        }
                    }
                }
            }
            catch
            {
                // 忽略参数解析失败
            }
        }

        private void ParseObjectProperties(object obj, LLRPMessageNode parentNode)
        {
            try
            {
                var properties = obj.GetType().GetProperties()
                    .Where(p => p.CanRead)
                    .ToList();

                foreach (var prop in properties)
                {
                    try
                    {
                        var value = prop.GetValue(obj);
                        if (value != null)
                        {
                            string stringValue = value.ToString();
                            if (!string.IsNullOrEmpty(stringValue) && stringValue != obj.GetType().FullName)
                            {
                                parentNode.AddChild(prop.Name, stringValue, prop.PropertyType.Name);
                            }
                        }
                    }
                    catch
                    {
                        // 忽略单个属性解析失败
                    }
                }
            }
            catch
            {
                // 忽略属性解析失败
            }
        }
        */

        private string GetMessageTypeDescription(ushort messageType)
        {
            // LLRP标准消息类型映射
            return messageType switch
            {
                1 => "GET_READER_CAPABILITIES",
                11 => "GET_READER_CAPABILITIES_RESPONSE",
                2 => "ADD_ROSPEC",
                12 => "ADD_ROSPEC_RESPONSE",
                3 => "DELETE_ROSPEC",
                13 => "DELETE_ROSPEC_RESPONSE",
                4 => "START_ROSPEC",
                14 => "START_ROSPEC_RESPONSE",
                5 => "STOP_ROSPEC",
                15 => "STOP_ROSPEC_RESPONSE",
                6 => "ENABLE_ROSPEC",
                16 => "ENABLE_ROSPEC_RESPONSE",
                7 => "DISABLE_ROSPEC",
                17 => "DISABLE_ROSPEC_RESPONSE",
                8 => "GET_ROSPECS",
                18 => "GET_ROSPECS_RESPONSE",
                20 => "RO_ACCESS_REPORT",
                21 => "READER_EVENT_NOTIFICATION",
                22 => "ENABLE_EVENTS_AND_REPORTS",
                23 => "ERROR_MESSAGE",
                24 => "GET_READER_CONFIG",
                34 => "GET_READER_CONFIG_RESPONSE",
                25 => "SET_READER_CONFIG",
                35 => "SET_READER_CONFIG_RESPONSE",
                26 => "CLOSE_CONNECTION",
                36 => "CLOSE_CONNECTION_RESPONSE",
                27 => "GET_REPORT",
                28 => "KEEPALIVE",
                38 => "KEEPALIVE_ACK",
                40 => "ADD_ACCESSSPEC",
                50 => "ADD_ACCESSSPEC_RESPONSE",
                41 => "DELETE_ACCESSSPEC",
                51 => "DELETE_ACCESSSPEC_RESPONSE",
                42 => "ENABLE_ACCESSSPEC",
                52 => "ENABLE_ACCESSSPEC_RESPONSE",
                _ => $"未知类型 (0x{messageType:X4})"
            };
        }

        private void TryParseWithLLRPBinaryDecoder(byte[] payload, LLRPMessageNode root)
        {
            try
            {
                // 使用LLRPBinaryDecoder解析消息
                var decoderNode = root.AddChild("LLRPBinaryDecoder解析", description: "使用LTK库完整解析");
                
                try
                {
                    // 解析信封头
                    LLRPBinaryDecoder.Decode_Envelope(payload, out var env);
                    var envelopeNode = decoderNode.AddChild("消息信封");
                    envelopeNode.AddChild("版本", $"0x{env.ver:X2}");
                    envelopeNode.AddChild("消息类型", $"{env.msg_type} (0x{(ushort)env.msg_type:X4})");
                    envelopeNode.AddChild("消息长度", $"{env.msg_len} bytes");
                    envelopeNode.AddChild("消息ID", $"0x{env.msg_id:X8}");
                    
                    // 尝试解析完整消息
                    try
                    {
                        LLRPBinaryDecoder.Decode(ref payload, out var message);
                        if (message != null)
                        {
                            var msgNode = decoderNode.AddChild("完整消息", description: message.GetType().Name);
                            
                            // 添加消息基本信息
                            var msgInfo = msgNode.AddChild("消息信息");
                            msgInfo.AddChild("类型", message.GetType().Name);
                            
                            // 使用反射获取MSG_ID属性
                            var msgIdProp = message.GetType().GetProperty("MSG_ID");
                            if (msgIdProp != null)
                            {
                                var msgIdValue = msgIdProp.GetValue(message);
                                msgInfo.AddChild("消息ID", msgIdValue?.ToString() ?? "N/A");
                            }
                            
                            // 尝试解析参数
                            ParseMessageParameters(message, msgNode);
                        }
                        else
                        {
                            decoderNode.AddChild("解析结果", "消息为空", "LLRPBinaryDecoder未能解析出有效消息");
                        }
                    }
                    catch (Exception ex)
                    {
                        decoderNode.AddChild("完整解析错误", ex.Message, "解析完整消息时出错");
                        
                        // 仍然显示信封信息
                        decoderNode.AddChild("状态", "部分解析完成", "已解析信封头，但完整消息解析失败");
                    }
                }
                catch (Exception ex)
                {
                    decoderNode.AddChild("信封解析错误", ex.Message, "解析消息信封时出错");
                }
            }
            catch (Exception ex)
            {
                root.AddChild("LLRPBinaryDecoder解析", "解析失败", ex.Message);
            }
        }

        private void ParseMessageParameters(Message msg, LLRPMessageNode parentNode)
        {
            try
            {
                // 获取所有公共属性
                var properties = msg.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead)
                    .OrderBy(p => p.Name)
                    .ToList();

                if (properties.Any())
                {
                    var paramsNode = parentNode.AddChild("参数列表");
                    
                    foreach (var prop in properties)
                    {
                        try
                        {
                            var value = prop.GetValue(msg);
                            if (value != null)
                            {
                                // 跳过复杂的嵌套对象，只显示简单类型
                                if (prop.PropertyType.IsPrimitive || 
                                    prop.PropertyType == typeof(string) || 
                                    prop.PropertyType == typeof(decimal) ||
                                    prop.PropertyType.IsEnum)
                                {
                                    paramsNode.AddChild(prop.Name, value.ToString(), prop.PropertyType.Name);
                                }
                                else if (prop.PropertyType.IsClass)
                                {
                                    // 对于类类型，显示类型信息但不深入解析
                                    var typeNode = paramsNode.AddChild(prop.Name, description: prop.PropertyType.Name);
                                    typeNode.AddChild("类型", prop.PropertyType.Name);
                                    
                                    // 尝试显示ToString()结果
                                    try
                                    {
                                        var stringValue = value.ToString();
                                        if (!string.IsNullOrEmpty(stringValue) && stringValue != prop.PropertyType.FullName)
                                        {
                                            typeNode.AddChild("值", stringValue);
                                        }
                                    }
                                    catch
                                    {
                                        // 忽略ToString失败
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // 忽略单个属性解析失败
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                parentNode.AddChild("参数解析错误", ex.Message, "解析消息参数时出错");
            }
        }

        [RelayCommand]
        private void ClearSelection()
        {
            SelectedRawFrame = null;
            MessageTree.Clear();
            RawHexString = string.Empty;
            StatusText = "已清除选择";
        }

        [RelayCommand]
        private async Task RefreshData()
        {
            await LoadRawFrames();
        }

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
                var fileName = $"LLRP_Message_{SelectedRawFrame.Timestamp:yyyyMMdd_HHmmss}.txt";
                
                System.IO.File.WriteAllText(fileName, treeText);
                StatusText = $"已导出到文件: {fileName}";
                _logs.LogOperation($"导出LLRP消息到文件: {fileName}");
            }
            catch (Exception ex)
            {
                StatusText = $"导出失败: {ex.Message}";
                _logs.LogOperation($"导出LLRP消息失败: {ex.Message}", Microsoft.Extensions.Logging.LogLevel.Error, ex);
            }
        }
    }
}
