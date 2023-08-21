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
    public class GameReplayCompletedConsumerTests
    {
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Default);
        private readonly Mock<ISharedConsumer> _sharedConsumer = new(MockBehavior.Default);
        private readonly Mock<IBingoReplayRecovery> _recovery = new(MockBehavior.Default);
        private readonly Mock<IGameDiagnostics> _diagnostics = new(MockBehavior.Default);

        private GameReplayCompletedConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new GameReplayCompletedConsumer(
                _eventBus.Object,
                _sharedConsumer.Object,
                _recovery.Object,
                _diagnostics.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
        }

        [DataRow(true, false, false)]
        [DataRow(false, true, false)]
        [DataRow(false, false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void NullConstructorArgumentTests(bool nullEvent, bool nullRecovery, bool nullDiagnostics)
        {
            _ = CreateTarget(nullEvent, nullRecovery, nullDiagnostics);
        }

        [DataRow(false, false, false, DisplayName = "Diagnostic is not currently active")]
        [DataRow(true, false, false, DisplayName = "Diagnostic context not in replay")]
        [DataRow(true, true, true, DisplayName = "Diagnostic context in replay")]
        [DataTestMethod]
        public async Task ConsumeTest(bool isDiagnosticsActive, bool validContext, bool replayStarted)
        {
            var gameHistory = new Mock<IGameHistoryLog>();
            var context = new Mock<IDiagnosticContext<IGameHistoryLog>>();
            context.Setup(x => x.Arguments).Returns(gameHistory.Object);
            _diagnostics.Setup(x => x.IsActive).Returns(isDiagnosticsActive);
            _diagnostics.Setup(x => x.Context).Returns(validContext ? context.Object : null);
            await _target.Consume(new GameReplayCompletedEvent(), CancellationToken.None);
            _recovery.Verify(
                x => x.Replay(gameHistory.Object, true, It.IsAny<CancellationToken>()),
                replayStarted ? Times.Once() : Times.Never());
        }

        private GameReplayCompletedConsumer CreateTarget(
            bool nullEventBus = false,
            bool nullRecovery = false,
            bool nullDiagnostics = false)
        {
            return new GameReplayCompletedConsumer(
                nullEventBus ? null : _eventBus.Object,
                _sharedConsumer.Object,
                nullRecovery ? null : _recovery.Object,
                nullDiagnostics ? null : _diagnostics.Object);
        }
    }
}