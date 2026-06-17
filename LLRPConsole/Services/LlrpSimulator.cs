using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Org.LLRP.LTK.LLRPV1;
using Org.LLRP.LTK.LLRPV1.DataType;

namespace LLRPConsole.Services
{
    /// <summary>
    /// A high-fidelity local virtual LLRP reader simulator listening on IPAddress.Loopback.
    /// Acts as a TCP server and responds to standard LLRP client connections and binary control frames.
    /// </summary>
    public class LlrpSimulator
    {
        private TcpListener _listener;
        private CancellationTokenSource _cts;
        private Task _listenTask;
        private bool _isInventoryRunning;
        private Timer _tagReportTimer;
        private Timer _keepaliveTimer;
        private int _port = 50840;
        private ENUM_ROSpecState _roSpecState = ENUM_ROSpecState.Disabled;

        public int Port => _port;
        public bool IsRunning => _listener != null;

        public void Start(int port = 50840)
        {
            if (_listener != null) return;
            _port = port;
            _listener = new TcpListener(IPAddress.Loopback, _port);
            _listener.Start();
            _cts = new CancellationTokenSource();
            _listenTask = Task.Run(() => ListenAsync(_cts.Token));
        }

        public void Stop()
        {
            if (_listener == null) return;
            try
            {
                _cts?.Cancel();
                _listener?.Stop();
            }
            catch { }
            _tagReportTimer?.Dispose();
            _tagReportTimer = null;
            _keepaliveTimer?.Dispose();
            _keepaliveTimer = null;
            _listener = null;
            _cts = null;
            _isInventoryRunning = false;
        }

