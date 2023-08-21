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
    using Test.Common;

    [TestClass]
    public class OperatorMenuEnteredConsumerTests
    {
        private OperatorMenuEnteredConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _sharedConsumer = new(MockBehavior.Strict);
        private readonly Mock<IReportEventQueueService> _reportingService = new(MockBehavior.Strict);
        private readonly OperatorMenuEnteredEvent _event = new();

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new OperatorMenuEnteredConsumer(_eventBus.Object, _sharedConsumer.Object, _reportingService.Object);
        }

        [DataRow(true, false, DisplayName = "Reporting Service Null")]
        [DataRow(false, true, DisplayName = "EventBus Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(bool reportingNull, bool eventBusNull)
        {
            _target = new OperatorMenuEnteredConsumer(
                eventBusNull ? null : _eventBus.Object,
                _sharedConsumer.Object,
                reportingNull ? null : _reportingService.Object);
        }

        [TestMethod]
        public void ConsumesTest()
        {
            _reportingService.Setup(m => m.AddNewEventToQueue(ReportableEvent.Operator)).Verifiable();
            _reportingService.Setup(m => m.AddNewEventToQueue(ReportableEvent.SetUpModeEntered)).Verifiable();

            _target.Consume(_event);

            _reportingService.Verify(m => m.AddNewEventToQueue(ReportableEvent.Operator), Times.Once());
            _reportingService.Verify(m => m.AddNewEventToQueue(ReportableEvent.SetUpModeEntered), Times.Once());
        }
    }
}