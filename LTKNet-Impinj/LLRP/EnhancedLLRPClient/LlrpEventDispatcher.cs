using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;


namespace Org.LLRP.LTK.LLRPV1
{
    internal sealed class LlrpEventDispatcher : IDisposable
    {
        private readonly Channel<LlrpFrame> channel;
        private readonly Action<LlrpFrame> dispatchAction;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly Task workerTask;

        public LlrpEventDispatcher(Action<LlrpFrame> dispatchAction)
        {
            this.dispatchAction = dispatchAction ?? throw new ArgumentNullException(nameof(dispatchAction));
            this.channel = Channel.CreateUnbounded<LlrpFrame>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false
            });
            this.cancellationTokenSource = new CancellationTokenSource();
            this.workerTask = Task.Run(this.ProcessLoopAsync);
        }

        public bool Enqueue(in LlrpFrame frame)
        {
            return this.channel.Writer.TryWrite(frame);
        }

        public void Complete()
        {
            this.channel.Writer.TryComplete();
        }

        public void Dispose()
        {
            this.cancellationTokenSource.Cancel();
            this.channel.Writer.TryComplete();

            try
            {
                this.workerTask.Wait(TimeSpan.FromSeconds(1));
            }
            catch
            {
            }

            this.cancellationTokenSource.Dispose();
        }

        private async Task ProcessLoopAsync()
        {
            try
            {
                await foreach (LlrpFrame frame in this.channel.Reader.ReadAllAsync(this.cancellationTokenSource.Token).ConfigureAwait(false))
                {
                    try
                    {
                        this.dispatchAction(frame);
                    }
                    catch
                    {
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch
            {
            }
        }
    }
}