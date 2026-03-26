using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LLRPReaderUI_Avalonia.Data;
using LLRPReaderUI_Avalonia.Logging;
using LLRPReaderUI_Avalonia.Models;
using LLRPReaderUI_Avalonia.Services;
using LLRPSdk;
using Org.LLRP.LTK.LLRPV1;
using Org.LLRP.LTK.LLRPV1.DataType;
using System.Xml.Linq;

namespace LLRPReaderUI_Avalonia.ViewModels
{
    public partial class LLRPMessageViewModel : ViewModelBase
    {
        private readonly LlrpReader _reader;
        private readonly IAppLogService _logs;
        private readonly IRawFrameRepository _rawFrameRepository;
        private readonly LanguageService _languageService;

        [ObservableProperty]
        private ObservableCollection<RawFrameEntity> _rawFrames = new ObservableCollection<RawFrameEntity>();

        [ObservableProperty]
        private ObservableCollection<RawFrameEntity> _filteredFrames = new ObservableCollection<RawFrameEntity>();

        [ObservableProperty]
        private RawFrameEntity? _selectedRawFrame;

        [ObservableProperty]
        private ObservableCollection<LLRPMessageNode> _messageTree = new ObservableCollection<LLRPMessageNode>();

        [ObservableProperty]
        private LLRPMessageNode? _selectedMessageNode;

        [ObservableProperty]
        private string _rawHexString = string.Empty;

        [ObservableProperty]
        private string _statusText = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        // 筛选属性
        [ObservableProperty]
        private DateTime? _filterStartDate;

        [ObservableProperty]
        private DateTime? _filterEndDate;

        [ObservableProperty]
        private string _filterDirection = string.Empty;

        [ObservableProperty]
        private string _filterMsgType = string.Empty;

        [ObservableProperty]
        private string _filterDeviceId = string.Empty;

        [ObservableProperty]
        private string _searchText = string.Empty;

        // 下拉选项
        public ObservableCollection<string> DirectionOptions { get; private set; }

        private List<string> _msgTypeOptions = new List<string>();
        public ObservableCollection<string> MsgTypeOptions { get; private set; } = new ObservableCollection<string>();

        private List<string> _deviceIdOptions = new List<string>();
        public ObservableCollection<string> DeviceIdOptions { get; private set; } = new ObservableCollection<string>();

        public LLRPMessageViewModel(
            LlrpReader reader,
            IAppLogService logs,
            IRawFrameRepository rawFrameRepository,
            LanguageService languageService)
        {
            _reader = reader;
            _logs = logs;
            _rawFrameRepository = rawFrameRepository;
            _languageService = languageService;

            // Use "All" directly without localization to avoid filter issues when switching languages
            const string allText = "All";
            _filterDirection = allText;
            _filterMsgType = allText;
            _filterDeviceId = allText;
            DirectionOptions = new ObservableCollection<string> { allText, "RX", "TX" };
            _msgTypeOptions = new List<string> { allText };
            MsgTypeOptions = new ObservableCollection<string> { allText };
            _deviceIdOptions = new List<string> { allText };
            DeviceIdOptions = new ObservableCollection<string> { allText };

            // Set initial status
            StatusText = _languageService.GetLocalizedString("LLRPMessage.Ready");

            _ = LoadRawFrames();
        }

        /// <summary>
        /// Get a localized string with format arguments.
        /// </summary>
        private string GetLocalizedString(string key, params object[] args)
        {
            var format = _languageService.GetLocalizedString(key);
            return args.Length > 0 ? string.Format(format, args) : format;
        }

