namespace Aristocrat.Monaco.Application.Tests
{
    using Contracts;
    using Contracts.OperatorMenu;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.KeySwitch;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Test.Common;
    using Vgt.Client12.Application.OperatorMenu;

    /// <summary>
    ///     This is a test class for OperatorMenuLauncher and is intended
    ///     to contain all OperatorMenuLauncher Unit Tests
    /// </summary>
    [TestClass]
    public class OperatorMenuLauncherTest
    {
        private dynamic _accessor;
        private Mock<ISystemDisableManager> _disableManager;
        private Mock<IDoorService> _door;
        private Mock<IEventBus> _eventBus;
        private readonly AutoResetEvent _waiter = new AutoResetEvent(false);

        /// <summary>
        ///     A green key turn event used by this unit test.
        /// </summary>
        private OnEvent _operatorKeyOnEvent;
        private Mock<IMeterManager> _meterManager;
        private Dictionary<string, Mock<IMeter>> _meterMocks;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IOperatorMenuConfiguration> _operatorMenuConfig;

        private OperatorMenuLauncher _target;
        private int keyEnablerCounts = 0;

        private Action<OverlayMenuEnteredEvent> _menuEntered = null;
        private Action<OverlayMenuExitedEvent> _menuExited = null;

        /// <summary>
        ///     Sets up the ServiceManager singleton and a test IEventBus implementation.
        /// </summary>
        [TestInitialize]
        public void TestInitialization()
        {
            var workingDir = Directory.GetCurrentDirectory();

            // get rid of extra addin files that are copied during a build
            File.Delete(Path.Combine(workingDir, "Aristocrat.Monaco.Application.addin.xml"));

            AddinManager.Initialize(Directory.GetCurrentDirectory());
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _door = MoqServiceManager.CreateAndAddService<IDoorService>(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _disableManager = MoqServiceManager.CreateAndAddService<ISystemDisableManager>(MockBehavior.Strict);
            _meterManager = MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Strict);
            _operatorMenuConfig = MoqServiceManager.CreateAndAddService<IOperatorMenuConfiguration>(MockBehavior.Strict);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OccurrenceMeterRolloverText, It.IsAny<int>()))
                .Returns(100L);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMeterRolloverText, It.IsAny<long>()))
                .Returns(100L);

            var meterNames = new List<string> { "AdministratorAccess", "TechnicianAccess" };
            _meterMocks = new Dictionary<string, Mock<IMeter>>();
            foreach (var meterName in meterNames)
            {
                _meterMocks[meterName] = new Mock<IMeter>(MockBehavior.Strict);
                _meterMocks[meterName].Setup(mock => mock.Lifetime).Returns(0).Verifiable();
                _meterMocks[meterName].Setup(mock => mock.Period).Returns(0).Verifiable();
                _meterMocks[meterName].Setup(mock => mock.Classification).Returns(new OccurrenceMeterClassification());
                _meterMocks[meterName].Setup(mock => mock.Increment(1));
                _meterManager.Setup(mock => mock.IsMeterProvided(meterName)).Returns(true);
                _meterManager.Setup(mock => mock.GetMeter(meterName)).Returns(_meterMocks[meterName].Object);
            }

            var access = MoqServiceManager.CreateAndAddService<IOperatorMenuAccess>(MockBehavior.Strict);
            access.Setup(m => m.TechnicianMode).Returns(It.IsAny<bool>());

            _disableManager.Setup(x => x.CurrentDisableKeys).Returns(new Guid[] { });
            _target = new OperatorMenuLauncher();
            _accessor = new DynamicPrivateObject(_target);
            keyEnablerCounts = _accessor._keyEnablers.Count;
            _operatorKeyOnEvent = new OnEvent((int)_accessor.OperatorKeySwitch1LogicalId, String.Empty);
            _propertiesManager.Setup(m => m.GetProperty("Mono.SelectedAddinConfigurationHashCode", null)).Returns(null);

            _eventBus.Setup(m => m.Subscribe(It.IsAny<OperatorMenuLauncher>(), It.IsAny<Action<OnEvent>>()));

            _eventBus.Setup(m => m.Subscribe(It.IsAny<OperatorMenuLauncher>(), It.IsAny<Action<OverlayMenuEnteredEvent>>()))
                .Callback<object, Action<OverlayMenuEnteredEvent>>(
                    (tar, act) =>
                    {
                        _menuEntered = act;
                    });

            _eventBus.Setup(m => m.Subscribe(It.IsAny<OperatorMenuLauncher>(), It.IsAny<Action<OverlayMenuExitedEvent>>()))
                .Callback<object, Action<OverlayMenuExitedEvent>>(
                    (tar, act) =>
                    {
                        _menuExited = act;
                    });
        }

        /// <summary>
        ///     Releases resources used by the test
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            MoqServiceManager.RemoveInstance();
            try
            {
                AddinManager.Shutdown();
            }
            catch (InvalidOperationException)
            {
                // temporarily swallow exception
            }
        }

        [TestMethod]
        public void ServiceTypeTest()
        {
            Assert.AreEqual(1, _target.ServiceTypes.Count);
            Assert.AreEqual(typeof(IOperatorMenuLauncher), _target.ServiceTypes.ToList()[0]);
        }

        [TestMethod]
        public void NameTest()
        {
            Assert.AreEqual("Operator Menu Launcher", _accessor.Name);
        }

        [TestMethod]
        public void RolePropertyKeyTest()
        {
            Assert.IsFalse(string.IsNullOrEmpty(ApplicationConstants.RolePropertyKey));
        }

        [TestMethod]
        public void IsShowingTest()
        {
            Assert.IsFalse(_accessor.IsShowing);

            _accessor.IsShowing = true;
            Assert.IsTrue(_accessor.IsShowing);
        }

        [TestMethod]
        public void ShowWhenNotShowingOperatorMenuTest()
        {
            var testOperatorMenu = new TestOperatorMenu();
            _accessor._operatorMenu = testOperatorMenu;

            _door.Setup(m => m.LogicalDoors).Returns(new Dictionary<int, LogicalDoor>());
            _propertiesManager.Setup(m => m.SetProperty(It.IsAny<string>(), ApplicationConstants.DefaultRole)).Verifiable();
            _disableManager.Setup(m => m.Disable(It.IsAny<Guid>(), SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), null))
                .Verifiable();

            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuEnteredEvent>()))
                .Callback(() => _waiter.Set());

            _target.EnableKey(ApplicationConstants.OperatorMenuInitializationKey);
            _target.Show();
            _waiter.WaitOne(3000);

            Assert.AreEqual(1, testOperatorMenu.ShowCount);
            Assert.IsTrue(_target.IsShowing);
            _disableManager.Verify();
        }

        [TestMethod]
        public void ShowWhenAlreadyShowingOperatorMenuTest()
        {
            var testOperatorMenu = new TestOperatorMenu();
            _accessor._operatorMenu = testOperatorMenu;

            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuEnteredEvent>()))
                .Callback(() => _waiter.Set());

            _propertiesManager.Setup(m => m.SetProperty(It.IsAny<string>(), "Administrator")).Verifiable();

            _disableManager.Setup(m => m.Disable(It.IsAny<Guid>(), SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), null))
                .Verifiable();

            _target.EnableKey(ApplicationConstants.OperatorMenuInitializationKey);
            _target.Show();
            _waiter.WaitOne(3000);
            _target.Show();
            Thread.Sleep(500);

            Assert.AreEqual(1, testOperatorMenu.ShowCount);
            Assert.IsTrue(_target.IsShowing);
        }

        [TestMethod]
        public void ShowWithMainDoorOpenTest()
        {
            var testOperatorMenu = new TestOperatorMenu();
            _accessor._operatorMenu = testOperatorMenu;

            _door.Setup(m => m.LogicalDoors).Returns(
                new Dictionary<int, LogicalDoor>
                {
                    { (int)DoorLogicalId.Main, new LogicalDoor { Closed = false } },
                    { (int)DoorLogicalId.Logic, new LogicalDoor { Closed = false } },
                });
            _propertiesManager.Setup(m => m.SetProperty(It.IsAny<string>(), "Administrator")).Verifiable();
            //_propertiesManager.Setup(m => m.SetProperty(It.IsAny<string>(), ApplicationConstants.TechnicianRole)).Verifiable();
            _disableManager.Setup(m => m.Disable(It.IsAny<Guid>(), SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), null))
                .Verifiable();

            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuEnteredEvent>()))
                .Callback(() => _waiter.Set());

            _target.EnableKey(ApplicationConstants.OperatorMenuInitializationKey);
            _target.Show();
            _waiter.WaitOne(3000);

            Assert.AreEqual(1, testOperatorMenu.ShowCount);
            Assert.IsTrue(_target.IsShowing);
            _disableManager.Verify();
            _propertiesManager.Verify();
        }

        [TestMethod]
        public void ShowWithMainDoorClosedTest()
        {
            var testOperatorMenu = new TestOperatorMenu();
            _accessor._operatorMenu = testOperatorMenu;

            _door.Setup(m => m.LogicalDoors).Returns(
                new Dictionary<int, LogicalDoor>
                {
                    { (int)DoorLogicalId.Logic, new LogicalDoor { Closed = false } },
                });
            _propertiesManager.Setup(m => m.SetProperty(It.IsAny<string>(), ApplicationConstants.DefaultRole)).Verifiable();
            _disableManager.Setup(m => m.Disable(It.IsAny<Guid>(), SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), null))
                .Verifiable();

            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuEnteredEvent>()))
                .Callback(() => _waiter.Set());

            _target.EnableKey(ApplicationConstants.OperatorMenuInitializationKey);
            _target.Show();
            _waiter.WaitOne(3000);

            Assert.AreEqual(1, testOperatorMenu.ShowCount);
            Assert.IsTrue(_target.IsShowing);
            _disableManager.Verify();
            _propertiesManager.Verify();
        }

        [TestMethod]
        public void CloseTestWhenNotShowing()
        {
            var testOperatorMenu = new TestOperatorMenu();
            _accessor._operatorMenu = testOperatorMenu;
            _accessor.IsShowing = false;

            _accessor.Close();

            Assert.AreEqual(0, testOperatorMenu.CloseCount);
            Assert.IsFalse(_target.IsShowing);
        }

        [TestMethod]
        [Ignore]
        public void AccessOperatorMenu_OverlayIsOpen_OperatorMenuDoesNotOpen()
        {
            var testOperatorMenu = new TestOperatorMenu();
            _accessor._operatorMenu = testOperatorMenu;

            _target.Initialize();

            _propertiesManager.Setup(m => m.SetProperty(It.IsAny<string>(), ApplicationConstants.DefaultRole)).Verifiable();
            _disableManager.Setup(m => m.Disable(It.IsAny<Guid>(), SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), null))
                .Verifiable();

            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuEnteredEvent>()))
                .Callback(() => _waiter.Set());

            _target.EnableKey(ApplicationConstants.OperatorMenuInitializationKey);

            // Open the overlay menu
            _menuEntered(new OverlayMenuEnteredEvent());

            // Should not be able to enter audit menu
            _accessor.HandleEvent(new OnEvent(_accessor.OperatorKeySwitch1LogicalId, string.Empty));
            _waiter.WaitOne(3000);

            Assert.IsFalse(_target.IsShowing);

            // Close the overlay menu
            _menuExited(new OverlayMenuExitedEvent());

            // Should be able to enter audit menu
            _accessor.HandleEvent(new OnEvent(_accessor.OperatorKeySwitch1LogicalId, string.Empty));
            _waiter.WaitOne(3000);

            Assert.IsTrue(_target.IsShowing);
        }

        [TestMethod]
        public void CloseTestWhenShowing()
        {
            var testOperatorMenu = new TestOperatorMenu();
            _accessor._operatorMenu = testOperatorMenu;

            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuExitingEvent>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuExitedEvent>()));

            _propertiesManager.Setup(m => m.SetProperty("OperatorMenu.Role", ApplicationConstants.DefaultRole)).Verifiable();

            _door.Setup(m => m.LogicalDoors).Returns(
                new Dictionary<int, LogicalDoor>
                {
                    { 1, new LogicalDoor { Closed = false } }
                });
            _propertiesManager.Setup(m => m.SetProperty(It.IsAny<string>(), ApplicationConstants.TechnicianRole)).Verifiable();
            _disableManager.Setup(m => m.Disable(It.IsAny<Guid>(), SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), null))
                .Verifiable();
            _disableManager.Setup(m => m.Enable(It.IsAny<Guid>())).Verifiable();

            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuEnteredEvent>()))
                .Callback(() => _waiter.Set());

            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuExitedEvent>()))
                .Callback(() => _waiter.Set());

            _target.EnableKey(ApplicationConstants.OperatorMenuInitializationKey);
            _target.Show();
            _waiter.WaitOne(3000);
            _target.Close();
            _waiter.WaitOne(3000);
            Assert.IsFalse(_target.IsShowing);
            _disableManager.Verify();
        }

        [TestMethod]
        public void ActivateTest()
        {
            var fakeWindow = new TestOperatorMenu();
            _accessor._operatorMenu = fakeWindow;

            // test when IsShowing is false
            _accessor.IsShowing = false;
            _target.Activate();

            Assert.AreEqual(0, fakeWindow.ActivateCount);

            // test when IsShowing is true so Activate() is called on the window
            _accessor.IsShowing = true;
            _target.Activate();

            Assert.AreEqual(1, fakeWindow.ActivateCount);
        }

        [TestMethod]
        public void HandleEventTestWhenDisabled()
        {
            var testOperatorMenu = new TestOperatorMenu();
            _accessor._operatorMenu = testOperatorMenu;

            // Disable
            var key = Guid.NewGuid();
            _target.DisableKey(key);

            _accessor.HandleEvent(_operatorKeyOnEvent);

            Assert.IsFalse(_target.IsShowing);
            Assert.AreEqual(1 + keyEnablerCounts, _accessor._keyEnablers.Count);
            Assert.IsTrue(_accessor._keyEnablers.ContainsKey(key));
        }

        [TestMethod]
        public void HandleEventTestWhenDisabledUsingSecondLogicalId()
        {
            var testOperatorMenu = new TestOperatorMenu();
            _accessor._operatorMenu = testOperatorMenu;

            // Disable
            var key = Guid.NewGuid();
            _target.DisableKey(key);

////            _accessor.HandleEvent(new KeyTurnedEvent((int)_target.GetType().GetField("OperatorKeySwitch2LogicalId", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Static).GetValue(null)));
            _accessor.HandleEvent(new OnEvent(_accessor.OperatorKeySwitch2LogicalId, string.Empty));
            Assert.IsFalse(_target.IsShowing);
            Assert.AreEqual(1 + keyEnablerCounts, _accessor._keyEnablers.Count);
            Assert.IsTrue(_accessor._keyEnablers.ContainsKey(key));
        }

        [TestMethod]
        public void HandleEventTestWhenNotShowing()
        {
            var testOperatorMenu = new TestOperatorMenu();
            _accessor._operatorMenu = testOperatorMenu;

            _door.Setup(m => m.LogicalDoors).Returns(new Dictionary<int, LogicalDoor>());
            _propertiesManager.Setup(m => m.SetProperty(It.IsAny<string>(), ApplicationConstants.DefaultRole))
                .Verifiable();
            _disableManager.Setup(m => m.Disable(It.IsAny<Guid>(), SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), null))
                .Verifiable();

            _accessor.HandleEvent(_operatorKeyOnEvent);

            Assert.IsFalse(_target.IsShowing);
            //_propertiesManager.Verify();
            //_disableManager.Verify();
        }

        [TestMethod]
        public void EnableKeyWhenPreviouslyDisabledTest()
        {
            var key = Guid.NewGuid();
            _target.DisableKey(key);
            _target.EnableKey(key);

            Assert.AreEqual(keyEnablerCounts, _accessor._keyEnablers.Count);
        }

        [TestMethod]
        public void EnableKeyTestWhenEnabled()
        {
            _target.EnableKey(Guid.NewGuid());

            Assert.AreEqual(keyEnablerCounts, _accessor._keyEnablers.Count);
        }

        [TestMethod]
        public void EnableKeyTestWithRemainingEnablers()
        {
            var key = Guid.NewGuid();
            _target.DisableKey(key);
            var key2 = Guid.NewGuid();
            _target.DisableKey(key2);
            _target.EnableKey(key);

            Assert.AreEqual(1 + keyEnablerCounts, _accessor._keyEnablers.Count);
            Assert.IsTrue(_accessor._keyEnablers.ContainsKey(key2));
        }

        [TestMethod]
        public void DisableKeyTestWhenEnabled()
        {
            var key = Guid.NewGuid();
            _target.DisableKey(key);

            Assert.AreEqual(1 + keyEnablerCounts, _accessor._keyEnablers.Count);
            Assert.IsTrue(_accessor._keyEnablers.ContainsKey(key));
        }

        [TestMethod]
        public void DisableKeyTestForExistingEnabler()
        {
            var key = Guid.NewGuid();
            _target.DisableKey(key);
            _target.DisableKey(key);

            Assert.AreEqual(1 + keyEnablerCounts, _accessor._keyEnablers.Count);
            Assert.IsTrue(_accessor._keyEnablers.ContainsKey(key));
        }

        [TestMethod]
        public void DisableKeyTestForSecondEnabler()
        {
            var key = Guid.NewGuid();
            _target.DisableKey(key);
            var key2 = Guid.NewGuid();
            _accessor.DisableKey(key2);

            Assert.AreEqual(2 + keyEnablerCounts, _accessor._keyEnablers.Count);
            Assert.IsTrue(_accessor._keyEnablers.ContainsKey(key2));
            Assert.IsTrue(_accessor._keyEnablers.ContainsKey(key));
        }

        [TestMethod]
        public void IsOperatorKeyDisabledTest()
        {
            if (_accessor._keyEnablers.Count > 0)
            {
                Assert.IsTrue(_target.IsOperatorKeyDisabled);
            }
            else
            {
                Assert.IsFalse(_target.IsOperatorKeyDisabled);
            }
        }

        [TestMethod]
        public void RapidSwitchTest()
        {
            var testOperatorMenu = new TestOperatorMenu();
            _accessor._operatorMenu = testOperatorMenu;
            
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuEnteredEvent>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuExitingEvent>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuExitedEvent>()))
                .Callback(() => _waiter.Set());


            _disableManager.Setup(m => m.Disable(It.IsAny<Guid>(), SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), null))
                .Verifiable();
            _disableManager.Setup(m => m.Enable(It.IsAny<Guid>())).Verifiable();

            _target.EnableKey(ApplicationConstants.OperatorMenuInitializationKey);
            _target.Show();
            _target.Close();
            _waiter.WaitOne(3000);

            Assert.IsFalse(_target.IsShowing);
        }
    }
}
