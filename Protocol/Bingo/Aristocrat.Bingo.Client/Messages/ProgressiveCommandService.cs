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

        private AsyncDuplexStreamingCall<Progressive, ProgressiveUpdate> _commandHandler;
        private IAsyncStreamReader<ProgressiveUpdate> _responseStream;
        private IClientStreamWriter<Progressive> _requestStream;
        private bool _disposed;

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
                _commandHandler = Invoke(
                    (client, token) => client.ProgressiveUpdates(cancellationToken: token),
                    cancellationToken);
                _responseStream = _commandHandler.ResponseStream;
                _requestStream = _commandHandler.RequestStream;
            }
            catch (Exception e)
            {
                Logger.Error("Failed to open progressive stream", e);
            }

            try
            {
                // Send the progressive message to start the progressive updates to the EGM
                await Respond(_requestStream, machineSerial, cancellationToken);
            }
            catch (Exception e)
            {
                Logger.Error("Failed to process commands", e);
            }

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await ProcessCommands(machineSerial, cancellationToken);
                    Logger.Debug($"Progressive command service exited.  IsCancelled={cancellationToken.IsCancellationRequested}");
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Error($"Progressive command service exited.  IsCancelled={cancellationToken.IsCancellationRequested}", e);
                return false;
            }
            finally
            {
                await _requestStream.CompleteAsync();
                CloseCommandHandler();
            }
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
                Logger.Error("Failed to process commands", e);
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

        private void CloseCommandHandler()
        {
            if (_disposed)
            {
                return;
            }

            if (_commandHandler is not null)
            {
                _commandHandler.Dispose();
            }

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