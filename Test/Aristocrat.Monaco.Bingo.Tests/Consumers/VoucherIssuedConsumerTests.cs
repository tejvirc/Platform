namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Accounting.Contracts.TransferOut;
    using Aristocrat.Monaco.Bingo.Common;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Bingo.Consumers;
    using Gaming.Contracts;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Protocol.Common.Storage.Entity;

    [TestClass]
    public class VoucherIssuedConsumerTests
    {
        private VoucherIssuedConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Loose);
        private readonly Mock<IReportTransactionQueueService> _reportingService = new(MockBehavior.Strict);
        private readonly Mock<IReportEventQueueService> _bingoEventQueue = new(MockBehavior.Strict);
        private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactory = new(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> _propertiesManager = new(MockBehavior.Default);

        private readonly VoucherOutTransaction _transaction = new(
            0,
            DateTime.Now,
            1234L.CentsToMillicents(),
            AccountType.Cashable,
            "TestBarcode",
            0,
            string.Empty)
        {
            VoucherPrinted = true
        };

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new VoucherIssuedConsumer(
                _eventBus.Object,
                _consumerContext.Object,
                _reportingService.Object,
                _bingoEventQueue.Object,
                _unitOfWorkFactory.Object,
                _propertiesManager.Object);

            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.IsGameRunning, It.IsAny<bool>())).Returns(false);
        }

        [DataRow(true, false, false, false, false, DisplayName = "Transaction Reporting Service Null")]
        [DataRow(false, true, false, false, false, DisplayName = "EventBus Null")]
        [DataRow(false, false, true, false, false, DisplayName = "Event Reporting Service Null")]
        [DataRow(false, false, false, true, false, DisplayName = "Unit of Work Factory Null")]
        [DataRow(false, false, false, false, true, DisplayName = "Properties manager Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(
            bool reportingNull,
            bool eventBusNull,
            bool queueNull,
            bool nullUnitOfWork,
            bool nullProperties)
        {
            _target = new VoucherIssuedConsumer(
                eventBusNull ? null : _eventBus.Object,
                _consumerContext.Object,
                reportingNull ? null : _reportingService.Object,
                queueNull ? null : _bingoEventQueue.Object,
                nullUnitOfWork ? null : _unitOfWorkFactory.Object,
                nullProperties ? null : _propertiesManager.Object);
        }

        [TestMethod]
        public void ConsumesCashoutCashableTest()
        {
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashOut, _transaction.Amount.MillicentsToCents(), 0, 0, 0, 0, _transaction.Barcode)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TicketOut)).Verifiable();

            _target.Consume(new VoucherIssuedEvent(_transaction, new Ticket()));

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashOut, _transaction.Amount.MillicentsToCents(), 0, 0, 0, 0, _transaction.Barcode), Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TicketOut), Times.Once());
        }

        [TestMethod]
        public void ConsumesCashoutPromoTest()
        {
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashPromoTicketOut, _transaction.Amount.MillicentsToCents(), 0, 0, 0, 0, _transaction.Barcode)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TicketOut)).Verifiable();

            var transaction = (VoucherOutTransaction)_transaction.Clone();
            transaction.Reason = TransferOutReason.CashOut;
            transaction.TypeOfAccount = AccountType.Promo;
            VoucherIssuedEvent evt = new(transaction, new Ticket());

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashPromoTicketOut, _transaction.Amount.MillicentsToCents(), 0, 0, 0, 0, _transaction.Barcode), Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TicketOut), Times.Once());
        }

        [TestMethod]
        public void ConsumesCashoutNonCashTest()
        {
            var transaction = (VoucherOutTransaction)_transaction.Clone();
            transaction.Reason = TransferOutReason.CashOut;
            transaction.TypeOfAccount = AccountType.NonCash;
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.NonTransferablePromoTicketOut, transaction.Amount.MillicentsToCents(), 0, 0, 0, 0, transaction.Barcode)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TicketOut)).Verifiable();

            var evt = new VoucherIssuedEvent(transaction, new Ticket());

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.NonTransferablePromoTicketOut, transaction.Amount.MillicentsToCents(), 0, 0, 0, 0, transaction.Barcode), Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TicketOut), Times.Once());
        }

        [TestMethod]
        public void ConsumesLargeWinTest()
        {
            var transaction = (VoucherOutTransaction)_transaction.Clone();
            transaction.Reason = TransferOutReason.LargeWin;
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.Jackpot, transaction.Amount.MillicentsToCents(), 0, 0, 0, 0, transaction.Barcode)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.VoucherIssuedJackpot)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TicketOut)).Verifiable();
            var evt = new VoucherIssuedEvent(transaction, new Ticket());

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.Jackpot, transaction.Amount.MillicentsToCents(), 0, 0, 0, 0, transaction.Barcode), Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.VoucherIssuedJackpot), Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TicketOut), Times.Once());
        }

        [TestMethod]
        public void ConsumesBonusWinTest()
        {
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashoutExternalBonus, _transaction.Amount.MillicentsToCents(), 0, 0, 0, 0, _transaction.Barcode)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.CashoutBonus)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TicketOut)).Verifiable();

            var transaction = (VoucherOutTransaction)_transaction.Clone();
            transaction.Reason = TransferOutReason.BonusPay;
            var evt = new VoucherIssuedEvent(transaction, new Ticket());

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashoutExternalBonus, _transaction.Amount.MillicentsToCents(), 0, 0, 0, 0, _transaction.Barcode), Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.CashoutBonus), Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TicketOut), Times.Once());
        }

        [TestMethod]
        public void ConsumesCashWinTest()
        {
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashOut, _transaction.Amount.MillicentsToCents(), 0, 0, 0, 0, _transaction.Barcode)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TicketOut)).Verifiable();

            var transaction = (VoucherOutTransaction)_transaction.Clone();
            transaction.Reason = TransferOutReason.CashWin;
            var evt = new VoucherIssuedEvent(transaction, new Ticket());

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashOut, _transaction.Amount.MillicentsToCents(), 0, 0, 0, 0, _transaction.Barcode), Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TicketOut), Times.Once());
        }

        [TestMethod]
        public void ConsumesZeroAmountDoesntReportTest()
        {
            var transaction = (VoucherOutTransaction)_transaction.Clone();
            transaction.Amount = 0;
            var evt = new VoucherIssuedEvent(transaction, new Ticket());
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashOut, 0, 0, 0, 0, 0, transaction.Barcode)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TicketOut)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashOut, 0, 0, 0, 0, 0, transaction.Barcode), Times.Never());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TicketOut), Times.Never());
        }

        [TestMethod]
        public void ConsumesVoucherNotPrintedTest()
        {
            var transaction = (VoucherOutTransaction)_transaction.Clone();
            transaction.Amount = 0;
            transaction.VoucherPrinted = false;
            var evt = new VoucherIssuedEvent(transaction, new Ticket());
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashOut, 0, 0, 0, 0, 0, transaction.Barcode)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.VoucherIssueTimeout)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashOut, 0, 0, 0, 0, 0, transaction.Barcode), Times.Never());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.VoucherIssueTimeout), Times.Once());
        }
    }
}