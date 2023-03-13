namespace Aristocrat.Bingo.Client.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Commands;
    using Google.Protobuf.WellKnownTypes;
    using Grpc.Core;
    using log4net;
    using Polly;
    using ServerApiGateway;

    public sealed class CommandService :
        BaseClientCommunicationService<ClientApi.ClientApiClient>,
        ICommandService,
        IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly ICommandProcessorFactory _processorFactory;
        private readonly IEnumerable<IClient> _clients;
        private readonly SemaphoreSlim _locker = new(1);
        private readonly IAsyncPolicy _commandRetryPolicy;

        private CancellationTokenSource _tokenSource;
        private AsyncDuplexStreamingCall<CommandResponse, Command> _commandHandler;
        private bool _disposed;

        public TimeSpan BackOffTime { get; set; } = TimeSpan.FromMilliseconds(200);

        public CommandService(
            IClientEndpointProvider<ClientApi.ClientApiClient> endpointProvider,
            ICommandProcessorFactory processorFactory,
            IEnumerable<IClient> clients)
            : base(endpointProvider)
        {
            _processorFactory = processorFactory ?? throw new ArgumentNullException(nameof(processorFactory));
            _clients = clients ?? throw new ArgumentNullException(nameof(clients));
            _commandRetryPolicy = CreatePolicy();

            foreach (var client in _clients)
            {
                client.ConnectionStateChanged += HandleConnectionStateChanges;
            }
        }

        public async Task<bool> HandleCommands(string machineSerial, CancellationToken cancellationToken)
        {
            try
            {
                _tokenSource?.Cancel();

                while (!cancellationToken.IsCancellationRequested)
                {
                    using var source = _tokenSource = new CancellationTokenSource();
                    try
                    {
                        using var shared = CancellationTokenSource.CreateLinkedTokenSource(
                            source.Token,
                            cancellationToken);
                        await ProcessCommands(machineSerial, shared.Token);
                        Logger.Debug(
                            $"Command service exited.  IsCancelled={cancellationToken.IsCancellationRequested}");
                        await Task.Delay(BackOffTime, cancellationToken);
                    }
                    finally
                    {
                        _tokenSource = null;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Error($"Command service exited.  IsCancelled={cancellationToken.IsCancellationRequested}", e);
                return false;
            }
        }

        private async Task ProcessCommands(string machineSerial, CancellationToken cancellationToken)
        {
            IAsyncStreamReader<Command> input;
            IClientStreamWriter<CommandResponse> output;

            await _locker.WaitAsync(cancellationToken);
            try
            {
                _commandHandler = Invoke(
                    (client, token) => client.ReadCommands(cancellationToken: token),
                    cancellationToken);
                input = _commandHandler.ResponseStream;
                output = _commandHandler.RequestStream;
            }
            finally
            {
                _locker.Release();
            }

            try
            {
                await Respond(new OpenConnection(), output, machineSerial, cancellationToken);
                while (await input.MoveNext(cancellationToken))
                {
                    var command = input.Current;
                    await ProcessCommand(command, cancellationToken, output, machineSerial);
                }

                await output.CompleteAsync();
            }
            catch (Exception e)
            {
                Logger.Error("Failed to process commands", e);
            }
            finally
            {
                CloseCommandHandler();
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            foreach (var client in _clients)
            {
                client.ConnectionStateChanged -= HandleConnectionStateChanges;
            }

            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = null;
            CloseCommandHandler();
            _locker.Dispose();

            _disposed = true;
        }

        private void CloseCommandHandler()
        {
            if (_disposed)
            {
                return;
            }

            _locker.Wait();
            try
            {
                if (_commandHandler is not null)
                {
                    _commandHandler.Dispose();
                }

                _commandHandler = null;
            }
            finally
            {
                _locker.Release();
            }
        }

        private void HandleConnectionStateChanges(object sender, ConnectionStateChangedEventArgs eventArgs)
        {
            switch (eventArgs.State)
            {
                case RpcConnectionState.Disconnected:
                case RpcConnectionState.Closed:
                    _tokenSource?.Cancel();
                    break;
            }
        }

        private async Task Respond(
            Google.Protobuf.IMessage internalResponse,
            IAsyncStreamWriter<CommandResponse> output,
            string machineSerial,
            CancellationToken token)
        {
            await _locker.WaitAsync(token);
            try
            {
                token.ThrowIfCancellationRequested();
                var response = new CommandResponse
                {
                    MachineSerial = machineSerial,
                    Response = Any.Pack(internalResponse)
                };

                await output.WriteAsync(response);
            }
            finally
            {
                _locker.Release();
            }
        }

        private async Task ProcessCommand(
            Command command,
            CancellationToken token,
            IAsyncStreamWriter<CommandResponse> output,
            string machineSerial)
        {
            if (command?.Command_ is null)
            {
                return;
            }

            var result = await _processorFactory.ProcessCommand(command, token);
            if (result is not null)
            {
                await _commandRetryPolicy.ExecuteAsync(t => Respond(result, output, machineSerial, t), token);
            }
        }
    }
}