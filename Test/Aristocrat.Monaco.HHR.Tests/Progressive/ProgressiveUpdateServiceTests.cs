namespace Aristocrat.Monaco.Hhr.Tests.Progressive
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Reactive.Subjects;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Hhr.Client.Communication;
    using Client.Messages;
    using Events;
    using Hhr.Services;
    using Hhr.Services.Progressive;
    using Storage.Models;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Kernel;
    using Protocol.Common.Storage.Entity;
    using Test.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Storage.Helpers;
    using Gaming.Contracts.Central;

    /// <summary>
    ///     This is a test class for ProgressiveHitEventTest and is intended
    ///     to contain all ProgressiveHitEventTest Unit Tests
    ///     How the test is setup:
    ///         - Progressive Win is denoted by an even length string of the form "0X0Y". (Taking two characters at a time) this means Level 0 is hit 'X' times, Level 1 is hit 'Y' times
    ///         - Eg: If win string is "0203", means :
    ///             - Level 0 hit 2 times, Level 1 hit 3 times. So, There are only 2 Progressive Levels. This will generate 5 Jackpot transactions (2 for Level 0 and 3 for Level 1).
    ///             - JP Transaction are indexed according to Level Hit count [TransactionIndex = PreviousLevelHitCount, PreviousLevelHitCount + 1..., (PreviousLevelHitCount + CurrentLevelHitCount - 1)]
    ///                - Eg: Level 0 Hit 2 times, the transaction Indexes are 0, 1. Level 1 Hit 3 times, the transaction indexes are 2, 3, 4
    /// </summary>
    [TestClass]
    public class ProgressiveUpdateServiceTests
    {
        private const int ProgressiveLevelChunk = 2;
        private const int Wager = 40;
        private readonly List<LinkedProgressiveLevel> _linkedProgressiveLevels = new List<LinkedProgressiveLevel>();
        private readonly List<ProgressiveLevel> _progressiveLevels = new List<ProgressiveLevel>();
        private readonly List<JackpotTransaction> _jackpotTransactions = new List<JackpotTransaction>();

        private readonly Subject<GameProgressiveUpdate> _progressiveUpdatesSubject =
            new Subject<GameProgressiveUpdate>();

        private Mock<IEventBus> _eventBus;
        private Action<GameEndedEvent> _gameEndedEventHandler;
        private Mock<IGameHistoryLog> _gameHistoryLog;
        private IEnumerable<string> _levelsEnumerable = new List<string>();
        private Mock<IPathMapper> _pathMapper;
        private Action<PrizeInformationEvent> _prizeInformationEventHandler;
        private Action<LinkedProgressiveResetEvent> _progressiveAwardedEventHandler;
        private Action<OutcomeFailedEvent> _outcomeFailedEventHandler;
        private Action<GamePlayStateChangedEvent> _gamePlayStateChangedEventHandler;
        private Mock<IProgressiveBroadcastService> _progressiveService;
        private Mock<IGameHistory> _gameHistory;
        private ProgressiveUpdateService _progressiveUpdateService;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IGameProvider> _gameProvider;
        private Mock<IProtocolLinkedProgressiveAdapter> _protocolLinkedProgressiveAdapter;
        private Mock<IUnitOfWork> _unitOfWork;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactory;
        private Mock<IProgressiveUpdateEntityHelper> _progressiveUpdateEntityHelper;
        private Mock<ITransactionHistory> _transactions;
        private Mock<IServiceManager> _serviceManager;

        [TestInitialize]
        public void Initialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            _serviceManager = MoqServiceManager.CreateInstance(MockBehavior.Strict);

            _eventBus = new Mock<IEventBus>(MockBehavior.Strict);
            _protocolLinkedProgressiveAdapter = new Mock<IProtocolLinkedProgressiveAdapter>(MockBehavior.Strict);
            _progressiveService = new Mock<IProgressiveBroadcastService>(MockBehavior.Strict);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);
            _unitOfWorkFactory = new Mock<IUnitOfWorkFactory>(MockBehavior.Strict);
            _unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
            _gameHistoryLog = new Mock<IGameHistoryLog>(MockBehavior.Default);
            _gameHistory = new Mock<IGameHistory>(MockBehavior.Default);
            _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);
            _transactions = new Mock<ITransactionHistory>(MockBehavior.Strict);
            _progressiveUpdateEntityHelper = new Mock<IProgressiveUpdateEntityHelper>(MockBehavior.Strict);

            _progressiveUpdateEntityHelper.SetupGet(p => p.ProgressiveUpdates).Returns(new List<ProgressiveUpdateEntity>());
            _serviceManager.Setup(s => s.GetService<IProtocolLinkedProgressiveAdapter>())
                .Returns(_protocolLinkedProgressiveAdapter.Object);

            _propertiesManager.Setup(p => p.GetProperty(It.IsAny<string>(), It.IsAny<object>()))
                .Returns<string, object>((s, o) => o);

            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.SelectedBetCredits, It.IsAny<long>()))
                .Returns((long) Wager);

            _progressiveService.SetupGet(x => x.ProgressiveUpdates).Returns(_progressiveUpdatesSubject);

            _gameHistory.SetupGet(x => x.CurrentLog).Returns(_gameHistoryLog.Object);

            var outcome = new List<Outcome>
            {
                new Outcome(
                    1,
                    1,
                    1,
                    OutcomeReference.Direct,
                    OutcomeType.Standard,
                    5m.DollarsToMillicents(),
                    0,
                    string.Empty)
            };

            _gameHistoryLog.SetupGet(x => x.Outcomes).Returns(outcome);

            _pathMapper = new Mock<IPathMapper>(MockBehavior.Strict);
            _pathMapper.Setup(m => m.GetDirectory(It.IsAny<string>())).Returns(new DirectoryInfo("."));

            _unitOfWorkFactory.Setup(x => x.Create()).Returns(_unitOfWork.Object);
            _unitOfWork.Setup(x => x.BeginTransaction(IsolationLevel.ReadCommitted));
            _unitOfWork.Setup(x => x.Commit());
            _unitOfWork.Setup(x => x.Dispose());

            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<ProgressiveUpdateService>(),
                        It.IsAny<Action<LinkedProgressiveResetEvent>>()))
                .Callback<object, Action<LinkedProgressiveResetEvent>
                >((y, x) => _progressiveAwardedEventHandler = x);

            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<ProgressiveUpdateService>(),
                        It.IsAny<Action<GameEndedEvent>>()))
                .Callback<object, Action<GameEndedEvent>>((y, x) => _gameEndedEventHandler = x);

            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<ProgressiveUpdateService>(),
                        It.IsAny<Action<PrizeInformationEvent>>()))
                .Callback<object, Action<PrizeInformationEvent>>((y, x) => _prizeInformationEventHandler = x);

            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<ProgressiveUpdateService>(),
                        It.IsAny<Action<OutcomeFailedEvent>>()))
                .Callback<object, Action<OutcomeFailedEvent>
                >((y, x) => _outcomeFailedEventHandler = x);

            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<ProgressiveUpdateService>(),
                        It.IsAny<Action<GamePlayStateChangedEvent>>()))
                .Callback<object, Action<GamePlayStateChangedEvent>
                >((y, x) => _gamePlayStateChangedEventHandler = x);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _eventBus.Setup(x => x.UnsubscribeAll(It.IsAny<object>())).Verifiable();

            _progressiveLevels.Clear();
            _linkedProgressiveLevels.Clear();
            _progressiveUpdateService?.Dispose();

            MoqServiceManager.RemoveInstance();
            AddinManager.Shutdown();
        }

        [DataRow(true, false, false, false, false, false, false, false)]
        [DataRow(false, true, false, false, false, false, false, false)]
        [DataRow(false, false, true, false, false, false, false, false)]
        [DataRow(false, false, false, true, false, false, false, false)]
        [DataRow(false, false, false, false, true, false, false, false)]
        [DataRow(false, false, false, false, false, true, false, false)]
        [DataRow(false, false, false, false, false, false, true, false)]
        [DataRow(false, false, false, false, false, false, false, true)]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenNullExpectException(
            bool nullProtocolLinkedProgressiveAdapter = false,
            bool nullEvent = false,
            bool nullProgressiveService = false,
            bool nullTransactionHistory = false,
            bool nullProgressiveUpdateEntityHelper = false,
            bool nullPropertiesManager = false,
            bool nullGameProvider = false,
            bool nullGameHistory = false)
        {
            SetupProgressiveUpdateService(nullProtocolLinkedProgressiveAdapter, nullEvent, nullProgressiveService,
                nullTransactionHistory, nullProgressiveUpdateEntityHelper, nullPropertiesManager, nullGameProvider, nullGameHistory);

            Assert.IsNull(_progressiveUpdateService);
        }

        private void SetupProgressiveUpdateService(
            bool nullProtocolLinkedProgressiveAdapter = false,
            bool nullEvent = false,
            bool nullProgressiveBroadcastService = false,
            bool nullTransactionHistory = false,
            bool nullProgressiveUpdateEntityHelper = false,
            bool nullPropertiesManager = false,
            bool nullGameProvider = false,
            bool nullGameHistory = false)
        {
            _progressiveUpdateService = new ProgressiveUpdateService(
                nullProtocolLinkedProgressiveAdapter ? null : _protocolLinkedProgressiveAdapter.Object,
                nullEvent ? null : _eventBus.Object,
                nullProgressiveBroadcastService ? null : _progressiveService.Object,
                nullTransactionHistory ? null : _transactions.Object,
                nullProgressiveUpdateEntityHelper ? null : _progressiveUpdateEntityHelper.Object,
                nullPropertiesManager ? null : _propertiesManager.Object,
                nullGameProvider ? null : _gameProvider.Object,
                nullGameHistory ? null : _gameHistory.Object);
        }

        private void SetupProgressiveUpdate(string progressiveWin,
            int linkedProgressiveGroupId)
        {
            var savedProgUpdated = new List<ProgressiveUpdateEntity>();
            var index = 0;

            _levelsEnumerable = Enumerable.Range(0, progressiveWin.Length / ProgressiveLevelChunk)
                .Select(i => progressiveWin.Substring(i * ProgressiveLevelChunk, ProgressiveLevelChunk));

            foreach (var level in _levelsEnumerable)
            {
                int.TryParse(level, out var levelHitTimes);
                savedProgUpdated.Add(new ProgressiveUpdateEntity
                {
                    ProgressiveId = linkedProgressiveGroupId + index,
                    RemainingHitCount = levelHitTimes,
                    AmountToBePaid = 105,
                    CurrentValue = 100,
                    LockUpdates = true
                });
                ++index;
            }

            _progressiveUpdateEntityHelper.SetupGet(p => p.ProgressiveUpdates).Returns(savedProgUpdated);
        }

        private void SetupPreExistingProgressiveUpdates(string progressiveWin,
            int levelsToCreate, int progId,
            int progLevelId, int linkedLevelId,
            int linkedProgressiveGroupId, string protocolName)
        {
            SetupProgressiveUpdate(progressiveWin, linkedProgressiveGroupId);
            CreateProgressiveLevels(levelsToCreate, progId,
                progLevelId, linkedLevelId, linkedProgressiveGroupId,
                protocolName, Wager);

            _protocolLinkedProgressiveAdapter.Setup(x => x.ViewLinkedProgressiveLevels()).Returns(
                _linkedProgressiveLevels);
            _protocolLinkedProgressiveAdapter.Setup(x =>
                x.UpdateLinkedProgressiveLevels(It.IsAny<IReadOnlyCollection<LinkedProgressiveLevel>>(),
                    It.IsAny<string>())).Verifiable();

            SetupProgressiveUpdateService();
        }

        private void SetupLinkedProgressiveLevel(string assignedKey, IViewableLinkedProgressiveLevel level)
        {
            _protocolLinkedProgressiveAdapter.Setup(x => x.ViewLinkedProgressiveLevel(assignedKey, out level))
                .Returns(true);
        }

        [TestMethod]
        public void CreateProgressiveUpdateServiceSuccessfully()
        {
            SetupProgressiveUpdateService();
            Assert.IsNotNull(_progressiveUpdateService);
        }

        [DataRow("0000", 2, 1, 1, 1, 1000)]
        [DataTestMethod]
        public void WithSavedProgressivesWithoutHit_CreateProgressiveUpdateService_ExpectSuccess(string progressiveWin,
            int levelsToCreate, int progId,
            int progLevelId, int linkedLevelId,
            int linkedProgressiveGroupId)
        {
            SetupPreExistingProgressiveUpdates(progressiveWin, levelsToCreate, progId,
                progLevelId, linkedLevelId, linkedProgressiveGroupId, ProtocolNames.HHR);

            Assert.IsNotNull(_progressiveUpdateService);
            _protocolLinkedProgressiveAdapter.Verify(x =>
                x.UpdateLinkedProgressiveLevels(It.IsAny<IReadOnlyCollection<LinkedProgressiveLevel>>(),
                    It.IsAny<string>()), Times.Exactly(2));
        }


        [DataRow("0000", 2, 1, 1, 1, 1000)]
        [DataTestMethod]
        public void WithSavedProgressivesWithoutHit_CallLockProgressiveUpdates_ExpectUpdatesToBeRemoved(
            string progressiveWin, int levelsToCreate, int progId,
            int progLevelId, int linkedLevelId, int linkedProgressiveGroupId)
        {
            SetupPreExistingProgressiveUpdates(progressiveWin, levelsToCreate, progId,
                progLevelId, linkedLevelId, linkedProgressiveGroupId, ProtocolNames.HHR);

            IEnumerable<ProgressiveUpdateEntity> progressiveUpdateEntities = null;

            _progressiveUpdateEntityHelper
                .SetupSet(x => x.ProgressiveUpdates = It.IsAny<List<ProgressiveUpdateEntity>>())
                .Callback<IEnumerable<ProgressiveUpdateEntity>>(
                    progUpdates => progressiveUpdateEntities = progUpdates);

            _outcomeFailedEventHandler?.Invoke(new OutcomeFailedEvent(new CentralTransaction()));

            Assert.IsNotNull(_progressiveUpdateService);

            _progressiveUpdateEntityHelper.VerifySet(
                p => p.ProgressiveUpdates = It.IsAny<IEnumerable<ProgressiveUpdateEntity>>(), Times.Once);

            Assert.IsTrue(!progressiveUpdateEntities.Any());

            _protocolLinkedProgressiveAdapter.Verify(x =>
                x.UpdateLinkedProgressiveLevels(It.IsAny<IReadOnlyCollection<LinkedProgressiveLevel>>(),
                    It.IsAny<string>()), Times.Exactly(4));
        }

        [DataRow("0203", 2, 1, 1, 1, 1000)]
        [DataTestMethod]
        public void WithSavedProgressivesWitHit_CreateProgressiveUpdateService_ExpectSuccess(
            string progressiveWin, int levelsToCreate, int progId,
            int progLevelId, int linkedLevelId, int linkedProgressiveGroupId)
        {
            SetupPreExistingProgressiveUpdates(progressiveWin, levelsToCreate, progId,
                progLevelId, linkedLevelId, linkedProgressiveGroupId, ProtocolNames.HHR);

            Assert.IsNotNull(_progressiveUpdateService);

            _protocolLinkedProgressiveAdapter.Verify(x =>
                x.UpdateLinkedProgressiveLevels(It.IsAny<IReadOnlyCollection<LinkedProgressiveLevel>>(),
                    It.IsAny<string>()), Times.Exactly(2));
        }

        [DataRow("0203", 2, 1, 1, 1, 1000)]
        [DataTestMethod]
        public void WithSavedProgressivesWithHit_CallLockProgressiveUpdates_ExpectUpdates(
            string progressiveWin, int levelsToCreate, int progId,
            int progLevelId, int linkedLevelId, int linkedProgressiveGroupId)
        {
            SetupPreExistingProgressiveUpdates(progressiveWin, levelsToCreate, progId,
                progLevelId, linkedLevelId, linkedProgressiveGroupId, ProtocolNames.HHR);

            IEnumerable<ProgressiveUpdateEntity> progressiveUpdateEntities = null;

            _progressiveUpdateEntityHelper
                .SetupSet(x => x.ProgressiveUpdates = It.IsAny<List<ProgressiveUpdateEntity>>())
                .Callback<IEnumerable<ProgressiveUpdateEntity>>(
                    progUpdates => progressiveUpdateEntities = progUpdates);

            _outcomeFailedEventHandler?.Invoke(new OutcomeFailedEvent(new CentralTransaction()));

            Assert.IsTrue(_progressiveUpdateEntityHelper.Object.ProgressiveUpdates.Count() == 2);

            Assert.IsNotNull(_progressiveUpdateService);

            _protocolLinkedProgressiveAdapter.Verify(x =>
                x.UpdateLinkedProgressiveLevels(It.IsAny<IReadOnlyCollection<LinkedProgressiveLevel>>(),
                    It.IsAny<string>()), Times.Exactly(2));

            Assert.AreEqual(2, progressiveUpdateEntities.Count());
        }


        [DataRow(2, 1, 1, 1, 1000, 12345000)]
        [DataTestMethod]
        public void StartGameRound_ExpectUpdateToBeSavedWithCurrentValue(
            int levelsToCreate, int progId, int progLevelId, int linkedLevelId,
            int linkedProgressiveGroupId, long currentAmount)
        {
            CreateProgressiveLevels(levelsToCreate, progId, progLevelId, linkedLevelId,
                linkedProgressiveGroupId, ProtocolNames.HHR, Wager, currentAmount);

            _protocolLinkedProgressiveAdapter.Setup(x => x.ViewLinkedProgressiveLevels()).Returns(
                _linkedProgressiveLevels);
            _protocolLinkedProgressiveAdapter.Setup(x =>
                x.UpdateLinkedProgressiveLevels(It.IsAny<IReadOnlyCollection<LinkedProgressiveLevel>>(),
                    It.IsAny<string>())).Verifiable();

            IEnumerable<ProgressiveUpdateEntity> progressiveUpdateEntities = null;

            _progressiveUpdateEntityHelper
                .SetupSet(x => x.ProgressiveUpdates = It.IsAny<List<ProgressiveUpdateEntity>>())
                .Callback<IEnumerable<ProgressiveUpdateEntity>>(progUpdates => progressiveUpdateEntities = progUpdates);

            SetupProgressiveUpdateService();

            _gamePlayStateChangedEventHandler?.Invoke(new GamePlayStateChangedEvent(PlayState.Initiated, PlayState.PrimaryGameEscrow));

            Assert.IsNotNull(_progressiveUpdateService);
            _protocolLinkedProgressiveAdapter.Verify(x =>
                x.UpdateLinkedProgressiveLevels(It.IsAny<IReadOnlyCollection<LinkedProgressiveLevel>>(),
                    It.IsAny<string>()), Times.Never);

            var updateEntities = progressiveUpdateEntities as ProgressiveUpdateEntity[] ?? progressiveUpdateEntities.ToArray();

            foreach (var entity in updateEntities)
            {
                Assert.IsTrue(entity.CurrentValue == currentAmount.MillicentsToCents());
            }
        }

        [DataRow(2, 1, 1, 1, 1000, 9021)]
        [DataTestMethod]
        public void WhenWaitingForPrizeInformation_GetProgressiveUpdates_ExpectUpdateToBeSaved(
            int levelsToCreate, int progId,
            int progLevelId, int linkedLevelId,
            int linkedProgressiveGroupId, long amountToUpdate)
        {
            CreateProgressiveLevels(levelsToCreate, progId, progLevelId, linkedLevelId,
                linkedProgressiveGroupId, ProtocolNames.HHR, Wager);

            _protocolLinkedProgressiveAdapter.Setup(x => x.ViewLinkedProgressiveLevels()).Returns(
                _linkedProgressiveLevels);
            _protocolLinkedProgressiveAdapter.Setup(x =>
                x.UpdateLinkedProgressiveLevels(It.IsAny<IReadOnlyCollection<LinkedProgressiveLevel>>(),
                    It.IsAny<string>())).Verifiable();

            IEnumerable<ProgressiveUpdateEntity> progressiveUpdateEntities = null;

            _progressiveUpdateEntityHelper
                .SetupSet(x => x.ProgressiveUpdates = It.IsAny<List<ProgressiveUpdateEntity>>())
                .Callback<IEnumerable<ProgressiveUpdateEntity>>(progUpdates => progressiveUpdateEntities = progUpdates);

            SetupProgressiveUpdateService();

            _gamePlayStateChangedEventHandler?.Invoke(new GamePlayStateChangedEvent(PlayState.Initiated, PlayState.PrimaryGameEscrow));

            // Send updates during recovery
            var progUpdate = new GameProgressiveUpdate
            {
                Id = (uint)(linkedProgressiveGroupId),
                Amount = (uint)amountToUpdate
            };

            _progressiveUpdatesSubject.OnNext(progUpdate);

            Assert.IsNotNull(_progressiveUpdateService);
            _protocolLinkedProgressiveAdapter.Verify(x =>
                x.UpdateLinkedProgressiveLevels(It.IsAny<IReadOnlyCollection<LinkedProgressiveLevel>>(),
                    It.IsAny<string>()), Times.Never);

            var updateEntities = progressiveUpdateEntities as ProgressiveUpdateEntity[] ?? progressiveUpdateEntities.ToArray();

            Assert.IsNotNull(updateEntities.Where(x => x.ProgressiveId == linkedProgressiveGroupId));
            Assert.IsTrue(
                updateEntities.First(x => x.ProgressiveId == linkedProgressiveGroupId).CurrentValue ==
                amountToUpdate);
        }

        [DataRow(2, 1, 1, 1, 1000, 2345)]
        [DataTestMethod]
        public void WhenProgressiveUpdatePresent_CallIsProgressiveLevelUpdateLocked_ExpectUpdateToBeSaved(
            int levelsToCreate, int progId, int progLevelId, int linkedLevelId,
            int linkedProgressiveGroupId, long amountToUpdate)
        {
            CreateProgressiveLevels(levelsToCreate, progId, progLevelId, linkedLevelId,
                linkedProgressiveGroupId, ProtocolNames.HHR, Wager);

            _protocolLinkedProgressiveAdapter.Setup(x => x.ViewLinkedProgressiveLevels()).Returns(
                _linkedProgressiveLevels);
            _protocolLinkedProgressiveAdapter.Setup(x =>
                x.UpdateLinkedProgressiveLevels(It.IsAny<IReadOnlyCollection<LinkedProgressiveLevel>>(),
                    It.IsAny<string>())).Verifiable();

            IEnumerable<ProgressiveUpdateEntity> progressiveUpdateEntities = null;

            _progressiveUpdateEntityHelper
                .SetupSet(x => x.ProgressiveUpdates = It.IsAny<List<ProgressiveUpdateEntity>>())
                .Callback<IEnumerable<ProgressiveUpdateEntity>>(progUpdates => progressiveUpdateEntities = progUpdates);

            SetupProgressiveUpdateService();

            _gamePlayStateChangedEventHandler?.Invoke(new GamePlayStateChangedEvent(PlayState.Initiated, PlayState.PrimaryGameEscrow));

            // Send updates during recovery
            var progUpdate = new GameProgressiveUpdate
            {
                Id = (uint)(linkedProgressiveGroupId),
                Amount = (uint)new Random().Next(0, int.MaxValue)
            };

            _progressiveUpdatesSubject.OnNext(progUpdate);

            var tempUpdate = _linkedProgressiveLevels[0];
            tempUpdate.Amount = amountToUpdate;

            _progressiveUpdateService.IsProgressiveLevelUpdateLocked(tempUpdate);

            Assert.IsNotNull(_progressiveUpdateService);
            _protocolLinkedProgressiveAdapter.Verify(x =>
                x.UpdateLinkedProgressiveLevels(It.IsAny<IReadOnlyCollection<LinkedProgressiveLevel>>(),
                    It.IsAny<string>()), Times.Never);

            var updateEntities = progressiveUpdateEntities.ToList();

            Assert.IsTrue(updateEntities.ToList().First().CurrentValue == amountToUpdate);
        }

        //creates the given number of progressive levels and linked Progressive levels
        // and associates them
        private void CreateProgressiveLevels(int levelsToCreate, int progId,
            int progLevelId, int linkedLevelId,
            int linkedProgressiveGroupId, string protocolName, int wager, long currentAmount = 0)
        {
            for (var i = 0; i < levelsToCreate; ++i)
            {
                var assignedProgressiveKey = $"{protocolName}, " +
                                             $"Level Id: {linkedLevelId + i}, " +
                                             $"Progressive Group Id: {linkedProgressiveGroupId + i}";

                _progressiveLevels.Add(new ProgressiveLevel
                {
                    ProgressiveId = progId + i,
                    LevelId = progLevelId + i,
                    AssignedProgressiveId = new AssignableProgressiveId(AssignableProgressiveType.Linked,
                        assignedProgressiveKey),
                    WagerCredits = wager,
                    ResetValue = (i + 1) * 10000,
                    CurrentValue = currentAmount == 0 ? 100 : currentAmount
                });

                _linkedProgressiveLevels.Add(new LinkedProgressiveLevel
                {
                    ProtocolName = ProtocolNames.HHR,
                    ProgressiveGroupId = linkedProgressiveGroupId + i,
                    LevelId = linkedLevelId + i,
                    Amount = currentAmount == 0 ? 100 : currentAmount,
                    Expiration = DateTime.UtcNow + TimeSpan.FromDays(365),
                    CurrentErrorStatus = ProgressiveErrors.None
                });
                _protocolLinkedProgressiveAdapter.Setup(p => p.GetActiveProgressiveLevels())
                    .Returns(_progressiveLevels);
                SetupLinkedProgressiveLevel(assignedProgressiveKey, _linkedProgressiveLevels[i]);
            }
        }

        private void SetupProgressiveWin(
            string progressiveWin, int levelsToCreate, int progId,
            int progLevelId, int linkedLevelId,
            int linkedProgressiveGroupId, string protocolName, int updatesFromServer, bool simulateServerUpdate = true)
        {

            var prizeInformation = new PrizeInformation
            {
                RaceSet1Wager = Wager / 2,
                RaceSet2Wager = Wager / 2,
                ProgressiveWin = new List<uint> { 2000, 4000, 6000 }
            };

            var prizeLevelsHit = new List<(int, int)>();

            _levelsEnumerable = Enumerable.Range(0, progressiveWin.Length / ProgressiveLevelChunk)
                .Select(i => progressiveWin.Substring(i * ProgressiveLevelChunk, ProgressiveLevelChunk));
            var savedProgUpdated = new List<ProgressiveUpdateEntity>();
            var index = 0;
            foreach (var level in _levelsEnumerable)
            {
                int.TryParse(level, out var levelHitTimes);
                prizeLevelsHit.Add((index, levelHitTimes));
                savedProgUpdated.Add(new ProgressiveUpdateEntity
                {
                    ProgressiveId = linkedProgressiveGroupId + index,
                    RemainingHitCount = levelHitTimes,
                    AmountToBePaid = 105,
                    CurrentValue = 100
                });
                ++index;
            }

            if (prizeLevelsHit.Any())
            {
                // If prize levels are hit, we will have stored these
                _progressiveUpdateEntityHelper.SetupGet(p => p.ProgressiveUpdates).Returns(savedProgUpdated);
            }

            CreateProgressiveLevels(levelsToCreate, progId,
                progLevelId, linkedLevelId,
                linkedProgressiveGroupId, protocolName, Wager);

            CreateJackpotTransactions();

            _protocolLinkedProgressiveAdapter.Setup(x => x.GetActiveProgressiveLevels()).Returns(_progressiveLevels);

            _protocolLinkedProgressiveAdapter.Setup(x =>
                x.UpdateLinkedProgressiveLevels(It.IsAny<IReadOnlyCollection<LinkedProgressiveLevel>>(),
                    It.IsAny<string>()));

            prizeInformation.ProgressiveLevelsHit = prizeLevelsHit;

            var list = Enumerable.Range(1, levelsToCreate).Select(i => (uint)(i * 100)).ToList();
            prizeInformation.ProgressiveWin = list.AsReadOnly();

            _protocolLinkedProgressiveAdapter.Setup(x => x.ViewLinkedProgressiveLevels()).Returns(
                _linkedProgressiveLevels);

            if (simulateServerUpdate)
            {
                _prizeInformationEventHandler?.Invoke(new PrizeInformationEvent(prizeInformation));

                for (var i = 0; i < updatesFromServer; i++)
                {
                    var progUpdate = new GameProgressiveUpdate
                    {
                        Id = (uint)(linkedProgressiveGroupId + i),
                        Amount = (uint)new Random().Next(0, int.MaxValue)
                    };

                    _progressiveUpdatesSubject.OnNext(progUpdate);
                }
            }
        }

        private void CreateJackpotTransactions()
        {
            // Setup JP transactions corresponding to LP Awards events
            var index = 0;
            var transactionId = 0;
            foreach (var level in _levelsEnumerable)
            {
                long.TryParse(level, out var levelHitTimes);

                for (var i = 0; i < levelHitTimes; i++)
                {
                    var jpTransaction = new JackpotTransaction
                    {
                        TransactionId = transactionId,
                        WinAmount = _progressiveLevels[index].CurrentValue,
                        WinText = string.Empty,
                        PayMethod = PayMethod.Any,
                        AssignedProgressiveKey = _progressiveLevels[index].AssignedProgressiveId.AssignedProgressiveKey
                    };

                    _jackpotTransactions.Add(jpTransaction);

                    transactionId++;
                }

                ++index;
            }

            _transactions.Setup(t => t.RecallTransactions<JackpotTransaction>()).Returns(_jackpotTransactions);
        }

        [DataRow("0203", 2, 1, 1, 1, 1000, 3)]
        [DataTestMethod]
        public void WhenWeStartGameRound_HitProgressivesFromGame_ExpectSuccess(
            string progressiveWin, int levelsToCreate, int progId, int progLevelId,
            int linkedLevelId, int linkedProgressiveGroupId, int updatesFromServer)
        {
            _progressiveUpdateEntityHelper.SetupSet(p =>
                p.ProgressiveUpdates = It.IsAny<List<ProgressiveUpdateEntity>>());

            SetupProgressiveWin(progressiveWin, levelsToCreate, progId, progLevelId, linkedLevelId,
                linkedProgressiveGroupId, ProtocolNames.HHR, updatesFromServer);

            SetupProgressiveUpdateService();

            var previousHitCount = 0;

            foreach (var level in _levelsEnumerable)
            {
                long.TryParse(level, out var levelHitTimes);

                var transactionIndex = previousHitCount;

                for (var i = 0; i < levelHitTimes; i++)
                {
                    var jpTransaction = _jackpotTransactions[transactionIndex];
                    var lpResetEvent = new LinkedProgressiveResetEvent(jpTransaction.TransactionId);

                    _progressiveAwardedEventHandler?.Invoke(lpResetEvent);

                    ++transactionIndex;
                }

                previousHitCount = (int)levelHitTimes;
            }

            _gameEndedEventHandler?.Invoke(new GameEndedEvent(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<string>(),
                _gameHistoryLog.Object));
        }

        [DataRow("0203", 2, 1, 1, 1, 1000, 3)]
        [DataTestMethod]
        public void Recovery_WhenProgressivesWereInProgress_DoNotUpdateProgressives(
            string progressiveWin, int levelsToCreate, int progId, int progLevelId,
            int linkedLevelId, int linkedProgressiveGroupId, int updatesFromServer)
        {
            _progressiveUpdateEntityHelper.SetupGet(p => p.ProgressiveUpdates)
                .Returns(new List<ProgressiveUpdateEntity> { CreateProgressiveUpdateEntity(linkedProgressiveGroupId) });
            _progressiveUpdateEntityHelper.SetupSet(p => p.ProgressiveUpdates = It.IsAny<List<ProgressiveUpdateEntity>>());

            SetupProgressiveWin(progressiveWin, levelsToCreate, progId, progLevelId, linkedLevelId,
                linkedProgressiveGroupId, ProtocolNames.HHR, updatesFromServer, false);

            SetupProgressiveUpdateService();

            var attemptProgressiveUpdate = false;
            _protocolLinkedProgressiveAdapter.Setup(p => p.UpdateLinkedProgressiveLevels(It.IsAny<IReadOnlyCollection<IViewableLinkedProgressiveLevel>>(), It.IsAny<string>()))
                                             .Callback<IReadOnlyCollection<IViewableLinkedProgressiveLevel>, object>((one, two) =>
                                             {
                                                 attemptProgressiveUpdate = true;
                                             });

            // Send updates during recovery
            var progUpdate = new GameProgressiveUpdate
            {
                Id = (uint)(linkedProgressiveGroupId),
                Amount = (uint)new Random().Next(0, int.MaxValue)
            };

            _progressiveUpdatesSubject.OnNext(progUpdate);

            Assert.IsFalse(attemptProgressiveUpdate);
        }

        [DataRow("", 2, 1, 1, 1, 1000, 3)]
        [DataTestMethod]
        public void Recovery_WhenProgressivesWereNotInProgress_UpdateProgressives(
            string progressiveWin, int levelsToCreate, int progId, int progLevelId,
            int linkedLevelId, int linkedProgressiveGroupId, int updatesFromServer)
        {
            SetupProgressiveWin(progressiveWin, levelsToCreate, progId, progLevelId, linkedLevelId,
                linkedProgressiveGroupId, ProtocolNames.HHR, updatesFromServer, false);

            SetupProgressiveUpdateService();

            var attemptProgressiveUpdate = false;
            _protocolLinkedProgressiveAdapter.Setup(p => p.UpdateLinkedProgressiveLevels(It.IsAny<IReadOnlyCollection<IViewableLinkedProgressiveLevel>>(), It.IsAny<string>()))
                                             .Callback<IReadOnlyCollection<IViewableLinkedProgressiveLevel>, object>((one, two) =>
                                             {
                                                 attemptProgressiveUpdate = true;
                                             });

            // Send updates during recovery
            var progUpdate = new GameProgressiveUpdate
            {
                Id = (uint)(linkedProgressiveGroupId),
                Amount = (uint)new Random().Next(0, int.MaxValue)
            };

            _progressiveUpdatesSubject.OnNext(progUpdate);

            Assert.IsTrue(attemptProgressiveUpdate);
        }

        [DataRow("0203", 3, 1, 1, 1, 1000, 3)]
        [DataTestMethod]
        public void ReceiveProgressiveUpdate_WhenProgressivesInProgressButNotForReceivedLevel_ExpectUpdateProgressives(
            string progressiveWin, int levelsToCreate, int progId, int progLevelId,
            int linkedLevelId, int linkedProgressiveGroupId, int updatesFromServer)
        {
            _progressiveUpdateEntityHelper.SetupSet(p => p.ProgressiveUpdates = It.IsAny<List<ProgressiveUpdateEntity>>());

            SetupProgressiveWin(progressiveWin, levelsToCreate, progId, progLevelId, linkedLevelId,
                linkedProgressiveGroupId, ProtocolNames.HHR, updatesFromServer, false);

            SetupProgressiveUpdateService();

            var attemptProgressiveUpdate = false;
            _protocolLinkedProgressiveAdapter.Setup(p => p.UpdateLinkedProgressiveLevels(It.IsAny<IReadOnlyCollection<IViewableLinkedProgressiveLevel>>(), It.IsAny<string>()))
                .Callback<IReadOnlyCollection<IViewableLinkedProgressiveLevel>, object>((one, two) =>
                {
                    attemptProgressiveUpdate = true;
                });

            // Send updates during recovery
            var progUpdate = new GameProgressiveUpdate
            {
                Id = (uint)(linkedProgressiveGroupId + 2), // Receive update for another level
                Amount = (uint)new Random().Next(0, int.MaxValue)
            };

            _progressiveUpdatesSubject.OnNext(progUpdate);

            Assert.IsTrue(attemptProgressiveUpdate);
        }

        [DataRow("0203", 2, 1, 1, 1, 1000, 3)]
        [DataTestMethod]
        public void ReceiveProgressiveUpdate_WhenProgressivesInProgressReceivedUpdateForSameLevels_ExpectNoUpdateProgressives(
            string progressiveWin, int levelsToCreate, int progId, int progLevelId,
            int linkedLevelId, int linkedProgressiveGroupId, int updatesFromServer)
        {
            _progressiveUpdateEntityHelper.SetupSet(p => p.ProgressiveUpdates = It.IsAny<List<ProgressiveUpdateEntity>>());

            SetupProgressiveWin(progressiveWin, levelsToCreate, progId, progLevelId, linkedLevelId,
                linkedProgressiveGroupId, ProtocolNames.HHR, updatesFromServer, false);

            SetupProgressiveUpdateService();

            var attemptProgressiveUpdate = false;
            _protocolLinkedProgressiveAdapter.Setup(p => p.UpdateLinkedProgressiveLevels(It.IsAny<IReadOnlyCollection<IViewableLinkedProgressiveLevel>>(), It.IsAny<string>()))
                .Callback<IReadOnlyCollection<IViewableLinkedProgressiveLevel>, object>((one, two) =>
                {
                    attemptProgressiveUpdate = true;
                });

            // Send updates during recovery
            var progUpdate = new GameProgressiveUpdate
            {
                Id = (uint)(linkedProgressiveGroupId), // Receive update for same level
                Amount = (uint)new Random().Next(0, int.MaxValue)
            };

            _progressiveUpdatesSubject.OnNext(progUpdate);

            Assert.IsFalse(attemptProgressiveUpdate);
        }

        [DataRow("", 2, 1, 1, 1, 1000, 3)]
        [DataTestMethod]
        public void ReceiveProgressiveUpdate_WhenProgressivesNotInProgress_ExpectUpdateProgressives(
            string progressiveWin, int levelsToCreate, int progId, int progLevelId,
            int linkedLevelId, int linkedProgressiveGroupId, int updatesFromServer)
        {
            _progressiveUpdateEntityHelper.SetupSet(p => p.ProgressiveUpdates = It.IsAny<List<ProgressiveUpdateEntity>>());

            SetupProgressiveWin(progressiveWin, levelsToCreate, progId, progLevelId, linkedLevelId,
                linkedProgressiveGroupId, ProtocolNames.HHR, updatesFromServer, false);

            SetupProgressiveUpdateService();

            var attemptProgressiveUpdate = false;
            _protocolLinkedProgressiveAdapter.Setup(p => p.UpdateLinkedProgressiveLevels(It.IsAny<IReadOnlyCollection<IViewableLinkedProgressiveLevel>>(), It.IsAny<string>()))
                .Callback<IReadOnlyCollection<IViewableLinkedProgressiveLevel>, object>((one, two) =>
                {
                    attemptProgressiveUpdate = true;
                });

            // Send updates during recovery
            var progUpdate = new GameProgressiveUpdate
            {
                Id = (uint)(linkedProgressiveGroupId + 1), // Receive update for another level
                Amount = (uint)new Random().Next(0, int.MaxValue)
            };

            _progressiveUpdatesSubject.OnNext(progUpdate);

            Assert.IsTrue(attemptProgressiveUpdate);
        }


        [DataRow("0203", 2, 1, 1, 1, 1000, 3)]
        [DataTestMethod]
        public void ProgressiveHit_WhenReceivedProgressiveHitReset_ExpectProgressivesUpdate(
            string progressiveWin, int levelsToCreate, int progId, int progLevelId,
            int linkedLevelId, int linkedProgressiveGroupId, int updatesFromServer)
        {
            IEnumerable<ProgressiveUpdateEntity> progressiveUpdateEntities = null;

            //Progressive hit in progress, if we have persisted it
            _progressiveUpdateEntityHelper.SetupGet(p => p.ProgressiveUpdates)
                .Returns(new List<ProgressiveUpdateEntity> { CreateProgressiveUpdateEntity(linkedProgressiveGroupId) });
            _progressiveUpdateEntityHelper.SetupSet(p => p.ProgressiveUpdates = It.IsAny<List<ProgressiveUpdateEntity>>()).Callback<IEnumerable<ProgressiveUpdateEntity>>((progUpdates) => progressiveUpdateEntities = progUpdates);

            SetupProgressiveWin(progressiveWin, levelsToCreate, progId, progLevelId, linkedLevelId,
                linkedProgressiveGroupId, ProtocolNames.HHR, updatesFromServer, false);

            SetupProgressiveUpdateService();
            var attemptProgressiveUpdate = false;
            _protocolLinkedProgressiveAdapter.Setup(p => p.UpdateLinkedProgressiveLevels(It.IsAny<IReadOnlyCollection<IViewableLinkedProgressiveLevel>>(), It.IsAny<string>()))
                .Callback<IReadOnlyCollection<IViewableLinkedProgressiveLevel>, object>((one, two) =>
                {
                    attemptProgressiveUpdate = true;
                });

            // Send progressive awarded event
            var previousHitCount = 0;
            foreach (var level in _levelsEnumerable)
            {
                long.TryParse(level, out var levelHitTimes);

                var transactionIndex = previousHitCount;

                for (var i = 0; i < levelHitTimes; i++)
                {
                    var jpTransaction = _jackpotTransactions[transactionIndex];
                    var lpResetEvent = new LinkedProgressiveResetEvent(jpTransaction.TransactionId);
                    _transactions.Setup(t => t.RecallTransactions<JackpotTransaction>())
                        .Returns(new List<JackpotTransaction> { jpTransaction });
                    _progressiveAwardedEventHandler?.Invoke(lpResetEvent);

                    ++transactionIndex;
                }

                previousHitCount = (int)levelHitTimes;
            }

            Assert.IsNotNull(progressiveUpdateEntities);
            Assert.AreEqual(0, progressiveUpdateEntities.Count()); // Progressive updates have been applied so, this will be empty
            Assert.IsTrue(attemptProgressiveUpdate); // once progressive updates have been applied this will be true.
        }

        private static ProgressiveUpdateEntity CreateProgressiveUpdateEntity(long progressiveGroupId, long remainingHitCount = 1, long amountToBePaid = 110, long currentValue = 100)
        {
            return new ProgressiveUpdateEntity
            {
                Id = progressiveGroupId,
                ProgressiveId = progressiveGroupId,
                RemainingHitCount = remainingHitCount,
                AmountToBePaid = amountToBePaid,
                CurrentValue = currentValue
            };
        }
    }
}