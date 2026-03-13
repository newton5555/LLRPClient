using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;


namespace Org.LLRP.LTK.LLRPV1
{
    /// <summary>
    /// 改进版 TCP 传输实现。
    /// 通过 System.IO.Pipelines + ReadOnlySequence 进行 LLRP 帧解析，
    /// 并通过独立事件投递层隔离业务回调。
    /// </summary>
    public class TCPIPClientOptimized : CommunicationInterface
    {
        private const int BUFFER_SIZE = 4096;
        private const int MAX_MESSAGE_SIZE = 4 * 1024 * 1024;

        private readonly object syncRoot = new object();

        /// <summary>
        /// 控制 TLS 握手期间的证书验证策略。
        /// 默认为 <see cref="TlsCertificateOptions.SkipVerification"/>，兼容携带自签名证书的读写器。
        /// 在开放网络场景中建议改为 <see cref="TlsCertificateOptions.StrictSystemValidation"/>
        /// 或 <see cref="TlsCertificateOptions.PinThumbprint"/>。
        /// 必须在调用 <see cref="Open"/> 之前设置。
        /// </summary>
        public TlsCertificateOptions CertificateOptions { get; set; } = TlsCertificateOptions.SkipVerification;

        private TcpClient tcpClient;
        private Stream networkStream;
        private Pipe llrpPipe;
        private LlrpEventDispatcher eventDispatcher;
        private CancellationTokenSource ioCancellationTokenSource;
        private Task receiveLoopTask;
        private Task parseLoopTask;
        private ConnectionStatus connectionStatus = ConnectionStatus.DISCONNECTED;

        public override bool Open(string device_name, int port) => this.Open(device_name, port, Timeout.Infinite);

        public override bool Open(string device_name, int port, bool useTLS)
        {
            return this.Open(device_name, port, Timeout.Infinite, useTLS);
        }

        public override bool Open(string device_name, int port, int timeout)
        {
            return this.Open(device_name, port, timeout, false);
        }

        public override bool Open(string device_name, int port, int timeout, bool useTLS)
        {
            return this.Open(device_name, port, timeout, useTLS, LLRPClient.TlsProtocols.OsDefault);
        }

        public override bool Open(
            string device_name,
            int port,
            int timeout,
            bool useTLS,
            LLRPClient.TlsProtocols tlsProtocol)
        {
            TcpClient client = null;
            Stream stream = null;

            lock (this.syncRoot)
            {
                if (this.connectionStatus != ConnectionStatus.DISCONNECTED)
                    return false;

                this.connectionStatus = ConnectionStatus.CONNECTING;
            }

            try
            {
                IPAddress[] hostAddresses = Dns.GetHostAddresses(device_name);
                if (hostAddresses == null || hostAddresses.Length == 0)
                    return this.FailOpen(client, stream);

                client = new TcpClient(AddressFamily.InterNetworkV6);
                client.Client.DualMode = true;

                if (!ConnectWithTimeout(client, hostAddresses, port, timeout))
                    return this.FailOpen(client, stream);

                stream = useTLS
                    ? this.CreateAuthenticatedStream(client, device_name, tlsProtocol)
                    : client.GetStream();

                lock (this.syncRoot)
                {
                    if (this.connectionStatus != ConnectionStatus.CONNECTING)
                        return this.FailOpen(client, stream);

                    this.tcpClient = client;
                    this.networkStream = stream;
                    this.StartIoPipelinesUnsafe();
                    this.connectionStatus = ConnectionStatus.CONNECTED;
                }

                this.TriggerOnClientConnectionStatusChange(ENUM_CONNECTION_STATUS.CONNECTED);
                return true;
            }
            catch
            {
                return this.FailOpen(client, stream);
            }
        }

