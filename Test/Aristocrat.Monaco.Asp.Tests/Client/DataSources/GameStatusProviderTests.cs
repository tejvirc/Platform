namespace Aristocrat.Monaco.Asp.Tests.Client.DataSources
{
    using System;
    using System.IO;
    using Aristocrat.Monaco.Test.Common;
    using Asp.Client.Contracts;
    using Asp.Client.DataSources;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;

    [TestClass]
    public class GameStatusProviderTests
    {
        private Mock<IEventBus> _eventBus;
        private Mock<IFundsTransferDisable> _fundsTransferDisable;
        private GameStatusProvider _gameStatusProvider;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _eventBus = new Mock<IEventBus>(MockBehavior.Strict);
            _fundsTransferDisable = new Mock<IFundsTransferDisable>(MockBehavior.Strict);
            EventBusSetUp();
            _gameStatusProvider = new GameStatusProvider(_eventBus.Object, _fundsTransferDisable.Object);

            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();

            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
        }

        private void EventBusSetUp()
        {
            _eventBus.Setup(e => e.Publish(It.IsAny<DacomGameStatusChangedEvent>()));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullEventBusTest()
        {
            _gameStatusProvider = new GameStatusProvider(null, _fundsTransferDisable.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullFundTransferDisableTest()
        {
            _gameStatusProvider = new GameStatusProvider(_eventBus.Object, null);
        }
        
        [TestMethod]
        public void GameStatusProviderTest()
        {
            Assert.IsNotNull(_gameStatusProvider);
        }

        [TestMethod]
        public void SetGameStatusTest()
        {
            var status = GameEnableStatus.DisableGameDisallowCollect;
            var reason = GameDisableReason.LogicSealBroken;
            _gameStatusProvider.SetGameStatus(status, reason);
            _eventBus.Verify(m => m.Publish(It.IsAny<DacomGameStatusChangedEvent>()), Times.Once);
            Assert.AreEqual(status, (GameEnableStatus)_gameStatusProvider.Status);
            Assert.AreEqual(reason, (GameDisableReason)_gameStatusProvider.Reason);
        }

        [TestMethod]
        public void SetGameStatusWithSameDataTest()
        {
            var status = (GameEnableStatus)_gameStatusProvider.Status;
            var reason = (GameDisableReason)_gameStatusProvider.Reason;
            _gameStatusProvider.SetGameStatus(status, reason);
            _eventBus.Verify(m => m.Publish(It.IsAny<DacomGameStatusChangedEvent>()), Times.Never);
            Assert.AreEqual(status, (GameEnableStatus)_gameStatusProvider.Status);
            Assert.AreEqual(reason, (GameDisableReason)_gameStatusProvider.Reason);
        }

        [TestMethod]
        public void SetHostStatusTest()
        {
            var status = GameEnableStatus.DisableGameDisallowCollect;
            var reason = GameDisableReason.LogicSealBroken;
            _gameStatusProvider.SetHostStatus(status, reason);
            Assert.AreEqual(status, (GameEnableStatus)_gameStatusProvider.HostStatus);
            Assert.AreEqual(reason, (GameDisableReason)_gameStatusProvider.HostReason);
        }

        [TestMethod]
        public void OnSystemDisableRemovedEventWithSystemDisabledTest()
        {
            var status = GameEnableStatus.DisableGameDisallowCollect;
            var reason = GameDisableReason.LogicSealBroken;
            _gameStatusProvider.SetGameStatus(status, reason);

            _gameStatusProvider.HandleEvent(new SystemDisableRemovedEvent(SystemDisablePriority.Normal, new Guid(), string.Empty, false, true));

            Assert.AreEqual(GameEnableStatus.EnableGame, (GameEnableStatus)_gameStatusProvider.Status);
            Assert.AreEqual(reason, (GameDisableReason)_gameStatusProvider.Reason);
        }

        [TestMethod]
        public void OnSystemDisableRemovedEventWithSystemNotDisabledTest()
        {
            var status = GameEnableStatus.DisableGameDisallowCollect;
            var reason = GameDisableReason.LogicSealBroken;
            _gameStatusProvider.SetGameStatus(status, reason);

            _gameStatusProvider.HandleEvent(new SystemDisableRemovedEvent(SystemDisablePriority.Normal, new Guid(), string.Empty, true, true));

            Assert.AreEqual(status, (GameEnableStatus)_gameStatusProvider.Status);
            Assert.AreEqual(reason, (GameDisableReason)_gameStatusProvider.Reason);
        }

        [TestMethod]
        public void OnSystemDisabledAddedEventWithIdleStateNotAffectedTest()
        {
            var status = GameEnableStatus.DisableGameDisallowCollect;
            var reason = GameDisableReason.LogicSealBroken;
            _gameStatusProvider.SetGameStatus(status, reason);

            _gameStatusProvider.HandleEvent(new SystemDisableAddedEvent(SystemDisablePriority.Normal, new Guid(), string.Empty, false));

            Assert.AreEqual(status, (GameEnableStatus)_gameStatusProvider.Status);
            Assert.AreEqual(reason, (GameDisableReason)_gameStatusProvider.Reason);
        }

        [TestMethod]
        public void OnSystemDisabledAddedEventWithIdleStateAffectedTest()
        {
            //Set up Transfer out Disabled and Reason is Empty
            _fundsTransferDisable.Setup(m => m.TransferOffDisabled).Returns(true);

            _gameStatusProvider.HandleEvent(new SystemDisableAddedEvent(SystemDisablePriority.Normal, new Guid(), string.Empty, true));

            Assert.AreEqual(GameEnableStatus.DisableGameDisallowCollect, (GameEnableStatus)_gameStatusProvider.Status);
            Assert.AreEqual(GameDisableReason.OtherEgmLockups, (GameDisableReason)_gameStatusProvider.Reason);

            //Set up Transfer out Disabled and Reason is "Progressive Disconnected"
            _fundsTransferDisable.Setup(m => m.TransferOffDisabled).Returns(true);
            _gameStatusProvider.HandleEvent(new SystemDisableAddedEvent(
                SystemDisablePriority.Normal,
                new Guid(),
                "Progressive Disconnected",
                true));

            Assert.AreEqual(GameEnableStatus.DisableGameDisallowCollect, (GameEnableStatus)_gameStatusProvider.Status);
            Assert.AreEqual(
                GameDisableReason.LinkProgressiveCommsFailure,
                (GameDisableReason)_gameStatusProvider.Reason);

            //Set up Transfer out Disabled and Reason is "Host Cashout Failure"
            _fundsTransferDisable.Setup(m => m.TransferOffDisabled).Returns(true);
            _gameStatusProvider.HandleEvent(new SystemDisableAddedEvent(SystemDisablePriority.Normal, new Guid(), "Host Cashout Failure", true));

            Assert.AreEqual(GameEnableStatus.DisableGameDisallowCollect, (GameEnableStatus)_gameStatusProvider.Status);
            Assert.AreEqual(GameDisableReason.OtherEgmLockups, (GameDisableReason)_gameStatusProvider.Reason);

            //Set up Transfer out Disabled and Reason is "Progressive Disable"
            var hostStatus = GameEnableStatus.DisableGameAllowCollect;
            var hostReason = GameDisableReason.Emergency;
            _gameStatusProvider.SetHostStatus(hostStatus, hostReason);
            _fundsTransferDisable.Setup(m => m.TransferOffDisabled).Returns(true);
            _gameStatusProvider.HandleEvent(new SystemDisableAddedEvent(SystemDisablePriority.Normal, new Guid(), "Progressive Disable", true));

            Assert.AreEqual(hostStatus, (GameEnableStatus)_gameStatusProvider.Status);
            Assert.AreEqual(hostReason, (GameDisableReason)_gameStatusProvider.Reason);

            //Set up Transfer out Disabled and Reason is "Logic Seal Broken"
            _fundsTransferDisable.Setup(m => m.TransferOffDisabled).Returns(true);
            _gameStatusProvider.HandleEvent(new SystemDisableAddedEvent(SystemDisablePriority.Normal, new Guid(), "Logic Seal Is Broken", true));
            Assert.AreEqual(GameEnableStatus.DisableGameDisallowCollect, (GameEnableStatus)_gameStatusProvider.Status);
            Assert.AreEqual(GameDisableReason.LogicSealBroken, (GameDisableReason)_gameStatusProvider.Reason);

            //Set up Transfer out Enabled and Reason is Empty
            _fundsTransferDisable.Setup(m => m.TransferOffDisabled).Returns(false);
            _gameStatusProvider.HandleEvent(new SystemDisableAddedEvent(SystemDisablePriority.Normal, new Guid(), string.Empty, true));

            Assert.AreEqual(GameEnableStatus.DisableGameAllowCollect, (GameEnableStatus)_gameStatusProvider.Status);
            Assert.AreEqual(GameDisableReason.OtherEgmLockups, (GameDisableReason)_gameStatusProvider.Reason);
        }
    }
}