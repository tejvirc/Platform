namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Accounting.Contracts;
    using Accounting.Contracts.Vouchers;
    using Aristocrat.Monaco.Bingo.Common;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Bingo.Consumers;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class VoucherRejectedConsumerTests
    {
        private VoucherRejectedConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Loose);
        private readonly Mock<IReportEventQueueService> _bingoEventQueue = new(MockBehavior.Strict);

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
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(bool eventBusNull, bool queueNull)
        {
            _target = new VoucherRejectedConsumer(
                eventBusNull ? null : _eventBus.Object,
                _consumerContext.Object,
                queueNull ? null : _bingoEventQueue.Object);
        }

        [TestMethod]
        public void ConsumesTimedOutTest()
        {
            var evt = new VoucherRejectedEvent(
                new VoucherInTransaction
                { Exception = (int)VoucherInExceptionCode.TimedOut });
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.VoucherRedeemTimeout)).Verifiable();

            _target.Consume(evt);

            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.VoucherRedeemTimeout), Times.Once());
        }

        [TestMethod]
        public void ConsumesLimitExceededTest()
        {
            var evt = new VoucherRejectedEvent(
                new VoucherInTransaction
                { Exception = (int)VoucherInExceptionCode.VoucherInLimitExceeded });
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.CashInExceedsVoucherInLimit)).Verifiable();

            _target.Consume(evt);

            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.CashInExceedsVoucherInLimit), Times.Once());
        }
    }
}