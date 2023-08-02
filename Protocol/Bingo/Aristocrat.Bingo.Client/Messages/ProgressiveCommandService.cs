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
        IProgressiveCommandService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IProgressiveCommandProcessorFactory _processorFactory;
        private readonly IAsyncPolicy _commandRetryPolicy;

        public TimeSpan BackOffTime { get; set; } = TimeSpan.FromMilliseconds(200);

        public ProgressiveCommandService(
            IClientEndpointProvider<ProgressiveApi.ProgressiveApiClient> endpointProvider,
            IProgressiveCommandProcessorFactory processorFactory)
            : base(endpointProvider)
        {
            _processorFactory = processorFactory ?? throw new ArgumentNullException(nameof(processorFactory));
            _commandRetryPolicy = CreatePolicy();
        }

        public async Task<bool> HandleCommands(string machineSerial, int gameTitleId, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    using var commandCaller = GetCommandHandler(cancellationToken);
                    if (commandCaller is not null &&
                        await StartProgressiveUpdates(machineSerial, commandCaller.RequestStream, cancellationToken))
                    {
                        await ProcessCommandsOnStream(
                            machineSerial,
                            commandCaller.RequestStream,
                            commandCaller.ResponseStream,
                            cancellationToken);
                    }

                    await Task.Delay(BackOffTime, cancellationToken);
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
    }
}