namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Bingo.Common;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Bingo.Consumers;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Protocol.Common.Storage.Entity;

    [TestClass]
    public class VoucherRedeemedConsumerTests
    {
        private const long TestAmount = 1234000;
        private VoucherRedeemedConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Loose);
        private readonly Mock<IReportTransactionQueueService> _reportingService = new(MockBehavior.Strict);
        private readonly Mock<IReportEventQueueService> _bingoEventQueue = new(MockBehavior.Strict);
        private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactory = new(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> _propertiesManager = new(MockBehavior.Default);
        private readonly Mock<IGameProvider> _gameProvider = new(MockBehavior.Default);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new VoucherRedeemedConsumer(
                _eventBus.Object,
                _consumerContext.Object,
                _reportingService.Object,
                _bingoEventQueue.Object,
                _unitOfWorkFactory.Object,
                _gameProvider.Object);

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
            bool nullGameProvider)
        {

            //IEventBus eventBus,
            //ISharedConsumer consumerContext,
            //IReportTransactionQueueService bingoTransactionReportHandler,
            //IReportEventQueueService bingoEventQueue,
            //IUnitOfWorkFactory unitOfWorkFactory,
            //IGameProvider gameProvider)
            _target = new VoucherRedeemedConsumer(
                eventBusNull ? null : _eventBus.Object,
                _consumerContext.Object,
                reportingNull ? null : _reportingService.Object,
                queueNull ? null : _bingoEventQueue.Object,
                nullUnitOfWork ? null : _unitOfWorkFactory.Object,
                nullGameProvider ? null : _gameProvider.Object);
        }

        [TestMethod]
        public void ConsumesPromoTest()
        {
            var evt = new VoucherRedeemedEvent(
                new VoucherInTransaction(0, DateTime.Now, "TestBarcode")
                {
                    Amount = TestAmount, TypeOfAccount = AccountType.Promo
                });
            _reportingService.Setup(
                m => m.AddNewTransactionToQueue(
                    Common.TransactionType.CashPromoTicketIn, TestAmount.MillicentsToCents(), 0, 0, 0, 0, "TestBarcode")).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TicketIn)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(
                m => m.AddNewTransactionToQueue(
                    Common.TransactionType.CashPromoTicketIn, TestAmount.MillicentsToCents(), 0, 0, 0, 0, "TestBarcode"), Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TicketIn), Times.Once());
        }

        [TestMethod]
        public void ConsumesNonCashTest()
        {
            var evt = new VoucherRedeemedEvent(
                new VoucherInTransaction(0, DateTime.Now, "TestBarcode")
                {
                    Amount = TestAmount, TypeOfAccount = AccountType.NonCash
                });
            _reportingService.Setup(
                m => m.AddNewTransactionToQueue(
                    Common.TransactionType.TransferablePromoTicketIn, TestAmount.MillicentsToCents(), 0, 0, 0, 0, "TestBarcode")).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TicketIn)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(
                m => m.AddNewTransactionToQueue(
                    Common.TransactionType.TransferablePromoTicketIn, TestAmount.MillicentsToCents(), 0, 0, 0, 0, "TestBarcode"), Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TicketIn), Times.Once());
        }

        [TestMethod]
        public void ConsumesCashTest()
        {
            var evt = new VoucherRedeemedEvent(
                new VoucherInTransaction(0, DateTime.Now, "TestBarcode")
                {
                    Amount = TestAmount, TypeOfAccount = AccountType.Cashable
                });
            _reportingService.Setup(
                m => m.AddNewTransactionToQueue(
                    Common.TransactionType.TicketIn, TestAmount.MillicentsToCents(), 0, 0, 0, 0, "TestBarcode")).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TicketIn)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(
                m => m.AddNewTransactionToQueue(
                    Common.TransactionType.TicketIn, TestAmount.MillicentsToCents(), 0, 0, 0, 0, "TestBarcode"), Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TicketIn), Times.Once());
        }

        [TestMethod]
        public void ConsumesZeroAmountDoesntReportTest()
        {
            var evt = new VoucherRedeemedEvent(
                new VoucherInTransaction(0, DateTime.Now, "TestBarcode") { Amount = 0, TypeOfAccount = AccountType.Cashable });
            _reportingService.Setup(
                m => m.AddNewTransactionToQueue(
                    Common.TransactionType.TicketIn, It.IsAny<long>(), 0, 0, 0, 0, "TestBarcode")).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.TicketIn)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(
                m => m.AddNewTransactionToQueue(
                    Common.TransactionType.TicketIn, It.IsAny<long>(), 0, 0, 0, 0, "TestBarcode"), Times.Never());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.TicketIn), Times.Never());
        }
    }
}