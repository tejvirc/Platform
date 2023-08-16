namespace Aristocrat.Monaco.Asp.Tests.Progressive
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Tests;
    using Aristocrat.Monaco.Asp.Progressive;
    using Aristocrat.Monaco.Gaming.Contracts.Meters;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked;
    using Aristocrat.Monaco.Hardware.Contracts.Button;
    using Aristocrat.Monaco.Hardware.Contracts.Persistence;
    using Aristocrat.Monaco.Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ProgressiveManagerTests
    {
        private Mock<IEventBus> _eventBus;
        private Mock<IProtocolProgressiveEventsRegistry> _protocolProgressiveEventRegistry;
        private Mock<IProtocolLinkedProgressiveAdapter> _protocolLinkedProgressiveAdapter;
        private Mock<IPersistentStorageManager> _persistenceStorageManager;
        private Mock<IPerLevelMeterProvider> _perLevelMeterProvider;
        private Mock<IProgressiveMeterManager> _progressiveMeterManager;
        private Mock<IScopedTransaction> _scopedTransaction;
        private IProgressiveManager _subject;
        private Dictionary<string, Mock<IMeter>> _meters;

        private Action<LinkedProgressiveClaimExpiredEvent> _linkedProgressiveClaimExpiredEventCallback;
        private Action<JackpotNumberAndControllerIdUpdateEvent> _jackpotNumberAndControllerIdUpdateEvent;
        private Action<LinkedProgressiveClaimRefreshedEvent> _linkedProgressiveClaimRefreshedEvent;
        private Action<LinkProgressiveLevelConfigurationAppliedEvent> _linkProgressiveLevelConfigurationAppliedEvent;
        private Action<LinkedProgressiveUpdatedEvent> _LinkedProgressiveUpdatedEvent;
        private Action<DownEvent> _buttonDownEventCallback;

        private readonly LinkedProgressiveLevel linkedLevel = new LinkedProgressiveLevel
        {
            ProtocolName = "DACOM",
            ProgressiveGroupId = 0,
            LevelId = 0,
            Amount = 1000,
            Expiration = DateTime.UtcNow + TimeSpan.FromDays(365),
            CurrentErrorStatus = ProgressiveErrors.None,
            ClaimStatus = new LinkedProgressiveClaimStatus
            {
                WinAmount = 1234,
                Status = LinkedClaimState.Hit
            }
        };

        private readonly ProgressiveLevel progressiveLevel = new ProgressiveLevel
        {
            DeviceId = 1,
            LevelId = 0,
            LevelName = "DACOM, Level Id: 0, Progressive Group Id: 0",
            LevelType = ProgressiveLevelType.LP,
            CurrentValue = 1234,
        };

        private List<IViewableLinkedProgressiveLevel> progressiveLevels = new List<IViewableLinkedProgressiveLevel>
            {
                new LinkedProgressiveLevel
                {
                    LevelId = 0,
                    Expiration = DateTime.Now.AddDays(365),
                    ProgressiveGroupId = 0,
                    ProtocolName = "DACOM",
                    Amount = 100,
                    CurrentErrorStatus = ProgressiveErrors.None,
                },
                new LinkedProgressiveLevel
                {
                    LevelId = 1,
                    Expiration = DateTime.Now.AddDays(365),
                    ProgressiveGroupId = 0,
                    ProtocolName = "DACOM",
                    Amount = 200,
                    CurrentErrorStatus = ProgressiveErrors.None,
                },
                new LinkedProgressiveLevel
                {
                    LevelId = 2,
                    Expiration = DateTime.Now.AddDays(365),
                    ProgressiveGroupId = 0,
                    ProtocolName = "DACOM",
                    Amount = 300,
                    CurrentErrorStatus = ProgressiveErrors.None,
                },
                new LinkedProgressiveLevel
                {
                    LevelId = 3,
                    Expiration = DateTime.Now.AddDays(365),
                    ProgressiveGroupId = 0,
                    ProtocolName = "DACOM",
                    Amount = 400,
                    CurrentErrorStatus = ProgressiveErrors.None,
                },
            };

        public List<(int LevelId, string MeterName)> _notifications = new List<(int LevelId, string MeterName)>();

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _eventBus = new Mock<IEventBus>();
            _protocolProgressiveEventRegistry = new Mock<IProtocolProgressiveEventsRegistry>();
            _protocolLinkedProgressiveAdapter = new Mock<IProtocolLinkedProgressiveAdapter>();
            _persistenceStorageManager = new Mock<IPersistentStorageManager>();
            _perLevelMeterProvider = new Mock<IPerLevelMeterProvider>();
            _progressiveMeterManager = new Mock<IProgressiveMeterManager>();

            _meters = new Dictionary<string, Mock<IMeter>>
            {
                { ProgressivePerLevelMeters.JackpotResetCounter, CreateMockMeter(ProgressivePerLevelMeters.JackpotResetCounter, 0, 0) },
            };

            _scopedTransaction = new Mock<IScopedTransaction>();
            _persistenceStorageManager.Setup(s => s.ScopedTransaction()).Returns(() => _scopedTransaction.Object);

            _progressiveMeterManager.Setup(s => s.IsMeterProvided(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns((int d, int l, string n) => _meters.ContainsKey(n));
            _progressiveMeterManager.Setup(s => s.GetMeter(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns((int d, int l, string n) => _meters[n].Object);

            _protocolLinkedProgressiveAdapter.Setup(s => s.ViewLinkedProgressiveLevels()).Returns(() => progressiveLevels);
            _protocolLinkedProgressiveAdapter.Setup(s => s.ViewConfiguredProgressiveLevels()).Returns(() => new List<ProgressiveLevel>() { progressiveLevel });

            SetupEventBus();

            _subject = new ProgressiveManager(_eventBus.Object, _protocolProgressiveEventRegistry.Object, _protocolLinkedProgressiveAdapter.Object, _persistenceStorageManager.Object, _perLevelMeterProvider.Object);

            _subject.OnNotificationEvent += OnNotificationEvent;
        }

        private void SetupEventBus()
        {
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<LinkedProgressiveClaimExpiredEvent>>()))
                .Callback<object, Action<LinkedProgressiveClaimExpiredEvent>>((subscriber, callback) => _linkedProgressiveClaimExpiredEventCallback = callback);

            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<JackpotNumberAndControllerIdUpdateEvent>>()))
                .Callback<object, Action<JackpotNumberAndControllerIdUpdateEvent>>((subscriber, callback) => _jackpotNumberAndControllerIdUpdateEvent = callback);

            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<DownEvent>>()))
                .Callback<object, Action<DownEvent>>((subscriber, callback) => _buttonDownEventCallback = callback);

            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<LinkedProgressiveClaimRefreshedEvent>>()))
                .Callback<object, Action<LinkedProgressiveClaimRefreshedEvent>>((subscriber, callback) => _linkedProgressiveClaimRefreshedEvent = callback);

            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<LinkProgressiveLevelConfigurationAppliedEvent>>()))
                .Callback<object, Action<LinkProgressiveLevelConfigurationAppliedEvent>>((subscriber, callback) => _linkProgressiveLevelConfigurationAppliedEvent = callback);

            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<LinkedProgressiveUpdatedEvent>>()))
                .Callback<object, Action<LinkedProgressiveUpdatedEvent>>((subscriber, callback) => _LinkedProgressiveUpdatedEvent = callback);
        }

        private void OnNotificationEvent(object sender, OnNotificationEventArgs e) => _notifications.AddRange(e.Notifications.SelectMany(m => m.Value.Select(s => (m.Key, s))));

        [TestCleanup]
        public virtual void Cleanup()
        {
            _subject.OnNotificationEvent -= OnNotificationEvent;
        }

        [TestMethod]
        public void LinkProgressiveLevelConfigurationEvent_NoConfiguredProgressiveLevelTest()
        {
            _protocolLinkedProgressiveAdapter.Setup(x => x.ViewConfiguredProgressiveLevels()).Returns(new List<IViewableProgressiveLevel>());

            LinkProgressiveLevelConfigurationAppliedEvent @event = new LinkProgressiveLevelConfigurationAppliedEvent();
            _linkProgressiveLevelConfigurationAppliedEvent(@event);

            _eventBus.Verify(e => e.Publish(It.IsAny<ProgressiveManageUpdatedEvent>()), Times.Once); // Once on initilaize
        }

        [TestMethod]
        public void UpdateProgressiveJackpotAmountUpdate_NoConfiguredLevelsTest()
        {
            _protocolLinkedProgressiveAdapter.Setup(x => x.ViewLinkedProgressiveLevels()).Returns(new List<IViewableLinkedProgressiveLevel>() );
            _subject.UpdateProgressiveJackpotAmountUpdate(0, 400);
            _protocolLinkedProgressiveAdapter.Verify(x => x.UpdateLinkedProgressiveLevelsAsync(It.IsAny<List<IViewableLinkedProgressiveLevel>>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void LinkProgressiveClaimRefreshedEvent_NewEventTest()
        {
            LinkedProgressiveClaimRefreshedEvent @event = new LinkedProgressiveClaimRefreshedEvent(new List<IViewableLinkedProgressiveLevel>() { linkedLevel });
            _linkedProgressiveClaimRefreshedEvent(@event);

            _eventBus.Verify(e => e.Unsubscribe<DownEvent>(It.IsAny<object>()), Times.Once);
        }

        [TestMethod]
        public void LinkProgressiveLevelConfigurationAppliedEvent_NewEventTest()
        {
            LinkProgressiveLevelConfigurationAppliedEvent @event = new LinkProgressiveLevelConfigurationAppliedEvent();
            _linkProgressiveLevelConfigurationAppliedEvent(@event);

            _eventBus.Verify(e => e.Publish(It.IsAny<ProgressiveManageUpdatedEvent>()), Times.Exactly(2)); // Once on initilaize
        }

        [TestMethod]
        public void LinkProgressiveUpdateEvent_UpdateMeterTest()
        {
            LinkedProgressiveUpdatedEvent @event = new LinkedProgressiveUpdatedEvent(new List<IViewableLinkedProgressiveLevel>() { linkedLevel });
            _LinkedProgressiveUpdatedEvent(@event);
            Assert.AreEqual(1, _notifications.Count(c => c.LevelId == 0 && c.MeterName == ProgressivePerLevelMeters.JackpotAmountUpdate));
        }

        [TestMethod]
        public void UpdateProgressiveJackpotAmountTest()
        {
            _protocolLinkedProgressiveAdapter.Setup(s => s.ViewLinkedProgressiveLevels()).Returns(progressiveLevels);

            _protocolLinkedProgressiveAdapter.Setup(s => s.UpdateLinkedProgressiveLevels(It.IsAny<IReadOnlyCollection<IViewableLinkedProgressiveLevel>>(), ProtocolNames.DACOM)).Verifiable();
            var multipleProgressiveUpdateList = new Dictionary<int, long>()
            {
                [0] = 400, // Progressive with Id 0 set value to 400
                [1] = 322 // Id = 1
            };

            // Get an id and a level amount
            _subject.UpdateProgressiveJackpotAmountUpdate(multipleProgressiveUpdateList);
            _protocolLinkedProgressiveAdapter.Verify(v => v.UpdateLinkedProgressiveLevels(It.IsAny<IReadOnlyCollection<IViewableLinkedProgressiveLevel>>(), ProtocolNames.DACOM), Times.Once);
            _perLevelMeterProvider.Verify(x => x.SetValue(It.IsInRange<int>(0, 1, Moq.Range.Inclusive), It.IsAny<string>(), It.IsAny<long>()), Times.Exactly(2));
        }

        [TestMethod]
        public void GetJackpotAmountsPerLevelTest()
        {
            _perLevelMeterProvider.Setup(x => x.GetValue(It.IsAny<int>(), It.IsAny<string>())).Verifiable();
            _protocolLinkedProgressiveAdapter.Setup(s => s.ViewLinkedProgressiveLevels()).Returns(() => new List<IViewableLinkedProgressiveLevel> { progressiveLevels[0] });
            var levelList = _subject.GetJackpotAmountsPerLevel(); // Calls GetValue only once
            _perLevelMeterProvider.Verify(x => x.GetValue(It.IsAny<int>(), It.IsAny<string>()), Times.Exactly(5));
        }

        /// <summary>Test for Dispose().</summary>
        [TestMethod]
        public void DisposeTest()
        {
            _eventBus.Setup(m => m.UnsubscribeAll(It.IsAny<object>())).Verifiable();

            _subject.Dispose();
            _subject.Dispose();  // test for dispose after already disposed

            _eventBus.Verify(m => m.UnsubscribeAll(It.IsAny<object>()), Times.Once());
        }

        [TestMethod]
        public void ConstructorLoadProgressivesPublishToDataSourceTest()
        {
            _eventBus.Verify(x => x.Publish(It.IsAny<ProgressiveManageUpdatedEvent>()), Times.Once);
        }

        [TestMethod]
        public void NullConstructorTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new ProgressiveManager(null, _protocolProgressiveEventRegistry.Object, _protocolLinkedProgressiveAdapter.Object, _persistenceStorageManager.Object, _perLevelMeterProvider.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new ProgressiveManager(_eventBus.Object, null, _protocolLinkedProgressiveAdapter.Object, _persistenceStorageManager.Object, _perLevelMeterProvider.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new ProgressiveManager(_eventBus.Object, _protocolProgressiveEventRegistry.Object, null, _persistenceStorageManager.Object, _perLevelMeterProvider.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new ProgressiveManager(_eventBus.Object, _protocolProgressiveEventRegistry.Object, _protocolLinkedProgressiveAdapter.Object, null, _perLevelMeterProvider.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new ProgressiveManager(_eventBus.Object, _protocolProgressiveEventRegistry.Object, _protocolLinkedProgressiveAdapter.Object, _persistenceStorageManager.Object, null));
        }

        [TestMethod]
        public void UpdateLinkJackpotHitAmountWon_ShouldIgnoreUpdatesWhenNoActiveJackpot()
        {
            _protocolLinkedProgressiveAdapter.Setup(s => s.ViewLinkedProgressiveLevels()).Returns(() => new List<LinkedProgressiveLevel>());

            _subject.UpdateLinkJackpotHitAmountWon(linkedLevel.LevelId, linkedLevel.Amount);

            _persistenceStorageManager.Verify(s => s.ScopedTransaction(), Times.Never);
            _protocolLinkedProgressiveAdapter.Verify(s => s.ClaimLinkedProgressiveLevel(It.IsAny<string>(), It.Is<string>(i => i == "DACOM")), Times.Never);
            _protocolLinkedProgressiveAdapter.Verify(s => s.AwardLinkedProgressiveLevel(It.IsAny<string>(), It.Is<string>(i => i == "DACOM")), Times.Never);

            Assert.AreEqual(0, _notifications.Count(c => c.LevelId == 0 && c.MeterName == ProgressivePerLevelMeters.JackpotHitStatus));
            Assert.AreEqual(0, _notifications.Count(c => c.LevelId == 0 && c.MeterName == ProgressiveMeters.ProgressiveLevelWinOccurrence));
            Assert.AreEqual(0, _notifications.Count(c => c.LevelId == 0 && c.MeterName == ProgressivePerLevelMeters.JackpotResetCounter));
        }

        [TestMethod]
        public void UpdateLinkJackpotHitAmountWon_ShouldCompleteClaimProcessForActiveJackpot()
        {
            long totalAmount = 0;

            var jackpotHitStatus = 0L;
            _perLevelMeterProvider.Setup(s => s.GetValue(It.Is<int>(i => i == 0), It.Is<string>(i => i == ProgressivePerLevelMeters.JackpotHitStatus))).Returns(jackpotHitStatus);
            _perLevelMeterProvider.Setup(s => s.SetValue(It.Is<int>(i => i == 0), It.Is<string>(i => i == ProgressivePerLevelMeters.JackpotHitStatus), It.IsAny<long>())).Callback((int level, string name, long value) => jackpotHitStatus = value);
            _perLevelMeterProvider.Setup(s => s.IncrementValue(It.Is<int>(i => i == 0), It.Is<string>(i => i == ProgressivePerLevelMeters.TotalJackpotAmount), It.Is<long>(i => i == 1000))).Callback((int l, string m, long a) => totalAmount += a);
            _perLevelMeterProvider.Setup(s => s.GetValue(It.Is<int>(i => i == 0), It.Is<string>(i => i == ProgressivePerLevelMeters.TotalJackpotAmount))).Returns(() => totalAmount);

            int? levelId = null;
            _subject.OnNotificationEvent += (s, e) => levelId = e?.Notifications.Keys.FirstOrDefault();

            //Reset so we can check it's called again after jackpot claim process is complete
            levelId = null;

            //Inject a hit event that we can claim against
            _subject.HandleProgressiveEvent(new LinkedProgressiveHitEvent(progressiveLevel, new List<IViewableLinkedProgressiveLevel> { linkedLevel }, new JackpotTransaction()));

            //Make sure datasource is notified
            Assert.IsNotNull(levelId);
            Assert.IsTrue(levelId == linkedLevel.LevelId);

            //Claim the jackpot and award to the player
            _subject.UpdateLinkJackpotHitAmountWon(linkedLevel.LevelId, linkedLevel.Amount);

            //Check we wrote the changes to datastore
            _persistenceStorageManager.Verify(s => s.ScopedTransaction(), Times.Once);

            //Make sure the game was instructed to claim and award
            _protocolLinkedProgressiveAdapter.Verify(s => s.ClaimLinkedProgressiveLevel(It.Is<string>(i => i == linkedLevel.LevelName), It.Is<string>(i => i == "DACOM")), Times.Once);
            _protocolLinkedProgressiveAdapter.Verify(s => s.AwardLinkedProgressiveLevel(It.Is<string>(i => i == linkedLevel.LevelName), It.Is<long>(i => i == linkedLevel.Amount), It.Is<string>(i => i == "DACOM")), Times.Once);

            //Check we notified the datasource
            Assert.IsNotNull(levelId);
            Assert.IsTrue(levelId == linkedLevel.LevelId);

            //Should be set to one when jackpot hit received, then back to 0 when claim process completed
            _perLevelMeterProvider.Verify(v => v.SetValue(It.Is<int>(i => i == progressiveLevel.LevelId), It.Is<string>(i => i == ProgressivePerLevelMeters.JackpotHitStatus), It.Is<long>(i => i == 1)), Times.Once);
            _perLevelMeterProvider.Verify(v => v.SetValue(It.Is<int>(i => i == progressiveLevel.LevelId), It.Is<string>(i => i == ProgressivePerLevelMeters.JackpotHitStatus), It.Is<long>(i => i == 0)), Times.Once);

            //Should update total hit count and total amount meters
            _perLevelMeterProvider.Verify(v => v.IncrementValue(It.Is<int>(i => i == progressiveLevel.LevelId), It.Is<string>(i => i == ProgressivePerLevelMeters.TotalJackpotHitCount), It.Is<long>(i => i == 1)), Times.Once);
            _perLevelMeterProvider.Verify(v => v.IncrementValue(It.Is<int>(i => i == progressiveLevel.LevelId), It.Is<string>(i => i == ProgressivePerLevelMeters.TotalJackpotAmount), It.Is<long>(i => i == 1000)), Times.Once);
            Assert.AreEqual(1000, totalAmount);

            //Should be updated once during hit/claim cycle
            _perLevelMeterProvider.Verify(v => v.IncrementValue(It.Is<int>(i => i == progressiveLevel.LevelId), It.Is<string>(i => i == ProgressivePerLevelMeters.JackpotResetCounter), It.Is<long>(i => i == 1)), Times.Once);

            _perLevelMeterProvider.Verify(v => v.SetValue(It.Is<int>(i => i == progressiveLevel.LevelId), It.Is<string>(i => i == ProgressivePerLevelMeters.LinkJackpotHitAmountWon), It.Is<long>(i => i == linkedLevel.Amount)), Times.Once);

            Assert.AreEqual(2, _notifications.Count(c => c.LevelId == 0 && c.MeterName == ProgressivePerLevelMeters.JackpotHitStatus));
            Assert.AreEqual(1, _notifications.Count(c => c.LevelId == 0 && c.MeterName == ProgressivePerLevelMeters.TotalJackpotHitCount));
            Assert.AreEqual(1, _notifications.Count(c => c.LevelId == 0 && c.MeterName == ProgressivePerLevelMeters.TotalJackpotAmount));
            Assert.AreEqual(1, _notifications.Count(c => c.LevelId == 0 && c.MeterName == ProgressivePerLevelMeters.JackpotResetCounter));
        }

        [TestMethod]
        public void UpdateProgressiveJackpotAmountUpdate_ShouldUpdateProgressiveLevel()
        {
            _protocolLinkedProgressiveAdapter.Setup(s => s.ViewLinkedProgressiveLevels()).Returns(() => new List<IViewableLinkedProgressiveLevel> { progressiveLevels[0] });

            _protocolLinkedProgressiveAdapter.Setup(s => s.UpdateLinkedProgressiveLevelsAsync(It.IsAny<IReadOnlyCollection<IViewableLinkedProgressiveLevel>>(), ProtocolNames.DACOM)).Verifiable();

            _subject.UpdateProgressiveJackpotAmountUpdate(linkedLevel.LevelId, 5000);

            var newLevel = new LinkedProgressiveLevel
            {
                ProtocolName = linkedLevel.ProtocolName,
                ProgressiveGroupId = linkedLevel.ProgressiveGroupId,
                LevelId = linkedLevel.LevelId,
                Amount = 5000,
                Expiration = It.IsAny<DateTime>(),
                CurrentErrorStatus = ProgressiveErrors.None,
            };

            //Make sure the game was notified of the jackpot amount change
            _protocolLinkedProgressiveAdapter.Verify(v => v.UpdateLinkedProgressiveLevelsAsync(It.IsAny<IReadOnlyCollection<IViewableLinkedProgressiveLevel>>(), ProtocolNames.DACOM), Times.Once);
        }

        [TestMethod]
        public void JackpotNumberAndControllerIdUpdateEvent_ShouldUpdateCurrentJackpotNumberAndControllerIdMeters()
        {
            _jackpotNumberAndControllerIdUpdateEvent(new JackpotNumberAndControllerIdUpdateEvent(0, 1234, 100200300));

            _perLevelMeterProvider.Verify(v => v.SetValue(It.Is<int>(i => i == 0), It.Is<string>(i => i == ProgressivePerLevelMeters.CurrentJackpotNumber), It.Is<long>(i => i == 1234)), Times.Once);
            _perLevelMeterProvider.Verify(v => v.SetValue(It.Is<int>(i => i == 0), It.Is<string>(i => i == ProgressivePerLevelMeters.JackpotControllerId), It.Is<long>(i => i == 100200300)), Times.Once);
        }

        [TestMethod]
        public void HandlerClaimTimeoutCleared_WrongKeyEvent()
        {
            int? levelId = null;
            var perLevelMeterCalls = new Dictionary<string, long>();
            levelId = SetupValidHandleClaimTimeoutCleared(levelId, perLevelMeterCalls);

            //Make sure datasource is notified
            Assert.IsNotNull(levelId);
            Assert.IsTrue(levelId == linkedLevel.LevelId);

            //Signal that the jackpot claim has expired - this will tell system to listen for operator opening jackpot menu
            _linkedProgressiveClaimExpiredEventCallback(new LinkedProgressiveClaimExpiredEvent(new List<IViewableLinkedProgressiveLevel> { linkedLevel }));

            //Incorrect DownEvent
            _buttonDownEventCallback(new DownEvent(110));

            _perLevelMeterProvider.Verify(v => v.IncrementValue(It.Is<int>(i => i == progressiveLevel.LevelId), It.Is<string>(i => i == ProgressivePerLevelMeters.TotalJackpotHitCount), It.Is<long>(i => i == 1)), Times.Once);
            CheckHandleClaimTimeoutDidNotFinish(levelId);
        }

        [TestMethod]
        public void HandlerClainTimeoutCleared_NullActiveJackpotHit()
        {
            int? levelId = null;
            levelId = SetupInvalidHandleClaimTimeout(levelId);

            // Incorrect Event passed -> Creates NullActiveJackpotHit
            _subject.HandleProgressiveEvent(new LinkedProgressiveAddedEvent(new[] { linkedLevel }));
            //Signal that the jackpot claim has expired - this will tell system to listen for operator opening jackpot menu
            _linkedProgressiveClaimExpiredEventCallback(new LinkedProgressiveClaimExpiredEvent(new List<IViewableLinkedProgressiveLevel> { linkedLevel }));

            //Incorrect DownEvent
            _buttonDownEventCallback(new DownEvent(130));

            CheckHandleClaimTimeoutDidNotFinish(levelId);
        }

        [TestMethod]
        public void HandlerClaimTimeoutClearedExceptionRaised_NoJackpotCleared()
        {
            int? levelId = null;
            //var perLevelMeterCalls = new Dictionary<string, long>();
            levelId = SetupInvalidHandleClaimTimeout(levelId);
            _subject.HandleProgressiveEvent(new LinkedProgressiveHitEvent(progressiveLevel, null, new JackpotTransaction()));
            //Signal that the jackpot claim has expired - this will tell system to listen for operator opening jackpot menu
            _linkedProgressiveClaimExpiredEventCallback(new LinkedProgressiveClaimExpiredEvent(new List<LinkedProgressiveLevel> { linkedLevel }));

            //Signal that operator has opened jackpot menu (button id 130)
            _buttonDownEventCallback(new DownEvent(130));

            CheckHandleClaimTimeoutDidNotFinish(levelId);
            _perLevelMeterProvider.Verify(v => v.SetValue(It.Is<int>(i => i == progressiveLevel.LevelId), It.Is<string>(i => i == ProgressivePerLevelMeters.JackpotHitStatus), It.Is<long>(i => i == 1)), Times.Once);
            _perLevelMeterProvider.Verify(v => v.SetValue(It.Is<int>(i => i == progressiveLevel.LevelId), It.Is<string>(i => i == ProgressivePerLevelMeters.JackpotHitStatus), It.Is<long>(i => i == 0)), Times.Never);
            _perLevelMeterProvider.Verify(v => v.IncrementValue(It.Is<int>(i => i == progressiveLevel.LevelId), It.Is<string>(i => i == ProgressivePerLevelMeters.TotalJackpotHitCount), It.Is<long>(i => i == 1)), Times.Once);
            _perLevelMeterProvider.Verify(v => v.IncrementValue(It.Is<int>(i => i == progressiveLevel.LevelId), It.Is<string>(i => i == ProgressivePerLevelMeters.JackpotResetCounter), It.Is<long>(i => i == 1)), Times.Never);

        }

        private int? SetupInvalidHandleClaimTimeout(int? levelId)
        {
            _subject.OnNotificationEvent += (s, e) => levelId = e?.Notifications.Keys.FirstOrDefault();

            var perLevelMeterCalls = new Dictionary<string, long>();
            var jackpotHitStatus = 0L;
            _perLevelMeterProvider.Setup(s => s.SetValue(It.Is<int>(i => i == 0), It.Is<string>(i => i == ProgressivePerLevelMeters.JackpotHitStatus), It.IsAny<long>())).Callback((int level, string meter, long value) =>
            {
                jackpotHitStatus = value;
                if (value == 0)
                {
                    perLevelMeterCalls.Add(meter, value);
                }
            });

            _perLevelMeterProvider.Setup(s => s.GetValue(It.Is<int>(i => i == 0), It.Is<string>(i => i == ProgressivePerLevelMeters.JackpotHitStatus))).Returns(jackpotHitStatus);

            _protocolLinkedProgressiveAdapter.Setup(s => s.ViewLinkedProgressiveLevels()).Returns(() => new List<IViewableLinkedProgressiveLevel> { linkedLevel });

            return levelId;
        }

        private void CheckHandleClaimTimeoutDidNotFinish(int? levelId)
        {
            // No Transaction done
            _persistenceStorageManager.Verify(x => x.ScopedTransaction(), Times.Never);

            // No Claim or Awards given
            _protocolLinkedProgressiveAdapter.Verify(s => s.ClaimLinkedProgressiveLevel(It.Is<string>(i => i == linkedLevel.LevelName), It.Is<string>(i => i == "DACOM")), Times.Never);
            _protocolLinkedProgressiveAdapter.Verify(s => s.AwardLinkedProgressiveLevel(It.Is<string>(i => i == linkedLevel.LevelName), It.Is<long>(i => i == 0), It.Is<string>(i => i == "DACOM")), Times.Never);
        }

        private int? SetupValidHandleClaimTimeoutCleared(int? levelId, Dictionary<string, long> perLevelMeterCalls)
        {
            _subject.OnNotificationEvent += (s, e) => levelId = e?.Notifications.Keys.FirstOrDefault();

            var jackpotHitStatus = 0L;
            _perLevelMeterProvider.Setup(s => s.SetValue(It.Is<int>(i => i == 0), It.Is<string>(i => i == ProgressivePerLevelMeters.JackpotHitStatus), It.IsAny<long>())).Callback((int level, string meter, long value) =>
            {
                jackpotHitStatus = value;
                if (value == 0)
                {
                    perLevelMeterCalls.Add(meter, value);
                }
            });

            _perLevelMeterProvider.Setup(s => s.GetValue(It.Is<int>(i => i == 0), It.Is<string>(i => i == ProgressivePerLevelMeters.JackpotHitStatus))).Returns(jackpotHitStatus);

            _protocolLinkedProgressiveAdapter.Setup(s => s.ViewLinkedProgressiveLevels()).Returns(() => new List<IViewableLinkedProgressiveLevel> { linkedLevel });

            //Inject a hit event that we can claim against
            _subject.HandleProgressiveEvent(new LinkedProgressiveHitEvent(progressiveLevel, new List<IViewableLinkedProgressiveLevel> { linkedLevel }, new JackpotTransaction()));
            return levelId;
        }

        [TestMethod]
        public void HandleClaimTimeoutCleared_ShouldClearActiveJackpotAndPreventPaymentToPlayer()
        {
            int? levelId = null;
            var perLevelMeterCalls = new Dictionary<string, long>();
            levelId = SetupValidHandleClaimTimeoutCleared(levelId, perLevelMeterCalls);

            //Make sure datasource is notified
            Assert.IsNotNull(levelId);
            Assert.IsTrue(levelId == linkedLevel.LevelId);

            //Signal that the jackpot claim has expired - this will tell system to listen for operator opening jackpot menu
            _linkedProgressiveClaimExpiredEventCallback(new LinkedProgressiveClaimExpiredEvent(new[] { linkedLevel }));

            //Signal that operator has opened jackpot menu (button id 130)
            _buttonDownEventCallback(new DownEvent(130));

            //Check we wrote the changes to datastore
            _persistenceStorageManager.Verify(s => s.ScopedTransaction(), Times.Once);

            //Make sure the game was instructed to claim and award
            _protocolLinkedProgressiveAdapter.Verify(s => s.ClaimLinkedProgressiveLevel(It.Is<string>(i => i == linkedLevel.LevelName), It.Is<string>(i => i == "DACOM")), Times.Once);
            _protocolLinkedProgressiveAdapter.Verify(s => s.AwardLinkedProgressiveLevel(It.Is<string>(i => i == linkedLevel.LevelName), It.Is<long>(i => i == 0), It.Is<string>(i => i == "DACOM")), Times.Once);

            //Check we notified the datasource
            Assert.IsNotNull(levelId);
            Assert.IsTrue(levelId == linkedLevel.LevelId);

            //Make sure we updated all required meters
            _perLevelMeterProvider.Verify(v => v.SetValue(It.Is<int>(i => i == progressiveLevel.LevelId), It.Is<string>(i => i == ProgressivePerLevelMeters.JackpotHitStatus), It.Is<long>(i => i == 1)), Times.Once);
            _perLevelMeterProvider.Verify(v => v.SetValue(It.Is<int>(i => i == progressiveLevel.LevelId), It.Is<string>(i => i == ProgressivePerLevelMeters.JackpotHitStatus), It.Is<long>(i => i == 0)), Times.Once);
            _perLevelMeterProvider.Verify(v => v.IncrementValue(It.Is<int>(i => i == progressiveLevel.LevelId), It.Is<string>(i => i == ProgressivePerLevelMeters.TotalJackpotHitCount), It.Is<long>(i => i == 1)), Times.Once);
            _perLevelMeterProvider.Verify(v => v.IncrementValue(It.Is<int>(i => i == progressiveLevel.LevelId), It.Is<string>(i => i == ProgressivePerLevelMeters.JackpotResetCounter), It.Is<long>(i => i == 1)), Times.Once);

            // _progressiveMeterManager.Verify(v => v.GetMeter(It.Is<int>(i => i == progressiveLevel.DeviceId), It.Is<int>(i => i == progressiveLevel.LevelId), It.Is<string>(i => i == ProgressivePerLevelMeters.JackpotResetCounter)), Times.Once);

            //Should be set to zero
            Assert.IsTrue(perLevelMeterCalls.ContainsKey(ProgressivePerLevelMeters.JackpotHitStatus));
            Assert.AreEqual(0, perLevelMeterCalls[ProgressivePerLevelMeters.JackpotHitStatus]);

            //Should be updated once during hit/claim cycle
            //_meters[ProgressivePerLevelMeters.JackpotResetCounter].Verify(v => v.Increment(It.Is<long>(i => i == 1)), Times.Once);

            Assert.AreEqual(2, _notifications.Count(c => c.LevelId == 0 && c.MeterName == ProgressivePerLevelMeters.JackpotHitStatus));
            Assert.AreEqual(1, _notifications.Count(c => c.LevelId == 0 && c.MeterName == ProgressivePerLevelMeters.TotalJackpotHitCount));
            Assert.AreEqual(1, _notifications.Count(c => c.LevelId == 0 && c.MeterName == ProgressivePerLevelMeters.JackpotResetCounter));
        }

        [TestMethod]
        public void HandleProgressiveEvent_SecondJackpotBeforeFirstProcessed_ShouldThrow()
        {
            int? levelId = null;
            _subject.OnNotificationEvent += (s, e) => levelId = e?.Notifications.Keys.FirstOrDefault();

            var perLevelMeterCalls = new Dictionary<string, long>();

            _perLevelMeterProvider.Setup(s => s.SetValue(It.Is<int>(i => i == 0), It.Is<string>(i => i == ProgressivePerLevelMeters.JackpotHitStatus), It.Is<long>(i => i == 0))).Callback((int level, string meter, long value) => perLevelMeterCalls.Add(meter, value));

            _perLevelMeterProvider.SetupSequence(s => s.GetValue(It.Is<int>(i => i == 0), It.Is<string>(i => i == ProgressivePerLevelMeters.JackpotHitStatus))).Returns(1).Returns(0);

            _protocolLinkedProgressiveAdapter.Setup(s => s.ViewLinkedProgressiveLevels()).Returns(() => new List<IViewableLinkedProgressiveLevel> { linkedLevel });

            //Inject a hit event
            _subject.HandleProgressiveEvent(new LinkedProgressiveHitEvent(progressiveLevel, new List<IViewableLinkedProgressiveLevel> { linkedLevel }, new JackpotTransaction()));

            //Make sure datasource is notified
            Assert.IsNotNull(levelId);
            Assert.IsTrue(levelId == linkedLevel.LevelId);

            //Inject a duplicate hit event
            Assert.ThrowsException<DuplicateJackpotHitForLevelException>(() => _subject.HandleProgressiveEvent(new LinkedProgressiveHitEvent(progressiveLevel, new List<IViewableLinkedProgressiveLevel> { linkedLevel }, new JackpotTransaction())));
        }

        private Mock<IMeter> CreateMockMeter(string name, long masterValue, long periodValue)
        {
            Mock<IMeter> meter = new Mock<IMeter>(MockBehavior.Strict);
            meter.SetupGet(m => m.Name).Returns(name);
            meter.SetupGet(m => m.Lifetime).Returns(masterValue);
            meter.SetupGet(m => m.Period).Returns(periodValue);
            meter.SetupGet(m => m.Classification).Returns(new TestMeterClassification());

            meter.Setup(s => s.Increment(It.IsAny<long>())).Verifiable();

            return meter;
        }
    }
}
