using Org.LLRP.LTK.LLRPV1;
using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Timers;

#nullable disable
namespace LLRPSdk
{
    // https://www.iana.org/assignments/enterprise-numbers/
    public enum ManufacturerNumber
    {
        Unknown = 0,
        Impinj = 25882,
        Motorola=161,
        Zebra= 10642,
        Silion= 47706,
        Seuic=57690,
    }





    /// <summary>
    /// The main class for controlling and configuring an Llrp RFID reader.
    /// </summary>
    public class LlrpReader
    {
        public class ReaderEventNotificationState
        {
            public ENUM_NotificationEventType EventType { get; set; }

            public bool IsEnabled { get; set; }
        }

        private static readonly int RO_SPEC_ID = 14150;
        private static readonly int RO_SPEC_INDEX = 0;
        private static readonly uint ATTACHED_DATA_ACCESS_SPEC_ID = 65534U; // 特殊 ID 用于附加数据 AO
        private static readonly int DEFAULT_TIMEOUT_MS = 5000;
        private static readonly int DEFAULT_LLRP_PORT_UNSECURED = 5084;
        private static readonly int DEFAULT_LLRP_PORT_ENCRYPTED = 5085;
        private const ushort UNIVERSAL_ANTENNA_ID = 0;
        internal LLRPClient reader;
        //internal Org.LLRP.LTK.LLRPV1.LLRPClient_OverTCPServer reader;
        
        private FeatureSet _readerCapabilities;
        private SyncProgramOpStatus syncProgramOpStatus;
        private SyncReadOpStatus syncReadOpStatus;
        private BackgroundWorker backgroundWorkerThread;
        private System.Timers.Timer keepaliveTimer = new System.Timers.Timer();
        private ConcurrentDictionary<int, TagData> outstandingAuthenticateRequests = new ConcurrentDictionary<int, TagData>();
        private List<ReaderEventNotificationState> cachedReaderEventNotifications;

        /// <summary>
        /// Returns a copy of the reader capabilities available after a successful connection.
        /// To refresh capabilites after a connection is made, use the QueryFeatureSet method.
        /// A successful connection is mandotory before calling the property.
        /// </summary>
        public FeatureSet ReaderCapabilities
        {
            get
            {
                return this._readerCapabilities != null ? new FeatureSet(this._readerCapabilities) : throw new LLRPSdkException("You must connect to the reader before getting the capabilities.");
            }
        }

        /// <summary>
        /// Event to provide notification when a tag report is available.
        /// </summary>
        public event LlrpReader.TagsReportedHandler TagsReported;

        /// <summary>
        /// Event raised when a raw LLRP frame is received.
        /// </summary>
        public event LlrpReader.RawFrameReceivedHandler RawFrameReceived;

        /// <summary>
        /// Event raised when a raw LLRP frame is sent.
        /// </summary>
        public event LlrpReader.RawFrameSentHandler RawFrameSent;

        /// <summary>
        /// Event raised for high-level LLRP message traces.
        /// </summary>
        public event LlrpReader.LlrpMessageLoggedHandler LlrpMessageLogged;

        /// <summary>
        /// Whether LLRP message logging should include Message.ToString() detailed payload.
        /// </summary>
        public bool LlrpMessageLogAsXml { get; set; }

        /// <summary>
        /// Event to provide notification when a GPI port status changes.
        /// </summary>
        public event LlrpReader.GpiChangedHandler GpiChanged;

        /// <summary>
        /// Event to provide notification when an AntennaChanged event
        /// occurs.
        /// </summary>
        public event LlrpReader.AntennaEventHandler AntennaChanged;

        /// <summary>
        /// Event to provide notification when an AntennaStarted event
        /// occurs.
        /// </summary>
        public event LlrpReader.AntennaStartEventHandler AntennaStarted;

        /// <summary>
        /// Event to provide notification when an End Of Cycle event
        /// occurs.
        /// </summary>
        public event LlrpReader.EndOfCycleEventHandler EndOfCycle;

        /// <summary>
        /// Event to provide notification when the report buffer on the reader is nearly full.
        /// </summary>
        public event LlrpReader.ReportBufferWarningEventHandler ReportBufferWarning;

        /// <summary>
        /// Event to provide notification when the report buffer on the reader
        /// could not hold more tag reports.
        /// </summary>
        public event LlrpReader.ReportBufferOverflowEventHandler ReportBufferOverflow;

        /// <summary>
        /// Event to provide notification when the reader has started.
        /// </summary>
        public event LlrpReader.ReaderStartedEventHandler ReaderStarted;

        /// <summary>
        /// Event to provide notification when the reader has started.
        /// </summary>
        public event LlrpReader.ReaderStoppedEventHandler ReaderStopped;

        /// <summary>
        /// Event to provide notification that a tag operation has completed,
        /// including the results of the operation.
        /// </summary>
        public event LlrpReader.TagOpCompleteHandler TagOpComplete;

        /// <summary>
        /// Event to provide notification that a keep alive TCP/IP packet
        /// was received from the reader.
        /// </summary>
        public event LlrpReader.KeepaliveHandler KeepaliveReceived;

        /// <summary>
        /// Event to provide notification that the TCP/IP connection to the
        /// LLRP Reader has been lost.
        /// </summary>
        public event LlrpReader.ConnectionLostHandler ConnectionLost;

        /// <summary>
        /// Event to provide notification of a completed asynchronous connection attempt.
        /// </summary>
        public event LlrpReader.ConnectAsyncCompleteHandler ConnectAsyncComplete;



        /// <summary>
        /// Event to provide notification of a diagnostic report. Only available on the xArray.
        /// </summary>
        public event LlrpReader.DiagnosticsReportedHandler DiagnosticsReported;

        /// <summary>
        /// Event to provide notification that an unhandled exception error has occurred. Needs to cleanup, close this instance.
        /// </summary>
        public event LlrpReader.ErrorNotificationHandler ErrorNotification;


        ///<summary>Handle to the build in debug logging handler.</summary>
        public LlrpReaderDebug Debug { get; private set; }



        /// <summary>Creates and initializes an LLRPReader object.</summary>
        public LlrpReader(string address, string name)
        {
            this.Address = address;
            this.Name = name;
            this.InitVars();
        }

        /// <summary>Creates and initializes an LlrpReader object.</summary>
        public LlrpReader() => this.InitVars();

        /// <summary>Destroys an LLRPReader object and frees resources.</summary>
        ~LlrpReader()
        {
            if (this.reader == null)
                return;
            this.reader.Dispose();
        }

        private void InitVars()
        {
            this.MessageTimeout = LlrpReader.DEFAULT_TIMEOUT_MS;
            this.ConnectTimeout = LlrpReader.DEFAULT_TIMEOUT_MS;
            this.Debug = new LlrpReaderDebug(this);

            this.keepaliveTimer.Elapsed += new ElapsedEventHandler(this.OnKeepaliveMissed);
        }

   
        [Obsolete("This property is no longer supported.", true)]
        public int LogLevel { get; set; }


        ///<summary>
        /// The connection timeout in milliseconds.
        /// If a connection to the reader cannot be established in this time, an exception is thrown.
        /// </summary>
        public int ConnectTimeout { get; set; }

        /// <summary>
        /// [Deprecated] Number of times to attempt a reader connection before an exception is thrown.
        /// </summary>
        [Obsolete("This property is no longer supported.", true)]
        public int MaxConnectionAttempts { get; set; }

        /// <summary>The message reply timeout.</summary>
        public int MessageTimeout { get; set; }

        /// <summary>Closes the connection to the reader.</summary>
        public void Disconnect()
        {
            if (!this.IsConnected)
                return;
            this.keepaliveTimer.Stop();
            this.cachedReaderEventNotifications = null;
            this.reader.Close();
            this.reader.OnRoAccessReportReceived -= new delegateRoAccessReport(this.OnTagReportAvailableInternal);
            this.reader.OnReaderEventNotification -= new delegateReaderEventNotification(this.OnReaderEventInternal);
            this.reader.OnKeepAlive -= new delegateKeepAlive(this.OnKeepAliveInternal);
            this.reader.OnRawReceived -= new delegateRawFrame(this.OnRawFrameReceivedInternal);
            this.reader.OnRawSent -= new delegateRawFrame(this.OnRawFrameSentInternal);
        }

        /// <summary>Stops the reader. Tags will no longer be read.</summary>
        public void Stop()
        {
            if (!this.IsConnected)
                return;
            this.DisableRoSpec();
        }

        /// <summary>
        /// Starts the reader. Tag reports will be received asynchronously via an event.
        /// </summary>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">
        /// Thrown if the Start method is called before connecting to a reader.
        /// </exception>
        public void Start()
        {
            if (!this.IsConnected)
                throw new LLRPSdkException("You must connect to the reader before starting it.");

            try
            {
                this.DisableRoSpec();//add to fix issue where reader doesn't start after Stop is called
            }
            catch (LLRPSdkException)
            {
            }

            LLRPSdkException lastError = null;
            for (int attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    this.EnsureFirstRoSpecInactiveBeforeStart();
                    this.StartRoSpec();
                    this.EnsureFirstRoSpecActiveAfterStart();
                    return;
                }
                catch (LLRPSdkException ex)
                {
                    lastError = ex;
                    bool isDisabledStateError = ex.Message.Contains("START_ROSPEC", StringComparison.OrdinalIgnoreCase)
                        && ex.Message.Contains("disabled", StringComparison.OrdinalIgnoreCase);
                    if (!isDisabledStateError)
                        throw;

                    Thread.Sleep(120);
                }
            }

            throw lastError ?? new LLRPSdkException("Failed to start ROSpec.");
        }

        private void EnsureFirstRoSpecInactiveBeforeStart()
        {
            if (this.reader == null)
                throw new LLRPSdkException("You must connect to the reader before validating rospec state.");

            for (int attempt = 0; attempt < 6; attempt++)
            {
                PARAM_ROSpec[] roSpecs = this.GetRoSpecs().ROSpec;
                if (roSpecs == null || roSpecs.Length == 0 || roSpecs[0] == null)
                    throw new LLRPSdkException("No ROSpec found on reader.");

                PARAM_ROSpec firstRoSpec = roSpecs[0];
                if (firstRoSpec.CurrentState == ENUM_ROSpecState.Inactive)
                    return;

                if (firstRoSpec.CurrentState == ENUM_ROSpecState.Active)
                {
                    this.StopRoSpec();
                    Thread.Sleep(80);
                    continue;
                }

                this.EnableRoSpec();
                Thread.Sleep(80);
            }

            PARAM_ROSpec[] finalSpecs = this.GetRoSpecs().ROSpec;
            string finalState = finalSpecs != null && finalSpecs.Length > 0 && finalSpecs[0] != null
                ? finalSpecs[0].CurrentState.ToString()
                : "Unknown";
            throw new LLRPSdkException($"First ROSpec is not Inactive before START_ROSPEC. CurrentState={finalState}");
        }

        private void EnsureFirstRoSpecActiveAfterStart()
        {
            if (this.reader == null)
                throw new LLRPSdkException("You must connect to the reader before checking rospec state.");

            for (int attempt = 0; attempt < 8; attempt++)
            {
                PARAM_ROSpec[] roSpecs = this.GetRoSpecs().ROSpec;
                if (roSpecs == null || roSpecs.Length == 0 || roSpecs[0] == null)
                    throw new LLRPSdkException("No ROSpec found on reader after START_ROSPEC.");

                if (roSpecs[0].CurrentState == ENUM_ROSpecState.Active)
                    return;

                Thread.Sleep(80);
            }

            PARAM_ROSpec[] finalSpecs = this.GetRoSpecs().ROSpec;
            string finalState = finalSpecs != null && finalSpecs.Length > 0 && finalSpecs[0] != null
                ? finalSpecs[0].CurrentState.ToString()
                : "Unknown";
            throw new LLRPSdkException($"START_ROSPEC returned but first ROSpec is not Active. CurrentState={finalState}");
        }

        /// <summary>
        /// Connect to an %LLRP RFID reader asynchronously.
        /// An event will be raised when the connection attempt succeeds or fails.
        /// The Address property must be set prior to calling this method.
        /// </summary>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">
        /// Thrown when the connection attempt fails or if the Address
        /// property has not been set.
        /// </exception>
        public void ConnectAsync()
        {
            if (this.Address == null || this.Address.Trim() == "")
                throw new LLRPSdkException("The Address property must be set before calling Connect.");
            this.ConnectAsync(this.Address, LlrpReader.DEFAULT_LLRP_PORT_UNSECURED, false);
        }

        /// <summary>
        /// Connect to an %LLRP RFID reader asynchronously.
        /// An event will be raised when the connection attempt succeeds or fails.
        /// </summary>
        /// <param name="address">IP address or hostname of the target reader.</param>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">
        /// Thrown when the connection attempt fails.
        /// </exception>
        public void ConnectAsync(string address)
        {
            this.ConnectAsync(address, LlrpReader.DEFAULT_LLRP_PORT_UNSECURED, false);
        }

        /// <summary>
        /// Connect to an %LLRP RFID reader asynchronously.
        /// An event will be raised when the connection attempt succeeds or fails.
        /// </summary>
        /// <param name="address">IP address or hostname of the target reader.</param>
        /// <param name="port">TCP/IP port number used by the target reader.</param>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">
        /// Thrown when the connection attempt fails.
        /// </exception>
        public void ConnectAsync(string address, int port) => this.ConnectAsync(address, port, false);

        /// <summary>
        /// Connect to an %LLRP RFID reader asynchronously.
        /// An event will be raised when the connection attempt succeeds or fails.
        /// </summary>
        /// <param name="address">IP address or hostname of the target reader.</param>
        /// <param name="useTLS">Enable TLS encryption if "true".</param>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">
        /// Thrown when the connection attempt fails.
        /// </exception>
        public void ConnectAsync(string address, bool useTLS)
        {
            int port = useTLS ? LlrpReader.DEFAULT_LLRP_PORT_ENCRYPTED : LlrpReader.DEFAULT_LLRP_PORT_UNSECURED;
            this.ConnectAsync(address, port, useTLS);
        }

        /// <summary>
        /// Connect to an %LLRP RFID reader asynchronously.
        /// An event will be raised when the connection attempt succeeds or fails.
        /// </summary>
        /// <param name="address">IP address or hostname of the target reader.</param>
        /// <param name="port">TCP/IP port number used by the target reader.</param>
        /// <param name="useTLS">Enable TLS encryption if "true".</param>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">
        /// Thrown when the connection attempt fails.
        /// </exception>
        public void ConnectAsync(string address, int port, bool useTLS)
        {
            this.ConnectAsync(address, port, useTLS, TlsProtocols.OsDefault);
        }

