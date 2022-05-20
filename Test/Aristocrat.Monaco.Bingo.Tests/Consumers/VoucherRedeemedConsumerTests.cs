namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Bingo.Common;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Bingo.Consumers;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class VoucherRedeemedConsumerTests
    {
        private const long TestAmount = 1234000;
        private VoucherRedeemedConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Loose);
        private readonly Mock<IReportTransactionQueueService> _reportingService = new(MockBehavior.Strict);
        private readonly Mock<IReportEventQueueService> _bingoEventQueue = new(MockBehavior.Strict);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new VoucherRedeemedConsumer(
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
            _target = new VoucherRedeemedConsumer(
                eventBusNull ? null : _eventBus.Object,
                _consumerContext.Object,
                reportingNull ? null : _reportingService.Object,
                queueNull ? null : _bingoEventQueue.Object);
        }

        [TestMethod]
        public void ConsumesPromoTest()
        {
            var evt = new VoucherRedeemedEvent(new VoucherInTransaction { Amount = TestAmount, TypeOfAccount = AccountType.Promo });
            _reportingService.Setup(
                m => m.AddNewTransactionToQueue(
                    Common.TransactionType.CashPromoTicketIn, TestAmount.MillicentsToCents(), 0, 0, 0, 0)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TicketIn)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(
                m => m.AddNewTransactionToQueue(
                    Common.TransactionType.CashPromoTicketIn, TestAmount.MillicentsToCents(), 0, 0, 0, 0), Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TicketIn), Times.Once());
        }

        [TestMethod]
        public void ConsumesNonCashTest()
        {
            var evt = new VoucherRedeemedEvent(
                new VoucherInTransaction { Amount = TestAmount, TypeOfAccount = AccountType.NonCash });
            _reportingService.Setup(
                m => m.AddNewTransactionToQueue(
                    Common.TransactionType.NonTransferablePromoTicketIn, TestAmount.MillicentsToCents(), 0, 0, 0, 0)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TicketIn)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(
                m => m.AddNewTransactionToQueue(
                    Common.TransactionType.NonTransferablePromoTicketIn, TestAmount.MillicentsToCents(), 0, 0, 0, 0), Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TicketIn), Times.Once());
        }

        [TestMethod]
        public void ConsumesCashTest()
        {
            var evt = new VoucherRedeemedEvent(
                new VoucherInTransaction { Amount = TestAmount, TypeOfAccount = AccountType.Cashable });
            _reportingService.Setup(
                m => m.AddNewTransactionToQueue(
                    Common.TransactionType.TicketIn, TestAmount.MillicentsToCents(), 0, 0, 0, 0)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TicketIn)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(
                m => m.AddNewTransactionToQueue(
                    Common.TransactionType.TicketIn, TestAmount.MillicentsToCents(), 0, 0, 0, 0), Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TicketIn), Times.Once());
        }

        [TestMethod]
        public void ConsumesZeroAmountDoesntReportTest()
        {
            var evt = new VoucherRedeemedEvent(
                new VoucherInTransaction { Amount = 0, TypeOfAccount = AccountType.Cashable });
            _reportingService.Setup(
                m => m.AddNewTransactionToQueue(
                    Common.TransactionType.TicketIn, It.IsAny<long>(), 0, 0, 0, 0)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TicketIn)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(
                m => m.AddNewTransactionToQueue(
                    Common.TransactionType.TicketIn, It.IsAny<long>(), 0, 0, 0, 0), Times.Never());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TicketIn), Times.Never());
        }
    }
}