        public override void Close()
        {
            Stream streamToDispose = null;
            TcpClient clientToDispose = null;
            CancellationTokenSource cancellationTokenSource = null;
            Task receiveTask = null;
            Task parseTask = null;
            LlrpEventDispatcher dispatcherToDispose = null;
            bool notifyDisconnected = false;

            lock (this.syncRoot)
            {
                if (this.connectionStatus == ConnectionStatus.DISCONNECTED)
                    return;

                notifyDisconnected = this.connectionStatus == ConnectionStatus.CONNECTED;
                this.connectionStatus = ConnectionStatus.DISCONNECTING;

                streamToDispose = this.networkStream;
                clientToDispose = this.tcpClient;
                cancellationTokenSource = this.ioCancellationTokenSource;
                receiveTask = this.receiveLoopTask;
                parseTask = this.parseLoopTask;
                dispatcherToDispose = this.eventDispatcher;

                this.networkStream = null;
                this.tcpClient = null;
                this.ioCancellationTokenSource = null;
                this.receiveLoopTask = null;
                this.parseLoopTask = null;
                this.eventDispatcher = null;
                this.llrpPipe = null;
                this.connectionStatus = ConnectionStatus.DISCONNECTED;
            }

            try
            {
                cancellationTokenSource?.Cancel();
            }
            catch
            {
            }

            try
            {
                streamToDispose?.Dispose();
            }
            catch
            {
            }

            try
            {
                clientToDispose?.Dispose();
            }
            catch
            {
            }

            this.WaitTaskQuietly(receiveTask);
            this.WaitTaskQuietly(parseTask);

            try
            {
                dispatcherToDispose?.Complete();
                dispatcherToDispose?.Dispose();
            }
            catch
            {
            }

            try
            {
                cancellationTokenSource?.Dispose();
            }
            catch
            {
            }

            if (notifyDisconnected)
                this.TriggerOnClientConnectionStatusChange(ENUM_CONNECTION_STATUS.DISCONNECTED);
        }

        public override int Send(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            bool shouldClose = false;
            int bytesWritten = 0;

            lock (this.syncRoot)
            {
                if (this.connectionStatus != ConnectionStatus.CONNECTED || this.networkStream == null)
                    return 0;

                try
                {
                    this.networkStream.Write(data, 0, data.Length);
                    bytesWritten = data.Length;
                }
                catch (IOException)
                {
                    shouldClose = true;
                }
                catch (InvalidOperationException)
                {
                    shouldClose = true;
                }
            }

            if (shouldClose)
            {
                this.Close();
                return -1;
            }

            try
            {
                this.TriggerRawSent(data);
            }
            catch
            {
            }

            return bytesWritten;
        }

        public override int Receive(out byte[] buffer)
        {
            buffer = null;
            throw new NotSupportedException("Synchronous receive unsupported");
        }

        public override void Dispose()
        {
            this.Close();
        }

        private bool FailOpen(TcpClient client, Stream stream)
        {
            try
            {
                stream?.Dispose();
            }
            catch
            {
            }

            try
            {
                client?.Dispose();
            }
            catch
            {
            }

            lock (this.syncRoot)
            {
                this.networkStream = null;
                this.tcpClient = null;
                this.ioCancellationTokenSource = null;
                this.receiveLoopTask = null;
                this.parseLoopTask = null;
                this.eventDispatcher = null;
                this.llrpPipe = null;
                this.connectionStatus = ConnectionStatus.DISCONNECTED;
            }

            return false;
        }

        private void StartIoPipelinesUnsafe()
        {
            if (this.networkStream == null)
                throw new InvalidOperationException("Network stream is not available.");

            this.llrpPipe = new Pipe(new PipeOptions(
                pauseWriterThreshold: 64 * 1024,
                resumeWriterThreshold: 32 * 1024,
                minimumSegmentSize: BUFFER_SIZE,
                useSynchronizationContext: false));

            this.eventDispatcher = new LlrpEventDispatcher(this.DispatchFrameToEvents);
            this.ioCancellationTokenSource = new CancellationTokenSource();

            this.receiveLoopTask = Task.Run(() => this.ReceiveToPipeLoopAsync(
                this.networkStream,
                this.llrpPipe.Writer,
                this.ioCancellationTokenSource.Token));

            this.parseLoopTask = Task.Run(() => this.ParseFromPipeLoopAsync(
                this.llrpPipe.Reader,
                this.eventDispatcher,
                this.ioCancellationTokenSource.Token));
        }

        private void DispatchFrameToEvents(LlrpFrame frame)
        {
            this.TriggerMessageEvent(frame.Version, frame.Type, frame.Id, frame.Data);
        }

