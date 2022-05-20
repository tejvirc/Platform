namespace Aristocrat.Monaco.Mgam.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using Application.Contracts.Identification;
    using Application.Contracts.Localization;
    using Application.Contracts.TiltLogger;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Monaco.Mgam.Services.Attributes;
    using Aristocrat.Monaco.Mgam.Services.Notification;
    using Common;
    using Hardware.Contracts.Battery;
    using Hardware.Contracts.Door;
    using Kernel;
    using Mgam.Services.Event;
    using Mgam.Services.Lockup;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class LockupTest
    {
        private const string TestMessage = "Test Message";

        private Mock<IEventBus> _eventBus;
        private Mock<ISystemDisableManager> _systemDisable;
        private Mock<ITiltLogger> _tilts;
        private Mock<ILogger<Lockup>> _logger;
        private Mock<IEventDispatcher> _eventDispatcher;
        private Mock<INotificationLift> _notifier;
        private Mock<IAttributeManager> _attributes;
        private Mock<ILocalizerFactory> _localizerFactory;

        private Action<EmployeeLoggedInEvent> _subscriptionToEmployeeLoggedIn;
        private Action<EmployeeLoggedOutEvent> _subscriptionToEmployeeLoggedOut;
        private int _notCode;
        private string _notMsg;

        private Lockup _target;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);

            _subscriptionToEmployeeLoggedIn = null;
            _subscriptionToEmployeeLoggedOut = null;
            _notCode = -1;
            _notMsg = null;
            _logger = new Mock<ILogger<Lockup>>();
            _attributes = new Mock<IAttributeManager>();

            MockEventBus();
            MockSystemDisable();
            MockTilts();
            MockEventDispatcher();
            MockNotifier();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [DataRow(false, true, true, true, true, true, true, DisplayName = "Null Event Bus Object")]
        [DataRow(true, false, true, true, true, true, true, DisplayName = "Null System Disable Manager Object")]
        [DataRow(true, true, false, true, true, true, true, DisplayName = "Null Tilt Logger Object")]
        [DataRow(true, true, true, false, true, true, true, DisplayName = "Null Logger Object")]
        [DataRow(true, true, true, true, true, false, true, DisplayName = "Null Notification Lift Object")]
        [DataRow(true, true, true, true, true, true, false, DisplayName = "Null Attribute Manager Object")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullParameterTest(
            bool eventBus,
            bool disableManager,
            bool tilts,
            bool logger,
            bool eventDispatcher,
            bool notifier,
            bool attributes)
        {
            _target = new Lockup(
                eventBus ? _eventBus.Object : null,
                disableManager ? _systemDisable.Object : null,
                tilts ? _tilts.Object : null,
                logger ? _logger.Object : null,
                eventDispatcher ? _eventDispatcher.Object : null,
                notifier ? _notifier.Object : null,
                attributes ? _attributes.Object : null);
        }

        [TestMethod]
        public void WhenConstructExpectSuccess()
        {
            CreateNewTarget();

            Assert.IsNotNull(_target);
            Assert.IsInstanceOfType(_target, typeof(ILockup));
        }

        [TestMethod]
        public void AddHostLockClearHostLockTest()
        {
            CreateNewTarget();

            _target.AddHostLock(TestMessage);

            _systemDisable.Verify(m => m.Disable(MgamConstants.ProtocolCommandDisableKey, SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), null), Times.Once);

            Task.Delay(100);

            _systemDisable.SetupGet(d => d.CurrentDisableKeys).Returns(new List<Guid> { MgamConstants.ProtocolCommandDisableKey });

            _target.ClearHostLock();

            _systemDisable.Verify(m => m.Enable(MgamConstants.ProtocolCommandDisableKey), Times.Once);
        }

        [TestMethod]
        public void AddLockForEmployeeClearThenClearTest()
        {
            CreateNewTarget();

            _target.LockupForEmployeeCard("test");

            _systemDisable.Verify(m => m.Disable(MgamConstants.NeedEmployeeCardGuid, SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), null), Times.Once);

            Task.Delay(100);

            _systemDisable.SetupGet(d => d.CurrentDisableKeys).Returns(new List<Guid> { MgamConstants.NeedEmployeeCardGuid });

            _subscriptionToEmployeeLoggedIn(new EmployeeLoggedInEvent());

            _systemDisable.Verify(m => m.Enable(MgamConstants.NeedEmployeeCardGuid), Times.Once);
            Assert.IsTrue(_target.IsEmployeeLoggedIn);

            Task.Delay(100);

            _subscriptionToEmployeeLoggedOut(new EmployeeLoggedOutEvent());

            Assert.IsFalse(_target.IsEmployeeLoggedIn);
        }

        [TestMethod]
        public void AddLockForEmployeeClearWhileLoggedInThenClearTest()
        {
            CreateNewTarget();

            _systemDisable.SetupGet(d => d.CurrentDisableKeys).Returns(new List<Guid> { MgamConstants.NeedEmployeeCardGuid });

            _subscriptionToEmployeeLoggedIn(new EmployeeLoggedInEvent());

            Assert.IsTrue(_target.IsEmployeeLoggedIn);
            _systemDisable.Verify(m => m.Enable(MgamConstants.NeedEmployeeCardGuid));

            Task.Delay(100);

            _systemDisable.SetupGet(d => d.CurrentDisableKeys).Returns(new List<Guid>());

            _target.LockupForEmployeeCard();

            _systemDisable.Verify(m => m.Disable(MgamConstants.NeedEmployeeCardGuid, SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), null), Times.Never);

            Task.Delay(100);

            _subscriptionToEmployeeLoggedOut(new EmployeeLoggedOutEvent());

            Assert.IsFalse(_target.IsEmployeeLoggedIn);
        }

        [TestMethod]
        public void AddHardTiltNotifyTest()
        {
            CreateNewTarget();

            _tilts.Raise(e => e.TiltLogAppendedTilt += null,
                _tilts.Object,
                new TiltLogAppendedEventArgs(
                    new EventDescription {  Level = "tilt", AdditionalInfos = new[] { ("TestMessage", TestMessage) } },
                    typeof(OpenEvent)));

            Assert.AreEqual((int)NotificationCode.LockedTilt, _notCode);
            Assert.AreNotEqual(TestMessage, _notMsg);
        }

        [TestMethod]
        public void AddSoftTiltDontNotifyTest()
        {
            CreateNewTarget();

            _tilts.Raise(e => e.TiltLogAppendedTilt += null,
                _tilts.Object,
                new TiltLogAppendedEventArgs(
                    new EventDescription { Level = "info", AdditionalInfos = new[] { ("TestMessage", TestMessage) } },
                    typeof(OpenEvent)));

            Assert.AreNotEqual((int)NotificationCode.LockedTilt, _notCode);
            Assert.AreNotEqual(TestMessage, _notMsg);
        }

        [TestMethod]
        public void AddAlreadyHandledHardTiltDontNotifyTest()
        {
            CreateNewTarget();

            _tilts.Raise(e => e.TiltLogAppendedTilt += null,
                _tilts.Object,
                new TiltLogAppendedEventArgs(
                    new EventDescription { Level = "tilt", AdditionalInfos = new[] { ("TestMessage", TestMessage) } },
                    typeof(BatteryLowEvent)));

            Assert.AreNotEqual((int)NotificationCode.LockedTilt, _notCode);
            Assert.AreNotEqual(TestMessage, _notMsg);
        }

        private void MockEventBus()
        {
            _eventBus = new Mock<IEventBus>();
            _eventBus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<EmployeeLoggedInEvent>>()))
                .Callback<object, Action<EmployeeLoggedInEvent>>((_, callback) => _subscriptionToEmployeeLoggedIn = callback);
            _eventBus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<EmployeeLoggedOutEvent>>()))
                .Callback<object, Action<EmployeeLoggedOutEvent>>((_, callback) => _subscriptionToEmployeeLoggedOut = callback);
        }

        private void MockSystemDisable()
        {
            _systemDisable = new Mock<ISystemDisableManager>();

            _systemDisable.SetupGet(d => d.CurrentDisableKeys).Returns(new List<Guid>());
            _systemDisable.SetupGet(d => d.CurrentImmediateDisableKeys).Returns(new List<Guid>());

            _localizerFactory = MoqServiceManager.CreateAndAddService<ILocalizerFactory>(MockBehavior.Loose);
            _localizerFactory.Setup(m => m.For(It.IsAny<string>())).Returns<string>(
                name =>
                {
                    var localizer = new Mock<ILocalizer>();
                    localizer.Setup(m => m.CurrentCulture).Returns(new CultureInfo("en-US"));
                    localizer.Setup(m => m.GetString(It.IsAny<string>())).Returns<string>(s => s);
                    return localizer.Object;
                });
        }

        private void MockTilts()
        {
            _tilts = MoqServiceManager.CreateAndAddService<ITiltLogger>(MockBehavior.Loose);
        }

        private void MockEventDispatcher()
        {
            _eventDispatcher = new Mock<IEventDispatcher>();
            _eventDispatcher.SetupGet(d => d.ConsumedEventTypes).Returns(new [] { typeof(BatteryLowEvent) });
        }

        private void MockNotifier()
        {
            _notifier = new Mock<INotificationLift>();
            _notifier.Setup(n => n.Notify(It.IsAny<NotificationCode>(), It.IsAny<string>()))
                .Callback<NotificationCode, string>((code, msg) => { _notCode =(int) code; _notMsg = msg; });
        }

        private void CreateNewTarget()
        {
            _target = new Lockup(_eventBus.Object, _systemDisable.Object, _tilts.Object,
                _logger.Object, _eventDispatcher.Object, _notifier.Object, _attributes.Object);
        }
    }
}
