namespace Aristocrat.Monaco.Gaming.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Vouchers;
    using Contracts;
    using Contracts.Bonus;
    using Contracts.Meters;
    using Gaming.Bonus;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using PayMethod = Contracts.Bonus.PayMethod;

    // TODO: Fix these
    [TestClass]
    public class BonusHandlerTests
    {
/* 
        private const string TestHostTransactionId = "123456";
        private const long TestTransactionId = 7;
        private const long TestTransferAmount = 5000 * GamingConstants.Millicents;
        private readonly Guid _testGuid = new Guid("{A56A5964-4FE2-480A-BBEE-97E68CE39FF5}");

        private const int GameId = 2;
        private const long Denom = 1000;
        private const long LargeLimit = 119999000;
        private Mock<IServiceManager> _serviceManager;
        private Mock<IEventBus> _eventBus;
        private Mock<IGamePlayState> _gamePlayState;
        private Mock<IPersistentStorageManager> _persistentStorage;
        private Mock<IPropertiesManager> _properties;
        private Mock<IScopedTransaction> _scopedTransaction;
        private Mock<ITransactionHistory> _transactionHistory;
        private Mock<IMessageDisplay> _messageDisplay;
        private Mock<ITransactionCoordinator> _transactionCoordinator;
        private Mock<ISystemDisableManager> _disableManager;
        private Mock<IBank> _bank;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _serviceManager = MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _persistentStorage = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Strict);
            _scopedTransaction = MoqServiceManager.CreateAndAddService<IScopedTransaction>(MockBehavior.Strict);
            _transactionHistory = MoqServiceManager.CreateAndAddService<ITransactionHistory>(MockBehavior.Strict);
            _properties = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _messageDisplay = MoqServiceManager.CreateAndAddService<IMessageDisplay>(MockBehavior.Strict);
            _gamePlayState = MoqServiceManager.CreateAndAddService<IGamePlayState>(MockBehavior.Strict);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _transactionCoordinator = MoqServiceManager.CreateAndAddService<ITransactionCoordinator>(MockBehavior.Strict);
            _disableManager = MoqServiceManager.CreateAndAddService<ISystemDisableManager>(MockBehavior.Strict);
            MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _bank = MoqServiceManager.CreateAndAddService<IBank>(MockBehavior.Strict);
            MoqServiceManager.CreateAndAddService<IGameMeterManager>(MockBehavior.Strict);
            MoqServiceManager.CreateAndAddService<IBonusHandler>(MockBehavior.Strict);
            MoqServiceManager.CreateAndAddService<ITransferOutHandler>(MockBehavior.Strict);

            _serviceManager.Setup(s => s.GetService<IPropertiesManager>()).Returns(_properties.Object);
            _serviceManager.Setup(s => s.GetService<IBank>()).Returns(_bank.Object);

            _eventBus.Setup(e => e.Subscribe(It.IsAny<object>(), It.IsAny<Action<PrimaryGameStartedEvent>>()));
            _eventBus.Setup(e => e.Subscribe(It.IsAny<object>(), It.IsAny<Action<MessageRemovedEvent>>()));
            _eventBus.Setup(e => e.Subscribe(It.IsAny<object>(), It.IsAny<Action<BonusAwardedEvent>>()));
            _eventBus.Setup(e => e.Subscribe(It.IsAny<object>(), It.IsAny<Action<CancelBonusEvent>>()));
            _eventBus.Setup(e => e.Subscribe(It.IsAny<object>(), It.IsAny<Action<GameEndedEvent>>()));
            _eventBus.Setup(e => e.Publish(It.IsAny<BonusPendingEvent>()));
            _eventBus.Setup(e => e.Publish(It.IsAny<BonusCancelledEvent>()));
            _eventBus.Setup(e => e.Publish(It.IsAny<BonusFailedEvent>()));
            _eventBus.Setup(e => e.Publish(It.IsAny<GameDelayPeriodStartedEvent>()));
            _eventBus.Setup(e => e.Publish(It.IsAny<GameDelayPeriodEndedEvent>()));
            _eventBus.Setup(e => e.UnsubscribeAll(It.IsAny<object>()));
            _persistentStorage.Setup(p => p.ScopedTransaction()).Returns(_scopedTransaction.Object);
            _scopedTransaction.Setup(s => s.Complete());
            _scopedTransaction.Setup(s => s.Dispose());
            _transactionHistory.Setup(t => t.SaveTransaction(It.IsAny<ITransaction>()));
            _transactionHistory.Setup(t => t.UpdateTransaction(It.IsAny<ITransaction>()));
            _transactionCoordinator
                .Setup(t => t.RequestTransaction(It.IsAny<ITransactionRequestor>(), It.IsAny<TransactionType>()))
                .Returns(_testGuid);
            _gamePlayState.Setup(g => g.InGameRound).Returns(false);
            _disableManager.Setup(d => d.IsDisabled).Returns(false);
            _bank.Setup(b => b.QueryBalance()).Returns(0);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [DataRow(false, true, true, true, true, true, DisplayName = "Null IGamePlayState")]
        [DataRow(true, false, true, true, true, true, DisplayName = "Null ITransactionHistory")]
        [DataRow(true, true, false, true, true, true, DisplayName = "Null IEventBus")]
        [DataRow(true, true, true, false, true, true, DisplayName = "Null IPersistentStorageManager")]
        [DataRow(true, true, true, true, false, true, DisplayName = "Null IPropertiesManager")]
        [DataRow(true, true, true, true, true, false, DisplayName = "Null IMessageDisplay")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenSomethingIsNullExpectException(
            bool gamePlayState,
            bool transactionHistory,
            bool eventBus,
            bool persistentStorage,
            bool propertyManager,
            bool messageDisplay)
        {
            CreateBonusHandler(
                gamePlayState,
                transactionHistory,
                eventBus,
                persistentStorage,
                propertyManager,
                messageDisplay);
        }

        [TestMethod]
        public void WhenNothingIsNullExpectSuccess()
        {
            using (var handler = CreateBonusHandler())
            {
                Assert.IsNotNull(handler);
            }
        }

        [TestMethod]
        public void WhenSetTimedGameEndDelayExpectSuccess()
        {
            var delay = TimeSpan.FromSeconds(3);

            var handler = CreateBonusHandler();

            _gamePlayState.Setup(g => g.SetGameEndDelay(delay));
            handler.SetGameEndDelay(delay);

            Assert.AreEqual(handler.GameEndDelay, delay);

            _eventBus.Verify(m => m.Publish(It.IsAny<GameDelayPeriodStartedEvent>()));
        }

        [TestMethod]
        public void WhenUpdateTimedGameEndDelayGameRunningExpectSuccess()
        {
            var initialDelay = TimeSpan.FromSeconds(3);
            var newDelay = TimeSpan.FromSeconds(5);

            var handler = CreateBonusHandler();

            _gamePlayState.Setup(g => g.SetGameEndDelay(initialDelay));
            handler.SetGameEndDelay(initialDelay);

            _gamePlayState.Setup(g => g.InGameRound).Returns(true);
            _gamePlayState.Setup(g => g.CurrentState).Returns(PlayState.Initiated);
            _gamePlayState.Setup(g => g.SetGameEndDelay(newDelay));
            handler.SetGameEndDelay(newDelay);

            Assert.AreEqual(handler.GameEndDelay, newDelay);
        }

        [TestMethod]
        public void WhenUpdateTimedGameEndDelayGameEndedExpectSuccess()
        {
            var initialDelay = TimeSpan.FromSeconds(3);
            var newDelay = TimeSpan.FromSeconds(5);

            var handler = CreateBonusHandler();

            _gamePlayState.Setup(g => g.SetGameEndDelay(initialDelay));
            handler.SetGameEndDelay(initialDelay);

            _gamePlayState.Setup(g => g.InGameRound).Returns(true);
            _gamePlayState.Setup(g => g.CurrentState).Returns(PlayState.GameEnded);
            _gamePlayState.Setup(g => g.SetGameEndDelay(newDelay));
            handler.SetGameEndDelay(newDelay);

            Assert.AreEqual(handler.GameEndDelay, newDelay);
        }

        [TestMethod]
        public void WhenSetNoGameEndDelayExpectSuccess()
        {
            var handler = CreateBonusHandler();

            _gamePlayState.Setup(g => g.SetGameEndDelay(TimeSpan.Zero));
            handler.SetGameEndDelay(TimeSpan.Zero);

            Assert.AreEqual(handler.GameEndDelay, TimeSpan.Zero);

            _eventBus.Verify(m => m.Publish(It.IsAny<GameDelayPeriodEndedEvent>()));
        }

        [TestMethod]
        public void WhenCanSkipExpectSuccess()
        {
            var handler = CreateBonusHandler();

            _gamePlayState.SetupGet(m => m.InGameRound).Returns(true);
            _gamePlayState.SetupGet(m => m.CurrentState).Returns(PlayState.PrimaryGameStarted);
            _gamePlayState.Setup(g => g.SetGameEndDelay(TimeSpan.Zero));

            handler.SkipGameEndDelay();

            _gamePlayState.Verify(g => g.SetGameEndDelay(TimeSpan.Zero));
        }

        [TestMethod]
        public void WhenCantSkipExpectSuccess()
        {
            var handler = CreateBonusHandler();

            _gamePlayState.Setup(g => g.SetGameEndDelay(TimeSpan.Zero));
            _gamePlayState.SetupGet(m => m.InGameRound).Returns(false);

            handler.SkipGameEndDelay();

            _gamePlayState.Verify(g => g.SetGameEndDelay(TimeSpan.Zero), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenProvideNullRequestExpectException()
        {
            _properties.Setup(m => m.GetProperty(GamingConstants.IsGameRunning, false)).Returns(true);
            _properties.Setup(m => m.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(GameId);
            _properties.Setup(m => m.GetProperty(GamingConstants.SelectedDenom, 0L)).Returns(Denom);

            var handler = CreateBonusHandler();

            handler.Award((BonusRequest)null);
        }

        [TestMethod]
        public void WhenCreateAwardExpectSuccess()
        {
            _properties.Setup(m => m.GetProperty(GamingConstants.IsGameRunning, false)).Returns(true);
            _properties.Setup(m => m.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(GameId);
            _properties.Setup(m => m.GetProperty(GamingConstants.SelectedDenom, 0L)).Returns(Denom);
            _properties.Setup(m => m.GetProperty(AccountingConstants.LargeWinLimit, It.IsAny<long>()))
                .Returns(LargeLimit);
            _properties.Setup(m => m.GetProperty(AccountingConstants.MaxCreditMeter, It.IsAny<long>()))
                .Returns(LargeLimit);

            var handler = CreateBonusHandler();

            var request = new BonusRequest(
                TestHostTransactionId,
                TestTransferAmount,
                0,
                0,
                PayMethod.Any);

            var transaction = handler.Award(request);

            _transactionHistory.Verify(m => m.SaveTransaction(It.IsAny<BonusTransaction>()));
            _scopedTransaction.Verify(m => m.Complete());

            _eventBus.Verify(m => m.Publish(It.IsAny<BonusPendingEvent>()));

            Assert.IsNotNull(transaction);
            Assert.AreEqual(transaction.BonusId, TestHostTransactionId);
            Assert.AreEqual(transaction.CashableAmount, TestTransferAmount);
            Assert.AreEqual(transaction.PayMethod, PayMethod.Any);
            Assert.AreEqual(transaction.Exception, 0);
        }

        [TestMethod]
        public void WhenCreateAwardNotInGameExpectFailure()
        {
            _properties.Setup(m => m.GetProperty(GamingConstants.IsGameRunning, false)).Returns(false);
            _properties.Setup(m => m.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(GameId);
            _properties.Setup(m => m.GetProperty(GamingConstants.SelectedDenom, 0L)).Returns(Denom);

            var handler = CreateBonusHandler();

            var request = new BonusRequest(
                TestHostTransactionId,
                TestTransferAmount,
                0,
                0,
                PayMethod.Any);

            var transaction = handler.Award(request);

            _transactionHistory.Verify(m => m.SaveTransaction(It.IsAny<BonusTransaction>()));
            _scopedTransaction.Verify(m => m.Complete());

            _eventBus.Verify(m => m.Publish(It.IsAny<BonusPendingEvent>()));

            Assert.IsNotNull(transaction);
            Assert.AreEqual(transaction.BonusId, TestHostTransactionId);
            Assert.AreEqual(transaction.CashableAmount, TestTransferAmount);
            Assert.AreEqual(transaction.PayMethod, PayMethod.Any);
            Assert.AreEqual(transaction.Exception, 99);
        }

        [TestMethod]
        public void WhenCancelWithInvalidBonusIdExpectFail()
        {
            _transactionHistory.Setup(m => m.RecallTransactions<BonusTransaction>())
                .Returns(Enumerable.Empty<BonusTransaction>().ToList());

            var handler = CreateBonusHandler();

            Assert.IsFalse(handler.Cancel(TestHostTransactionId));
        }

        [TestMethod]
        public void WhenCancelWithValidBonusIdExpectSuccess()
        {
            _transactionHistory.Setup(m => m.RecallTransactions<BonusTransaction>())
                .Returns(
                    new List<BonusTransaction>
                    {
                        new BonusTransaction(
                            1,
                            DateTime.UtcNow,
                            TestHostTransactionId,
                            TestTransferAmount,
                            0,
                            0,
                            GameId,
                            Denom,
                            PayMethod.Any)
                    });

            var handler = CreateBonusHandler();

            Assert.IsTrue(handler.Cancel(TestHostTransactionId));
        }

        [TestMethod]
        public void WhenAcknowledgeWithValidTransactionExpectSuccess()
        {
            var bonus = new BonusTransaction(
                1,
                DateTime.UtcNow,
                TestHostTransactionId,
                TestTransferAmount,
                0,
                0,
                GameId,
                Denom,
                PayMethod.Any)
            {
                TransactionId = TestTransactionId
            };

            _transactionHistory.Setup(m => m.RecallTransactions<BonusTransaction>())
                .Returns(new List<BonusTransaction> { bonus });

            var handler = CreateBonusHandler();

            handler.Acknowledge(TestTransactionId);

            Assert.AreEqual(bonus.State, BonusState.Acknowledged);

            _transactionHistory.Verify(m => m.UpdateTransaction(bonus));
        }

        [TestMethod]
        public void WhenAcknowledgeWithValidBonusIdExpectSuccess()
        {
            var bonus = new BonusTransaction(
                1,
                DateTime.UtcNow,
                TestHostTransactionId,
                TestTransferAmount,
                0,
                0,
                GameId,
                Denom,
                PayMethod.Any)
            {
                TransactionId = TestTransactionId
            };

            _transactionHistory.Setup(m => m.RecallTransactions<BonusTransaction>())
                .Returns(new List<BonusTransaction> { bonus });

            var handler = CreateBonusHandler();

            handler.Acknowledge(TestHostTransactionId);

            Assert.AreEqual(bonus.State, BonusState.Acknowledged);
        }

        [TestMethod]
        public void WhenCancelWithInvalidTransactionIdExpectFail()
        {
            _transactionHistory.Setup(m => m.RecallTransactions<BonusTransaction>())
                .Returns(Enumerable.Empty<BonusTransaction>().ToList());

            var handler = CreateBonusHandler();

            Assert.IsFalse(handler.Cancel(TestTransactionId));
        }

        [TestMethod]
        public void WhenCancelWithValidTransactionIdExpectFail()
        {
            var bonus = new BonusTransaction(
                1,
                DateTime.UtcNow,
                TestHostTransactionId,
                TestTransferAmount,
                0,
                0,
                GameId,
                Denom,
                PayMethod.Any)
            {
                TransactionId = TestTransactionId
            };

            _transactionHistory.Setup(m => m.RecallTransactions<BonusTransaction>())
                .Returns(new List<BonusTransaction> { bonus });

            var handler = CreateBonusHandler();

            Assert.IsTrue(handler.Cancel(TestHostTransactionId));

            Assert.AreEqual(bonus.PaidAmount, 0);
            Assert.AreEqual(bonus.State, BonusState.Committed);

            _transactionHistory.Verify(m => m.UpdateTransaction(bonus));

            _eventBus.Verify(m => m.Publish(It.IsAny<BonusCancelledEvent>()));
        }

        [TestMethod]
        public void WhenCheckExistWithInvalidBonusIdExpectFail()
        {
            _transactionHistory.Setup(m => m.RecallTransactions<BonusTransaction>())
                .Returns(Enumerable.Empty<BonusTransaction>().ToList());

            var handler = CreateBonusHandler();

            Assert.IsFalse(handler.Exists(TestHostTransactionId));
        }

        [TestMethod]
        public void WhenCheckExistWithValidBonusIdExpectFail()
        {
            _transactionHistory.Setup(m => m.RecallTransactions<BonusTransaction>())
                .Returns(
                    new List<BonusTransaction>
                    {
                        new BonusTransaction(
                            1,
                            DateTime.UtcNow,
                            TestHostTransactionId,
                            TestTransferAmount,
                            0,
                            0,
                            GameId,
                            Denom,
                            PayMethod.Any)
                        {
                            TransactionId = TestTransactionId
                        }
                    });

            var handler = CreateBonusHandler();

            Assert.IsTrue(handler.Exists(TestHostTransactionId));
        }

        [TestMethod]
        public void WhenCheckExistWithInvalidTransactionIdExpectFail()
        {
            _transactionHistory.Setup(m => m.RecallTransactions<BonusTransaction>())
                .Returns(Enumerable.Empty<BonusTransaction>().ToList());

            var handler = CreateBonusHandler();

            Assert.IsFalse(handler.Exists(TestTransactionId));
        }

        [TestMethod]
        public void WhenCheckExistWithValidTransactionIdExpectFail()
        {
            _transactionHistory.Setup(m => m.RecallTransactions<BonusTransaction>())
                .Returns(
                    new List<BonusTransaction>
                    {
                        new BonusTransaction(
                            1,
                            DateTime.UtcNow,
                            TestHostTransactionId,
                            TestTransferAmount,
                            0,
                            0,
                            GameId,
                            Denom,
                            PayMethod.Any)
                        {
                            TransactionId = TestTransactionId
                        }
                    });

            var handler = CreateBonusHandler();

            Assert.IsTrue(handler.Exists(TestTransactionId));
        }

        [TestMethod]
        public void WhenSetExpiringGameEndDelayExpectSuccess()
        {
            var delay = TimeSpan.FromSeconds(3);
            var duration = TimeSpan.FromMinutes(10);
            const int games = 10;

            var handler = CreateBonusHandler();

            _gamePlayState.Setup(g => g.SetGameEndDelay(delay));
            handler.SetGameEndDelay(delay, duration, games, true);

            Assert.AreEqual(handler.GameEndDelay, delay);
            Assert.AreEqual(handler.DelayDuration, duration);
            Assert.AreEqual(handler.DelayedGames, games);
            Assert.IsTrue(handler.EvaluateBoth);
        }

        private BonusHandler CreateBonusHandler()
        {
            return CreateBonusHandler(true, true, true, true, true, true);
        }

        private BonusHandler CreateBonusHandler(
            bool gamePlayState,
            bool transactionHistory,
            bool eventBus,
            bool persistentStorage,
            bool propertyManager,
            bool messageDisplay)
        {
            return new BonusHandler(
                gamePlayState ? _gamePlayState.Object : null,
                transactionHistory ? _transactionHistory.Object : null,
                eventBus ? _eventBus.Object : null,
                persistentStorage ? _persistentStorage.Object : null,
                propertyManager ? _properties.Object : null,
                messageDisplay ? _messageDisplay.Object : null);
        }
*/
    }
}