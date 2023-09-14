namespace Aristocrat.Bingo.Client.Tests.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Client.Messages;
    using Client.Messages.Commands;
    using Google.Protobuf.WellKnownTypes;
    using Grpc.Core;
    using Grpc.Core.Testing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using ServerApiGateway;
    using IMessage = Google.Protobuf.IMessage;

    [TestClass]
    public class CommandServiceTests
    {
        private const string MachineId = "123";

        private readonly Mock<IClientEndpointProvider<ClientApi.ClientApiClient>> _clientEndpointProvider = new(MockBehavior.Default);
        private readonly Mock<ICommandProcessorFactory> _commandFactory = new(MockBehavior.Default);
        private Mock<IClient<ClientApi.ClientApiClient>> _bingoClient =
            new Mock<IClient<ClientApi.ClientApiClient>>(MockBehavior.Default);
        private CommandService _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
        }

        [DataRow(true, false, false)]
        [DataRow(false, true, false)]
        [DataRow(false, false, true)]
        [DataTestMethod]
        public void NullConstructorArgumentsTest(bool nullCommandFactory, bool nullEndpoint, bool nullClient)
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => _ = CreateTarget(nullCommandFactory, nullEndpoint, nullClient));
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
        }

        [TestMethod]
        public async Task NoClientConnectedTest()
        {
            var client = new Mock<ClientApi.ClientApiClient>(MockBehavior.Default);
            _clientEndpointProvider.Setup(x => x.IsConnected).Returns(false);
            _clientEndpointProvider.Setup(x => x.Client).Returns(client.Object);
            var result = await _target.HandleCommands(MachineId, CancellationToken.None);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task NullClientConnectedTest()
        {
            _clientEndpointProvider.Setup(x => x.IsConnected).Returns(true);
            _clientEndpointProvider.Setup(x => x.Client).Returns((ClientApi.ClientApiClient)null);
            var result = await _target.HandleCommands(MachineId, CancellationToken.None);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task ProcessValidCommandTest()
        {
            var command = new Command
            {
                Command_ = Any.Pack(new EnableCommand())
            };

            _commandFactory.Setup(x => x.ProcessCommand(It.IsAny<Command>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IMessage>(new EnableCommandResponse()));
            var writer = await ProcessCommand(command);
            writer.Verify(
                x => x.WriteAsync(It.Is<CommandResponse>(c => c.Response.Is(EnableCommandResponse.Descriptor))));
        }

        [TestMethod]
        public async Task NullCommandHasNoResponseTest()
        {
            var writer = await ProcessCommand(null);
            writer.Verify(
                x => x.WriteAsync(It.Is<CommandResponse>(c => !c.Response.Is(OpenConnection.Descriptor))),
                Times.Never);
        }

        [TestMethod]
        public async Task NullCommandDescriptionHasNoResponseTest()
        {
            var writer = await ProcessCommand(new Command());
            writer.Verify(
                x => x.WriteAsync(It.Is<CommandResponse>(c => !c.Response.Is(OpenConnection.Descriptor))),
                Times.Never);
        }

        [TestMethod]
        public async Task FailedCommandsGetRetried()
        {
            using var source = new CancellationTokenSource();
            using var waiter = new ManualResetEvent(false);
            var client = new Mock<ClientApi.ClientApiClient>(MockBehavior.Default);
            _clientEndpointProvider.Setup(x => x.IsConnected).Returns(true);
            _clientEndpointProvider.Setup(x => x.Client).Returns(client.Object);

            var clientWriter = new Mock<IClientStreamWriter<CommandResponse>>(MockBehavior.Default);
            var serverWriter = new Mock<IAsyncStreamReader<Command>>(MockBehavior.Default);
            serverWriter.Setup(x => x.MoveNext(It.IsAny<CancellationToken>()))
                .Throws<Exception>();
            serverWriter.Setup(x => x.Current).Returns(new Command());
            clientWriter.Setup(x => x.WriteAsync(It.IsAny<CommandResponse>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var callsCount = 0;
            var response = TestCalls.AsyncDuplexStreamingCall(
                clientWriter.Object,
                serverWriter.Object,
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () =>
                {
                    if (callsCount++ > 0)
                    {
                        waiter.Set();
                        source.Cancel();
                    }
                });

            client.Setup(
                    x => x.ReadCommands(
                        It.IsAny<Metadata>(),
                        It.IsAny<DateTime?>(),
                        It.IsAny<CancellationToken>()))
                .Returns(response)
                .Verifiable();

            var commandTask = _target.HandleCommands(MachineId, source.Token);
            Assert.IsTrue(waiter.WaitOne(1000));
            client.Verify(
                x => x.ReadCommands(It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
            await commandTask;
        }

        [TestMethod]
        public async Task CommandResponseWillRetry()
        {
            var command = new Command
            {
                Command_ = Any.Pack(new EnableCommand())
            };

            _commandFactory.Setup(x => x.ProcessCommand(It.IsAny<Command>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IMessage>(new EnableCommandResponse()));
            var writer = await ProcessCommand(command, true);
            writer.Verify(
                x => x.WriteAsync(It.Is<CommandResponse>(c => c.Response.Is(EnableCommandResponse.Descriptor))));
        }

        [DataRow(RpcConnectionState.Disconnected)]
        [DataRow(RpcConnectionState.Closed)]
        [DataTestMethod]
        public async Task StateChangeRecreatesStream(RpcConnectionState state)
        {
            using var source = new CancellationTokenSource();
            using var waiter = new ManualResetEvent(false);

            var client = new Mock<ClientApi.ClientApiClient>(MockBehavior.Default);
            _clientEndpointProvider.Setup(x => x.IsConnected).Returns(true);
            _clientEndpointProvider.Setup(x => x.Client).Returns(client.Object);

            var clientWriter = new Mock<IClientStreamWriter<CommandResponse>>(MockBehavior.Default);

            var streamReader = new StreamReader();
            var response = TestCalls.AsyncDuplexStreamingCall(
                clientWriter.Object,
                streamReader,
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });

            client.Setup(
                    x => x.ReadCommands(
                        It.IsAny<Metadata>(),
                        It.IsAny<DateTime?>(),
                        It.IsAny<CancellationToken>()))
                .Returns(response)
                .Callback(() => waiter.Set())
                .Verifiable();

            var commandTask = _target.HandleCommands(MachineId, source.Token);
            waiter.WaitOne(1000);
            waiter.Reset();
            _bingoClient.Raise(x => x.ConnectionStateChanged += null, new ConnectionStateChangedEventArgs(state));
            waiter.WaitOne(1000);
            client.Verify(
                x => x.ReadCommands(
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(2));
            source.Cancel();
            streamReader.SetResult(null);
            await commandTask;
        }

        private async Task<Mock<IClientStreamWriter<CommandResponse>>> ProcessCommand(Command command, bool retry = false)
        {
            using var source = new CancellationTokenSource();
            var client = new Mock<ClientApi.ClientApiClient>(MockBehavior.Default);
            _clientEndpointProvider.Setup(x => x.IsConnected).Returns(true);
            _clientEndpointProvider.Setup(x => x.Client).Returns(client.Object);

            var clientWriter = new Mock<IClientStreamWriter<CommandResponse>>(MockBehavior.Default);
            var serverWriter = new Mock<IAsyncStreamReader<Command>>(MockBehavior.Default);
            serverWriter.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Returns(Task.FromResult(false));
            serverWriter.Setup(x => x.Current).Returns(command);
            if (retry)
            {
                clientWriter.SetupSequence(x => x.WriteAsync(It.IsAny<CommandResponse>()))
                    .Returns(Task.CompletedTask) // First command is the open response
                    .Throws(new RpcException(new Status(StatusCode.DataLoss, string.Empty)))
                    .Returns(Task.CompletedTask);
            }
            else
            {
                clientWriter.Setup(x => x.WriteAsync(It.IsAny<CommandResponse>()))
                    .Returns(Task.CompletedTask)
                    .Verifiable();
            }

            clientWriter.Setup(x => x.CompleteAsync())
                .Callback(() => source.Cancel())
                .Returns(Task.CompletedTask);

            var response = TestCalls.AsyncDuplexStreamingCall(
                clientWriter.Object,
                serverWriter.Object,
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { source.Cancel(); });

            client.SetupSequence(
                    x => x.ReadCommands(
                        It.IsAny<Metadata>(),
                        It.IsAny<DateTime?>(),
                        It.IsAny<CancellationToken>()))
                .Returns(response);

            await _target.HandleCommands(MachineId, source.Token);
            return clientWriter;
        }

        private CommandService CreateTarget(
            bool nullCommandFactory = false,
            bool nullEndpoint = false,
            bool nullClient = false)
        {
            return new CommandService(
                nullEndpoint ? null : _clientEndpointProvider.Object,
                nullCommandFactory ? null : _commandFactory.Object,
                nullClient ? null : _bingoClient.Object);
        }

        private class StreamReader : IAsyncStreamReader<Command>
        {
            private TaskCompletionSource<bool> _task = new();

            public async Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                using var register = cancellationToken.Register(() =>
                {
                    _task.TrySetCanceled();
                    _task = new TaskCompletionSource<bool>();
                });

                return await _task.Task;
            }

            public void SetResult(Command command)
            {
                Current = command;
                _task.TrySetResult(command != null);
                _task = new TaskCompletionSource<bool>();
            }

            public Command Current { get; private set; }
        }
    }
}