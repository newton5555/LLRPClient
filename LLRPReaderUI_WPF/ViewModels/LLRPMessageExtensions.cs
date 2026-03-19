using LLRPReaderUI_WPF.Models;
using Org.LLRP.LTK.LLRPV1;

namespace LLRPReaderUI_WPF.ViewModels
{
    public static class LLRPMessageExtensions
    {
        public static LLRPMessageNode BuildTreeNode(this MSG_SET_READER_CONFIG msg)
        {
            var root = new LLRPMessageNode("SET_READER_CONFIG", $"MessageID={msg.MSG_ID}");
            root.AddChild("Version", msg.VERSION.ToString());
            root.AddChild("MessageID", msg.MSG_ID.ToString());
            root.AddChild("ResetToFactoryDefault", msg.ResetToFactoryDefault.ToString());
            //todo
            if (msg.Custom != null && msg.Custom.Length > 0)
            {
                var customNode = root.AddChild("Custom", $"Count={msg.Custom.Length}");
                for (int i = 0; i < msg.Custom.Length; i++)
                {
                    var param = msg.Custom[i];
                    customNode.AddChild($"Custom[{i}]", description: param?.GetType().Name)
                        .AddChild("ToString()", param?.ToString() ?? "null");
                }
            }
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_ADD_ROSPEC msg)
        {
            var root = new LLRPMessageNode("ADD_ROSPEC", $"MessageID={msg.MSG_ID}");
            root.AddChild("Version", msg.VERSION.ToString());
            root.AddChild("MessageID", msg.MSG_ID.ToString());
            if (msg.ROSpec != null)
                root.AddChild("ROSpec").AddChild("ToString()", msg.ROSpec.ToString());

            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_START_ROSPEC msg)
        {
            var root = new LLRPMessageNode("START_ROSPEC", $"MessageID={msg.MSG_ID}");
            root.AddChild("Version", msg.VERSION.ToString());
            root.AddChild("MessageID", msg.MSG_ID.ToString());
            root.AddChild("ROSpecID", msg.ROSpecID.ToString());

            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_STOP_ROSPEC msg)
        {
            var root = new LLRPMessageNode("STOP_ROSPEC", $"MessageID={msg.MSG_ID}");
            root.AddChild("Version", msg.VERSION.ToString());
            root.AddChild("MessageID", msg.MSG_ID.ToString());
            root.AddChild("ROSpecID", msg.ROSpecID.ToString());

            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_DELETE_ROSPEC msg)
        {
            var root = new LLRPMessageNode("DELETE_ROSPEC", $"MessageID={msg.MSG_ID}");
            root.AddChild("Version", msg.VERSION.ToString());
            root.AddChild("MessageID", msg.MSG_ID.ToString());
            root.AddChild("ROSpecID", msg.ROSpecID.ToString());

            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_ENABLE_ROSPEC msg)
        {
            var root = new LLRPMessageNode("ENABLE_ROSPEC", $"MessageID={msg.MSG_ID}");
            root.AddChild("Version", msg.VERSION.ToString());
            root.AddChild("MessageID", msg.MSG_ID.ToString());
            root.AddChild("ROSpecID", msg.ROSpecID.ToString());

            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_DISABLE_ROSPEC msg)
        {
            var root = new LLRPMessageNode("DISABLE_ROSPEC", $"MessageID={msg.MSG_ID}");
            root.AddChild("Version", msg.VERSION.ToString());
            root.AddChild("MessageID", msg.MSG_ID.ToString());
            root.AddChild("ROSpecID", msg.ROSpecID.ToString());

            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_GET_ROSPECS msg)
        {
            var root = new LLRPMessageNode("GET_ROSPECS", $"MessageID={msg.MSG_ID}");
            root.AddChild("Version", msg.VERSION.ToString());
            root.AddChild("MessageID", msg.MSG_ID.ToString());

            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_GET_ROSPECS_RESPONSE msg)
        {
            var root = new LLRPMessageNode("GET_ROSPECS_RESPONSE", $"MessageID={msg.MSG_ID}");
            root.AddChild("Version", msg.VERSION.ToString());
            root.AddChild("MessageID", msg.MSG_ID.ToString());
            if (msg.LLRPStatus != null)
                root.Children.Add(msg.LLRPStatus.BuildTreeNode());
            if (msg.ROSpec != null && msg.ROSpec.Length > 0)
            {
                var node = root.AddChild("ROSpec", $"Count={msg.ROSpec.Length}");
                for (int i = 0; i < msg.ROSpec.Length; i++)
                {
                    var item = msg.ROSpec[i];
                    node.AddChild($"ROSpec[{i}]", description: item?.GetType().Name)
                        .AddChild("ToString()", item?.ToString() ?? "null");
                }
            }

            return root;
        }

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

