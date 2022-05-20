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
    public class GameConnectedConsumerTests
    {
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Default);
        private readonly Mock<ISharedConsumer> _context = new(MockBehavior.Default);
        private readonly Mock<IBingoReplayRecovery> _recovery = new(MockBehavior.Default);

        private GameConnectedConsumer _target;

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

        [DataRow(false, true, DisplayName = "Game Loaded Not in Replay")]
        [DataRow(true, false, DisplayName = "Game Loaded in Replay")]
        [DataTestMethod]
        public async Task ConsumeTest(bool inReplay, bool recoveryStarted)
        {
            _recovery.Setup(x => x.RecoverDisplay(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            await _target.Consume(new GameConnectedEvent(inReplay), CancellationToken.None);
            _recovery.Verify(
                x => x.RecoverDisplay(It.IsAny<CancellationToken>()),
                recoveryStarted ? Times.Once() : Times.Never());
        }

        private GameConnectedConsumer CreateTarget(bool nullEvent = false, bool nullRecovery = false)
        {
            return new GameConnectedConsumer(
                nullEvent ? null : _eventBus.Object,
                _context.Object,
                nullRecovery ? null : _recovery.Object);
        }
    }
}