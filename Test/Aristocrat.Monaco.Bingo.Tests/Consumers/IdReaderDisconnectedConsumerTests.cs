namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Aristocrat.Monaco.Bingo.Common;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Aristocrat.Monaco.Hardware.Contracts.IdReader;
    using Bingo.Consumers;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class IdReaderDisconnectedConsumerTests
    {
        private IdReaderDisconnectedConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Loose);
        private readonly Mock<IReportEventQueueService> _reportingService = new(MockBehavior.Strict);
        private readonly DisconnectedEvent _event = new();

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new IdReaderDisconnectedConsumer(_eventBus.Object, _consumerContext.Object, _reportingService.Object);
        }

        [DataRow(true, false, DisplayName = "Reporting Service Null")]
        [DataRow(false, true, DisplayName = "EventBus Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(bool reportingNull, bool eventBusNull)
        {
            _target = new IdReaderDisconnectedConsumer(
                eventBusNull ? null : _eventBus.Object,
                _consumerContext.Object,
                reportingNull ? null : _reportingService.Object);
        }

        [TestMethod]
        public void ConsumesTest()
        {
            _reportingService.Setup(m => m.AddNewEventToQueue(ReportableEvent.CardReaderCommunicationError)).Verifiable();

            _target.Consume(_event);

            _reportingService.Verify(m => m.AddNewEventToQueue(ReportableEvent.CardReaderCommunicationError), Times.Once());
        }
    }
}