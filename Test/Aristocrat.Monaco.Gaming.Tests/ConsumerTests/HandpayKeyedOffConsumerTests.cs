namespace Aristocrat.Monaco.Gaming.Tests.ConsumerTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Monaco.Kernel.Contracts.Events;
    using Aristocrat.Monaco.Localization.Properties;
    using Consumers;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class HandpayKeyedOffConsumerTests
    {
        private Mock<ICurrencyInContainer> _currencyHandler;
        private Mock<IGameHistory> _gameHistory;
        private Mock<IGameHistoryLog> _gameHistoryLog;
        private Mock<ITransactionHistory> _transactionHistory;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ISessionInfoService> _sessionInfoService;
        private Mock<IPersistentStorageManager> _persistentStorage;
        private Mock<IMessageDisplay> _messageDisplay;

        private Mock<IDisposable> _disposable;

        private HandpayKeyedOffConsumer _target;
        private InitializationCompletedConsumer _startupTarget;
        private DateTime _lastPlayedDate;
        private const string DefaultCancelCreditTickerMessage = "Unit Test Handpay Credit - {0}";
        private const string AlternativeCancelCreditTickerMessage = "UNIT TEST CANCEL CREDIT {0}";
        private const string AlternativeCancelCreditTickerMessageTotalSuffix = " (PAID IN TOTAL {1})";

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            MockLocalization.Setup(MockBehavior.Strict);
            MockLocalization.Localizer.Setup(l => l.GetString(ResourceKeys.JackpotToCreditsKeyedOff)).Returns("UNIT TEST Paid {0} to Credit Meter");
            MockLocalization.Localizer.Setup(l => l.GetString(ResourceKeys.JackpotHandpayKeyedOff)).Returns("UNIT TEST Jackpot Handpay Paid - {0}");
            MockLocalization.Localizer.Setup(l => l.GetString(ResourceKeys.CashOutHandpayKeyedOff)).Returns(DefaultCancelCreditTickerMessage);
            MockLocalization.Localizer.Setup(l => l.GetString(ResourceKeys.CashOutHandpayKeyedOffTotalSuffix)).Returns(string.Empty);
            _messageDisplay = MoqServiceManager.CreateAndAddService<IMessageDisplay>(MockBehavior.Default);
            _currencyHandler = new Mock<ICurrencyInContainer>(MockBehavior.Default);
            _gameHistory = MoqServiceManager.CreateAndAddService<IGameHistory>(MockBehavior.Default);
            _transactionHistory = MoqServiceManager.CreateAndAddService<ITransactionHistory>(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _sessionInfoService = new Mock<ISessionInfoService>(MockBehavior.Default);
            _persistentStorage = new Mock<IPersistentStorageManager>(MockBehavior.Default);

            _lastPlayedDate = DateTime.UtcNow;
            _gameHistoryLog = new Mock<IGameHistoryLog>(MockBehavior.Default);
            var gameHistoryLog1 = new GameHistoryLog(0) { PlayState = PlayState.Idle, EndDateTime = _lastPlayedDate };
            var gameHistoryLog2 = new GameHistoryLog(1) { PlayState = PlayState.Initiated, EndDateTime = _lastPlayedDate.AddMinutes(30.0) };
            var gameHistoryLog3 = new GameHistoryLog(2) { PlayState = PlayState.PrimaryGameStarted, EndDateTime = _lastPlayedDate.AddMinutes(60.0) };
            _gameHistory.Setup(g => g.GetGameHistory()).Returns(new List<IGameHistoryLog> { gameHistoryLog1, gameHistoryLog2, gameHistoryLog3 });
            
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.MeterFreeGamesIndependently, false))
                .Returns(false);

            _persistentStorage.Setup(s => s.ScopedTransaction()).Returns(new Mock<IScopedTransaction>().Object);

            _disposable = new Mock<IDisposable>(MockBehavior.Default);
            _disposable.Setup(d => d.Dispose()).Verifiable();

            _target = new HandpayKeyedOffConsumer(
                _currencyHandler.Object,
                _gameHistory.Object,
                _transactionHistory.Object,
                _propertiesManager.Object,
                _sessionInfoService.Object,
                _persistentStorage.Object);

            _startupTarget = new InitializationCompletedConsumer();
            ForceStaticObjectsToBeSetToMockObjects();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullCurrencyInContainer()
        {
            _target = new HandpayKeyedOffConsumer(
                null,
                _gameHistory.Object,
                _transactionHistory.Object,
                _propertiesManager.Object,
                _sessionInfoService.Object,
                _persistentStorage.Object);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullGameHistory()
        {
            _target = new HandpayKeyedOffConsumer(
                _currencyHandler.Object,
                null,
                _transactionHistory.Object,
                _propertiesManager.Object,
                _sessionInfoService.Object,
                _persistentStorage.Object);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullPropertiesManager()
        {
            _target = new HandpayKeyedOffConsumer(
                _currencyHandler.Object,
                _gameHistory.Object,
                _transactionHistory.Object,
                null,
                _sessionInfoService.Object,
                _persistentStorage.Object);
        }

        [DataRow(100, 100, false, DisplayName = "Handpay Transaction same in and out amounts")]
        [DataRow(100, 50, false, DisplayName = "Handpay Transaction different in and out amounts with no log")]
        [DataRow(0, 50, true, DisplayName = "Handpay Transaction different in and out amounts with log")]
        [DataTestMethod]
        public void Consume(
            long inAmount,
            long outAmount,
            bool hasCurrentLog)
        {
            _transactionHistory.Setup(x => x.RecallTransactions()).Returns(new List<ITransaction>().OrderBy(x => x));
            var transaction = new HandpayTransaction { KeyOffCashableAmount = outAmount };

            var transactionEvent = new HandpayKeyedOffEvent(transaction);
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
            else
            {
                _currencyHandler.Verify(x => x.Credit(transaction), Times.Never);
            }
        }

        [TestMethod]
        public void ConsumeSingleCancelCredit()
        {
            var transaction = GenerateCancelCreditTransaction(_lastPlayedDate);
            _transactionHistory.Setup(t => t.RecallTransactions<HandpayTransaction>()).Returns(new List<HandpayTransaction> { transaction });

            var amount = 1000L;
            var transactionEvent = new HandpayKeyedOffEvent(transaction);
            _currencyHandler.Setup(x => x.AmountIn).Returns(amount);
            // Check if message is displayed as expected
            _messageDisplay.Setup(m => m.RemoveMessage(It.Is<DisplayableMessage>(dm => dm.Message.Equals("Unit Test Handpay Credit - $0.01") && dm.MessageHasDynamicGuid))).Verifiable();
            _messageDisplay.Setup(m => m.DisplayMessage(It.Is<DisplayableMessage>(dm => dm.Message.Equals("Unit Test Handpay Credit - $0.01") && dm.MessageHasDynamicGuid))).Verifiable();
            _target.Consume(transactionEvent);

            _messageDisplay.Verify();
        }

        [TestMethod]
        public void ConsumeMultipleCancelCredit()
        {
            var transaction1 = GenerateCancelCreditTransaction(_lastPlayedDate);
            var transaction2 = GenerateCancelCreditTransaction(_lastPlayedDate.AddMinutes(200.0), 2000);
            _transactionHistory.Setup(t => t.RecallTransactions<HandpayTransaction>()).Returns(new List<HandpayTransaction> { transaction1, transaction2 });

            var amount = 3000L;
            var transactionEvent1 = new HandpayKeyedOffEvent(transaction1);
            var transactionEvent2 = new HandpayKeyedOffEvent(transaction2);
            _currencyHandler.Setup(x => x.AmountIn).Returns(amount);
            // Check if message is displayed as expected
            _messageDisplay.Setup(m => m.RemoveMessage(It.Is<DisplayableMessage>(dm => dm.Message.Equals("Unit Test Handpay Credit - $0.01") && dm.MessageHasDynamicGuid))).Verifiable();
            _messageDisplay.Setup(m => m.DisplayMessage(It.Is<DisplayableMessage>(dm => dm.Message.Equals("Unit Test Handpay Credit - $0.01") && dm.MessageHasDynamicGuid))).Verifiable();
            _messageDisplay.Setup(m => m.RemoveMessage(It.Is<DisplayableMessage>(dm => dm.Message.Equals("Unit Test Handpay Credit - $0.02") && dm.MessageHasDynamicGuid))).Verifiable();
            _messageDisplay.Setup(m => m.DisplayMessage(It.Is<DisplayableMessage>(dm => dm.Message.Equals("Unit Test Handpay Credit - $0.02") && dm.MessageHasDynamicGuid))).Verifiable();
            _target.Consume(transactionEvent1);
            _target.Consume(transactionEvent2);

            _messageDisplay.Verify();
        }


        [TestMethod]
        public void ConsumeSingleCancelCreditWithAlternativeTickerMessage()
        {
            var transaction = GenerateCancelCreditTransaction(_lastPlayedDate);
            _transactionHistory.Setup(t => t.RecallTransactions<HandpayTransaction>()).Returns(new List<HandpayTransaction> { transaction });
            MockLocalization.Localizer.Setup(l => l.GetString(ResourceKeys.CashOutHandpayKeyedOff)).Returns(AlternativeCancelCreditTickerMessage);
            MockLocalization.Localizer.Setup(l => l.GetString(ResourceKeys.CashOutHandpayKeyedOffTotalSuffix)).Returns(AlternativeCancelCreditTickerMessageTotalSuffix);

            var amount = 1000L;
            var transactionEvent = new HandpayKeyedOffEvent(transaction);
            _currencyHandler.Setup(x => x.AmountIn).Returns(amount);
            // Check if message is displayed as expected
            _messageDisplay.Setup(m => m.RemoveMessage(It.Is<DisplayableMessage>(dm => dm.Message.Equals("UNIT TEST CANCEL CREDIT $0.01") && dm.Id == AccountingConstants.AlternativeCancelCreditTickerMessageGuid))).Verifiable();
            _messageDisplay.Setup(m => m.DisplayMessage(It.Is<DisplayableMessage>(dm => dm.Message.Equals("UNIT TEST CANCEL CREDIT $0.01") && dm.Id == AccountingConstants.AlternativeCancelCreditTickerMessageGuid))).Verifiable();
            _target.Consume(transactionEvent);

            _messageDisplay.Verify();
        }

        [TestMethod]
        public void ConsumeMultipleCancelCreditWithAlternativeTickerMessage()
        {
            var transaction1 = GenerateCancelCreditTransaction(_lastPlayedDate.AddMinutes(30.0));
            var transaction2 = GenerateCancelCreditTransaction(_lastPlayedDate.AddMinutes(90.0));
            var transaction3 = GenerateCancelCreditTransaction(_lastPlayedDate.AddMinutes(-90.0));
            _transactionHistory.Setup(t => t.RecallTransactions<HandpayTransaction>()).Returns(new List<HandpayTransaction> { transaction1, transaction2, transaction3 });
            MockLocalization.Localizer.Setup(l => l.GetString(ResourceKeys.CashOutHandpayKeyedOff)).Returns(AlternativeCancelCreditTickerMessage);
            MockLocalization.Localizer.Setup(l => l.GetString(ResourceKeys.CashOutHandpayKeyedOffTotalSuffix)).Returns(AlternativeCancelCreditTickerMessageTotalSuffix);

            var amount = 3000L;
            var transactionEvent = new HandpayKeyedOffEvent(transaction3);
            _currencyHandler.Setup(x => x.AmountIn).Returns(amount);
            // Check if message is displayed as expected
            _messageDisplay.Setup(m => m.RemoveMessage(It.Is<DisplayableMessage>(dm => dm.Message.Equals("UNIT TEST CANCEL CREDIT $0.01 (PAID IN TOTAL $0.02)") && dm.Id == AccountingConstants.AlternativeCancelCreditTickerMessageGuid))).Verifiable();
            _messageDisplay.Setup(m => m.DisplayMessage(It.Is<DisplayableMessage>(dm => dm.Message.Equals("UNIT TEST CANCEL CREDIT $0.01 (PAID IN TOTAL $0.02)") && dm.Id == AccountingConstants.AlternativeCancelCreditTickerMessageGuid))).Verifiable();
            _target.Consume(transactionEvent);

            _messageDisplay.Verify();
        }

        [TestMethod]
        public void RecoverMultipleCancelCreditWithAlternativeTickerMessage()
        {
            _transactionHistory.Setup(t => t.RecallTransactions<HandpayTransaction>()).Returns(new List<HandpayTransaction> { GenerateCancelCreditTransaction(_lastPlayedDate.AddMinutes(30.0)), GenerateCancelCreditTransaction(_lastPlayedDate.AddMinutes(90.0)), GenerateCancelCreditTransaction(_lastPlayedDate.AddMinutes(-90.0)) });
            MockLocalization.Localizer.Setup(l => l.GetString(ResourceKeys.CashOutHandpayKeyedOff)).Returns(AlternativeCancelCreditTickerMessage);
            MockLocalization.Localizer.Setup(l => l.GetString(ResourceKeys.CashOutHandpayKeyedOffTotalSuffix)).Returns(AlternativeCancelCreditTickerMessageTotalSuffix);
            var amount = 3000L;
            var initCompletedEvent = new InitializationCompletedEvent();
            _currencyHandler.Setup(x => x.AmountIn).Returns(amount);
            // Check if message is displayed as expected
            _messageDisplay.Setup(m => m.RemoveMessage(It.Is<DisplayableMessage>(dm => dm.Message.Equals("UNIT TEST CANCEL CREDIT $0.01 (PAID IN TOTAL $0.02)")))).Verifiable();
            _messageDisplay.Setup(m => m.DisplayMessage(It.Is<DisplayableMessage>(dm => dm.Message.Equals("UNIT TEST CANCEL CREDIT $0.01 (PAID IN TOTAL $0.02)")))).Verifiable();
            _startupTarget.Consume(initCompletedEvent);

            _messageDisplay.Verify();
        }

        [TestMethod]
        public void RecoverNoCancelCreditsWithAlternativeTickerMessage()
        {
            _transactionHistory.Setup(t => t.RecallTransactions<HandpayTransaction>()).Returns(new List<HandpayTransaction>());
            MockLocalization.Localizer.Setup(l => l.GetString(ResourceKeys.CashOutHandpayKeyedOff)).Returns(AlternativeCancelCreditTickerMessage);
            MockLocalization.Localizer.Setup(l => l.GetString(ResourceKeys.CashOutHandpayKeyedOffTotalSuffix)).Returns(AlternativeCancelCreditTickerMessageTotalSuffix);
            var initCompletedEvent = new InitializationCompletedEvent();
            _startupTarget.Consume(initCompletedEvent);
            _messageDisplay.Verify(m => m.RemoveMessage(It.IsAny<DisplayableMessage>()), Times.Never());
            _messageDisplay.Verify(m => m.RemoveMessage(It.IsAny<DisplayableMessage>()), Times.Never());
        }

        // For some reason, the mock service manager is returning new objects for the static properties
        // of HandpayDisplayHelper. Force them to be the same as our test class objects.
        private void ForceStaticObjectsToBeSetToMockObjects()
        {
            Type type = typeof(HandpayDisplayHelper);
            type.GetField("History", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, _transactionHistory.Object);
            type.GetField("MessageDisplay", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, _messageDisplay.Object);
            type.GetField("GameHistory", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, _gameHistory.Object);
        }

        private HandpayTransaction GenerateCancelCreditTransaction(DateTime dt, long amount = 1000)
        {
            var transaction = new HandpayTransaction(0,
            dt,
            amount,
            0,
            0,
            123,
            HandpayType.CancelCredit,
            true,
            Guid.NewGuid());
            transaction.State = HandpayState.Committed;
            transaction.KeyOffCashableAmount = amount;
            return transaction;
        }
    }
}