namespace Aristocrat.Monaco.Hhr.Tests.Progressive
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts;
    using Client.Messages;
    using Events;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Hhr.Services;
    using Hhr.Services.Progressive;
    using Kernel;
    using Test.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ProgressiveAssociationTests
    {
        private const int Million = 1000000;

        private readonly Mock<IEventBus> _eventBus =
            new Mock<IEventBus>(MockBehavior.Strict);
        private readonly Mock<IGameDataService> _gameDataService =
            new Mock<IGameDataService>(MockBehavior.Strict);
        private readonly Mock<IGameProvider> _gameProvider =
            new Mock<IGameProvider>(MockBehavior.Strict);
        private readonly Mock<IProtocolLinkedProgressiveAdapter> _progAdapter =
            new Mock<IProtocolLinkedProgressiveAdapter>(MockBehavior.Strict);
        private readonly Mock<IProgressiveUpdateService> _progressiveUpdateService =
            new Mock<IProgressiveUpdateService>(MockBehavior.Strict);
        private readonly Mock<IPropertiesManager> _propertiesManager =
            new Mock<IPropertiesManager>(MockBehavior.Strict);

        private readonly List<GameInfoResponse> _gameInfo = new List<GameInfoResponse>();
        private readonly List<ProgressiveInfoResponse> _progInfo = new List<ProgressiveInfoResponse>();
        private readonly List<ProgressiveLevel> _progressiveLevel = new List<ProgressiveLevel>();


        //private readonly ManualResetEvent _waiter = new ManualResetEvent(false);

        private ProgressiveAssociation _target;

        [TestInitialize]
        public void Initialize()
        {
            _gameDataService.Setup(g => g.GetGameInfo(It.IsAny<bool>())).Returns(Task.FromResult(_gameInfo.AsEnumerable()));

            _progAdapter.Setup(p => p.ViewProgressiveLevels()).Returns(_progressiveLevel);

            _gameProvider.Setup(g => g.GetGame(1)).Returns(GetGame(1));
            _gameProvider.Setup(g => g.GetGame(2)).Returns(GetGame(2));

            _gameProvider.Setup(g => g.GetEnabledGames()).Returns(GetGames());

            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.MaxBetLimit, It.IsAny<long>())).Returns(1000000L);
        }

        [DataRow(true, false, false, false, false, DisplayName = "Null GameProvider")]
        [DataRow(false, true, false, false, false, DisplayName = "Null ProtocolLinkedProgressiveAdapter")]
        [DataRow(false, false, true, false, false, DisplayName = "Null EventBus")]
        [DataRow(false, false, false, true, false, DisplayName = "Null ProgressiveUpdateService")]
        [DataRow(false, false, false, false, true, DisplayName = "Null PropertiesManager")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_InvalidParams_ThrowsException(
            bool nullGameProvider,
            bool nullLpProvider,
            bool nullEventBus,
            bool nullProgressiveUpdateService,
            bool nullPropertiesManager)
        {
            _ = CreateProgressiveAssociation(nullGameProvider, nullLpProvider, nullEventBus,
                nullProgressiveUpdateService, nullPropertiesManager);
        }

        [TestMethod]
        public void ProgressiveAssociation_GameCreatedLevelWagerMismatchForServerLevel_AssociationFails()
        {
            // Game progressive level info
            _progressiveLevel.Add(CreateProgressiveLevel(40, 1, "Pack1", 1, 1, 0, 10, "Grand", 2000,
                ProgressiveLevelType.LP, new List<long> {1}));

            // Server progressive level info with wrong bet
            _progInfo.Add(CreateProgressiveResponse(101, 1, 10, 999, 2000));

            SetupGameOpenResponses();
            CreateProgressiveAssociation();

            foreach (var result in _progInfo.Select(progressiveInfo =>
                _target.AssociateServerLevelsToGame(progressiveInfo, _gameInfo[0], new List<ProgressiveLevelAssignment>())
                    .Result))
            {
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void ProgressiveAssociation_GameCreatedLevelIdMismatchForServerLevel_AssociationFails()
        {
            // Game progressive level info
            _progressiveLevel.Add(CreateProgressiveLevel(40, 1, "Pack1", 1, 1, 1, 10, "Grand", 2000,
                ProgressiveLevelType.LP, new List<long> {1}));

            // Server progressive level info with wrong ID
            _progInfo.Add(CreateProgressiveResponse(999, 1, 10, 40, 2000));

            SetupGameOpenResponses();
            CreateProgressiveAssociation();

            foreach (var result in _progInfo.Select(progressiveInfo =>
                _target.AssociateServerLevelsToGame(progressiveInfo, _gameInfo[0], new List<ProgressiveLevelAssignment>())
                    .Result))
            {
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void ProgressiveAssociation_GameCreatedLevelIsNotAllOrMax_AssociationFails()
        {
            // Game progressive level info
            _progressiveLevel.Add(CreateProgressiveLevel(40, 1, "Pack1", 1, 1, 0, 10, "Grand", 2000,
                ProgressiveLevelType.LP, new List<long> {1}, false, LevelCreationType.Default));

            // Server progressive level info
            _progInfo.Add(CreateProgressiveResponse(101, 1, 10, 40, 2000));

            SetupGameOpenResponses();
            CreateProgressiveAssociation();

            foreach (var result in _progInfo.Select(progressiveInfo =>
                _target.AssociateServerLevelsToGame(progressiveInfo, _gameInfo[0], new List<ProgressiveLevelAssignment>())
                    .Result))
            {
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void ProgressiveAssociation_NoOfLevelsDoNotMatch_AssociationFails()
        {
            // Game Levels - Total 1
            _progressiveLevel.Add(CreateProgressiveLevel(40, 1, "Pack1", 1, 1, 0, 10 * Million, "Grand",
                2000 * GamingConstants.Millicents,
                ProgressiveLevelType.LP, new List<long> {1 * GamingConstants.Millicents}));

            // Server Prog Info - Total 2
            _progInfo.Add(CreateProgressiveResponse(101, 1, 10, 40, 2000));
            _progInfo.Add(CreateProgressiveResponse(102, 2, 11, 40, 1000));

            SetupGameOpenResponses();
            CreateProgressiveAssociation();

            foreach (var result in _progInfo.Select(progressiveInfo =>
                _target.AssociateServerLevelsToGame(progressiveInfo, _gameInfo[0],
                    new List<ProgressiveLevelAssignment>()).Result))
            {
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void ProgressiveAssociation_DenomNotMatched_AssociationFails()
        {
            SetupProgressiveMatchFailureDueToDenomMismatch();

            CreateProgressiveAssociation();

            _progressiveUpdateService.Setup(x => x.IsProgressiveLevelUpdateLocked(It.IsAny<LinkedProgressiveLevel>()))
                .Returns(false);

            var resultArray = new List<bool>();
            var index = 1;

            foreach (var progInfo in _progInfo)
            {
                resultArray.Add(index > 2
                    ? _target.AssociateServerLevelsToGame(progInfo, _gameInfo[1],
                        new List<ProgressiveLevelAssignment>()).Result
                    : _target.AssociateServerLevelsToGame(progInfo, _gameInfo[0],
                        new List<ProgressiveLevelAssignment>()).Result);

                ++index;
            }

            Assert.IsTrue(resultArray[0]);
            Assert.IsTrue(resultArray[1]);
            Assert.IsFalse(resultArray[2]);
        }

        [TestMethod]
        public void ProgressiveAssociation_ReferenceIdNotMatched_AssociationFails()
        {
            SetupProgressiveMatchFailureDueToReferenceIdMismatch();

            CreateProgressiveAssociation();

            _progressiveUpdateService.Setup(x => x.IsProgressiveLevelUpdateLocked(It.IsAny<LinkedProgressiveLevel>()))
                .Returns(false);

            var resultArray = new List<bool>();
            var index = 1;

            foreach (var progInfo in _progInfo)
            {
                resultArray.Add(index > 2
                    ? _target.AssociateServerLevelsToGame(progInfo, _gameInfo[1],
                        new List<ProgressiveLevelAssignment>()).Result
                    : _target.AssociateServerLevelsToGame(progInfo, _gameInfo[0],
                        new List<ProgressiveLevelAssignment>()).Result);

                ++index;
            }

            Assert.IsTrue(resultArray[0]);
            Assert.IsTrue(resultArray[1]);
            Assert.IsFalse(resultArray[2]);
        }

        [TestMethod]
        public void ProgressiveAssociation_ProgressiveLevelAlreadyAssignedNotFound_AssociationFails()
        {
            SetupProgressiveWithAssignedProgressiveId(null);

            CreateProgressiveAssociation();

            _progressiveUpdateService.Setup(x => x.IsProgressiveLevelUpdateLocked(It.IsAny<LinkedProgressiveLevel>()))
                .Returns(false);

            var resultArray = _progInfo
                .Select(progInfo =>
                    _target.AssociateServerLevelsToGame(progInfo, _gameInfo[0], new List<ProgressiveLevelAssignment>())
                        .Result).ToList();

            Assert.IsTrue(resultArray[0]);
            Assert.IsTrue(resultArray[1]);
            Assert.IsFalse(resultArray[2]);
        }

        [TestMethod]
        public void ProgressiveAssociation_ProgressiveLevelAlreadyAssignedFoundWhichDoNotMatch_AssociationFails()
        {
            var linkedLevel = new LinkedProgressiveLevel {ProgressiveGroupId = 104, LevelId = 1};

            SetupProgressiveWithAssignedProgressiveId(linkedLevel, true);

            CreateProgressiveAssociation();

            _progressiveUpdateService.Setup(x => x.IsProgressiveLevelUpdateLocked(It.IsAny<LinkedProgressiveLevel>()))
                .Returns(false);

            var resultArray = _progInfo
                .Select(progInfo =>
                    _target.AssociateServerLevelsToGame(progInfo, _gameInfo[0], new List<ProgressiveLevelAssignment>())
                        .Result).ToList();

            Assert.IsTrue(resultArray[0]);
            Assert.IsTrue(resultArray[1]);
            Assert.IsFalse(resultArray[2]);
        }

        [TestMethod]
        public void ProgressiveAssociation_ProgressiveLevelAlreadyAssignedFoundInRecoveryWhichMatch_AssociationSucceeds()
        {
            var linkedLevel = new LinkedProgressiveLevel {ProgressiveGroupId = 103, LevelId = 1};

            SetupProgressiveWithAssignedProgressiveId(linkedLevel, true);

            _progressiveUpdateService.Setup(x => x.IsProgressiveLevelUpdateLocked(It.IsAny<LinkedProgressiveLevel>()))
                .Returns(true);

            CreateProgressiveAssociation();

            foreach (var progInfo in _progInfo)
            {
                Assert.IsTrue(_target
                    .AssociateServerLevelsToGame(progInfo, _gameInfo[0], new List<ProgressiveLevelAssignment>())
                    .Result);
            }

            _progAdapter.Verify(x => x.UpdateLinkedProgressiveLevels(
                It.IsAny<IReadOnlyCollection<IViewableLinkedProgressiveLevel>>(),
                ProtocolNames.HHR), Times.Never);
        }

        [TestMethod]
        public void ProgressiveAssociation_ProgressiveLevelAlreadyAssignedFoundWhichMatch_AssociationSucceeds()
        {
            var linkedLevel = new LinkedProgressiveLevel {ProgressiveGroupId = 103, LevelId = 1};

            SetupProgressiveWithAssignedProgressiveId(linkedLevel, true);

            _progressiveUpdateService.Setup(x => x.IsProgressiveLevelUpdateLocked(It.IsAny<LinkedProgressiveLevel>()))
                .Returns(false);

            CreateProgressiveAssociation();

            foreach (var progInfo in _progInfo)
            {
                Assert.IsTrue(_target
                    .AssociateServerLevelsToGame(progInfo, _gameInfo[0], new List<ProgressiveLevelAssignment>())
                    .Result);
            }

            _progAdapter.Verify(x => x.UpdateLinkedProgressiveLevels(
                It.IsAny<IReadOnlyCollection<IViewableLinkedProgressiveLevel>>(),
                ProtocolNames.HHR), Times.Exactly(3));
        }

        [TestMethod]
        public void ProgressiveAssociation_GameNotFound_AssociationFails()
        {
            SetupProgressiveAssociationFailureDueToGameNotFound();

            CreateProgressiveAssociation();

            foreach (var result in _progInfo.Select(progressiveInfo =>
                _target.AssociateServerLevelsToGame(progressiveInfo, _gameInfo[0],
                    new List<ProgressiveLevelAssignment>()).Result))
            {
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void ProgressiveAssociation_GameLevelsMatchWithServerLevels_AssociationSucceeds()
        {
            // Setup progressive game levels and server levels
            SetupProgressiveLevels();

            // GameOpen Responses setup
            _gameInfo.Add(CreateGameInfoResponse(new uint[] {101, 102}, 1, 100));

            _progressiveUpdateService.Setup(x => x.IsProgressiveLevelUpdateLocked(It.IsAny<LinkedProgressiveLevel>()))
                .Returns(false);

            _progAdapter.Setup(l => l.UpdateLinkedProgressiveLevels(
                It.IsAny<IReadOnlyCollection<IViewableLinkedProgressiveLevel>>(),
                ProtocolNames.HHR)).Verifiable();

            CreateProgressiveAssociation();

            foreach (var result in _progInfo.Select(progressiveInfo =>
                _target.AssociateServerLevelsToGame(progressiveInfo, _gameInfo[0],
                    new List<ProgressiveLevelAssignment>()).Result))
            {
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void ProgressiveAssociation_LevelDenomExceedsMaxBet_AssociationIgnored()
        {
            // Setup progressive game levels and server levels
            SetupProgressiveLevels();

            // GameOpen Responses setup
            _gameInfo.Add(CreateGameInfoResponse(new uint[] { 101, 102 }, 1, 100, 999999));

            _progressiveUpdateService.Setup(x => x.IsProgressiveLevelUpdateLocked(It.IsAny<LinkedProgressiveLevel>()))
                .Returns(false);

            _progAdapter.Setup(l => l.UpdateLinkedProgressiveLevels(
                It.IsAny<IReadOnlyCollection<IViewableLinkedProgressiveLevel>>(),
                ProtocolNames.HHR)).Verifiable();

            CreateProgressiveAssociation();

            foreach (var result in _progInfo.Select(progressiveInfo =>
                _target.AssociateServerLevelsToGame(progressiveInfo, _gameInfo[0],
                    new List<ProgressiveLevelAssignment>()).Result))
            {
                Assert.IsTrue(result);
            }

            _progAdapter.Verify(x => x.UpdateLinkedProgressiveLevels(
                It.IsAny<IReadOnlyCollection<IViewableLinkedProgressiveLevel>>(),
                ProtocolNames.HHR), Times.Never);
        }

        [TestMethod]
        public void UnsupportedGameConfig_DuplicateCdsInfos_ExpectSystemDisable()
        {
            _gameProvider.Setup(g => g.GetEnabledGames()).Returns(GetGamesWithDuplicateCdsInfos());
            _eventBus.Setup(e => e.Publish(It.IsAny<GameConfigurationNotSupportedEvent>())).Verifiable();

            CreateProgressiveAssociation();

            _eventBus.Verify();
        }

        [TestMethod]
        public void UnsupportedGameConfig_DistinctCdsInfos_ExpectSystemNotDisabled()
        {
            _gameProvider.Setup(g => g.GetEnabledGames()).Returns(GetGames());
            _eventBus.Setup(e => e.Publish(It.IsAny<GameConfigurationNotSupportedEvent>()))
                .Callback<GameConfigurationNotSupportedEvent>(evt =>
                    throw new Exception("Shouldn't raise this event as game config is fine."));

            CreateProgressiveAssociation();
            _eventBus.Verify(x => x.Publish(It.IsAny<GameConfigurationNotSupportedEvent>()), Times.Never());
        }

        private ProgressiveAssociation CreateProgressiveAssociation(
            bool nullGameProvider = false,
            bool nullLpProvider = false,
            bool nullEventBus = false,
            bool nullProgressiveUpdateService = false,
            bool nullPropertiesManager = false)
        {
            _target = new ProgressiveAssociation(
                nullGameProvider ? null : _gameProvider.Object,
                nullLpProvider ? null : _progAdapter.Object,
                nullEventBus ? null : _eventBus.Object,
                nullProgressiveUpdateService ? null : _progressiveUpdateService.Object,
                nullPropertiesManager ? null : _propertiesManager.Object);

            return _target;
        }

        private void SetupProgressiveAssociationFailureDueToGameNotFound()
        {
            // Setup progressive game levels and server levels
            SetupProgressiveLevels();
            // Setup game open responses
            SetupGameOpenResponses();

            // Setup Game not found
            _gameProvider.Setup(g => g.GetGame(It.IsAny<int>())).Returns((IGameDetail) null);
        }

        private void SetupProgressiveMatchFailureDueToDenomMismatch()
        {
            SetupProgressiveLevels();
            SetupGameOpenResponses();

            // Game Level : Denom-1
            _progressiveLevel.Add(CreateProgressiveLevel(80, 1, "Pack2", 2, 2, 0, 12 * Million, "Grand",
                4000 * GamingConstants.Millicents,
                ProgressiveLevelType.LP, new List<long> {1 * GamingConstants.Millicents}));
            _progInfo.Add(CreateProgressiveResponse(103, 1, 12, 80, 4000));

            // Server GameOpen: Denom-2
            _gameInfo.Add(CreateGameInfoResponse(new uint[] {103}, 2, 101));

            _progAdapter.Setup(l => l.UpdateLinkedProgressiveLevels(
                It.IsAny<IReadOnlyCollection<IViewableLinkedProgressiveLevel>>(),
                ProtocolNames.HHR)).Verifiable();
        }

        private void SetupProgressiveMatchFailureDueToReferenceIdMismatch()
        {
            SetupProgressiveLevels();
            SetupGameOpenResponses();

            // Game Level : Denom-1, referenceId-104
            _progressiveLevel.Add(CreateProgressiveLevel(80, 1, "Pack2", 2, 2, 0, 12 * Million, "Grand",
                4000 * GamingConstants.Millicents,
                ProgressiveLevelType.LP, new List<long> {1 * GamingConstants.Millicents}));
            _progInfo.Add(CreateProgressiveResponse(103, 1, 12, 80, 4000));

            // Server GameOpen: Denom-1, referenceId-101
            _gameInfo.Add(CreateGameInfoResponse(new uint[] {103}, 1, 103));

            _progAdapter.Setup(l => l.UpdateLinkedProgressiveLevels(
                It.IsAny<IReadOnlyCollection<IViewableLinkedProgressiveLevel>>(),
                ProtocolNames.HHR)).Verifiable();
        }

        private void SetupProgressiveWithAssignedProgressiveId(IViewableLinkedProgressiveLevel level, bool ret = false)
        {
            SetupProgressiveLevels();

            _progressiveLevel.Add(CreateProgressiveLevel(80, 1, "Pack2", 2, 1, 0,
                12 * Million, "Grand", 4000 * GamingConstants.Millicents,
                ProgressiveLevelType.LP, new List<long> {1 * GamingConstants.Millicents}, true));
            _progInfo.Add(CreateProgressiveResponse(103, 1, 12, 80, 4000));

            // GameOpen
            _gameInfo.Add(CreateGameInfoResponse(new uint[] {101, 102, 103}, 1, 100));

            _progAdapter.Setup(l => l.UpdateLinkedProgressiveLevels(
                It.IsAny<IReadOnlyCollection<IViewableLinkedProgressiveLevel>>(),
                ProtocolNames.HHR)).Verifiable();

            _progAdapter.Setup(l => l.ViewLinkedProgressiveLevel(
                It.IsAny<string>(), out level)).Returns(ret);
        }

        private void SetupProgressiveLevels()
        {
            // Game Levels
            _progressiveLevel.Add(CreateProgressiveLevel(40, 1, "Pack1", 1, 1, 0,
                10 * Million, "Grand", 2000 * GamingConstants.Millicents,
                ProgressiveLevelType.LP, new List<long> {1 * GamingConstants.Millicents}));
            _progressiveLevel.Add(CreateProgressiveLevel(40, 2, "Pack1", 1, 1, 1,
                11 * Million, "Major", 1000 * GamingConstants.Millicents,
                ProgressiveLevelType.LP, new List<long> {1 * GamingConstants.Millicents}));

            // Server Prog Info
            _progInfo.Add(CreateProgressiveResponse(101, 1, 10, 40, 2000));
            _progInfo.Add(CreateProgressiveResponse(102, 2, 11, 40, 1000));
        }

        private void SetupGameOpenResponses()
        {
            // GameOpen
            _gameInfo.Add(CreateGameInfoResponse(new uint[] {101, 102}, 1, 100));
        }

        private GameInfoResponse CreateGameInfoResponse(uint[] progIds, uint denom, uint referenceId, uint progBet = 1)
        {
            return new GameInfoResponse
            {
                ProgressiveIds = progIds,
                Denomination = denom,
                GameId = referenceId,
                ProgCreditsBet = new uint[] { progBet }
            };
        }

        private IGameDetail GetGame(int gameId)
        {
            return GetGames().FirstOrDefault(g => g.Id == gameId);
        }


        private IReadOnlyCollection<IGameDetail> GetGames()
        {
            return new List<IGameDetail>
            {
                new MockGameInfo
                {
                    Id = 1,
                    ReferenceId = "100",
                    VariationId = "1",
                    CdsGameInfos = new[] { new CdsGameInfo("One", 1, 2), new CdsGameInfo("Two", 1, 2) },
                    WagerCategories =
                        new List<IWagerCategory> { new WagerCategory("One", 1), new WagerCategory("Two", 1) }
                },
                new MockGameInfo
                {
                    Id = 2,
                    ReferenceId = "101",
                    VariationId = "2",
                    CdsGameInfos = new[] { new CdsGameInfo("One", 1, 2), new CdsGameInfo("Two", 1, 2) },
                    WagerCategories =
                        new List<IWagerCategory> { new WagerCategory("One", 1), new WagerCategory("Two", 1) }
                }
            };
        }

        private IReadOnlyCollection<IGameDetail> GetGamesWithDuplicateCdsInfos()
        {
            var wagerCategory = new WagerCategory("One", 1, 50, 100, 1000);
            var cdsInfo = new CdsGameInfo("One", 50, 100);

            return new List<IGameDetail>
            {
                new MockGameInfo
                {
                    Id = 1, ReferenceId = "100", VariationId = "1",
                    CdsGameInfos = new ICdsGameInfo[]
                    {
                        cdsInfo,
                        cdsInfo
                    },
                    WagerCategories =
                        new List<IWagerCategory> {wagerCategory, wagerCategory}
                }
            };
        }

        private ProgressiveLevel CreateProgressiveLevel(long wager,
            int progId, string packName, int packId, int gameId, int levelId, long incrementRate, string levelName,
            long resetVal,
            ProgressiveLevelType levelType, IEnumerable<long> denoms,
            bool assignProgId = false, LevelCreationType creationType = LevelCreationType.All)
        {
            return new ProgressiveLevel
            {
                WagerCredits = wager,
                ProgressiveId = progId,
                ProgressivePackName = packName,
                ProgressivePackId = packId,
                GameId = gameId,
                LevelId = levelId,
                IncrementRate = incrementRate,
                LevelName = levelName,
                ResetValue = resetVal,
                LevelType = levelType,
                CanEdit = false,
                Denomination = denoms,
                BetOption = string.Empty,
                AssignedProgressiveId = assignProgId
                    ? new AssignableProgressiveId(AssignableProgressiveType.Linked, "Dummy")
                    : new AssignableProgressiveId(AssignableProgressiveType.None, ""),
                CreationType = creationType,
                Variation = Convert.ToString(gameId)
            };
        }

        private ProgressiveInfoResponse CreateProgressiveResponse(uint progId, uint progLevel, uint contribution,
            uint creditBet, uint resetValue)
        {
            return new ProgressiveInfoResponse
            {
                ProgressiveId = progId,
                ProgLevel = progLevel,
                ProgContribPercent = contribution,
                ProgCreditsBet = creditBet,
                ProgResetValue = resetValue
            };
        }

        private class CdsGameInfo : ICdsGameInfo
        {
            public CdsGameInfo(string id, int minWagerCredits, int maxWagerCredits)
            {
                Id = id;
                MinWagerCredits = minWagerCredits;
                MaxWagerCredits = maxWagerCredits;
            }

            public string Id { get; }

            public int MinWagerCredits { get; }

            public int MaxWagerCredits { get; }
        }
    }
}