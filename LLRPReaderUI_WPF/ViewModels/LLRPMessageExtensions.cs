using LLRPReaderUI_WPF.Models;
using Org.LLRP.LTK.LLRPV1;

namespace LLRPReaderUI_WPF.ViewModels
{
    public static class LLRPMessageExtensions
    {
        public static LLRPMessageNode BuildTreeNode(this MSG_SET_READER_CONFIG msg)
        {
            var root = new LLRPMessageNode("SET_READER_CONFIG", $"MessageID={msg.MSG_ID}");
            root.AddChild("ResetToFactoryDefault", msg.ResetToFactoryDefault.ToString());
            if (msg.ReaderEventNotificationSpec != null)
                root.Children.Add(msg.ReaderEventNotificationSpec.BuildTreeNode());

            if (msg.AntennaProperties != null && msg.AntennaProperties.Length > 0)
            {
                var node = root.AddChild("AntennaProperties", $"Count={msg.AntennaProperties.Length}");
                for (int i = 0; i < msg.AntennaProperties.Length; i++)
                {
                    var item = msg.AntennaProperties[i];
                    if (item != null)
                        node.Children.Add(item.BuildTreeNode());
                    else
                        node.AddChild($"AntennaProperties[{i}]", description: "null");
                }
            }

            if (msg.AntennaConfiguration != null && msg.AntennaConfiguration.Length > 0)
            {
                var node = root.AddChild("AntennaConfiguration", $"Count={msg.AntennaConfiguration.Length}");
                for (int i = 0; i < msg.AntennaConfiguration.Length; i++)
                {
                    var item = msg.AntennaConfiguration[i];
                    if (item != null)
                        node.Children.Add(item.BuildTreeNode());
                    else
                        node.AddChild($"AntennaConfiguration[{i}]", description: "null");
                }
            }

            if (msg.ROReportSpec != null)
                root.Children.Add(msg.ROReportSpec.BuildTreeNode());
            if (msg.AccessReportSpec != null)
                root.Children.Add(msg.AccessReportSpec.BuildTreeNode());
            if (msg.KeepaliveSpec != null)
                root.Children.Add(msg.KeepaliveSpec.BuildTreeNode());

            if (msg.GPOWriteData != null && msg.GPOWriteData.Length > 0)
            {
                var node = root.AddChild("GPOWriteData", $"Count={msg.GPOWriteData.Length}");
                for (int i = 0; i < msg.GPOWriteData.Length; i++)
                {
                    var item = msg.GPOWriteData[i];
                    if (item != null)
                        node.Children.Add(item.BuildTreeNode());
                    else
                        node.AddChild($"GPOWriteData[{i}]", description: "null");
                }
            }

            if (msg.GPIPortCurrentState != null && msg.GPIPortCurrentState.Length > 0)
            {
                var node = root.AddChild("GPIPortCurrentState", $"Count={msg.GPIPortCurrentState.Length}");
                for (int i = 0; i < msg.GPIPortCurrentState.Length; i++)
                {
                    var item = msg.GPIPortCurrentState[i];
                    if (item != null)
                        node.Children.Add(item.BuildTreeNode());
                    else
                        node.AddChild($"GPIPortCurrentState[{i}]", description: "null");
                }
            }

            if (msg.EventsAndReports != null)
                root.Children.Add(msg.EventsAndReports.BuildTreeNode());

            if (msg.Custom != null && msg.Custom.Length > 0)
            {
                var node = root.AddChild("Custom", $"Count={msg.Custom.Length}");
                for (int i = 0; i < msg.Custom.Length; i++)
                {
                    var item = msg.Custom[i];
                    node.AddChild($"Custom[{i}]", description: item?.GetType().Name)
                        .AddChild("ToString()", item?.ToString() ?? "null");
                }
            }
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_GET_READER_CONFIG msg)
        {
            var root = new LLRPMessageNode("GET_READER_CONFIG");
            root.AddChild("ToString()", msg.ToString());
            return root;
        }