        private async Task ReceiveToPipeLoopAsync(Stream stream, PipeWriter writer, CancellationToken cancellationToken)
        {
            bool closeRequired = false;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Memory<byte> memory = writer.GetMemory(BUFFER_SIZE);
                    int bytesRead = await stream.ReadAsync(memory, cancellationToken).ConfigureAwait(false);

                    if (bytesRead == 0)
                    {
                        closeRequired = true;
                        break;
                    }

                    writer.Advance(bytesRead);
                    FlushResult flushResult = await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
                    if (flushResult.IsCompleted || flushResult.IsCanceled)
                        break;
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch
            {
                closeRequired = true;
            }
            finally
            {
                try
                {
                    await writer.CompleteAsync().ConfigureAwait(false);
                }
                catch
                {
                }
            }

            if (closeRequired)
                this.RequestCloseFromBackground();
        }

        private async Task ParseFromPipeLoopAsync(
            PipeReader reader,
            LlrpEventDispatcher dispatcher,
            CancellationToken cancellationToken)
        {
            bool closeRequired = false;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    ReadResult readResult = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                    ReadOnlySequence<byte> sequence = readResult.Buffer;

                    try
                    {
                        while (LlrpFramePipeParser.TryReadFrame(ref sequence, MAX_MESSAGE_SIZE, out LlrpFrame frame))
                        {
                            if (!dispatcher.Enqueue(frame))
                            {
                                closeRequired = true;
                                break;
                            }
                        }
                    }
                    catch (MalformedPacket)
                    {
                        closeRequired = true;
                    }

                    reader.AdvanceTo(sequence.Start, sequence.End);

                    if (closeRequired || readResult.IsCanceled)
                        break;

                    if (readResult.IsCompleted)
                    {
                        if (sequence.Length > 0)
                            closeRequired = true;
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch
            {
                closeRequired = true;
            }
            finally
            {
                try
                {
                    await reader.CompleteAsync().ConfigureAwait(false);
                }
                catch
                {
                }
            }

            try
            {
                dispatcher.Complete();
            }
            catch
            {
            }

            if (closeRequired)
                this.RequestCloseFromBackground();
        }

        private static bool ConnectWithTimeout(TcpClient client, IPAddress[] addresses, int port, int timeout)
        {
            Exception connectException = null;

            using (ManualResetEventSlim connectCompleted = new ManualResetEventSlim(false))
            {
                client.BeginConnect(addresses, port, ar =>
                {
                    try
                    {
                        client.EndConnect(ar);
                    }
                    catch (Exception ex)
                    {
                        connectException = ex;
                    }
                    finally
                    {
                        connectCompleted.Set();
                    }
                }, null);

                bool completed = timeout <= 0
                    ? connectCompleted.Wait(Timeout.Infinite)
                    : connectCompleted.Wait(timeout);

                if (!completed)
                    return false;
            }

            if (connectException != null)
                throw new LLRPNetworkException(connectException.Message);

            return client.Connected;
        }

        private Stream CreateAuthenticatedStream(
            TcpClient client,
            string deviceName,
            LLRPClient.TlsProtocols tlsProtocol)
        {
            TlsCertificateOptions options = this.CertificateOptions ?? TlsCertificateOptions.SkipVerification;

            SslStream sslStream = new SslStream(
                client.GetStream(),
                false,
                options.Validate);

            try
            {
                sslStream.AuthenticateAsClient(new SslClientAuthenticationOptions
                {
                    TargetHost = deviceName,
                    EnabledSslProtocols = this.GetSslProtocol(tlsProtocol),
                    CertificateRevocationCheckMode = X509RevocationMode.NoCheck
                });

                return sslStream;
            }
            catch (AuthenticationException ex)
            {
                sslStream.Dispose();
                throw new LLRPNetworkException(ex.Message);
            }
            catch
            {
                sslStream.Dispose();
                throw;
            }
        }

        private void RequestCloseFromBackground()
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    this.Close();
                }
                catch
                {
                }
            });
        }

        private void WaitTaskQuietly(Task task)
        {
            if (task == null)
                return;

            try
            {
                task.Wait(TimeSpan.FromSeconds(1));
            }
            catch
            {
            }
        }

        private SslProtocols GetSslProtocol(LLRPClient.TlsProtocols tlsProtocol)
        {
            return tlsProtocol switch
            {
                LLRPClient.TlsProtocols.Tls12 => SslProtocols.Tls12,
                LLRPClient.TlsProtocols.Tls13 => SslProtocols.Tls13,
                _ => SslProtocols.None
            };
        }

        private enum ConnectionStatus
        {
            DISCONNECTED,
            CONNECTING,
            CONNECTED,
            DISCONNECTING,
        }
    }
}
