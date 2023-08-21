namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Accounting.Contracts.Wat;
    using Aristocrat.Monaco.Bingo.Common;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Bingo.Consumers;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Protocol.Common.Storage.Entity;

    [TestClass]
    public class WatOnCompleteConsumerTests
    {
        private const long TestAmount = 10000;
        private WatOnCompleteConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Loose);
        private readonly Mock<IReportTransactionQueueService> _reportingService = new(MockBehavior.Strict);
        private readonly Mock<IReportEventQueueService> _bingoEventQueue = new(MockBehavior.Strict);
        private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactory = new(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> _propertiesManager = new(MockBehavior.Default);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new WatOnCompleteConsumer(
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
            _target = new WatOnCompleteConsumer(
                eventBusNull ? null : _eventBus.Object,
                _consumerContext.Object,
                reportingNull ? null : _reportingService.Object,
                queueNull ? null : _bingoEventQueue.Object,
                nullUnitOfWork ? null : _unitOfWorkFactory.Object,
                nullProperties ? null : _propertiesManager.Object);
        }

        [TestMethod]
        public void ConsumesPromoTest()
        {
            var evt = new WatOnCompleteEvent(new WatOnTransaction
            {
                Status = WatStatus.Complete,
                TransferredPromoAmount = TestAmount,
                PromoAmount = TestAmount
            });
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashPromoTransferIn, TestAmount.MillicentsToCents(), 0, 0, 0, 0, string.Empty)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TransferInComplete)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashPromoTransferIn, TestAmount.MillicentsToCents(), 0, 0, 0, 0, string.Empty),
                Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TransferInComplete), Times.Once());
        }

        [TestMethod]
        public void ConsumesNonCashTest()
        {
            var evt = new WatOnCompleteEvent(new WatOnTransaction
            {
                Status = WatStatus.Complete,
                TransferredNonCashAmount = TestAmount,
                NonCashAmount = TestAmount
            });
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.TransferablePromoTransferIn, TestAmount.MillicentsToCents(), 0, 0, 0, 0, string.Empty)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TransferInComplete)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.TransferablePromoTransferIn, TestAmount.MillicentsToCents(), 0, 0, 0, 0, string.Empty),
                Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TransferInComplete), Times.Once());
        }

        [TestMethod]
        public void ConsumesCashableTest()
        {
            var evt = new WatOnCompleteEvent(new WatOnTransaction
            {
                Status = WatStatus.Complete,
                TransferredCashableAmount = TestAmount,
                CashableAmount = TestAmount
            });
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.TransferIn, TestAmount.MillicentsToCents(), 0, 0, 0, 0, string.Empty)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TransferInComplete)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.TransferIn, TestAmount.MillicentsToCents(), 0, 0, 0, 0, string.Empty),
                Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TransferInComplete), Times.Once());
        }

        [TestMethod]
        public void ConsumesMultipleTypesTest()
        {
            var evt = new WatOnCompleteEvent(new WatOnTransaction
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
                Common.TransactionType.TransferIn, TestAmount.MillicentsToCents(), 0, 0, 0, 0, string.Empty)).Verifiable();
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashPromoTransferIn, TestAmount.MillicentsToCents(), 0, 0, 0, 0, string.Empty)).Verifiable();
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.TransferablePromoTransferIn, TestAmount.MillicentsToCents(), 0, 0, 0, 0, string.Empty)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TransferInComplete)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.TransferIn, TestAmount.MillicentsToCents(), 0, 0, 0, 0, string.Empty),
                Times.Once());
            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashPromoTransferIn, TestAmount.MillicentsToCents(), 0, 0, 0, 0, string.Empty),
                Times.Once());
            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.TransferablePromoTransferIn, TestAmount.MillicentsToCents(), 0, 0, 0, 0, string.Empty),
                Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TransferInComplete), Times.Once());
        }

        [TestMethod]
        public void ConsumesNotCompleteTest()
        {
            var evt = new WatOnCompleteEvent(new WatOnTransaction { Status = WatStatus.Initiated });
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.TransferIn, 0, 0, 0, 0, 0, string.Empty)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TransferInComplete)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.TransferIn, 0, 0, 0, 0, 0, string.Empty),
                Times.Never());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TransferInComplete), Times.Never());
        }

        [TestMethod]
        public void ConsumesTransferRefusedTest()
        {
            var evt = new WatOnCompleteEvent(new WatOnTransaction { Status = WatStatus.Rejected });
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.TransferIn, 0, 0, 0, 0, 0, string.Empty)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TransferInRefusedByEgm)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.TransferIn, 0, 0, 0, 0, 0, string.Empty),
                Times.Never());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TransferInRefusedByEgm), Times.Once());
        }

        [TestMethod]
        public void ConsumesTransferFailedTest()
        {
            var evt = new WatOnCompleteEvent(new WatOnTransaction { EgmException = (int)WatExceptionCode.PowerFailure });
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.TransferIn, 0, 0, 0, 0, 0, string.Empty)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TransferInFailed)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.TransferIn, 0, 0, 0, 0, 0, string.Empty),
                Times.Never());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TransferInFailed), Times.Once());
        }

        [TestMethod]
        public void ConsumesPartialTransferTest()
        {
            var evt = new WatOnCompleteEvent(new WatOnTransaction
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
                Common.TransactionType.TransferIn, TestAmount.MillicentsToCents(), 0, 0, 0, 0, string.Empty)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.PartialTransferInComplete)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TransferInComplete)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.TransferIn, TestAmount.MillicentsToCents(), 0, 0, 0, 0, string.Empty),
                Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.PartialTransferInComplete), Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TransferInComplete), Times.Never());
        }
    }
}