        public static LLRPMessageNode BuildTreeNode(this PARAM_ReaderEventNotificationSpec p)
        {
            var root = new LLRPMessageNode("ReaderEventNotificationSpec");
            if (p.EventNotificationState != null && p.EventNotificationState.Length > 0)
            {
                var node = root.AddChild("EventNotificationState", $"Count={p.EventNotificationState.Length}");
                for (int i = 0; i < p.EventNotificationState.Length; i++)
                {
                    var item = p.EventNotificationState[i];
                    if (item != null)
                        node.Children.Add(item.BuildTreeNode());
                    else
                        node.AddChild($"EventNotificationState[{i}]", description: "null");
                }
            }
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_EventNotificationState p)
        {
            var root = new LLRPMessageNode("EventNotificationState");
            root.AddChild("EventType", p.EventType.ToString());
            root.AddChild("NotificationState", p.NotificationState.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_AntennaProperties p)
        {
            var root = new LLRPMessageNode("AntennaProperties");
            root.AddChild("AntennaConnected", p.AntennaConnected.ToString());
            root.AddChild("AntennaID", p.AntennaID.ToString());
            root.AddChild("AntennaGain", p.AntennaGain.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_AntennaConfiguration p)
        {
            var root = new LLRPMessageNode("AntennaConfiguration");
            root.AddChild("AntennaID", p.AntennaID.ToString());
            if (p.RFReceiver != null) root.Children.Add(p.RFReceiver.BuildTreeNode());
            if (p.RFTransmitter != null) root.Children.Add(p.RFTransmitter.BuildTreeNode());

            if (p.AirProtocolInventoryCommandSettings != null && p.AirProtocolInventoryCommandSettings.Count > 0)
            {
                var node = root.AddChild("AirProtocolInventoryCommandSettings", $"Count={p.AirProtocolInventoryCommandSettings.Count}");
                for (int i = 0; i < p.AirProtocolInventoryCommandSettings.Count; i++)
                {
                    var item = p.AirProtocolInventoryCommandSettings[i];
                    if (item is PARAM_C1G2InventoryCommand c1g2)
                        node.Children.Add(c1g2.BuildTreeNode());
                    else
                        node.AddChild($"[{i}] {item?.GetType().Name}", description: item?.GetType().Name)
                            .AddChild("ToString()", item?.ToString() ?? "null");
                }
            }

            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_RFReceiver p)
        {
            var root = new LLRPMessageNode("RFReceiver");
            root.AddChild("ReceiverSensitivity", p.ReceiverSensitivity.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_RFTransmitter p)
        {
            var root = new LLRPMessageNode("RFTransmitter");
            root.AddChild("HopTableID", p.HopTableID.ToString());
            root.AddChild("ChannelIndex", p.ChannelIndex.ToString());
            root.AddChild("TransmitPower", p.TransmitPower.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_C1G2InventoryCommand p)
        {
            var root = new LLRPMessageNode("C1G2InventoryCommand");
            root.AddChild("TagInventoryStateAware", p.TagInventoryStateAware.ToString());

            if (p.C1G2Filter != null && p.C1G2Filter.Length > 0)
            {
                var node = root.AddChild("C1G2Filter", $"Count={p.C1G2Filter.Length}");
                for (int i = 0; i < p.C1G2Filter.Length; i++)
                {
                    var item = p.C1G2Filter[i];
                    if (item != null)
                        node.Children.Add(item.BuildTreeNode());
                    else
                        node.AddChild($"C1G2Filter[{i}]", description: "null");
                }
            }

            if (p.C1G2RFControl != null)
                root.Children.Add(p.C1G2RFControl.BuildTreeNode());
            if (p.C1G2SingulationControl != null)
                root.Children.Add(p.C1G2SingulationControl.BuildTreeNode());

            if (p.Custom != null && p.Custom.Length > 0)
            {
                var node = root.AddChild("Custom", $"Count={p.Custom.Length}");
                for (int i = 0; i < p.Custom.Length; i++)
                {
                    var item = p.Custom[i];
                    node.AddChild($"Custom[{i}]", description: item?.GetType().Name)
                        .AddChild("ToString()", item?.ToString() ?? "null");
                }
            }

            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_C1G2Filter p)
        {
            var root = new LLRPMessageNode("C1G2Filter");
            root.AddChild("T", p.T.ToString());
            if (p.C1G2TagInventoryMask != null)
                root.Children.Add(p.C1G2TagInventoryMask.BuildTreeNode());
            if (p.C1G2TagInventoryStateAwareFilterAction != null)
                root.Children.Add(p.C1G2TagInventoryStateAwareFilterAction.BuildTreeNode());
            if (p.C1G2TagInventoryStateUnawareFilterAction != null)
                root.Children.Add(p.C1G2TagInventoryStateUnawareFilterAction.BuildTreeNode());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_C1G2TagInventoryMask p)
        {
            var root = new LLRPMessageNode("C1G2TagInventoryMask");
            if (p.MB != null)
                root.AddChild("MB", p.MB.ToString());
            root.AddChild("Pointer", p.Pointer.ToString());
            if (p.TagMask != null)
            {
                var tagMaskNode = root.AddChild("TagMask", $"Count={p.TagMask.Count}");
                tagMaskNode.AddChild("Hex", p.TagMask.ToHexString());
            }
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_C1G2TagInventoryStateAwareFilterAction p)
        {
            var root = new LLRPMessageNode("C1G2TagInventoryStateAwareFilterAction");
            root.AddChild("Target", p.Target.ToString());
            root.AddChild("Action", p.Action.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_C1G2TagInventoryStateUnawareFilterAction p)
        {
            var root = new LLRPMessageNode("C1G2TagInventoryStateUnawareFilterAction");
            root.AddChild("Action", p.Action.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_C1G2RFControl p)
        {
            var root = new LLRPMessageNode("C1G2RFControl");
            root.AddChild("ModeIndex", p.ModeIndex.ToString());
            root.AddChild("Tari", p.Tari.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_C1G2SingulationControl p)
        {
            var root = new LLRPMessageNode("C1G2SingulationControl");
            if (p.Session != null)
                root.AddChild("Session", p.Session.ToString());
            root.AddChild("TagPopulation", p.TagPopulation.ToString());
            root.AddChild("TagTransitTime", p.TagTransitTime.ToString());
            if (p.C1G2TagInventoryStateAwareSingulationAction != null)
                root.Children.Add(p.C1G2TagInventoryStateAwareSingulationAction.BuildTreeNode());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_C1G2TagInventoryStateAwareSingulationAction p)
        {
            var root = new LLRPMessageNode("C1G2TagInventoryStateAwareSingulationAction");
            root.AddChild("I", p.I.ToString());
            root.AddChild("S", p.S.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_ROReportSpec p)
        {
            var root = new LLRPMessageNode("ROReportSpec");
            root.AddChild("ROReportTrigger", p.ROReportTrigger.ToString());
            root.AddChild("N", p.N.ToString());
            if (p.TagReportContentSelector != null)
                root.Children.Add(p.TagReportContentSelector.BuildTreeNode());
            if (p.Custom != null && p.Custom.Length > 0)
            {
                var node = root.AddChild("Custom", $"Count={p.Custom.Length}");
                for (int i = 0; i < p.Custom.Length; i++)
                {
                    var item = p.Custom[i];
                    node.AddChild($"Custom[{i}]", description: item?.GetType().Name)
                        .AddChild("ToString()", item?.ToString() ?? "null");
                }
            }
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_TagReportContentSelector p)
        {
            var root = new LLRPMessageNode("TagReportContentSelector");
            root.AddChild("EnableROSpecID", p.EnableROSpecID.ToString());
            root.AddChild("EnableSpecIndex", p.EnableSpecIndex.ToString());
            root.AddChild("EnableInventoryParameterSpecID", p.EnableInventoryParameterSpecID.ToString());
            root.AddChild("EnableAntennaID", p.EnableAntennaID.ToString());
            root.AddChild("EnableChannelIndex", p.EnableChannelIndex.ToString());
            root.AddChild("EnablePeakRSSI", p.EnablePeakRSSI.ToString());
            root.AddChild("EnableFirstSeenTimestamp", p.EnableFirstSeenTimestamp.ToString());
            root.AddChild("EnableLastSeenTimestamp", p.EnableLastSeenTimestamp.ToString());
            root.AddChild("EnableTagSeenCount", p.EnableTagSeenCount.ToString());
            root.AddChild("EnableAccessSpecID", p.EnableAccessSpecID.ToString());

            if (p.AirProtocolEPCMemorySelector != null && p.AirProtocolEPCMemorySelector.Count > 0)
            {
                var node = root.AddChild("AirProtocolEPCMemorySelector", $"Count={p.AirProtocolEPCMemorySelector.Count}");
                for (int i = 0; i < p.AirProtocolEPCMemorySelector.Count; i++)
                {
                    var item = p.AirProtocolEPCMemorySelector[i];
                    if (item is PARAM_C1G2EPCMemorySelector c1g2)
                        node.Children.Add(c1g2.BuildTreeNode());
                    else
                        node.AddChild($"[{i}] {item?.GetType().Name}", description: item?.GetType().Name)
                            .AddChild("ToString()", item?.ToString() ?? "null");
                }
            }

            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_C1G2EPCMemorySelector p)
        {
            var root = new LLRPMessageNode("C1G2EPCMemorySelector");
            root.AddChild("EnableCRC", p.EnableCRC.ToString());
            root.AddChild("EnablePCBits", p.EnablePCBits.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_AccessReportSpec p)
        {
            var root = new LLRPMessageNode("AccessReportSpec");
            root.AddChild("AccessReportTrigger", p.AccessReportTrigger.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_KeepaliveSpec p)
        {
            var root = new LLRPMessageNode("KeepaliveSpec");
            root.AddChild("KeepaliveTriggerType", p.KeepaliveTriggerType.ToString());
            root.AddChild("PeriodicTriggerValue", p.PeriodicTriggerValue.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_GPOWriteData p)
        {
            var root = new LLRPMessageNode("GPOWriteData");
            root.AddChild("GPOPortNumber", p.GPOPortNumber.ToString());
            root.AddChild("GPOData", p.GPOData.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_GPIPortCurrentState p)
        {
            var root = new LLRPMessageNode("GPIPortCurrentState");
            root.AddChild("GPIPortNum", p.GPIPortNum.ToString());
            root.AddChild("Config", p.Config.ToString());
            root.AddChild("State", p.State.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_EventsAndReports p)
        {
            var root = new LLRPMessageNode("EventsAndReports");
            root.AddChild("HoldEventsAndReportsUponReconnect", p.HoldEventsAndReportsUponReconnect.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_ADD_ROSPEC msg)
        {
            var root = new LLRPMessageNode("ADD_ROSPEC");
            if (msg.ROSpec != null)
                root.Children.Add(msg.ROSpec.BuildTreeNode());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_ADD_ROSPEC_RESPONSE msg)
        {
            var root = new LLRPMessageNode("ADD_ROSPEC_RESPONSE");
            if (msg.LLRPStatus != null)
                root.Children.Add(msg.LLRPStatus.BuildTreeNode());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_START_ROSPEC msg)
        {
            var root = new LLRPMessageNode("START_ROSPEC");
            root.AddChild("ROSpecID", msg.ROSpecID.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_START_ROSPEC_RESPONSE msg)
        {
            var root = new LLRPMessageNode("START_ROSPEC_RESPONSE");
            if (msg.LLRPStatus != null)
                root.Children.Add(msg.LLRPStatus.BuildTreeNode());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_STOP_ROSPEC msg)
        {
            var root = new LLRPMessageNode("STOP_ROSPEC");
            root.AddChild("ROSpecID", msg.ROSpecID.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_STOP_ROSPEC_RESPONSE msg)
        {
            var root = new LLRPMessageNode("STOP_ROSPEC_RESPONSE");
            if (msg.LLRPStatus != null)
                root.Children.Add(msg.LLRPStatus.BuildTreeNode());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_DELETE_ROSPEC msg)
        {
            var root = new LLRPMessageNode("DELETE_ROSPEC");
            root.AddChild("ROSpecID", msg.ROSpecID.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_DELETE_ROSPEC_RESPONSE msg)
        {
            var root = new LLRPMessageNode("DELETE_ROSPEC_RESPONSE");
            if (msg.LLRPStatus != null)
                root.Children.Add(msg.LLRPStatus.BuildTreeNode());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_ENABLE_ROSPEC msg)
        {
            var root = new LLRPMessageNode("ENABLE_ROSPEC");
            root.AddChild("ROSpecID", msg.ROSpecID.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_ENABLE_ROSPEC_RESPONSE msg)
        {
            var root = new LLRPMessageNode("ENABLE_ROSPEC_RESPONSE");
            if (msg.LLRPStatus != null)
                root.Children.Add(msg.LLRPStatus.BuildTreeNode());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_DISABLE_ROSPEC msg)
        {
            var root = new LLRPMessageNode("DISABLE_ROSPEC");
            root.AddChild("ROSpecID", msg.ROSpecID.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_DISABLE_ROSPEC_RESPONSE msg)
        {
            var root = new LLRPMessageNode("DISABLE_ROSPEC_RESPONSE");
            if (msg.LLRPStatus != null)
                root.Children.Add(msg.LLRPStatus.BuildTreeNode());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_GET_ROSPECS msg)
        {
            var root = new LLRPMessageNode("GET_ROSPECS");
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_GET_ROSPECS_RESPONSE msg)
        {
            var root = new LLRPMessageNode("GET_ROSPECS_RESPONSE");
            if (msg.LLRPStatus != null)
                root.Children.Add(msg.LLRPStatus.BuildTreeNode());
            if (msg.ROSpec != null && msg.ROSpec.Length > 0)
            {
                var node = root.AddChild("ROSpec", $"Count={msg.ROSpec.Length}");
                for (int i = 0; i < msg.ROSpec.Length; i++)
                {
                    var item = msg.ROSpec[i];
                    if (item != null)
                        node.Children.Add(item.BuildTreeNode());
                    else
                        node.AddChild($"ROSpec[{i}]", description: "null");
                }
            }
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_ROSpec p)
        {
            var root = new LLRPMessageNode("ROSpec");
            root.AddChild("ROSpecID", p.ROSpecID.ToString());
            root.AddChild("Priority", p.Priority.ToString());
            root.AddChild("CurrentState", p.CurrentState.ToString());

            if (p.ROBoundarySpec != null)
                root.Children.Add(p.ROBoundarySpec.BuildTreeNode());

            if (p.SpecParameter != null && p.SpecParameter.Count > 0)
            {
                var node = root.AddChild("SpecParameter", $"Count={p.SpecParameter.Count}");
                for (int i = 0; i < p.SpecParameter.Count; i++)
                {
                    var item = p.SpecParameter[i];
                    if (item is PARAM_AISpec ai)
                        node.Children.Add(ai.BuildTreeNode());
                    else if (item is PARAM_RFSurveySpec rf)
                        node.Children.Add(rf.BuildTreeNode());
                    else
                        node.AddChild($"[{i}] {item?.GetType().Name}", description: item?.GetType().Name)
                            .AddChild("ToString()", item?.ToString() ?? "null");
                }
            }

            if (p.ROReportSpec != null)
                root.Children.Add(p.ROReportSpec.BuildTreeNode());

            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_ROBoundarySpec p)
        {
            var root = new LLRPMessageNode("ROBoundarySpec");
            if (p.ROSpecStartTrigger != null)
                root.Children.Add(p.ROSpecStartTrigger.BuildTreeNode());
            if (p.ROSpecStopTrigger != null)
                root.Children.Add(p.ROSpecStopTrigger.BuildTreeNode());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_ROSpecStartTrigger p)
        {
            var root = new LLRPMessageNode("ROSpecStartTrigger");
            root.AddChild("ROSpecStartTriggerType", p.ROSpecStartTriggerType.ToString());
            if (p.PeriodicTriggerValue != null)
                root.Children.Add(p.PeriodicTriggerValue.BuildTreeNode());
            if (p.GPITriggerValue != null)
                root.Children.Add(p.GPITriggerValue.BuildTreeNode());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_ROSpecStopTrigger p)
        {
            var root = new LLRPMessageNode("ROSpecStopTrigger");
            root.AddChild("ROSpecStopTriggerType", p.ROSpecStopTriggerType.ToString());
            root.AddChild("DurationTriggerValue", p.DurationTriggerValue.ToString());
            if (p.GPITriggerValue != null)
                root.Children.Add(p.GPITriggerValue.BuildTreeNode());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_PeriodicTriggerValue p)
        {
            var root = new LLRPMessageNode("PeriodicTriggerValue");
            root.AddChild("Offset", p.Offset.ToString());
            root.AddChild("Period", p.Period.ToString());
            if (p.UTCTimestamp != null)
                root.Children.Add(p.UTCTimestamp.BuildTreeNode());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_GPITriggerValue p)
        {
            var root = new LLRPMessageNode("GPITriggerValue");
            root.AddChild("GPIPortNum", p.GPIPortNum.ToString());
            root.AddChild("GPIEvent", p.GPIEvent.ToString());
            root.AddChild("Timeout", p.Timeout.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_UTCTimestamp p)
        {
            var root = new LLRPMessageNode("UTCTimestamp");
            root.AddChild("Microseconds", p.Microseconds.ToString(), LlrpDisplayHelper.FormatUtcMicroseconds(p.Microseconds));
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_Uptime p)
        {
            var root = new LLRPMessageNode("Uptime");
            root.AddChild("Microseconds", p.Microseconds.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_AISpec p)
        {
            var root = new LLRPMessageNode("AISpec");
            if (p.AntennaIDs != null)
            {
                var node = root.AddChild("AntennaIDs", $"Count={p.AntennaIDs.Count}");
                node.AddChild("Values", p.AntennaIDs.ToString());
            }

            if (p.AISpecStopTrigger != null)
                root.Children.Add(p.AISpecStopTrigger.BuildTreeNode());

            if (p.InventoryParameterSpec != null && p.InventoryParameterSpec.Length > 0)
            {
                var node = root.AddChild("InventoryParameterSpec", $"Count={p.InventoryParameterSpec.Length}");
                for (int i = 0; i < p.InventoryParameterSpec.Length; i++)
                {
                    var item = p.InventoryParameterSpec[i];
                    if (item != null)
                        node.Children.Add(item.BuildTreeNode());
                    else
                        node.AddChild($"InventoryParameterSpec[{i}]", description: "null");
                }
            }

            if (p.Custom != null && p.Custom.Length > 0)
            {
                var node = root.AddChild("Custom", $"Count={p.Custom.Length}");
                for (int i = 0; i < p.Custom.Length; i++)
                {
                    var item = p.Custom[i];
                    node.AddChild($"Custom[{i}]", description: item?.GetType().Name)
                        .AddChild("ToString()", item?.ToString() ?? "null");
                }
            }

            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_AISpecStopTrigger p)
        {
            var root = new LLRPMessageNode("AISpecStopTrigger");
            root.AddChild("AISpecStopTriggerType", p.AISpecStopTriggerType.ToString());
            root.AddChild("DurationTrigger", p.DurationTrigger.ToString());
            if (p.GPITriggerValue != null)
                root.Children.Add(p.GPITriggerValue.BuildTreeNode());
            if (p.TagObservationTrigger != null)
                root.Children.Add(p.TagObservationTrigger.BuildTreeNode());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_TagObservationTrigger p)
        {
            var root = new LLRPMessageNode("TagObservationTrigger");
            root.AddChild("TriggerType", p.TriggerType.ToString());
            root.AddChild("NumberOfTags", p.NumberOfTags.ToString());
            root.AddChild("NumberOfAttempts", p.NumberOfAttempts.ToString());
            root.AddChild("T", p.T.ToString());
            root.AddChild("Timeout", p.Timeout.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_InventoryParameterSpec p)
        {
            var root = new LLRPMessageNode("InventoryParameterSpec");
            root.AddChild("InventoryParameterSpecID", p.InventoryParameterSpecID.ToString());
            root.AddChild("ProtocolID", p.ProtocolID.ToString());

            if (p.AntennaConfiguration != null && p.AntennaConfiguration.Length > 0)
            {
                var node = root.AddChild("AntennaConfiguration", $"Count={p.AntennaConfiguration.Length}");
                for (int i = 0; i < p.AntennaConfiguration.Length; i++)
                {
                    var item = p.AntennaConfiguration[i];
                    if (item != null)
                        node.Children.Add(item.BuildTreeNode());
                    else
                        node.AddChild($"AntennaConfiguration[{i}]", description: "null");
                }
            }

            if (p.Custom != null && p.Custom.Length > 0)
            {
                var node = root.AddChild("Custom", $"Count={p.Custom.Length}");
                for (int i = 0; i < p.Custom.Length; i++)
                {
                    var item = p.Custom[i];
                    node.AddChild($"Custom[{i}]", description: item?.GetType().Name)
                        .AddChild("ToString()", item?.ToString() ?? "null");
                }
            }

            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_RFSurveySpec p)
        {
            var root = new LLRPMessageNode("RFSurveySpec");
            root.AddChild("AntennaID", p.AntennaID.ToString());
            root.AddChild("StartFrequency", p.StartFrequency.ToString());
            root.AddChild("EndFrequency", p.EndFrequency.ToString());
            if (p.RFSurveySpecStopTrigger != null)
                root.Children.Add(p.RFSurveySpecStopTrigger.BuildTreeNode());
            if (p.Custom != null && p.Custom.Length > 0)
            {
                var node = root.AddChild("Custom", $"Count={p.Custom.Length}");
                for (int i = 0; i < p.Custom.Length; i++)
                {
                    var item = p.Custom[i];
                    node.AddChild($"Custom[{i}]", description: item?.GetType().Name)
                        .AddChild("ToString()", item?.ToString() ?? "null");
                }
            }
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_RFSurveySpecStopTrigger p)
        {
            var root = new LLRPMessageNode("RFSurveySpecStopTrigger");
            root.AddChild("StopTriggerType", p.StopTriggerType.ToString());
            root.AddChild("DurationPeriod", p.DurationPeriod.ToString());
            root.AddChild("N", p.N.ToString());
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
                root.Children.Add(p.UHFBandCapabilities.BuildTreeNode());

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
            if (p.Timestamp != null && p.Timestamp.Count > 0)
            {
                for (int i = 0; i < p.Timestamp.Count; i++)
                {
                    var item = p.Timestamp[i];
                    LLRPMessageNode itemNode;
                    if (item is PARAM_UTCTimestamp utcTimestamp)
                    {
                        itemNode = utcTimestamp.BuildTreeNode();
                        itemNode.Name = $"[{i}] UTCTimestamp";
                    }
                    else if (item is PARAM_Uptime uptime)
                    {
                        itemNode = uptime.BuildTreeNode();
                        itemNode.Name = $"[{i}] Uptime";
                    }
                    else
                    {
                        itemNode = root.AddChild($"[{i}] {item?.GetType().Name}", description: item?.GetType().Name)
                            .AddChild("ToString()", item?.ToString() ?? "null");
                        continue;
                    }
                    root.Children.Add(itemNode);
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
            root.AddChild("RequestedData", LlrpDisplayHelper.FormatEnum(msg.RequestedData));
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

        public static LLRPMessageNode BuildTreeNode(this MSG_GET_READER_CAPABILITIES_RESPONSE msg)
        {
            var root = new LLRPMessageNode("GET_READER_CAPABILITIES_RESPONSE", description: $"MessageID={msg.MSG_ID}");
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
            if (msg.ReaderEventNotificationData != null)
            {
                root.Children.Add(msg.ReaderEventNotificationData.BuildTreeNode());
            }
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_ERROR_MESSAGE msg)
        {
            var root = new LLRPMessageNode("ERROR_MESSAGE", description: $"MessageID={msg.MSG_ID}");
            if (msg.LLRPStatus != null)
            {
                root.Children.Add(msg.LLRPStatus.BuildTreeNode());
            }
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_UHFBandCapabilities p)
        {
            var root = new LLRPMessageNode("UHFBandCapabilities");
            if (p.TransmitPowerLevelTableEntry != null)
            {
                var transmitPowerLevelTableEntryNode = root.AddChild("TransmitPowerLevelTableEntry", $"Count={p.TransmitPowerLevelTableEntry.Length}");
                var nodes = p.TransmitPowerLevelTableEntry.BuildTreeNodes();
                for (int i = 0; i < nodes.Length; i++)
                {
                    transmitPowerLevelTableEntryNode.Children.Add(nodes[i]);
                }

            }
            return root;
        }


        private static LLRPMessageNode[] BuildTreeNodes(this PARAM_TransmitPowerLevelTableEntry[] ps)
        {
            LLRPMessageNode[] nodes = new LLRPMessageNode[ps.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                nodes[i] = new LLRPMessageNode($"TransmitPowerLevelTableEntry[{i}]");
                nodes[i].AddChild("Index", ps[i]?.Index.ToString());
                nodes[i].AddChild("TransmitPowerValue", ps[i]?.TransmitPowerValue.ToString());
            }
            return nodes;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_CLIENT_REQUEST_OP msg)
        {
            var root = new LLRPMessageNode("CLIENT_REQUEST_OP", $"MessageID={msg.MSG_ID}");
            root.AddChild("ToString", msg.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_ADD_ACCESSSPEC msg)
        {
            var root = new LLRPMessageNode("ADD_ACCESSSPEC");
            if (msg.AccessSpec != null)
                root.Children.Add(msg.AccessSpec.BuildTreeNode());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_ADD_ACCESSSPEC_RESPONSE msg)
        {
            var root = new LLRPMessageNode("ADD_ACCESSSPEC_RESPONSE");
            if (msg.LLRPStatus != null)
                root.Children.Add(msg.LLRPStatus.BuildTreeNode());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_DISABLE_ACCESSSPEC msg)
        {
            var root = new LLRPMessageNode("DISABLE_ACCESSSPEC");
            root.AddChild("AccessSpecID", msg.AccessSpecID.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_DISABLE_ACCESSSPEC_RESPONSE msg)
        {
            var root = new LLRPMessageNode("DISABLE_ACCESSSPEC_RESPONSE");
            if (msg.LLRPStatus != null)
                root.Children.Add(msg.LLRPStatus.BuildTreeNode());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_DELETE_ACCESSSPEC msg)
        {
            var root = new LLRPMessageNode("DELETE_ACCESSSPEC");
            root.AddChild("AccessSpecID", msg.AccessSpecID.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_DELETE_ACCESSSPEC_RESPONSE msg)
        {
            var root = new LLRPMessageNode("DELETE_ACCESSSPEC_RESPONSE");
            if (msg.LLRPStatus != null)
                root.Children.Add(msg.LLRPStatus.BuildTreeNode());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_ENABLE_ACCESSSPEC msg)
        {
            var root = new LLRPMessageNode("ENABLE_ACCESSSPEC");
            root.AddChild("AccessSpecID", msg.AccessSpecID.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_ENABLE_ACCESSSPEC_RESPONSE msg)
        {
            var root = new LLRPMessageNode("ENABLE_ACCESSSPEC_RESPONSE");
            if (msg.LLRPStatus != null)
                root.Children.Add(msg.LLRPStatus.BuildTreeNode());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_GET_ACCESSSPECS msg)
        {
            var root = new LLRPMessageNode("GET_ACCESSSPECS");
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_GET_ACCESSSPECS_RESPONSE msg)
        {
            var root = new LLRPMessageNode("GET_ACCESSSPECS_RESPONSE");
            if (msg.LLRPStatus != null)
                root.Children.Add(msg.LLRPStatus.BuildTreeNode());
            if (msg.AccessSpec != null && msg.AccessSpec.Length > 0)
            {
                var node = root.AddChild("AccessSpec", $"Count={msg.AccessSpec.Length}");
                for (int i = 0; i < msg.AccessSpec.Length; i++)
                {
                    var item = msg.AccessSpec[i];
                    if (item != null)
                        node.Children.Add(item.BuildTreeNode());
                    else
                        node.AddChild($"AccessSpec[{i}]", description: "null");
                }
            }
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_AccessSpec p)
        {
            var root = new LLRPMessageNode("AccessSpec");
            root.AddChild("AccessSpecID", p.AccessSpecID.ToString());
            root.AddChild("AntennaID", p.AntennaID.ToString());
            root.AddChild("ProtocolID", p.ProtocolID.ToString());
            root.AddChild("CurrentState", p.CurrentState.ToString());
            root.AddChild("ROSpecID", p.ROSpecID.ToString());

            if (p.AccessSpecStopTrigger != null)
                root.Children.Add(p.AccessSpecStopTrigger.BuildTreeNode());
            if (p.AccessCommand != null)
                root.Children.Add(p.AccessCommand.BuildTreeNode());
            if (p.AccessReportSpec != null)
                root.Children.Add(p.AccessReportSpec.BuildTreeNode());

            if (p.Custom != null && p.Custom.Length > 0)
            {
                var node = root.AddChild("Custom", $"Count={p.Custom.Length}");
                for (int i = 0; i < p.Custom.Length; i++)
                {
                    var item = p.Custom[i];
                    node.AddChild($"Custom[{i}]", description: item?.GetType().Name)
                        .AddChild("ToString()", item?.ToString() ?? "null");
                }
            }
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_AccessSpecStopTrigger p)
        {
            var root = new LLRPMessageNode("AccessSpecStopTrigger");
            root.AddChild("AccessSpecStopTrigger", p.AccessSpecStopTrigger.ToString());
            root.AddChild("OperationCountValue", p.OperationCountValue.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_AccessCommand p)
        {
            var root = new LLRPMessageNode("AccessCommand");

            if (p.AirProtocolTagSpec != null && p.AirProtocolTagSpec.Count > 0)
            {
                var node = root.AddChild("AirProtocolTagSpec", $"Count={p.AirProtocolTagSpec.Count}");
                for (int i = 0; i < p.AirProtocolTagSpec.Count; i++)
                {
                    var item = p.AirProtocolTagSpec[i];
                    if (item is PARAM_C1G2TagSpec tagSpec)
                        node.Children.Add(tagSpec.BuildTreeNode());
                    else
                        node.AddChild($"[{i}] {item?.GetType().Name}", description: item?.GetType().Name)
                            .AddChild("ToString()", item?.ToString() ?? "null");
                }
            }

            if (p.AccessCommandOpSpec != null && p.AccessCommandOpSpec.Count > 0)
            {
                var node = root.AddChild("AccessCommandOpSpec", $"Count={p.AccessCommandOpSpec.Count}");
                for (int i = 0; i < p.AccessCommandOpSpec.Count; i++)
                {
                    var item = p.AccessCommandOpSpec[i];
                    if (item is PARAM_C1G2Read read)
                        node.Children.Add(read.BuildTreeNode());
                    else if (item is PARAM_C1G2Write write)
                        node.Children.Add(write.BuildTreeNode());
                    else if (item is PARAM_C1G2Kill kill)
                        node.Children.Add(kill.BuildTreeNode());
                    else if (item is PARAM_C1G2Lock lockOp)
                        node.Children.Add(lockOp.BuildTreeNode());
                    else if (item is PARAM_C1G2BlockErase erase)
                        node.Children.Add(erase.BuildTreeNode());
                    else if (item is PARAM_C1G2BlockWrite blockWrite)
                        node.Children.Add(blockWrite.BuildTreeNode());
                    else if (item is PARAM_ClientRequestOpSpec clientReq)
                        node.Children.Add(clientReq.BuildTreeNode());
                    else
                        node.AddChild($"[{i}] {item?.GetType().Name}", description: item?.GetType().Name)
                            .AddChild("ToString()", item?.ToString() ?? "null");
                }
            }

            if (p.Custom != null && p.Custom.Length > 0)
            {
                var node = root.AddChild("Custom", $"Count={p.Custom.Length}");
                for (int i = 0; i < p.Custom.Length; i++)
                {
                    var item = p.Custom[i];
                    node.AddChild($"Custom[{i}]", description: item?.GetType().Name)
                        .AddChild("ToString()", item?.ToString() ?? "null");
                }
            }
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_C1G2TagSpec p)
        {
            var root = new LLRPMessageNode("C1G2TagSpec");
            if (p.C1G2TargetTag != null && p.C1G2TargetTag.Length > 0)
            {
                var node = root.AddChild("C1G2TargetTag", $"Count={p.C1G2TargetTag.Length}");
                for (int i = 0; i < p.C1G2TargetTag.Length; i++)
                {
                    var item = p.C1G2TargetTag[i];
                    if (item != null)
                        node.Children.Add(item.BuildTreeNode());
                    else
                        node.AddChild($"C1G2TargetTag[{i}]", description: "null");
                }
            }
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_C1G2TargetTag p)
        {
            var root = new LLRPMessageNode("C1G2TargetTag");
            root.AddChild("MB", p.MB.ToString());
            root.AddChild("Pointer", p.Pointer.ToString());
            if (p.Match != null)
                root.AddChild("Match", p.Match.ToString());
            if (p.TagMask != null)
                root.AddChild("TagMask", p.TagMask.ToHexString());
            if (p.TagData != null)
                root.AddChild("TagData", p.TagData.ToHexString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_C1G2Read p)
        {
            var root = new LLRPMessageNode("C1G2Read");
            root.AddChild("OpSpecID", p.OpSpecID.ToString());
            root.AddChild("AccessPassword", p.AccessPassword.ToString());
            root.AddChild("MB", p.MB.ToString());
            root.AddChild("WordPointer", p.WordPointer.ToString());
            root.AddChild("WordCount", p.WordCount.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_C1G2Write p)
        {
            var root = new LLRPMessageNode("C1G2Write");
            root.AddChild("OpSpecID", p.OpSpecID.ToString());
            root.AddChild("AccessPassword", p.AccessPassword.ToString());
            root.AddChild("MB", p.MB.ToString());
            root.AddChild("WordPointer", p.WordPointer.ToString());
            if (p.WriteData != null)
                root.AddChild("WriteData", p.WriteData.ToHexString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_C1G2Kill p)
        {
            var root = new LLRPMessageNode("C1G2Kill");
            root.AddChild("OpSpecID", p.OpSpecID.ToString());
            root.AddChild("KillPassword", p.KillPassword.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_C1G2Lock p)
        {
            var root = new LLRPMessageNode("C1G2Lock");
            root.AddChild("OpSpecID", p.OpSpecID.ToString());
            root.AddChild("AccessPassword", p.AccessPassword.ToString());
            if (p.C1G2LockPayload != null && p.C1G2LockPayload.Length > 0)
            {
                var node = root.AddChild("C1G2LockPayload", $"Count={p.C1G2LockPayload.Length}");
                for (int i = 0; i < p.C1G2LockPayload.Length; i++)
                {
                    var item = p.C1G2LockPayload[i];
                    if (item != null)
                        node.Children.Add(item.BuildTreeNode());
                    else
                        node.AddChild($"C1G2LockPayload[{i}]", description: "null");
                }
            }
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_C1G2LockPayload p)
        {
            var root = new LLRPMessageNode("C1G2LockPayload");
            root.AddChild("Privilege", p.Privilege.ToString());
            root.AddChild("DataField", p.DataField.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_C1G2BlockErase p)
        {
            var root = new LLRPMessageNode("C1G2BlockErase");
            root.AddChild("OpSpecID", p.OpSpecID.ToString());
            root.AddChild("AccessPassword", p.AccessPassword.ToString());
            root.AddChild("MB", p.MB.ToString());
            root.AddChild("WordPointer", p.WordPointer.ToString());
            root.AddChild("WordCount", p.WordCount.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_C1G2BlockWrite p)
        {
            var root = new LLRPMessageNode("C1G2BlockWrite");
            root.AddChild("OpSpecID", p.OpSpecID.ToString());
            root.AddChild("AccessPassword", p.AccessPassword.ToString());
            root.AddChild("MB", p.MB.ToString());
            root.AddChild("WordPointer", p.WordPointer.ToString());
            if (p.WriteData != null)
                root.AddChild("WriteData", p.WriteData.ToHexString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this PARAM_ClientRequestOpSpec p)
        {
            var root = new LLRPMessageNode("ClientRequestOpSpec");
            root.AddChild("OpSpecID", p.OpSpecID.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_CUSTOM_MESSAGE msg)
        {
            var root = new LLRPMessageNode("CUSTOM_MESSAGE", $"MessageID={msg.MSG_ID}");
            root.AddChild("ToString", msg.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_CLOSE_CONNECTION msg)
        {
            var root = new LLRPMessageNode("CLOSE_CONNECTION", $"MessageID={msg.MSG_ID}");
            root.AddChild("ToString", msg.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_CLOSE_CONNECTION_RESPONSE msg)
        {
            var root = new LLRPMessageNode("CLOSE_CONNECTION_RESPONSE", $"MessageID={msg.MSG_ID}");
            root.AddChild("ToString", msg.ToString());
            return root;
        }

        public static LLRPMessageNode BuildTreeNode(this MSG_KEEPALIVE_ACK msg)
        {
            var root = new LLRPMessageNode("KEEPALIVE_ACK", $"MessageID={msg.MSG_ID}");
            root.AddChild("ToString", msg.ToString());
            return root;
        }
    }


   
}
