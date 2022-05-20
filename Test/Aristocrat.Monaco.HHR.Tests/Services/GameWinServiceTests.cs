namespace Aristocrat.Monaco.Hhr.Tests.Services
{
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Gaming;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Hhr.Client.WorkFlow;
    using Aristocrat.Monaco.Hhr.Services;
    using Aristocrat.Monaco.Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;
    using Hhr.Client.Messages;
    using System.Threading;
    using Aristocrat.Monaco.Hhr;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Monaco.Test.Common;
    using System.Globalization;
    using HandpayType = Accounting.Contracts.Handpay.HandpayType;
    using HHRHandpayType = Client.Messages.HandpayType;
    using System.Linq;
    using Aristocrat.Monaco.Accounting.Contracts.Handpay;
    using Aristocrat.Monaco.Accounting.Contracts.Wat;

    [TestClass]
    public class GameWinServiceTests
    {
        private readonly Mock<IEventBus> _eventBus = new Mock<IEventBus>(MockBehavior.Default);
        private readonly Mock<ICentralManager> _centralManager = new Mock<ICentralManager>(MockBehavior.Strict);
        private readonly Mock<IPlayerSessionService> _playerSessionService = new Mock<IPlayerSessionService>(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);
        private readonly Mock<IBank> _bank = new Mock<IBank>(MockBehavior.Strict);
        private readonly Mock<IGameDataService> _gameDataService = new Mock<IGameDataService>(MockBehavior.Default);
        private Mock<ILocalizerFactory> _localizerFactory;

        private GameWinService _gameWinService;
        private Action<GameResultEvent> _sendGameResultEvent;
        private List<TransactionRequest> _transactionRequests;
        private GameHistoryLog _gameHistoryLog;

        [TestInitialize]
        public void TestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);

            _localizerFactory = MoqServiceManager.CreateAndAddService<ILocalizerFactory>(MockBehavior.Loose);
            _localizerFactory.Setup(m => m.For(It.IsAny<string>())).Returns<string>(
                name =>
                {
                    var localizer = new Mock<ILocalizer>();
                    localizer.Setup(m => m.CurrentCulture).Returns(new CultureInfo("es-US"));
                    localizer.Setup(m => m.GetString(It.IsAny<string>())).Returns<string>(s => s);
                    return localizer.Object;
                });

            _eventBus.Setup(m => m.Subscribe(It.IsAny<GameWinService>(), It.IsAny<Action<GameResultEvent>>()))
                .Callback<object, Action<GameResultEvent>>(
                    (tar, act) =>
                    {
                        _sendGameResultEvent = act;
                    });

            _propertiesManager.Setup(p => p.GetProperty(HHRPropertyNames.LastGamePlayTime, It.IsAny<uint>()))
                .Returns(12345u);

            _bank.Setup(b => b.QueryBalance(AccountType.Cashable)).Returns(It.IsAny<long>());
            _bank.Setup(b => b.QueryBalance(AccountType.NonCash)).Returns(It.IsAny<long>());
            _bank.Setup(b => b.QueryBalance(AccountType.Promo)).Returns(It.IsAny<long>());

            _centralManager.Setup(
                m => m.Send<TransactionRequest, CloseTranResponse>(
                    It.IsAny<TransactionRequest>(), It.IsAny<CancellationToken>()))
                    .Callback<TransactionRequest, CancellationToken>((msg, tok) => _transactionRequests.Add(msg))
                    .ReturnsAsync(new CloseTranResponse());

            _transactionRequests = new List<TransactionRequest>();
            _gameHistoryLog = new GameHistoryLog(0);

            _gameWinService = new GameWinService(
                _eventBus.Object,
                _centralManager.Object,
                _playerSessionService.Object,
                _propertiesManager.Object,
                _bank.Object,
                _gameDataService.Object);
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_gameWinService);
        }

        [DataTestMethod]
        [DataRow(
            typeof(HandpayTransaction),
            HandpayType.CancelCredit,
            1,
            CommandTransactionType.GameWinToHandpayNoReceipt,
            HHRHandpayType.HandpayTypeNonProgressive,
            DisplayName = "HandpayTransaction")]
        [DataRow(
            typeof(HandpayTransaction),
            HandpayType.GameWin,
            1,
            CommandTransactionType.Unknown,
            HHRHandpayType.HandpayTypeNone,
            DisplayName = "HandpayTransaction GameWin")]
        [DataRow(
            typeof(HandpayTransaction),
            null,
            0,
            CommandTransactionType.Unknown,
            HHRHandpayType.HandpayTypeNone,
            DisplayName = "HandpayTransaction 0 amount")]
        [DataRow(
            typeof(VoucherOutTransaction),
            null,
            1,
            CommandTransactionType.GameWinToCashableOutTicket,
            HHRHandpayType.HandpayTypeNone,
            DisplayName = "VoucherOutTransaction")]
        [DataRow(
            typeof(VoucherOutTransaction),
            null,
            0,
            CommandTransactionType.Unknown,
            HHRHandpayType.HandpayTypeNone,
            DisplayName = "VoucherOutTransaction 0 amount")]
        [DataRow(
            typeof(WatTransaction),
            null,
            1,
            CommandTransactionType.GameWinToAftHost,
            HHRHandpayType.HandpayTypeNone,
            DisplayName = "WatTransaction")]
        [DataRow(
            typeof(WatTransaction),
            null,
            0,
            CommandTransactionType.Unknown,
            HHRHandpayType.HandpayTypeNone,
            DisplayName = "WatTransaction 0 amount")]
        public void GameWinService_WhenHandlingGameResults_ShouldSendHhrServerCorrectTransaction(
            Type transactionType,
            HandpayType? handpayType,
            long amount,
            CommandTransactionType commandTransactionType,
            HHRHandpayType hhrHandpayType)
        {
            _gameHistoryLog.Transactions = new List<TransactionInfo>()
            {
                new TransactionInfo()
                {
                    Amount = amount,
                    HandpayType = handpayType,
                    TransactionType = transactionType
                }
            };

            _sendGameResultEvent(new GameResultEvent(0, 0, "", _gameHistoryLog));

            Assert.AreEqual(commandTransactionType == CommandTransactionType.Unknown ? 0  : 1, _transactionRequests.Count);

            var tansactionRequest = _transactionRequests.FirstOrDefault();
            if (tansactionRequest != null)
            {
                Assert.AreEqual(commandTransactionType, tansactionRequest.TransactionType);
                Assert.AreEqual((uint)hhrHandpayType, tansactionRequest.HandpayType);
            }
        }
    }
}
