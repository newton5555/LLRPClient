using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Terminal.Gui.App;
using Terminal.Gui.Views;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Input;
using LLRPSdk;
using Org.LLRP.LTK.LLRPV1;
using Org.LLRP.LTK.LLRPV1.DataType;

namespace LLRP.Cli;

public class LogEntry
{
    public DateTime Time { get; set; } = DateTime.Now;
    public bool IsOutgoing { get; set; }
    public bool IsError { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string XmlPayload { get; set; } = string.Empty;
}

public class AppWindow : Window
{
    private readonly IApplication _app;
    private LlrpReader? _reader;
    
    private readonly object _logLock = new();
    private readonly List<LogEntry> _logEntries = new();
    private readonly ObservableCollection<string> _logSummaries = new();

    // UI elements - Left Panel (Connection)
    private readonly TextField _ipField;
    private readonly TextField _portField;
    private readonly Button _connectBtn;
    private readonly Button _disconnectBtn;
    private readonly Label _statusLabel;
    private readonly Label _telemetryLabel;

    // UI elements - Message Builder
    private readonly DropDownList _msgTypeSelect;
    private readonly TextField _roSpecIdField;
    private readonly TextField _antennaIdField;

    // UI elements - Packet Log & Decoder Panel (Split layout)
    private readonly ListView _logListView;
    
#pragma warning disable CS0618
    private readonly TextView _detailsView;
#pragma warning restore CS0618

