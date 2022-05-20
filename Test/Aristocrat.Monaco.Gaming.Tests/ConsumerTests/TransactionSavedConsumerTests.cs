namespace Aristocrat.Monaco.Gaming.Tests.ConsumerTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Wat;
    using Consumers;
    using Contracts;
    using Contracts.Bonus;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class TransactionSavedConsumerTests
    {
        private Mock<ICurrencyInContainer> _currencyHandler;
        private Mock<IGameHistory> _gameHistory;
        private Mock<IGameHistoryLog> _gameHistoryLog;
        private Mock<ITransactionHistory> _history;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ISessionInfoService> _sessionInfoService;
        private Mock<IPersistentStorageManager> _persistentStorage;
        private TransactionSavedConsumer _target;

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

            _target = new TransactionSavedConsumer(
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
            _target = new TransactionSavedConsumer(
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
            _target = new TransactionSavedConsumer(
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
            _target = new TransactionSavedConsumer(
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
            _target = new TransactionSavedConsumer(
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
            _target = new TransactionSavedConsumer(
                _currencyHandler.Object,
                _gameHistory.Object,
                _history.Object,
                _sessionInfoService.Object,
                null,
                _persistentStorage.Object);
        }

        [DataRow(
            typeof(WatTransaction),
            false,
            false,
            false,
            0,
            0,
            false,
            DisplayName = "WAT Transaction")]
        [DataRow(
            typeof(BillTransaction),
            false,
            false,
            false,
            0,
            0,
            false,
            DisplayName = "Bill Transaction")]
        [DataRow(
            typeof(WatOnTransaction),
            false,
            false,
            false,
            0,
            0,
            false,
            DisplayName = "WAT On Transaction")]
        [DataRow(
            typeof(BonusTransaction),
            true,
            false,
            true,
            0,
            0,
            false,
            DisplayName = "Bonus Transaction")]
        [DataRow(
            typeof(VoucherOutTransaction),
            true,
            true,
            true,
            0,
            0,
            false,
            DisplayName = "Voucher Out Transaction")]
        [DataRow(
            typeof(HandpayTransaction),
            false,
            false,
            true,
            0,
            0,
            false,
            DisplayName = "Voucher Out Transaction 0 amount")]
        [DataRow(
            typeof(HandpayTransaction),
            true,
            true,
            true,
            100,
            100,
            false,
            DisplayName = "Voucher Out Transaction same in & out amounts")]
        [DataRow(
            typeof(HandpayTransaction),
            true,
            true,
            true,
            100,
            50,
            false,
            DisplayName = "Voucher Out Transaction different in & out amounts with no log")]
        [DataRow(
            typeof(HandpayTransaction),
            false,
            true,
            true,
            0,
            50,
            true,
            DisplayName = "Voucher Out Transaction different in & out amounts with log")]
        [DataTestMethod]
        public void Consume(
            Type transactionType,
            bool shouldCallCredit,
            bool shouldHandleTransactions,
            bool sessionUpdated,
            long inAmount,
            long outAmount,
            bool hasCurrentLog)
        {
            ITransaction transaction;
            if (transactionType == typeof(WatOnTransaction))
            {
                transaction = new WatOnTransaction(
                    0,
                    DateTime.Now,
                    0,
                    0,
                    0,
                    false,
                    string.Empty);
            }
            else if (transactionType == typeof(VoucherOutTransaction))
            {
                transaction = new VoucherOutTransaction(
                    0,
                    DateTime.Now,
                    0,
                    AccountType.Cashable,
                    string.Empty,
                    0,
                    0,
                    string.Empty);
            }
            else if (transactionType == typeof(HandpayTransaction))
            {
                transaction = (ITransaction)Activator.CreateInstance(transactionType);
                ((HandpayTransaction)transaction).KeyOffCashableAmount = outAmount;
            }
            else
            {
                transaction = (ITransaction)Activator.CreateInstance(transactionType);
            }

            var transactionEvent = new TransactionSavedEvent(transaction);
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


            if (sessionUpdated)
            {
                _sessionInfoService.Verify(x => x.HandleTransaction(transaction), Times.Once);
            }
            else
            {
                _sessionInfoService.Verify(x => x.HandleTransaction(transaction), Times.Never);
            }

            if (shouldHandleTransactions)
            {
                if (hasCurrentLog)
                {
                    _gameHistory.Verify(
                        x =>
                            x.AssociateTransactions(It.IsAny<IEnumerable<TransactionInfo>>(), It.IsAny<bool>()));
                }
            }
        }
    }
}