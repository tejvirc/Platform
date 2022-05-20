namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Accounting.Contracts.Handpay;
    using Aristocrat.Monaco.Bingo.Common;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Bingo.Consumers;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class HandpayKeyedOffConsumerTests
    {
        private const long TestAmount = 10000;
        private HandpayKeyedOffConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Loose);
        private readonly Mock<IReportTransactionQueueService> _reportingService = new(MockBehavior.Strict);
        private readonly Mock<IReportEventQueueService> _bingoEventQueue = new(MockBehavior.Strict);
        private HandpayKeyedOffEvent _event;
        private readonly HandpayTransaction _transaction = new(1, DateTime.Now, TestAmount, 0, 0, HandpayType.GameWin, false, new Guid())
                                                            { KeyOffCashableAmount = TestAmount };

        [TestInitialize]
        public void MyTestInitialize()
        {
            _event = new(_transaction);

            _target = new HandpayKeyedOffConsumer(
                _eventBus.Object,
                _consumerContext.Object,
                _reportingService.Object,
                _bingoEventQueue.Object);
        }


        [DataRow(true, false, false, DisplayName = "Transaction Reporting Service Null")]
        [DataRow(false, true, false, DisplayName = "EventBus Null")]
        [DataRow(false, false, true, DisplayName = "Event Reporting Service Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(bool reportingNull, bool eventBusNull, bool queueNull)
        {
            _target = new HandpayKeyedOffConsumer(
                eventBusNull ? null : _eventBus.Object,
                _consumerContext.Object,
                reportingNull ? null : _reportingService.Object,
                queueNull ? null : _bingoEventQueue.Object);
        }

        [TestMethod]
        public void ConsumesTest()
        {
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.HandPayKeyOff, TestAmount.MillicentsToCents(), 0, 0, 0, 0)).Verifiable();
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CancelledCredits, TestAmount.MillicentsToCents(), 0, 0, 0, 0)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.HandpayKeyOff)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.CancelCredits)).Verifiable();

            _target.Consume(_event);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.HandPayKeyOff, TestAmount.MillicentsToCents(), 0, 0, 0, 0),
                Times.Once());
            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CancelledCredits, TestAmount.MillicentsToCents(), 0, 0, 0, 0),
                Times.Never());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.HandpayKeyOff), Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.CancelCredits), Times.Never());
        }

        [TestMethod]
        public void ConsumesCancelledCreditTest()
        {
            var evt = new HandpayKeyedOffEvent(
                new HandpayTransaction(1, DateTime.Now, TestAmount, 0, 0, HandpayType.CancelCredit, false, new Guid())
                { KeyOffCashableAmount = TestAmount });
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CancelledCredits, TestAmount.MillicentsToCents(), 0, 0, 0, 0)).Verifiable();
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.HandPayKeyOff, TestAmount.MillicentsToCents(), 0, 0, 0, 0)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.HandpayKeyOff)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.CancelCredits)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CancelledCredits, TestAmount.MillicentsToCents(), 0, 0, 0, 0),
                Times.Once());
            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.HandPayKeyOff, TestAmount.MillicentsToCents(), 0, 0, 0, 0),
                Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.HandpayKeyOff), Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.CancelCredits), Times.Once());
        }

        [TestMethod]
        public void ConsumesZeroAmountTest()
        {
            var evt = new HandpayKeyedOffEvent(
                new HandpayTransaction(1, DateTime.Now, 0, 0, 0, HandpayType.CancelCredit, false, new Guid()));
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashIn, 0, 0, 0, 0, 0)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.HandpayKeyOff)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.CancelCredits)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashIn, TestAmount.MillicentsToCents(), 0, 0, 0, 0),
                Times.Never());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.HandpayKeyOff), Times.Never());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.CancelCredits), Times.Never());
        }
    }
}