        private async Task ListenAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync(ct);
                    _ = Task.Run(() => HandleClientAsync(client, ct));
                }
                catch
                {
                    break;
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
        {
            using (client)
            using (var stream = client.GetStream())
            {
                try
                {
                    // 1. Send READER_EVENT_NOTIFICATION (Connection greeting)
                    SendConnectionGreeting(stream);

                    // 2. Start periodic KEEPALIVE timer (every 5 seconds) to prevent client timeouts
                    var keepaliveMsgId = 1U;
                    _keepaliveTimer = new Timer(_ =>
                    {
                        try
                        {
                            var keepalive = new MSG_KEEPALIVE();
                            keepalive.MSG_ID = keepaliveMsgId++;
                            SendMsg(stream, keepalive);
                        }
                        catch { }
                    }, null, 5000, 5000);

                    byte[] header = new byte[10];
                    while (!ct.IsCancellationRequested && client.Connected)
                    {
                        // Read binary message header (10 bytes)
                        int read = await ReadBytesAsync(stream, header, 10, ct);
                        if (read < 10) break;

                        // Parse header information
                        int num = ((int)header[0] << 8) + (int)header[1];
                        short msgType = (short)(num & 1023);
                        int msgLen = ((int)header[2] << 24) + ((int)header[3] << 16) + ((int)header[4] << 8) + (int)header[5];
                        uint msgId = ((uint)header[6] << 24) + ((uint)header[7] << 16) + ((uint)header[8] << 8) + (uint)header[9];

                        // Read remaining message body
                        byte[] body = new byte[msgLen - 10];
                        if (body.Length > 0)
                        {
                            read = await ReadBytesAsync(stream, body, body.Length, ct);
                            if (read < body.Length) break;
                        }

                        // Process message and send matching response
                        ProcessAndReply(stream, msgType, msgId);
                    }
                }
                catch { }
            }
            _tagReportTimer?.Dispose();
            _tagReportTimer = null;
            _keepaliveTimer?.Dispose();
            _keepaliveTimer = null;
            _isInventoryRunning = false;
        }

        private async Task<int> ReadBytesAsync(NetworkStream stream, byte[] buffer, int length, CancellationToken ct)
        {
            int offset = 0;
            while (offset < length)
            {
                int read = await stream.ReadAsync(buffer, offset, length - offset, ct);
                if (read <= 0) return offset;
                offset += read;
            }
            return offset;
        }

        private void SendConnectionGreeting(NetworkStream stream)
        {
            var msg = new MSG_READER_EVENT_NOTIFICATION();
            msg.MSG_ID = 0;
            msg.ReaderEventNotificationData = new PARAM_ReaderEventNotificationData();
            msg.ReaderEventNotificationData.Timestamp = new UNION_Timestamp();
            
            var utcTimestamp = new PARAM_UTCTimestamp();
            utcTimestamp.Microseconds = (ulong)(DateTime.UtcNow.Ticks / 10);
            msg.ReaderEventNotificationData.Timestamp.Add(utcTimestamp);
            
            msg.ReaderEventNotificationData.ConnectionAttemptEvent = new PARAM_ConnectionAttemptEvent();
            msg.ReaderEventNotificationData.ConnectionAttemptEvent.Status = ENUM_ConnectionAttemptStatusType.Success;

            SendMsg(stream, msg);
        }

        private void ProcessAndReply(NetworkStream stream, short msgType, uint msgId)
        {
            switch (msgType)
            {
                case 1: // GET_READER_CAPABILITIES
                    var capRes = new MSG_GET_READER_CAPABILITIES_RESPONSE();
                    capRes.MSG_ID = msgId;
                    capRes.LLRPStatus = new PARAM_LLRPStatus { StatusCode = ENUM_StatusCode.M_Success };
                    
                    capRes.GeneralDeviceCapabilities = new PARAM_GeneralDeviceCapabilities
                    {
                        MaxNumberOfAntennaSupported = 4,
                        CanSetAntennaProperties = true,
                        HasUTCClockCapability = true,
                        DeviceManufacturerName = 258, // Zebra
                        ModelName = 1,
                        ReaderFirmwareVersion = "Virtual_1.0.0",
                        GPIOCapabilities = new PARAM_GPIOCapabilities
                        {
                            NumGPIs = 4,
                            NumGPOs = 4
                        }
                    };

                    capRes.GeneralDeviceCapabilities.ReceiveSensitivityTableEntry = new PARAM_ReceiveSensitivityTableEntry[1];
                    capRes.GeneralDeviceCapabilities.ReceiveSensitivityTableEntry[0] = new PARAM_ReceiveSensitivityTableEntry
                    {
                        Index = 1,
                        ReceiveSensitivityValue = -80
                    };

                    capRes.LLRPCapabilities = new PARAM_LLRPCapabilities
                    {
                        CanDoRFSurvey = false,
                        CanReportBufferFillWarning = false,
                        SupportsClientRequestOpSpec = false,
                        CanDoTagInventoryStateAwareSingulation = false,
                        SupportsEventAndReportHolding = false,
                        MaxNumPriorityLevelsSupported = 1,
                        ClientRequestOpSpecTimeout = 0,
                        MaxNumROSpecs = 1,
                        MaxNumSpecsPerROSpec = 1,
                        MaxNumInventoryParameterSpecsPerAISpec = 1,
                        MaxNumAccessSpecs = 1,
                        MaxNumOpSpecsPerAccessSpec = 1
                    };

                    capRes.RegulatoryCapabilities = new PARAM_RegulatoryCapabilities
                    {
                        CountryCode = 840,
                        CommunicationsStandard = ENUM_CommunicationsStandard.US_FCC_Part_15
                    };
                    
                    capRes.RegulatoryCapabilities.UHFBandCapabilities = new PARAM_UHFBandCapabilities();
                    
                    capRes.RegulatoryCapabilities.UHFBandCapabilities.TransmitPowerLevelTableEntry = new PARAM_TransmitPowerLevelTableEntry[1];
                    capRes.RegulatoryCapabilities.UHFBandCapabilities.TransmitPowerLevelTableEntry[0] = new PARAM_TransmitPowerLevelTableEntry
                    {
                        Index = 1,
                        TransmitPowerValue = 3000 // 30 dBm
                    };

                    capRes.RegulatoryCapabilities.UHFBandCapabilities.FrequencyInformation = new PARAM_FrequencyInformation
                    {
                        Hopping = false,
                        FixedFrequencyTable = new PARAM_FixedFrequencyTable()
                    };
                    capRes.RegulatoryCapabilities.UHFBandCapabilities.FrequencyInformation.FixedFrequencyTable.Frequency.Add(920250); // 920.25 MHz in kHz

                    var rfModeTable = new PARAM_C1G2UHFRFModeTable();
                    rfModeTable.C1G2UHFRFModeTableEntry = new PARAM_C1G2UHFRFModeTableEntry[1];
                    rfModeTable.C1G2UHFRFModeTableEntry[0] = new PARAM_C1G2UHFRFModeTableEntry
                    {
                        ModeIdentifier = 1002,
                        DRValue = ENUM_C1G2DRValue.DRV_8,
                        EPCHAGTCConformance = true,
                        MValue = ENUM_C1G2MValue.MV_4,
                        ForwardLinkModulation = ENUM_C1G2ForwardLinkModulation.PR_ASK,
                        SpectralMaskIndicator = ENUM_C1G2SpectralMaskIndicator.SI,
                        BDRValue = 250000,
                        PIEValue = 1500,
                        MinTariValue = 6250,
                        MaxTariValue = 6250,
                        StepTariValue = 0
                    };
                    capRes.RegulatoryCapabilities.UHFBandCapabilities.AirProtocolUHFRFModeTable.Add(rfModeTable);

                    var c1g2Cap = new PARAM_C1G2LLRPCapabilities
                    {
                        CanSupportBlockErase = true,
                        CanSupportBlockWrite = true,
                        MaxNumSelectFiltersPerQuery = 2
                    };
                    capRes.AirProtocolLLRPCapabilities.Add(c1g2Cap);

                    SendMsg(stream, capRes);
                    break;

                case 2: // GET_READER_CONFIG
                    var confRes = new MSG_GET_READER_CONFIG_RESPONSE();
                    confRes.MSG_ID = msgId;
                    confRes.LLRPStatus = new PARAM_LLRPStatus { StatusCode = ENUM_StatusCode.M_Success };
                    
                    confRes.AntennaProperties = new PARAM_AntennaProperties[4];
                    for (int i = 0; i < 4; i++)
                    {
                        confRes.AntennaProperties[i] = new PARAM_AntennaProperties
                        {
                            AntennaConnected = true,
                            AntennaID = (ushort)(i + 1),
                            AntennaGain = 0
                        };
                    }

                    confRes.GPIPortCurrentState = new PARAM_GPIPortCurrentState[4];
                    for (int i = 0; i < 4; i++)
                    {
                        confRes.GPIPortCurrentState[i] = new PARAM_GPIPortCurrentState
                        {
                            GPIPortNum = (ushort)(i + 1),
                            Config = true,
                            State = ENUM_GPIPortState.Low
                        };
                    }

                    confRes.EventsAndReports = new PARAM_EventsAndReports
                    {
                        HoldEventsAndReportsUponReconnect = false
                    };

                    confRes.KeepaliveSpec = new PARAM_KeepaliveSpec
                    {
                        KeepaliveTriggerType = ENUM_KeepaliveTriggerType.Null,
                        PeriodicTriggerValue = 0
                    };

                    confRes.ReaderEventNotificationSpec = new PARAM_ReaderEventNotificationSpec();
                    confRes.ReaderEventNotificationSpec.EventNotificationState = new PARAM_EventNotificationState[5];
                    for (int i = 0; i < 5; i++)
                    {
                        confRes.ReaderEventNotificationSpec.EventNotificationState[i] = new PARAM_EventNotificationState
                        {
                            EventType = (ENUM_NotificationEventType)i,
                            NotificationState = true
                        };
                    }

                    SendMsg(stream, confRes);
                    break;

                case 26: // GET_ROSPECS
                    var roSpecRes = new MSG_GET_ROSPECS_RESPONSE();
                    roSpecRes.MSG_ID = msgId;
                    roSpecRes.LLRPStatus = new PARAM_LLRPStatus { StatusCode = ENUM_StatusCode.M_Success };
                    
                    var defaultRoSpec = new PARAM_ROSpec
                    {
                        ROSpecID = 14150,
                        CurrentState = _roSpecState,
                        ROBoundarySpec = new PARAM_ROBoundarySpec
                        {
                            ROSpecStartTrigger = new PARAM_ROSpecStartTrigger
                            {
                                ROSpecStartTriggerType = ENUM_ROSpecStartTriggerType.Null
                            },
                            ROSpecStopTrigger = new PARAM_ROSpecStopTrigger
                            {
                                ROSpecStopTriggerType = ENUM_ROSpecStopTriggerType.Null
                            }
                        },
                        ROReportSpec = new PARAM_ROReportSpec
                        {
                            ROReportTrigger = ENUM_ROReportTriggerType.Upon_N_Tags_Or_End_Of_ROSpec,
                            N = 1,
                            TagReportContentSelector = new PARAM_TagReportContentSelector
                            {
                                EnableAntennaID = true,
                                EnablePeakRSSI = true,
                                EnableTagSeenCount = true,
                                EnableFirstSeenTimestamp = true,
                                EnableLastSeenTimestamp = true
                            }
                        }
                    };

                    var aiSpec = new PARAM_AISpec
                    {
                        AntennaIDs = new UInt16Array()
                    };
                    aiSpec.AntennaIDs.Add(1);
                    aiSpec.AntennaIDs.Add(2);
                    aiSpec.AntennaIDs.Add(3);
                    aiSpec.AntennaIDs.Add(4);

                    aiSpec.InventoryParameterSpec = new PARAM_InventoryParameterSpec[1];
                    aiSpec.InventoryParameterSpec[0] = new PARAM_InventoryParameterSpec
                    {
                        InventoryParameterSpecID = 1,
                        ProtocolID = ENUM_AirProtocols.EPCGlobalClass1Gen2
                    };
                    
                    aiSpec.InventoryParameterSpec[0].AntennaConfiguration = new PARAM_AntennaConfiguration[4];
                    for (int i = 0; i < 4; i++)
                    {
                        aiSpec.InventoryParameterSpec[0].AntennaConfiguration[i] = new PARAM_AntennaConfiguration
                        {
                            AntennaID = (ushort)(i + 1)
                        };
                        var inventoryCommand = new PARAM_C1G2InventoryCommand
                        {
                            C1G2RFControl = new PARAM_C1G2RFControl { ModeIndex = 1002 },
                            C1G2SingulationControl = new PARAM_C1G2SingulationControl
                            {
                                Session = new TwoBits(0),
                                TagPopulation = 32
                            }
                        };
                        aiSpec.InventoryParameterSpec[0].AntennaConfiguration[i].AirProtocolInventoryCommandSettings.Add(inventoryCommand);
                        aiSpec.InventoryParameterSpec[0].AntennaConfiguration[i].RFReceiver = new PARAM_RFReceiver
                        {
                            ReceiverSensitivity = 1
                        };
                        aiSpec.InventoryParameterSpec[0].AntennaConfiguration[i].RFTransmitter = new PARAM_RFTransmitter
                        {
                            HopTableID = 0,
                            ChannelIndex = 0,
                            TransmitPower = 1
                        };
                    }

                    defaultRoSpec.SpecParameter = new UNION_SpecParameter();
                    defaultRoSpec.SpecParameter.Add(aiSpec);

                    roSpecRes.ROSpec = new PARAM_ROSpec[1];
                    roSpecRes.ROSpec[0] = defaultRoSpec;

                    SendMsg(stream, roSpecRes);
                    break;

                case 44: // GET_ACCESSSPECS
                    var accSpecRes = new MSG_GET_ACCESSSPECS_RESPONSE();
                    accSpecRes.MSG_ID = msgId;
                    accSpecRes.LLRPStatus = new PARAM_LLRPStatus { StatusCode = ENUM_StatusCode.M_Success };
                    accSpecRes.AccessSpec = new PARAM_AccessSpec[0];
                    SendMsg(stream, accSpecRes);
                    break;

                case 3: // SET_READER_CONFIG
                    var setConfRes = new MSG_SET_READER_CONFIG_RESPONSE();
                    setConfRes.MSG_ID = msgId;
                    setConfRes.LLRPStatus = new PARAM_LLRPStatus { StatusCode = ENUM_StatusCode.M_Success };
                    SendMsg(stream, setConfRes);
                    break;

                case 20: // ADD_ROSPEC
                    var addRoRes = new MSG_ADD_ROSPEC_RESPONSE();
                    addRoRes.MSG_ID = msgId;
                    addRoRes.LLRPStatus = new PARAM_LLRPStatus { StatusCode = ENUM_StatusCode.M_Success };
                    SendMsg(stream, addRoRes);
                    break;

                case 24: // ENABLE_ROSPEC
                    var enRoRes = new MSG_ENABLE_ROSPEC_RESPONSE();
                    enRoRes.MSG_ID = msgId;
                    enRoRes.LLRPStatus = new PARAM_LLRPStatus { StatusCode = ENUM_StatusCode.M_Success };
                    SendMsg(stream, enRoRes);
                    _roSpecState = ENUM_ROSpecState.Inactive;
                    break;

                case 25: // DISABLE_ROSPEC
                    var disRoRes = new MSG_DISABLE_ROSPEC_RESPONSE();
                    disRoRes.MSG_ID = msgId;
                    disRoRes.LLRPStatus = new PARAM_LLRPStatus { StatusCode = ENUM_StatusCode.M_Success };
                    SendMsg(stream, disRoRes);
                    _isInventoryRunning = false;
                    _roSpecState = ENUM_ROSpecState.Disabled;
                    _tagReportTimer?.Dispose();
                    _tagReportTimer = null;
                    break;

                case 22: // START_ROSPEC
                    var startRoRes = new MSG_START_ROSPEC_RESPONSE();
                    startRoRes.MSG_ID = msgId;
                    startRoRes.LLRPStatus = new PARAM_LLRPStatus { StatusCode = ENUM_StatusCode.M_Success };
                    SendMsg(stream, startRoRes);

                    _isInventoryRunning = true;
                    _roSpecState = ENUM_ROSpecState.Active;
                    StartTagReporting(stream);
                    break;

                case 23: // STOP_ROSPEC
                    var stopRoRes = new MSG_STOP_ROSPEC_RESPONSE();
                    stopRoRes.MSG_ID = msgId;
                    stopRoRes.LLRPStatus = new PARAM_LLRPStatus { StatusCode = ENUM_StatusCode.M_Success };
                    SendMsg(stream, stopRoRes);

                    _isInventoryRunning = false;
                    _roSpecState = ENUM_ROSpecState.Inactive;
                    _tagReportTimer?.Dispose();
                    _tagReportTimer = null;
                    break;

                case 14: // CLOSE_CONNECTION
                    var closeRes = new MSG_CLOSE_CONNECTION_RESPONSE();
                    closeRes.MSG_ID = msgId;
                    closeRes.LLRPStatus = new PARAM_LLRPStatus { StatusCode = ENUM_StatusCode.M_Success };
                    SendMsg(stream, closeRes);
                    break;
            }
        }

        private void StartTagReporting(NetworkStream stream)
        {
            _tagReportTimer?.Dispose();
            
            var random = new Random();
            var epcs = new string[] 
            {
                "E280113060000207B67D3B01",
                "E280113060000207B67D3B02",
                "E2003412013A900007D4F3A5",
                "E280116060000207B67D03A4",
                "E280110520003008A1B2C3D4"
            };

            _tagReportTimer = new Timer(_ =>
            {
                if (!_isInventoryRunning) return;

                try
                {
                    var report = new MSG_RO_ACCESS_REPORT();
                    report.MSG_ID = 0;
                    
                    int tagCount = random.Next(1, 4);
                    report.TagReportData = new PARAM_TagReportData[tagCount];

                    for (int i = 0; i < tagCount; i++)
                    {
                        var epcStr = epcs[random.Next(epcs.Length)];

                        var tagData = new PARAM_TagReportData();
                        
                        tagData.EPCParameter = new UNION_EPCParameter();
                        var epc96 = new PARAM_EPC_96();
                        epc96.EPC = LLRPBitArray.FromHexString(epcStr);
                        tagData.EPCParameter.Add(epc96);

                        tagData.AntennaID = new PARAM_AntennaID();
                        tagData.AntennaID.AntennaID = (ushort)random.Next(1, 5); // Ant 1 to 4

                        tagData.PeakRSSI = new PARAM_PeakRSSI();
                        tagData.PeakRSSI.PeakRSSI = (sbyte)random.Next(-75, -45); // RSSI in dBm

                        tagData.TagSeenCount = new PARAM_TagSeenCount();
                        tagData.TagSeenCount.TagCount = (ushort)random.Next(1, 6);

                        report.TagReportData[i] = tagData;
                    }

                    SendMsg(stream, report);
                }
                catch
                {
                    _tagReportTimer?.Dispose();
                    _tagReportTimer = null;
                }
            }, null, 1000, 1000);
        }

        private void SendMsg(NetworkStream stream, Message msg)
        {
            byte[] bytes = Util.ConvertBitArrayToByteArray(msg.ToBitArray());
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
        }
    }
}

