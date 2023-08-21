namespace Aristocrat.Monaco.Gaming.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Gaming.Contracts.Events;
    using Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class ReserveServiceTests
    {
        private Mock<IEventBus> _eventBus;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IGamePlayState> _gamePlay;
        private Mock<IPlayerBank> _playerBank;
        private Mock<ISystemDisableManager> _systemDisableManager;
        private Action<ExitReserveButtonPressedEvent> _exitReserveButtonPressedHandler;
        private Action<PropertyChangedEvent> _propertyChangedHandler;
        private Action<OverlayWindowVisibilityChangedEvent> _overlayWindowVisibilityChangedEventHandler;

        private ReserveService _reserve;

        private dynamic _accessor;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _gamePlay = MoqServiceManager.CreateAndAddService<IGamePlayState>(MockBehavior.Strict);
            _playerBank = MoqServiceManager.CreateAndAddService<IPlayerBank>(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _systemDisableManager = MoqServiceManager.CreateAndAddService<ISystemDisableManager>(MockBehavior.Strict);
            _propertiesManager.Setup(p => p.GetProperty(It.IsAny<string>(), It.IsAny<object>()))
                .Returns<string, object>((s, o) => o);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _eventBus.Setup(x => x.UnsubscribeAll(It.IsAny<object>())).Verifiable();
            MoqServiceManager.RemoveInstance();
            _reserve?.Dispose();
        }

        [DataRow(true, false, false, false, false)]
        [DataRow(false, true, false, false, false)]
        [DataRow(false, false, true, false, false)]
        [DataRow(false, false, false, true, false)]
        [DataRow(false, false, false, false, true)]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenNullExpectException(
            bool nullEvent = false,
            bool nullProperties = false,
            bool nullSystemDisable = false,
            bool nullGamePlay = false,
            bool nullPlayerBank = false)
        {
            SetupReserveService(nullEvent, nullProperties, nullSystemDisable, nullGamePlay, nullPlayerBank);
            Assert.IsNull(_reserve);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            SetupReserveService();

            Assert.IsNotNull(_reserve);
        }

        [TestMethod]
        public void WhenReserveServiceNotEnabled_ExpectReserveMachineToFail()
        {
            _propertiesManager.Setup(p => p.GetProperty(ApplicationConstants.ReserveServiceEnabled, It.IsAny<bool>()))
                .Returns(false);

            _propertiesManager.Setup(
                    p => p.GetProperty(ApplicationConstants.ReserveServiceLockupPresent, It.IsAny<bool>()))
                .Returns(false);

            _systemDisableManager.Setup(x => x.Enable(ApplicationConstants.ReserveDisableKey));
            _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid>());

            SetupReserveService();

            _gamePlay.Setup(x => x.CurrentState).Returns(PlayState.Idle);
            _playerBank.Setup(x => x.Balance).Returns(1000);

            Assert.IsFalse(_reserve.ReserveMachine());
            Assert.IsFalse(_reserve.CanReserveMachine);

            _accessor = new DynamicPrivateObject(_reserve);

            Assert.IsNotNull(_accessor._reserveServiceLockupTimer);
            Assert.IsFalse(_reserve.IsMachineReserved);

            //system should not contain Reserve Machine lockup
            _systemDisableManager.Verify(
                m => m.Disable(
                    ApplicationConstants.ReserveDisableKey,
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    null),
                Times.Never);
        }

        [TestMethod]
        public void WhenReserveServiceNotAllowed_ExpectReserveMachineToFail()
        {
            SetupCommon(false, false, 1, false);

            _systemDisableManager.Setup(x => x.Enable(ApplicationConstants.ReserveDisableKey));
            _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid>());

            SetupReserveService();

            _gamePlay.Setup(x => x.CurrentState).Returns(PlayState.Idle);
            _playerBank.Setup(x => x.Balance).Returns(1000);

            Assert.IsFalse(_reserve.ReserveMachine());
            Assert.IsFalse(_reserve.CanReserveMachine);

            _accessor = new DynamicPrivateObject(_reserve);

            Assert.IsNotNull(_accessor._reserveServiceLockupTimer);
            Assert.IsFalse(_reserve.IsMachineReserved);

            //system should not contain Reserve Machine lockup
            _systemDisableManager.Verify(
                m => m.Disable(
                    ApplicationConstants.ReserveDisableKey,
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    null),
                Times.Never);
        }

        [TestMethod]
        public void ReserveMachineLockupOnStartupWithReserveServiceNotAllowed_ExpectLockupToBeRemoved()
        {
            SetupCommon(false, false, 1, true);

            _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid>());
            _propertiesManager.Setup(p => p.SetProperty(ApplicationConstants.ReserveServiceLockupPresent, false));
            _systemDisableManager.Setup(x => x.Enable(ApplicationConstants.ReserveDisableKey));
            _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid>());
            _gamePlay.Setup(x => x.CurrentState).Returns(PlayState.Idle);
            _playerBank.Setup(x => x.Balance).Returns(1000);
            _propertiesManager.Setup(x => x.SetProperty(ApplicationConstants.ReserveServiceLockupRemainingSeconds, It.IsAny<int>()));

            SetupReserveService();

            _accessor = new DynamicPrivateObject(_reserve);

            Assert.IsNotNull(_accessor._reserveServiceLockupTimer);
            Assert.IsFalse(_reserve.IsMachineReserved);
            Assert.IsFalse(_reserve.CanReserveMachine);

            //system Reserve Machine lockup should have been called once
            _systemDisableManager.Verify(
                m => m.Disable(
                    ApplicationConstants.ReserveDisableKey,
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    null),
                Times.Never);

            // we would have tired to remove the lockup
            _systemDisableManager.Verify(m => m.Enable(ApplicationConstants.ReserveDisableKey), Times.Once);
        }

        [TestMethod]
        public void ReserveMachineLockupOnStartupWithReserveServiceEnabled_ExpectLockupToBePresent()
        {
            SetupCommon(true, true, 1, true);

            _propertiesManager.Setup(p => p.SetProperty(ApplicationConstants.ReserveServiceLockupPresent, true));

            _systemDisableManager.Setup(
                x => x.Disable(
                    ApplicationConstants.ReserveDisableKey,
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    null));

            SetupReserveService();

            _accessor = new DynamicPrivateObject(_reserve);

            Assert.IsNotNull(_accessor._reserveServiceLockupTimer);
            Assert.IsTrue(_reserve.IsMachineReserved);

            //system Reserve Machine lockup should have been called once, via initialize
            _systemDisableManager.Verify(
                m => m.Disable(
                    ApplicationConstants.ReserveDisableKey,
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    null),
                Times.Once);

            // we would have not called to remove the Machine Reserved lockup
            _systemDisableManager.Verify(m => m.Enable(ApplicationConstants.ReserveDisableKey), Times.Never);
        }

        [DataTestMethod]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(5)]
        public async Task ChangingEnabledState_AfterReservingMachine_ExpectLockupToBeRemovedAutomaticallyAfterTimeout(int timeout)
        {
            await Task.Run(
                async () =>
                {
                    const int deltaTimeInMs = 300;

                    CreateReserveLockupSuccessfully(timeout);

                    _systemDisableManager.Setup(x => x.Enable(ApplicationConstants.ReserveDisableKey));

                    _propertiesManager.Setup(
                            p => p.GetProperty(ApplicationConstants.ReserveServiceEnabled, It.IsAny<bool>()))
                        .Returns(false);

                    _propertyChangedHandler?.Invoke(
                        new PropertyChangedEvent { PropertyName = ApplicationConstants.ReserveServiceEnabled });

                    await Task.Delay(timeout * 1000 - deltaTimeInMs);

                    Assert.IsTrue(_reserve.IsMachineReserved);

                    await Task.Delay(deltaTimeInMs * 2);

                    Assert.IsFalse(_reserve.IsMachineReserved);

                    _systemDisableManager.Verify(x => x.Enable(ApplicationConstants.ReserveDisableKey), Times.Once);

                    //system Reserve Machine lockup should have been called once, when successful
                    _systemDisableManager.Verify(
                        m => m.Disable(
                            ApplicationConstants.ReserveDisableKey,
                            SystemDisablePriority.Immediate,
                            It.IsAny<Func<string>>(),
                            null),
                        Times.Once);
                });
        }

        [TestMethod]
        public void ChangingEnabledState_AfterServiceInitialization_ExpectReserveToFail()
        {
            SetupCommon(true, true, 1, false);

            SetupReserveService();

            _accessor = new DynamicPrivateObject(_reserve);

            Assert.IsNotNull(_accessor._reserveServiceLockupTimer);
            Assert.IsFalse(_reserve.IsMachineReserved);
            Assert.AreEqual(_accessor._timeoutInSeconds, 1);

            _propertiesManager.Setup(
                    p => p.GetProperty(ApplicationConstants.ReserveServiceEnabled, It.IsAny<bool>()))
                .Returns(false);

            _propertyChangedHandler?.Invoke(
                new PropertyChangedEvent { PropertyName = ApplicationConstants.ReserveServiceEnabled });

            Assert.IsFalse(_reserve.ReserveMachine());
            Assert.IsFalse(_reserve.IsMachineReserved);
        }

        [DataTestMethod]
        [DataRow(1, 10)]
        [DataRow(1, 2)]
        [DataRow(2, 5)]
        public void ChangeInTimeoutForLockupAfterServiceInitialization_ExpectUpdatedTime(int initialTimeout, int updatedTimeOut)
        {
            SetupCommon(true, true, initialTimeout, false);

            SetupReserveService();

            _accessor = new DynamicPrivateObject(_reserve);

            Assert.IsNotNull(_accessor._reserveServiceLockupTimer);
            Assert.IsFalse(_reserve.IsMachineReserved);
            Assert.AreEqual(_accessor._timeoutInSeconds, initialTimeout);

            _propertiesManager.Setup(
                    p => p.GetProperty(ApplicationConstants.ReserveServiceTimeoutInSeconds, It.IsAny<object>()))
                .Returns(updatedTimeOut);

            _propertyChangedHandler?.Invoke(
                new PropertyChangedEvent { PropertyName = ApplicationConstants.ReserveServiceTimeoutInSeconds });

            Assert.AreEqual(_accessor._timeoutInSeconds, updatedTimeOut);
        }

        [TestMethod]
        public void ReserveMachineLockupOnStartupWithReserveServiceDisabled_ExpectLockupToBeRemoved()
        {
            SetupCommon(true, false, 1, true);

            _propertiesManager.Setup(p => p.SetProperty(ApplicationConstants.ReserveServiceLockupPresent, false));
            _propertiesManager.Setup(x => x.SetProperty(ApplicationConstants.ReserveServiceLockupRemainingSeconds, It.IsAny<int>()));
            _systemDisableManager.Setup(x => x.Enable(ApplicationConstants.ReserveDisableKey));

            SetupReserveService();

            _accessor = new DynamicPrivateObject(_reserve);

            Assert.IsNotNull(_accessor._reserveServiceLockupTimer);
            Assert.IsFalse(_reserve.IsMachineReserved);

            //system Reserve Machine lockup should have been called once
            _systemDisableManager.Verify(
                m => m.Disable(
                    ApplicationConstants.ReserveDisableKey,
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    null),
                Times.Never);

            // we would have tired to remove the lockup
            _systemDisableManager.Verify(m => m.Enable(ApplicationConstants.ReserveDisableKey), Times.Once);
        }

        [TestMethod]
        public void ReserveRequestedWhenGameIsNotIdle_ExpectReserveMachineToFail()
        {
            SetupCommon(true, true, 1, false);

            SetupReserveService();

            _accessor = new DynamicPrivateObject(_reserve);

            Assert.IsNotNull(_accessor._reserveServiceLockupTimer);

            _playerBank.Setup(x => x.Balance).Returns(1000);

            foreach (var gameState in Enum.GetValues(typeof(PlayState)).Cast<PlayState>())
            {
                if (gameState.Equals(PlayState.Idle))
                {
                    continue;
                }

                _gamePlay.Setup(x => x.CurrentState).Returns(gameState);

                Assert.IsFalse(_reserve.ReserveMachine());
                Assert.IsFalse(_reserve.IsMachineReserved);

                //system should not contain Reserve Machine lockup
                _systemDisableManager.Verify(
                    m => m.Disable(
                        ApplicationConstants.ReserveDisableKey,
                        SystemDisablePriority.Immediate,
                        It.IsAny<Func<string>>(),
                        null),
                    Times.Never);
            }
        }

        [TestMethod]
        public void ReserveRequestedWhenCreditIsZero_ExpectReserveMachineToFail()
        {
            SetupCommon(true, true, 1, false);

            SetupReserveService();

            _accessor = new DynamicPrivateObject(_reserve);

            Assert.IsNotNull(_accessor._reserveServiceLockupTimer);

            _playerBank.Setup(x => x.Balance).Returns(0);
            _gamePlay.Setup(x => x.CurrentState).Returns(PlayState.Idle);

            Assert.IsFalse(_reserve.ReserveMachine());
            Assert.IsFalse(_reserve.IsMachineReserved);

            //system should not contain Reserve Machine lockup
            _systemDisableManager.Verify(
                m => m.Disable(
                    ApplicationConstants.ReserveDisableKey,
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    null),
                Times.Never);
        }

        [TestMethod]
        public void ReserveRequestedWhenOtherLockupIsPresent_ExpectReserveMachineToFail()
        {
            SetupCommon(true, true, 1, false);

            SetupReserveService();

            _accessor = new DynamicPrivateObject(_reserve);

            Assert.IsNotNull(_accessor._reserveServiceLockupTimer);
            _systemDisableManager.Setup(x => x.CurrentDisableKeys)
                .Returns(new List<Guid> { GamingConstants.OperatorMenuDisableKey });

            _playerBank.Setup(x => x.Balance).Returns(0);
            _gamePlay.Setup(x => x.CurrentState).Returns(PlayState.Idle);

            Assert.IsFalse(_reserve.ReserveMachine());
            Assert.IsFalse(_reserve.IsMachineReserved);

            //system should not contain Reserve Machine lockup
            _systemDisableManager.Verify(
                m => m.Disable(
                    ApplicationConstants.ReserveDisableKey,
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    null),
                Times.Never);
        }

        [TestMethod]
        public void CreateReserveLockupSuccessfully()
        {
            CreateReserveLockupSuccessfully(100);
        }

        [TestMethod]
        public void HandlingExitReserveButtonPressedEvent_ExpectLockupToBeRemoved()
        {
            CreateReserveLockupSuccessfully(100);

            Assert.IsTrue(_reserve.IsMachineReserved);

            _exitReserveButtonPressedHandler?.Invoke(new ExitReserveButtonPressedEvent());

            //system Reserve Machine lockup should have been called once, when successful
            _systemDisableManager.Verify(
                m => m.Disable(
                    ApplicationConstants.ReserveDisableKey,
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    null),
                Times.Once);
        }

        [DataTestMethod]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(5)]
        public async Task WhenExitReserveHappensAutomaticallyAfterTimeOut_ExpectLockupToBeRemoved(int timeout)
        {
            await Task.Run(
                async () =>
                {
                    const int deltaTimeInMs = 300;

                    CreateReserveLockupSuccessfully(timeout);

                    _systemDisableManager.Setup(x => x.Enable(ApplicationConstants.ReserveDisableKey));

                    await Task.Delay(timeout * 1000 - deltaTimeInMs);

                    Assert.IsTrue(_reserve.IsMachineReserved);

                    await Task.Delay(deltaTimeInMs * 2);

                    Assert.IsFalse(_reserve.IsMachineReserved);

                    _systemDisableManager.Verify(x => x.Enable(ApplicationConstants.ReserveDisableKey), Times.Once);

                    //system Reserve Machine lockup should have been called once, when successful
                    _systemDisableManager.Verify(
                        m => m.Disable(
                            ApplicationConstants.ReserveDisableKey,
                            SystemDisablePriority.Immediate,
                            It.IsAny<Func<string>>(),
                            null),
                        Times.Once);
                });
        }

        [TestMethod]
        public async Task WhenExitReserveHappenBeforeTimeOut_ExpectLockupToBeRemoved()
        {
            await Task.Run(
                () =>
                {
                    CreateReserveLockupSuccessfully(4);

                    _systemDisableManager.Setup(x => x.Enable(ApplicationConstants.ReserveDisableKey));

                    _propertiesManager.Setup(
                            p => p.GetProperty(ApplicationConstants.ReserveServiceLockupPresent, It.IsAny<bool>()))
                        .Returns(true);

                    _propertiesManager.Setup(x => x.SetProperty(ApplicationConstants.ReserveServiceLockupRemainingSeconds, It.IsAny<int>()));

                    _propertiesManager.Setup(
                        p => p.SetProperty(ApplicationConstants.ReserveServiceLockupPresent, false));

                    Assert.IsTrue(_reserve.ExitReserveMachine());
                    Assert.IsFalse(_reserve.IsMachineReserved);

                    _systemDisableManager.Verify(x => x.Enable(ApplicationConstants.ReserveDisableKey), Times.Once);

                    //system Reserve Machine lockup should have been called once, when successful
                    _systemDisableManager.Verify(
                        m => m.Disable(
                            ApplicationConstants.ReserveDisableKey,
                            SystemDisablePriority.Immediate,
                            It.IsAny<Func<string>>(),
                            null),
                        Times.Once);
                });
        }

        private void SetupReserveService(
            bool nullEvent = false,
            bool nullProperties = false,
            bool nullSystemDisable = false,
            bool nullGamePlay = false,
            bool nullPlayerBank = false)
        {
            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<ReserveService>(),
                        It.IsAny<Action<ExitReserveButtonPressedEvent>>()))
                .Callback<object, Action<ExitReserveButtonPressedEvent>
                >((y, x) => _exitReserveButtonPressedHandler = x);

            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<ReserveService>(),
                        It.IsAny<Action<PropertyChangedEvent>>()))
                .Callback<object, Action<PropertyChangedEvent>>((y, x) => _propertyChangedHandler = x);

            _eventBus.Setup(
                    x => x.Subscribe(
                    It.IsAny<ReserveService>(),
                    It.IsAny<Action<OverlayWindowVisibilityChangedEvent>>()))
                .Callback<object, Action<OverlayWindowVisibilityChangedEvent>>((y, x) => _overlayWindowVisibilityChangedEventHandler = x);

            _reserve = new ReserveService(
                nullEvent ? null : _eventBus.Object,
                nullProperties ? null : _propertiesManager.Object,
                nullSystemDisable ? null : _systemDisableManager.Object,
                nullGamePlay ? null : _gamePlay.Object,
                nullPlayerBank ? null : _playerBank.Object);
        }

        private void CreateReserveLockupSuccessfully(int timeout)
        {
            SetupCommon(true, true, timeout, false);

            _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid>());

            SetupReserveService();

            _accessor = new DynamicPrivateObject(_reserve);

            Assert.IsNotNull(_accessor._reserveServiceLockupTimer);

            _systemDisableManager.Setup(
                x => x.Disable(
                    ApplicationConstants.ReserveDisableKey,
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    null));

            _propertiesManager.Setup(p => p.SetProperty(ApplicationConstants.ReserveServiceLockupPresent, true));

            _playerBank.Setup(x => x.Balance).Returns(1000);
            _gamePlay.Setup(x => x.CurrentState).Returns(PlayState.Idle);

            Assert.IsTrue(_reserve.CanReserveMachine);
            Assert.IsTrue(_reserve.ReserveMachine());
            Assert.IsTrue(_reserve.IsMachineReserved);

            //system Reserve Machine lockup should have been called once
            _systemDisableManager.Verify(
                m => m.Disable(
                    ApplicationConstants.ReserveDisableKey,
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    null),
                Times.Once);
        }

        private void SetupCommon(
            bool reserveServiceAllowed,
            bool reserveServiceEnabled,
            int reserveServiceTimeoutInSeconds,
            bool reserveServiceLockupPresent)
        {
            _propertiesManager.Setup(p => p.GetProperty(ApplicationConstants.ReserveServiceAllowed, It.IsAny<bool>()))
                .Returns(reserveServiceAllowed);
            _propertiesManager.Setup(p => p.GetProperty(ApplicationConstants.ReserveServiceEnabled, It.IsAny<bool>()))
                .Returns(reserveServiceEnabled);
            _propertiesManager.Setup(
                    p => p.GetProperty(ApplicationConstants.ReserveServiceTimeoutInSeconds, It.IsAny<object>()))
                .Returns(reserveServiceTimeoutInSeconds);
            _propertiesManager.Setup(
                    p => p.GetProperty(ApplicationConstants.ReserveServiceLockupPresent, It.IsAny<bool>()))
                .Returns(reserveServiceLockupPresent);
            _propertiesManager.Setup(
                    p => p.SetProperty(ApplicationConstants.ReserveServiceLockupRemainingSeconds, It.IsAny<int>()));
            _propertiesManager.Setup(
                    p => p.SetProperty(ApplicationConstants.ReserveServiceLockupPresent, It.IsAny<bool>()));
            _propertiesManager.Setup(
                    p => p.SetProperty(ApplicationConstants.ReserveServiceEnabled, It.IsAny<bool>()));
        }
    }
}