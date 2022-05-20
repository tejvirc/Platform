namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Bingo.Consumers;
    using Bingo.Services.GamePlay;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GameDiagnosticsStartedConsumerTests
    {
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Default);
        private readonly Mock<ISharedConsumer> _context = new(MockBehavior.Default);
        private readonly Mock<IBingoReplayRecovery> _recovery = new(MockBehavior.Default);

        private GameDiagnosticsStartedConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
        }

        [DataRow(true, false)]
        [DataRow(false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void NullConstructorArgumentTests(bool nullEvent, bool nullRecovery)
        {
            _ = CreateTarget(nullEvent, nullRecovery);
        }

        [DataRow(false, false, DisplayName = "Diagnostic context not in replay")]
        [DataRow(true, true, DisplayName = "Diagnostic context in replay")]
        [DataTestMethod]
        public async Task ConsumeTest(bool validContext, bool replayStarted)
        {
            var gameHistory = new Mock<IGameHistoryLog>();
            var context = new Mock<IDiagnosticContext<IGameHistoryLog>>();
            context.Setup(x => x.Arguments).Returns(gameHistory.Object);
            await _target.Consume(
                new GameDiagnosticsStartedEvent(123, 1000, string.Empty, validContext ? context.Object : null),
                CancellationToken.None);
            _recovery.Verify(
                x => x.Replay(gameHistory.Object, false, It.IsAny<CancellationToken>()),
                replayStarted ? Times.Once() : Times.Never());
        }

        private GameDiagnosticsStartedConsumer CreateTarget(bool nullEvent = false, bool nullRecovery = false)
        {
            return new GameDiagnosticsStartedConsumer(
                nullEvent ? null : _eventBus.Object,
                _context.Object,
                nullRecovery ? null : _recovery.Object);
        }
    }
}