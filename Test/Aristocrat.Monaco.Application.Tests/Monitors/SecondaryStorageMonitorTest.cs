namespace Aristocrat.Monaco.Application.Tests.Monitors
{
    using System;
    using System.Collections.Generic;
    using Application.Monitors;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     Test class for unit testing the SecondaryStorageMonitor class
    /// </summary>
    [TestClass]
    public class SecondaryStorageMonitorTest
    {
        private SecondaryStorageMonitor _target;

        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IEventBus> _eventBus;
        private Mock<ISecondaryStorageManager> _secondaryStorageManager;

        private Action<DeviceDisconnectedEvent> _deviceDisconnectedEvent;

        private dynamic _accessor;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(m => m.GetProperty(It.IsAny<string>(), It.IsAny<object>()))
                .Returns<string, object>((s, o) => o);

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            
            _secondaryStorageManager =
                MoqServiceManager.CreateAndAddService<ISecondaryStorageManager>(MockBehavior.Strict);
            _secondaryStorageManager.Setup(s => s.VerifyConfiguration()).Verifiable();

            _target = new SecondaryStorageMonitor();
            _accessor = new DynamicPrivateObject(_target);

            _eventBus.Setup(x => x.UnsubscribeAll(_target));
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
            _target.Dispose();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void NameTest()
        {
            Assert.AreEqual(typeof(SecondaryStorageMonitor).FullName, _target.Name);
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            Assert.IsTrue(_target.ServiceTypes.Contains(typeof(SecondaryStorageMonitor)));
            Assert.AreEqual(1, _target.ServiceTypes.Count);
        }

        [TestMethod]
        public void WhenSecondaryStorageMonitorInitializedSecondaryStorageRequirementVerified()
        {
            _target.Initialize();

            _secondaryStorageManager.Verify();
        }

        [TestMethod]
        public void WhenSecondaryStorageRequiredEventsAreSubscribed()
        {
            SetupSecondaryStorageRequired();

            _target.Initialize();

            _secondaryStorageManager.Verify();
            _eventBus.Verify();
        }

        [TestMethod]
        public void WhenSecondaryStorageNotRequiredEventsNotSubscribed()
        {
            SetupSecondaryStorageNotRequired();

            _target.Initialize();

            _secondaryStorageManager.Verify();
        }

        [TestMethod]
        public void WhenSecondaryStorageRequiredStorageRemovedEventsAreProcessed()
        {
            SetupSecondaryStorageRequired();

            Assert.IsFalse(_accessor._storageDeviceRemoved);

            _target.Initialize();

            _deviceDisconnectedEvent.Invoke(new DeviceDisconnectedEvent(new Dictionary<string, object>()));

            Assert.IsTrue(_accessor._storageDeviceRemoved);

            _secondaryStorageManager.Verify();
            _eventBus.Verify();
        }

        private void SetupSecondaryStorageRequired()
        {
            _propertiesManager
                .Setup(m => m.GetProperty(SecondaryStorageConstants.SecondaryStorageRequired, It.IsAny<bool>()))
                .Returns(true);
            _eventBus.Setup(
                    x => x.Subscribe(
                        _target,
                        It.IsAny<Action<DeviceDisconnectedEvent>>(),
                        It.IsAny<Predicate<DeviceDisconnectedEvent>>()))
                .Callback<object, Action<DeviceDisconnectedEvent>, Predicate<DeviceDisconnectedEvent>>(
                    (dm, handler, filter) =>
                    {
                        _deviceDisconnectedEvent = handler;
                        ValidateFilter(filter, x => new DeviceDisconnectedEvent(x));
                    }).Verifiable();
        }

        private void SetupSecondaryStorageNotRequired()
        {
            _propertiesManager
                .Setup(m => m.GetProperty(SecondaryStorageConstants.SecondaryStorageRequired, It.IsAny<bool>()))
                .Returns(false);
            _eventBus.Setup(
                    e => e.Subscribe(It.IsAny<SecondaryStorageMonitor>(), It.IsAny<Action<DeviceDisconnectedEvent>>()))
                .Throws(new Exception("Shouldn't subscribe to events"));
        }

        private static void ValidateFilter<T>(Predicate<T> filter, Func<IDictionary<string, object>, T> factory)
            where T : BaseDeviceEvent
        {
            var descriptions = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { { "DeviceCategory", "STORAGE" } },
                new Dictionary<string, object> { { "DeviceCategory", "USB" } }
            };
            descriptions.ForEach(
                x =>
                {
                    var mock = factory(x);
                    Assert.AreEqual(
                        mock.DeviceCategory == "STORAGE",
                        filter.Invoke(mock));
                });
        }
    }
}