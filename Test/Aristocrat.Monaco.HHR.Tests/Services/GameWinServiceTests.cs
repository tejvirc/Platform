namespace Aristocrat.Monaco.Hhr.Tests.Services
{
    using System.Threading;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Wat;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Gaming;
    using Gaming.Contracts;
    using Hhr;
    using Hhr.Client.Messages;
    using Hhr.Client.WorkFlow;
    using Hhr.Services;
    using Hhr.Storage.Helpers;
    using Kernel;
    using Test.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using HandpayType = Accounting.Contracts.Handpay.HandpayType;
    using HhrHandpayType = Client.Messages.HandpayType;

    [TestClass]
    public class GameWinServiceTests
    {
        private readonly Mock<IEventBus> _eventBus = new Mock<IEventBus>(MockBehavior.Default);
        private readonly Mock<ICentralManager> _centralManager = new Mock<ICentralManager>(MockBehavior.Strict);
        private readonly Mock<IPlayerSessionService> _playerSessionService = new Mock<IPlayerSessionService>(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);
        private readonly Mock<IBank> _bank = new Mock<IBank>(MockBehavior.Strict);
        private readonly Mock<IPrizeInformationEntityHelper> _prizeInfo = new Mock<IPrizeInformationEntityHelper>(MockBehavior.Default);
        private readonly Mock<ITransactionIdProvider> _transactionIds = new Mock<ITransactionIdProvider>(MockBehavior.Default);
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
                    .Callback<TransactionRequest, CancellationToken>((msg, tok) =>
                    {
                        _transactionRequests.Add(msg);
                    })
                    .ReturnsAsync(new CloseTranResponse());

            _transactionRequests = new List<TransactionRequest>();
            _gameHistoryLog = new GameHistoryLog(0);

            _gameWinService = new GameWinService(
                _eventBus.Object,
                _centralManager.Object,
                _playerSessionService.Object,
                _propertiesManager.Object,
                _bank.Object,
                _prizeInfo.Object,
                _transactionIds.Object);
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_gameWinService);
        }

        [DataTestMethod]
        [DataRow(
            typeof(HandpayTransaction), HandpayType.CancelCredit, 1, 0, 0,
            new CommandTransactionType[]
            {
                CommandTransactionType.GameWinToHandpayNoReceipt
            },
            new HhrHandpayType[]
            {
                HhrHandpayType.HandpayTypeCancelledCredits
            },
            DisplayName = "HandpayTransaction")]
        [DataRow(
            typeof(HandpayTransaction), HandpayType.GameWin, 1, 0, 0,
            new CommandTransactionType[]
            {
            },
            new HhrHandpayType[]
            {
            },
            DisplayName = "HandpayTransaction GameWin")]
        [DataRow(
            typeof(HandpayTransaction), null, 0, 0, 0,
            new CommandTransactionType[]
            {
            },
            new HhrHandpayType[]
            {
            },
            DisplayName = "HandpayTransaction 0 amount")]
        [DataRow(
            typeof(VoucherOutTransaction), null, 5, 5, 0,
            new CommandTransactionType[]
            {
                CommandTransactionType.GameWinToCashableOutTicket
            },
            new HhrHandpayType[]
            {
                HhrHandpayType.HandpayTypeProgressive
            },
            DisplayName = "VoucherOutTransaction Progressive")]
        [DataRow(
            typeof(VoucherOutTransaction), null, 5, 0, 5,
            new CommandTransactionType[]
            {
                CommandTransactionType.GameWinToCashableOutTicket
            },
            new HhrHandpayType[]
            {
                HhrHandpayType.HandpayTypeNonProgressive
            },
            DisplayName = "VoucherOutTransaction Non-Prog")]
        [DataRow(
            typeof(VoucherOutTransaction), null, 5, 2, 3,
            new CommandTransactionType[]
            {
                CommandTransactionType.GameWinToCashableOutTicket,
                CommandTransactionType.GameWinToCashableOutTicket
            },
            new HhrHandpayType[]
            {
                HhrHandpayType.HandpayTypeProgressive,
                HhrHandpayType.HandpayTypeNonProgressive
            },
            DisplayName = "VoucherOutTransaction Mixed")]
        [DataRow(
            typeof(VoucherOutTransaction), null, 0, 0, 0,
            new CommandTransactionType[]
            {
            },
            new HhrHandpayType[]
            {
            },
            DisplayName = "VoucherOutTransaction 0 amount")]
        [DataRow(
            typeof(WatTransaction), null, 1, 0, 0,
            new CommandTransactionType[]
            {
                CommandTransactionType.GameWinToAftHost
            },
            new HhrHandpayType[]
            {
                HhrHandpayType.HandpayTypeNone
            },
            DisplayName = "WatTransaction")]
        [DataRow(
            typeof(WatTransaction), null, 0, 0, 0,
            new CommandTransactionType[]
            {
            },
            new HhrHandpayType[]
            {
            },
            DisplayName = "WatTransaction 0 amount")]
        public void GameWinService_WhenHandlingGameResults_ShouldSendHhrServerCorrectTransaction(
            Type transactionType, HandpayType? handpayType, long amount, long progAmount, long lineAmount,
            CommandTransactionType[] commands, HhrHandpayType[] handpays)
        {
            _gameHistoryLog.Transactions = new List<TransactionInfo>()
            {
                new TransactionInfo()
                {
                    Amount = amount.CentsToMillicents(),
                    HandpayType = handpayType,
                    TransactionType = transactionType
                }
            };

            if (progAmount != 0 || lineAmount != 0)
            {
                _prizeInfo.SetupGet(m => m.PrizeInformation).Returns(new PrizeInformation()
                {
                    TotalProgressiveAmountWon = progAmount,
                    RaceSet1AmountWon = lineAmount
                });
            }

            _sendGameResultEvent(new GameResultEvent(0, 0, "", _gameHistoryLog));

            Assert.AreEqual(commands.Length, _transactionRequests.Count);

            for (int x = 0; x < commands.Length; x++)
            {
                var transactionRequest = _transactionRequests[x];
                Assert.AreEqual(commands[x], transactionRequest.TransactionType);
                Assert.AreEqual(handpays[x], (HhrHandpayType)transactionRequest.HandpayType);
            }
        }
    }
}