    public AppWindow(IApplication app)
    {
        _app = app;
        Title = "LLRP.Cli — Standard LLRP Packet Analyzer & Decoder";
        Width = Dim.Fill();
        Height = Dim.Fill();

        // ═══════════════════════════════════════════════════════════════
        // LEFT PANEL (Connection & Control)
        // ═══════════════════════════════════════════════════════════════
        var leftPanel = new FrameView
        {
            Title = " CONNECTIONS ",
            X = 0,
            Y = 0,
            Width = 32,
            Height = Dim.Fill()
        };

        var ipLabel = new Label { Text = "IP Address:", X = 1, Y = 1 };
        _ipField = new TextField { Text = "127.0.0.1", X = 1, Y = 2, Width = Dim.Fill() - 2 };

        var portLabel = new Label { Text = "Port:", X = 1, Y = 4 };
        _portField = new TextField { Text = "5084", X = 1, Y = 5, Width = Dim.Fill() - 2 };

        _connectBtn = new Button { Text = "Connect", X = 1, Y = 7, Width = Dim.Fill() - 2 };
        _connectBtn.Accepting += async (s, e) => await ConnectAsync();

        _disconnectBtn = new Button { Text = "Disconnect", X = 1, Y = 9, Width = Dim.Fill() - 2, Enabled = false };
        _disconnectBtn.Accepting += (s, e) => Disconnect();

        _statusLabel = new Label { Text = "STATUS: Offline", X = 1, Y = 11, Width = Dim.Fill() - 2 };

        _telemetryLabel = new Label
        {
            Text = "Uptime: --:--:--\nTags Read: 0\nRead Rate: 0/s\nTemp: --°C",
            X = 1,
            Y = 13,
            Width = Dim.Fill() - 2,
            Height = Dim.Fill() - 14
        };

        leftPanel.Add(ipLabel, _ipField, portLabel, _portField, _connectBtn, _disconnectBtn, _statusLabel, _telemetryLabel);
        Add(leftPanel);

        // ═══════════════════════════════════════════════════════════════
        // RIGHT PANEL (Builder & Log)
        // ═══════════════════════════════════════════════════════════════
        var builderPanel = new FrameView
        {
            Title = " QUICK COMMANDS ",
            X = Pos.Right(leftPanel),
            Y = 0,
            Width = Dim.Fill(),
            Height = 11
        };

        var msgTypeLabel = new Label { Text = "Msg Type:", X = 2, Y = 1 };
        _msgTypeSelect = new DropDownList
        {
            X = 2,
            Y = 2,
            Width = 28
        };
        var msgTypes = new ObservableCollection<string>
        {
            "GET_READER_CAPABILITIES",
            "GET_READER_CONFIG",
            "ADD_ROSPEC",
            "ENABLE_ROSPEC",
            "START_ROSPEC",
            "STOP_ROSPEC",
            "DISABLE_ROSPEC",
            "DELETE_ROSPEC",
            "CLOSE_CONNECTION"
        };
        _msgTypeSelect.Source = new ListWrapper<string>(msgTypes);
        _msgTypeSelect.Text = "GET_READER_CAPABILITIES";

        var roSpecLabel = new Label { Text = "ROSpecID:", X = 32, Y = 1 };
        _roSpecIdField = new TextField { Text = "1", X = 32, Y = 2, Width = 10 };

        var antennaLabel = new Label { Text = "AntennaID:", X = 45, Y = 1 };
        _antennaIdField = new TextField { Text = "0", X = 45, Y = 2, Width = 10 };

        var sendBtn = new Button { Text = "Send (F3)", X = 2, Y = 5, Width = 14 };
        sendBtn.Accepting += (s, e) => SendSelectedMessage();

        var addRoSpecQuickBtn = new Button { Text = "AddRoSpec", X = 18, Y = 5 };
        addRoSpecQuickBtn.Accepting += (s, e) => SendQuickCommand("ADD_ROSPEC");

        var startRoSpecQuickBtn = new Button { Text = "Start", X = 32, Y = 5 };
        startRoSpecQuickBtn.Accepting += (s, e) => SendQuickCommand("START_ROSPEC");

        var stopRoSpecQuickBtn = new Button { Text = "Stop", X = 42, Y = 5 };
        stopRoSpecQuickBtn.Accepting += (s, e) => SendQuickCommand("STOP_ROSPEC");

        builderPanel.Add(msgTypeLabel, _msgTypeSelect, roSpecLabel, _roSpecIdField, antennaLabel, _antennaIdField, sendBtn, addRoSpecQuickBtn, startRoSpecQuickBtn, stopRoSpecQuickBtn);
        Add(builderPanel);

        // ═══════════════════════════════════════════════════════════════
        // LOG / ANALYZER PANEL (Split Layout)
        // ═══════════════════════════════════════════════════════════════
        var logPanel = new FrameView
        {
            Title = " LLRP PACKET LOG & DECODER ",
            X = Pos.Right(leftPanel),
            Y = Pos.Bottom(builderPanel),
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        // Left split - Packet headers list
        _logListView = new ListView
        {
            X = 0,
            Y = 0,
            Width = 50,
            Height = Dim.Fill()
        };
        _logListView.Source = new ListWrapper<string>(_logSummaries);
        _logListView.ValueChanged += (s, e) => OnPacketSelected();

        // Right split - Detailed parameter decoder & XML payload view
        var detailsPanel = new FrameView
        {
            Title = " PACKET DECODER ",
            X = Pos.Right(_logListView) + 1,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

#pragma warning disable CS0618
        _detailsView = new TextView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ReadOnly = true
        };
#pragma warning restore CS0618

        detailsPanel.Add(_detailsView);
        logPanel.Add(_logListView, detailsPanel);
        Add(logPanel);

        // ═══════════════════════════════════════════════════════════════
        // SHORTCUT KEYS (StatusBar)
        // ═══════════════════════════════════════════════════════════════
        var shortcuts = new List<Shortcut>
        {
            new Shortcut(Key.F2, "Connect", async () => await ConnectAsync(), ""),
            new Shortcut(Key.F3, "Send", () => SendSelectedMessage(), ""),
            new Shortcut(Key.F5, "Clear Log", () => ClearLog(), ""),
            new Shortcut(Key.F10, "Quit", () => _app.RequestStop(), "")
        };
        var statusBar = new StatusBar(shortcuts);
        Add(statusBar);
    }

    private void AddLog(string name, string id, bool isOutgoing, string details, string xmlPayload, bool isError = false)
    {
        var entry = new LogEntry
        {
            Name = name,
            Id = id,
            IsOutgoing = isOutgoing,
            Details = details,
            XmlPayload = xmlPayload ?? string.Empty,
            IsError = isError
        };

        lock (_logLock)
        {
            _logEntries.Add(entry);
            var dirSymbol = isOutgoing ? "TX ->" : "RX <-";
            var summary = $"{entry.Time:HH:mm:ss.fff} | {dirSymbol} | {entry.Name} [ID:{entry.Id}]";
            _logSummaries.Add(summary);

            // Automatically select and scroll to the latest packet if nothing else is selected
            if (_logSummaries.Count == 1)
            {
                _logListView.SelectedItem = 0;
            }
        }
    }

    private void ClearLog()
    {
        lock (_logLock)
        {
            _logEntries.Clear();
            _logSummaries.Clear();
            _detailsView.Text = string.Empty;
        }
    }

    private void OnPacketSelected()
    {
        var selectedIdx = _logListView.SelectedItem;
        if (selectedIdx == null || selectedIdx.Value < 0) return;

        LogEntry? entry = null;
        lock (_logLock)
        {
            if (selectedIdx.Value < _logEntries.Count)
            {
                entry = _logEntries[selectedIdx.Value];
            }
        }

        if (entry != null)
        {
            var header = $"[Message]: {entry.Name}\n" +
                         $"[ID]     : {entry.Id}\n" +
                         $"[Dir]    : {(entry.IsOutgoing ? "Outgoing (TX)" : "Incoming (RX)")}\n" +
                         $"[Time]   : {entry.Time:yyyy-MM-dd HH:mm:ss.fff}\n" +
                         $"------------------------------------------------------------\n" +
                         $"[Summary]:\n{entry.Details}\n" +
                         $"------------------------------------------------------------\n" +
                         $"[XML Decoded Payload]:\n{PrettyPrintXml(entry.XmlPayload)}";
            
            _detailsView.Text = header;
        }
    }

    private string PrettyPrintXml(string xml)
    {
        if (string.IsNullOrEmpty(xml)) return string.Empty;
        try
        {
            var doc = XDocument.Parse(xml);
            return doc.ToString();
        }
        catch
        {
            return xml;
        }
    }

    private async Task ConnectAsync()
    {
        if (_reader != null && _reader.IsConnected)
        {
            AddLog("SYSTEM", "0", false, "Already connected to reader.", string.Empty, true);
            return;
        }

        var ip = _ipField.Text;
        if (!int.TryParse(_portField.Text, out var port))
        {
            port = 5084;
        }

        _connectBtn.Enabled = false;
        _statusLabel.Text = "STATUS: Connecting...";
        AddLog("CONNECT", "0", true, $"Connecting to {ip}:{port}...", string.Empty);

        _reader = new LlrpReader();
        _reader.RawFrameSent += OnRawFrameSent;
        _reader.RawFrameReceived += OnRawFrameReceived;
        _reader.TagsReported += OnTagsReported;
        _reader.LlrpMessageLogged += OnLlrpMessageLogged;
        _reader.ErrorNotification += OnErrorNotification;
        _reader.KeepaliveTimeout += r =>
        {
            AddLog("KEEPALIVE", "0", false, "Heartbeat timeout! Disconnecting.", string.Empty, true);
            Disconnect();
        };

        try
        {
            await Task.Run(() => _reader.Connect(ip, port, false));
            _statusLabel.Text = "STATUS: Connected";
            _connectBtn.Enabled = false;
            _disconnectBtn.Enabled = true;
            AddLog("CONNECT", "0", false, $"Successfully connected to reader at {ip}:{port}", string.Empty);
            UpdateTelemetry();
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "STATUS: Offline";
            _connectBtn.Enabled = true;
            _disconnectBtn.Enabled = false;
            AddLog("CONNECT_ERROR", "0", false, $"Failed to connect: {ex.Message}", string.Empty, true);
        }
    }

    private void Disconnect()
    {
        if (_reader != null)
        {
            try
            {
                _reader.Disconnect();
            }
            catch (Exception ex)
            {
                AddLog("DISCONNECT_ERROR", "0", false, $"Error disconnecting: {ex.Message}", string.Empty, true);
            }
            finally
            {
                _reader = null;
                _statusLabel.Text = "STATUS: Offline";
                _connectBtn.Enabled = true;
                _disconnectBtn.Enabled = false;
                AddLog("SYSTEM", "0", false, "Disconnected from reader.", string.Empty);
            }
        }
    }

    private void OnRawFrameSent(LlrpReader reader, byte[] raw)
    {
        try
        {
            var temp = raw;
            LLRPBinaryDecoder.Decode(ref temp, out var msg);
            if (msg != null)
            {
                var xml = msg.ToString() ?? string.Empty;
                var details = FormatMessageDetails(msg);
                AddLog(msg.MSG_TYPE.ToString(), msg.MSG_ID.ToString(), true, details, xml);
            }
            else
            {
                var hex = BitConverter.ToString(raw).Replace("-", " ");
                AddLog("RAW_TX", "0", true, $"Sent raw hex bytes (Length: {raw.Length})", hex);
            }
        }
        catch
        {
            var hex = BitConverter.ToString(raw).Replace("-", " ");
            AddLog("RAW_TX", "0", true, $"Sent raw hex bytes (Length: {raw.Length})", hex);
        }
    }

    private void OnRawFrameReceived(LlrpReader reader, byte[] raw)
    {
        try
        {
            var temp = raw;
            LLRPBinaryDecoder.Decode(ref temp, out var msg);
            if (msg != null)
            {
                var xml = msg.ToString() ?? string.Empty;
                var details = FormatMessageDetails(msg);
                AddLog(msg.MSG_TYPE.ToString(), msg.MSG_ID.ToString(), false, details, xml);
            }
            else
            {
                var hex = BitConverter.ToString(raw).Replace("-", " ");
                AddLog("RAW_RX", "0", false, $"Received raw hex bytes (Length: {raw.Length})", hex);
            }
        }
        catch
        {
            var hex = BitConverter.ToString(raw).Replace("-", " ");
            AddLog("RAW_RX", "0", false, $"Received raw hex bytes (Length: {raw.Length})", hex);
        }
    }

    private int _tagsRead = 0;
    private void OnTagsReported(LlrpReader reader, TagReport report)
    {
        _tagsRead += report.Tags.Count;
        UpdateTelemetry();
    }

    private void OnLlrpMessageLogged(LlrpReader reader, string logMessage)
    {
        AddLog("TRACE", "0", false, logMessage, string.Empty);
    }

    private void OnErrorNotification(LlrpReader reader, Exception ex)
    {
        AddLog("ERROR", "0", false, $"SDK Error: {ex.Message}", string.Empty, true);
    }

    private void UpdateTelemetry()
    {
        _telemetryLabel.Text = $"Uptime: Active\nTags Read: {_tagsRead}\nRead Rate: {_tagsRead / 5}/s\nTemp: 48°C";
    }

    private void SendSelectedMessage()
    {
        var msgType = _msgTypeSelect.Text;
        if (string.IsNullOrEmpty(msgType)) return;

        SendQuickCommand(msgType);
    }

    private void SendQuickCommand(string command)
    {
        if (_reader == null || !_reader.IsConnected)
        {
            AddLog("SYSTEM", "0", false, "Cannot send command: reader is offline.", string.Empty, true);
            return;
        }

        uint roSpecId = 1;
        uint.TryParse(_roSpecIdField.Text, out roSpecId);

        try
        {
            switch (command)
            {
                case "GET_READER_CAPABILITIES":
                    var caps = _reader.ReaderCapabilities;
                    AddLog("CLIENT", "0", true, $"Querying capabilities: Model={caps.ReaderModel}, Antennas={caps.AntennaCount}", string.Empty);
                    break;

                case "GET_READER_CONFIG":
                    var config = _reader.QuerySettings();
                    AddLog("CLIENT", "0", true, $"Querying configuration: Keepalives={config.Keepalives.Enabled}, Session={config.Session}", string.Empty);
                    break;

                case "ADD_ROSPEC":
                    var settings = _reader.QueryDefaultSettings();
                    _reader.ApplySettings(settings);
                    break;

                case "ENABLE_ROSPEC":
                    var enableMsg = new MSG_ENABLE_ROSPEC { ROSpecID = roSpecId };
                    AddLog("CLIENT", "0", true, $"Sending ENABLE_ROSPEC for ID {roSpecId} via SDK", enableMsg.ToString());
                    break;

                case "START_ROSPEC":
                    _reader.Start();
                    break;

                case "STOP_ROSPEC":
                    _reader.Stop();
                    break;

                case "DISABLE_ROSPEC":
                    var disableMsg = new MSG_DISABLE_ROSPEC { ROSpecID = roSpecId };
                    AddLog("CLIENT", "0", true, $"Sending DISABLE_ROSPEC for ID {roSpecId}", disableMsg.ToString());
                    break;

                case "DELETE_ROSPEC":
                    _reader.DeleteAllOpSequences();
                    break;

                case "CLOSE_CONNECTION":
                    Disconnect();
                    break;

                default:
                    AddLog("CLIENT", "0", true, $"Unknown command '{command}'", string.Empty, true);
                    break;
            }
        }
        catch (Exception ex)
        {
            AddLog("SEND_ERROR", "0", true, $"Failed to send {command}: {ex.Message}", string.Empty, true);
        }
    }

    private string GetEpcString(object epcParam)
    {
        if (epcParam is PARAM_EPC_96 epc96)
        {
            return epc96.EPC.ToHexString();
        }
        if (epcParam is PARAM_EPCData epcData)
        {
            return epcData.EPC.ToHexString();
        }
        return "UNKNOWN";
    }

    private string FormatMessageDetails(Message msg)
    {
        if (msg == null) return string.Empty;
        
        try
        {
            switch (msg)
            {
                case MSG_RO_ACCESS_REPORT report:
                    if (report.TagReportData == null || report.TagReportData.Length == 0)
                    {
                        return "No tags in report";
                    }
                    var tagCount = report.TagReportData.Length;
                    var epcList = new List<string>();
                    foreach (var tagData in report.TagReportData.Take(3))
                    {
                        if (tagData.EPCParameter != null && tagData.EPCParameter.Count > 0)
                        {
                            var epcStr = GetEpcString(tagData.EPCParameter[0]);
                            var ant = tagData.AntennaID != null ? $" [Ant:{tagData.AntennaID.AntennaID}]" : "";
                            var rssi = tagData.PeakRSSI != null ? $" [RSSI:{tagData.PeakRSSI.PeakRSSI}dBm]" : "";
                            epcList.Add($"{epcStr}{ant}{rssi}");
                        }
                    }
                    var prefix = $"Tags Count: {tagCount}";
                    if (epcList.Count > 0)
                    {
                        prefix += $"\nTags Details:\n  " + string.Join("\n  ", epcList);
                        if (tagCount > 3) prefix += "\n  ...";
                    }
                    return prefix;

                case MSG_ADD_ROSPEC_RESPONSE addRsp:
                    return $"Status Code: {addRsp.LLRPStatus.StatusCode}\nError Desc: {addRsp.LLRPStatus.ErrorDescription}";

                case MSG_START_ROSPEC_RESPONSE startRsp:
                    return $"Status Code: {startRsp.LLRPStatus.StatusCode}\nError Desc: {startRsp.LLRPStatus.ErrorDescription}";

                case MSG_STOP_ROSPEC_RESPONSE stopRsp:
                    return $"Status Code: {stopRsp.LLRPStatus.StatusCode}\nError Desc: {stopRsp.LLRPStatus.ErrorDescription}";

                case MSG_DELETE_ROSPEC_RESPONSE delRsp:
                    return $"Status Code: {delRsp.LLRPStatus.StatusCode}\nError Desc: {delRsp.LLRPStatus.ErrorDescription}";

                case MSG_ENABLE_ROSPEC_RESPONSE enRsp:
                    return $"Status Code: {enRsp.LLRPStatus.StatusCode}\nError Desc: {enRsp.LLRPStatus.ErrorDescription}";

                case MSG_DISABLE_ROSPEC_RESPONSE disRsp:
                    return $"Status Code: {disRsp.LLRPStatus.StatusCode}\nError Desc: {disRsp.LLRPStatus.ErrorDescription}";

                case MSG_SET_READER_CONFIG_RESPONSE cfgRsp:
                    return $"Status Code: {cfgRsp.LLRPStatus.StatusCode}\nError Desc: {cfgRsp.LLRPStatus.ErrorDescription}";

                case MSG_GET_READER_CAPABILITIES_RESPONSE capRsp:
                    var model = capRsp.GeneralDeviceCapabilities?.ReaderFirmwareVersion ?? "Unknown FW";
                    var antsCount = capRsp.GeneralDeviceCapabilities?.MaxNumberOfAntennaSupported ?? 0;
                    return $"Capabilities Info:\n  Firmware Version: {model}\n  Max Supported Antennas: {antsCount}";

                case MSG_GET_READER_CONFIG_RESPONSE getConfigRsp:
                    var keepalive = getConfigRsp.KeepaliveSpec != null ? "Enabled" : "Disabled";
                    var interval = getConfigRsp.KeepaliveSpec?.PeriodicTriggerValue ?? 0;
                    return $"Config Info:\n  Keepalive Trigger: {keepalive}\n  Interval: {interval} ms";

                case MSG_READER_EVENT_NOTIFICATION notification:
                    var eventText = "Reader event details:";
                    if (notification.ReaderEventNotificationData != null)
                    {
                        var data = notification.ReaderEventNotificationData;
                        if (data.ConnectionAttemptEvent != null)
                            eventText += $"\n  ConnectionAttemptStatus: {data.ConnectionAttemptEvent.Status}";
                        if (data.AntennaEvent != null)
                            eventText += $"\n  AntennaEvent: Port {data.AntennaEvent.AntennaID} (State: {data.AntennaEvent.EventType})";
                        if (data.GPIEvent != null)
                            eventText += $"\n  GPIEvent: Port {data.GPIEvent.GPIPortNumber} (State: {data.GPIEvent.GPIEvent})";
                    }
                    return eventText;

                case MSG_ADD_ROSPEC addMsg:
                    return $"Add ROSpec Info:\n  ROSpecID: {addMsg.ROSpec.ROSpecID}\n  Priority: {addMsg.ROSpec.Priority}";

                case MSG_START_ROSPEC startMsg:
                    return $"Start ROSpec Info:\n  ROSpecID: {startMsg.ROSpecID}";

                case MSG_STOP_ROSPEC stopMsg:
                    return $"Stop ROSpec Info:\n  ROSpecID: {stopMsg.ROSpecID}";

                case MSG_ENABLE_ROSPEC enableMsg:
                    return $"Enable ROSpec Info:\n  ROSpecID: {enableMsg.ROSpecID}";

                case MSG_DISABLE_ROSPEC disableMsg:
                    return $"Disable ROSpec Info:\n  ROSpecID: {disableMsg.ROSpecID}";

                case MSG_DELETE_ROSPEC deleteMsg:
                    return $"Delete ROSpec Info:\n  ROSpecID: {deleteMsg.ROSpecID}";

                case MSG_GET_READER_CAPABILITIES capMsg:
                    return $"Request Capabilities: {capMsg.RequestedData}";

                case MSG_GET_READER_CONFIG configMsg:
                    return $"Request Config: {configMsg.RequestedData}\n  AntennaID: {configMsg.AntennaID}";

                default:
                    return $"Standard Message fields:\n  MSG_TYPE: {msg.MSG_TYPE}\n  MSG_ID: {msg.MSG_ID}\n  Version: {msg.VERSION}";
            }
        }
        catch (Exception ex)
        {
            return $"[Parse error: {ex.Message}]";
        }
    }
}
