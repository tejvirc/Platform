namespace Aristocrat.Bingo.Client.Messages
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Commands;
    using Grpc.Core;
    using log4net;
    using Polly;
    using ServerApiGateway;

    public sealed class ProgressiveCommandService :
        BaseClientCommunicationService<ProgressiveApi.ProgressiveApiClient>,
        IProgressiveCommandService,
        IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IClient<ProgressiveApi.ProgressiveApiClient> _progressiveClient;
        private readonly IProgressiveCommandProcessorFactory _processorFactory;
        private readonly IAsyncPolicy _commandRetryPolicy;
        private CancellationTokenSource _tokenSource;
        private bool _disposed;

        public TimeSpan BackOffTime { get; set; } = TimeSpan.FromMilliseconds(200);

        public ProgressiveCommandService(
            IClientEndpointProvider<ProgressiveApi.ProgressiveApiClient> endpointProvider,
            IClient<ProgressiveApi.ProgressiveApiClient> progressiveClient,
            IProgressiveCommandProcessorFactory processorFactory)
            : base(endpointProvider)
        {
            _progressiveClient = progressiveClient ?? throw new ArgumentNullException(nameof(progressiveClient));
            _processorFactory = processorFactory ?? throw new ArgumentNullException(nameof(processorFactory));
            _commandRetryPolicy = CreatePolicy();

            _progressiveClient.ConnectionStateChanged += HandleConnectionStateChanges;
        }

        public async Task<bool> HandleCommands(string machineSerial, int gameTitleId, CancellationToken cancellationToken)
        {
            _tokenSource?.Cancel();

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    using var source = _tokenSource = new CancellationTokenSource();
                    try
                    {
                        using var shared = CancellationTokenSource.CreateLinkedTokenSource(
                            source.Token,
                            cancellationToken);
                        await ProgressProgressiveCommands(machineSerial, cancellationToken, shared.Token)
                            .ConfigureAwait(false);
                    }
                    finally
                    {
                        _tokenSource = null;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Progressive command service exited.  IsCancelled={cancellationToken.IsCancellationRequested}", e);
                return false;
            }

            return true;
        }

        private AsyncDuplexStreamingCall<Progressive, ProgressiveUpdate> GetCommandHandler(CancellationToken cancellationToken)
        {
            try
            {
                return Invoke(
                    (client, token) => client.ProgressiveUpdates(cancellationToken: token),
                    cancellationToken);
            }
            catch (Exception e)
            {
                Logger.Error("Failed to get progressive command handler",e);
                return null;
            }
        }

        private async Task ProcessCommands(
            string machineSerial,
            IAsyncStreamReader<ProgressiveUpdate> responseStream,
            IAsyncStreamWriter<Progressive> requestStream,
            CancellationToken cancellationToken)
        {
            try
            {
                while (await responseStream.MoveNext(cancellationToken))
                {
                    var command = responseStream.Current;
                    await ProcessCommand(command, requestStream, machineSerial, cancellationToken);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Failed to process progressive commands", e);
            }
        }

        /// <summary>
        /// Send the progressive command to the server which starts the progressive updates to the EGM
        /// </summary>
        /// <param name="machineSerial">The machine serial number</param>
        /// <param name="requestStream"></param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns></returns>
        private static async Task<bool> StartProgressiveUpdates(
            string machineSerial,
            IClientStreamWriter<Progressive> requestStream,
            CancellationToken cancellationToken)
        {
            if (requestStream is null)
            {
                Logger.Error("Failed to start progressive update as request stream is null");
                return false;
            }

            try
            {
                await Respond(requestStream, machineSerial, cancellationToken);
            }
            catch (Exception e)
            {
                Logger.Error("Failed to process progressive command to start progressive updates", e);
                return false;
            }

            return true;
        }

        private async Task ProcessCommandsOnStream(
            string machineSerial,
            IClientStreamWriter<Progressive> requestStream,
            IAsyncStreamReader<ProgressiveUpdate> responseStream,
            CancellationToken cancellationToken)
        {
            try
            {
                await ProcessCommands(machineSerial, responseStream, requestStream, cancellationToken);
                Logger.Debug($"Progressive command service exited.  IsCancelled={cancellationToken.IsCancellationRequested}");

                await requestStream.CompleteAsync();
            }
            catch (Exception e)
            {
                Logger.Error($"Progressive command service exited.  IsCancelled={cancellationToken.IsCancellationRequested}", e);
            }
        }

        private static async Task Respond(
            IAsyncStreamWriter<Progressive> output,
            string machineSerial,
            CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var response = new Progressive
            {
                MachineSerial = machineSerial,
            };

            await output.WriteAsync(response);
        }

        private async Task ProcessCommand(
            ProgressiveUpdate command,
            IAsyncStreamWriter<Progressive> output,
            string machineSerial,
            CancellationToken token)
        {
            if (command is null)
            {
                return;
            }

            var result = await _processorFactory.ProcessCommand(command, token);
            if (result is not null)
            {
                await _commandRetryPolicy.ExecuteAsync(t => Respond(output, machineSerial, t), token);
            }
        }

        private async Task ProgressProgressiveCommands(
            string machineSerial,
            CancellationToken cancellationToken,
            CancellationToken token)
        {
            try
            {
                using var commandCaller = GetCommandHandler(cancellationToken);
                if (commandCaller is not null &&
                    await StartProgressiveUpdates(
                        machineSerial,
                        commandCaller.RequestStream,
                        token).ConfigureAwait(false))
                    {
                        await ProcessCommandsOnStream(
                            machineSerial,
                            commandCaller.RequestStream,
                            commandCaller.ResponseStream,
                            token).ConfigureAwait(false);
                    }

                await Task.Delay(BackOffTime, _tokenSource.Token).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                // Do nothing
            }
            catch (OperationCanceledException)
            {
                // Do nothing
            }
        }
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _progressiveClient.ConnectionStateChanged -= HandleConnectionStateChanges;
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = null;
            _disposed = true;
        }

        private void HandleConnectionStateChanges(object sender, ConnectionStateChangedEventArgs eventArgs)
        {
            if (eventArgs.State == RpcConnectionState.Disconnected || eventArgs.State == RpcConnectionState.Closed)
            {
                _tokenSource?.Cancel();
            }
        }
    }
}