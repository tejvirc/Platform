namespace Aristocrat.Monaco.Gaming.Tests.ConsumerTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Accounting.Contracts.Wat;
    using Consumers;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class WatTransferCommittedConsumerTests
    {
        private Mock<ICurrencyInContainer> _currencyHandler;
        private Mock<IGameHistory> _gameHistory;
        private Mock<IGameHistoryLog> _gameHistoryLog;
        private Mock<ITransactionHistory> _history;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IPersistentStorageManager> _persistentStorage;
        private Mock<ISessionInfoService> _sessionInfoService;
        private WatTransferCommittedConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _currencyHandler = new Mock<ICurrencyInContainer>(MockBehavior.Default);
            _gameHistory = new Mock<IGameHistory>(MockBehavior.Default);
            _history = new Mock<ITransactionHistory>(MockBehavior.Default);
            _sessionInfoService = new Mock<ISessionInfoService>(MockBehavior.Default);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _persistentStorage = new Mock<IPersistentStorageManager>(MockBehavior.Default);
            _gameHistoryLog = new Mock<IGameHistoryLog>(MockBehavior.Default);

            _propertiesManager
                .Setup(x => x.GetProperty(GamingConstants.MeterFreeGamesIndependently, It.IsAny<bool>()))
                .Returns(It.IsAny<bool>());

            _persistentStorage.Setup(s => s.ScopedTransaction()).Returns(new Mock<IScopedTransaction>().Object);

            _history.Setup(x => x.RecallTransactions()).Returns(new List<ITransaction>().OrderBy(x => x));

            _target = new WatTransferCommittedConsumer(
                _currencyHandler.Object,
                _gameHistory.Object,
                _history.Object,
                _sessionInfoService.Object,
                _propertiesManager.Object,
                _persistentStorage.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullCurrencyHandler()
        {
            _target = new WatTransferCommittedConsumer(
                null,
                _gameHistory.Object,
                _history.Object,
                _sessionInfoService.Object,
                _propertiesManager.Object,
                _persistentStorage.Object);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullGameHistoryHandler()
        {
            _target = new WatTransferCommittedConsumer(
                _currencyHandler.Object,
                null,
                _history.Object,
                _sessionInfoService.Object,
                _propertiesManager.Object,
                _persistentStorage.Object);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullHistoryHandler()
        {
            _target = new WatTransferCommittedConsumer(
                _currencyHandler.Object,
                _gameHistory.Object,
                null,
                _sessionInfoService.Object,
                _propertiesManager.Object,
                _persistentStorage.Object);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullSessionInfoHandler()
        {
            _target = new WatTransferCommittedConsumer(
                _currencyHandler.Object,
                _gameHistory.Object,
                _history.Object,
                null,
                _propertiesManager.Object,
                _persistentStorage.Object);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullPropertiesHandler()
        {
            _target = new WatTransferCommittedConsumer(
                _currencyHandler.Object,
                _gameHistory.Object,
                _history.Object,
                _sessionInfoService.Object,
                null,
                _persistentStorage.Object);
        }

        [DataRow(100, 100, false, DisplayName = "WAT Transaction same in and out amounts")]
        [DataRow(100, 50, false, DisplayName = "WAT Transaction different in and out amounts with no log")]
        [DataRow(0, 50, true, DisplayName = "WAT Transaction different in and out amounts with log")]
        [DataTestMethod]
        public void Consume(
            long inAmount,
            long outAmount,
            bool hasCurrentLog)
        {
            var transaction = new WatTransaction(
                    0,
                    DateTime.Now,
                    0,
                    0,
                    0,
                    false,
                    string.Empty)
                { AuthorizedCashableAmount = outAmount };

            var transactionEvent = new WatTransferCommittedEvent(transaction);
            _currencyHandler.Setup(x => x.AmountIn).Returns(inAmount);

            if (hasCurrentLog)
            {
                _gameHistory.Setup(x => x.CurrentLog).Returns(_gameHistoryLog.Object);
            }
            else
            {
                _gameHistory.Setup(x => x.CurrentLog).Returns((IGameHistoryLog)null);
            }

            _target.Consume(transactionEvent);

            if (hasCurrentLog)
            {
                _gameHistory.Verify(
                    x => x.AssociateTransactions(
                        It.IsAny<IEnumerable<TransactionInfo>>(),
                        It.IsAny<bool>()));
            }
        }
    }
}