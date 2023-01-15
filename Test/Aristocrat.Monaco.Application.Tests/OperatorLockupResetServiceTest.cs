namespace Aristocrat.Monaco.Application.Tests
{
    using Aristocrat.Monaco.Kernel.Contracts.MessageDisplay;
    using Contracts;
    using Hardware.Contracts.Button;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;
    using Test.Common;

    [TestClass]
    public class OperatorLockupResetServiceTest
    {
        private const string ReasonText = "Dummy";

        private Mock<IEventBus> _bus;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ISystemDisableManager> _systemDisableManager;
        private Action<SystemDisableAddedEvent> _systemDisabledAddedEvent;
        private Action<DownEvent> _jackpotKeyResetEvent;

        private OperatorLockupResetService _target;

        [TestInitialize]
        public void TestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MockLocalization.Setup(MockBehavior.Strict);
            MockLocalization.Localizer.Setup(x => x.GetString(It.IsAny<string>())).Returns(ReasonText);
            _bus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _systemDisableManager = MoqServiceManager.CreateAndAddService<ISystemDisableManager>(MockBehavior.Default);

            _systemDisableManager.SetupGet(m => m.CurrentDisableKeys).Returns(new List<Guid>());

            _propertiesManager.Setup(m => m.GetProperty(It.IsAny<string>(), It.IsAny<object>()))
                .Returns<string, object>((s, o) => o);

            _bus.Setup(
                    b => b.Subscribe(
                        It.IsAny<OperatorLockupResetService>(),
                        It.IsAny<Action<DownEvent>>()))
                .Callback<object, Action<DownEvent>>((subscriber, callback) => _jackpotKeyResetEvent = callback);

            _bus.Setup(
                    b => b.Subscribe(
                        It.IsAny<OperatorLockupResetService>(),
                        It.IsAny<Action<DownEvent>>(),
                        It.IsAny<Predicate<DownEvent>>()))
                .Callback<object, Action<DownEvent>, Predicate<DownEvent>>(
                    (subscriber, callback, filter) => _jackpotKeyResetEvent = callback);

            _bus.Setup(
                    b => b.Subscribe(
                        It.IsAny<OperatorLockupResetService>(),
                        It.IsAny<Action<SystemDisableAddedEvent>>()))
                .Callback<object, Action<SystemDisableAddedEvent>>(
                    (subscriber, callback) => _systemDisabledAddedEvent = callback);

        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void WhenEventBusIsNullExpectException()
        {
            var _ = new OperatorLockupResetService(null, null, null);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void WhenPropertiesManagerIsNullExpectException()
        {
            var _ = new OperatorLockupResetService(_bus.Object, null, null);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void WhenSystemDisableManagerIsNullExpectException()
        {
            var _ = new OperatorLockupResetService(_bus.Object, _propertiesManager.Object, null);
        }

        [TestMethod]
        public void WhenAdditionalLockupSupportIsDisabledEventsAreNotSubscribed()
        {
            _propertiesManager
                .Setup(m => m.GetProperty(ApplicationConstants.OperatorLockupResetEnabled, It.IsAny<bool>()))
                .Returns(false);
            CreateTarget();
            Assert.IsNull(_jackpotKeyResetEvent);
            Assert.IsNull(_systemDisabledAddedEvent);
        }

        [TestMethod]
        public void WhenAdditionalLockupSupportIsEnabledEventsAreSubscribed()
        {
            _propertiesManager
                .Setup(m => m.GetProperty(ApplicationConstants.OperatorLockupResetEnabled, It.IsAny<bool>()))
                .Returns(true);
            CreateTarget();
            Assert.IsNotNull(_jackpotKeyResetEvent);
            Assert.IsNotNull(_systemDisabledAddedEvent);
        }

        [TestMethod]
        public void WhenHardLockupNotRequiresOperatorResetThenOperatorResetLockupIsNotCreated()
        {
            _propertiesManager
                .Setup(m => m.GetProperty(ApplicationConstants.OperatorLockupResetEnabled, It.IsAny<bool>()))
                .Returns(true);

            CreateTarget();

            _systemDisabledAddedEvent(
                new SystemDisableAddedEvent(
                    SystemDisablePriority.Immediate,
                    ApplicationConstants.HandpayPendingDisableKey,
                    ReasonText,
                    true));

            _systemDisableManager.Verify(m => m.Disable(ApplicationConstants.OperatorResetRequiredDisableKey, SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), null), Times.Never);
        }

        [TestMethod]
        public void WhenAllExistingHardLockupsAreRemovedThenOperatorResetLockupCleared()
        {
            _propertiesManager
                .Setup(m => m.GetProperty(ApplicationConstants.OperatorLockupResetEnabled, It.IsAny<bool>()))
                .Returns(true);
            
            CreateTarget();

            _systemDisabledAddedEvent(
                new SystemDisableAddedEvent(
                    SystemDisablePriority.Immediate,
                    ApplicationConstants.HardMeterDisabled,
                    ReasonText,
                    true));
            _systemDisabledAddedEvent(
                new SystemDisableAddedEvent(
                    SystemDisablePriority.Immediate,
                    ApplicationConstants.NoteAcceptorDisconnectedGuid,
                    ReasonText,
                    true));

            // Send jackpot key event
            _jackpotKeyResetEvent(new DownEvent((int)ButtonLogicalId.Button30));

            _jackpotKeyResetEvent(new DownEvent((int)ButtonLogicalId.Button30));

            _jackpotKeyResetEvent(new DownEvent((int)ButtonLogicalId.Button30));

            _systemDisableManager.Verify(m => m.Enable(ApplicationConstants.OperatorResetRequiredDisableKey), Times.Once);
        }

        [TestMethod]
        public void WhenHardLockupsAlreadyExistsThenOperatorResetLockupIsCreatedOnInitialization()
        {
            _systemDisableManager.SetupGet(m => m.CurrentDisableKeys).Returns(new List<Guid> { ApplicationConstants.NoteAcceptorDisconnectedGuid });

            _propertiesManager
                .Setup(m => m.GetProperty(ApplicationConstants.OperatorLockupResetEnabled, It.IsAny<bool>()))
                .Returns(true);

            CreateTarget();

            _systemDisableManager.Verify(m => m.Disable(ApplicationConstants.OperatorResetRequiredDisableKey, SystemDisablePriority.Normal, It.IsAny<string>(), It.IsAny<CultureProviderType>(), It.IsAny<object[]>()), Times.Once);
        }

        [TestMethod]
        public void ExcessiveDocumentRejectNotIncludedInOperatorResetLockupService()
        {
            _propertiesManager
                .Setup(m => m.GetProperty(ApplicationConstants.OperatorLockupResetEnabled, It.IsAny<bool>()))
                .Returns(true);

            CreateTarget();

            _systemDisabledAddedEvent(
                new SystemDisableAddedEvent(
                    SystemDisablePriority.Immediate,
                    ApplicationConstants.ExcessiveDocumentRejectGuid,
                    ReasonText,
                    true));

            _systemDisableManager.Verify(
                m => m.Disable(
                    ApplicationConstants.OperatorResetRequiredDisableKey,
                    SystemDisablePriority.Immediate,
                    It.IsAny<string>(),
                    It.IsAny<CultureProviderType>(),
                    It.IsAny<object[]>()),
                Times.Never);
        }

        [TestMethod]
        public void OperatorLockupResetServiceCountTest()
        {
            //        private readonly List<Guid> _lockupsRequireOperatorReset = new List<Guid>
            _propertiesManager
                .Setup(m => m.GetProperty(ApplicationConstants.OperatorLockupResetEnabled, It.IsAny<bool>()))
                .Returns(true);

            CreateTarget();

            PrivateObject obj = new PrivateObject(_target);
            List<Guid> lockupsRequireOperatorReset = (List < Guid > )obj.GetField("_lockupsRequireOperatorReset");
            Assert.AreEqual(31, lockupsRequireOperatorReset.Count);
        }

        private void CreateTarget()
        {
            _target = new OperatorLockupResetService(_bus.Object, _propertiesManager.Object, _systemDisableManager.Object);

            _target.Initialize();
        }
    }
}
