namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Aristocrat.Monaco.Accounting.Contracts.Wat;
    using Aristocrat.Monaco.Bingo.Common;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Bingo.Consumers;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class WatTransferCancelRequestedConsumerTests
    {
        private WatTransferCancelRequestedConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Loose);
        private readonly Mock<IReportEventQueueService> _bingoEventQueue = new(MockBehavior.Strict);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new WatTransferCancelRequestedConsumer(
                _eventBus.Object,
                _consumerContext.Object,
                _bingoEventQueue.Object);
        }

        [DataRow(true, false, DisplayName = "EventBus Null")]
        [DataRow(false, true, DisplayName = "Event Reporting Service Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(bool eventBusNull, bool queueNull)
        {
            _target = new WatTransferCancelRequestedConsumer(
                eventBusNull ? null : _eventBus.Object,
                _consumerContext.Object,
                queueNull ? null : _bingoEventQueue.Object);
        }

        [TestMethod]
        public void ConsumesCancelOutTest()
        {
            var evt = new WatTransferCancelRequestedEvent(new WatTransaction
            {
                Status = WatStatus.CancelReceived,
                RequestId = null
            });

            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TransferOutTimeout)).Verifiable();

            _target.Consume(evt);

            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TransferOutTimeout), Times.Once());
        }

        [TestMethod]
        public void ConsumesNotCancelledTest()
        {
            var evt = new WatTransferCancelRequestedEvent(new WatTransaction
            {
                Status = WatStatus.RequestReceived,
                RequestId = null
            });

            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TransferOutTimeout)).Verifiable();

            _target.Consume(evt);

            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TransferOutTimeout), Times.Never());
        }
    }
}