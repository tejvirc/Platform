namespace Aristocrat.Monaco.Gaming.UI.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.Input;
    using Application.Contracts.OperatorMenu;
    using Aristocrat.Monaco.Hardware.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.Audio;
    using Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using UI.ViewModels;

    [TestClass]
    public class ReserveOverlayViewModelTests
    {
        private const int MaxNoOfRetries = 5;
        private const string TimeFormat = "m\\:ss";
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IEventBus> _eventBus;
        private ReserveOverlayViewModel _target;
        private Mock<IReserveService> _reserveService;
        private Mock<ISystemDisableManager> _disableManager;
        private Mock<IOnScreenKeyboardService> _keyboardService;
        private Mock<IAudio> _audioService;
        private Action<PropertyChangedEvent> _propertyChangedHandler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _reserveService = MoqServiceManager.CreateAndAddService<IReserveService>(MockBehavior.Strict);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _disableManager = MoqServiceManager.CreateAndAddService<ISystemDisableManager>(MockBehavior.Default);
            _keyboardService = MoqServiceManager.CreateAndAddService<IOnScreenKeyboardService>(MockBehavior.Default);
            _audioService = MoqServiceManager.CreateAndAddService<IAudio>(MockBehavior.Loose);

            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<ReserveOverlayViewModel>(),
                        It.IsAny<Action<PropertyChangedEvent>>()))
                .Callback<object, Action<PropertyChangedEvent>>((y, x) => _propertyChangedHandler = x);

            _eventBus.Setup(
                x => x.Subscribe(
                    It.IsAny<ReserveOverlayViewModel>(),
                    It.IsAny<Action<OperatorMenuEnteredEvent>>()));

            _propertiesManager.Setup(x => x.SetProperty(ApplicationConstants.ReserveServiceLockupRemainingSeconds, It.IsAny<int>()));
            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.ReserveServiceLockupRemainingSeconds, It.IsAny<int>()))
                .Returns(It.IsAny<int>());

            _propertiesManager.Setup(x => x.GetProperty(HardwareConstants.PlayerVolumeScalarKey, It.IsAny<byte>()))
                .Returns(It.IsAny<byte>());

            _propertiesManager.Setup(p => p.GetProperty(ApplicationConstants.ReserveServiceLockupPresent, It.IsAny<bool>())).Returns(false);
            _propertiesManager.Setup(x => x.SetProperty(ApplicationConstants.ReserveServicePin, string.Empty));
            _disableManager.Setup(d => d.CurrentDisableKeys).Returns(new List<Guid>());

            _audioService.Setup(a => a.Play(It.IsAny<SoundName>(), It.IsAny<byte>(), It.IsAny<SpeakerMix>(), null));
            _audioService.Setup(a => a.Play(It.IsAny<SoundName>(), It.IsAny<int>(), It.IsAny<float>(), It.IsAny<SpeakerMix>(), null));
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [DataRow(true, false, false, false, false)]
        [DataRow(false, true, false, false, false)]
        [DataRow(false, false, true, false, false)]
        [DataRow(false, false, false, true, false)]
        [DataRow(false, false, false, false, true)]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorTest(
            bool nullEvent,
            bool nullProperties,
            bool nullReserve,
            bool nullDisable,
            bool nullKeyboard)
        {
            CreateTarget(nullEvent, nullProperties, nullReserve, nullDisable, nullKeyboard);
        }

        [TestMethod]
        public void WhenViewModel_CreatedWithoutLockup_ExpectNoLockup()
        {
            const int ReserveServiceTimeoutInSeconds = 10;

            var time = TimeSpan.FromSeconds(ReserveServiceTimeoutInSeconds);

            SetupCommon(ReserveServiceTimeoutInSeconds, false, string.Empty);

            CreateTarget();

            TestInitialState();

            Assert.IsNotNull(_target);
            Assert.IsTrue(IsState(ReserveMachineDisplayState.None));
            Assert.AreEqual(_target.CountdownTimerText, time.ToString(TimeFormat));
            Assert.AreEqual(_target.IncorrectPinWaitTimeLeft, TimeSpan.FromMinutes(1).ToString(TimeFormat));
        }

        [TestMethod]
        public void WhenViewModel_CreatedWithLockup_ExpectLockup()
        {
            const int ReserveServiceTimeoutInSeconds = 10;

            var time = TimeSpan.FromSeconds(ReserveServiceTimeoutInSeconds);

            SetupCommon(ReserveServiceTimeoutInSeconds, true, string.Empty);

            CreateTarget();

            Assert.IsNotNull(_target);

            TestInitialState();

            Assert.IsTrue(IsState(ReserveMachineDisplayState.Countdown));
            Assert.AreEqual(_target.CountdownTimerText, time.ToString(TimeFormat));
            Assert.AreEqual(_target.IncorrectPinWaitTimeLeft, TimeSpan.FromMinutes(1).ToString(TimeFormat));
        }

        [TestMethod]
        public void WhenAskedConfirmPin_EnteringWrongConfirmPin_ExpectNoLockup()
        {
            const int ReserveServiceTimeoutInSeconds = 10;

            var time = TimeSpan.FromSeconds(ReserveServiceTimeoutInSeconds);

            SetupCommon(ReserveServiceTimeoutInSeconds, false, string.Empty);

            CreateTarget();

            _reserveService.Setup(x => x.ReserveMachine()).Returns(true);
            _target.IsDialogVisible = true;

            //Pin Entered
            _target.DigitClickedCommand.Execute("1234");

            _target.ReserveButtonClickedCommand.Execute(new object());

            //Confirm Pin Entered
            _target.DigitClickedCommand.Execute("1235");

            _target.ReserveButtonClickedCommand.Execute(new object());

            // The wrong PIN verification was entered, state still should be Confirm, as
            // they can keep trying until they press Cancel
            Assert.IsTrue(IsState(ReserveMachineDisplayState.Confirm));

            _propertiesManager.Setup(x => x.SetProperty(ApplicationConstants.ReserveServicePin, string.Empty));

            _target.CancelButtonClickedCommand.Execute(new object());

            Assert.IsFalse(_target.IsDialogVisible);

            TestInitialState();
        }

        [TestMethod]
        public void WhenEnteredPin_EnteringCorrectConfirmPin_ExpectLockupCreation()
        {
            const int ReserveServiceTimeoutInSeconds = 10;

            var time = TimeSpan.FromSeconds(ReserveServiceTimeoutInSeconds);

            _reserveService.Setup(x => x.ReserveMachine()).Returns(true);

            SetupCommon(ReserveServiceTimeoutInSeconds, false, string.Empty);

            CreateTarget();

            _propertiesManager.Setup(
                    p => p.GetProperty(ApplicationConstants.ReserveServiceLockupPresent, It.IsAny<bool>()))
                .Returns(true);

            _target.DigitClickedCommand.Execute("1234");

            _target.ReserveButtonClickedCommand.Execute(new object());

            _propertiesManager.Setup(x => x.SetProperty(ApplicationConstants.ReserveServicePin, _target.Pin));

            _target.DigitClickedCommand.Execute("1234");

            _target.ReserveButtonClickedCommand.Execute(new object());

            //Invoke the propertyChange to reflect that the reserve service has locked up the system
            //so that the behaviour can be asserted as per expectation
            _propertyChangedHandler.Invoke(
                new PropertyChangedEvent { PropertyName = ApplicationConstants.ReserveServiceLockupPresent });

            Assert.IsTrue(IsState(ReserveMachineDisplayState.Countdown));
        }

        [TestMethod]
        public void WhenTryingToExitReserve_EnteringCorrectPin_ExpectLockupRemoved()
        {
            CreateLockupSuccessfully();

            _target.ExitReserveButtonClickedCommand.Execute(new object());
            _target.DigitClickedCommand.Execute("1234");

            VerifyPin("1234");

            _reserveService.Setup(x => x.ExitReserveMachine()).Returns(true);
            _propertiesManager.Setup(p => p.GetProperty(ApplicationConstants.ReserveServicePin, It.IsAny<string>())).Returns("1234");
            _propertiesManager.Setup(x => x.SetProperty(ApplicationConstants.ReserveServicePin, string.Empty));

            _target.UnlockButtonClickedCommand.Execute(new object());

            Assert.IsFalse(_target.IsDialogVisible);
        }

        [TestMethod]
        public void WhenMachineReserved_ExhaustMaxRetries_ExpectMaxRetriesDialog()
        {
            CreateLockupSuccessfully();

            var rand = new Random();

            _reserveService.Setup(x => x.ExitReserveMachine()).Returns(true);
            _propertiesManager.Setup(p => p.GetProperty(ApplicationConstants.ReserveServicePin, It.IsAny<string>())).Returns("1234");
            _propertiesManager.Setup(x => x.SetProperty(ApplicationConstants.ReserveServicePin, string.Empty));

            _target.ExitReserveButtonClickedCommand.Execute(new object());

            for (var i = 0; i < MaxNoOfRetries; ++i)
            {
                var pin = rand.Next(1235, 9999).ToString();

                _target.DigitClickedCommand.Execute(pin);

                VerifyPin(pin);

                _target.UnlockButtonClickedCommand.Execute(new object());
            }

            Assert.IsTrue(IsState(ReserveMachineDisplayState.IncorrectPin));
        }

        [TestMethod]
        public void WhenPopulatingPin_CallingClearButton_ExpectedPinToClear()
        {
            const int ReserveServiceTimeoutInSeconds = 10;

            _reserveService.Setup(x => x.ReserveMachine()).Returns(true);

            SetupCommon(ReserveServiceTimeoutInSeconds, false, string.Empty);

            CreateTarget();

            _target.DigitClickedCommand.Execute("1234");

            _target.BackspaceButtonClickedCommand.Execute(new object());

            VerifyPin("123");
        }

        [TestMethod]
        public void WhenMachineNotReserved_CallingCloseButton_ExpectedDialogToClose()
        {
            const int ReserveServiceTimeoutInSeconds = 10;

            SetupCommon(ReserveServiceTimeoutInSeconds, false, string.Empty);

            CreateTarget();

            _target.DigitClickedCommand.Execute("1234");

            VerifyPin("1234");

            _propertiesManager.Setup(x => x.SetProperty(ApplicationConstants.ReserveServicePin, string.Empty));
            _target.CancelButtonClickedCommand.Execute(new object());

            Assert.IsFalse(_target.IsDialogVisible);
        }

        [TestMethod]
        public void WhenMachineReserved_CallingCloseButton_ExpectedLockupScreenToDisplay()
        {
            CreateLockupSuccessfully();

            _target.CancelButtonClickedCommand.Execute(new object());

            Assert.IsTrue(IsState(ReserveMachineDisplayState.Countdown));
        }

        [TestMethod]
        public async Task WhenMachineReserved_LockupCleared_ExpectDialogToClear()
        {
            const int ReserveServiceTimeoutInSeconds = 2;

            var time = TimeSpan.FromSeconds(ReserveServiceTimeoutInSeconds);

            SetupCommon(ReserveServiceTimeoutInSeconds, false, string.Empty);

            CreateLockupSuccessfully();

            await Task.Run(
                async () =>
                {
                    //Wait for 2 seconds and raise PropertyChangedEvent indicating reserve service has removed the lockup
                    await Task.Delay((int)time.TotalMilliseconds);
                    _propertyChangedHandler.Invoke(
                        new PropertyChangedEvent { PropertyName = ApplicationConstants.ReserveServiceLockupPresent });

                    Assert.IsFalse(_target.IsDialogVisible);
                });
        }

        private void CreateTarget(
            bool nullEvent = false,
            bool nullProperties = false,
            bool nullReserve = false,
            bool nullSystemDisable = false,
            bool nullKeyboard = false,
            bool nullAudio = false)
        {
            _target = new ReserveOverlayViewModel(
                nullEvent ? null : _eventBus.Object,
                nullProperties ? null : _propertiesManager.Object,
                nullReserve ? null : _reserveService.Object,
                nullSystemDisable ? null : _disableManager.Object,
                nullKeyboard ? null : _keyboardService.Object,
                nullAudio ? null : _audioService.Object);

            Assert.IsNotNull(_target.BackspaceButtonClickedCommand);
            Assert.IsNotNull(_target.CancelButtonClickedCommand);
            Assert.IsNotNull(_target.ExitReserveButtonClickedCommand);
            Assert.IsNotNull(_target.ReserveButtonClickedCommand);
            Assert.IsNotNull(_target.DigitClickedCommand);
        }

        private void SetupCommon(
            int ReserveServiceTimeoutInSeconds,
            bool reserveServiceLockupPresent,
            string reserveServicePin)
        {
            _propertiesManager.Setup(
                    p => p.GetProperty(ApplicationConstants.ReserveServiceTimeoutInSeconds, It.IsAny<object>()))
                .Returns(ReserveServiceTimeoutInSeconds);
            _propertiesManager.Setup(
                    p => p.GetProperty(ApplicationConstants.ReserveServiceLockupPresent, It.IsAny<bool>()))
                .Returns(reserveServiceLockupPresent);
            _propertiesManager.Setup(
                    p => p.GetProperty(ApplicationConstants.ReserveServicePin, It.IsAny<string>()))
                .Returns(reserveServicePin);
        }

        private void VerifyPin(string pin)
        {
            Assert.IsTrue(_target.Pin == pin);
        }

        private void TestInitialState()
        {
            Assert.IsTrue(_target.Pin == string.Empty);
        }

        private void CreateLockupSuccessfully()
        {
            const int ReserveServiceTimeoutInSeconds = 10;

            _reserveService.Setup(x => x.ReserveMachine()).Returns(true);

            SetupCommon(ReserveServiceTimeoutInSeconds, false, string.Empty);

            CreateTarget();

            _propertiesManager.Setup(x => x.SetProperty(ApplicationConstants.ReserveServicePin, It.IsAny<string>()));
            _propertiesManager.Setup(
                    p => p.GetProperty(ApplicationConstants.ReserveServiceLockupPresent, It.IsAny<bool>()))
                .Returns(true);

            _target.DigitClickedCommand.Execute("1234");

            _target.ReserveButtonClickedCommand.Execute(new object());

            _target.DigitClickedCommand.Execute("1234");

            _target.ReserveButtonClickedCommand.Execute(new object());

            //Invoke the propertyChange to reflect that the reserve service has locked up the system
            //so that the behaviour can be asserted as per expectation
            _propertyChangedHandler.Invoke(
                new PropertyChangedEvent { PropertyName = ApplicationConstants.ReserveServiceLockupPresent });
        }

        private bool IsState(ReserveMachineDisplayState state)
        {
            Assert.AreEqual(state, _target.State);

            switch (state)
            {
                case ReserveMachineDisplayState.Confirm:
                    return
                    _target.ShowPinEntryPanel == true &&
                    _target.ShowCountDownTimer == false &&
                    _target.ShowLockupBackground == false &&
                    _target.ShowIncorrectUnlockPinDisplay == false;
                case ReserveMachineDisplayState.Countdown:
                    return
                    _target.ShowPinEntryPanel == false &&
                    _target.ShowCountDownTimer == true &&
                    _target.ShowLockupBackground == true &&
                    _target.ShowIncorrectUnlockPinDisplay == false;
                case ReserveMachineDisplayState.Exit:
                    return
                    _target.ShowPinEntryPanel == true &&
                    _target.ShowCountDownTimer == false &&
                    _target.ShowLockupBackground == true &&
                    _target.ShowIncorrectUnlockPinDisplay == false;
                case ReserveMachineDisplayState.IncorrectPin:
                    return
                    _target.ShowPinEntryPanel == false &&
                    _target.ShowCountDownTimer == false &&
                    _target.ShowLockupBackground == true &&
                    _target.ShowIncorrectUnlockPinDisplay == true;
                case ReserveMachineDisplayState.None:
                    return
                    _target.ShowPinEntryPanel == false &&
                    _target.ShowCountDownTimer == false &&
                    _target.ShowLockupBackground == false &&
                    _target.ShowIncorrectUnlockPinDisplay == false;
            }
            // Should not come here;
            Assert.IsTrue(false);
            return false;
        }
    }
}