namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Accounting.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts.TransferOut;
    using Aristocrat.Monaco.Bingo.Common;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Bingo.Consumers;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class VoucherIssuedConsumerTests
    {
        private VoucherIssuedConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Loose);
        private readonly Mock<IReportTransactionQueueService> _reportingService = new(MockBehavior.Strict);
        private readonly Mock<IReportEventQueueService> _bingoEventQueue = new(MockBehavior.Strict);
        private readonly VoucherIssuedEvent _event = new(
                                                    new VoucherOutTransaction { Amount = 1234, VoucherPrinted = true },
                                                    new Ticket());

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new VoucherIssuedConsumer(
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
            _target = new VoucherIssuedConsumer(
                eventBusNull ? null : _eventBus.Object,
                _consumerContext.Object,
                reportingNull ? null : _reportingService.Object,
                queueNull ? null : _bingoEventQueue.Object);
        }

        [TestMethod]
        public void ConsumesCashoutCashableTest()
        {
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashOut, It.IsAny<long>(), 0, 0, 0, 0)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TicketOut)).Verifiable();

            _target.Consume(_event);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashOut, It.IsAny<long>(), 0, 0, 0, 0), Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TicketOut), Times.Once());
        }

        [TestMethod]
        public void ConsumesCashoutPromoTest()
        {
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashPromoTicketOut, It.IsAny<long>(), 0, 0, 0, 0)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TicketOut)).Verifiable();

            VoucherIssuedEvent evt = new(
                new VoucherOutTransaction
                {
                    Amount = 1234,
                    Reason = TransferOutReason.CashOut,
                    VoucherPrinted = true,
                    TypeOfAccount = AccountType.Promo
                },
                new Ticket());

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashPromoTicketOut, It.IsAny<long>(), 0, 0, 0, 0), Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TicketOut), Times.Once());
        }

        [TestMethod]
        public void ConsumesCashoutNonCashTest()
        {
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.NonTransferablePromoTicketOut, It.IsAny<long>(), 0, 0, 0, 0)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TicketOut)).Verifiable();

            VoucherIssuedEvent evt = new(
                new VoucherOutTransaction {
                    Amount = 1234,
                    Reason = TransferOutReason.CashOut,
                    VoucherPrinted = true,
                    TypeOfAccount = AccountType.NonCash },
                new Ticket());

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.NonTransferablePromoTicketOut, It.IsAny<long>(), 0, 0, 0, 0), Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TicketOut), Times.Once());
        }

        [TestMethod]
        public void ConsumesLargeWinTest()
        {
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashOutJackpot, It.IsAny<long>(), 0, 0, 0, 0)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.CashoutJackpot)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TicketOut)).Verifiable();
            VoucherIssuedEvent evt = new(
                new VoucherOutTransaction { Amount = 1234, Reason = TransferOutReason.LargeWin, VoucherPrinted = true },
                new Ticket());

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashOutJackpot, It.IsAny<long>(), 0, 0, 0, 0), Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.CashoutJackpot), Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TicketOut), Times.Once());
        }

        [TestMethod]
        public void ConsumesBonusWinTest()
        {
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashoutBonus, It.IsAny<long>(), 0, 0, 0, 0)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.CashoutBonus)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TicketOut)).Verifiable();

            VoucherIssuedEvent evt = new(
                new VoucherOutTransaction { Amount = 1234, Reason = TransferOutReason.BonusPay, VoucherPrinted = true },
                new Ticket());

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashoutBonus, It.IsAny<long>(), 0, 0, 0, 0), Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.CashoutBonus), Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TicketOut), Times.Once());
        }

        [TestMethod]
        public void ConsumesZeroAmountDoesntReportTest()
        {
            var evt = new VoucherIssuedEvent(new VoucherOutTransaction { Amount = 0, VoucherPrinted = true }, new Ticket());
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashOut, It.IsAny<long>(), 0, 0, 0, 0)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TicketOut)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashOut, It.IsAny<long>(), 0, 0, 0, 0), Times.Never());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TicketOut), Times.Never());
        }

        [TestMethod]
        public void ConsumesVoucherNotPrintedTest()
        {
            var evt = new VoucherIssuedEvent(new VoucherOutTransaction { Amount = 0, VoucherPrinted = false }, new Ticket());
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashOut, It.IsAny<long>(), 0, 0, 0, 0)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.VoucherIssueTimeout)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashOut, It.IsAny<long>(), 0, 0, 0, 0), Times.Never());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.VoucherIssueTimeout), Times.Once());
        }
    }
}