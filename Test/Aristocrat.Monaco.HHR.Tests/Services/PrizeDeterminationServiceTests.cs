namespace Aristocrat.Monaco.Hhr.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper.Internal;
    using Client.Data;
    using Client.Messages;
    using Client.WorkFlow;
    using Exceptions;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Hhr.Services;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Storage.Helpers;
    using Test.Common;
    using System.Reactive.Subjects;


    [TestClass]
    public class PrizeDeterminationServiceTests
    {
        // Values in millicents
        private const uint Level1CurrentValue = 110000;
        private const uint Level1InitialValue = 100000;
        private const uint Level1ResetValue = 100000;
        private const uint Level2CurrentValue = 2 * Level1CurrentValue;
        private const uint Level2InitialValue = 2 * Level1InitialValue;
        private const uint Level2ResetValue = 2 * Level1ResetValue;
        private const int GameId = 1;
        private const int MaxLines = 60;
        private const int NumberOfCredits = 10;
        private const uint RaceSet1Wager = 4;
        private const uint RaceSet2Wager = 6;
        private const string Odds1 = "123456789ABCD";
        private const string Odds2 = "EFGHIJKLMNOPQ";
        private const uint ExtraWinningsRace1 = 1;
        private const uint ExtraWinningsRace2 = 2;

        private const uint PrizePattern1 = 100;
        private const uint PrizePattern2 = 200;
        private const int RaceTicketSetId = 1;
        private readonly List<(string odds, string actual)> _oddsActual = new List<(string odds, string actual)>();
        private Mock<ICentralManager> _centralManager;
        private Mock<IEventBus> _eventBus;
        private Mock<IGameRecoveryService> _gameRecoveryService;
        private Mock<IGameDataService> _gameDataService;

        private Mock<IPlayerSessionService> _playerSessionService;
        private PrizeDeterminationService _prizeDeterminationService;
        private Mock<IProtocolLinkedProgressiveAdapter> _protocolLinkedProgressiveAdapter;
        private IReadOnlyCollection<(uint initialValue, uint currentValue, uint resetValue)> _progressives;
        private Mock<IPropertiesManager> _propertiesManager;
        private readonly Mock<IGamePlayEntityHelper> _gamePlayEntityHelper = new Mock<IGamePlayEntityHelper>(MockBehavior.Default);
        private Mock<IServiceManager> _serviceManager;
        private Mock<IPrizeInformationEntityHelper> _prizeInformationEntityHelper;
        private Mock<ISystemDisableManager> _systemDisableManager;
        private readonly Subject<(Request, Type)> _requests = new Subject<(Request, Type)>();
        private Mock<IGameHistory> _gameHistory;

        [TestInitialize]
        public void Initialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            _serviceManager = MoqServiceManager.CreateInstance(MockBehavior.Strict);

            _gameDataService = new Mock<IGameDataService>(MockBehavior.Default);
            _centralManager = new Mock<ICentralManager>(MockBehavior.Default);
            _playerSessionService = new Mock<IPlayerSessionService>(MockBehavior.Default);
            _protocolLinkedProgressiveAdapter = new Mock<IProtocolLinkedProgressiveAdapter>(MockBehavior.Default);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _eventBus = new Mock<IEventBus>(MockBehavior.Default);
            _gameRecoveryService = new Mock<IGameRecoveryService>(MockBehavior.Default);
            _prizeInformationEntityHelper = new Mock<IPrizeInformationEntityHelper>(MockBehavior.Default);
            _systemDisableManager = new Mock<ISystemDisableManager>(MockBehavior.Default);
            _gameHistory = new Mock<IGameHistory>(MockBehavior.Default);

            // Setups
            _centralManager.Setup(x => x.RequestObservable).Returns(_requests);
            _prizeDeterminationService = new PrizeDeterminationService(_eventBus.Object, _centralManager.Object,
                _playerSessionService.Object, _gameDataService.Object,
                _protocolLinkedProgressiveAdapter.Object, _propertiesManager.Object, _gameRecoveryService.Object, _gamePlayEntityHelper.Object, _prizeInformationEntityHelper.Object, _systemDisableManager.Object, _gameHistory.Object);

            _serviceManager.Setup(s => s.GetService<IProtocolLinkedProgressiveAdapter>())
                .Returns(_protocolLinkedProgressiveAdapter.Object);
            _serviceManager.Setup(s => s.GetService<IPropertiesManager>())
                .Returns(_propertiesManager.Object);

            SetupDenomSelected();
            SetupProgressives();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
            AddinManager.Shutdown();
        }

        [DataRow(true, DisplayName = "Manual Handicap or Quick Pick")]
        [DataRow(false, DisplayName = "Auto Pick")]
        [DataTestMethod]
        public async Task WinRace1ExpectRace1Prize(bool manualHandicap)
        {
            AddWinningRaceSet();
            AddLoosingRaceSet();

            SetupBonanza(RaceSet1Wager, 1, PrizePattern1, manualHandicap, PrizePattern1);
            SetupGameData(GameId, MaxLines, 0xff);
            await SetupManualHandicapProperty(manualHandicap);

            var prizeInformation = await _prizeDeterminationService.DeterminePrize(GameId, 123, 1, NumberOfCredits);

            Verify(prizeInformation, manualHandicap);
            Assert.AreEqual(prizeInformation.RaceSet1AmountWon, PrizePattern1);
            Assert.AreEqual(prizeInformation.RaceSet2AmountWonWithoutProgressives, 0);
            Assert.AreEqual(prizeInformation.RaceSet1Wager, RaceSet1Wager);
            Assert.AreEqual(prizeInformation.RaceSet2Wager, RaceSet2Wager);
            Assert.AreEqual(prizeInformation.Denomination, 1u);
        }

        [DataRow(true, DisplayName = "Manual Handicap or Quick Pick")]
        [DataRow(false, DisplayName = "Auto Pick")]
        [DataTestMethod]
        public async Task WinRace2ExpectRace2Prize(bool manualHandicap)
        {
            AddLoosingRaceSet();
            AddWinningRaceSet();

            SetupBonanza(RaceSet2Wager, 2, PrizePattern2, manualHandicap, 0, PrizePattern2);
            SetupGameData(GameId, MaxLines, 0xff);
            await SetupManualHandicapProperty(manualHandicap);

            var prizeInformation = await _prizeDeterminationService.DeterminePrize(GameId, 234, 2, NumberOfCredits);

            Verify(prizeInformation, manualHandicap);
            Assert.AreEqual(prizeInformation.RaceSet2AmountWonWithoutProgressives, PrizePattern2);
            Assert.AreEqual(prizeInformation.RaceSet1AmountWon, 0);
            Assert.AreEqual(prizeInformation.RaceSet1Wager, RaceSet1Wager);
            Assert.AreEqual(prizeInformation.RaceSet2Wager, RaceSet2Wager);
        }

        [DataRow("0102", true, DisplayName = "Win level 1 progressive and level 2 progressive twice with Manual Handicap")]
        [DataRow("0002", true, DisplayName = "Win level 2 progressive twice with Manual Handicap")]
        [DataRow("1100", true, DisplayName = "Win level 1 progressive 11 times with Manual Handicap")]
        [DataRow("0000", true, DisplayName = "No progressives with Manual Handicap.")]
        [DataRow("1010", true, DisplayName = "Win level 1 progressive 10 times and level 2 progressive 10 times with Manual Handicap")]
        [DataRow("0102", false, DisplayName = "Win level 1 progressive and level 2 progressive twice with AutoHandicap")]
        [DataRow("0002", false, DisplayName = "Win level 2 progressive twice with Auto Handicap")]
        [DataRow("1100", false, DisplayName = "Win level 1 progressive 11 times with Auto Handicap")]
        [DataRow("0000", false, DisplayName = "No progressives with Auto Handicap.")]
        [DataRow("1010", false, DisplayName = "Win level 1 progressive 10 times and level 2 progressive 10 times with Auto Handicap")]
        [DataTestMethod]
        public async Task WinProgressivesExpectProgressivePrize(string progressiveString, bool manualHandicap)
        {
            AddLoosingRaceSet();
            AddWinningRaceSet();
            var prizeValue = GetPrizeValueWithProgressive(PrizePattern2, progressiveString);

            SetupBonanza(RaceSet2Wager, 2, prizeValue, manualHandicap, PrizePattern1, prizeValue, progressiveString);
            SetupGameData(GameId, MaxLines, 0xff, PrizePattern1, prizeValue, progressiveString);
            await SetupManualHandicapProperty(manualHandicap);

            var prizeInformation = await _prizeDeterminationService.DeterminePrize(GameId, 345, 5, NumberOfCredits);

            Verify(prizeInformation, manualHandicap);
            
            Assert.AreEqual(prizeInformation.RaceSet2AmountWonWithoutProgressives, prizeInformation.ProgressiveLevelsHit.Any() ? (prizeValue - GetProgressiveHitTotalResetValues(progressiveString)) : PrizePattern2);
            Assert.AreEqual(prizeInformation.RaceSet1AmountWon, 0);
            if (progressiveString.ParseProgressiveHitInfo().Count > 0)
            {
                Assert.AreNotEqual(prizeInformation.TotalProgressiveAmountWon, 0);
            }
            Assert.AreEqual(prizeInformation.ProgressiveLevelsHit.Count, progressiveString.ParseProgressiveHitInfo().Count);
            Assert.AreEqual(prizeInformation.RaceSet1Wager, RaceSet1Wager);
            Assert.AreEqual(prizeInformation.RaceSet2Wager, RaceSet2Wager);
            Assert.AreEqual(prizeInformation.Denomination, 5u);

            // Interpretation of progressive string
            Assert.AreEqual(prizeInformation.TotalProgressiveAmountWon, GetTotalProgressiveWinForLevels(progressiveString));
        }

        [DataRow(true, DisplayName = "Manual Handicap or Quick Pick")]
        [DataRow(false, DisplayName = "Auto Pick")]
        [DataTestMethod]
        public async Task WinBothRacesExpectBothRacePrize(bool manualHandicap)
        {
            AddWinningRaceSet();
            AddWinningRaceSet();

            SetupBonanza(RaceSet2Wager, 2, PrizePattern2, manualHandicap, PrizePattern1, PrizePattern2);
            SetupGameData(GameId, MaxLines, 0xff);
            await SetupManualHandicapProperty(manualHandicap);

            var prizeInformation = await _prizeDeterminationService.DeterminePrize(GameId, 456, 10, NumberOfCredits);

            Verify(prizeInformation, manualHandicap);
            Assert.AreEqual(prizeInformation.RaceSet2AmountWonWithoutProgressives, PrizePattern2);
            Assert.AreEqual(prizeInformation.RaceSet1AmountWon, PrizePattern1);
            Assert.AreEqual(prizeInformation.RaceSet1Wager, RaceSet1Wager);
            Assert.AreEqual(prizeInformation.RaceSet2Wager, RaceSet2Wager);
        }

        [DataRow("0102", true, DisplayName = "Win level 1 progressive and level 2 progressive twice with Manual Handicap")]
        [DataRow("0002", true, DisplayName = "Win level 2 progressive twice with Manual Handicap")]
        [DataRow("1100", true, DisplayName = "Win level 1 progressive 11 times with Manual Handicap")]
        [DataRow("0000", true, DisplayName = "No progressives with Manual Handicap.")]
        [DataRow("1010", true, DisplayName = "Win level 1 progressive 10 times and level 2 progressive 10 times with Manual Handicap")]
        [DataRow("0102", false, DisplayName = "Win level 1 progressive and level 2 progressive twice with AutoHandicap")]
        [DataRow("0002", false, DisplayName = "Win level 2 progressive twice with Auto Handicap")]
        [DataRow("1100", false, DisplayName = "Win level 1 progressive 11 times with Auto Handicap")]
        [DataRow("0000", false, DisplayName = "No progressives with Auto Handicap.")]
        [DataRow("1010", false, DisplayName = "Win level 1 progressive 10 times and level 2 progressive 10 times with Auto Handicap")]
        [DataTestMethod]
        public async Task WinPrizeProgressivesAndExtraWinningsExpectAllToBePresent(string progressiveString,
            bool manualHandicap)
        {
            AddWinningRaceSet();
            AddWinningRaceSet();
            var prizeValue = GetPrizeValueWithProgressive(PrizePattern2, progressiveString);

            SetupBonanza(RaceSet2Wager, 2, prizeValue, manualHandicap, PrizePattern1, prizeValue, progressiveString,
                ExtraWinningsRace1,
                ExtraWinningsRace2);
            SetupGameData(GameId, MaxLines, 0xff, PrizePattern1, prizeValue, progressiveString);
            await SetupManualHandicapProperty(manualHandicap);

            var prizeInformation = await _prizeDeterminationService.DeterminePrize(GameId, 567, 20, NumberOfCredits);

            Verify(prizeInformation, manualHandicap);
            Assert.AreEqual(prizeInformation.RaceSet2AmountWonWithoutProgressives, prizeInformation.ProgressiveLevelsHit.Any() ? (prizeValue - GetProgressiveHitTotalResetValues(progressiveString)) : PrizePattern2);
            Assert.AreEqual(prizeInformation.RaceSet1AmountWon, PrizePattern1);
            Assert.AreEqual(prizeInformation.TotalProgressiveAmountWon, GetTotalProgressiveWinForLevels(progressiveString));
            Assert.AreEqual(prizeInformation.RaceSet1ExtraWinnings, ExtraWinningsRace1);
            Assert.AreEqual(prizeInformation.RaceSet2ExtraWinnings, ExtraWinningsRace2);
        }

        [DataRow(true, DisplayName = "Manual Handicap or Quick Pick")]
        [DataRow(false, DisplayName = "Auto Pick")]
        [DataTestMethod]
        public async Task LooseBothRacesExpectPrizeToBeZero(bool manualHandicap)
        {
            AddLoosingRaceSet();
            AddLoosingRaceSet();

            SetupBonanza(RaceSet2Wager, 2, 0, manualHandicap);
            SetupGameData(GameId, MaxLines, 0xff, 0, 0);
            await SetupManualHandicapProperty(manualHandicap);

            var prizeInformation = await _prizeDeterminationService.DeterminePrize(GameId, 678, 50, NumberOfCredits);

            Verify(prizeInformation, manualHandicap);
            Assert.AreEqual(prizeInformation.RaceSet2AmountWonWithoutProgressives, 0);
            Assert.AreEqual(prizeInformation.RaceSet1AmountWon, 0);
            Assert.AreEqual(prizeInformation.TotalProgressiveAmountWon, 0);
            Assert.AreEqual(prizeInformation.ProgressiveLevelsHit.Count, 0);
            Assert.AreEqual(prizeInformation.RaceSet1ExtraWinnings, 0u);
            Assert.AreEqual(prizeInformation.RaceSet2ExtraWinnings, 0u);
        }

        [TestMethod]
        [ExpectedException(typeof(PrizeCalculationException))]
        public async Task WinRace1WithPrizeMismatchExpectException()
        {
            AddWinningRaceSet();
            AddLoosingRaceSet();

            SetupBonanza(RaceSet2Wager, 1, 0, false, 0, PrizePattern2);
            SetupGameData(GameId, MaxLines, 0xff);
            await SetupManualHandicapProperty(false);

            await _prizeDeterminationService.DeterminePrize(GameId, 789, 100, NumberOfCredits);
        }

        [DataRow(true, DisplayName = "Manual Handicap or Quick Pick")]
        [DataRow(false, DisplayName = "Auto Pick")]
        [DataTestMethod]
        public async Task PlayRequestTimeoutExpectForRecoveryAttempt(bool manualhandicap)
        {
            AddLoosingRaceSet();
            AddWinningRaceSet();

            SetupBonanza(RaceSet2Wager, 2, PrizePattern2, manualhandicap, 0, PrizePattern2);
            SetupGameData(GameId, MaxLines, 0xff);
            await SetupManualHandicapProperty(manualhandicap);

            _centralManager.Setup(x => x.Send<RaceStartRequest, GamePlayResponse>(It.IsAny<RaceStartRequest>(), It.IsAny<CancellationToken>())).Throws(new UnexpectedResponseException(new Response()));

            _centralManager.Setup(x => x.Send<GamePlayRequest, GamePlayResponse>(It.IsAny<GamePlayRequest>(), It.IsAny<CancellationToken>())).Throws(new UnexpectedResponseException(new Response()));
                                    
            await _prizeDeterminationService.DeterminePrize(GameId, 789, 200, NumberOfCredits);

            if (manualhandicap)
            {
                _centralManager.Verify(x => x.Send<RaceStartRequest, GamePlayResponse>(It.IsAny<RaceStartRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            }

            _centralManager.Verify(x => x.Send<GamePlayRequest, GamePlayResponse>(It.IsAny<GamePlayRequest>(), It.IsAny<CancellationToken>()), Times.Once);

            _gameRecoveryService.Verify(x => x.Recover(It.IsAny<uint>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [DataRow(1, 1U, false, DisplayName = "Game Play Request with SequenceId non zero")]
        [DataRow(1, 0U, false, DisplayName = "Game Play Request with SequenceId non zero")]
        [DataRow(1, 1U, true, DisplayName = "Game Play Request with SequenceId non zero and force check true")]
        [DataRow(2, 1U, false, DisplayName = "Race start Request with SequenceId non zero")]
        [DataRow(3, 1U, false, DisplayName = "Generic Request with SequenceId non zero")]
        [DataTestMethod]
        public void CentralManagerRaisesRequestHandleEvent_ExpectCorrectResponse(int req, uint sequenceId, bool forceCheck)
        {
            Request request;

            switch (req)
            {
                case 1:
                    request = new GamePlayRequest
                    {
                        GameId = 1,
                        PlayerId = "1234",
                        CreditsPlayed = 10,
                        SequenceId = sequenceId,
                        ForceCheck = forceCheck
                    };
                    _gamePlayEntityHelper.SetupGet(x => x.GamePlayRequest).Returns((GamePlayRequest)request).Verifiable();

                    break;
                case 2:
                    request = new RaceStartRequest
                    {
                        GameId = 1,
                        PlayerId = "1234",
                        CreditsPlayed = 10,
                        SequenceId = sequenceId
                    };
                    _gamePlayEntityHelper.SetupGet(x => x.RaceStartRequest).Returns((RaceStartRequest)request);
                    break;

                default:
                    request = new Request(Command.CmdHeartbeat);
                    break;
            }

            _centralManager.Raise(x => x.RequestModifiedHandler += null, this, request);

            if (req == 1 && request.SequenceId > 0 && !((GamePlayRequest) request).ForceCheck)
            {
                _gamePlayEntityHelper.VerifySet(x => x.GamePlayRequest = It.IsAny<GamePlayRequest>(), Times.Once());
            }
            else if (req == 2 && request.SequenceId > 0)
            {
                _gamePlayEntityHelper.VerifySet(x => x.RaceStartRequest = It.IsAny<RaceStartRequest>(), Times.Once());
            }
            else
            {
                _gamePlayEntityHelper.VerifySet(x => x.RaceStartRequest = It.IsAny<RaceStartRequest>(), Times.Never);
                _gamePlayEntityHelper.VerifySet(x => x.GamePlayRequest = It.IsAny<GamePlayRequest>(), Times.Never);
            }
        }

        [DataRow(true, DisplayName = "Manual Handicap or Quick Pick")]
        [DataRow(false, DisplayName = "Auto Pick")]
        [ExpectedException(typeof(UnexpectedResponseException))]
        [DataTestMethod]
        public async Task PlayRequestTimeoutAttemptRecoveryWrongResponseExpectFailure(bool manualhandicap)
        {
            AddLoosingRaceSet();
            AddWinningRaceSet();

            SetupBonanza(RaceSet2Wager, 2, PrizePattern2, manualhandicap, 0, PrizePattern2);
            SetupGameData(GameId, MaxLines, 0xff);
            await SetupManualHandicapProperty(manualhandicap);

            _gameRecoveryService.Setup(x => x.Recover(It.IsAny<uint>(), It.IsAny<CancellationToken>())).Throws(new UnexpectedResponseException(new Response()));

            _centralManager.Setup(x => x.Send<RaceStartRequest, GamePlayResponse>(It.IsAny<RaceStartRequest>(), It.IsAny<CancellationToken>())).Throws(new UnexpectedResponseException(new Response()));

            _centralManager.Setup(x => x.Send<GamePlayRequest, GamePlayResponse>(It.IsAny<GamePlayRequest>(), It.IsAny<CancellationToken>())).Throws(new UnexpectedResponseException(new Response()));

            await _prizeDeterminationService.DeterminePrize(GameId, 789, 500, NumberOfCredits);

            if (manualhandicap)
            {
                _centralManager.Verify(x => x.Send<RaceStartRequest, GamePlayResponse>(It.IsAny<RaceStartRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            }

            _centralManager.Verify(x => x.Send<GamePlayRequest, GamePlayResponse>(It.IsAny<GamePlayRequest>(), It.IsAny<CancellationToken>()), Times.Once);

            _gameRecoveryService.Verify(x => x.Recover(It.IsAny<uint>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [DataRow(true, DisplayName = "Manual Handicap or Quick Pick")]
        [DataRow(false, DisplayName = "Auto Pick")]
        [ExpectedException(typeof(GameRecoveryFailedException))]
        [DataTestMethod]
        public async Task PlayRequestTimeoutAttemptRecoveryFailedExpectFailure(bool manualhandicap)
        {
            AddLoosingRaceSet();
            AddWinningRaceSet();

            SetupBonanza(RaceSet2Wager, 2, PrizePattern2, manualhandicap, 0, PrizePattern2);
            SetupGameData(GameId, MaxLines, 0xff);
            await SetupManualHandicapProperty(manualhandicap);

            _gameRecoveryService.Setup(x => x.Recover(It.IsAny<uint>(), It.IsAny<CancellationToken>())).Throws(new GameRecoveryFailedException());

            _centralManager.Setup(x => x.Send<RaceStartRequest, GamePlayResponse>(It.IsAny<RaceStartRequest>(), It.IsAny<CancellationToken>())).Throws(new UnexpectedResponseException(new Response()));

            _centralManager.Setup(x => x.Send<GamePlayRequest, GamePlayResponse>(It.IsAny<GamePlayRequest>(), It.IsAny<CancellationToken>())).Throws(new UnexpectedResponseException(new Response()));

            await _prizeDeterminationService.DeterminePrize(GameId, 789, 1000, NumberOfCredits);

            if (manualhandicap)
            {
                _centralManager.Verify(x => x.Send<RaceStartRequest, GamePlayResponse>(It.IsAny<RaceStartRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            }

            _centralManager.Verify(x => x.Send<GamePlayRequest, GamePlayResponse>(It.IsAny<GamePlayRequest>(), It.IsAny<CancellationToken>()), Times.Once);

            _gameRecoveryService.Verify(x => x.Recover(It.IsAny<uint>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [DataTestMethod]
        public async Task SetupManualHandicapExpectRacePatternsSet()
        {
            SetupBonanza(RaceSet1Wager, 1, PrizePattern1, true, PrizePattern1);
            SetupGameData(GameId, MaxLines, 0xff);
            await SetupManualHandicapProperty(true);

            _gameDataService.Verify(x => x.GetGameOpen(It.IsAny<uint>()), Times.Once);
            _gameDataService.Verify(x => x.GetRacePatterns(It.IsAny<uint>(), It.IsAny<uint>(), It.IsAny<uint>()), Times.Once);
        }

        private async Task SetupManualHandicapProperty(bool manualHandicap)
        {
            if (manualHandicap)
            {
                _prizeDeterminationService.SetHandicapPicks(GetPicks());
                await _prizeDeterminationService.RequestRaceInfo(0, 0);
            }
        }

        private void Verify(PrizeInformation prizeInformation, bool manualHandicap = false)
        {
            Assert.AreEqual(prizeInformation.RaceSet1Wager, RaceSet1Wager);
            Assert.AreEqual(prizeInformation.RaceSet2Wager, RaceSet2Wager);
            _centralManager.Verify(
                x => x.Send<GamePlayRequest, GamePlayResponse>(It.IsAny<GamePlayRequest>(),
                    It.IsAny<CancellationToken>()), Times.Once);
            _centralManager.Verify(
                x => x.Send<RaceStartRequest, GamePlayResponse>(It.IsAny<RaceStartRequest>(),
                    It.IsAny<CancellationToken>()), Times.Exactly(manualHandicap ? 1 : 0));
        }

        private uint GetPrizeValueWithProgressive(uint normalWin, string progressiveString)
        {
            return normalWin + GetProgressiveHitTotalResetValues(progressiveString);
        }


        // This gives the ProgWon array that we get as part of GamePlay response. Value at index 0 represents the Current value for that Level(which includes reset value + increment)
        private uint[] GetServerProgressiveWinForLevelsArray(string progressiveString)
        {
            var progWon = new uint[progressiveString.Length / 2];
            var hits = progressiveString.ParseProgressiveHitInfo();

            hits.ForAll(progressiveHit =>
                progWon[progressiveHit.levels] = (_progressives.ElementAt(progressiveHit.levels).currentValue / 1000));

            return progWon;
        }

        // This gives total progressive win amount calculated on platform's end : (Lvl1CurrentValue + [Lvl1ResetValue * Lvl1HitCount]) + (Lvl2CurrentValue + [Lvl2ResetValue * Lvl2HitCount])
        private uint GetTotalProgressiveWinForLevels(string progressiveString)
        {
            var progWon = new uint[progressiveString.Length / 2];
            var hits = progressiveString.ParseProgressiveHitInfo();

            hits.ForAll(progressiveHit =>
                progWon[progressiveHit.levels] = (uint)( (_progressives.ElementAt(progressiveHit.levels).currentValue / 1000)
                                                        + ( (_progressives.ElementAt(progressiveHit.levels).resetValue / 1000) * (progressiveHit.count - 1))));
            return progWon.Aggregate(0u, (sum, value) => sum + value);
        }

        // This gives total reset values for the progressive level hits : Lvl1ResetValue * Lvl1HitCount + Lvl2ResetValue * Lvl2HitCount
        private uint GetProgressiveHitTotalResetValues(string progressiveString)
        {
            uint progressivesWon = 0;

            progressiveString.ParseProgressiveHitInfo().ForAll(pro =>
            {
                progressivesWon += (uint)((_progressives.ElementAt(pro.levels).resetValue / 1000) * pro.count);
            });
            return progressivesWon;

        }

        private void SetupProgressives()
        {
            _progressives = new[] { (Level1InitialValue, Level1CurrentValue, Level1ResetValue), (Level2InitialValue, Level2CurrentValue, Level2ResetValue) };

            _protocolLinkedProgressiveAdapter.Setup(x => x.GetActiveProgressiveLevels())
                .Returns(() => new List<ProgressiveLevel>
                {
                    new ProgressiveLevel
                    {
                        LevelId = 0,
                        ResetValue = Level1ResetValue,
                        InitialValue = Level1InitialValue,
                        CurrentValue = Level1CurrentValue,
                        WagerCredits = RaceSet1Wager + RaceSet2Wager,
                    },
                    new ProgressiveLevel
                    {
                        LevelId = 1,
                        ResetValue = Level2ResetValue,
                        InitialValue = Level2InitialValue,
                        CurrentValue = Level2CurrentValue,
                        WagerCredits = RaceSet1Wager + RaceSet2Wager,
                    }
                });
        }

        private void AddWinningRaceSet()
        {
            _oddsActual.Add((Odds1, Odds1));
            _oddsActual.Add((Odds1, Odds1));
            _oddsActual.Add((Odds1, Odds1));
            _oddsActual.Add((Odds1, Odds1));
            _oddsActual.Add((Odds1, Odds1));
        }

        private void AddLoosingRaceSet()
        {
            _oddsActual.Add((Odds1, Odds2));
            _oddsActual.Add((Odds1, Odds2));
            _oddsActual.Add((Odds1, Odds2));
            _oddsActual.Add((Odds1, Odds2));
            _oddsActual.Add((Odds1, Odds2));
        }

        private string PopulatePrizeString(uint wager, int raceGroup, uint prizeValue = 0, string progressiveString = "",
            string extendedInfo = "")
        {
            return $"W={wager}~R={raceGroup}~E={extendedInfo}~P={prizeValue}~PW={progressiveString}";
        }

        private void SetupBonanza(uint wager, int raceGroup, uint prize, bool manualHandicap, uint race1Prize = 0,
            uint race2Prize = 0, string progressivePrize = "", uint pariPrize1 = 0, uint pariPrize2 = 0)
        {
            var prizeString = prize == 0 ? "P=0" : PopulatePrizeString(wager, raceGroup, prize, progressivePrize);
            var gamePlayRequest = new GamePlayResponse
            {
                Prize = prizeString,
                HandicapEnter = 0,
                RaceInfo = PopulateAutoCRaceInfo(_oddsActual, race1Prize, race2Prize, progressivePrize,
                        pariPrize1,
                        pariPrize2)
            };

            _centralManager.Setup(x =>
                    x.Send<GamePlayRequest, GamePlayResponse>(It.IsAny<GamePlayRequest>(),
                        It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(gamePlayRequest));

            _gameRecoveryService.Setup(x => x.Recover(It.IsAny<uint>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(gamePlayRequest));

            if (manualHandicap)
            {
                _centralManager.Setup(x =>
                        x.Send<RaceStartRequest, GamePlayResponse>(It.IsAny<RaceStartRequest>(),
                            It.IsAny<CancellationToken>()))
                    .Returns(() => Task.FromResult(new GamePlayResponse
                    {
                        Prize = prizeString,
                        HandicapEnter = 0,
                        RaceInfo = PopulateAutoCRaceInfo(_oddsActual, race1Prize, race2Prize, progressivePrize,
                            pariPrize1,
                            pariPrize2)
                    }));
            }
        }

        private CRaceInfo PopulateAutoCRaceInfo(IEnumerable<(string odd, string actual)> oddsActual,
            uint patternPrize1 = 0, uint patternPrize2 = 0,
            string progressivePrize = "", uint pariPrize1 = 0, uint pariPrize2 = 2)
        {
            var raceData = new CRaceData[10];
            var i = 0;
            foreach (var (odd, actual) in oddsActual)
            {
                raceData[i++] = GetRaceData(odd, actual);
            }

            return new CRaceInfo
            {
                RaceData = raceData,
                PrizeRaceSet1 = PopulatePrizeString(RaceSet1Wager, 1, patternPrize1),
                PrizeRaceSet2 = PopulatePrizeString(RaceSet2Wager, 2, patternPrize2, progressivePrize),
                ProgWon = GetServerProgressiveWinForLevelsArray(progressivePrize),
                PariPrize1 = pariPrize1,
                PariPrize2 = pariPrize2
            };
        }

        private CRaceData GetRaceData(string odd, string actual)
        {
            return new CRaceData
            {
                Runners = 12,
                HorseOdds = odd,
                HorseActual = actual,
                HorseSelection = "123456789ABC"
            };
        }

        private ReadOnlyCollection<string> GetPicks()
        {
            var picks = new List<string>
            {
                "123456789ABC",
                "123456789ABC",
                "123456789ABC",
                "123456789ABC",
                "123456789ABC",
                "123456789ABC",
                "123456789ABC",
                "123456789ABC",
                "123456789ABC",
                "123456789ABC"
            };
            return new ReadOnlyCollection<string>(picks);
        }

        private void SetupGameData(uint gameId, uint maxLines, byte pattern, uint prizePattern1 = 100,
            uint prizePattern2 = 200, string progressiveString = "")
        {
            var racePatterns = new CRacePatterns
            {
                Credits = NumberOfCredits,
                Line = maxLines,
                RaceTicketSetId = RaceTicketSetId,
                Pattern = new[]
                                    {
                                        new CRacePattern
                                        {
                                            Pattern1 = pattern,
                                            Pattern2 = (byte) (pattern >> 1),
                                            Pattern3 = (byte) (pattern >> 2),
                                            Pattern4 = (byte) (pattern >> 3),
                                            Pattern5 = (byte) (pattern >> 4),
                                            RaceGroup = 1,
                                            Prize = PopulatePrizeString(RaceSet1Wager, 1, prizePattern1),
                                            PrizeValue = PrizePattern1
                                        },
                                        new CRacePattern
                                        {
                                            Pattern5 = pattern,
                                            Pattern4 = (byte) (pattern >> 1),
                                            Pattern3 = (byte) (pattern >> 2),
                                            Pattern2 = (byte) (pattern >> 3),
                                            Pattern1 = (byte) (pattern >> 4),
                                            RaceGroup = 2,
                                            Prize = PopulatePrizeString(RaceSet2Wager, 2, prizePattern2,
                                                progressiveString),
                                            PrizeValue = PrizePattern2
                                        }
                                    }
            };
            var gameInforResponse = new GameInfoResponse
                    {
                        GameId = gameId,
                        MaxLines = maxLines,
                        RaceTicketSets = new CRaceTicketSets
                        {
                            TicketSet = new[]
                            {
                                racePatterns
                            }
                        }
            };

            _gameDataService.Setup(x => x.GetGameOpen(It.IsAny<uint>())).Returns(Task.FromResult(gameInforResponse));
            _gameDataService.Setup(x => x.GetRacePatterns(It.IsAny<uint>(), It.IsAny<uint>(), It.IsAny<uint>())).Returns(Task.FromResult(racePatterns));
            _gameDataService.Setup(x => x.GetGameInfo(It.IsAny<bool>())).Returns(() => {
                return Task.FromResult(new List<GameInfoResponse> { gameInforResponse }.AsEnumerable());
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

        }
    }
}