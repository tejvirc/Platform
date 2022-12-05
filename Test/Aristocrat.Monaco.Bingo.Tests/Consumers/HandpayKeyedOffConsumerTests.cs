namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Accounting.Contracts.Handpay;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Bingo.Consumers;
    using Common;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Protocol.Common.Storage.Entity;

    [TestClass]
    public class HandpayKeyedOffConsumerTests
    {
        private const long TestAmount = 10000;
        private const string TestBarcode = "TestBarcode";

        private HandpayKeyedOffConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Loose);
        private readonly Mock<IReportTransactionQueueService> _reportingService = new(MockBehavior.Strict);
        private readonly Mock<IReportEventQueueService> _bingoEventQueue = new(MockBehavior.Strict);
        private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactory = new(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> _propertiesManager = new(MockBehavior.Default);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new HandpayKeyedOffConsumer(
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
            _target = new HandpayKeyedOffConsumer(
                eventBusNull ? null : _eventBus.Object,
                _consumerContext.Object,
                reportingNull ? null : _reportingService.Object,
                queueNull ? null : _bingoEventQueue.Object,
                nullUnitOfWork ? null : _unitOfWorkFactory.Object,
                nullProperties ? null : _propertiesManager.Object);
        }

        [TestMethod]
        public void ConsumesCancelledCreditTest()
        {
            var evt = new HandpayKeyedOffEvent(
                new HandpayTransaction(1, DateTime.Now, TestAmount, 0, 0, 123, HandpayType.CancelCredit, false, new Guid())
                { KeyOffCashableAmount = TestAmount, Barcode = TestBarcode });
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                TransactionType.CancelledCredits, TestAmount.MillicentsToCents(), 0, 0, 0, 0, TestBarcode)).Verifiable();
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                TransactionType.HandPayKeyOff, TestAmount.MillicentsToCents(), 0, 0, 0, 0, TestBarcode)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.HandpayKeyOff)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.CancelCredits)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                TransactionType.CancelledCredits, TestAmount.MillicentsToCents(), 0, 0, 0, 0, TestBarcode),
                Times.Once());
            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                TransactionType.HandPayKeyOff, TestAmount.MillicentsToCents(), 0, 0, 0, 0, TestBarcode),
                Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.HandpayKeyOff), Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.CancelCredits), Times.Once());
        }

        [TestMethod]
        public void ConsumesGameWinTest()
        {
            var evt = new HandpayKeyedOffEvent(
                new HandpayTransaction(1, DateTime.Now, TestAmount, 0, 0, 123, HandpayType.GameWin, false, new Guid())
                { KeyOffCashableAmount = TestAmount, Barcode = TestBarcode });
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                TransactionType.HandPayKeyOff, TestAmount.MillicentsToCents(), 0, 0, 0, 0, TestBarcode)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.HandpayKeyOff)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.CashoutJackpot)).Verifiable();
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                TransactionType.CashOutJackpot, TestAmount.MillicentsToCents(), 0, 0, 0, 0, TestBarcode)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                TransactionType.HandPayKeyOff, TestAmount.MillicentsToCents(), 0, 0, 0, 0, TestBarcode),
                Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.HandpayKeyOff), Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.CashoutJackpot), Times.Once());
            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                TransactionType.CashOutJackpot, TestAmount.MillicentsToCents(), 0, 0, 0, 0, TestBarcode), Times.Once());
        }

        [TestMethod]
        public void ConsumesBonusPayTest()
        {
            var evt = new HandpayKeyedOffEvent(
                new HandpayTransaction(1, DateTime.Now, TestAmount, 0, 0, 123, HandpayType.BonusPay, false, new Guid())
                { KeyOffCashableAmount = TestAmount, Barcode = TestBarcode });
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                TransactionType.ExternalBonusLargeWin, TestAmount.MillicentsToCents(), 0, 0, 0, 0, TestBarcode)).Verifiable();
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                TransactionType.CashOutJackpot, TestAmount.MillicentsToCents(), 0, 0, 0, 0, TestBarcode)).Verifiable();
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                TransactionType.HandPayKeyOff, TestAmount.MillicentsToCents(), 0, 0, 0, 0, TestBarcode)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.HandpayKeyOff)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.CashoutExternalBonus)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                TransactionType.ExternalBonusLargeWin, TestAmount.MillicentsToCents(), 0, 0, 0, 0, TestBarcode),
                Times.Once());
            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                    TransactionType.CashOutJackpot, TestAmount.MillicentsToCents(), 0, 0, 0, 0, TestBarcode),
                Times.Once());
            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                TransactionType.HandPayKeyOff, TestAmount.MillicentsToCents(), 0, 0, 0, 0, TestBarcode),
                Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.HandpayKeyOff), Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.CashoutExternalBonus), Times.Once());
        }

        [TestMethod]
        public void ConsumesZeroAmountTest()
        {
            var evt = new HandpayKeyedOffEvent(
                new HandpayTransaction(1, DateTime.Now, 0, 0, 0, 123, HandpayType.CancelCredit, false, new Guid())
                {
                    Barcode = TestBarcode
                });
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                TransactionType.CashIn, 0, 0, 0, 0, 0, TestBarcode)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.HandpayKeyOff)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.CancelCredits)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                TransactionType.CashIn, TestAmount.MillicentsToCents(), 0, 0, 0, 0, TestBarcode),
                Times.Never());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.HandpayKeyOff), Times.Never());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.CancelCredits), Times.Never());
        }
    }
}