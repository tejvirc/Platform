namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Bingo.Common;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Bingo.Consumers;
    using Bingo.Services.GamePlay;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class PlatformBootedConsumerTests
    {
        private PlatformBootedConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _sharedConsumer = new(MockBehavior.Strict);
        private readonly Mock<IReportEventQueueService> _reportingService = new(MockBehavior.Strict);
        private readonly Mock<IBingoReplayRecovery> _replayRecovery = new(MockBehavior.Strict);
        private readonly PlatformBootedEvent _event = new(DateTime.UtcNow, true);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
        }

        [DataRow(true, false, false, DisplayName = "Reporting Service Null")]
        [DataRow(false, true, false, DisplayName = "EventBus Null")]
        [DataRow(false, false, true, DisplayName = "ReplayRecovery Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(bool reportingNull, bool eventBusNull, bool replayRecoveryNull)
        {
            _ = CreateTarget(reportingNull, eventBusNull, replayRecoveryNull);
        }

        [TestMethod]
        public void ConsumesTest()
        {
            _replayRecovery.Setup(x => x.RecoverGamePlay(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();
            _reportingService.Setup(m => m.AddNewEventToQueue(ReportableEvent.PowerUp)).Verifiable();
            _reportingService.Setup(m => m.AddNewEventToQueue(ReportableEvent.NvRamCleared)).Verifiable();

            _target.Consume(_event);

            _reportingService.Verify(m => m.AddNewEventToQueue(ReportableEvent.PowerUp), Times.Once());
            _reportingService.Verify(m => m.AddNewEventToQueue(ReportableEvent.NvRamCleared), Times.Once());
        }

        [TestMethod]
        public void ConsumesWhenNoClearTest()
        {
            _replayRecovery.Setup(x => x.RecoverGamePlay(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();
            _reportingService.Setup(m => m.AddNewEventToQueue(ReportableEvent.PowerUp)).Verifiable();
            _reportingService.Setup(m => m.AddNewEventToQueue(ReportableEvent.NvRamCleared)).Verifiable();
            PlatformBootedEvent @event = new(DateTime.UtcNow, false);

            _target.Consume(@event);

            _reportingService.Verify(m => m.AddNewEventToQueue(ReportableEvent.PowerUp), Times.Once());
            _reportingService.Verify(m => m.AddNewEventToQueue(ReportableEvent.NvRamCleared), Times.Never());
            _replayRecovery.Verify(x => x.RecoverGamePlay(It.IsAny<CancellationToken>()));
        }

        private PlatformBootedConsumer CreateTarget(
            bool reportingNull = false,
            bool eventBusNull = false,
            bool replayRecoveryNull = false)
        {
            return new PlatformBootedConsumer(
                eventBusNull ? null : _eventBus.Object,
                _sharedConsumer.Object,
                reportingNull ? null : _reportingService.Object,
                replayRecoveryNull ? null : _replayRecovery.Object);
        }
    }
}