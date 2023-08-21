namespace Aristocrat.Monaco.G2S.Tests
{
    using System;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Common.Events;
    using G2S.Handlers;
    using Gaming.Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     Contains tests for the EgmStateObserver class
    /// </summary>
    [TestClass]
    public class EgmStateObserverTest
    {
        private Mock<IPlayerBank> _bank;

        private readonly Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>> _commandBuilder =
            new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>(MockBehavior.Strict);

        private readonly Mock<IEventBus> _eventBus = new Mock<IEventBus>(MockBehavior.Strict);
        private readonly Mock<IEventLift> _eventLift = new Mock<IEventLift>(MockBehavior.Strict);
        private readonly Mock<IG2SEgm> _egm = new Mock<IG2SEgm>(MockBehavior.Strict);
        private readonly Mock<IGameHistory> _gameHistory = new Mock<IGameHistory>(MockBehavior.Strict);
        private readonly Mock<IGamePlayState> _gamePlayState = new Mock<IGamePlayState>();
        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>();

        private EgmStateObserver _target;
        private readonly Mock<ITransactionCoordinator> _transactionCoordinator = new Mock<ITransactionCoordinator>();

        [TestInitialize]
        public void Setup()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _bank = MoqServiceManager.CreateAndAddService<IPlayerBank>(MockBehavior.Strict);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Loose);

            _gameHistory.SetupGet(m => m.IsRecoveryNeeded).Returns(false);

            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<GameIdleEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<PersistentStorageClearStartedEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<EgmStateChangedEvent>()));
            _target = new EgmStateObserver(
                _egm.Object,
                _eventBus.Object,
                _gamePlayState.Object,
                _eventLift.Object,
                _bank.Object,
                _gameHistory.Object,
                _commandBuilder.Object,
                _transactionCoordinator.Object,
                _propertiesManager.Object);
            _egm.Setup(m => m.GetDevice<ICabinetDevice>()).Returns(new Mock<ICabinetDevice>().Object);
            _target.Subscribe();
        }

        [TestCleanup]
        public void Cleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullEgmTest()
        {
            _target = new EgmStateObserver(null, null, null, null, null, null, null, null, null);
            Assert.Fail("Should have thrown exception");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullEventBusTest()
        {
            _target = new EgmStateObserver(_egm.Object, null, null, null, null, null, null, null, null);
            Assert.Fail("Should have thrown exception");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullGamePlayStateTest()
        {
            _target = new EgmStateObserver(_egm.Object, _eventBus.Object, null, null, null, null, null, null, null);
            Assert.Fail("Should have thrown exception");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullEventLiftTest()
        {
            _target = new EgmStateObserver(
                _egm.Object,
                _eventBus.Object,
                _gamePlayState.Object,
                null,
                null,
                null,
                null,
                null,
                null);
            Assert.Fail("Should have thrown exception");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullBankTest()
        {
            _target = new EgmStateObserver(
                _egm.Object,
                _eventBus.Object,
                _gamePlayState.Object,
                _eventLift.Object,
                null,
                null,
                null,
                null,
                null);
            Assert.Fail("Should have thrown exception");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullGameHistoryTest()
        {
            _target = new EgmStateObserver(
                _egm.Object,
                _eventBus.Object,
                _gamePlayState.Object,
                _eventLift.Object,
                _bank.Object,
                null,
                null,
                null,
                null);
            Assert.Fail("Should have thrown exception");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullCommandBuilderTest()
        {
            _target = new EgmStateObserver(
                _egm.Object,
                _eventBus.Object,
                _gamePlayState.Object,
                _eventLift.Object,
                _bank.Object,
                _gameHistory.Object,
                null,
                null,
                null);
            Assert.Fail("Should have thrown exception");
        }

        [TestMethod]
        public void DisposeTest()
        {
            _eventBus.Setup(m => m.UnsubscribeAll(It.IsAny<EgmStateObserver>())).Verifiable();
            _target.Dispose();

            _eventBus.Verify();

            _target.Dispose();
        }

        [TestMethod]
        public void NotifyEnabledChangedNullCabinetTest()
        {
            _egm.Setup(m => m.GetDevice<ICabinetDevice>()).Returns((ICabinetDevice)null).Verifiable();
            _target.NotifyEnabledChanged(null, true);

            _egm.Verify();
        }

        [TestMethod]
        public void NotifyEnabledChangedTest()
        {
            var cabinet = new CabinetDevice(null, _target);
            _egm.Setup(m => m.GetDevice<ICabinetDevice>()).Returns(cabinet).Verifiable();
            _commandBuilder.Setup(m => m.Build(It.IsAny<ICabinetDevice>(), It.IsAny<cabinetStatus>()))
                .Returns((Task)null)
                .Verifiable();
            _eventLift.Setup(m => m.Report(It.IsAny<ICabinetDevice>(), It.IsAny<string>(), It.IsAny<deviceList1>()))
                .Verifiable();

            _target.NotifyEnabledChanged(null, false);

            _egm.Verify();
            _commandBuilder.Verify();
            _eventLift.Verify();
        }

        [TestMethod]
        public void NotifyStateChangedEnabledTest()
        {
            _egm.Setup(m => m.GetDevice<ICabinetDevice>()).Returns((ICabinetDevice)null).Verifiable();
            _target.Unsubscribe();

            _target.NotifyStateChanged(null, EgmState.Enabled, 0);

            _egm.Verify();
        }

        [TestMethod]
        public void NotifyStateChangedOperatorModeTest()
        {
            _egm.Setup(m => m.GetDevice<ICabinetDevice>()).Returns((ICabinetDevice)null).Verifiable();
            _bank.SetupGet(m => m.Balance).Returns(0L);

            _target.NotifyStateChanged(null, EgmState.OperatorMode, 0);

            _egm.Verify();
        }

        [TestMethod]
        public void NotifyStateChangedAuditModeTest()
        {
            _egm.Setup(m => m.GetDevice<ICabinetDevice>()).Returns((ICabinetDevice)null).Verifiable();
            _bank.SetupGet(m => m.Balance).Returns(0L);

            _target.NotifyStateChanged(null, EgmState.AuditMode, 0);

            _egm.Verify();
        }

        [TestMethod]
        public void NotifyStateChangedOperatorDisabledTest()
        {
            _egm.Setup(m => m.GetDevice<ICabinetDevice>()).Returns((ICabinetDevice)null).Verifiable();
            _bank.SetupGet(m => m.Balance).Returns(0L).Verifiable();

            _target.NotifyStateChanged(null, EgmState.OperatorDisabled, 0);

            _egm.Verify();
        }

        [TestMethod]
        public void NotifyStateChangedOperatorLockedTest()
        {
            _egm.Setup(m => m.GetDevice<ICabinetDevice>()).Returns((ICabinetDevice)null).Verifiable();
            _bank.SetupGet(m => m.Balance).Returns(1L).Verifiable();
            _gameHistory.Setup(m => m.IsRecoveryNeeded).Returns(true).Verifiable();

            _target.NotifyStateChanged(null, EgmState.OperatorLocked, 0);

            _egm.Verify();
            _gameHistory.Verify();
        }

        [TestMethod]
        public void NotifyStateChangedTransportDisabledTest()
        {
            _egm.Setup(m => m.GetDevice<ICabinetDevice>()).Returns((ICabinetDevice)null).Verifiable();
            _bank.SetupGet(m => m.Balance).Returns(1L);
            _bank.Setup(m => m.CashOut()).Returns(true);
            _gameHistory.Setup(m => m.IsRecoveryNeeded).Returns(false).Verifiable();

            _target.NotifyStateChanged(null, EgmState.TransportDisabled, 0);

            _egm.Verify();
            _gameHistory.Verify();
        }

        [TestMethod]
        public void NotifyStateChangedHostDisabledTest()
        {
            _egm.Setup(m => m.GetDevice<ICabinetDevice>()).Returns((ICabinetDevice)null).Verifiable();
            _bank.SetupGet(m => m.Balance).Returns(1L);
            _gameHistory.Setup(m => m.IsRecoveryNeeded).Returns(true).Verifiable();

            _target.NotifyStateChanged(null, EgmState.HostDisabled, 0);

            _egm.Verify();
            _gameHistory.Verify();
        }

        [TestMethod]
        public void NotifyStateChangedEgmDisabledTest()
        {
            _egm.Setup(m => m.GetDevice<ICabinetDevice>()).Returns((ICabinetDevice)null).Verifiable();
            _bank.SetupGet(m => m.Balance).Returns(1L);
            _gameHistory.Setup(m => m.IsRecoveryNeeded).Returns(true).Verifiable();

            _target.NotifyStateChanged(null, EgmState.EgmDisabled, -1);

            _egm.Verify();
            _gameHistory.Verify();
        }

        [TestMethod]
        public void NotifyStateChangedEgmLockedTest()
        {
            _egm.Setup(m => m.GetDevice<ICabinetDevice>()).Returns((ICabinetDevice)null).Verifiable();
            _bank.SetupGet(m => m.Balance).Returns(0L).Verifiable();

            _target.NotifyStateChanged(null, EgmState.EgmLocked, 0);

            _egm.Verify();
        }

        [TestMethod]
        public void NotifyStateChangedHostLockedTest()
        {
            _egm.Setup(m => m.GetDevice<ICabinetDevice>()).Returns((ICabinetDevice)null).Verifiable();
            _bank.SetupGet(m => m.Balance).Returns(1L).Verifiable();
            _gameHistory.Setup(m => m.IsRecoveryNeeded).Returns(true).Verifiable();

            _target.NotifyStateChanged(null, EgmState.HostLocked, 0);

            _egm.Verify();
            _gameHistory.Verify();
        }

        [TestMethod]
        public void NotifyStateChangedDemoModeTest()
        {
            _egm.Setup(m => m.GetDevice<ICabinetDevice>()).Returns((ICabinetDevice)null).Verifiable();
            _bank.SetupGet(m => m.Balance).Returns(0L).Verifiable();

            _target.NotifyStateChanged(null, EgmState.DemoMode, 0);

            _egm.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void NotifyStateChangedUnknownStateTest()
        {
            _target.NotifyStateChanged(null, (EgmState)40, 0);
            Assert.Fail("Didn't get exception");
        }

        [TestMethod]
        public void StateRemovedTest()
        {
            var cabinet = new Mock<ICabinetDevice>();
            cabinet.SetupGet(m => m.State).Returns(EgmState.Enabled).Verifiable();
            cabinet.Setup(m => m.Evaluate()).Returns(true).Verifiable();
            _egm.Setup(m => m.GetDevice<ICabinetDevice>()).Returns(cabinet.Object).Verifiable();

            _target.StateRemoved(null, EgmState.DemoMode, -1);
        }

        [TestMethod]
        public void StateAddedWithNullCabinetTest()
        {
            _egm.Setup(m => m.GetDevice<ICabinetDevice>()).Returns((ICabinetDevice)null).Verifiable();

            _gamePlayState.SetupGet(m => m.Idle).Returns(true);

            _target.StateAdded(null, EgmState.DemoMode, -1);

            _egm.Verify();
        }

        [TestMethod]
        public void StateAddedWithStateEnabledAndIdleGameTest()
        {
            var cabinet = new Mock<ICabinetDevice>(MockBehavior.Strict);
            cabinet.Setup(m => m.Evaluate()).Returns(true).Verifiable();
            _egm.Setup(m => m.GetDevice<ICabinetDevice>()).Returns(cabinet.Object).Verifiable();
            _gamePlayState.SetupGet(m => m.Idle).Returns(true);

            _target.StateAdded(null, EgmState.DemoMode, -1);

            _egm.Verify();
            cabinet.Verify();
        }

        [TestMethod]
        public void StateAddedWithStateNotEnabledTest()
        {
            var cabinet = new Mock<ICabinetDevice>(MockBehavior.Strict);
            cabinet.Setup(m => m.Evaluate()).Returns(true).Verifiable();
            _egm.Setup(m => m.GetDevice<ICabinetDevice>()).Returns(cabinet.Object).Verifiable();

            _target.StateAdded(null, EgmState.DemoMode, -1);

            _egm.Verify();
            cabinet.Verify();
        }
    }
}
