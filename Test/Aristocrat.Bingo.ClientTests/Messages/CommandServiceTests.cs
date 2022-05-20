namespace Aristocrat.Bingo.Client.Tests.Messages
{
    using System;
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

        private CommandService _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
        }

        [DataRow(true, false)]
        [DataRow(false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void NullConstructorArgumentsTest(bool nullCommandFactory, bool nullEndpoint)
        {
            _target = CreateTarget(nullCommandFactory, nullEndpoint);
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
                Command_ = Any.Pack(new PingCommand { PingRequestTime = DateTime.UtcNow.ToTimestamp() })
            };

            _commandFactory.Setup(x => x.ProcessCommand(It.IsAny<Command>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IMessage>(new PingResponse()));
            var writer = await ProcessCommand(command);
            writer.Verify(
                x => x.WriteAsync(It.Is<CommandResponse>(c => c.Response.Is(PingResponse.Descriptor))));
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
                Command_ = Any.Pack(new PingCommand { PingRequestTime = DateTime.UtcNow.ToTimestamp() })
            };

            _commandFactory.Setup(x => x.ProcessCommand(It.IsAny<Command>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IMessage>(new PingResponse()));
            var writer = await ProcessCommand(command, true);
            writer.Verify(
                x => x.WriteAsync(It.Is<CommandResponse>(c => c.Response.Is(PingResponse.Descriptor))));
        }

        [TestMethod]
        public async Task ReportStatusTest()
        {
            const long cashInMeter = 10000;
            const long cashoutMeter = 20000;
            const long cashPlayedMeter = 30000;
            const long cashWonMeter = 40000;
            const int egmStatus = 0;

            using var source = new CancellationTokenSource();
            var completion = new TaskCompletionSource<bool>();
            using var waiter = new ManualResetEvent(false);
            var message = new StatusResponseMessage(MachineId)
            {
                CashInMeterValue = cashInMeter,
                CashOutMeterValue = cashoutMeter,
                CashPlayedMeterValue = cashPlayedMeter,
                CashWonMeterValue = cashWonMeter,
                EgmStatusFlags = egmStatus
            };

            var client = new Mock<ClientApi.ClientApiClient>(MockBehavior.Default);
            _clientEndpointProvider.Setup(x => x.IsConnected).Returns(true);
            _clientEndpointProvider.Setup(x => x.Client).Returns(client.Object);

            var clientWriter = new Mock<IClientStreamWriter<CommandResponse>>(MockBehavior.Default);
            var serverWriter = new Mock<IAsyncStreamReader<Command>>(MockBehavior.Default);
            serverWriter.Setup(x => x.MoveNext(It.IsAny<CancellationToken>()))
                .Callback(() => waiter.Set())
                .Returns(completion.Task);
            serverWriter.Setup(x => x.Current).Returns(new Command());
            clientWriter.Setup(x => x.WriteAsync(It.IsAny<CommandResponse>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var response = TestCalls.AsyncDuplexStreamingCall(
                clientWriter.Object,
                serverWriter.Object,
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { source.Cancel(); });

            client.Setup(
                    x => x.ReadCommands(
                        It.IsAny<Metadata>(),
                        It.IsAny<DateTime?>(),
                        It.IsAny<CancellationToken>()))
                .Returns(response);

            var commandTask = _target.HandleCommands(MachineId, source.Token);
            waiter.WaitOne(1000);
            await _target.ReportStatus(message, source.Token);
            clientWriter.Verify(
                x => x.WriteAsync(It.Is<CommandResponse>(c => c.Response.Is(StatusResponse.Descriptor))),
                Times.Once);
            completion.SetResult(false);
            await commandTask;
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ReportStatusWithoutOpeningCommandsTest()
        {
            const long cashInMeter = 10000;
            const long cashoutMeter = 20000;
            const long cashPlayedMeter = 30000;
            const long cashWonMeter = 40000;
            const int egmStatus = 0;
            var message = new StatusResponseMessage(MachineId)
            {
                CashInMeterValue = cashInMeter,
                CashOutMeterValue = cashoutMeter,
                CashPlayedMeterValue = cashPlayedMeter,
                CashWonMeterValue = cashWonMeter,
                EgmStatusFlags = egmStatus
            };

            await _target.ReportStatus(message, CancellationToken.None);
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

            Assert.IsTrue(await _target.HandleCommands(MachineId, source.Token));
            return clientWriter;
        }

        private CommandService CreateTarget(bool nullCommandFactory = false, bool nullEndpoint = false)
        {
            return new CommandService(
                nullEndpoint ? null : _clientEndpointProvider.Object,
                nullCommandFactory ? null : _commandFactory.Object);
        }
    }
}