        [RelayCommand]
        private async Task LoadRawFrames()
        {
            IsLoading = true;
            StatusText = _languageService.GetLocalizedString("LLRPMessage.Loading");
            try
            {
                var frames = await _rawFrameRepository.GetRecentAsync(1000);
                RawFrames.Clear();
                foreach (var frame in frames.OrderBy(f => f.Timestamp))
                {
                    RawFrames.Add(frame);
                }

                // 更新消息类型选项
                UpdateMsgTypeOptions();
                UpdateDeviceIdOptions();

                // 重置筛选条件
                FilterStartDate = null;
                FilterEndDate = null;
                FilterDirection = "All";
                FilterMsgType = "All";
                FilterDeviceId = "All";
                SearchText = string.Empty;

                ApplyFilter();
                StatusText = GetLocalizedString("LLRPMessage.Loaded", RawFrames.Count);
            }
            catch (Exception ex)
            {
                StatusText = GetLocalizedString("LLRPMessage.LoadFailed", ex.Message);
                _logs.LogOperation(GetLocalizedString("LLRPMessage.LoadFailedLog", ex.Message), Microsoft.Extensions.Logging.LogLevel.Error, ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateMsgTypeOptions()
        {
            const string allText = "All";
            var types = RawFrames.Select(f => f.MsgTypeName).Distinct().OrderBy(t => t).ToList();
            types.Insert(0, allText);
            _msgTypeOptions = types;
            MsgTypeOptions.Clear();
            foreach (var t in types)
            {
                MsgTypeOptions.Add(t);
            }
        }

        private void UpdateDeviceIdOptions()
        {
            const string allText = "All";
            var deviceIds = RawFrames
                .Where(f => !string.IsNullOrEmpty(f.DeviceId))
                .Select(f => f.DeviceId!)
                .Distinct()
                .OrderBy(d => d)
                .ToList();
            deviceIds.Insert(0, allText);
            _deviceIdOptions = deviceIds;
            DeviceIdOptions.Clear();
            foreach (var d in deviceIds)
            {
                DeviceIdOptions.Add(d);
            }
        }

        [RelayCommand]
        private void ApplyFilter()
        {
            const string allText = "All";
            var query = RawFrames.AsEnumerable();

            // 日期筛选
            if (FilterStartDate.HasValue)
            {
                query = query.Where(f => f.Timestamp >= FilterStartDate.Value);
            }
            if (FilterEndDate.HasValue)
            {
                var endOfDay = FilterEndDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(f => f.Timestamp <= endOfDay);
            }

            // 方向筛选
            if (!string.IsNullOrEmpty(FilterDirection) && FilterDirection != allText)
            {
                query = query.Where(f => f.Direction == FilterDirection);
            }

            // 消息类型筛选
            if (!string.IsNullOrEmpty(FilterMsgType) && FilterMsgType != allText)
            {
                query = query.Where(f => f.MsgTypeName == FilterMsgType);
            }

            // 设备 ID 筛选
            if (!string.IsNullOrEmpty(FilterDeviceId) && FilterDeviceId != allText)
            {
                query = query.Where(f => f.DeviceId == FilterDeviceId);
            }

            // 文本搜索
            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(f =>
                    f.MsgTypeName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    f.Direction.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (f.DeviceId != null && f.DeviceId.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
            }

            FilteredFrames.Clear();
            foreach (var frame in query.OrderBy(f => f.Timestamp))
            {
                FilteredFrames.Add(frame);
            }

            StatusText = GetLocalizedString("LLRPMessage.FilterResult", FilteredFrames.Count);
        }

        [RelayCommand]
        private void ClearFilter()
        {
            const string allText = "All";
            FilterStartDate = null;
            FilterEndDate = null;
            FilterDirection = allText;
            FilterMsgType = allText;
            FilterDeviceId = allText;
            SearchText = string.Empty;
            ApplyFilter();
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
                StatusText = GetLocalizedString("LLRPMessage.ParseError", ex.Message);
                var errorNode = new LLRPMessageNode("LLRP Message", description: _languageService.GetLocalizedString("LLRPMessage.ParseFailed"));
                errorNode.AddChild(_languageService.GetLocalizedString("LLRPMessage.ErrorInfo"), ex.Message);
                errorNode.AddChild(_languageService.GetLocalizedString("LLRPMessage.RawDataHex"), BitConverter.ToString(payload).Replace("-", " "));
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
                    var msgNode = root.AddChild(_languageService.GetLocalizedString("LLRPMessage.MessageBody"), description: message.GetType().Name);
                    //ParseSpecificMessage(message, msgNode);

                    ParseSpecificMessageXML(message, msgNode);
                }
                else
                {
                    root.AddChild(_languageService.GetLocalizedString("LLRPMessage.ParseResult"), _languageService.GetLocalizedString("LLRPMessage.BodyEmpty"), "LLRPBinaryDecoder未能解析出有效的消息对象");
                }
            }
            catch (Exception ex)
            {
                root.AddChild(_languageService.GetLocalizedString("LLRPMessage.LLRPParseError"), ex.Message, _languageService.GetLocalizedString("LLRPMessage.PossibleReason"));
            }
        }

        private void ParseSpecificMessage(Message message, LLRPMessageNode parentNode)
        {
            parentNode.AddChild(_languageService.GetLocalizedString("LLRPMessage.MsgType"), message.GetType().Name);

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
            parentNode.AddChild(_languageService.GetLocalizedString("LLRPMessage.MsgType"), message.GetType().Name);

            var child=message.BuildTreeFromMSG();

            parentNode.Children.Add(child);
        }


        [RelayCommand] private async Task RefreshData() => await LoadRawFrames();

        [RelayCommand]
        private void ExportToText()
        {
            if (SelectedRawFrame == null || MessageTree.Count == 0)
            {
                StatusText = _languageService.GetLocalizedString("LLRPMessage.SelectFirst");
                return;
            }
            try
            {
                var treeText = MessageTree[0].BuildTreeString();
                var fileName = $"LLRP_Message_{SelectedRawFrame.Timestamp:yyyyMMdd_HHmmss}_{SelectedRawFrame.Direction}.txt";
                System.IO.File.WriteAllText(fileName, treeText);
                StatusText = GetLocalizedString("LLRPMessage.Exported", fileName);
            }
            catch (Exception ex)
            {
                StatusText = GetLocalizedString("LLRPMessage.ExportFailed", ex.Message);
            }
        }


    }
}
