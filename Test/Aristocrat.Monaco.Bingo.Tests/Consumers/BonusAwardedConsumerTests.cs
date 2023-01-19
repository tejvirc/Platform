namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Bingo.Consumers;
    using Common;
    using Gaming.Contracts;
    using Gaming.Contracts.Bonus;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Protocol.Common.Storage.Entity;

    [TestClass]
    public class BonusAwardedConsumerTests
    {
        private BonusAwardedConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Loose);
        private readonly Mock<IReportEventQueueService> _bingoEventQueue = new(MockBehavior.Default);
        private readonly Mock<IReportTransactionQueueService> _transactionQueue = new(MockBehavior.Default);
        private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactory = new(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> _properties = new(MockBehavior.Default);
        private readonly Mock<IGameProvider> _gameProvider = new(MockBehavior.Default);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
            _properties.Setup(x => x.GetProperty(GamingConstants.IsGameRunning, It.IsAny<bool>())).Returns(false);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
        }

        [DataRow(true, false, false, false, false)]
        [DataRow(false, true, false, false, false)]
        [DataRow(false, false, true, false, false)]
        [DataRow(false, false, false, true, false)]
        [DataRow(false, false, false, false, true)]
        [DataTestMethod]
        public void NullConstructorParametersTest(
            bool eventBusNull,
            bool queueNull,
            bool transactionNull,
            bool unitOfWorkNull,
            bool propertiesNull)
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => _ = CreateTarget(eventBusNull, queueNull, transactionNull, unitOfWorkNull, propertiesNull));
        }

        [DataRow(PayMethod.Any)]
        [DataRow(PayMethod.Credit)]
        [DataRow(PayMethod.Wat)]
        [DataTestMethod]
        public void ConsumesNormalBonusTest(PayMethod method)
        {
            const long paidAmount = 1000000;
            var bonusTransaction = new BonusTransaction
            {
                Mode = BonusMode.Standard, PayMethod = method, PaidCashableAmount = paidAmount
            };

            _target.Consume(new BonusAwardedEvent(bonusTransaction));

            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.BonusWinAwarded), Times.Once);
            _transactionQueue.Verify(
                m => m.AddNewTransactionToQueue(
                    TransactionType.ExternalBonusWin,
                    paidAmount.MillicentsToCents(),
                    0,
                    0,
                    0,
                    0,
                    string.Empty),
                Times.Once);
        }

        [TestMethod]
        public void ConsumesGameWinBonusTest()
        {
            const long paidAmount = 1000000;
            var bonusTransaction = new BonusTransaction
            {
                Mode = BonusMode.GameWin, PayMethod = PayMethod.Any, PaidCashableAmount = paidAmount
            };

            _target.Consume(new BonusAwardedEvent(bonusTransaction));

            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(It.IsAny<ReportableEvent>()), Times.Never);
            _transactionQueue.Verify(
                m => m.AddNewTransactionToQueue(
                    It.IsAny<TransactionType>(),
                    It.IsAny<long>(),
                    It.IsAny<uint>(),
                    It.IsAny<int>(),
                    It.IsAny<long>(),
                    It.IsAny<int>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [DataRow(PayMethod.Handpay, ReportableEvent.BonusLargeWinAwarded)]
        [DataRow(PayMethod.Voucher, ReportableEvent.BonusWinAwarded)]
        [DataTestMethod]
        public void ConsumesNoTransactionReportBonusTest(PayMethod method, ReportableEvent reportableEvent)
        {
            const long paidAmount = 1000000;
            var bonusTransaction = new BonusTransaction
            {
                Mode = BonusMode.Standard, PayMethod = method, PaidCashableAmount = paidAmount
            };

            _target.Consume(new BonusAwardedEvent(bonusTransaction));

            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(reportableEvent), Times.Once);
            _transactionQueue.Verify(
                m => m.AddNewTransactionToQueue(
                    It.IsAny<TransactionType>(),
                    It.IsAny<long>(),
                    It.IsAny<uint>(),
                    It.IsAny<int>(),
                    It.IsAny<long>(),
                    It.IsAny<int>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        private BonusAwardedConsumer CreateTarget(
            bool eventBusNull = false,
            bool queueNull = false,
            bool transactionNull = false,
            bool unitOfWorkNull = false,
            bool gameProviderNull = false)
        {
            return new BonusAwardedConsumer(
                eventBusNull ? null : _eventBus.Object,
                _consumerContext.Object,
                queueNull ? null : _bingoEventQueue.Object,
                transactionNull ? null : _transactionQueue.Object,
                unitOfWorkNull ? null : _unitOfWorkFactory.Object,
                gameProviderNull ? null : _gameProvider.Object);
        }
    }
}