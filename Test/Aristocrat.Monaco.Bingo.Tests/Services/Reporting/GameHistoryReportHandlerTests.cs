namespace Aristocrat.Monaco.Bingo.Tests.Services.Reporting
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Bingo.Client.Messages.GamePlay;
    using Bingo.Services.Reporting;
    using Gaming.Contracts.Central;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GameHistoryReportHandlerTests
    {
        private const int WaitTime = 1000;
        private readonly ReportGameOutcomeResponse _ack = new(ResponseCode.Ok);
        private GameHistoryReportHandler _target;
        private readonly Mock<IAcknowledgedQueue<ReportGameOutcomeMessage, long>> _queue = new(MockBehavior.Default);
        private readonly Mock<ICentralProvider> _centralProvider = new(MockBehavior.Default);
        private readonly Mock<IGameOutcomeService> _gameOutcomeService = new(MockBehavior.Default);
        private readonly Mock<IBingoClientConnectionState> _clientConnection = new(MockBehavior.Default);

        [TestInitialize]
        public void Initialize()
        {
            _target = new GameHistoryReportHandler(
                _centralProvider.Object,
                _clientConnection.Object,
                _queue.Object,
                _gameOutcomeService.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
        }

        [DataRow(true, false, false, false, DisplayName = "Null Central Provider")]
        [DataRow(false, true, false, false, DisplayName = "Null Queue")]
        [DataRow(false, false, true, false, DisplayName = "Null Reporting Service")]
        [DataRow(false, false, false, true, DisplayName = "Null Client connection")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest(
            bool centralProvider,
            bool queue,
            bool service,
            bool clientConnection)
        {
            _target = new GameHistoryReportHandler(
                centralProvider ? null : _centralProvider.Object,
                clientConnection ? null : _clientConnection.Object,
                queue ? null : _queue.Object,
                service ? null : _gameOutcomeService.Object);
        }

        [TestMethod]
        public void AddNewGameOutcomeTest()
        {
            var outcomeMessage = new ReportGameOutcomeMessage();
            _target.AddReportToQueue(outcomeMessage);
            _queue.Verify(x => x.Enqueue(outcomeMessage), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullGameOutcomeTest()
        {
            _target.AddReportToQueue(null);
        }

        [TestMethod]
        public void AddNewEventToQueueAndConnectTest()
        {
            var task = new TaskCompletionSource<ReportGameOutcomeMessage>();
            using var wait = new ManualResetEvent(false);
            var outcomeMessage = new ReportGameOutcomeMessage { TransactionId = 123 };
            _queue.SetupSequence(x => x.GetNextItem(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(outcomeMessage))
                .Returns(task.Task);
            _gameOutcomeService.Setup(m => m.ReportGameOutcome(It.IsAny<ReportGameOutcomeMessage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_ack));
            _queue.Setup(x => x.Acknowledge(outcomeMessage.TransactionId)).Callback(() => wait.Set());
            _clientConnection.Raise(x => x.ClientConnected += null, this, EventArgs.Empty);
            wait.WaitOne(WaitTime);
            _clientConnection.Raise(x => x.ClientDisconnected += null, this, EventArgs.Empty);
            _centralProvider.Verify(x => x.AcknowledgeOutcome(outcomeMessage.TransactionId), Times.Once);
        }
    }
}