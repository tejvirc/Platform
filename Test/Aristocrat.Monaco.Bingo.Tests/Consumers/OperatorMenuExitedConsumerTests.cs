namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Application.Contracts.OperatorMenu;
    using Aristocrat.Monaco.Bingo.Common;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Bingo.Consumers;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class OperatorMenuExitedConsumerTests
    {
        private OperatorMenuExitedConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _sharedConsumer = new(MockBehavior.Strict);
        private readonly Mock<IReportEventQueueService> _reportingService = new(MockBehavior.Strict);
        private readonly OperatorMenuExitedEvent _event = new();

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new OperatorMenuExitedConsumer(_eventBus.Object, _sharedConsumer.Object, _reportingService.Object);
        }

        [DataRow(true, false, DisplayName = "Reporting Service Null")]
        [DataRow(false, true, DisplayName = "EventBus Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(bool reportingNull, bool eventBusNull)
        {
            _target = new OperatorMenuExitedConsumer(
                eventBusNull ? null : _eventBus.Object,
                _sharedConsumer.Object,
                reportingNull ? null : _reportingService.Object);
        }

        [TestMethod]
        public void ConsumesTest()
        {
            _reportingService.Setup(m => m.AddNewEventToQueue(ReportableEvent.SetUpModeExited)).Verifiable();

            _target.Consume(_event);

            _reportingService.Verify(m => m.AddNewEventToQueue(ReportableEvent.SetUpModeExited), Times.Once());
        }
    }
}