namespace Aristocrat.Monaco.Gaming.Tests
{
    using System;
    using Kernel;
    using Contracts;
    using Application.Contracts.Media;
    using Gaming.Runtime;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Hardware.Contracts.Cabinet;
    using Moq;
    using Test.Common;

    [TestClass]
    public class AttendantServiceTests
    {
        private readonly Mock<IEventBus> _eventBus = new Mock<IEventBus>();
        private readonly Mock<IMediaPlayerResizeManager> _resizeManager = new Mock<IMediaPlayerResizeManager>();
        private readonly Mock<IHardwareHelper> _hardwareHelper = new Mock<IHardwareHelper>();
        private readonly Mock<IRuntime> _runtime = new Mock<IRuntime>();
        private readonly Mock<IRuntimeFlagHandler> _runtimeFlags = new Mock<IRuntimeFlagHandler>();
        private readonly Mock<ICabinetDetectionService> _cabinetDetectionService = new Mock<ICabinetDetectionService>();
        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>();

        private AttendantService _attendant;

        private dynamic _accessor;

        [TestInitialize]
        public void Initialize()
        {
            _attendant = CreateTarget();
            _propertiesManager.Setup(p => p.GetProperty(It.IsAny<string>(), It.IsAny<object>()))
                .Returns<string, object>((s, o) => o);
        }

        [DataTestMethod]
        [DataRow(true, false, false, false, false, false)]
        [DataRow(false, true, false, false, false, false)]
        [DataRow(false, false, true, false, false, false)]
        [DataRow(false, false, false, true, false, false)]
        [DataRow(false, false, false, false, true, false)]
        [DataRow(false, false, false, false, false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorArgumentsTest(
            bool nullEvent,
            bool nullResize,
            bool nullFlags,
            bool nullRuntime,
            bool nullCabinet,
            bool nullProperty)
        {
            CreateTarget(nullEvent, nullResize, nullFlags, nullRuntime, nullCabinet, nullProperty);
        }

        [TestMethod]
        public void WhenServiceButtonIsPressedExpectShowServiceConfirmationEvent()
        {
            SetupAttendantServiceWithTouchVbd();

            _attendant.OnServiceButtonPressed();

            _eventBus.Verify(b => b.Publish(It.IsAny<ShowServiceConfirmationEvent>()), Times.Once);
        }

        [TestMethod]
        public void WhenServiceButtonIsPressedDoNotExpectShowServiceConfirmationEvent()
        {
            SetupAttendantServiceWithoutTouchVbd();

            _attendant.OnServiceButtonPressed();

            _eventBus.Verify(b => b.Publish(It.IsAny<ShowServiceConfirmationEvent>()), Times.Never);
        }

        [TestMethod]
        public void WhenServiceIsRequestedExpectCallAttendantButtonOnEvent()
        {
            SetupAttendantService();

            _attendant.IsServiceRequested = true;

            _eventBus.Verify(b => b.Publish(It.IsAny<CallAttendantButtonOnEvent>()), Times.Once);
        }

        [TestMethod]
        public void WhenGameInitializationIsCompletedExpectRuntimeCall()
        {
            Action<GameInitializationCompletedEvent> callback = null;
            _eventBus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<GameInitializationCompletedEvent>>()))
                .Callback(
                    (object subscriber, Action<GameInitializationCompletedEvent> eventCallback) =>
                    {
                        callback = eventCallback;
                    });

            SetupAttendantService();

            callback.Invoke(new GameInitializationCompletedEvent());
        }

        [TestMethod]
        public void WhenWaitingForPlayerInputStartedExpectRuntimeCall()
        {
            Action<WaitingForPlayerInputStartedEvent> callback = null;
            _eventBus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<WaitingForPlayerInputStartedEvent>>()))
                .Callback(
                    (object subscriber, Action<WaitingForPlayerInputStartedEvent> eventCallback) =>
                    {
                        callback = eventCallback;
                    });

            SetupAttendantServiceWithMediaContent();

