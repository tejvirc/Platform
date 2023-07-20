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

    public class ProgressiveCommandService :
        BaseClientCommunicationService<ProgressiveApi.ProgressiveApiClient>,
        IProgressiveCommandService,
        IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IProgressiveCommandProcessorFactory _processorFactory;
        private readonly IAsyncPolicy _commandRetryPolicy;
        private readonly IProgressiveAuthorizationProvider _authorization;
        private CancellationTokenSource _tokenSource;
        private AsyncDuplexStreamingCall<Progressive, ProgressiveUpdate> _commandHandler;
        private IAsyncStreamReader<ProgressiveUpdate> _responseStream;
        private IClientStreamWriter<Progressive> _requestStream;
        private bool _disposed;

        public TimeSpan BackOffTime { get; set; } = TimeSpan.FromMilliseconds(200);

        public ProgressiveCommandService(
            IClientEndpointProvider<ProgressiveApi.ProgressiveApiClient> endpointProvider,
            IProgressiveCommandProcessorFactory processorFactory,
            IProgressiveAuthorizationProvider authorization)
            : base(endpointProvider)
        {
            _processorFactory = processorFactory ?? throw new ArgumentNullException(nameof(processorFactory));
            _authorization = authorization ?? throw new ArgumentNullException(nameof(authorization));
            _commandRetryPolicy = CreatePolicy();
        }

        public async Task<bool> HandleCommands(string machineSerial, int gameTitleId, CancellationToken cancellationToken)
        {
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

                        if (OpenStream(shared.Token).Result)
                        {
                            if (StartProgressiveUpdates(machineSerial, shared.Token).Result)
                            {
                                await ProcessCommandsOnStream(machineSerial, shared.Token);
                            }
                        }

                        await Task.Delay(BackOffTime, cancellationToken);
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

        private async Task ProcessCommands(string machineSerial, CancellationToken cancellationToken)
        {
            try
            {
                while (await _responseStream.MoveNext(cancellationToken))
                {
                    var command = _responseStream.Current;
                    await ProcessCommand(command, cancellationToken, _requestStream, machineSerial);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Failed to process progressive commands", e);
            }
            finally
            {
                CloseCommandHandler();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                CloseCommandHandler();
                _authorization.AuthorizationData = null;
            }

            _disposed = true;
        }

        private Task<bool> OpenStream(CancellationToken cancellationToken)
        {
            _responseStream = null;
            _requestStream = null;

            try
            {
                _commandHandler = Invoke(
                    (client, token) => client.ProgressiveUpdates(cancellationToken: token),
                    cancellationToken);
                _responseStream = _commandHandler.ResponseStream;
                _requestStream = _commandHandler.RequestStream;
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                Logger.Error("Failed to open progressive stream", e);
            }

            return Task.FromResult(false);
        }

        /// <summary>
        /// Send the progressive command to the server which starts the progressive updates to the EGM
        /// </summary>
        /// <param name="machineSerial">The machine serial number</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns></returns>
        private async Task<bool> StartProgressiveUpdates(string machineSerial, CancellationToken cancellationToken)
        {
            if (_requestStream is null)
            {
                Logger.Error("Failed to start progressive update as request stream is null");
                return false;
            }

            try
            {
                await Respond(_requestStream, machineSerial, cancellationToken);
            }
            catch (Exception e)
            {
                Logger.Error("Failed to process progressive command to start progressive updates", e);
                return false;
            }

            return true;
        }

        private async Task ProcessCommandsOnStream(string machineSerial, CancellationToken cancellationToken)
        {
            try
            {
                await ProcessCommands(machineSerial, cancellationToken);
                Logger.Debug($"Progressive command service exited.  IsCancelled={cancellationToken.IsCancellationRequested}");

                await _requestStream.CompleteAsync();
            }
            catch (Exception e)
            {
                Logger.Error($"Progressive command service exited.  IsCancelled={cancellationToken.IsCancellationRequested}", e);
            }
        }

        private void CloseCommandHandler()
        {
            if (_disposed)
            {
                return;
            }

            _commandHandler?.Dispose();

            _commandHandler = null;
        }

        private async Task Respond(
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
            CancellationToken token,
            IAsyncStreamWriter<Progressive> output,
            string machineSerial)
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
    }
}