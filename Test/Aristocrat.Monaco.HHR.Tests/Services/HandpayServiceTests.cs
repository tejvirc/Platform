namespace Aristocrat.Monaco.Hhr.Tests.Services
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Application.Contracts.Extensions;
    using Hhr.Services;
    using Client.Messages;
    using Client.WorkFlow;
    using Gaming.Contracts;
    using Gaming.Contracts.Payment;
    using Kernel;
    using Storage.Helpers;
    using Test.Common;
    using Mono.Addins;

    [TestClass]
    public class HandpayServiceTests
    {
        private readonly Mock<ICentralManager> _mockManager = new Mock<ICentralManager>(MockBehavior.Strict);
        private readonly Mock<IPlayerSessionService> _mockPlayerService = new Mock<IPlayerSessionService>(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> _mockProperties = new Mock<IPropertiesManager>(MockBehavior.Strict);
        private readonly Mock<IPlayerBank> _mockPlayerBank = new Mock<IPlayerBank>(MockBehavior.Strict);
        private readonly Mock<IEventBus> _mockEventBus = new Mock<IEventBus>(MockBehavior.Default);
        private readonly Mock<IPaymentDeterminationProvider> _mockLargeWins = new Mock<IPaymentDeterminationProvider>(MockBehavior.Default);
        private readonly Mock<IPrizeInformationEntityHelper> _mockPrizeInformation = new Mock<IPrizeInformationEntityHelper>(MockBehavior.Default);
        private readonly Mock<IGamePlayState> _mockGameState = new Mock<IGamePlayState>(MockBehavior.Strict);
        private readonly Mock<ITransactionIdProvider> _idProvider = new Mock<ITransactionIdProvider>(MockBehavior.Strict);

        private HandpayService _handpayService;

        private List<TransactionRequest> _transactionRequests;
        private List<HandpayCreateRequest> _handpayRequests;

        private Action<HandpayCompletedEvent> _sendHandpayCompleted;
        private Action<TransferOutCompletedEvent> _sendTranferOutComplete;

        [TestInitialize]
        public void TestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);

            // Set up the property manager to handle requests for IRS limits and game ID.
            _mockProperties.Setup(m => m.GetProperty(AccountingConstants.LargeWinLimit, It.IsAny<long>())).Returns(120000000L);
            _mockProperties.Setup(m => m.GetProperty(AccountingConstants.LargeWinRatioThreshold, It.IsAny<long>())).Returns(60000000L);
            _mockProperties.Setup(m => m.GetProperty(AccountingConstants.LargeWinRatio, It.IsAny<long>())).Returns(30000L); // 300.00x
            _mockProperties.Setup(m => m.GetProperty(GamingConstants.SelectedGameId, It.IsAny<uint>())).Returns(99u); _mockProperties.Setup(m => m.GetProperty(GamingConstants.GameWinMaxCreditCashOutStrategy, It.IsAny<MaxCreditCashOutStrategy>())).Returns(MaxCreditCashOutStrategy.Win);
            _mockProperties.Setup(m => m.GetProperty(AccountingConstants.MaxCreditMeter, It.IsAny<long>())).Returns(500000000L);

            // Set up the central manager so we can monitor for handpays being sent over the connection.
            _mockManager.Setup(
                m => m.Send<TransactionRequest, CloseTranResponse>(
                    It.IsAny<TransactionRequest>(), It.IsAny<CancellationToken>()))
                    .Callback<TransactionRequest, CancellationToken>((msg, tok) => HandleTrans(msg))
                    .ReturnsAsync(new CloseTranResponse());
            _transactionRequests = new List<TransactionRequest>();

            _mockManager.Setup(
                m => m.Send<HandpayCreateRequest, CloseTranResponse>(
                    It.IsAny<HandpayCreateRequest>(), It.IsAny<CancellationToken>()))
                    .Callback<HandpayCreateRequest, CancellationToken>((msg, tok) => HandleCreate(msg))
                    .ReturnsAsync(new CloseTranResponse());
            _handpayRequests = new List<HandpayCreateRequest>();

            // Set up the event bus so we can capture the handlers it will use to subscribe to events, and pretend the events
            // happened in order to drive the service.
            _mockEventBus.Setup(m => m.Subscribe(It.IsAny<HandpayService>(), It.IsAny<Action<HandpayCompletedEvent>>()))
                .Callback<object, Action<HandpayCompletedEvent>>(
                    (tar, act) =>
                    {
                        _sendHandpayCompleted = act;
                    });

            _mockEventBus.Setup(m => m.Subscribe(It.IsAny<HandpayService>(), It.IsAny<Action<TransferOutCompletedEvent>>()))
                .Callback<object, Action<TransferOutCompletedEvent>>(
                    (tar, act) =>
                    {
                        _sendTranferOutComplete = act;
                    });

            _mockPrizeInformation.SetupGet(m => m.PrizeInformation).Returns(() => GetLastPrizeInfo());
            _mockPrizeInformation.SetupSet(m => m.PrizeInformation = It.IsAny<PrizeInformation>())
                .Callback<PrizeInformation>(
                    (prizeInfo) => SetLastPrizeInfo(prizeInfo));

            _mockGameState.SetupGet(m => m.InGameRound).Returns(true);

            _idProvider.Setup(i => i.GetNextTransactionId()).Returns(It.IsAny<uint>());

            // Now we can create the service which will be tested in each test.
            _handpayService = new HandpayService(
                _mockManager.Object,
                _mockProperties.Object,
                _mockPlayerService.Object,
                _mockPlayerBank.Object,
                _mockEventBus.Object,
                _mockLargeWins.Object,
                _mockPrizeInformation.Object,
                _mockGameState.Object,
                _idProvider.Object);
        }

        [DataTestMethod]
        [DataRow(10u, 50u, 3u, 30u, 100u, 2u, 0u,
            new uint[] { }, new uint[] { },
            new uint[] { }, new uint[] { },
            DisplayName = "Small win, no handpay")
            ]
        [DataRow(10u, 50u, 3u, 30u, 100u, 2u, 800u,
            new uint[] { }, new uint[] { },
            new uint[] { }, new uint[] { },
            DisplayName = "Mixed win, no handpay")
            ]
        [DataRow(10u, 500000u, 3u, 30u, 0u, 0u, 0u,
            new uint[] { 10, 499993 }, new uint[] { 1, 1 },
            new uint[] { 499993 }, new uint[] { 1 },
            DisplayName = "Handpay set 1")
            ]
        [DataRow(700u, 70000u, 3u, 30u, 0u, 0u, 0u,
            new uint[] { }, new uint[] { },
            new uint[] { }, new uint[] { },
            DisplayName = "Ratio too low")
            ]
        [DataRow(10u, 500000u, 0u, 30u, 59997u, 2u, 0u,
            new uint[] { 10, 499990 }, new uint[] { 1, 1 },
            new uint[] { 499990 }, new uint[] { 1 },
            DisplayName = "Handpay set 1, small win set 2")
            ]
        [DataRow(10u, 500000u, 0u, 30u, 0u, 2u, 400u,
            new uint[] { 10, 499990 }, new uint[] { 1, 1 },
            new uint[] { 499990 }, new uint[] { 1 },
            DisplayName = "Handpay set 1, mixed win set 2")
            ]
        [DataRow(10u, 0u, 0u, 30u, 600000u, 2u, 0u,
            new uint[] { 30, 599972 }, new uint[] { 1, 1 },
            new uint[] { 599972 }, new uint[] { 1 },
            DisplayName = "Handpay set 2")
            ]
        [DataRow(10u, 14u, 17u, 30u, 600000u, 2u, 0u,
            new uint[] { 30, 599972 }, new uint[] { 1, 1 },
            new uint[] { 599972 }, new uint[] { 1 },
            DisplayName = "Handpay set 2, small win set 1")
            ]
        [DataRow(10u, 0u, 0u, 30u, 0u, 0u, 6000u,
            new uint[] { }, new uint[] { },
            new uint[] { }, new uint[] { },
            DisplayName = "Progressive win, no handpay")
            ]
        [DataRow(10u, 0u, 0u, 30u, 0u, 0u, 60030u,
            new uint[] { 30, 60000 }, new uint[] { 0, 0 },
            new uint[] { 60000 }, new uint[] { 0 },
            DisplayName = "Handpay progressive")
            ]
        [DataRow(10u, 0u, 7u, 30u, 280u, 0u, 60030u,
            new uint[] { 30, 250, 60030 }, new uint[] { 1, 1, 0 },
            new uint[] { 60280 }, new uint[] { 0 },
            DisplayName = "Handpay mixed win")
            ]
        [DataRow(10u, 9u, 0u, 30u, 30040u, 0u, 30000u,
            new uint[] { 30, 30010, 30000 }, new uint[] { 1, 1, 0 },
            new uint[] { 60010 }, new uint[] { 0 },
            DisplayName = "Handpay mixed win II")
            ]
        [DataRow(10u, 12u, 7u, 30u, 20u, 3u, 70000u,
            new uint[] { 23, 7, 69993 }, new uint[] { 1, 0, 0 },
            new uint[] { 69993 }, new uint[] { 0 },
            DisplayName = "Handpay mixed win deficit")
            ]
        [DataRow(10u, 50u, 3u, 30u, 100u, 2u, 0u,
            new uint[] { }, new uint[] { },
            new uint[] { }, new uint[] { },
            499999000L,
            DisplayName = "Small win, no handpay, over credit limit")
            ]
        [DataRow(10u, 50u, 3u, 30u, 100u, 2u, 800u,
            new uint[] { }, new uint[] { },
            new uint[] { }, new uint[] { },
            499999000L,
            DisplayName = "Mixed win, no handpay, over credit limit")
            ]
        [DataRow(10u, 500000u, 3u, 30u, 0u, 0u, 0u,
            new uint[] { 10, 499993 }, new uint[] { 1, 1 },
            new uint[] { 499993 }, new uint[] { 1 },
            499999000L,
            DisplayName = "Handpay set 1, over credit limit")
            ]
        public void MessageSending_WithVarious_ShouldBehave(
            uint raceSet1Wager,
            uint raceSet1AmountWon,
            uint raceSet1ExtraWon,
            uint raceSet2Wager,
            uint raceSet2AmountWon,
            uint raceSet2ExtraWon,
            uint progressiveWon,
            uint[] transAmounts,
            uint[] transTypes,
            uint[] handpayAmounts,
            uint[] handpayTypes,
            long playerBankBalance = 987L
            )
        {
            // Set up the bank to handle requests for balance.
            _mockPlayerBank.SetupGet(m => m.Balance).Returns(playerBankBalance);

            // Persist a PrizeInfo object so we can run the function on it.
            var prizeInfo = new PrizeInformation()
            {
                RaceSet1Wager = raceSet1Wager,
                RaceSet1AmountWon = raceSet1AmountWon,
                RaceSet1ExtraWinnings = raceSet1ExtraWon,
                RaceSet1HandpayGuid = Guid.NewGuid(),
                RaceSet2Wager = raceSet2Wager,
                RaceSet2AmountWonWithoutProgressives = raceSet2AmountWon,
                RaceSet2ExtraWinnings = raceSet2ExtraWon,
                TotalProgressiveAmountWon = progressiveWon,
                RaceSet2HandpayGuid = Guid.NewGuid()
            };

            _lastPrizeInfo = prizeInfo;

            // Here we will be firing events to indicate handpay cleared. We also fire voucher out complete events, because
            // those happen as well if the handpay is keyed off as a voucher, but we should only respond once.
            HandpayTransaction trn = new HandpayTransaction();
            trn.TraceId = prizeInfo.RaceSet1HandpayGuid;
            HandpayCompletedEvent evt = new HandpayCompletedEvent(trn);
            _sendHandpayCompleted(evt);

            TransferOutCompletedEvent tfo = new TransferOutCompletedEvent(1, 2, 3, false, prizeInfo.RaceSet2HandpayGuid);
            _sendTranferOutComplete(tfo);

            // Note we're sending these in a different order for race set 2.
            trn = new HandpayTransaction();
            trn.TraceId = prizeInfo.RaceSet2HandpayGuid;
            evt = new HandpayCompletedEvent(trn);
            _sendHandpayCompleted(evt);

            tfo = new TransferOutCompletedEvent(1, 2, 3, false, prizeInfo.RaceSet1HandpayGuid);
            _sendTranferOutComplete(tfo);

            Assert.AreEqual(transAmounts.Length, _transactionRequests.Count);
            Assert.AreEqual(handpayAmounts.Length, _handpayRequests.Count);

            for (int x = 0; x < transAmounts.Length; x++)
            {
                Assert.AreEqual(transAmounts[x], _transactionRequests[x].Credit);
                Assert.AreEqual(transTypes[x], _transactionRequests[x].HandpayType);
            }

            for (int x = 0; x < handpayAmounts.Length; x++)
            {
                Assert.AreEqual(handpayAmounts[x], _handpayRequests[x].Amount);
                Assert.AreEqual(handpayTypes[x], _handpayRequests[x].HandpayType);
            }
        }

        [DataTestMethod]
        [DataRow(10u, 50u, 3u, 30u, 100u, 2u, 0u,
            new long[] { 53, 102 }, new long[] { 0, 0 },
            new uint[] { 53, 102 }, new uint[] { 1, 1 },
            DisplayName = "Small win, no handpay")
            ]
        [DataRow(10u, 50u, 3u, 30u, 100u, 2u, 800u,
            new long[] { 53, 902 }, new long[] { 0, 0 },
            new uint[] { 53, 102, 800 }, new uint[] { 1, 1, 0 },
            DisplayName = "Mixed win, no handpay")
            ]
        [DataRow(10u, 500000u, 3u, 30u, 0u, 0u, 0u,
            new long[] { 10 }, new long[] { 499993 },
            new uint[] { }, new uint[] { },
            DisplayName = "Handpay set 1")
            ]
        [DataRow(700u, 70000u, 3u, 30u, 0u, 0u, 0u,
            new long[] { 70003 }, new long[] { 0 },
            new uint[] { 70003 }, new uint[] { 1 },
            DisplayName = "Ratio too low")
            ]
        [DataRow(10u, 500000u, 0u, 30u, 59997u, 2u, 0u,
            new long[] { 10, 59999 }, new long[] { 499990, 0 },
            new uint[] { 59999 }, new uint[] { 1 },
            DisplayName = "Handpay set 1, small win set 2")
            ]
        [DataRow(10u, 500000u, 0u, 30u, 0u, 2u, 400u,
            new long[] { 10, 402 }, new long[] { 499990, 0 },
            new uint[] { 2, 400 }, new uint[] { 1, 0 },
            DisplayName = "Handpay set 1, mixed win set 2")
            ]
        [DataRow(10u, 0u, 0u, 30u, 600000u, 2u, 0u,
            new long[] { 30 }, new long[] { 599972 },
            new uint[] { }, new uint[] { },
            DisplayName = "Handpay set 2")
            ]
        [DataRow(10u, 14u, 17u, 30u, 600000u, 2u, 0u,
            new long[] { 31, 30 }, new long[] { 0, 599972 },
            new uint[] { 31 }, new uint[] { 1 },
            DisplayName = "Handpay set 2, small win set 1")
            ]
        [DataRow(10u, 0u, 0u, 30u, 0u, 0u, 6000u,
            new long[] { 6000 }, new long[] { 0 },
            new uint[] { 6000 }, new uint[] { 0 },
            DisplayName = "Progressive win, no handpay")
            ]
        [DataRow(10u, 0u, 0u, 30u, 0u, 0u, 60030u,
            new long[] { 30 }, new long[] { 60000 },
            new uint[] { }, new uint[] { },
            DisplayName = "Handpay progressive")
            ]
        [DataRow(10u, 0u, 7u, 30u, 280u, 0u, 60030u,
            new long[] { 7, 30 }, new long[] { 0, 60280 },
            new uint[] { 7 }, new uint[] { 1 },
            DisplayName = "Handpay mixed win")
            ]
        [DataRow(10u, 9u, 0u, 30u, 30040u, 0u, 30000u,
            new long[] { 9, 30 }, new long[] { 0, 60010 },
            new uint[] { 9 }, new uint[] { 1 },
            DisplayName = "Handpay mixed win II")
            ]
        [DataRow(10u, 12u, 7u, 30u, 20u, 3u, 70000u,
            new long[] { 19, 30 }, new long[] { 0, 69993 },
            new uint[] { 19 }, new uint[] { 1 },
            DisplayName = "Handpay mixed win deficit")
            ]
        [DataRow(10u, 50u, 3u, 30u, 100u, 2u, 0u,
            new long[] { 53, 102 }, new long[] { 0, 0 },
            new uint[] { 53, 102 }, new uint[] { 1, 1 },
            499999000L,
            DisplayName = "Small win, no handpay, over credit limit")
            ]
        [DataRow(10u, 50u, 3u, 30u, 100u, 2u, 800u,
            new long[] { 53, 902 }, new long[] { 0, 0 },
            new uint[] { 53, 102, 800 }, new uint[] { 1, 1, 0 },
            499999000L,
            DisplayName = "Mixed win, no handpay, over credit limit")
            ]
        [DataRow(10u, 500000u, 3u, 30u, 0u, 0u, 0u,
            new long[] { 10 }, new long[] { 499993 },
            new uint[] { }, new uint[] { },
            499999000L,
            DisplayName = "Handpay set 1, over credit limit")
            ]
        public void LargeWinDetermination_WithVarious_ShouldBehave(
            uint raceSet1Wager,
            uint raceSet1AmountWon,
            uint raceSet1ExtraWon,
            uint raceSet2Wager,
            uint raceSet2AmountWon,
            uint raceSet2ExtraWon,
            uint progressiveWon,
            long[] wonAmounts,
            long[] largeAmounts,
            uint[] transAmounts,
            uint[] transTypes,
            long playerBankBalance = 987L
            )
        {
            // Set up the bank to handle requests for balance.
            _mockPlayerBank.SetupGet(m => m.Balance).Returns(playerBankBalance);

            // Persist a PrizeInfo object so we can run the function on it.
            var prizeInfo = new PrizeInformation()
            {
                RaceSet1Wager = raceSet1Wager,
                RaceSet1AmountWon = raceSet1AmountWon,
                RaceSet1ExtraWinnings = raceSet1ExtraWon,
                RaceSet2Wager = raceSet2Wager,
                RaceSet2AmountWonWithoutProgressives = raceSet2AmountWon,
                RaceSet2ExtraWinnings = raceSet2ExtraWon,
                TotalProgressiveAmountWon = progressiveWon
            };

            _mockPrizeInformation.SetupGet(m => m.PrizeInformation).Returns(prizeInfo);

            // Here we call the function directly, to simulate what the PayGameResultsCommandHandler would do.
            List<PaymentDeterminationResult> results = _handpayService.GetPaymentResults(123);
            Assert.AreEqual(wonAmounts.Length, results.Count);
            Assert.AreEqual(largeAmounts.Length, results.Count);

            for (int x = 0; x < results.Count; x++)
            {
                Assert.AreEqual(wonAmounts[x].CentsToMillicents(), results[x].MillicentsToPayToCreditMeter);
                Assert.AreEqual(largeAmounts[x].CentsToMillicents(), results[x].MillicentsToPayUsingLargeWinStrategy);
            }

            // No handpay messages should be sent for this phase of checking, that comes later when handpays are cleared.
            // However, we may see transaction messages for amounts that are not handpaid.
            Assert.AreEqual(transAmounts.Length, _transactionRequests.Count);
            Assert.AreEqual(0, _handpayRequests.Count);

            for (int x = 0; x < transAmounts.Length; x++)
            {
                Assert.AreEqual(transAmounts[x], _transactionRequests[x].Credit);
                Assert.AreEqual(transTypes[x], _transactionRequests[x].HandpayType);
            }
        }

        private void HandleTrans(TransactionRequest req) => _transactionRequests.Add(req);

        private void HandleCreate(HandpayCreateRequest req) => _handpayRequests.Add(req);

        private PrizeInformation _lastPrizeInfo;

        private void SetLastPrizeInfo(PrizeInformation prizeInfo)
        {
            _lastPrizeInfo = prizeInfo;
        }

        private PrizeInformation GetLastPrizeInfo()
        {
            return _lastPrizeInfo;
        }
    }
}
