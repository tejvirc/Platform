namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Accounting.Contracts.Wat;
    using Aristocrat.Monaco.Bingo.Common;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Bingo.Consumers;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class WatTransferCompletedConsumerTests
    {
        private const long TestAmount = 10000;
        private WatTransferCompletedConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Loose);
        private readonly Mock<IReportTransactionQueueService> _reportingService = new(MockBehavior.Strict);
        private readonly Mock<IReportEventQueueService> _bingoEventQueue = new(MockBehavior.Strict);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new WatTransferCompletedConsumer(
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
            _target = new WatTransferCompletedConsumer(
                eventBusNull ? null : _eventBus.Object,
                _consumerContext.Object,
                reportingNull ? null : _reportingService.Object,
                queueNull ? null : _bingoEventQueue.Object);
        }

        [TestMethod]
        public void ConsumesPromoTest()
        {
            var evt = new WatTransferCompletedEvent(new WatTransaction
            {
                Status = WatStatus.Complete,
                TransferredPromoAmount = TestAmount,
                PromoAmount = TestAmount
            });
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashPromoTransferOut, TestAmount.MillicentsToCents(), 0, 0, 0, 0)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TransferOutComplete)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashPromoTransferOut, TestAmount.MillicentsToCents(), 0, 0, 0, 0),
                Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TransferOutComplete), Times.Once());
        }

        [TestMethod]
        public void ConsumesNonCashTest()
        {
            var evt = new WatTransferCompletedEvent(new WatTransaction
            {
                Status = WatStatus.Complete,
                TransferredNonCashAmount = TestAmount,
                NonCashAmount = TestAmount
            });
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.NonTransferablePromoTransferOut, TestAmount.MillicentsToCents(), 0, 0, 0, 0)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TransferOutComplete)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.NonTransferablePromoTransferOut, TestAmount.MillicentsToCents(), 0, 0, 0, 0),
                Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TransferOutComplete), Times.Once());
        }

        [TestMethod]
        public void ConsumesCashableTest()
        {
            var evt = new WatTransferCompletedEvent(new WatTransaction
            {
                Status = WatStatus.Complete,
                TransferredCashableAmount = TestAmount,
                CashableAmount = TestAmount
            });
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.TransferOut, TestAmount.MillicentsToCents(), 0, 0, 0, 0)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TransferOutComplete)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.TransferOut, TestAmount.MillicentsToCents(), 0, 0, 0, 0),
                Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TransferOutComplete), Times.Once());
        }

        [TestMethod]
        public void ConsumesMultipleTypesTest()
        {
            var evt = new WatTransferCompletedEvent(new WatTransaction
            {
                Status = WatStatus.Complete,
                TransferredCashableAmount = TestAmount,
                TransferredPromoAmount = TestAmount,
                TransferredNonCashAmount = TestAmount,
                CashableAmount = TestAmount,
                PromoAmount = TestAmount,
                NonCashAmount = TestAmount,
            });
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.TransferOut, TestAmount.MillicentsToCents(), 0, 0, 0, 0)).Verifiable();
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashPromoTransferOut, TestAmount.MillicentsToCents(), 0, 0, 0, 0)).Verifiable();
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.NonTransferablePromoTransferOut, TestAmount.MillicentsToCents(), 0, 0, 0, 0)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TransferOutComplete)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.TransferOut, TestAmount.MillicentsToCents(), 0, 0, 0, 0),
                Times.Once());
            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashPromoTransferOut, TestAmount.MillicentsToCents(), 0, 0, 0, 0),
                Times.Once());
            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.NonTransferablePromoTransferOut, TestAmount.MillicentsToCents(), 0, 0, 0, 0),
                Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TransferOutComplete), Times.Once());
        }

        [TestMethod]
        public void ConsumesNotCompleteTest()
        {
            var evt = new WatTransferCompletedEvent(new WatTransaction { Status = WatStatus.Initiated });
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.TransferOut, 0, 0, 0, 0, 0)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TransferOutComplete)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.TransferOut, 0, 0, 0, 0, 0),
                Times.Never());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TransferOutComplete), Times.Never());
        }

        [TestMethod]
        public void ConsumesTransferRefusedTest()
        {
            var evt = new WatTransferCompletedEvent(new WatTransaction { Status = WatStatus.Rejected });
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.TransferOut, 0, 0, 0, 0, 0)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TransferOutRefusedByEgm)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.TransferOut, 0, 0, 0, 0, 0),
                Times.Never());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TransferOutRefusedByEgm), Times.Once());
        }

        [TestMethod]
        public void ConsumesTransferFailedTest()
        {
            var evt = new WatTransferCompletedEvent(new WatTransaction { EgmException = (int)WatExceptionCode.PowerFailure });
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.TransferOut, 0, 0, 0, 0, 0)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TransferOutFailed)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.TransferOut, 0, 0, 0, 0, 0),
                Times.Never());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TransferOutFailed), Times.Once());
        }

        [TestMethod]
        public void ConsumesPartialTransferTest()
        {
            var evt = new WatTransferCompletedEvent(new WatTransaction
            (
                0,  // device id
                DateTime.Now,
                TestAmount, // cash
                0,          // promo
                0,          // non-cash
                true,       // allow partial
                "abc")      // request id
            { Status = WatStatus.Complete, TransferredCashableAmount = TestAmount });

            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.TransferOut, TestAmount.MillicentsToCents(), 0, 0, 0, 0)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.PartialTransferOutComplete)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TransferOutComplete)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.TransferOut, TestAmount.MillicentsToCents(), 0, 0, 0, 0),
                Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.PartialTransferOutComplete), Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TransferOutComplete), Times.Never());
        }
    }
}