        /// <summary>
        /// Connect to an %LLRP RFID reader asynchronously.
        /// An event will be raised when the connection attempt succeeds or fails.
        /// </summary>
        /// <param name="address">IP address or hostname of the target reader.</param>
        /// <param name="port">TCP/IP port number used by the target reader.</param>
        /// <param name="useTLS">Enable TLS encryption if "true".</param>
        /// <param name="tlsProtocol">Tls protocol to use - OsDefault, Tls12, Tls13</param>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">
        /// Thrown when the connection attempt fails.
        /// </exception>
        public void ConnectAsync(string address, int port, bool useTLS, TlsProtocols tlsProtocol)
        {
            if (this.backgroundWorkerThread != null)
                throw new LLRPSdkException("Asynchronous connection attempt already in progress for reader " + address);
            AsyncConnectInfo asyncConnectInfo = new AsyncConnectInfo()
            {
                Address = address,
                Port = port,
                UseTLS = useTLS,
                TlsProtocol = tlsProtocol
            };
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            this.backgroundWorkerThread = backgroundWorker;
            backgroundWorker.DoWork += new DoWorkEventHandler(this.ConnectAsyncWorkerDoWork);
            backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.ConnectAsyncWorkerCompleted);
            backgroundWorker.RunWorkerAsync((object)asyncConnectInfo);
        }

        private void ConnectAsyncWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            AsyncConnectInfo asyncConnectInfo = e.Argument as AsyncConnectInfo;
            try
            {
                this.Connect(asyncConnectInfo.Address, asyncConnectInfo.Port, asyncConnectInfo.UseTLS);
            }
            catch (LLRPSdkException ex)
            {
                if (this.ConnectAsyncComplete == null)
                    return;
                e.Result = (object)ex.Message;
            }
        }

        private void ConnectAsyncWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.backgroundWorkerThread = (BackgroundWorker)null;
            if (this.ConnectAsyncComplete == null)
                return;
            if (this.IsConnected)
                this.ConnectAsyncComplete(this, ConnectAsyncResult.Success, "");
            else
                this.ConnectAsyncComplete(this, ConnectAsyncResult.Failure, e.Result.ToString());
        }

        /// <summary>
        /// Connect to an %LLRP RFID reader.
        /// The Address property must be set prior to calling this method.
        /// </summary>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">
        /// Thrown when the connection attempt fails or if the Address
        /// property has not been set.
        /// </exception>
        public void Connect()
        {
            if (this.Address == null || this.Address.Trim() == "")
                throw new LLRPSdkException("The Address property must be set before calling Connect.");
            this.Connect(this.Address, LlrpReader.DEFAULT_LLRP_PORT_UNSECURED);
        }

        /// <summary>Connect to an %LLRP RFID reader.</summary>
        /// <param name="address">IP address or hostname of the target reader.</param>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">
        /// Thrown when the connection attempt fails.
        /// </exception>
        public void Connect(string address)
        {
            this.Connect(address, LlrpReader.DEFAULT_LLRP_PORT_UNSECURED, false);
        }

        /// <summary>Connect to an %LLRP RFID reader.</summary>
        /// <param name="address">IP address or hostname of the target reader.</param>
        /// <param name="port">The port with which to connect.</param>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">
        /// Thrown when the connection attempt fails.
        /// </exception>
        public void Connect(string address, int port) => this.Connect(address, port, false);

        /// <summary>Connect to an %LLRP RFID reader.</summary>
        /// <param name="address">IP address or hostname of the target reader.</param>
        /// <param name="useTLS">Enable TLS encryption if "true".</param>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">
        /// Thrown when the connection attempt fails.
        /// </exception>
        public void Connect(string address, bool useTLS)
        {
            int port = useTLS ? LlrpReader.DEFAULT_LLRP_PORT_ENCRYPTED : LlrpReader.DEFAULT_LLRP_PORT_UNSECURED;
            this.Connect(address, port, useTLS, TlsProtocols.OsDefault);
        }

        /// <summary>Connect to an %LLRP RFID reader.</summary>
        /// <param name="address">IP address or hostname of the target reader.</param>
        /// <param name="useTLS">Enable TLS encryption if "true".</param>
        /// <param name="tlsProtocol">TLS Protocol to use OsDefault, Tls12, Tls13</param>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">
        /// Thrown when the connection attempt fails.
        /// </exception>
        public void Connect(string address, bool useTLS, TlsProtocols tlsProtocol)
        {
            int port = useTLS ? LlrpReader.DEFAULT_LLRP_PORT_ENCRYPTED : LlrpReader.DEFAULT_LLRP_PORT_UNSECURED;
            this.Connect(address, port, useTLS, tlsProtocol);
        }

        /// <summary>Connect to an %LLRP RFID reader.</summary>
        /// <param name="address">IP address or hostname of the target reader.</param>
        /// <param name="port">TCP/IP port number used by the target reader.</param>
        /// <param name="useTLS">Enable TLS encryption if "true".</param>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">
        /// Thrown when the connection attempt fails.
        /// </exception>
        public void Connect(string address, int port, bool useTLS)
        {
            this.Connect(address, port, useTLS, TlsProtocols.OsDefault);
        }

        /// <summary>Connect to an %LLRP RFID reader.</summary>
        /// <param name="address">IP address or hostname of the target reader.</param>
        /// <param name="port">TCP/IP port number used by the target reader.</param>
        /// <param name="useTLS">Enable TLS encryption if "true".</param>
        /// <param name="tlsProtocol">TLS protocol to use OsDefault, Tls12, Tls13</param>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">
        /// Thrown when the connection attempt fails.
        /// </exception>
        public void Connect(string address, int port, bool useTLS, TlsProtocols tlsProtocol)
        {
            if (this.reader != null)
                this.reader.Dispose();
            this.reader = new LLRPClient(port);
            //this.reader = new LLRPClient_OverTCPServer(port);
            this.reader.OnRoAccessReportReceived += new delegateRoAccessReport(this.OnTagReportAvailableInternal);
            this.reader.OnReaderEventNotification += new delegateReaderEventNotification(this.OnReaderEventInternal);
            this.reader.OnKeepAlive += new delegateKeepAlive(this.OnKeepAliveInternal);
            this.reader.OnErrorNotification += new delegateErrorNotification(this.OnErrorNotification);
            this.reader.OnRawReceived += new delegateRawFrame(this.OnRawFrameReceivedInternal);
            this.reader.OnRawSent += new delegateRawFrame(this.OnRawFrameSentInternal);
            ENUM_ConnectionAttemptStatusType status;
            this.reader.Open(address, this.ConnectTimeout, useTLS, LlrpReader.ToLLRP(tlsProtocol), out status);
            string str = "Error connecting to the reader (" + address + ") : ";
            if (status == ENUM_ConnectionAttemptStatusType.Success)
            {
                this.Address = address;

                this.QueryFeatureSet();
                if (this._readerCapabilities.ModelNumber == 1000000U)//Todo
                {
                    this.Disconnect();
                    throw new LLRPSdkException("The Octane SDK does not support the Speedway R1000 reader.");
                }
            }
            else
            {
                this.reader.Dispose();
                this.reader = null;
                string message;
                switch (status - 1)
                {
                    case ENUM_ConnectionAttemptStatusType.Success:
                        message = str + "A reader initiated connection already exists.";
                        break;
                    case ENUM_ConnectionAttemptStatusType.Failed_A_Reader_Initiated_Connection_Already_Exists:
                        message = str + "A client initiated connection already exists.";
                        break;
                    case ENUM_ConnectionAttemptStatusType.Failed_A_Client_Initiated_Connection_Already_Exists:
                        message = str + "Reason other than an existing connection.";
                        break;
                    case ENUM_ConnectionAttemptStatusType.Failed_Reason_Other_Than_A_Connection_Already_Exists:
                        message = str + "Another connection attempted.";
                        break;
                    default:
                        message = str + "Timeout.";
                        break;
                }
                throw new LLRPSdkException(message);
            }
        }

        private void OnErrorNotification(string reader, Exception error)
        {
            if (!this.IsConnected)
                return;
            LlrpReader.ErrorNotificationHandler errorNotification = this.ErrorNotification;
            if (errorNotification == null)
                return;
            errorNotification(this, error);
        }

        private void OnRawFrameReceivedInternal(byte[] raw)
        {
            LlrpReader.RawFrameReceivedHandler rawFrameReceived = this.RawFrameReceived;
            if (rawFrameReceived != null)
                rawFrameReceived(this, raw);
        }

        private void OnRawFrameSentInternal(byte[] raw)
        {
            LlrpReader.RawFrameSentHandler rawFrameSent = this.RawFrameSent;
            if (rawFrameSent != null)
                rawFrameSent(this, raw);
        }

        private void LogLlrpMessage(string message)
        {
            LlrpReader.LlrpMessageLoggedHandler llrpMessageLogged = this.LlrpMessageLogged;
            if (llrpMessageLogged == null)
                return;
            llrpMessageLogged(this, message);
        }

        /// <summary>
        /// 通用的 Transaction 日志记录辅助方法：在调用前后记录 XML
        /// </summary>
        private void LogTransaction(Message request, Message response, MSG_ERROR_MESSAGE error, string operationName, bool? isxml = null)
        {
            bool includeXml = isxml ?? this.LlrpMessageLogAsXml;

            // 记录发送的请求
            if (request != null)
            {
                string content = includeXml ? request.ToString() : string.Empty;
                this.LogLlrpMessage($"TX {operationName} (MsgId={request.MSG_ID})\n{content}");
            }

            // 记录收到的响应
            if (response != null)
            {
                string content = includeXml ? response.ToString() : string.Empty;
                this.LogLlrpMessage($"RX {operationName}_RESPONSE (MsgId={response.MSG_ID})\n{content}");
            }
            
            // 记录错误消息
            if (error != null)
                this.LogLlrpMessage($"RX ERROR_MESSAGE (MsgId={error.MSG_ID})\n{error.ToString()}");
        }

        /// <summary>Converts to the LLRP TlsProtocols</summary>
        /// <param name="tlsProtocol"></param>
        /// <returns></returns>
        private static LLRPClient.TlsProtocols ToLLRP(TlsProtocols tlsProtocol)
        {
            LLRPClient.TlsProtocols llrp = LLRPClient.TlsProtocols.OsDefault;
            switch (tlsProtocol)
            {
                case TlsProtocols.OsDefault:
                    llrp = LLRPClient.TlsProtocols.OsDefault;
                    break;
                case TlsProtocols.Tls12:
                    llrp = LLRPClient.TlsProtocols.Tls12;
                    break;
                case TlsProtocols.Tls13:
                    llrp = LLRPClient.TlsProtocols.Tls13;
                    break;
            }
            return llrp;
        }

        /// <summary>
        /// Indicates whether or not a connection to the reader exists.
        /// </summary>
        public bool IsConnected => this.reader != null && this.reader.IsConnected;

        /// <summary>Applies the provided settings to the reader.</summary>
        /// <param name="settings">Settings to apply to the reader</param>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">
        /// Thrown when this method is called prior to establishing a
        /// connection with a reader, or if the settings are invalid.
        /// </exception>
        public void ApplySettings(Settings settings)
        {
            this.ApplySettings(this.BuildSetReaderConfigMessage(settings), this.BuildAddROSpecMessage(settings), settings);
        }

        /// <summary>
        /// Applies the provided settings to the reader, without performing a factory reset.
        /// </summary>
        /// <param name="settings">Settings to apply to the reader</param>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">
        /// Thrown when this method is called prior to establishing a
        /// connection with a reader, or if the settings are invalid.
        /// </exception>
        public void ApplySettingsWithoutFactoryReset(Settings settings)
        {
            this.ApplySettingsWithoutFactoryReset(this.BuildSetReaderConfigMessage(settings), this.BuildAddROSpecMessage(settings), settings);
        }

        /// <summary>Applies the provided settings to the reader.</summary>
        /// <param name="setReaderConfigMessage">The set reader config LLRP LTK message to be sent</param>
        /// <param name="addRoSpecMessage">The add RO Spec LLRP LTK message to be sent</param>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">
        /// Thrown when this method is called prior to establishing a
        /// connection with a reader, or if the settings are invalid.
        /// </exception>
        public void ApplySettings(
          MSG_SET_READER_CONFIG setReaderConfigMessage,
          MSG_ADD_ROSPEC addRoSpecMessage)
        {
                        this.ApplySettingsWithOptionalFactoryReset(setReaderConfigMessage, addRoSpecMessage, true, (Settings)null);
                }

                public void ApplySettings(
                    MSG_SET_READER_CONFIG setReaderConfigMessage,
                    MSG_ADD_ROSPEC addRoSpecMessage,
                    Settings settings)
                {
                        this.ApplySettingsWithOptionalFactoryReset(setReaderConfigMessage, addRoSpecMessage, true, settings);
        }

        /// <summary>
        /// Applies the provided settings to the reader, without performing a factory reset.
        /// </summary>
        /// <param name="setReaderConfigMessage">The set reader config LLRP LTK message to be sent</param>
        /// <param name="addRoSpecMessage">The add RO Spec LLRP LTK message to be sent</param>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">
        /// Thrown when this method is called prior to establishing a
        /// connection with a reader, or if the settings are invalid.
        /// </exception>
        public void ApplySettingsWithoutFactoryReset(
          MSG_SET_READER_CONFIG setReaderConfigMessage,
          MSG_ADD_ROSPEC addRoSpecMessage)
        {
                        this.ApplySettingsWithOptionalFactoryReset(setReaderConfigMessage, addRoSpecMessage, false, (Settings)null);
                }

                public void ApplySettingsWithoutFactoryReset(
                    MSG_SET_READER_CONFIG setReaderConfigMessage,
                    MSG_ADD_ROSPEC addRoSpecMessage,
                    Settings settings)
                {
                        this.ApplySettingsWithOptionalFactoryReset(setReaderConfigMessage, addRoSpecMessage, false, settings);
        }

        private void ApplySettingsWithOptionalFactoryReset(
          MSG_SET_READER_CONFIG setReaderConfigMessage,
          MSG_ADD_ROSPEC addRoSpecMessage,
                    bool resetToFactorySettings,
                    Settings settings)
        {
            if (!this.IsConnected)
                throw new LLRPSdkException("You must connect to the reader before configuring it.");
            if (resetToFactorySettings)
                this.ResetToFactoryDefaults();
            this.outstandingAuthenticateRequests.Clear();
            this.DeleteRoSpecs();
            this.DeleteAccessSpecs();
            this.SetReaderConfig(setReaderConfigMessage);
            this.AddRoSpec(addRoSpecMessage);
            this.AddAttachedDataReadOpIfNeeded(settings);
            this.EnableRoSpec();
        }

        private void AddAttachedDataReadOpIfNeeded(Settings settings)
        {
            if (settings?.AttachedData == null || !settings.AttachedData.Enabled)
                return;
            AttachedDataConfig attachedData = settings.AttachedData;
            if (attachedData.WordCount == 0)
                return;
            TagOpSequence sequence = new TagOpSequence()
            {
                AntennaId = AntennaIds.All,
                SequenceStopTrigger = SequenceTriggerType.None,
                State = SequenceState.Active
            };
            // 使用特殊 ID 标识附加数据 AccessSpec
            sequence.Id = ATTACHED_DATA_ACCESS_SPEC_ID;
            
            sequence.Ops.Add((TagOp)new TagReadOp()
            {
                AccessPassword = TagData.FromHexString(string.IsNullOrWhiteSpace(attachedData.AccessPassword) ? "00000000" : attachedData.AccessPassword),
                MemoryBank = attachedData.MemoryBank,
                WordPointer = attachedData.WordPointer,
                WordCount = attachedData.WordCount
            });
            this.AddOpSequence(sequence);
        }

        /// <summary>Obsolete method. Do not use.</summary>
        [Obsolete("This method has been renamed QueryDefaultSettings.", true)]
        public Settings QueryFactorySettings() => (Settings)null;

        /// <summary>Obsolete method. Do not use.</summary>
        /// <returns></returns>
        [Obsolete("This method has been renamed ApplyDefaultSettings.", true)]
        public Settings ApplyFactorySettings() => (Settings)null;

        /// <summary>Returns the reader default settings.</summary>
        /// <returns>The default settings of the reader.</returns>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">
        /// Thrown when this method is called prior to establishing a connection with a reader.
        /// </exception>
        public Settings QueryDefaultSettings()
        {
            if (!this.IsConnected)
                throw new LLRPSdkException("You must connect to the reader before getting the default configuration.");
            return new Settings(this._readerCapabilities.AntennaCount, this._readerCapabilities.GpiCount, this._readerCapabilities.GpoCount, this._readerCapabilities.FirmwareVersion);
        }

        /// <summary>
        /// Assigns a name to this reader. For example, "Dock Door #1 Reader".
        /// This name can be used to identify a particular reader in an application
        /// that controls multiple readers.
        /// </summary>
        public string Name { get; set; }

        /// <summary>Contains the IP address or hostname of the reader.</summary>
        /// <remarks>Read only.</remarks>
        public string Address { get; private set; }

        /// <summary>Applies the default settings to the reader.</summary>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">
        /// Thrown if this method is called
        /// prior to establishing a connection with a reader.
        /// </exception>
        public void ApplyDefaultSettings()
        {
            if (!this.IsConnected)
                throw new LLRPSdkException("You must connect to the reader before configuring it.");
            this.ApplySettings(new Settings(this._readerCapabilities.AntennaCount, this._readerCapabilities.GpiCount, this._readerCapabilities.GpoCount, this._readerCapabilities.FirmwareVersion));
        }

        /// <summary>
        /// Resets the reader to factory defaults only, without applying
        /// any SDK-generated settings afterwards.
        /// </summary>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">
        /// Thrown if this method is called prior to establishing a connection with a reader.
        /// </exception>
        public void ResetToFactoryDefaultsOnly()
        {
            if (!this.IsConnected)
                throw new LLRPSdkException("You must connect to the reader before resetting to factory defaults.");
            this.ResetToFactoryDefaults();
        }

        /// <summary>
        /// Instructs the Reader to save the current configuration to persistent
        /// storage. The saved parameters then become the Reader's power-on and
        /// reset settings.
        /// </summary>
        public void SaveSettings()
        {
            throw new NotSupportedException();


            //      MSG_ERROR_MESSAGE msg_err;
            //MSG_IMPINJ_SAVE_SETTINGS_RESPONSE rsp = this.reader != null ? this.reader.CUSTOM_MESSAGE((MSG_CUSTOM_MESSAGE) new MSG_IMPINJ_SAVE_SETTINGS()
            //{
            //  SaveConfiguration = true
            //}, out msg_err, this.MessageTimeout) as MSG_IMPINJ_SAVE_SETTINGS_RESPONSE : throw new OctaneSdkException("You must connect to the reader before saving settings.");
            //string str = "Impinj Save Settings";
            //ImpinjReader.CheckForNullReply(str, (Message) rsp, msg_err);
            //ImpinjReader.CheckLlrpReply(rsp.LLRPStatus, msg_err, str);
        }

        private void ResetToFactoryDefaults()
        {
            MSG_ERROR_MESSAGE msg_err;
            MSG_SET_READER_CONFIG_RESPONSE rsp = this.reader != null ? this.reader.SET_READER_CONFIG(new MSG_SET_READER_CONFIG()
            {
                ResetToFactoryDefault = true
            }, out msg_err, this.MessageTimeout) : throw new LLRPSdkException("You must connect to the reader before resetting to factory defaults.");
            string str = "Reset to factory defaults";
            LlrpReader.CheckForNullReply(str, (Message)rsp, msg_err);
            LlrpReader.CheckLlrpReply(rsp.LLRPStatus, msg_err, str);
        }

        /// <summary />
        /// <param name="seconds" />
        /// <returns />
        /// <exception cref="T:LLRPSdk.LLRPSdkException" />
        [Obsolete("The use of this method is discouraged because it blocks program execution and performs poorly. We suggest receiving tag reports asynchronously via an event.", false)]
        public TagReport QueryTags(double seconds)
        {
            if (!this.IsConnected)
                throw new LLRPSdkException("You must connect to the reader before querying tags.");
            TagReport QueryTagsTagReport = new TagReport();
            LlrpReader.TagsReportedHandler tagsReportedHandler = (LlrpReader.TagsReportedHandler)((reader, report) =>
            {
                foreach (Tag tag in report)
                    QueryTagsTagReport.Tags.Add(tag);
            });
            this.TagsReported += tagsReportedHandler;
            this.Start();
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            this.Stop();
            this.TagsReported -= tagsReportedHandler;
            return QueryTagsTagReport;
        }

        /// \endcond
        ///             <summary>
        /// Tells the reader to send all the tag reports it has buffered.
        /// The ReportMode should be set so that reader accumulates tag reads
        /// (WaitForQuery or BatchAfterStop).
        /// </summary>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">
        /// Thrown when this method is called prior to establishing a connection
        /// with a reader.</exception>
        public void QueryTags()
        {
            if (!this.IsConnected)
                throw new LLRPSdkException("You must connect to the reader before querying tags.");
            this.GetTagReports();
        }

        private void GetTagReports()
        {
            if (this.reader == null)
                throw new LLRPSdkException("You must connect to the reader before getting tag reports.");
            MSG_ERROR_MESSAGE msg_err;
            this.reader.GET_REPORT(new MSG_GET_REPORT(), out msg_err, this.MessageTimeout);
            if (msg_err != null)
                throw new LLRPSdkException("Error getting buffered tag reports.");
        }

        /// <summary />
        [Obsolete("This method is now obsolete. Use ApplyDefaultSettings or QueryDefaultSettings instead", true)]
        public void ClearSettings()
        {
        }

        /// <summary>
        /// Adds a sequence of tag operations (read, write, lock, kill, authenticate) to the
        /// reader.
        /// </summary>
        /// <param name="sequence">
        /// The sequence of operations to add to the reader.
        /// </param>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">
        /// Thrown if a zero length operations sequence is provided.</exception>
        public void AddOpSequence(TagOpSequence sequence)
        {
            if (sequence == null || sequence.Ops.Count == 0)
                throw new LLRPSdkException("Cannot add an empty operation sequence");
            this.AddAccessSpec(sequence);
            if (sequence.State != SequenceState.Active)
                return;
            this.EnableAccessSpec(sequence.Id);
        }

        /// <summary>Enables a tag operation sequence on the reader.</summary>
        /// <param name="sequenceId">
        /// The sequence ID of the operation sequence to enable.
        /// </param>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">
        /// Thrown if the referenced sequence does not exist on the reader.
        /// </exception>
        public void EnableOpSequence(uint sequenceId) => this.EnableAccessSpec(sequenceId);

        /// <summary>Deletes a tag operation sequence from the reader.</summary>
        /// <param name="sequenceId">
        /// The sequence ID of the operation sequence to delete.
        /// </param>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">
        /// Thrown if the referenced sequence does not exist on the reader.
        /// </exception>
        public void DeleteOpSequence(uint sequenceId) => this.DeleteAccessSpec(sequenceId);

        /// <summary>Deletes all tag operation sequences from the reader.</summary>
        public void DeleteAllOpSequences() => this.DeleteAccessSpec(0U);

        /// <summary>
        /// Checks if the attached data access spec is currently enabled on the reader.
        /// Returns null if not found or configuration disabled, true if enabled, false if disabled.
        /// </summary>
        public bool? IsAttachedDataAccessSpecEnabled()
        {
            try
            {
                var settings = this.QuerySettings();
                if (settings?.AttachedData == null || !settings.AttachedData.Enabled)
                    return null;
                
                var accessSpecs = this.GetAccessSpecs()?.AccessSpec;
                if (accessSpecs == null)
                    return null;
                    
                var attachedDataSpec = accessSpecs.FirstOrDefault(a => a.AccessSpecID == ATTACHED_DATA_ACCESS_SPEC_ID);
                if (attachedDataSpec == null)
                    return null;
                    
                return attachedDataSpec.CurrentState == ENUM_AccessSpecState.Active;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Restores the attached data access spec with its previous enabled state.
        /// </summary>
        public void RestoreAttachedDataAccessSpec(bool wasEnabled)
        {
            try
            {
                var settings = this.QuerySettings();
                // 只检查配置是否存在（WordCount > 0），不检查 Enabled 属性
                // 因为 Enabled 会随着设备状态动态变化（AccessSpec被删除后会被重置）
                if (settings?.AttachedData == null || settings.AttachedData.WordCount == 0)
                    return;

                // 重新添加附加数据 AO
                TagOpSequence sequence = new TagOpSequence()
                {
                    AntennaId = AntennaIds.All,
                    SequenceStopTrigger = SequenceTriggerType.None,
                    State = wasEnabled ? SequenceState.Active : SequenceState.Disabled
                };
                // 使用特殊 ID 标识附加数据 AccessSpec
                sequence.Id = ATTACHED_DATA_ACCESS_SPEC_ID;

                AttachedDataConfig attachedData = settings.AttachedData;
                sequence.Ops.Add((TagOp)new TagReadOp()
                {
                    AccessPassword = TagData.FromHexString(string.IsNullOrWhiteSpace(attachedData.AccessPassword) ? "00000000" : attachedData.AccessPassword),
                    MemoryBank = attachedData.MemoryBank,
                    WordPointer = attachedData.WordPointer,
                    WordCount = attachedData.WordCount
                });
                // 使用 AddOpSequence，它会根据 State 决定是否启用
                this.AddOpSequence(sequence);
            }
            catch (Exception)
            {
                // 记录错误但不抛出异常
                //this.LogLlrpMessage($"恢复附加数据 AO 失败：{ex.Message}");
            }
        }

        /// <summary>Deletes all tag operation sequences from the reader.</summary>
        /// Queries the reader for a summary of the features that it supports.
        /// </summary>
        /// <returns>
        /// An object summarizing the features supported by the reader.
        /// </returns>
        public FeatureSet QueryFeatureSet()
        {
            FeatureSet featureSet = new FeatureSet(this.GetReaderCapabilities());
            this._readerCapabilities = new FeatureSet(featureSet);
            return featureSet;
        }



        private void SetCustomReaderConfig(ICustom_Parameter param, string msgType)
        {
            if (this.reader == null)
                throw new LLRPSdkException("You must connect to the reader before setting reader config.");
            MSG_SET_READER_CONFIG msg = new MSG_SET_READER_CONFIG();
            msg.AddCustomParameter(param);
            MSG_ERROR_MESSAGE msg_err;
            MSG_SET_READER_CONFIG_RESPONSE rsp = this.reader.SET_READER_CONFIG(msg, out msg_err, this.MessageTimeout);
            LlrpReader.CheckForNullReply(msgType, (Message)rsp, msg_err);
            LlrpReader.CheckLlrpReply(rsp.LLRPStatus, msg_err, msgType);
        }

        private void DeleteAccessSpec(uint id)
        {
            if (this.reader == null)
                throw new LLRPSdkException("You must connect to the reader before deleting access spec.");
            MSG_ERROR_MESSAGE msg_err;
            MSG_DELETE_ACCESSSPEC_RESPONSE rsp = this.reader.DELETE_ACCESSSPEC(new MSG_DELETE_ACCESSSPEC()
            {
                AccessSpecID = id
            }, out msg_err, this.MessageTimeout);
            string str = "DELETE_ACCESSSPEC";
            LlrpReader.CheckForNullReply(str, (Message)rsp, msg_err);
            LlrpReader.CheckLlrpReply(rsp.LLRPStatus, msg_err, str);
        }

        private static string LlrpBitArrayToBinStr(LLRPBitArray array)
        {
            string binStr = "";
            for (int index = 0; index < array.Count; ++index)
                binStr = !array[index] ? binStr + "0" : binStr + "1";
            return binStr;
        }

        private static LLRPBitArray TruncateTagMask(LLRPBitArray mask, int len)
        {
            LLRPBitArray llrpBitArray = mask;
            if (len > mask.Count)
                throw new LLRPSdkException("Error setting the tag mask. The value specified for BitCount is greater than the data provided.");
            if (len < mask.Count)
                llrpBitArray = LLRPBitArray.FromBinString(LlrpReader.LlrpBitArrayToBinStr(mask).Substring(0, len));
            return llrpBitArray;
        }



        private PARAM_C1G2Filter[] GetC1G2Filters(FilterSettings filterSettings)
        {
            PARAM_C1G2Filter[] c1G2Filters = (PARAM_C1G2Filter[])null;
            if (filterSettings.Mode == TagFilterMode.UseTagSelectFilters)
            {
                int selectFiltersAllowed = this._readerCapabilities.MaxTagSelectFiltersAllowed;
                int count = filterSettings.TagSelectFilters.Count;
                c1G2Filters = count <= selectFiltersAllowed ? new PARAM_C1G2Filter[count] : throw new LLRPSdkException("Error parsing tag select filter list. " + string.Format("The tag select filter list has {0} filter(s), ", (object)count) + string.Format("which is more than the maximum supported by the reader which is {0}.", (object)selectFiltersAllowed));
                for (int index = 0; index < count; ++index)
                {
                    TagSelectFilter tagSelectFilter = filterSettings.TagSelectFilters[index];
                    c1G2Filters[index] = new PARAM_C1G2Filter()
                    {
                        C1G2TagInventoryMask = new PARAM_C1G2TagInventoryMask()
                        {
                            MB = new TwoBits((ushort)tagSelectFilter.MemoryBank),
                            Pointer = tagSelectFilter.BitPointer,
                            TagMask = LLRPBitArray.FromHexString(tagSelectFilter.TagMask)
                        }
                    };
                    if (tagSelectFilter.BitCount > 0)
                        c1G2Filters[index].C1G2TagInventoryMask.TagMask = LlrpReader.TruncateTagMask(c1G2Filters[index].C1G2TagInventoryMask.TagMask, tagSelectFilter.BitCount);
                    PARAM_C1G2TagInventoryStateUnawareFilterAction unawareFilterAction = new PARAM_C1G2TagInventoryStateUnawareFilterAction()
                    {
                        Action = StateUnawareActionExtensions.ConvertToC1G2StateUnawareAction(tagSelectFilter.MatchAction, tagSelectFilter.NonMatchAction)
                    };
                    c1G2Filters[index].C1G2TagInventoryStateUnawareFilterAction = unawareFilterAction;
                }
            }
            else if (filterSettings.Mode == TagFilterMode.Filter1AndFilter2 || filterSettings.Mode == TagFilterMode.Filter1OrFilter2)
            {
                c1G2Filters = new PARAM_C1G2Filter[2]
                {
          new PARAM_C1G2Filter()
          {
            C1G2TagInventoryMask = new PARAM_C1G2TagInventoryMask()
            {
              MB = new TwoBits((ushort) filterSettings.TagFilter1.MemoryBank),
              Pointer = filterSettings.TagFilter1.BitPointer,
              TagMask = LLRPBitArray.FromHexString(filterSettings.TagFilter1.TagMask)
            }
          },
          new PARAM_C1G2Filter()
          {
            C1G2TagInventoryMask = new PARAM_C1G2TagInventoryMask()
            {
              MB = new TwoBits((ushort) filterSettings.TagFilter2.MemoryBank),
              Pointer = filterSettings.TagFilter2.BitPointer,
              TagMask = LLRPBitArray.FromHexString(filterSettings.TagFilter2.TagMask)
            }
          }
                };
                if (filterSettings.TagFilter1.BitCount > 0)
                    c1G2Filters[0].C1G2TagInventoryMask.TagMask = LlrpReader.TruncateTagMask(c1G2Filters[0].C1G2TagInventoryMask.TagMask, filterSettings.TagFilter1.BitCount);
                if (filterSettings.TagFilter2.BitCount > 0)
                    c1G2Filters[1].C1G2TagInventoryMask.TagMask = LlrpReader.TruncateTagMask(c1G2Filters[1].C1G2TagInventoryMask.TagMask, filterSettings.TagFilter2.BitCount);
                c1G2Filters[0].C1G2TagInventoryStateUnawareFilterAction = new PARAM_C1G2TagInventoryStateUnawareFilterAction();
                c1G2Filters[0].C1G2TagInventoryStateUnawareFilterAction.Action = filterSettings.TagFilter1.FilterOp != TagFilterOp.Match ? ENUM_C1G2StateUnawareAction.Unselect_Select : ENUM_C1G2StateUnawareAction.Select_Unselect;
                c1G2Filters[1].C1G2TagInventoryStateUnawareFilterAction = new PARAM_C1G2TagInventoryStateUnawareFilterAction();
                if (filterSettings.Mode == TagFilterMode.Filter1AndFilter2)
                    c1G2Filters[1].C1G2TagInventoryStateUnawareFilterAction.Action = filterSettings.TagFilter2.FilterOp != TagFilterOp.Match ? ENUM_C1G2StateUnawareAction.Unselect_DoNothing : ENUM_C1G2StateUnawareAction.DoNothing_Unselect;
                else if (filterSettings.Mode == TagFilterMode.Filter1OrFilter2)
                    c1G2Filters[1].C1G2TagInventoryStateUnawareFilterAction.Action = filterSettings.TagFilter2.FilterOp != TagFilterOp.Match ? ENUM_C1G2StateUnawareAction.DoNothing_Select : ENUM_C1G2StateUnawareAction.Select_DoNothing;
            }
            else if (filterSettings.Mode != TagFilterMode.None)
            {
                TagFilter tagFilter = filterSettings.Mode != TagFilterMode.OnlyFilter1 ? filterSettings.TagFilter2 : filterSettings.TagFilter1;
                c1G2Filters = new PARAM_C1G2Filter[1]
                {
          new PARAM_C1G2Filter()
          {
            C1G2TagInventoryMask = new PARAM_C1G2TagInventoryMask()
            {
              MB = new TwoBits((ushort) tagFilter.MemoryBank),
              Pointer = tagFilter.BitPointer,
              TagMask = LLRPBitArray.FromHexString(tagFilter.TagMask)
            },
            C1G2TagInventoryStateUnawareFilterAction = new PARAM_C1G2TagInventoryStateUnawareFilterAction()
            {
              Action = tagFilter.FilterOp == TagFilterOp.Match ? ENUM_C1G2StateUnawareAction.Select_Unselect : ENUM_C1G2StateUnawareAction.Unselect_Select
            }
          }
                };
                if (tagFilter.BitCount > 0)
                    c1G2Filters[0].C1G2TagInventoryMask.TagMask = LlrpReader.TruncateTagMask(c1G2Filters[0].C1G2TagInventoryMask.TagMask, tagFilter.BitCount);
            }
            return c1G2Filters;
        }

        private PARAM_AISpec GetAiSpec(Settings config)
        {
            ushort length = 0;
            PARAM_AISpec aiSpec = new PARAM_AISpec()
            {
                AntennaIDs = new UInt16Array()
            };
            foreach (AntennaConfig antenna in config.Antennas)
            {
                if (antenna.IsEnabled)
                {
                    aiSpec.AntennaIDs.Add(antenna.PortNumber);
                    ++length;
                }
            }
            aiSpec.AISpecStopTrigger = new PARAM_AISpecStopTrigger();
            if (config.EndOfCycleEvent)
            {
                aiSpec.AISpecStopTrigger.AISpecStopTriggerType = ENUM_AISpecStopTriggerType.Tag_Observation;
                aiSpec.AISpecStopTrigger.DurationTrigger = 0U;
                aiSpec.AISpecStopTrigger.TagObservationTrigger = new PARAM_TagObservationTrigger()
                {
                    TriggerType = ENUM_TagObservationTriggerType.N_Attempts_To_See_All_Tags_In_FOV_Or_Timeout,
                    NumberOfTags = (ushort)0,
                    NumberOfAttempts = (ushort)1,
                    T = (ushort)0,
                    Timeout = 0U
                };
            }
            else
                aiSpec.AISpecStopTrigger.AISpecStopTriggerType = ENUM_AISpecStopTriggerType.Null;
            PARAM_C1G2InventoryCommand val = new PARAM_C1G2InventoryCommand()
            {
                TagInventoryStateAware = false//todo  impinj sdk 默认关闭，使用 search mode 后面需要开放这个参数
            };

            
            val.C1G2Filter = this.GetC1G2Filters(config.Filters);
            PARAM_C1G2RFControl rfControl = new PARAM_C1G2RFControl();
            uint? rfMode = config.RfMode;
            if (!rfMode.HasValue)
            {
               
            }
            else
            {
                rfControl.ModeIndex = (ushort)rfMode.Value;
            }
            rfControl.Tari = (ushort)0;
            val.C1G2RFControl = rfControl;
            PARAM_C1G2SingulationControl singulationControl = new PARAM_C1G2SingulationControl()
            {
                Session = new TwoBits(config.Session),
                TagPopulation = config.TagPopulationEstimate,
                TagTransitTime = 0
            };
            val.C1G2SingulationControl = singulationControl;


           

            if (length > (ushort)0)
            {
                ushort index = 0;
                aiSpec.InventoryParameterSpec = new PARAM_InventoryParameterSpec[1];
                aiSpec.InventoryParameterSpec[0] = new PARAM_InventoryParameterSpec()
                {
                    InventoryParameterSpecID = (ushort)123,
                    ProtocolID = ENUM_AirProtocols.EPCGlobalClass1Gen2,
                    AntennaConfiguration = new PARAM_AntennaConfiguration[(int)length]
                };
                foreach (AntennaConfig antenna in config.Antennas)
                {
                    if (antenna.IsEnabled)
                    {
                        ushort num1;
                        bool error;
                        if (antenna.MaxRxSensitivity || antenna.RxSensitivityInDbm == 0.0)
                        {
                            num1 = (ushort)1;//impinj index from 1
                            if (_readerCapabilities.DeviceManufacturerNumber == 161)
                            {
                                num1 -= 1;// zebra index from 0
                            }
                        }
                        else
                        {
                            num1 = this.GetRxSensitivityIndex(antenna.RxSensitivityInDbm, out error);
                            if (error && _readerCapabilities.DeviceManufacturerNumber != 161)
                                throw new LLRPSdkException("Invalid Rx Sensitivity setting for antenna port " + antenna.PortNumber.ToString());
                        }
                        ushort num2;
                        if (antenna.MaxTxPower || antenna.TxPowerInDbm == 0.0)
                        {
                            num2 = (ushort)this._readerCapabilities.TxPowers.Count;//impinj index from 1
                            if (_readerCapabilities.DeviceManufacturerNumber == 161)
                            {
                                num2 -= 1;// zebra index from 0
                            }
                        }
                        else
                        {
                            num2 = this.GetTxPowerIndex(antenna.TxPowerInDbm, out error);
                            if (error)
                                throw new LLRPSdkException("Invalid Tx Power setting for antenna port" + antenna.PortNumber.ToString());
                        }
                        aiSpec.InventoryParameterSpec[0].AntennaConfiguration[(int)index] = new PARAM_AntennaConfiguration()
                        {
                            AntennaID = antenna.PortNumber
                        };
                        aiSpec.InventoryParameterSpec[0].AntennaConfiguration[(int)index].AirProtocolInventoryCommandSettings.Add((IParameter)val);
                        aiSpec.InventoryParameterSpec[0].AntennaConfiguration[(int)index].RFTransmitter = new PARAM_RFTransmitter()
                        {
                            TransmitPower = num2,
                            //TransmitPower = 200,
                            HopTableID = (ushort)1,
                            ChannelIndex = (ushort)1
                        };
                        aiSpec.InventoryParameterSpec[0].AntennaConfiguration[(int)index].RFReceiver = new PARAM_RFReceiver()
                        {
                            ReceiverSensitivity = num1
                            //ReceiverSensitivity = 0
                        };
                        ++index;
                    }
                }
            }
            return aiSpec;
        }





        private void AddRoSpec(MSG_ADD_ROSPEC msg)
        {
            if (this.reader == null)
                throw new LLRPSdkException("You must connect to the reader before adding rospec.");
            
            MSG_ERROR_MESSAGE msg_err;
            MSG_ADD_ROSPEC_RESPONSE rsp = this.reader.ADD_ROSPEC(msg, out msg_err, this.MessageTimeout);
            
            // 记录 Transaction 日志
            this.LogTransaction(msg, rsp, msg_err, "ADD_ROSPEC",false);
            
            string str = "ADD_ROSPEC";
            LlrpReader.CheckForNullReply(str, (Message)rsp, msg_err);
            LlrpReader.CheckLlrpReply(rsp.LLRPStatus, msg_err, str);
        }

        /// <summary>Build up an LTK add ROSpec message.</summary>
        /// <param name="config">A settings object after which the message will be modeled.</param>
        /// <returns>An LTK add ROSpec message.</returns>
        public MSG_ADD_ROSPEC BuildAddROSpecMessage(Settings config)
        {
            MSG_ADD_ROSPEC msgAddRospec = new MSG_ADD_ROSPEC();
            this.outstandingAuthenticateRequests.Clear();
            msgAddRospec.ROSpec = new PARAM_ROSpec()
            {
                CurrentState = ENUM_ROSpecState.Disabled,
                ROSpecID = (uint)LlrpReader.RO_SPEC_ID,
                ROBoundarySpec = new PARAM_ROBoundarySpec()
            };
            msgAddRospec.ROSpec.ROBoundarySpec.ROSpecStartTrigger = new PARAM_ROSpecStartTrigger()
            {
                ROSpecStartTriggerType = (ENUM_ROSpecStartTriggerType)config.AutoStart.Mode
            };
            if (config.AutoStart.Mode == AutoStartMode.GpiTrigger)
            {
                PARAM_GPITriggerValue paramGpiTriggerValue = new PARAM_GPITriggerValue()
                {
                    GPIPortNum = config.AutoStart.GpiPortNumber,
                    GPIEvent = config.AutoStart.GpiLevel
                };
                msgAddRospec.ROSpec.ROBoundarySpec.ROSpecStartTrigger.GPITriggerValue = paramGpiTriggerValue;
            }
            else if (config.AutoStart.Mode == AutoStartMode.Periodic)
            {
                PARAM_PeriodicTriggerValue periodicTriggerValue = new PARAM_PeriodicTriggerValue()
                {
                    Offset = config.AutoStart.FirstDelayInMs,
                    Period = config.AutoStart.PeriodInMs
                };
                if (config.AutoStart.UtcTimestamp != 0UL)
                    periodicTriggerValue.UTCTimestamp = new PARAM_UTCTimestamp()
                    {
                        Microseconds = config.AutoStart.UtcTimestamp
                    };
                msgAddRospec.ROSpec.ROBoundarySpec.ROSpecStartTrigger.PeriodicTriggerValue = periodicTriggerValue;
            }
            msgAddRospec.ROSpec.ROBoundarySpec.ROSpecStopTrigger = new PARAM_ROSpecStopTrigger()
            {
                ROSpecStopTriggerType = (ENUM_ROSpecStopTriggerType)config.AutoStop.Mode
            };
            if (config.AutoStop.Mode == AutoStopMode.Duration)
                msgAddRospec.ROSpec.ROBoundarySpec.ROSpecStopTrigger.DurationTriggerValue = config.AutoStop.DurationInMs;
            else if (config.AutoStop.Mode == AutoStopMode.GpiTrigger)
            {
                PARAM_GPITriggerValue paramGpiTriggerValue = new PARAM_GPITriggerValue()
                {
                    GPIPortNum = config.AutoStop.GpiPortNumber,
                    GPIEvent = config.AutoStop.GpiLevel,
                    Timeout = config.AutoStop.Timeout
                };
                msgAddRospec.ROSpec.ROBoundarySpec.ROSpecStopTrigger.GPITriggerValue = paramGpiTriggerValue;
            }
            msgAddRospec.ROSpec.SpecParameter = new UNION_SpecParameter();

            msgAddRospec.ROSpec.SpecParameter.Add((IParameter)this.GetAiSpec(config));



            msgAddRospec.ROSpec.ROReportSpec = new PARAM_ROReportSpec();
            if (config.Report.Mode == ReportMode.Individual)
            {
                msgAddRospec.ROSpec.ROReportSpec.ROReportTrigger = ENUM_ROReportTriggerType.Upon_N_Tags_Or_End_Of_ROSpec;
                msgAddRospec.ROSpec.ROReportSpec.N = (ushort)1;
            }
            else if (config.Report.Mode == ReportMode.BatchAfterStop)
            {
                msgAddRospec.ROSpec.ROReportSpec.ROReportTrigger = ENUM_ROReportTriggerType.Upon_N_Tags_Or_End_Of_ROSpec;
                msgAddRospec.ROSpec.ROReportSpec.N = (ushort)0;
            }
            else if (config.Report.Mode == ReportMode.WaitForQuery)
            {
                msgAddRospec.ROSpec.ROReportSpec.ROReportTrigger = ENUM_ROReportTriggerType.None;
                msgAddRospec.ROSpec.ROReportSpec.N = (ushort)0;
            }
            msgAddRospec.ROSpec.ROReportSpec.TagReportContentSelector = new PARAM_TagReportContentSelector()
            {
                EnableAccessSpecID = true,
                EnableAntennaID = config.Report.IncludeAntennaPortNumber,
                EnableChannelIndex = config.Report.IncludeChannel,
                EnableFirstSeenTimestamp = config.Report.IncludeFirstSeenTime,
                EnableLastSeenTimestamp = config.Report.IncludeLastSeenTime
            };
            if (config.Report.IncludeCrc || config.Report.IncludePcBits)
            {
                PARAM_C1G2EPCMemorySelector val = new PARAM_C1G2EPCMemorySelector()
                {
                    EnableCRC = config.Report.IncludeCrc,
                    EnablePCBits = config.Report.IncludePcBits
                };
                msgAddRospec.ROSpec.ROReportSpec.TagReportContentSelector.AirProtocolEPCMemorySelector = new UNION_AirProtocolEPCMemorySelector();
                msgAddRospec.ROSpec.ROReportSpec.TagReportContentSelector.AirProtocolEPCMemorySelector.Add((IParameter)val);
            }
            msgAddRospec.ROSpec.ROReportSpec.TagReportContentSelector.EnableInventoryParameterSpecID = false;
            msgAddRospec.ROSpec.ROReportSpec.TagReportContentSelector.EnablePeakRSSI = config.Report.IncludePeakRssi;
            msgAddRospec.ROSpec.ROReportSpec.TagReportContentSelector.EnableROSpecID = false;
            msgAddRospec.ROSpec.ROReportSpec.TagReportContentSelector.EnableSpecIndex = false;
            msgAddRospec.ROSpec.ROReportSpec.TagReportContentSelector.EnableTagSeenCount = config.Report.IncludeSeenCount;

            return msgAddRospec;
        }

        private void EnableRoSpec()
        {
            if (this.reader == null)
                throw new LLRPSdkException("You must connect to the reader before enabling rospec.");
            MSG_ERROR_MESSAGE msg_err;
            MSG_ENABLE_ROSPEC_RESPONSE rsp = this.reader.ENABLE_ROSPEC(new MSG_ENABLE_ROSPEC()
            {
                ROSpecID = (uint)LlrpReader.RO_SPEC_ID
            }, out msg_err, this.MessageTimeout);
            string str = "ENABLE_ROSPEC";
            LlrpReader.CheckForNullReply(str, (Message)rsp, msg_err);
            LlrpReader.CheckLlrpReply(rsp.LLRPStatus, msg_err, str);
        }

        private ushort GetRxSensitivityIndex(double dBm, out bool error)
        {
            error = false;
            foreach (RxSensitivityTableEntry rxSensitivity in this._readerCapabilities.RxSensitivities)
            {
                if (dBm == rxSensitivity.Dbm)
                    return rxSensitivity.Index;
            }
            error = true;
            return 0;
        }

        private ushort GetTxPowerIndex(double dBm, out bool error)
        {
            error = false;
            foreach (TxPowerTableEntry txPower in this._readerCapabilities.TxPowers)
            {
                if (dBm == txPower.Dbm)
                    return txPower.Index;
            }
            error = true;
            return 0;
        }

        internal MSG_GET_READER_CAPABILITIES_RESPONSE GetReaderCapabilities()
        {
            MSG_GET_READER_CAPABILITIES req = new MSG_GET_READER_CAPABILITIES();
            MSG_ERROR_MESSAGE msg_err;
            MSG_GET_READER_CAPABILITIES_RESPONSE rsp = this.reader != null ? this.reader.GET_READER_CAPABILITIES(req, out msg_err, this.MessageTimeout) : throw new LLRPSdkException("You must connect to the reader before getting capabilities.");
            
            // 记录 Transaction 日志
            this.LogTransaction(req, rsp, msg_err, "GET_READER_CAPABILITIES("+ req.RequestedData + ")");
            
            string str = "GET_READER_CAPABILITIES";
            LlrpReader.CheckForNullReply(str, (Message)rsp, msg_err);
            LlrpReader.CheckLlrpReply(rsp.LLRPStatus, msg_err, str);
            return rsp;
        }

        private void StartRoSpec()
        {
            if (this.reader == null)
                throw new LLRPSdkException("You must connect to the reader before starting rospec.");
            MSG_ERROR_MESSAGE msg_err;
            MSG_START_ROSPEC_RESPONSE rsp = this.reader.START_ROSPEC(new MSG_START_ROSPEC()
            {
                ROSpecID = (uint)LlrpReader.RO_SPEC_ID
            }, out msg_err, this.MessageTimeout);
            string str = "START_ROSPEC";
            LlrpReader.CheckForNullReply(str, (Message)rsp, msg_err);
            LlrpReader.CheckLlrpReply(rsp.LLRPStatus, msg_err, str);
        }

        private void StopRoSpec()
        {
            if (this.reader == null)
                throw new LLRPSdkException("You must connect to the reader before stopping rospec.");
            MSG_ERROR_MESSAGE msg_err;
            MSG_STOP_ROSPEC_RESPONSE rsp = this.reader.STOP_ROSPEC(new MSG_STOP_ROSPEC()
            {
                ROSpecID = (uint)LlrpReader.RO_SPEC_ID
            }, out msg_err, this.MessageTimeout);
            string str = "STOP_ROSPEC";
            LlrpReader.CheckForNullReply(str, (Message)rsp, msg_err);
            LlrpReader.CheckLlrpReply(rsp.LLRPStatus, msg_err, str);
        }

        private void SetReaderConfig(MSG_SET_READER_CONFIG msg)
        {
            if (this.reader == null)
                throw new LLRPSdkException("You must connect to the reader before setting config.");
            MSG_ERROR_MESSAGE msg_err;
            MSG_SET_READER_CONFIG_RESPONSE rsp = this.reader.SET_READER_CONFIG(msg, out msg_err, this.MessageTimeout);
            string str = "SET_READER_CONFIG";
            LlrpReader.CheckForNullReply(str, (Message)rsp, msg_err);
            LlrpReader.CheckLlrpReply(rsp.LLRPStatus, msg_err, str);
        }

        /// <summary>Build up an LTK set reader config message.</summary>
        /// <param name="config">A settings object after which the message will be modeled.</param>
        /// <returns>An LTK set reader config message.</returns>
        public MSG_SET_READER_CONFIG BuildSetReaderConfigMessage(Settings config)
        {
            MSG_SET_READER_CONFIG readerConfig = new MSG_SET_READER_CONFIG()
            {
                ROReportSpec = new PARAM_ROReportSpec()
                {
                    TagReportContentSelector = new PARAM_TagReportContentSelector(),
                    ROReportTrigger = ENUM_ROReportTriggerType.Upon_N_Tags_Or_End_Of_ROSpec,
                    N = 1
                }
            };
            this.SetReaderConfigAntennaConfiguration(readerConfig, config);
            if (config.Keepalives.Enabled)
            {
                PARAM_KeepaliveSpec paramKeepaliveSpec = new PARAM_KeepaliveSpec()
                {
                    KeepaliveTriggerType = ENUM_KeepaliveTriggerType.Periodic,
                    PeriodicTriggerValue = config.Keepalives.PeriodInMs
                };
                readerConfig.KeepaliveSpec = paramKeepaliveSpec;
                this.keepaliveTimer.Interval = (double)config.Keepalives.PeriodInMs * 1.5;
                this.keepaliveTimer.AutoReset = false;
                this.keepaliveTimer.Start();

            }
            readerConfig.GPIPortCurrentState = new PARAM_GPIPortCurrentState[(int)this._readerCapabilities.GpiCount];
            readerConfig.ReaderEventNotificationSpec = new PARAM_ReaderEventNotificationSpec()
            {
                EventNotificationState = new PARAM_EventNotificationState[5]
            };
            readerConfig.ReaderEventNotificationSpec.EventNotificationState[0] = new PARAM_EventNotificationState()
            {
                EventType = ENUM_NotificationEventType.GPI_Event,
                NotificationState = true
            };
            readerConfig.ReaderEventNotificationSpec.EventNotificationState[1] = new PARAM_EventNotificationState()
            {
                EventType = ENUM_NotificationEventType.Antenna_Event,
                NotificationState = true
            };
            readerConfig.ReaderEventNotificationSpec.EventNotificationState[2] = new PARAM_EventNotificationState()
            {
                EventType = ENUM_NotificationEventType.Report_Buffer_Fill_Warning,
                NotificationState = true
            };
            readerConfig.ReaderEventNotificationSpec.EventNotificationState[3] = new PARAM_EventNotificationState()
            {
                EventType = ENUM_NotificationEventType.ROSpec_Event,
                NotificationState = true
            };
            readerConfig.ReaderEventNotificationSpec.EventNotificationState[4] = new PARAM_EventNotificationState()
            {
                EventType = ENUM_NotificationEventType.AISpec_Event,
                NotificationState = config.EndOfCycleEvent
            };

            int index = 0;
            foreach (GpiConfig gpi in config.Gpis)
            {
                readerConfig.GPIPortCurrentState[index] = new PARAM_GPIPortCurrentState()
                {
                    GPIPortNum = gpi.PortNumber
                };
                if (gpi.IsEnabled)
                {
                    readerConfig.GPIPortCurrentState[index].Config = true;
                }
                else
                    readerConfig.GPIPortCurrentState[index].Config = false;
                ++index;
            }
            readerConfig.EventsAndReports = new PARAM_EventsAndReports();
            readerConfig.EventsAndReports.HoldEventsAndReportsUponReconnect = config.HoldReportsOnDisconnect;
            return readerConfig;
        }

        private void SetReaderConfigAntennaConfiguration(
          MSG_SET_READER_CONFIG readerConfig,
          Settings settings)
        {
            PARAM_C1G2InventoryCommand val = new PARAM_C1G2InventoryCommand();
            PARAM_AntennaConfiguration antennaConfiguration = new PARAM_AntennaConfiguration()
            {
                AntennaID = 0
            };
            antennaConfiguration.AirProtocolInventoryCommandSettings.Add((IParameter)val);
            readerConfig.AntennaConfiguration = new PARAM_AntennaConfiguration[1];
            readerConfig.AntennaConfiguration[0] = antennaConfiguration;
        }

        private void DeleteRoSpecs()
        {
            MSG_ERROR_MESSAGE msg_err;
            MSG_DELETE_ROSPEC_RESPONSE rsp = this.reader != null ? this.reader.DELETE_ROSPEC(new MSG_DELETE_ROSPEC()
            {
                ROSpecID = 0U
            }, out msg_err, this.MessageTimeout) : throw new LLRPSdkException("You must connect to the reader before deleting rospecs.");
            string str = "DELETE_ROSPEC";
            LlrpReader.CheckForNullReply(str, (Message)rsp, msg_err);
            LlrpReader.CheckLlrpReply(rsp.LLRPStatus, msg_err, str);
        }

        internal MSG_GET_READER_CONFIG_RESPONSE GetReaderConfig()
        {
            return this.GetReaderConfig(ENUM_GetReaderConfigRequestedData.All);
        }

        internal MSG_GET_READER_CONFIG_RESPONSE GetReaderConfig(
          ENUM_GetReaderConfigRequestedData requestedData
          )
        {
            if (this.reader == null)
                throw new LLRPSdkException("You must connect to the reader before getting config.");
            MSG_GET_READER_CONFIG msg = new MSG_GET_READER_CONFIG();
            string str = "GET_READER_CONFIG";
            msg.RequestedData = requestedData;
            MSG_ERROR_MESSAGE msg_err;
            MSG_GET_READER_CONFIG_RESPONSE readerConfig = this.reader.GET_READER_CONFIG(msg, out msg_err, this.MessageTimeout);


            this.LogTransaction(msg, readerConfig, msg_err, "GET_READER_Config("+msg.RequestedData+")");



            LlrpReader.CheckForNullReply(str, (Message)readerConfig, msg_err);
            LlrpReader.CheckLlrpReply(readerConfig.LLRPStatus, msg_err, str);

            
            return readerConfig;
        }

        internal MSG_GET_ROSPECS_RESPONSE GetRoSpecs()
        {
            MSG_ERROR_MESSAGE msg_err;
            MSG_GET_ROSPECS_RESPONSE rsp = this.reader != null ? this.reader.GET_ROSPECS(new MSG_GET_ROSPECS(), out msg_err, this.MessageTimeout) : throw new LLRPSdkException("You must connect to the reader before getting rospecs.");
            string str = "GET_ROSPECS";
            LlrpReader.CheckForNullReply(str, (Message)rsp, msg_err);
            LlrpReader.CheckLlrpReply(rsp.LLRPStatus, msg_err, str);
            return rsp;
        }

        internal MSG_GET_ACCESSSPECS_RESPONSE GetAccessSpecs()
        {
            MSG_ERROR_MESSAGE msg_err;
            MSG_GET_ACCESSSPECS_RESPONSE rsp = this.reader != null ? this.reader.GET_ACCESSSPECS(new MSG_GET_ACCESSSPECS(), out msg_err, this.MessageTimeout) : throw new LLRPSdkException("You must connect to the reader before getting access specs.");
            string str = "GET_ACCESSSPECS";
            LlrpReader.CheckForNullReply(str, (Message)rsp, msg_err);
            LlrpReader.CheckLlrpReply(rsp.LLRPStatus, msg_err, str);
            return rsp;
        }

        /// <summary>
        /// This function tells the reader to resume sending reports and events.
        /// Messages in the queue on the reader may then be sent.
        /// </summary>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">Thrown if a communication error
        /// occurs while talking with the reader.</exception>
        public void ResumeEventsAndReports()
        {
            if (this.reader == null)
                throw new LLRPSdkException("You must connect to the reader before enabling events and reports.");
            MSG_ERROR_MESSAGE msg_err;
            this.reader.ENABLE_EVENTS_AND_REPORTS(new MSG_ENABLE_EVENTS_AND_REPORTS(), out msg_err, this.MessageTimeout);
            if (msg_err != null)
                throw new LLRPSdkException("Error enabling events and reports.");
        }

        private short GetTemperature(MSG_GET_READER_CONFIG_RESPONSE config)
        {
            throw new NotSupportedException();
        }

        private Settings ParseRoSpecAndConfig(
          PARAM_ROSpec rospec,
          MSG_GET_READER_CONFIG_RESPONSE configWithKeepAliveAndCustom,//只保留了KeepAlive
          MSG_GET_READER_CONFIG_RESPONSE configWithGPIInfo,
          MSG_GET_READER_CONFIG_RESPONSE configWithGPOInfo,
          MSG_GET_READER_CONFIG_RESPONSE configWithEventsReports)
        {
            Settings roSpecAndConfig = new Settings();
            roSpecAndConfig.AutoStart.Mode = (AutoStartMode)rospec.ROBoundarySpec.ROSpecStartTrigger.ROSpecStartTriggerType;
            if (roSpecAndConfig.AutoStart.Mode == AutoStartMode.GpiTrigger)
            {
                roSpecAndConfig.AutoStart.GpiPortNumber = rospec.ROBoundarySpec.ROSpecStartTrigger.GPITriggerValue.GPIPortNum;
                roSpecAndConfig.AutoStart.GpiLevel = rospec.ROBoundarySpec.ROSpecStartTrigger.GPITriggerValue.GPIEvent;
            }
            else if (roSpecAndConfig.AutoStart.Mode == AutoStartMode.Periodic)
            {
                roSpecAndConfig.AutoStart.FirstDelayInMs = rospec.ROBoundarySpec.ROSpecStartTrigger.PeriodicTriggerValue.Offset;
                roSpecAndConfig.AutoStart.PeriodInMs = rospec.ROBoundarySpec.ROSpecStartTrigger.PeriodicTriggerValue.Period;
                roSpecAndConfig.AutoStart.UtcTimestamp = (rospec.ROBoundarySpec.ROSpecStartTrigger.PeriodicTriggerValue.UTCTimestamp==null)? (ulong)0:rospec.ROBoundarySpec.ROSpecStartTrigger.PeriodicTriggerValue.UTCTimestamp.Microseconds;
            }
            roSpecAndConfig.AutoStop.Mode = (AutoStopMode)rospec.ROBoundarySpec.ROSpecStopTrigger.ROSpecStopTriggerType;
            if (roSpecAndConfig.AutoStop.Mode == AutoStopMode.Duration)
                roSpecAndConfig.AutoStop.DurationInMs = rospec.ROBoundarySpec.ROSpecStopTrigger.DurationTriggerValue;
            else if (roSpecAndConfig.AutoStop.Mode == AutoStopMode.GpiTrigger)
            {
                roSpecAndConfig.AutoStop.GpiPortNumber = rospec.ROBoundarySpec.ROSpecStopTrigger.GPITriggerValue.GPIPortNum;
                roSpecAndConfig.AutoStop.GpiLevel = rospec.ROBoundarySpec.ROSpecStopTrigger.GPITriggerValue.GPIEvent;
                roSpecAndConfig.AutoStop.Timeout = rospec.ROBoundarySpec.ROSpecStopTrigger.GPITriggerValue.Timeout;
            }
            if (rospec.SpecParameter[0] is PARAM_AISpec paramAiSpec)
            {
                //踢掉空间空间感知能力的配置
                roSpecAndConfig.Antennas = new AntennaConfigGroup();
                foreach (ushort NewPortNumber in paramAiSpec.AntennaIDs.data)//保存启用天线的ids
                    roSpecAndConfig.Antennas.AntennaConfigs.Add(new AntennaConfig(NewPortNumber));
                PARAM_C1G2InventoryCommand inventoryCommandSetting = (PARAM_C1G2InventoryCommand)paramAiSpec.InventoryParameterSpec[0].AntennaConfiguration[0].AirProtocolInventoryCommandSettings[0];
                roSpecAndConfig.LoadFilterData(inventoryCommandSetting.C1G2Filter);
                roSpecAndConfig.RfMode = new uint?((uint)inventoryCommandSetting.C1G2RFControl.ModeIndex);
                roSpecAndConfig.TagPopulationEstimate = inventoryCommandSetting.C1G2SingulationControl.TagPopulation;
                roSpecAndConfig.Session = inventoryCommandSetting.C1G2SingulationControl.Session.ToInt();
                for (int index = 0; index < inventoryCommandSetting.Custom.Length; ++index)
                {
                    //去掉Impinj Custom 扩展的功能
                    //1.ImpinjLowDutyCycle
                    //2.ImpinjInventorySearchMode
                    //3.ImpinjTagPopulationEstimationMode
                    //4.ImpinjTruncatedReplyConfiguration   Gen2 V2
                    //5.ImpinjFixedFrequencyList   ETSI
                    //6.ImpinjReducedPowerFrequencyList FCC
                    //7.ImpinjTagFilterVerificationConfiguration 


                }
                roSpecAndConfig.Antennas.DisableAll();
                PARAM_AntennaConfiguration[] antennaConfiguration = paramAiSpec.InventoryParameterSpec[0].AntennaConfiguration;
                for (int index1 = 0; index1 < antennaConfiguration.Length; ++index1)
                {
                    AntennaConfig antenna = roSpecAndConfig.Antennas.GetAntenna(antennaConfiguration[index1].AntennaID);
                    antenna.IsEnabled = true;
                    antenna.ReaderMaxSensitivityCapability = this._readerCapabilities.ReaderMaxSensitivityActualDbm;
                    ushort receiverSensitivity = antennaConfiguration[index1].RFReceiver.ReceiverSensitivity;
                    bool foundRxSensitivity = false;
                    for (int rxIndex = 0; rxIndex < this._readerCapabilities.RxSensitivities.Count; ++rxIndex)
                    {
                        if (this._readerCapabilities.RxSensitivities[rxIndex].Index == receiverSensitivity)
                        {
                            antenna.RxSensitivityInDbm = this._readerCapabilities.RxSensitivities[rxIndex].Dbm;
                            foundRxSensitivity = true;
                            break;
                        }
                    }
                    if (!foundRxSensitivity)
                        throw new LLRPSdkException("The specified receive sensitivity index is invalid : " + receiverSensitivity.ToString());
                    antenna.MaxRxSensitivity = antenna.RxSensitivityInDbm == antenna.ReaderMaxSensitivityCapability;
                    ushort transmitPowerIndex = antennaConfiguration[index1].RFTransmitter.TransmitPower;
                    int maxTxPowerEntryIndex = this._readerCapabilities.TxPowers.Count - 1;
                    antenna.ReaderMaxTxPowerCapability = maxTxPowerEntryIndex >= 0 ? this._readerCapabilities.TxPowers[maxTxPowerEntryIndex].Dbm : 0.0;
                    bool foundTxPower = false;
                    for (int txIndex = 0; txIndex < this._readerCapabilities.TxPowers.Count; ++txIndex)
                    {
                        if (this._readerCapabilities.TxPowers[txIndex].Index == transmitPowerIndex)
                        {
                            antenna.TxPowerInDbm = this._readerCapabilities.TxPowers[txIndex].Dbm;
                            foundTxPower = true;
                            break;
                        }
                    }
                    if (!foundTxPower)
                        throw new LLRPSdkException("The specified transmit power index is invalid : " + transmitPowerIndex.ToString());
                    antenna.MaxTxPower = antenna.TxPowerInDbm == antenna.ReaderMaxTxPowerCapability;
                }
            }
            PARAM_ROReportSpec roReportSpec = rospec.ROReportSpec;
            if (roReportSpec.N == (ushort)1 && roReportSpec.ROReportTrigger == ENUM_ROReportTriggerType.Upon_N_Tags_Or_End_Of_ROSpec)
                roSpecAndConfig.Report.Mode = ReportMode.Individual;
            else if (roReportSpec.N == (ushort)0 && roReportSpec.ROReportTrigger == ENUM_ROReportTriggerType.Upon_N_Tags_Or_End_Of_ROSpec)
                roSpecAndConfig.Report.Mode = ReportMode.BatchAfterStop;
            else if (roReportSpec.ROReportTrigger == ENUM_ROReportTriggerType.None)
                roSpecAndConfig.Report.Mode = ReportMode.WaitForQuery;
            roSpecAndConfig.Report.IncludeAntennaPortNumber = roReportSpec.TagReportContentSelector.EnableAntennaID;
            roSpecAndConfig.Report.IncludeChannel = roReportSpec.TagReportContentSelector.EnableChannelIndex;
            roSpecAndConfig.Report.IncludeFirstSeenTime = roReportSpec.TagReportContentSelector.EnableFirstSeenTimestamp;
            roSpecAndConfig.Report.IncludeLastSeenTime = roReportSpec.TagReportContentSelector.EnableLastSeenTimestamp;
            roSpecAndConfig.Report.IncludeSeenCount = roReportSpec.TagReportContentSelector.EnableTagSeenCount;
            roSpecAndConfig.Report.IncludePeakRssi = roReportSpec.TagReportContentSelector.EnablePeakRSSI;
            if (roReportSpec.TagReportContentSelector.AirProtocolEPCMemorySelector.Length > 0)
            {
                PARAM_C1G2EPCMemorySelector epcMemorySelector = (PARAM_C1G2EPCMemorySelector)roReportSpec.TagReportContentSelector.AirProtocolEPCMemorySelector[0];
                roSpecAndConfig.Report.IncludeCrc = epcMemorySelector.EnableCRC;
                roSpecAndConfig.Report.IncludePcBits = epcMemorySelector.EnablePCBits;
            }
            if (roReportSpec.Custom.Length > 0)
            {
                //去掉上报配置里Impinj Custom 扩展的功能
                //1.IncludeFastId
                //2.IncludeDopplerFrequency
                //3.IncludePhaseAngle
                //4.IncludePeakRssi
                //5.IncludeGpsCoordinates
                //6.OptimizedReadOps
            }
            if (configWithEventsReports.EventsAndReports != null)
                roSpecAndConfig.HoldReportsOnDisconnect = configWithEventsReports.EventsAndReports.HoldEventsAndReportsUponReconnect;
            if (configWithKeepAliveAndCustom.KeepaliveSpec != null)
            {
                roSpecAndConfig.Keepalives.Enabled = configWithKeepAliveAndCustom.KeepaliveSpec.KeepaliveTriggerType != 0;
                roSpecAndConfig.Keepalives.PeriodInMs = configWithKeepAliveAndCustom.KeepaliveSpec.PeriodicTriggerValue;
            }
            roSpecAndConfig.Gpis = new GpiConfigGroup(this._readerCapabilities.GpiCount);
            //roSpecAndConfig.Gpos = new GpoConfigGroup(this._readerCapabilities.GpoCount);
            if (configWithGPIInfo.GPIPortCurrentState != null)
            {
                foreach (PARAM_GPIPortCurrentState portCurrentState in configWithGPIInfo.GPIPortCurrentState)
                {
                    GpiConfig gpiConfig = roSpecAndConfig.Gpis.GpiConfigs[(int)portCurrentState.GPIPortNum - GpiConfigGroup.GPI_PORT_START_INDEX];
                    gpiConfig.PortNumber = portCurrentState.GPIPortNum;
                    gpiConfig.IsEnabled = portCurrentState.Config;
                }
            }
            //去掉更多配置
            //1.ImpinjAdvancedGPOConfiguration
            //2.ImpinjGPIDebounceConfiguration

            //使用旧的标准的configWithGPOInfo的配置
            //if (configWithGPOInfo.GPOWriteData != null)
            //{
            //    foreach (PARAM_GPOWriteData gpoWriteData in configWithGPOInfo.GPOWriteData)
            //    {
            //        GpoConfig gpoConfig = roSpecAndConfig.Gpos.GpoConfigs[(int)gpoWriteData.GPOPortNumber - GpoConfigGroup.GPO_PORT_START_INDEX];
            //        gpoConfig.PortNumber = gpoWriteData.GPOPortNumber;
            //    }
            //}

            //这里gpi gpo 的配置好像意义不大，通过QueryStatus()获取gpi状态 
            return roSpecAndConfig;
        }

        /// <summary>
        /// This function queries the reader for its current settings.
        /// </summary>
        /// <returns>An object containing the current reader settings</returns>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">Thrown if a communication error
        /// occurs while talking with the reader, the reader has not
        /// been configured, or if the configuration was not applied
        /// by the SDK.</exception>
        /// <remarks>
        /// When querying a SpatialReader that has most recently been used
        /// in either Direction or Location mode, not all configuration
        /// data may be available. In their place, certain defaults may be
        /// present. One example is RfMode and Session when the reader has
        /// run most recently in Direction mode.
        /// </remarks>
        public Settings QuerySettings()
        {
            PARAM_ROSpec[] roSpec = this.GetRoSpecs().ROSpec;
            MSG_GET_READER_CONFIG_RESPONSE readerConfig1 = this.GetReaderConfig(ENUM_GetReaderConfigRequestedData.KeepaliveSpec);
            MSG_GET_READER_CONFIG_RESPONSE readerConfig2 = this.GetReaderConfig(ENUM_GetReaderConfigRequestedData.GPIPortCurrentState);
            MSG_GET_READER_CONFIG_RESPONSE readerConfig3 = this.GetReaderConfig(ENUM_GetReaderConfigRequestedData.GPOWriteData);
            MSG_GET_READER_CONFIG_RESPONSE readerConfig4 = this.GetReaderConfig(ENUM_GetReaderConfigRequestedData.EventsAndReports);
            MSG_GET_READER_CONFIG_RESPONSE readerConfig5 = this.GetReaderConfig(ENUM_GetReaderConfigRequestedData.ReaderEventNotificationSpec);
            LlrpReader.ValidateReaderSettings(roSpec, readerConfig1, readerConfig2, readerConfig3, readerConfig4);
            this.cachedReaderEventNotifications = this.ParseReaderEventNotifications(readerConfig5);
            Settings settings = this.ParseRoSpecAndConfig(roSpec[LlrpReader.RO_SPEC_INDEX], readerConfig1, readerConfig2, readerConfig3, readerConfig4);
            PARAM_AccessSpec[] accessSpecs = this.GetAccessSpecs().AccessSpec;
            this.LoadAttachedDataConfigFromAccessSpecs(settings, accessSpecs);
            return settings;
        }

        /// <summary>
        /// Query ReaderEventNotificationSpec and return actual event enable states from reader.
        /// </summary>
        /// <returns>Reader configured event notification states.</returns>
        public List<ReaderEventNotificationState> QueryReaderEventNotifications()
        {
            if (this.cachedReaderEventNotifications != null)
            {
                return new List<ReaderEventNotificationState>(this.cachedReaderEventNotifications);
            }

            MSG_GET_READER_CONFIG_RESPONSE readerConfig = this.GetReaderConfig(ENUM_GetReaderConfigRequestedData.ReaderEventNotificationSpec);
            List<ReaderEventNotificationState> states = this.ParseReaderEventNotifications(readerConfig);
            this.cachedReaderEventNotifications = states;
            return new List<ReaderEventNotificationState>(states);
        }

        private List<ReaderEventNotificationState> ParseReaderEventNotifications(MSG_GET_READER_CONFIG_RESPONSE readerConfig)
        {
            List<ReaderEventNotificationState> states = new List<ReaderEventNotificationState>();

            if (readerConfig?.ReaderEventNotificationSpec?.EventNotificationState == null)
            {
                return states;
            }

            foreach (PARAM_EventNotificationState eventState in readerConfig.ReaderEventNotificationSpec.EventNotificationState)
            {
                if (eventState == null)
                {
                    continue;
                }

                states.Add(new ReaderEventNotificationState()
                {
                    EventType = eventState.EventType,
                    IsEnabled = eventState.NotificationState
                });
            }

            return states;
        }

        private void LoadAttachedDataConfigFromAccessSpecs(Settings settings, PARAM_AccessSpec[] accessSpecs)
        {
            if (settings?.AttachedData == null)
                return;
            settings.AttachedData.Enabled = false;
            if (accessSpecs == null)
                return;
            foreach (PARAM_AccessSpec accessSpec in accessSpecs)
            {
                if (accessSpec == null)
                    continue;
                if (accessSpec.ROSpecID != (uint)LlrpReader.RO_SPEC_ID)
                    continue;
                if (accessSpec.AccessCommand?.AccessCommandOpSpec == null)
                    continue;
                PARAM_C1G2Read readOp = (PARAM_C1G2Read)null;
                int readOpCount = 0;
                for (int index = 0; index < accessSpec.AccessCommand.AccessCommandOpSpec.Count; ++index)
                {
                    IParameter parameter = accessSpec.AccessCommand.AccessCommandOpSpec[index];
                    if (parameter is PARAM_C1G2Read paramC1G2Read)
                    {
                        ++readOpCount;
                        readOp = paramC1G2Read;
                    }
                }
                if (readOpCount != 1 || readOp == null)
                    continue;
                settings.AttachedData.Enabled = true;
                settings.AttachedData.MemoryBank = (MemoryBank)readOp.MB.ToInt();
                settings.AttachedData.WordPointer = readOp.WordPointer;
                settings.AttachedData.WordCount = readOp.WordCount;
                settings.AttachedData.AccessPassword = TagData.FromUnsignedInt(readOp.AccessPassword).ToHexString();
                return;
            }
        }

        /// <summary>
        /// This method ensures that the list of RO Specs and the reader configuration that is returned when we query the reader's settings is valid.
        /// </summary>
        /// <param name="rospecs">The roSpecList to validate.</param>
        /// <param name="config1">The 1st reader config to validate.</param>
        /// <param name="config2">The 2nd reader config to validate.</param>
        /// <param name="config3">The 3rd reader config to validate.</param>
        /// <param name="config4">The 4th reader config to validate.</param>
        private static void ValidateReaderSettings(
          PARAM_ROSpec[] rospecs,
          MSG_GET_READER_CONFIG_RESPONSE config1,
          MSG_GET_READER_CONFIG_RESPONSE config2,
          MSG_GET_READER_CONFIG_RESPONSE config3,
          MSG_GET_READER_CONFIG_RESPONSE config4)
        {
            if (rospecs == null || rospecs.Length == 0 || rospecs[LlrpReader.RO_SPEC_INDEX] == null || config1 == null || config2 == null || config3 == null || config4 == null)
                throw new LLRPSdkException("The reader has not been configured.");
            if ((long)rospecs[LlrpReader.RO_SPEC_INDEX].ROSpecID != (long)LlrpReader.RO_SPEC_ID)
                throw new LLRPSdkException("The reader configuration is invalid.");
        }

        /// <summary>
        /// This function queries the reader for a summary of its current status.
        /// </summary>
        /// <returns>An object containing the current reader status.</returns>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">Thrown if a communication error
        /// occurs while talking with the reader.</exception>
        public Status QueryStatus()
        {
            if (this.reader == null)
                throw new LLRPSdkException("You must connect to the reader before query status.");
            Status status1 = new Status();
            MSG_GET_READER_CONFIG_RESPONSE identificationConfig = this.GetReaderConfig(ENUM_GetReaderConfigRequestedData.Identification);
            MSG_GET_READER_CONFIG_RESPONSE antennaConfig = this.GetReaderConfig(ENUM_GetReaderConfigRequestedData.AntennaProperties);
            MSG_GET_READER_CONFIG_RESPONSE gpiConfig = this.GetReaderConfig(ENUM_GetReaderConfigRequestedData.GPIPortCurrentState);
            MSG_GET_READER_CONFIG_RESPONSE gpoConfig = this.GetReaderConfig(ENUM_GetReaderConfigRequestedData.GPOWriteData);
            if(identificationConfig.Identification!=null)
            {
                //前3后3 组成 48位的Mac地址,目前是移除中间的FFFF字符串
                status1.ReaderIdentity=identificationConfig.Identification.ReaderID.ToHexString().Remove(6,4);
            }
            if (antennaConfig.AntennaProperties != null)
            {
                foreach (PARAM_AntennaProperties antennaProperty in antennaConfig.AntennaProperties)
                {
                    AntennaStatus antennaStatus = new AntennaStatus()
                    {
                        PortNumber = antennaProperty.AntennaID,
                        IsConnected = antennaProperty.AntennaConnected
                    };
                    status1.Antennas.antennaStatuses.Add(antennaStatus);
                }
            }
            if (gpiConfig.GPIPortCurrentState != null)
            {
                foreach (PARAM_GPIPortCurrentState portCurrentState in gpiConfig.GPIPortCurrentState)
                {
                    GpiStatus gpiStatus = new GpiStatus()
                    {
                        PortNumber = portCurrentState.GPIPortNum,
                        State = portCurrentState.State == ENUM_GPIPortState.High
                    };
                    status1.Gpis.gpiStatuses.Add(gpiStatus);
                }
            }
            if (gpoConfig.GPOWriteData != null)
            {
                foreach (PARAM_GPOWriteData gpoWriteData in gpoConfig.GPOWriteData)
                {
                    GpoStatus gpoStatus = new GpoStatus()
                    {
                        PortNumber = gpoWriteData.GPOPortNumber,
                        State = gpoWriteData.GPOData
                    };
                    status1.GpoStates.gpoStatuses.Add(gpoStatus);
                }
            }
            status1.IsConnected = this.reader.IsConnected;
            //status1.TemperatureInCelsius = this.GetTemperature(readerConfig);
            PARAM_ROSpec[] roSpec = this.GetRoSpecs().ROSpec;
            status1.IsSingulating = roSpec != null && roSpec[0].CurrentState == ENUM_ROSpecState.Active;

            return status1;
        }

        private void DeleteAccessSpecs()
        {
            MSG_ERROR_MESSAGE msg_err;
            MSG_DELETE_ACCESSSPEC_RESPONSE rsp = this.reader != null ? this.reader.DELETE_ACCESSSPEC(new MSG_DELETE_ACCESSSPEC()
            {
                AccessSpecID = 0U
            }, out msg_err, this.MessageTimeout) : throw new LLRPSdkException("You must connect to the reader before deleting access specs.");
            string str = "DELETE_ACCESSSPEC";
            LlrpReader.CheckForNullReply(str, (Message)rsp, msg_err);
            LlrpReader.CheckLlrpReply(rsp.LLRPStatus, msg_err, str);
        }

        private void DisableRoSpec()
        {
            if (this.reader == null)
                throw new LLRPSdkException("You must connect to the reader before disabling rospec.");
            MSG_ERROR_MESSAGE msg_err;
            MSG_DISABLE_ROSPEC_RESPONSE rsp = this.reader.DISABLE_ROSPEC(new MSG_DISABLE_ROSPEC()
            {
                ROSpecID = (uint)LlrpReader.RO_SPEC_ID
            }, out msg_err, this.MessageTimeout);
            string str = "DISABLE_ROSPEC";
            LlrpReader.CheckForNullReply(str, (Message)rsp, msg_err);
            LlrpReader.CheckLlrpReply(rsp.LLRPStatus, msg_err, str);
        }

        /// <summary>
        /// This function is used to set the value of a GPO signal to the
        /// specified value. The output is set to high when state is true,
        /// and low when state is false.
        /// </summary>
        /// <param name="port">GPO port to set</param>
        /// <param name="state">Value to set GPO to</param>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">Thrown if a communication error
        /// occurs while talking with the reader.</exception>
        public void SetGpo(ushort port, bool state)
        {
            if (this.reader == null)
                throw new LLRPSdkException("You must connect to the reader before setting GPO.");
            MSG_SET_READER_CONFIG msg = new MSG_SET_READER_CONFIG()
            {
                GPOWriteData = new PARAM_GPOWriteData[1]
            };
            msg.GPOWriteData[0] = new PARAM_GPOWriteData()
            {
                GPOPortNumber = port,
                GPOData = state
            };
            string str = "Set GPO";
            MSG_ERROR_MESSAGE msg_err;
            MSG_SET_READER_CONFIG_RESPONSE rsp = this.reader.SET_READER_CONFIG(msg, out msg_err, this.MessageTimeout);
            LlrpReader.CheckForNullReply(str, (Message)rsp, msg_err);
            LlrpReader.CheckLlrpReply(rsp.LLRPStatus, msg_err, str);
        }



        private void OnReaderEventInternal(MSG_READER_EVENT_NOTIFICATION readerEvent)
        {
            this.LogLlrpMessage($"RX READER_EVENT_NOTIFICATION (MsgId={readerEvent.MSG_ID})");
            if (readerEvent.ReaderEventNotificationData.GPIEvent != null)
            {
                GpiEvent e = new GpiEvent()
                {
                    PortNumber = readerEvent.ReaderEventNotificationData.GPIEvent.GPIPortNumber,
                    State = readerEvent.ReaderEventNotificationData.GPIEvent.GPIEvent
                };
                LlrpReader.GpiChangedHandler gpiChanged = this.GpiChanged;
                if (gpiChanged != null)
                    gpiChanged(this, e);
            }
            if (readerEvent.ReaderEventNotificationData.AntennaEvent != null)
            {
                AntennaEvent e = new AntennaEvent()
                {
                    PortNumber = readerEvent.ReaderEventNotificationData.AntennaEvent.AntennaID,
                    State = (AntennaEventType)readerEvent.ReaderEventNotificationData.AntennaEvent.EventType
                };
                LlrpReader.AntennaEventHandler antennaChanged = this.AntennaChanged;
                if (antennaChanged != null)
                    antennaChanged(this, e);
            }
            if (readerEvent.ReaderEventNotificationData.AISpecEvent != null && readerEvent.ReaderEventNotificationData.AISpecEvent.EventType == ENUM_AISpecEventType.End_Of_AISpec)
            {
                LlrpReader.EndOfCycleEventHandler endOfCycle = this.EndOfCycle;
                if (endOfCycle != null)
                    endOfCycle(this);
            }
            if (readerEvent.ReaderEventNotificationData.ReportBufferLevelWarningEvent != null)
            {
                ReportBufferWarningEvent e = new ReportBufferWarningEvent()
                {
                    PercentFull = readerEvent.ReaderEventNotificationData.ReportBufferLevelWarningEvent.ReportBufferPercentageFull
                };
                LlrpReader.ReportBufferWarningEventHandler reportBufferWarning = this.ReportBufferWarning;
                if (reportBufferWarning != null)
                    reportBufferWarning(this, e);
            }
            if (readerEvent.ReaderEventNotificationData.ReportBufferLevelWarningEvent != null)
            {
                ReportBufferOverflowEvent e = new ReportBufferOverflowEvent();
                LlrpReader.ReportBufferOverflowEventHandler reportBufferOverflow = this.ReportBufferOverflow;
                if (reportBufferOverflow != null)
                    reportBufferOverflow(this, e);
            }
            if (readerEvent.ReaderEventNotificationData.ROSpecEvent != null)
            {
                RoSpecEvent roSpecEvent = new RoSpecEvent()
                {
                    Event = (RoSpecEventType)readerEvent.ReaderEventNotificationData.ROSpecEvent.EventType
                };
                if (roSpecEvent.Event == RoSpecEventType.StartOfROSpec)
                {
                    LlrpReader.ReaderStartedEventHandler readerStarted = this.ReaderStarted;
                    if (readerStarted == null)
                        return;
                    readerStarted(this, new ReaderStartedEvent());
                }
                else
                {
                    if (roSpecEvent.Event != RoSpecEventType.EndOfROSpec)
                        return;
                    LlrpReader.ReaderStoppedEventHandler readerStopped = this.ReaderStopped;
                    if (readerStopped == null)
                        return;
                    readerStopped(this, new ReaderStoppedEvent());
                }
            }
            if (readerEvent.ReaderEventNotificationData.ConnectionAttemptEvent != null)
            {

            }
            if(readerEvent.ReaderEventNotificationData.ConnectionCloseEvent!=null)
            {

            }
            if(readerEvent.ReaderEventNotificationData.ReaderExceptionEvent!=null)
            {
            }
        }

        /// <summary>Convert an LLRPBitArray to a list of shorts</summary>
        /// <param name="bits"></param>
        /// <returns></returns>
        private static List<ushort> BitArrayToList(LLRPBitArray bits)
        {
            int count = bits.Count;
            List<ushort> list = new List<ushort>();
            if (count == 0)
                return list;
            int index = 0;
            int num = 0;
            for (; index < count; ++index)
            {
                if (bits[index])
                    num |= 1;
                if (index == 0 && count != 1 || (index + 1) % 16 > 0 && index != count - 1)
                {
                    num <<= 1;
                }
                else
                {
                    list.Add((ushort)num);
                    num = 0;
                }
            }
            return list;
        }

        private static List<ushort> UInt16ArrayToList(UInt16Array array)
        {
            List<ushort> list = new List<ushort>();
            for (int index = 0; index < array.Count; ++index)
                list.Add(array[index]);
            return list;
        }

        private void EnableAccessSpec(uint id)
        {
            if (this.reader == null)
                throw new LLRPSdkException("You must connect to the reader before enabling access spec.");
            MSG_ERROR_MESSAGE msg_err;
            MSG_ENABLE_ACCESSSPEC_RESPONSE rsp = this.reader.ENABLE_ACCESSSPEC(new MSG_ENABLE_ACCESSSPEC()
            {
                AccessSpecID = id
            }, out msg_err, this.MessageTimeout);
            string str = "ENABLE_ACCESSSPEC";
            LlrpReader.CheckForNullReply(str, (Message)rsp, msg_err);
            LlrpReader.CheckLlrpReply(rsp.LLRPStatus, msg_err, str);
        }

        private static void ThrowTimeoutException(string msgType)
        {
            throw new LLRPSdkException("A timeout occurred while sending the message : " + msgType);
        }

        private static void ThrowParsingException(string msgType, MSG_ERROR_MESSAGE msgErr)
        {
            throw new LLRPSdkException("A error occurred while parsing the message reply : " + msgType + " : " + msgErr.LLRPStatus.StatusCode.ToString() + " : " + msgErr.LLRPStatus.ErrorDescription);
        }

        private static void CheckLlrpReply(
          PARAM_LLRPStatus status,
          MSG_ERROR_MESSAGE msgErr,
          string messageType)
        {
            if (status.StatusCode != ENUM_StatusCode.M_Success)
                throw new LLRPSdkException("Error while sending message " + messageType + " : " + status.StatusCode.ToString() + " : " + status.ErrorDescription);
        }

        private static void CheckForNullReply(string msgType, Message rsp, MSG_ERROR_MESSAGE msgErr)
        {
            if (rsp != null)
                return;
            if (msgErr == null)
                LlrpReader.ThrowTimeoutException(msgType);
            else
                LlrpReader.ThrowParsingException(msgType, msgErr);
        }

        /// <summary>
        /// This new method is for compatibility with v1.0. Remove in a future release.
        /// Asynchronous operations should be encouraged. This method has been made
        /// private because it should not be called by users of the SDK.
        /// </summary>
        /// <param name="accessParams">A tag programming configuration object.</param>
        /// <returns>The result of the write operation</returns>
        private ProgramTagMemoryResult ProgramTagMemory(ProgramTagMemoryParams accessParams)
        {
            ProgramTagMemoryResult programTagMemoryResult = new ProgramTagMemoryResult();
            if (accessParams.AccessPassword == null || accessParams.AccessPassword == "")
                accessParams.AccessPassword = "00000000";
            this.syncProgramOpStatus = new SyncProgramOpStatus();
            this.TagOpComplete += new LlrpReader.TagOpCompleteHandler(this.OnSyncProgramOpComplete);
            TagOpSequence sequence = new TagOpSequence()
            {
                ExecutionCount = 1,
                TargetTag = new TargetTag()
                {
                    MemoryBank = MemoryBank.Epc,
                    Data = accessParams.TargetTag,
                    BitPointer = 32
                },
                AntennaId = accessParams.AntennaPortNumber,
                State = SequenceState.Active
            };
            if (accessParams.BlockWriteWordCount > (ushort)0)
            {
                sequence.BlockWriteEnabled = true;
                sequence.BlockWriteRetryCount = accessParams.RetryCount;
                sequence.BlockWriteWordCount = accessParams.BlockWriteWordCount;
            }
            TagWriteOp tagWriteOp = new TagWriteOp()
            {
                AccessPassword = TagData.FromHexString(accessParams.AccessPassword)
            };
            switch (accessParams)
            {
                case ProgramUserMemoryParams _:
                    ProgramUserMemoryParams userMemoryParams = accessParams as ProgramUserMemoryParams;
                    tagWriteOp.Data = TagData.FromHexString(userMemoryParams.NewUserBlock);
                    tagWriteOp.MemoryBank = MemoryBank.User;
                    tagWriteOp.WordPointer = userMemoryParams.WordPointer;
                    break;
                case ProgramAccessPasswordParams _:
                    ProgramAccessPasswordParams accessPasswordParams = accessParams as ProgramAccessPasswordParams;
                    tagWriteOp.Data = TagData.FromHexString(accessPasswordParams.NewAccessPassword);
                    tagWriteOp.MemoryBank = MemoryBank.Reserved;
                    tagWriteOp.WordPointer = (ushort)2;
                    break;
                case ProgramKillPasswordParams _:
                    ProgramKillPasswordParams killPasswordParams = accessParams as ProgramKillPasswordParams;
                    tagWriteOp.Data = TagData.FromHexString(killPasswordParams.NewKillPassword);
                    tagWriteOp.MemoryBank = MemoryBank.Reserved;
                    tagWriteOp.WordPointer = (ushort)0;
                    break;
                case ProgramEpcParams _:
                    ProgramEpcParams programEpcParams = accessParams as ProgramEpcParams;
                    tagWriteOp.Data = TagData.FromHexString(programEpcParams.NewEpc);
                    tagWriteOp.MemoryBank = MemoryBank.Epc;
                    tagWriteOp.WordPointer = (ushort)2;
                    break;
            }
            this.syncProgramOpStatus.WrittenData = tagWriteOp.Data;
            sequence.Ops.Add((TagOp)tagWriteOp);
            if (accessParams.IsWriteVerified)
            {
                this.syncProgramOpStatus.VerifyEnabled = true;
                TagReadOp tagReadOp = new TagReadOp()
                {
                    AccessPassword = TagData.FromHexString(accessParams.AccessPassword),
                    MemoryBank = tagWriteOp.MemoryBank,
                    WordCount = (ushort)tagWriteOp.Data.ToList().Count,
                    WordPointer = tagWriteOp.WordPointer
                };
                sequence.Ops.Add((TagOp)tagReadOp);
            }
            if (accessParams.LockType != TagLockState.None)
            {
                TagLockOp tagLockOp = new TagLockOp()
                {
                    AccessPassword = TagData.FromHexString(accessParams.AccessPassword)
                };
                switch (accessParams)
                {
                    case ProgramUserMemoryParams _:
                        tagLockOp.UserLockType = TagLockState.Lock;
                        break;
                    case ProgramAccessPasswordParams _:
                        tagLockOp.AccessPasswordLockType = TagLockState.Lock;
                        break;
                    case ProgramKillPasswordParams _:
                        tagLockOp.KillPasswordLockType = TagLockState.Lock;
                        break;
                    case ProgramEpcParams _:
                        tagLockOp.EpcLockType = TagLockState.Lock;
                        break;
                }
                sequence.Ops.Add((TagOp)tagLockOp);
            }
            this.AddOpSequence(sequence);
            bool flag = false;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (!flag)
            {
                if (stopwatch.ElapsedMilliseconds > (long)accessParams.TimeoutInMs)
                {
                    stopwatch.Stop();
                    throw new LLRPSdkException("A timeout occurred during the tag access operation.");
                }
                lock (this.syncProgramOpStatus)
                {
                    if (!this.syncProgramOpStatus.AllOpsComplete)
                    {
                        if (!this.syncProgramOpStatus.ErrorOccurred)
                            goto label_28;
                    }
                    flag = true;
                    programTagMemoryResult = this.syncProgramOpStatus.Result;
                    break;
                }
            label_28:
                Thread.Sleep(100);
            }
            stopwatch.Stop();
            if (this.syncProgramOpStatus.ErrorOccurred)
                throw new LLRPSdkException("An error occurred during the tag access operation : " + this.syncProgramOpStatus.ErrorDescription);
            return programTagMemoryResult;
        }













        private void OnSyncProgramOpComplete(LlrpReader reader, TagOpReport results)
        {
            lock (this.syncProgramOpStatus)
            {
                foreach (TagOpResult result in results)
                {
                    switch (result)
                    {
                        case TagWriteOpResult _:
                            this.syncProgramOpStatus.Result.WriteResult = result as TagWriteOpResult;
                            this.syncProgramOpStatus.WriteOpComplete = true;
                            continue;
                        case TagLockOpResult _:
                            this.syncProgramOpStatus.Result.LockResult = result as TagLockOpResult;
                            this.syncProgramOpStatus.LockComplete = true;
                            continue;
                        case TagReadOpResult _:
                            this.syncProgramOpStatus.Result.VerifyResult = result as TagReadOpResult;
                            this.syncProgramOpStatus.VerifyComplete = true;
                            continue;
                        default:
                            continue;
                    }
                }
                if (!this.syncProgramOpStatus.AllOpsComplete && !this.syncProgramOpStatus.ErrorOccurred)
                    return;
                this.TagOpComplete -= new LlrpReader.TagOpCompleteHandler(this.OnSyncProgramOpComplete);
            }
        }

        /// <summary>
        /// This new method is for compatibility with v1.0. Remove in a future release.
        /// Asynchronous operations should be encouraged. This method has been made
        /// private because it should not be called by users of the SDK.
        /// </summary>
        /// <param name="accessParams">
        /// Object defining read memory specifics (e.g. access password or
        /// target tag).
        /// </param>
        /// <returns>The result of the read operation</returns>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">
        /// Thrown if a communication error occurs while talking with the
        /// reader or an invalid password was provided.
        /// </exception>
        private ReadTagMemoryResult ReadTagMemory(ReadTagMemoryParams accessParams)
        {
            ReadTagMemoryResult readTagMemoryResult = new ReadTagMemoryResult();
            if (accessParams.AccessPassword == null || accessParams.AccessPassword == "")
                accessParams.AccessPassword = "00000000";
            this.syncReadOpStatus = new SyncReadOpStatus();
            this.TagOpComplete += new LlrpReader.TagOpCompleteHandler(this.OnSyncReadOpComplete);
            TagOpSequence sequence = new TagOpSequence()
            {
                ExecutionCount = 1,
                TargetTag = new TargetTag()
                {
                    MemoryBank = MemoryBank.Epc,
                    Data = accessParams.TargetTag,
                    BitPointer = 32
                },
                AntennaId = accessParams.AntennaPortNumber,
                State = SequenceState.Active
            };
            TagReadOp tagReadOp = new TagReadOp()
            {
                AccessPassword = TagData.FromHexString(accessParams.AccessPassword)
            };
            switch (accessParams)
            {
                case ReadUserMemoryParams _:
                    ReadUserMemoryParams userMemoryParams = accessParams as ReadUserMemoryParams;
                    tagReadOp.MemoryBank = MemoryBank.User;
                    tagReadOp.WordPointer = userMemoryParams.WordPointer;
                    tagReadOp.WordCount = userMemoryParams.WordCount;
                    break;
                case ReadTidMemoryParams _:
                    ReadTidMemoryParams readTidMemoryParams = accessParams as ReadTidMemoryParams;
                    tagReadOp.MemoryBank = MemoryBank.Tid;
                    tagReadOp.WordPointer = readTidMemoryParams.WordPointer;
                    tagReadOp.WordCount = readTidMemoryParams.WordCount;
                    break;
                case ReadKillPasswordParams _:
                    tagReadOp.MemoryBank = MemoryBank.Reserved;
                    tagReadOp.WordPointer = (ushort)0;
                    tagReadOp.WordCount = (ushort)2;
                    break;
            }
            sequence.Ops.Add((TagOp)tagReadOp);
            this.AddOpSequence(sequence);
            bool flag = false;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (!flag)
            {
                if (stopwatch.ElapsedMilliseconds > (long)accessParams.TimeoutInMs)
                {
                    stopwatch.Stop();
                    throw new LLRPSdkException("A timeout occurred during the tag access operation.");
                }
                lock (this.syncReadOpStatus)
                {
                    if (!this.syncReadOpStatus.AllOpsComplete)
                    {
                        if (!this.syncReadOpStatus.ErrorOccurred)
                            goto label_16;
                    }
                    flag = true;
                    readTagMemoryResult = this.syncReadOpStatus.Result;
                    break;
                }
            label_16:
                Thread.Sleep(100);
            }
            stopwatch.Stop();
            if (this.syncReadOpStatus.ErrorOccurred)
                throw new LLRPSdkException("An error occurred during the tag access operation : " + this.syncReadOpStatus.ErrorDescription);
            return readTagMemoryResult;
        }







        private void OnSyncReadOpComplete(LlrpReader reader, TagOpReport results)
        {
            lock (this.syncReadOpStatus)
            {
                foreach (TagOpResult result in results)
                {
                    if (result is TagReadOpResult)
                    {
                        this.syncReadOpStatus.Result.ReadResult = result as TagReadOpResult;
                        this.syncReadOpStatus.ReadOpComplete = true;
                    }
                }
                if (!this.syncReadOpStatus.AllOpsComplete && !this.syncReadOpStatus.ErrorOccurred)
                    return;
                this.TagOpComplete -= new LlrpReader.TagOpCompleteHandler(this.OnSyncReadOpComplete);
            }
        }

        private void AddAccessSpec(TagOpSequence sequence)
        {
            if (this.reader == null)
                throw new LLRPSdkException("You must connect to the reader before adding access spec.");
            MSG_ADD_ACCESSSPEC msg = this.BuildAddAOSpecMessage(sequence);

            MSG_ERROR_MESSAGE msg_err;
            MSG_ADD_ACCESSSPEC_RESPONSE rsp = this.reader.ADD_ACCESSSPEC(msg, out msg_err, this.MessageTimeout);
            string str = "ADD_ACCESSSPEC";
            LlrpReader.CheckForNullReply(str, (Message)rsp, msg_err);
            LlrpReader.CheckLlrpReply(rsp.LLRPStatus, msg_err, str);
        }

        private MSG_ADD_ACCESSSPEC BuildAddAOSpecMessage(TagOpSequence sequence)
        {
            MSG_ADD_ACCESSSPEC msg = new MSG_ADD_ACCESSSPEC()
            {
                AccessSpec = new PARAM_AccessSpec()
                {
                    AntennaID = sequence.AntennaId,
                    ROSpecID = sequence.TargetTag?.Data == null ? (uint)LlrpReader.RO_SPEC_ID : 0U,
                    ProtocolID = ENUM_AirProtocols.EPCGlobalClass1Gen2,
                    CurrentState = ENUM_AccessSpecState.Disabled,
                    AccessSpecID = sequence.Id,
                    AccessCommand = new PARAM_AccessCommand()
                    {
                        AirProtocolTagSpec = new UNION_AirProtocolTagSpec()
                    },
                    AccessSpecStopTrigger = new PARAM_AccessSpecStopTrigger()
                }
            };
            if (sequence.SequenceStopTrigger == SequenceTriggerType.ExecutionCount)
            {
                msg.AccessSpec.AccessSpecStopTrigger.AccessSpecStopTrigger = ENUM_AccessSpecStopTriggerType.Operation_Count;
                msg.AccessSpec.AccessSpecStopTrigger.OperationCountValue = sequence.ExecutionCount;
            }
            else
                msg.AccessSpec.AccessSpecStopTrigger.AccessSpecStopTrigger = ENUM_AccessSpecStopTriggerType.Null;
            PARAM_C1G2TagSpec val1 = new PARAM_C1G2TagSpec()
            {
                C1G2TargetTag = new PARAM_C1G2TargetTag[1]
            };
            val1.C1G2TargetTag[0] = new PARAM_C1G2TargetTag();
            if (sequence.TargetTag?.Data != null)
            {
                if (sequence.TargetTag.Mask == null)
                {
                    sequence.TargetTag.Mask = "";
                    for (int index = 0; index < sequence.TargetTag.Data.Length; ++index)
                        sequence.TargetTag.Mask += "F";
                }
                val1.C1G2TargetTag[0].Match = true;
                val1.C1G2TargetTag[0].MB = new TwoBits((ushort)sequence.TargetTag.MemoryBank);
                val1.C1G2TargetTag[0].Pointer = sequence.TargetTag.BitPointer;
                val1.C1G2TargetTag[0].TagData = LLRPBitArray.FromHexString(sequence.TargetTag.Data);
                val1.C1G2TargetTag[0].TagMask = LLRPBitArray.FromHexString(sequence.TargetTag.Mask);
            }
            else
                val1.C1G2TargetTag[0].Match = true;
            msg.AccessSpec.AccessCommand.AirProtocolTagSpec.Add((IParameter)val1);
            msg.AccessSpec.AccessCommand.AccessCommandOpSpec = new UNION_AccessCommandOpSpec();
            foreach (TagOp op in sequence.Ops)
            {
                switch (op)
                {
                    case TagReadOp tagReadOp:
                        PARAM_C1G2Read val2 = new PARAM_C1G2Read()
                        {
                            AccessPassword = tagReadOp.AccessPassword.ToUnsignedInt(),
                            MB = new TwoBits((ushort)tagReadOp.MemoryBank),
                            OpSpecID = tagReadOp.Id,
                            WordCount = tagReadOp.WordCount,
                            WordPointer = tagReadOp.WordPointer
                        };
                        msg.AccessSpec.AccessCommand.AccessCommandOpSpec.Add((IParameter)val2);
                        continue;
                    case TagWriteOp tagWriteOp:
                        if (sequence.BlockWriteEnabled)
                        {
                            PARAM_C1G2BlockWrite val3 = new PARAM_C1G2BlockWrite()
                            {
                                AccessPassword = tagWriteOp.AccessPassword.ToUnsignedInt(),
                                MB = new TwoBits((ushort)tagWriteOp.MemoryBank),
                                OpSpecID = tagWriteOp.Id,
                                WriteData = UInt16Array.FromHexString(tagWriteOp.Data.ToHexString()),
                                WordPointer = tagWriteOp.WordPointer
                            };
                            msg.AccessSpec.AccessCommand.AccessCommandOpSpec.Add((IParameter)val3);
                            continue;
                        }
                        PARAM_C1G2Write val4 = new PARAM_C1G2Write()
                        {
                            AccessPassword = tagWriteOp.AccessPassword.ToUnsignedInt(),
                            MB = new TwoBits((ushort)tagWriteOp.MemoryBank),
                            OpSpecID = tagWriteOp.Id,
                            WriteData = UInt16Array.FromHexString(tagWriteOp.Data.ToHexString()),
                            WordPointer = tagWriteOp.WordPointer
                        };
                        msg.AccessSpec.AccessCommand.AccessCommandOpSpec.Add((IParameter)val4);
                        continue;
                    case TagLockOp tagLockOp:
                        PARAM_C1G2Lock val5 = new PARAM_C1G2Lock();
                        val5.OpSpecID = tagLockOp.LockCount != (ushort)0 ? tagLockOp.Id : throw new LLRPSdkException("The TagLockOp does not specify any lock operations.");
                        val5.AccessPassword = tagLockOp.AccessPassword.ToUnsignedInt();
                        val5.C1G2LockPayload = new PARAM_C1G2LockPayload[(int)tagLockOp.LockCount];
                        int index = 0;
                        if (tagLockOp.KillPasswordLockType != TagLockState.None)
                        {
                            PARAM_C1G2LockPayload paramC1G2LockPayload = new PARAM_C1G2LockPayload()
                            {
                                Privilege = (ENUM_C1G2LockPrivilege)tagLockOp.KillPasswordLockType,
                                DataField = ENUM_C1G2LockDataField.Kill_Password
                            };
                            val5.C1G2LockPayload[index] = paramC1G2LockPayload;
                            ++index;
                        }
                        if (tagLockOp.AccessPasswordLockType != TagLockState.None)
                        {
                            PARAM_C1G2LockPayload paramC1G2LockPayload = new PARAM_C1G2LockPayload()
                            {
                                Privilege = (ENUM_C1G2LockPrivilege)tagLockOp.AccessPasswordLockType,
                                DataField = ENUM_C1G2LockDataField.Access_Password
                            };
                            val5.C1G2LockPayload[index] = paramC1G2LockPayload;
                            ++index;
                        }
                        if (tagLockOp.EpcLockType != TagLockState.None)
                        {
                            PARAM_C1G2LockPayload paramC1G2LockPayload = new PARAM_C1G2LockPayload()
                            {
                                Privilege = (ENUM_C1G2LockPrivilege)tagLockOp.EpcLockType,
                                DataField = ENUM_C1G2LockDataField.EPC_Memory
                            };
                            val5.C1G2LockPayload[index] = paramC1G2LockPayload;
                            ++index;
                        }
                        if (tagLockOp.TidLockType != TagLockState.None)
                        {
                            PARAM_C1G2LockPayload paramC1G2LockPayload = new PARAM_C1G2LockPayload()
                            {
                                Privilege = (ENUM_C1G2LockPrivilege)tagLockOp.TidLockType,
                                DataField = ENUM_C1G2LockDataField.TID_Memory
                            };
                            val5.C1G2LockPayload[index] = paramC1G2LockPayload;
                            ++index;
                        }
                        if (tagLockOp.UserLockType != TagLockState.None)
                        {
                            PARAM_C1G2LockPayload paramC1G2LockPayload = new PARAM_C1G2LockPayload()
                            {
                                Privilege = (ENUM_C1G2LockPrivilege)tagLockOp.UserLockType,
                                DataField = ENUM_C1G2LockDataField.User_Memory
                            };
                            val5.C1G2LockPayload[index] = paramC1G2LockPayload;
                        }
                        msg.AccessSpec.AccessCommand.AccessCommandOpSpec.Add((IParameter)val5);
                        continue;
                    case TagKillOp tagKillOp:
                        PARAM_C1G2Kill val6 = new PARAM_C1G2Kill()
                        {
                            KillPassword = tagKillOp.KillPassword.ToUnsignedInt(),
                            OpSpecID = tagKillOp.Id
                        };
                        msg.AccessSpec.AccessCommand.AccessCommandOpSpec.Add((IParameter)val6);
                        continue;
                    case TagBlockEraseOp tagBlockEraseOp:
                        PARAM_C1G2BlockErase val7 = new PARAM_C1G2BlockErase()
                        {
                            AccessPassword = tagBlockEraseOp.AccessPassword.ToUnsignedInt(),
                            MB = new TwoBits((ushort)tagBlockEraseOp.MemoryBank),
                            OpSpecID = tagBlockEraseOp.Id,
                            WordPointer = tagBlockEraseOp.WordPointer,
                            WordCount = tagBlockEraseOp.WordCount
                        };
                        msg.AccessSpec.AccessCommand.AccessCommandOpSpec.Add((IParameter)val7);
                        continue;
                    default:
                        continue;
                }
            }
            msg.AccessSpec.AccessReportSpec = new PARAM_AccessReportSpec()
            {
                AccessReportTrigger = ENUM_AccessReportTriggerType.Whenever_ROReport_Is_Generated
            };
            return msg;
        }

        private void OnKeepAliveInternal(MSG_KEEPALIVE msg)
        {
            this.LogLlrpMessage($"RX KEEPALIVE (MsgId={msg.MSG_ID})");
            this.keepaliveTimer.Stop();
            this.keepaliveTimer.Start();
            if (this.reader != null)
            {
                MSG_KEEPALIVE_ACK msg1 = new MSG_KEEPALIVE_ACK();
                msg1.MSG_ID = msg.MSG_ID;
                this.reader.KEEPALIVE_ACK(msg1, out MSG_ERROR_MESSAGE _, this.MessageTimeout);
            }
            LlrpReader.KeepaliveHandler keepaliveReceived = this.KeepaliveReceived;
            if (keepaliveReceived == null)
                return;
            keepaliveReceived(this);
        }

        private void OnKeepaliveMissed(object sender, ElapsedEventArgs e)
        {
            LlrpReader.ConnectionLostHandler connectionLost = this.ConnectionLost;
            if (connectionLost == null)
                return;
            connectionLost(this);
        }

        private void OnTagReportAvailableInternal(MSG_RO_ACCESS_REPORT msg)
        {
            List<string> stringList = new List<string>();
            if (msg.TagReportData != null && msg.TagReportData.Length != 0)
            {
                this.LogLlrpMessage($"RX RO_ACCESS_REPORT (MsgId={msg.MSG_ID}) tags={msg.TagReportData.Length}");
                TagReport report = new TagReport();
                TagOpReport results = new TagOpReport();
                for (int index1 = 0; index1 < msg.TagReportData.Length; ++index1)
                {
                    Tag tag = new Tag();
                    if (msg.TagReportData[index1].EPCParameter.Count > 0)
                    {
                        List<ushort> data = !(msg.TagReportData[index1].EPCParameter[0].GetType() == typeof(PARAM_EPC_96)) ? LlrpReader.BitArrayToList(((PARAM_EPCData)msg.TagReportData[index1].EPCParameter[0]).EPC) : LlrpReader.BitArrayToList(((PARAM_EPC_96)msg.TagReportData[index1].EPCParameter[0]).EPC);
                        tag.Epc = TagData.FromWordList(data);
                        if (msg.TagReportData[index1].AntennaID != null)
                        {
                            tag.AntennaPortNumber = msg.TagReportData[index1].AntennaID.AntennaID;
                            tag.IsAntennaPortNumberPresent = true;
                        }
                        if (msg.TagReportData[index1].ChannelIndex != null)
                        {
                            tag.ChannelInMhz = this._readerCapabilities.TxFrequencies[(int)msg.TagReportData[index1].ChannelIndex.ChannelIndex - 1];
                            tag.IsChannelInMhzPresent = true;
                        }
                        if (msg.TagReportData[index1].FirstSeenTimestampUTC != null)
                        {
                            tag.FirstSeenTime = new Timestamp(msg.TagReportData[index1].FirstSeenTimestampUTC.Microseconds);
                            tag.IsFirstSeenTimePresent = true;
                        }
                        if (msg.TagReportData[index1].LastSeenTimestampUTC != null)
                        {
                            tag.LastSeenTime = new Timestamp(msg.TagReportData[index1].LastSeenTimestampUTC.Microseconds);
                            tag.IsLastSeenTimePresent = true;
                        }
                        if (msg.TagReportData[index1].TagSeenCount != null)
                        {
                            tag.TagSeenCount = msg.TagReportData[index1].TagSeenCount.TagCount;
                            tag.IsSeenCountPresent = true;
                        }
                        if (msg.TagReportData[index1].PeakRSSI != null)
                        {
                            tag.PeakRssi = (double)msg.TagReportData[index1].PeakRSSI.PeakRSSI;
                            tag.IsPeakRssiPresent = true;
                        }
                        int length = msg.TagReportData[index1].AirProtocolTagData.Length;
                        for (int index2 = 0; index2 < length; ++index2)
                        {
                            if (msg.TagReportData[index1].AirProtocolTagData[index2] is PARAM_C1G2_CRC paramC1G2Crc)
                            {
                                tag.Crc = paramC1G2Crc.CRC;
                                tag.IsCrcPresent = true;
                            }
                            else if (msg.TagReportData[index1].AirProtocolTagData[index2] is PARAM_C1G2_PC paramC1G2Pc)
                            {
                                tag.PcBits = paramC1G2Pc.PC_Bits;
                                tag.IsPcBitsPresent = true;
                            }
                        }
                        report.Tags.Add(tag);
                    }
                    if (msg.TagReportData[index1].AccessCommandOpSpecResult != null)
                    {
                        int length = msg.TagReportData[index1].AccessCommandOpSpecResult.Length;
                        for (int index4 = 0; index4 < length; ++index4)
                        {
                            uint accessSpecId = msg.TagReportData[index1].AccessSpecID.AccessSpecID;
                            if (msg.TagReportData[index1].AccessCommandOpSpecResult[index4] is PARAM_C1G2ReadOpSpecResult)
                            {
                                PARAM_C1G2ReadOpSpecResult readOpSpecResult = msg.TagReportData[index1].AccessCommandOpSpecResult[index4] as PARAM_C1G2ReadOpSpecResult;
                                TagReadOpResult tagReadOpResult1 = new TagReadOpResult();
                                tagReadOpResult1.Result = (ReadResultStatus)readOpSpecResult.Result;
                                tagReadOpResult1.OpId = readOpSpecResult.OpSpecID;
                                tagReadOpResult1.SequenceId = accessSpecId;
                                TagReadOpResult tagReadOpResult2 = tagReadOpResult1;
                                if (readOpSpecResult.Result == ENUM_C1G2ReadResultType.Success)
                                    tagReadOpResult2.Data = TagData.FromWordList(readOpSpecResult.ReadData.data);
                                tagReadOpResult2.Tag = tag;
                                tag.ReadOperationResults.Add(tagReadOpResult2); // 附加数据直接关联到标签
                                results.Results.Add((TagOpResult)tagReadOpResult2);
                            }
                            else if (msg.TagReportData[index1].AccessCommandOpSpecResult[index4] is PARAM_C1G2WriteOpSpecResult)
                            {
                                PARAM_C1G2WriteOpSpecResult writeOpSpecResult = msg.TagReportData[index1].AccessCommandOpSpecResult[index4] as PARAM_C1G2WriteOpSpecResult;
                                TagWriteOpResult tagWriteOpResult1 = new TagWriteOpResult();
                                tagWriteOpResult1.Result = (WriteResultStatus)writeOpSpecResult.Result;
                                tagWriteOpResult1.OpId = writeOpSpecResult.OpSpecID;
                                tagWriteOpResult1.SequenceId = accessSpecId;
                                tagWriteOpResult1.NumWordsWritten = writeOpSpecResult.NumWordsWritten;
                                tagWriteOpResult1.Tag = tag;
                                tagWriteOpResult1.IsBlockWrite = false;
                                TagWriteOpResult tagWriteOpResult2 = tagWriteOpResult1;
                                results.Results.Add((TagOpResult)tagWriteOpResult2);
                            }
                            else if (msg.TagReportData[index1].AccessCommandOpSpecResult[index4] is PARAM_C1G2BlockWriteOpSpecResult)
                            {
                                PARAM_C1G2BlockWriteOpSpecResult writeOpSpecResult = msg.TagReportData[index1].AccessCommandOpSpecResult[index4] as PARAM_C1G2BlockWriteOpSpecResult;
                                TagWriteOpResult tagWriteOpResult3 = new TagWriteOpResult();
                                tagWriteOpResult3.Result = (WriteResultStatus)writeOpSpecResult.Result;
                                tagWriteOpResult3.OpId = writeOpSpecResult.OpSpecID;
                                tagWriteOpResult3.SequenceId = accessSpecId;
                                tagWriteOpResult3.NumWordsWritten = writeOpSpecResult.NumWordsWritten;
                                tagWriteOpResult3.Tag = tag;
                                tagWriteOpResult3.IsBlockWrite = true;
                                TagWriteOpResult tagWriteOpResult4 = tagWriteOpResult3;
                                results.Results.Add((TagOpResult)tagWriteOpResult4);
                            }
                            else if (msg.TagReportData[index1].AccessCommandOpSpecResult[index4] is PARAM_C1G2BlockEraseOpSpecResult)
                            {
                                PARAM_C1G2BlockEraseOpSpecResult eraseOpSpecResult = msg.TagReportData[index1].AccessCommandOpSpecResult[index4] as PARAM_C1G2BlockEraseOpSpecResult;
                                TagBlockEraseOpResult tagBlockEraseOpResult = new TagBlockEraseOpResult();
                                tagBlockEraseOpResult.Result = (BlockEraseResultStatus)eraseOpSpecResult.Result;
                                tagBlockEraseOpResult.OpId = eraseOpSpecResult.OpSpecID;
                                tagBlockEraseOpResult.SequenceId = accessSpecId;
                                tagBlockEraseOpResult.Tag = tag;
                                results.Results.Add((TagOpResult)tagBlockEraseOpResult);
                            }
                            else if (msg.TagReportData[index1].AccessCommandOpSpecResult[index4] is PARAM_C1G2LockOpSpecResult)
                            {
                                PARAM_C1G2LockOpSpecResult lockOpSpecResult = msg.TagReportData[index1].AccessCommandOpSpecResult[index4] as PARAM_C1G2LockOpSpecResult;
                                TagLockOpResult tagLockOpResult1 = new TagLockOpResult();
                                tagLockOpResult1.OpId = lockOpSpecResult.OpSpecID;
                                tagLockOpResult1.SequenceId = accessSpecId;
                                tagLockOpResult1.Tag = tag;
                                tagLockOpResult1.Result = (LockResultStatus)lockOpSpecResult.Result;
                                TagLockOpResult tagLockOpResult2 = tagLockOpResult1;
                                results.Results.Add((TagOpResult)tagLockOpResult2);
                            }
                            else if (msg.TagReportData[index1].AccessCommandOpSpecResult[index4] is PARAM_C1G2KillOpSpecResult)
                            {
                                PARAM_C1G2KillOpSpecResult killOpSpecResult = msg.TagReportData[index1].AccessCommandOpSpecResult[index4] as PARAM_C1G2KillOpSpecResult;
                                TagKillOpResult tagKillOpResult1 = new TagKillOpResult();
                                tagKillOpResult1.OpId = killOpSpecResult.OpSpecID;
                                tagKillOpResult1.SequenceId = accessSpecId;
                                tagKillOpResult1.Tag = tag;
                                tagKillOpResult1.Result = (KillResultStatus)killOpSpecResult.Result;
                                TagKillOpResult tagKillOpResult2 = tagKillOpResult1;
                                results.Results.Add((TagOpResult)tagKillOpResult2);
                            }
                        }
                    }
                }
                if (report.Tags.Count > 0 && this.TagsReported != null)
                    this.TagsReported(this, report);
                if (results.Results.Count > 0 && this.TagOpComplete != null)
                    this.TagOpComplete(this, results);
            }
            if (msg.Custom == null)
                return;
        }

        /// <summary>
        /// Delegate declaration required to support declaration of TagsReported
        /// event.
        /// Internal use only - bind to the TagsReported event.
        /// </summary>
        /// <param name="reader">LLRPReader object.</param>
        /// <param name="report">TagReport object.</param>
        public delegate void TagsReportedHandler(LlrpReader reader, TagReport report);

        /// <summary>
        /// Delegate declaration required to support declaration of RawFrameReceived event.
        /// </summary>
        public delegate void RawFrameReceivedHandler(LlrpReader reader, byte[] raw);

        /// <summary>
        /// Delegate declaration required to support declaration of RawFrameSent event.
        /// </summary>
        public delegate void RawFrameSentHandler(LlrpReader reader, byte[] raw);

        /// <summary>
        /// Delegate declaration required to support declaration of LlrpMessageLogged event.
        /// </summary>
        public delegate void LlrpMessageLoggedHandler(LlrpReader reader, string message);

        /// <summary>
        /// Delegate declaration required to support declaration of GpiChanged
        /// event.
        /// Internal use only - bind to the GpiChanged event.
        /// </summary>
        /// <param name="reader">LLRPReader object.</param>
        /// <param name="e">GpiEvent object.</param>
        public delegate void GpiChangedHandler(LlrpReader reader, GpiEvent e);

        /// <summary>
        /// Delegate declaration required to support declaration of AntennaChanged
        /// event.
        /// Internal use only - bind to the AntennaChanged event.
        /// </summary>
        /// <param name="reader">LLRPReader object.</param>
        /// <param name="e">AntennaEvent object.</param>
        public delegate void AntennaEventHandler(LlrpReader reader, AntennaEvent e);

        /// <summary>
        /// Delegate declaration required to support declaration of AntennaStartEvent
        /// event.
        /// Internal use only - bind to the AntennaChanged event.
        /// </summary>
        /// <param name="reader">LLRPReader object.</param>
        /// <param name="e">AntennaEvent object.</param>
        public delegate void AntennaStartEventHandler(LlrpReader reader, AntennaEvent e);

        /// <summary>
        /// Delegate declaration required to support declaration of End_Of_AISpec, aka End Of Cycle
        /// event.
        /// Internal use only - bind to the AntennaChanged event.
        /// </summary>
        /// <param name="reader">LLRPReader object.</param>
        public delegate void EndOfCycleEventHandler(LlrpReader reader);

        /// <summary>
        /// Delegate declaration required to support declaration of
        /// ReportBufferWarning event.
        /// Internal use only - bind to the ReportBufferWarning event.
        /// </summary>
        /// <param name="reader">LLRPReader object.</param>
        /// <param name="e">ReportBufferWarningEvent object.</param>
        public delegate void ReportBufferWarningEventHandler(
          LlrpReader reader,
          ReportBufferWarningEvent e);

        /// <summary>
        /// Delegate declaration required to support declaration of
        /// ReportBufferOverflow event.
        /// Internal use only - bind to the ReportBufferOverflow event.
        /// </summary>
        /// <param name="reader">LLRPReader object.</param>
        /// <param name="e">ReportBufferOverflowEvent object.</param>
        public delegate void ReportBufferOverflowEventHandler(
          LlrpReader reader,
          ReportBufferOverflowEvent e);

        /// <summary>
        /// Delegate declaration required to support declaration of
        /// ReaderStarted event.
        /// Internal use only - bind to the ReaderStarted event.
        /// </summary>
        /// <param name="reader">LLRPReader object.</param>
        /// <param name="e">ReaderStarted object.</param>
        public delegate void ReaderStartedEventHandler(LlrpReader reader, ReaderStartedEvent e);

        /// <summary>
        /// Delegate declaration required to support declaration of
        /// ReaderStopped event.
        /// Internal use only - bind to the ReaderStopped event.
        /// </summary>
        /// <param name="reader">LLRPReader object.</param>
        /// <param name="e">ReaderStopped object.</param>
        public delegate void ReaderStoppedEventHandler(LlrpReader reader, ReaderStoppedEvent e);

        /// <summary>
        /// Delegate declaration required to support declaration of
        /// TagOpComplete event.
        /// Internal use only - bind to the TagOpCompete event.
        /// </summary>
        /// <param name="reader">LLRPReader object.</param>
        /// <param name="results">TagOpReport object.</param>
        public delegate void TagOpCompleteHandler(LlrpReader reader, TagOpReport results);

        /// <summary>
        /// Delegate declaration required to support declaration of
        /// KeepaliveReceived event.
        /// Internal use only - bind to KeepaliveReceived event.
        /// </summary>
        /// <param name="reader">LLRPReader object</param>
        public delegate void KeepaliveHandler(LlrpReader reader);

        /// <summary>
        /// Delegate declaration required to support declaration of
        /// ConnectionLostHandler event.
        /// Internal use only - bind to ConnectionLost event.
        /// </summary>
        /// <param name="reader">LLRPReader object</param>
        public delegate void ConnectionLostHandler(LlrpReader reader);

        /// <summary>
        /// Delegate declaration required to support declaration of
        /// Logging event.
        /// Internal use only - bind to Logging event.
        /// </summary>
        /// <param name="reader">LLRPReader</param>
        public delegate void LoggingHandler(LlrpReader reader);

        /// <summary>
        /// Delegate declaration required to support declaration of ConnectAsyncComplete event.
        /// Internal use only - bind to the ConnectAsyncComplete event.
        /// </summary>
        /// <param name="reader">
        /// If the connection attempt was successful, this contains a connected LLRPReader instance.
        /// If the connection failed, this contains a disconnected LLRPReader instance with only the address set.
        /// </param>
        /// <param name="result">The result of the connection attempt.</param>
        /// <param name="errorMessage">
        /// An error message, if an error occurred
        /// </param>
        public delegate void ConnectAsyncCompleteHandler(
          LlrpReader reader,
          ConnectAsyncResult result,
          string errorMessage);



        /// <summary>
        /// Delegate declaration required to support declaration of DiagnosticsReported event.
        /// Internal use only - bind to the DiagnosticsReported event.
        /// </summary>
        /// <param name="reader">The LLRPReader that raised the event.</param>
        /// <param name="report">The diagnostic report.</param>
        public delegate void DiagnosticsReportedHandler(LlrpReader reader, DiagnosticReport report);

        /// <summary>
        /// Delegate declaration required to support declaration of ErrorNotificationHandler event.
        /// Internal use only - bind to ErrorNotification event. Notifies unhandled exceptions, should clean-up, close SDK.
        /// </summary>
        /// <param name="reader">LLRPReader reader name</param>
        /// <param name="error">Exception object</param>
        public delegate void ErrorNotificationHandler(LlrpReader reader, Exception error);
    }
}
