namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Accounting.Contracts;
    using Accounting.Contracts.Vouchers;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Bingo.Consumers;
    using Common;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class VoucherRejectedConsumerTests
    {
        private VoucherRejectedConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Loose);
        private readonly Mock<IReportEventQueueService> _bingoEventQueue = new(MockBehavior.Default);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new VoucherRejectedConsumer(
                _eventBus.Object,
                _consumerContext.Object,
                _bingoEventQueue.Object);
        }

        [DataRow(true, false, DisplayName = "EventBus Null")]
        [DataRow(false, true, DisplayName = "Event Reporting Service Null")]
        [DataTestMethod]
        public void NullConstructorParametersTest(bool eventBusNull, bool queueNull)
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => _ = new VoucherRejectedConsumer(
                    eventBusNull ? null : _eventBus.Object,
                    _consumerContext.Object,
                    queueNull ? null : _bingoEventQueue.Object));
        }

        [TestMethod]
        public void ConsumesTimedOutTest()
        {
            var transaction = new VoucherInTransaction
            {
                Exception = (int)VoucherInExceptionCode.TimedOut
            };

            var evt = new VoucherRejectedEvent(transaction);

            _target.Consume(evt);
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.VoucherRedeemTimeout), Times.Once);
        }

        [TestMethod]
        public void ConsumesLimitExceededTest()
        {
            var transaction = new VoucherInTransaction
            {
                Exception = (int)VoucherInExceptionCode.VoucherInLimitExceeded
            };

            var evt = new VoucherRejectedEvent(transaction);

            _target.Consume(evt);
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.CashInExceedsVoucherInLimit), Times.Once);
        }

        [TestMethod]
        public void NullTransactionTest()
        {
            var evt = new VoucherRejectedEvent();
            _target.Consume(evt);
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(It.IsAny<ReportableEvent>()), Times.Never);
        }
    }
}