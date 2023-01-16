namespace Aristocrat.Monaco.Asp.Tests.Client.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Kernel.Contracts.MessageDisplay;
    using Asp.Client.Contracts;
    using Asp.Client.DataSources;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class GameStatusDataSourceTests
    {
        private Mock<IEventBus> _eventBus;
        private Mock<IGamePlayState> _gamePlayState;
        private Action<DacomGameStatusChangedEvent> _gameStatusChangedEventCallback;
        private GameStatusDataSource _gameStatusDataSource;
        private Mock<IGameStatusProvider> _gameStatusProvider;
        private Action<GameEndedEvent> _gameEndedEventCallback;
        private Mock<ISystemDisableManager> _systemDisableManager;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            SetupServiceManager();

            _eventBus = new Mock<IEventBus>(MockBehavior.Strict);
            _gamePlayState = new Mock<IGamePlayState>(MockBehavior.Strict);
            _gameStatusProvider = new Mock<IGameStatusProvider>(MockBehavior.Strict);
            _systemDisableManager = new Mock<ISystemDisableManager>(MockBehavior.Strict);

            SetUp();

            _gameStatusDataSource = new GameStatusDataSource(
                _gameStatusProvider.Object,
                _eventBus.Object,
                _systemDisableManager.Object,
                _gamePlayState.Object);
        }

        private static void SetupServiceManager()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MockLocalization.Setup(MockBehavior.Default);
        }

        private void SetUp()
        {
            _eventBus.Setup(
                    m => m.Subscribe(It.IsAny<GameStatusDataSource>(), It.IsAny<Action<DacomGameStatusChangedEvent>>()))
                .Callback<object, Action<DacomGameStatusChangedEvent>>(
                    (subscriber, callback) => _gameStatusChangedEventCallback = callback);

            _eventBus.Setup(
                    m => m.Subscribe(It.IsAny<GameStatusDataSource>(), It.IsAny<Action<GameEndedEvent>>()))
                .Callback<object, Action<GameEndedEvent>>(
                    (subscriber, callback) => _gameEndedEventCallback = callback);

            _eventBus.Setup(s => s.UnsubscribeAll(It.IsAny<object>())).Verifiable();
        }

        [TestMethod]
        public void GameStatusDataSourceTest()
        {
            Assert.IsNotNull(_gameStatusDataSource);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullGameStatusProviderTest()
        {
            _gameStatusDataSource = new GameStatusDataSource(
                null,
                _eventBus.Object,
                _systemDisableManager.Object,
                _gamePlayState.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullEventBusTest()
        {
            _gameStatusDataSource = new GameStatusDataSource(
                _gameStatusProvider.Object,
                null,
                _systemDisableManager.Object,
                _gamePlayState.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullSystemDisableManagerTest()
        {
            _gameStatusDataSource = new GameStatusDataSource(
                _gameStatusProvider.Object,
                _eventBus.Object,
                null,
                _gamePlayState.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullGamePlayStateTest()
        {
            _gameStatusDataSource = new GameStatusDataSource(
                _gameStatusProvider.Object,
                _eventBus.Object,
                _systemDisableManager.Object,
                null);
        }

        [TestMethod]
        public void DataSourceNameTest()
        {
            var expectedName = "Game_Enable";
            Assert.AreEqual(expectedName, _gameStatusDataSource.Name);
        }

        [TestMethod]
        public void MembersTest()
        {
            var expectedMembers = new List<string>()
            {
                "Status",
                "Reason_for_Disabling"
            };

            var actualMembers = _gameStatusDataSource.Members;

            Assert.AreEqual(expectedMembers.Count, actualMembers.Count);
            Assert.IsTrue(actualMembers.SequenceEqual(expectedMembers));
        }

        [TestMethod]
        public void GetMemberValueStatusTest()
        {
            var expected = GameEnableStatus.DisableGameAllowCollect;
            _gameStatusProvider.Setup(m => m.Status).Returns(expected);
            var value = _gameStatusDataSource.GetMemberValue("Status");
            Assert.AreEqual(expected, value);
        }

        [TestMethod]
        public void GetMemberValueReasonTest()
        {
            var expected = GameDisableReason.LargeWin;
            _gameStatusProvider.Setup(m => m.Reason).Returns(expected);
            var value = _gameStatusDataSource.GetMemberValue("Reason_for_Disabling");
            Assert.AreEqual(expected, value);
        }

        [TestMethod]
        public void SetMemberValueStatusTest()
        {
            _gameStatusProvider.Setup(m => m.SetHostStatus(It.IsAny<GameEnableStatus>(), It.IsAny<GameDisableReason>()));
            _systemDisableManager.Setup(m => m.Enable(It.IsAny<Guid>()));
            _systemDisableManager.Setup(m => m.Disable(It.IsAny<Guid>(), It.IsAny<SystemDisablePriority>(), It.IsAny<Func<string>>(), null));

            _gameStatusDataSource.SetMemberValue("Status", GameEnableStatus.EnableGame);
            _gameStatusDataSource.Commit();

            _gameStatusDataSource.SetMemberValue("Status", "Invalid GameEnableStatus");
            _gameStatusDataSource.Commit();

            _gameStatusProvider.Verify(m => m.SetHostStatus(GameEnableStatus.EnableGame, It.IsAny<GameDisableReason>()), Times.Exactly(2));
            _systemDisableManager.Verify(m => m.Enable(It.IsAny<Guid>()), Times.Exactly(2));
            _systemDisableManager.Verify(m => m.Disable(It.IsAny<Guid>(), It.IsAny<SystemDisablePriority>(), It.IsAny<Func<string>>(), null), Times.Never);
        }

        [TestMethod]
        public void SetMemberValueReasonTest()
        {
            _gameStatusProvider.Setup(m => m.SetHostStatus(It.IsAny<GameEnableStatus>(), It.IsAny<GameDisableReason>()));
            _systemDisableManager.Setup(m => m.Enable(It.IsAny<Guid>()));
            _systemDisableManager.Setup(m => m.Disable(It.IsAny<Guid>(), It.IsAny<SystemDisablePriority>(), It.IsAny<Func<string>>(), null));

            _gameStatusDataSource.SetMemberValue("Reason_for_Disabling", GameDisableReason.OtherEgmLockups);
            _gameStatusDataSource.Commit();

            _gameStatusDataSource.SetMemberValue("Reason_for_Disabling", "Invalid GameDisableReason");
            _gameStatusDataSource.Commit();

            _gameStatusProvider.Verify(m => m.SetHostStatus(It.IsAny<GameEnableStatus>(), GameDisableReason.OtherEgmLockups), Times.Exactly(2));
        }

        [TestMethod]
        public void BeginTest()
        {
        }

        [TestMethod]
        public void CommitTestWithGameEnable()
        {
            var status = GameEnableStatus.EnableGame;
            SetUpCommitTests(status);
            _systemDisableManager.Verify(m => m.Enable(It.IsAny<Guid>()), Times.Once);
        }

        [TestMethod]
        public void CommitTestWithGameDisableAllowCollectNotInGameRound()
        {
            var status = GameEnableStatus.DisableGameAllowCollect;
            SetUpCommitTests(status, false, SystemDisablePriority.Normal);
            _systemDisableManager.Verify(
                m => m.Disable(It.IsAny<Guid>(), SystemDisablePriority.Normal, It.IsAny<string>(), It.IsAny<CultureProviderType>(), It.IsAny<object[]>()),
                Times.Once);
        }

        [TestMethod]
        public void CommitTestWithGameDisableAllowCollectInGameRound()
        {
            var status = GameEnableStatus.DisableGameAllowCollect;
            SetUpCommitTests(status);
            _gameStatusDataSource.Commit();
            _systemDisableManager.Verify(
                m => m.Disable(It.IsAny<Guid>(), It.IsAny<SystemDisablePriority>(), It.IsAny<Func<string>>(), null),
                Times.Never);
        }

        [TestMethod]
        public void CommitTestWithGameDisableDisAllowCollectNotInGameRound()
        {
            var status = GameEnableStatus.DisableGameDisallowCollect;
            SetUpCommitTests(status, false);
            _systemDisableManager.Verify(
                m => m.Disable(It.IsAny<Guid>(), SystemDisablePriority.Immediate, It.IsAny<string>(), It.IsAny<CultureProviderType>(), It.IsAny<object[]>()),
                Times.Once);
        }

        [TestMethod]
        public void CommitTestWithGameDisableDisAllowCollectInGameRound()
        {
            var status = GameEnableStatus.DisableGameDisallowCollect;
            SetUpCommitTests(status);
            _systemDisableManager.Verify(
                m => m.Disable(It.IsAny<Guid>(), It.IsAny<SystemDisablePriority>(), It.IsAny<Func<string>>(), null),
                Times.Never);
        }

        [TestMethod]
        public void RollBackTest()
        {
            var status = GameEnableStatus.DisableGameDisallowCollect;
            var reason = GameDisableReason.HostInitiated;
            _gameStatusProvider.Setup(m => m.Status).Returns(status);
            _gameStatusProvider.Setup(m => m.Reason).Returns(reason);

            _gameStatusDataSource.RollBack();

            Assert.AreEqual(status.ToString(), GetGameStatusDataSourcePrivatePropertyValue("Status", _gameStatusDataSource));
            Assert.AreEqual(reason.ToString(), GetGameStatusDataSourcePrivatePropertyValue("Reason", _gameStatusDataSource));
        }

        private string GetGameStatusDataSourcePrivatePropertyValue(string propertyName, GameStatusDataSource gameStatusDataSource)
        {
            var property = typeof(GameStatusDataSource).GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
            var getter = property.GetGetMethod(nonPublic: true);
            return getter.Invoke(gameStatusDataSource, null)?.ToString();
        }

        private void SetUpCommitTests(
            GameEnableStatus status,
            bool inGameRound = true,
            SystemDisablePriority priority = SystemDisablePriority.Immediate)
        {
            //Setup In Game Round
            var reason = GameDisableReason.HostInitiated;
            _gameStatusDataSource.SetMemberValue("Status", status);
            _gameStatusDataSource.SetMemberValue("Reason_for_Disabling", reason);

            //In Game Round
            _gamePlayState.Setup(m => m.InGameRound).Returns(inGameRound);
            _gameStatusProvider.Setup(m => m.SetHostStatus(status, reason));
            if (status == GameEnableStatus.EnableGame)
            {
                _systemDisableManager.Setup(m => m.Enable(It.IsAny<Guid>()));
            }
            else
            {
                _systemDisableManager.Setup(m => m.Disable(It.IsAny<Guid>(), priority, It.IsAny<string>(), It.IsAny<CultureProviderType>(), It.IsAny<object[]>()));
            }

            var expectedDisableReasonKey = "ProgressiveDisable";
            _systemDisableManager.Setup(e => e.Disable(It.IsAny<Guid>(), It.IsAny<SystemDisablePriority>(), It.IsAny<string>(), It.IsAny<CultureProviderType>(), It.IsAny<object[]>()))
                .Callback<Guid, SystemDisablePriority,string, CultureProviderType, object[]>(
                    (guid, disablepriority, resourceKey, type, p) => Assert.AreEqual(expectedDisableReasonKey, resourceKey));

            _gameStatusDataSource.Commit();
        }

        [TestMethod]
        public void GameEndedEventHandlerWithDisableGameDisallowCollectTest()
        {
            var status = GameEnableStatus.DisableGameDisallowCollect;
            SetUpCommitTests(status);

            var log = new Mock<IGameHistoryLog>(MockBehavior.Strict);
            log.Setup(m => m.ShallowCopy()).Returns(log.Object);
            Assert.IsNotNull(_gameEndedEventCallback);
            _gameStatusProvider.Setup(m => m.HostStatus).Returns(status);
            _systemDisableManager.Setup(
                m => m.Disable(It.IsAny<Guid>(), SystemDisablePriority.Immediate, It.IsAny<string>(), It.IsAny<CultureProviderType>(), It.IsAny<object[]>()));
            _gameEndedEventCallback(new GameEndedEvent(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<string>(), log.Object));
            _systemDisableManager.Verify(
                m => m.Disable(It.IsAny<Guid>(), SystemDisablePriority.Immediate, It.IsAny<string>(), It.IsAny<CultureProviderType>(), It.IsAny<object[]>()),
                Times.Once);
        }

        [TestMethod]
        public void GameEndedEventHandlerWithDisableGameAllowCollectTest()
        {
            var status = GameEnableStatus.DisableGameAllowCollect;
            SetUpCommitTests(status);

            var log = new Mock<IGameHistoryLog>(MockBehavior.Strict);
            log.Setup(m => m.ShallowCopy()).Returns(log.Object);
            Assert.IsNotNull(_gameEndedEventCallback);
            _gameStatusProvider.Setup(m => m.HostStatus).Returns(status);
            _systemDisableManager.Setup(
                m => m.Disable(It.IsAny<Guid>(), SystemDisablePriority.Normal, It.IsAny<string>(), It.IsAny<CultureProviderType>(), It.IsAny<object[]>()));
            _gameEndedEventCallback(new GameEndedEvent(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<string>(), log.Object));
            _systemDisableManager.Verify(
                m => m.Disable(It.IsAny<Guid>(), SystemDisablePriority.Normal, It.IsAny<string>(), It.IsAny<CultureProviderType>(), It.IsAny<object[]>()),
                Times.Once);
        }

        [TestMethod]
        public void GameEndedEventHandlerTwice()
        {
            var status = GameEnableStatus.DisableGameAllowCollect;
            SetUpCommitTests(status);

            var log = new Mock<IGameHistoryLog>(MockBehavior.Strict);
            log.Setup(m => m.ShallowCopy()).Returns(log.Object);
            Assert.IsNotNull(_gameEndedEventCallback);
            _gameStatusProvider.Setup(m => m.HostStatus).Returns(status);

            var expectedDisableReasonKey = "ProgressiveDisable";
            _systemDisableManager.Setup(e => e.Disable(It.IsAny<Guid>(), It.IsAny<SystemDisablePriority>(), It.IsAny<string>(), It.IsAny<CultureProviderType>(), It.IsAny<object[]>()))
                .Callback<Guid, SystemDisablePriority, string, CultureProviderType, object[]>(
                    (guid, priority, resourceKey, type, p) => Assert.AreEqual(expectedDisableReasonKey, resourceKey));

            _gameEndedEventCallback(new GameEndedEvent(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<string>(), log.Object));
            _gameEndedEventCallback(new GameEndedEvent(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<string>(), log.Object));

            _systemDisableManager.Verify(
                m => m.Disable(It.IsAny<Guid>(), SystemDisablePriority.Normal, It.IsAny<string>(), It.IsAny<CultureProviderType>(), It.IsAny<object[]>()),
                Times.Once);
        }

        [TestMethod]
        public void HandleGameStatusChangedEventTest()
        {
            var memberValueChangedEventsReceived = 0;
            _gameStatusDataSource.MemberValueChanged += (sender, eventargs) => ++memberValueChangedEventsReceived;

            Assert.IsNotNull(_gameStatusChangedEventCallback);
            _gameStatusChangedEventCallback(
                new DacomGameStatusChangedEvent(It.IsAny<GameEnableStatus>(), It.IsAny<GameDisableReason>()));

            Assert.AreEqual(1, memberValueChangedEventsReceived);
        }

        [TestMethod]
        public void Dispose_ShouldUnsubscribeAll()
        {
            //Call dispose twice - should only unsubscribe/deregister from events once
            _gameStatusDataSource.Dispose();
            _gameStatusDataSource.Dispose();

            _eventBus.Verify(v => v.UnsubscribeAll(It.IsAny<object>()), Times.Once);
        }
    }
}