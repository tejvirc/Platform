namespace Aristocrat.Monaco.Hhr.Tests.Commands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts.Extensions;
    using Client.Messages;
    using Client.WorkFlow;
    using Events;
    using Exceptions;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;
    using Gaming.Contracts.Progressives;
    using Hhr.Commands;
    using Hhr.Services;
    using Kernel;
    using Mono.Addins;
    using Moq;
    using Storage.Helpers;
    using Test.Common;
    using AutoMapper.Internal;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RequestPlayCommandHandlerTests
    {
        private const int TestTransactionId = 100;
        private const uint RaceSet1AmountWon = 100;
        private const uint RaceSet1ExtraWinnings = 10;
        private const uint RaceSet2AmountWon = 200;

        private readonly Mock<ICentralManager> _centralManager =
            new Mock<ICentralManager>(MockBehavior.Default);
        private readonly Mock<ICentralProvider> _centralProvider =
            new Mock<ICentralProvider>(MockBehavior.Default);
        private readonly Mock<IGamePlayEntityHelper> _gamePlayEntityHelper =
            new Mock<IGamePlayEntityHelper>(MockBehavior.Default);
        private readonly Mock<IPlayerSessionService> _playerSessionService =
            new Mock<IPlayerSessionService>(MockBehavior.Default);
        private readonly Mock<IEventBus> _eventBus =
            new Mock<IEventBus>(MockBehavior.Default);

        private readonly PrizeCalculationException _prizeCalculationException =
            new PrizeCalculationException(string.Empty, string.Empty);

        private readonly GameRecoveryFailedException _gameRecoveryFailedException =
                    new GameRecoveryFailedException();

        private readonly UnexpectedResponseException _unexpectedResponseException =
                    new UnexpectedResponseException(null);

        private readonly Mock<IPrizeDeterminationService> _prizeDeterminationService =
            new Mock<IPrizeDeterminationService>(MockBehavior.Default);

        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);

        private readonly uint[] _progData = { 100, 200 };
        private readonly string _progressiveString = "0102";

        private readonly Mock<IProtocolLinkedProgressiveAdapter> _protocolLinkedProgressiveAdapter =
            new Mock<IProtocolLinkedProgressiveAdapter>(MockBehavior.Default);

        private readonly Mock<IGameProvider> _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);

        private readonly Mock<IRequestTimeoutBehaviorService> _requestTimeoutBehaviorService = new Mock<IRequestTimeoutBehaviorService>(MockBehavior.Strict);

        private OutcomeException _outcomeException;
        private IEnumerable<Outcome> _outcomes;
        private Mock<IServiceManager> _serviceManager;
        private RequestPlayCommandHandler _target;

        private long _transactionId;

        [TestInitialize]
        public void MyTestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            _serviceManager = MoqServiceManager.CreateInstance(MockBehavior.Strict);

            _transactionId = -1;
            _outcomeException = OutcomeException.None;

            _centralManager
                .Setup(
                    c => c.Send<GamePlayRequest, GamePlayResponse>(
                        It.IsAny<GamePlayRequest>(),
                        It.IsAny<CancellationToken>())).Returns(It.IsAny<Task<GamePlayResponse>>());

            _centralProvider.Setup(
                    c =>
                        c.OutcomeResponse(
                            It.IsAny<long>(),
                            It.IsAny<IEnumerable<Outcome>>(),
                            It.IsAny<OutcomeException>(),
                            It.IsAny<IEnumerable<IOutcomeDescription>>()))
                .Callback(
                    (
                        long transaction,
                        IEnumerable<Outcome> outcomes,
                        OutcomeException exception,
                        IEnumerable<IOutcomeDescription> _) =>
                    {
                        _transactionId = transaction;
                        _outcomes = outcomes;
                        _outcomeException = exception;
                    });

            _playerSessionService.Setup(p => p.GetCurrentPlayerId(It.IsAny<int>())).ReturnsAsync(It.IsAny<string>());

            _serviceManager.Setup(s => s.GetService<IProtocolLinkedProgressiveAdapter>())
                .Returns(_protocolLinkedProgressiveAdapter.Object);
            _serviceManager.Setup(s => s.GetService<IGameProvider>())
                .Returns(_gameProvider.Object);
            _serviceManager.Setup(s => s.GetService<IPropertiesManager>())
                .Returns(_propertiesManager.Object);

            _requestTimeoutBehaviorService.Setup(r => r.CanPlay()).Returns(Task.FromResult(true));

            _eventBus.Setup(b => b.Publish(It.IsAny<GamePlayRequestFailedEvent>())).Verifiable();

            SetupDenomSelected();

            CreateTarget();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
            AddinManager.Shutdown();
        }

        [DataRow(true, false, false, false, false)]
        [DataRow(false, true, false, false, false)]
        [DataRow(false, false, true, false, false)]
        [DataRow(false, false, false, true, false)]
        [DataRow(false, false, false, false, true)]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_InvalidParamas_ThrowsException(
            bool nullCentralProvider,
            bool nullPrizeDeterminationService,
            bool nullGamePlayEntityHelper,
            bool nullRequestTimeOutBehaviorService,
            bool nullEventBus)
        {
            _ = CreateRequestPlayCommandHandler(
                nullCentralProvider,
                nullPrizeDeterminationService,
                nullGamePlayEntityHelper,
                nullRequestTimeOutBehaviorService,
                nullEventBus);
        }

        [TestMethod]
        public async Task GamePlay_PrizeCalculationError_ExpectInvalidOutcome()
        {
            SetupPrizeDeterminationService(RaceSet1AmountWon, RaceSet2AmountWon, true);
            SetupPrizeDeterminationServiceToThrowException(_prizeCalculationException);
            await _target.Handle(CreatePlayRequest());

            Assert.AreEqual(0, _outcomes.Count());
            Assert.AreEqual(OutcomeException.Invalid, _outcomeException);
            Assert.AreEqual(TestTransactionId, _transactionId);

            _centralManager.Verify();
            _eventBus.Verify(b => b.Publish(It.IsAny<GamePlayRequestFailedEvent>()), Times.Exactly(1));
        }

        [TestMethod]
        public async Task GamePlay_GameRecoveryFailed_ExpectInvalidOutcome()
        {
            SetupPrizeDeterminationService(RaceSet1AmountWon, RaceSet2AmountWon, true);
            SetupPrizeDeterminationServiceToThrowException(_gameRecoveryFailedException);
            await _target.Handle(CreatePlayRequest());

            Assert.AreEqual(0, _outcomes.Count());
            Assert.AreEqual(OutcomeException.TimedOut, _outcomeException);
            Assert.AreEqual(TestTransactionId, _transactionId);

            _centralManager.Verify();
            _eventBus.Verify(b => b.Publish(It.IsAny<GamePlayRequestFailedEvent>()), Times.Exactly(1));
        }

        [TestMethod]
        public async Task GamePlay_UnexpectedResponse_ExpectInvalidOutcome()
        {
            SetupPrizeDeterminationService(RaceSet1AmountWon, RaceSet2AmountWon, true);
            SetupPrizeDeterminationServiceToThrowException(_unexpectedResponseException);
            await _target.Handle(CreatePlayRequest());

            Assert.AreEqual(0, _outcomes.Count());
            Assert.AreEqual(OutcomeException.TimedOut, _outcomeException);
            Assert.AreEqual(TestTransactionId, _transactionId);

            _centralManager.Verify();
            _eventBus.Verify(b => b.Publish(It.IsAny<GamePlayRequestFailedEvent>()), Times.Exactly(1));
        }

        [TestMethod]
        public async Task GamePlay_AutoHandicapNoWin_SingleOutcomeWithZeroWinValue()
        {
            _outcomeException = OutcomeException.Invalid;

            SetupPrizeDeterminationService();

            await _target.Handle(CreatePlayRequest());

            Assert.AreEqual(1, _outcomes.Count());
            Assert.AreEqual(0, _outcomes.First().Value);
            Assert.AreEqual(OutcomeException.None, _outcomeException);
            Assert.AreEqual(TestTransactionId, _transactionId);

            _centralManager.Verify();
        }

        [TestMethod]
        public async Task GamePlay_AutoHandicapWin_SingleOutcomeWithNonZeroWinValue()
        {
            SetupPrizeDeterminationService(RaceSet1AmountWon);

            await _target.Handle(CreatePlayRequest());

            Assert.AreEqual(1, _outcomes.Count());
            Assert.AreEqual(RaceSet1AmountWon, _outcomes.First().Value.MillicentsToCents());
            Assert.AreEqual(OutcomeException.None, _outcomeException);
            Assert.AreEqual(TestTransactionId, _transactionId);

            _centralManager.Verify();
        }

        [DataRow(1, DisplayName = "Auto handicap with 1 cent wager")]
        [DataRow(2, DisplayName = "Auto handicap with 2 cent wager")]
        [DataTestMethod]
        public async Task GamePlay_AutoHandicapWinWithProgressiveHit_OutcomeWithProgressiveHits(long denomInCents)
        {
            SetupDenomSelected(denomInCents);
            SetupProgressivesOutcome(10, 30, denomInCents);
            SetupPrizeDeterminationService(0, RaceSet2AmountWon, false, _progressiveString, 0, 0, 10, 30);

            await _target.Handle(CreatePlayRequest());

            Assert.AreEqual(4, _outcomes.Count()); // 1 Base Win + 3(1 level1 + 2 level2) Progressive Win
            Assert.AreEqual(OutcomeType.Standard, _outcomes.First().Type);
            Assert.AreEqual(OutcomeType.Progressive, _outcomes.Skip(1).Take(1).First().Type);
            Assert.AreEqual(OutcomeType.Progressive, _outcomes.Skip(2).Take(1).First().Type);
            Assert.AreEqual(OutcomeType.Progressive, _outcomes.Skip(3).Take(1).First().Type);

            //Normal win
            Assert.AreEqual(RaceSet2AmountWon, _outcomes.First().Value.MillicentsToCents());

            Assert.AreEqual(OutcomeException.None, _outcomeException);
            Assert.AreEqual(TestTransactionId, _transactionId);

            _centralManager.Verify();
        }

        [DataRow(
            "0101",
            2,
            DisplayName = "Level1 progressive x1 and Level2 progressive x1 and expect 2 progressive outcomes")]
        [DataRow("0100", 1, DisplayName = "Level1 progressive x1 and expect 1 progressive outcome")]
        [DataRow("0001", 1, DisplayName = "Level2 progressive x1 and expect 1 progressive outcomes")]
        [DataRow(
            "0304",
            7,
            DisplayName = "Level1 progressive x3 and Level2 progressive x4 and expect 7 progressive outcomes")]
        [DataRow("0000", 0, DisplayName = "No progressive hits and expect 0 progressive outcomes")]
        [DataTestMethod]
        public async Task GamePlay_WinProgressive_OutcomesWithCorrectNumberOfProgressiveWinsAndLookupData(
            string progressiveString,
            int progressiveOutcomes)
        {
            SetupProgressivesOutcome(10, 30);
            SetupPrizeDeterminationService(0, RaceSet2AmountWon, false, progressiveString, 0, 0, 10, 30);

            await _target.Handle(CreatePlayRequest());

            Assert.AreEqual(progressiveOutcomes, _outcomes.Count(x => x.Type == OutcomeType.Progressive));

            var progressiveInfo = progressiveString.ParseProgressiveHitInfo();
            progressiveInfo.ForAll(
                x =>
                    Assert.AreEqual(x.count, _outcomes.Count(y => y.LookupData == x.levels.ToString())));
        }

        [TestMethod]
        public async Task GamePlay_NormalWinWithExtraWinnings_ExpectOutcomeWithNormalAndFlexible()
        {
            SetupPrizeDeterminationService(RaceSet1AmountWon, 0, false, string.Empty, RaceSet1ExtraWinnings);

            await _target.Handle(CreatePlayRequest());

            Assert.AreEqual(2, _outcomes.Count());
            Assert.AreEqual(
                RaceSet1AmountWon,
                _outcomes.Where(o => o.Type == OutcomeType.Standard).Sum(o => o.Value).MillicentsToCents());
            Assert.AreEqual(
                RaceSet1ExtraWinnings,
                _outcomes.Where(o => o.Type == OutcomeType.Fractional).Sum(o => o.Value).MillicentsToCents());
            Assert.AreEqual(OutcomeException.None, _outcomeException);
            Assert.AreEqual(TestTransactionId, _transactionId);

            _centralManager.Verify();
        }

        [TestMethod]
        public async Task TransactionRequestPending_GamePlay_ExpectInvalidOutcome()
        {
            _requestTimeoutBehaviorService.Setup(r => r.CanPlay()).Returns(Task.FromResult(false));

            await _target.Handle(CreatePlayRequest());
            Assert.AreEqual(0, _outcomes.Count());
            Assert.AreEqual(OutcomeException.Invalid, _outcomeException);
            Assert.AreEqual(TestTransactionId, _transactionId);

            _centralManager.Verify();
            _eventBus.Verify(b => b.Publish(It.IsAny<GamePlayRequestFailedEvent>()), Times.Exactly(1));
        }

        [TestMethod]
        public async Task TransactionRequestNotPending_GamePlay_ExpectValidOutcome()
        {
            _requestTimeoutBehaviorService.Setup(r => r.CanPlay()).Returns(Task.FromResult(true));

            SetupPrizeDeterminationService(RaceSet1AmountWon, 0, false, string.Empty, RaceSet1ExtraWinnings);

            await _target.Handle(CreatePlayRequest());

            Assert.AreEqual(2, _outcomes.Count());
            Assert.AreEqual(
                RaceSet1AmountWon,
                _outcomes.Where(o => o.Type == OutcomeType.Standard).Sum(o => o.Value).MillicentsToCents());
            Assert.AreEqual(
                RaceSet1ExtraWinnings,
                _outcomes.Where(o => o.Type == OutcomeType.Fractional).Sum(o => o.Value).MillicentsToCents());
            Assert.AreEqual(OutcomeException.None, _outcomeException);
            Assert.AreEqual(TestTransactionId, _transactionId);

            _centralManager.Verify();
        }

        private RequestPlay CreatePlayRequest()
        {
            return new RequestPlay(10, 100, 1, 60, 10, 1, TestTransactionId, 2, false, CancellationToken.None);
        }

        private void SetupPrizeDeterminationService(
            uint raceSet1Amount = 0,
            uint raceSet2AmountWon = 0,
            bool over = false,
            string progressivePrize = "",
            uint raceSet1ExtraWinnings = 0,
            uint raceSet2ExtraWinnings = 0,
            uint raceSet1Wager = 0,
            uint raceSet2Wager = 0)
        {
            var prizeInfo = new PrizeInformation
            {
                RaceSet1AmountWon = raceSet1Amount,
                BOverride = over,
                TransactionId = 123,
                GameMapId = 234,
                RaceSet1ExtraWinnings = raceSet1ExtraWinnings,
                RaceSet2ExtraWinnings = raceSet2ExtraWinnings,
                RaceSet1Wager = raceSet1Wager,
                RaceSet2Wager = raceSet2Wager,
                RaceSet2AmountWonWithoutProgressives = raceSet2AmountWon,
                TotalProgressiveAmountWon = _progData.Aggregate(0u, (sum, value) => sum + value),
                ProgressiveWin = _progData,
                ProgressiveLevelsHit = progressivePrize.ParseProgressiveHitInfo()
            };
            prizeInfo.Outcomes = prizeInfo.ExtractOutcomes();

            _prizeDeterminationService.Setup(
                    x =>
                        x.DeterminePrize(
                            It.IsAny<uint>(),
                            It.IsAny<uint>(),
                            It.IsAny<uint>(),
                            It.IsAny<long>(),
                            It.IsAny<bool>(),
                            It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(prizeInfo));
        }

        private void SetupPrizeDeterminationServiceToThrowException(Exception exception)
        {
            _prizeDeterminationService.Setup(
                    x =>
                        x.DeterminePrize(
                            It.IsAny<uint>(),
                            It.IsAny<uint>(),
                            It.IsAny<uint>(),
                            It.IsAny<long>(),
                            It.IsAny<bool>(),
                            It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);
        }

        private void CreateTarget()
        {
            _target = new RequestPlayCommandHandler(
                _centralProvider.Object,
                _prizeDeterminationService.Object,
                _gamePlayEntityHelper.Object,
                _requestTimeoutBehaviorService.Object,
                _eventBus.Object);
        }

        private RequestPlayCommandHandler CreateRequestPlayCommandHandler(
            bool nullCentralProvider = false,
            bool nullPrizeDeterminationService = false,
            bool nullGamePlayEntityHelper = false,
            bool nullRequestTimeOutBehaviorService = false,
            bool nullEventBus = false)
        {
            return new RequestPlayCommandHandler(
                nullCentralProvider ? null : _centralProvider.Object,
                nullPrizeDeterminationService ? null : _prizeDeterminationService.Object,
                nullGamePlayEntityHelper ? null : _gamePlayEntityHelper.Object,
                nullRequestTimeOutBehaviorService ? null : _requestTimeoutBehaviorService.Object,
                nullEventBus ? null : _eventBus.Object);
        }

        private void SetupProgressivesOutcome(long raceSet1Wager, long raceSet2Wager, long denomInCents = 1)
        {
            _protocolLinkedProgressiveAdapter.Setup(x => x.GetActiveProgressiveLevels())
                .Returns(
                    () => new List<ProgressiveLevel>
                    {
                        new ProgressiveLevel
                        {
                            LevelId = 0,
                            ResetValue = 100,
                            InitialValue = 100,
                            CurrentValue = 110,
                            WagerCredits = (raceSet1Wager + raceSet2Wager)/denomInCents
                        },
                        new ProgressiveLevel
                        {
                            LevelId = 1,
                            ResetValue = 200,
                            InitialValue = 200,
                            CurrentValue = 210,
                            WagerCredits = (raceSet1Wager + raceSet2Wager)/denomInCents
                        }
                    });
        }

        private void SetupDenomSelected(long denomInCents = 1)
        {
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.IsGameRunning, It.IsAny<bool>())).Returns(true);
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.SelectedDenom, It.IsAny<long>())).Returns(denomInCents * 1000L);
            var denomination = new Mock<IDenomination>();
            denomination.SetupGet(x => x.Value).Returns(denomInCents * 1000L);
            var gameDetail = new MockGameInfo
            {
                Denominations = new List<IDenomination> { denomination.Object },
                Id = 1
            };
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.SelectedGameId, It.IsAny<int>())).Returns(1);
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.Games, It.IsAny<object>())).Returns(new List<IGameDetail> { gameDetail });

            _gameProvider.Setup(p => p.GetActiveGame()).Returns((null, denomination.Object));
        }
    }
}