        public static LLRPMessageNode BuildTreeNode(this PARAM_EPCData p)
        {
            var root = new LLRPMessageNode("EPCData");
            // ToString(): <EPC Count="...">...</EPC>
            if (p.EPC != null)
            {
                var epcNode = root.AddChild("EPC");
                epcNode.AddChild("Count", p.EPC.Count.ToString());
                epcNode.AddChild("Hex", p.EPC.ToHexString());
            }
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_EPC_96 p)
        {
            var root = new LLRPMessageNode("EPC_96");
            if (p.EPC != null)
                root.AddChild("EPC", p.EPC.ToHexString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_ROSpecID p)
        {
            var root = new LLRPMessageNode("ROSpecID");
            root.AddChild("ROSpecID", p.ROSpecID.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_SpecIndex p)
        {
            var root = new LLRPMessageNode("SpecIndex");
            root.AddChild("SpecIndex", p.SpecIndex.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_InventoryParameterSpecID p)
        {
            var root = new LLRPMessageNode("InventoryParameterSpecID");
            root.AddChild("InventoryParameterSpecID", p.InventoryParameterSpecID.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_AntennaID p)
        {
            var root = new LLRPMessageNode("AntennaID");
            root.AddChild("AntennaID", p.AntennaID.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_PeakRSSI p)
        {
            var root = new LLRPMessageNode("PeakRSSI");
            root.AddChild("PeakRSSI", p.PeakRSSI.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_ChannelIndex p)
        {
            var root = new LLRPMessageNode("ChannelIndex");
            root.AddChild("ChannelIndex", p.ChannelIndex.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_FirstSeenTimestampUTC p)
        {
            var root = new LLRPMessageNode("FirstSeenTimestampUTC");
            root.AddChild("Microseconds", p.Microseconds.ToString(), LlrpDisplayHelper.FormatUtcMicroseconds(p.Microseconds));
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_FirstSeenTimestampUptime p)
        {
            var root = new LLRPMessageNode("FirstSeenTimestampUptime");
            root.AddChild("Microseconds", p.Microseconds.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_LastSeenTimestampUTC p)
        {
            var root = new LLRPMessageNode("LastSeenTimestampUTC");
            root.AddChild("Microseconds", p.Microseconds.ToString(), LlrpDisplayHelper.FormatUtcMicroseconds(p.Microseconds));
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_LastSeenTimestampUptime p)
        {
            var root = new LLRPMessageNode("LastSeenTimestampUptime");
            root.AddChild("Microseconds", p.Microseconds.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_TagSeenCount p)
        {
            var root = new LLRPMessageNode("TagSeenCount");
            root.AddChild("TagCount", p.TagCount.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_C1G2_PC p)
        {
            var root = new LLRPMessageNode("C1G2_PC");
            root.AddChild("PC_Bits", p.PC_Bits.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_C1G2_CRC p)
        {
            var root = new LLRPMessageNode("C1G2_CRC");
            root.AddChild("CRC", p.CRC.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_AccessSpecID p)
        {
            var root = new LLRPMessageNode("AccessSpecID");
            root.AddChild("AccessSpecID", p.AccessSpecID.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_C1G2ReadOpSpecResult p)
        {
            var root = new LLRPMessageNode("C1G2ReadOpSpecResult");
            root.AddChild("Result", p.Result.ToString());
            root.AddChild("OpSpecID", p.OpSpecID.ToString());
            if (p.ReadData != null)
                root.AddChild("ReadData", p.ReadData.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_C1G2WriteOpSpecResult p)
        {
            var root = new LLRPMessageNode("C1G2WriteOpSpecResult");
            root.AddChild("Result", p.Result.ToString());
            root.AddChild("OpSpecID", p.OpSpecID.ToString());
            root.AddChild("NumWordsWritten", p.NumWordsWritten.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_C1G2KillOpSpecResult p)
        {
            var root = new LLRPMessageNode("C1G2KillOpSpecResult");
            root.AddChild("Result", p.Result.ToString());
            root.AddChild("OpSpecID", p.OpSpecID.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_C1G2LockOpSpecResult p)
        {
            var root = new LLRPMessageNode("C1G2LockOpSpecResult");
            root.AddChild("Result", p.Result.ToString());
            root.AddChild("OpSpecID", p.OpSpecID.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_C1G2BlockEraseOpSpecResult p)
        {
            var root = new LLRPMessageNode("C1G2BlockEraseOpSpecResult");
            root.AddChild("Result", p.Result.ToString());
            root.AddChild("OpSpecID", p.OpSpecID.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_C1G2BlockWriteOpSpecResult p)
        {
            var root = new LLRPMessageNode("C1G2BlockWriteOpSpecResult");
            root.AddChild("Result", p.Result.ToString());
            root.AddChild("OpSpecID", p.OpSpecID.ToString());
            root.AddChild("NumWordsWritten", p.NumWordsWritten.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_ClientRequestOpSpecResult p)
        {
            var root = new LLRPMessageNode("ClientRequestOpSpecResult");
            root.AddChild("OpSpecID", p.OpSpecID.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_HoppingEvent p)
        {
            var root = new LLRPMessageNode("HoppingEvent");
            root.AddChild("HopTableID", p.HopTableID.ToString());
            root.AddChild("NextChannelIndex", p.NextChannelIndex.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_GPIEvent p)
        {
            var root = new LLRPMessageNode("GPIEvent");
            root.AddChild("GPIPortNumber", p.GPIPortNumber.ToString());
            root.AddChild("GPIEvent", p.GPIEvent.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_ROSpecEvent p)
        {
            var root = new LLRPMessageNode("ROSpecEvent");
            root.AddChild("EventType", p.EventType.ToString());
            root.AddChild("ROSpecID", p.ROSpecID.ToString());
            root.AddChild("PreemptingROSpecID", p.PreemptingROSpecID.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_ReportBufferLevelWarningEvent p)
        {
            var root = new LLRPMessageNode("ReportBufferLevelWarningEvent");
            root.AddChild("ReportBufferPercentageFull", p.ReportBufferPercentageFull.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_ReportBufferOverflowErrorEvent p)
        {
            // ToString() 为空元素
            return new LLRPMessageNode("ReportBufferOverflowErrorEvent");
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_RFSurveyEvent p)
        {
            var root = new LLRPMessageNode("RFSurveyEvent");
            root.AddChild("EventType", p.EventType.ToString());
            root.AddChild("ROSpecID", p.ROSpecID.ToString());
            root.AddChild("SpecIndex", p.SpecIndex.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_C1G2SingulationDetails p)
        {
            var root = new LLRPMessageNode("C1G2SingulationDetails");
            root.AddChild("NumCollisionSlots", p.NumCollisionSlots.ToString());
            root.AddChild("NumEmptySlots", p.NumEmptySlots.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_AISpecEvent p)
        {
            var root = new LLRPMessageNode("AISpecEvent");
            root.AddChild("EventType", p.EventType.ToString());
            root.AddChild("ROSpecID", p.ROSpecID.ToString());
            root.AddChild("SpecIndex", p.SpecIndex.ToString());

            if (p.AirProtocolSingulationDetails != null && p.AirProtocolSingulationDetails.Count > 0)
            {
                var node = root.AddChild("AirProtocolSingulationDetails", $"Count={p.AirProtocolSingulationDetails.Count}");
                for (int i = 0; i < p.AirProtocolSingulationDetails.Count; i++)
                {
                    var item = p.AirProtocolSingulationDetails[i];
                    if (item is PARAM_C1G2SingulationDetails c1g2)
                        node.Children.Add(c1g2.BuildTreeNode());
                    else
                        node.AddChild($"[{i}] {item?.GetType().Name}", description: item?.GetType().Name)
                            .AddChild("ToString()", item?.ToString() ?? "null");
                }
            }

            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_AntennaEvent p)
        {
            var root = new LLRPMessageNode("AntennaEvent");
            root.AddChild("EventType", p.EventType.ToString());
            root.AddChild("AntennaID", p.AntennaID.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_ConnectionAttemptEvent p)
        {
            var root = new LLRPMessageNode("ConnectionAttemptEvent");
            root.AddChild("Status", p.Status.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_ConnectionCloseEvent p)
        {
            // ToString() 为空元素
            return new LLRPMessageNode("ConnectionCloseEvent");
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_OpSpecID p)
        {
            var root = new LLRPMessageNode("OpSpecID");
            root.AddChild("OpSpecID", p.OpSpecID.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_ReaderExceptionEvent p)
        {
            var root = new LLRPMessageNode("ReaderExceptionEvent");
            if (!string.IsNullOrEmpty(p.Message))
                root.AddChild("Message", p.Message);

            if (p.ROSpecID != null) root.Children.Add(p.ROSpecID.BuildTreeNode());
            if (p.SpecIndex != null) root.Children.Add(p.SpecIndex.BuildTreeNode());
            if (p.InventoryParameterSpecID != null) root.Children.Add(p.InventoryParameterSpecID.BuildTreeNode());
            if (p.AntennaID != null) root.Children.Add(p.AntennaID.BuildTreeNode());
            if (p.AccessSpecID != null) root.Children.Add(p.AccessSpecID.BuildTreeNode());
            if (p.OpSpecID != null) root.Children.Add(p.OpSpecID.BuildTreeNode());

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
                    if (item is PARAM_EPCData epcData)
                        node.Children.Add(epcData.BuildTreeNode());
                    else if (item is PARAM_EPC_96 epc96)
                        node.Children.Add(epc96.BuildTreeNode());
                    else
                        node.AddChild($"[{i}] {item?.GetType().Name}", description: item?.GetType().Name)
                            .AddChild("ToString()", item?.ToString() ?? "null");
                }
            }

            if (p.ROSpecID != null) root.Children.Add(p.ROSpecID.BuildTreeNode());
            if (p.SpecIndex != null) root.Children.Add(p.SpecIndex.BuildTreeNode());
            if (p.InventoryParameterSpecID != null) root.Children.Add(p.InventoryParameterSpecID.BuildTreeNode());
            if (p.AntennaID != null) root.Children.Add(p.AntennaID.BuildTreeNode());
            if (p.PeakRSSI != null) root.Children.Add(p.PeakRSSI.BuildTreeNode());
            if (p.ChannelIndex != null) root.Children.Add(p.ChannelIndex.BuildTreeNode());

            if (p.FirstSeenTimestampUTC != null) root.Children.Add(p.FirstSeenTimestampUTC.BuildTreeNode());
            if (p.FirstSeenTimestampUptime != null) root.Children.Add(p.FirstSeenTimestampUptime.BuildTreeNode());
            if (p.LastSeenTimestampUTC != null) root.Children.Add(p.LastSeenTimestampUTC.BuildTreeNode());
            if (p.LastSeenTimestampUptime != null) root.Children.Add(p.LastSeenTimestampUptime.BuildTreeNode());
            if (p.TagSeenCount != null) root.Children.Add(p.TagSeenCount.BuildTreeNode());

            if (p.AirProtocolTagData != null)
            {
                var node = root.AddChild("AirProtocolTagData", $"Count={p.AirProtocolTagData.Count}");
                for (int i = 0; i < p.AirProtocolTagData.Count; i++)
                {
                    var item = p.AirProtocolTagData[i];
                    if (item is PARAM_C1G2_PC pc)
                        node.Children.Add(pc.BuildTreeNode());
                    else if (item is PARAM_C1G2_CRC crc)
                        node.Children.Add(crc.BuildTreeNode());
                    else
                        node.AddChild($"[{i}] {item?.GetType().Name}", description: item?.GetType().Name)
                            .AddChild("ToString()", item?.ToString() ?? "null");
                }
            }

            if (p.AccessSpecID != null) root.Children.Add(p.AccessSpecID.BuildTreeNode());

            if (p.AccessCommandOpSpecResult != null)
            {
                var node = root.AddChild("AccessCommandOpSpecResult", $"Count={p.AccessCommandOpSpecResult.Count}");
                for (int i = 0; i < p.AccessCommandOpSpecResult.Count; i++)
                {
                    var item = p.AccessCommandOpSpecResult[i];
                    if (item is PARAM_C1G2ReadOpSpecResult read)
                        node.Children.Add(read.BuildTreeNode());
                    else if (item is PARAM_C1G2WriteOpSpecResult write)
                        node.Children.Add(write.BuildTreeNode());
                    else if (item is PARAM_C1G2KillOpSpecResult kill)
                        node.Children.Add(kill.BuildTreeNode());
                    else if (item is PARAM_C1G2LockOpSpecResult lockRes)
                        node.Children.Add(lockRes.BuildTreeNode());
                    else if (item is PARAM_C1G2BlockEraseOpSpecResult erase)
                        node.Children.Add(erase.BuildTreeNode());
                    else if (item is PARAM_C1G2BlockWriteOpSpecResult blockWrite)
                        node.Children.Add(blockWrite.BuildTreeNode());
                    else if (item is PARAM_ClientRequestOpSpecResult clientReq)
                        node.Children.Add(clientReq.BuildTreeNode());
                    else
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

            if (p.HoppingEvent != null) root.Children.Add(p.HoppingEvent.BuildTreeNode());
            if (p.GPIEvent != null) root.Children.Add(p.GPIEvent.BuildTreeNode());
            if (p.ROSpecEvent != null) root.Children.Add(p.ROSpecEvent.BuildTreeNode());
            if (p.ReportBufferLevelWarningEvent != null) root.Children.Add(p.ReportBufferLevelWarningEvent.BuildTreeNode());
            if (p.ReportBufferOverflowErrorEvent != null) root.Children.Add(p.ReportBufferOverflowErrorEvent.BuildTreeNode());
            if (p.ReaderExceptionEvent != null) root.Children.Add(p.ReaderExceptionEvent.BuildTreeNode());
            if (p.RFSurveyEvent != null) root.Children.Add(p.RFSurveyEvent.BuildTreeNode());
            if (p.AISpecEvent != null) root.Children.Add(p.AISpecEvent.BuildTreeNode());
            if (p.AntennaEvent != null) root.Children.Add(p.AntennaEvent.BuildTreeNode());
            if (p.ConnectionAttemptEvent != null) root.Children.Add(p.ConnectionAttemptEvent.BuildTreeNode());
            if (p.ConnectionCloseEvent != null) root.Children.Add(p.ConnectionCloseEvent.BuildTreeNode());

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
}