            callback.Invoke(new WaitingForPlayerInputStartedEvent());
        }

        [TestMethod]
        public void WhenAttendantTimeoutNotSupportedDoNotStartTimer()
        {
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.AttendantServiceTimeoutSupportEnabled, false))
                .Returns(false);
            SetupAttendantService();
            _attendant.OnServiceButtonPressed();
            _accessor = new DynamicPrivateObject(_attendant);
            Assert.IsNull(_accessor._attendantServiceRequestTimeoutTimer);
        }

        [TestMethod]
        public void WhenAttendantTimeoutIsSupportedStartTimerWhenServiceButtonPressed()
        {
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.AttendantServiceTimeoutSupportEnabled, false))
                .Returns(true);
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.AttendantServiceTimeoutInMilliseconds, It.IsAny<object>()))
                .Returns(10000);
            SetupAttendantService();
            _accessor = new DynamicPrivateObject(_attendant);
            Assert.IsNotNull(_accessor._attendantServiceRequestTimeoutTimer);
            Assert.IsFalse(_accessor._attendantServiceRequestTimeoutTimer.Enabled);

            _attendant.OnServiceButtonPressed();

            Assert.IsTrue(_accessor._attendantServiceRequestTimeoutTimer.Enabled);
        }

        [TestMethod]
        public void WhenAttendantTimeoutIsSupportedStopTimerWhenServiceButtonPressedAgainBeforeTimeout()
        {
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.AttendantServiceTimeoutSupportEnabled, false))
                .Returns(true);
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.AttendantServiceTimeoutInMilliseconds, It.IsAny<object>()))
                .Returns(10000);
            SetupAttendantService();
            _accessor = new DynamicPrivateObject(_attendant);
            Assert.IsNotNull(_accessor._attendantServiceRequestTimeoutTimer);
            Assert.IsFalse(_accessor._attendantServiceRequestTimeoutTimer.Enabled);

            _attendant.OnServiceButtonPressed();

            Assert.IsTrue(_accessor._attendantServiceRequestTimeoutTimer.Enabled);

            _attendant.OnServiceButtonPressed();

            Assert.IsFalse(_accessor._attendantServiceRequestTimeoutTimer.Enabled);
        }

        [TestMethod]
        public void WhenAttendantTimeoutIsSupportedResetAttendantServiceOnTimeoutExpiry()
        {
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.AttendantServiceTimeoutSupportEnabled, false))
                .Returns(true);
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.AttendantServiceTimeoutInMilliseconds, It.IsAny<object>()))
                .Returns(10000);
            SetupAttendantService();
            _accessor = new DynamicPrivateObject(_attendant);
            Assert.IsNotNull(_accessor._attendantServiceRequestTimeoutTimer);
            Assert.IsFalse(_accessor._attendantServiceRequestTimeoutTimer.Enabled);

            _attendant.OnServiceButtonPressed();

            Assert.IsTrue(_attendant.IsServiceRequested);
            Assert.IsTrue(_accessor._attendantServiceRequestTimeoutTimer.Enabled);

            //Invoke timeout callback
            _accessor.ClearAttendantServiceRequest(null, null);

            Assert.IsFalse(_accessor._attendantServiceRequestTimeoutTimer.Enabled);
            Assert.IsFalse(_attendant.IsServiceRequested);
        }

        [TestMethod]
        public void WhenAttendantTimeoutIsSupportedStartTimerWhenServiceIsRequestedByPropertyChange()
        {
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.AttendantServiceTimeoutSupportEnabled, false))
                .Returns(true);
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.AttendantServiceTimeoutInMilliseconds, It.IsAny<object>()))
                .Returns(10000);
            SetupAttendantService();
            _accessor = new DynamicPrivateObject(_attendant);
            Assert.IsNotNull(_accessor._attendantServiceRequestTimeoutTimer);
            Assert.IsFalse(_accessor._attendantServiceRequestTimeoutTimer.Enabled);
            Assert.IsFalse(_accessor.IsServiceRequested);

            _accessor.IsServiceRequested = true;

            Assert.IsTrue(_accessor._attendantServiceRequestTimeoutTimer.Enabled);
        }

        [TestMethod]
        public void WhenAttendantTimeoutIsSupportedStopTimerWhenServiceIsStoppedByPropertyChange()
        {
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.AttendantServiceTimeoutSupportEnabled, false))
                .Returns(true);
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.AttendantServiceTimeoutInMilliseconds, It.IsAny<object>()))
                .Returns(10000);
            SetupAttendantService();
            _accessor = new DynamicPrivateObject(_attendant);
            Assert.IsNotNull(_accessor._attendantServiceRequestTimeoutTimer);
            Assert.IsFalse(_accessor._attendantServiceRequestTimeoutTimer.Enabled);

            _accessor.IsServiceRequested = true;

            Assert.IsTrue(_accessor._attendantServiceRequestTimeoutTimer.Enabled);

            _accessor.IsServiceRequested = false;

            Assert.IsFalse(_accessor._attendantServiceRequestTimeoutTimer.Enabled);
        }

        [TestMethod]
        public void WhenAttendantTimeoutIsSupportedStopTimerWhenServiceIsStoppedByPropertyChangeBeforeTimeout()
        {
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.AttendantServiceTimeoutSupportEnabled, false))
                .Returns(true);
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.AttendantServiceTimeoutInMilliseconds, It.IsAny<object>()))
                .Returns(10000);
            SetupAttendantService();
            _accessor = new DynamicPrivateObject(_attendant);
            Assert.IsNotNull(_accessor._attendantServiceRequestTimeoutTimer);
            Assert.IsFalse(_accessor._attendantServiceRequestTimeoutTimer.Enabled);

            _accessor.IsServiceRequested = true;

            Assert.IsTrue(_accessor._attendantServiceRequestTimeoutTimer.Enabled);

            _accessor.IsServiceRequested = false;

            Assert.IsFalse(_accessor._attendantServiceRequestTimeoutTimer.Enabled);
        }

        private void SetupAttendantService()
        {
            _attendant.Initialize();
            _attendant.IsMediaContentUsed = false;
        }

        private void SetupAttendantServiceWithTouchVbd()
        {
            _hardwareHelper.Setup(h => h.CheckForVirtualButtonDeckHardware()).Returns(true);
            _cabinetDetectionService.Setup(h => h.IsTouchVbd()).Returns(true);
            _attendant.Initialize();
            _attendant.IsMediaContentUsed = false;
        }

        private void SetupAttendantServiceWithoutTouchVbd()
        {
            _hardwareHelper.Setup(h => h.CheckForVirtualButtonDeckHardware()).Returns(true);
            _cabinetDetectionService.Setup(h => h.IsTouchVbd()).Returns(false);
            _attendant.Initialize();
            _attendant.IsMediaContentUsed = false;
        }

        private AttendantService CreateTarget(
            bool nullEvent = false,
            bool nullResize = false,
            bool nullFlags = false,
            bool nullRuntime = false,
            bool nullCabinet = false,
            bool nullProperty = false)
        {
            return new AttendantService(
                nullEvent ? null : _eventBus.Object,
                nullResize ? null : _resizeManager.Object,
                nullFlags ? null : _runtimeFlags.Object,
                nullRuntime ? null : _runtime.Object,
                nullCabinet ? null : _cabinetDetectionService.Object,
                nullProperty ? null : _propertiesManager.Object);
        }

        private void SetupAttendantServiceWithMediaContent()
        {
            _attendant.Initialize();
            _attendant.IsMediaContentUsed = true;
        }
    }
}
