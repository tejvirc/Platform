namespace Aristocrat.Monaco.Application.Tests.Monitors
{
    using System;
    using Application.Monitors;
    using Kernel.Contracts.MessageDisplay;
    using Contracts;
    using Hardware.Contracts.Battery;
    using Hardware.Contracts.Door;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     Test class for unit testing the BatteryMonitor class
    /// </summary>
    [TestClass]
    public class BatteryMonitorTest
    {
        private BatteryMonitor _target;

        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IMessageDisplay> _messageDisplay;
        private Mock<IBattery> _batteryTestService;
        private Mock<IMeterManager> _meterManager;
        private Mock<IEventBus> _eventBus;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(m => m.SetProperty(It.IsAny<string>(), It.IsAny<bool>()));

            _messageDisplay = MoqServiceManager.CreateAndAddService<IMessageDisplay>(MockBehavior.Strict);
            _batteryTestService = MoqServiceManager.CreateAndAddService<IBattery>(MockBehavior.Strict);
            _meterManager = MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Strict);

            _meterManager.Setup(m => m.GetMeter(It.IsAny<string>()).Increment(1));
            _messageDisplay.Setup(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()));
            _messageDisplay.Setup(m => m.RemoveMessage(It.IsAny<Guid>()));

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _eventBus.Setup(m => m.Publish(It.IsAny<BatteryLowEvent>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<OpenEvent>>()));

            _target = new BatteryMonitor();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void NameTest()
        {
            Assert.AreEqual("BatteryMonitor", _target.Name);
            Assert.AreEqual(typeof(BatteryMonitor).Name, _target.Name);
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            Assert.IsTrue(_target.ServiceTypes.Contains(typeof(BatteryMonitor)));
            Assert.AreEqual(1, _target.ServiceTypes.Count);
        }

        [TestMethod]
        public void BatteriesChangedToLow()
        {
            _batteryTestService.Setup(m => m.Test()).Returns((false, false));
            _propertiesManager.Setup(m => m.GetProperty(It.IsAny<string>(), It.IsAny<bool>())).Returns(true);

            _target.Initialize();

            _meterManager.Verify(m => m.GetMeter(It.IsAny<string>()).Increment(1), Times.Exactly(2));
            _messageDisplay.Verify(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()), Times.Exactly(2));
            _messageDisplay.Verify(m => m.RemoveMessage(It.IsAny<Guid>()), Times.Never());
            _eventBus.Verify(m => m.Publish(It.IsAny<BatteryLowEvent>()), Times.Exactly(2));
        }

        [TestMethod]
        public void BatteriesChangedToHigh()
        {
            _batteryTestService.Setup(m => m.Test()).Returns((true, true));
            _propertiesManager.Setup(m => m.GetProperty(It.IsAny<string>(), It.IsAny<bool>())).Returns(false);

            _target.Initialize();

            _meterManager.Verify(m => m.GetMeter(It.IsAny<string>()).Increment(1), Times.Never());
            _messageDisplay.Verify(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()), Times.Never());
            _messageDisplay.Verify(m => m.RemoveMessage(It.IsAny<Guid>()), Times.Exactly(2));
            _eventBus.Verify(m => m.Publish(It.IsAny<BatteryLowEvent>()), Times.Never());
        }

        [TestMethod]
        public void BatteriesStayHigh()
        {
            _batteryTestService.Setup(m => m.Test()).Returns((true, true));
            _propertiesManager.Setup(m => m.GetProperty(It.IsAny<string>(), It.IsAny<bool>())).Returns(true);

            _target.Initialize();

            _meterManager.Verify(m => m.GetMeter(It.IsAny<string>()).Increment(1), Times.Never());
            _messageDisplay.Verify(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()), Times.Never());
            _messageDisplay.Verify(m => m.RemoveMessage(It.IsAny<Guid>()), Times.Exactly(2));
            _eventBus.Verify(m => m.Publish(It.IsAny<BatteryLowEvent>()), Times.Never());
        }

        [TestMethod]
        public void BatteriesStayLow()
        {
            _batteryTestService.Setup(m => m.Test()).Returns((false, false));
            _propertiesManager.Setup(m => m.GetProperty(It.IsAny<string>(), It.IsAny<bool>())).Returns(false);

            _target.Initialize();

            _meterManager.Verify(m => m.GetMeter(It.IsAny<string>()).Increment(1), Times.Never());
            _messageDisplay.Verify(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()), Times.Exactly(2));
            _messageDisplay.Verify(m => m.RemoveMessage(It.IsAny<Guid>()), Times.Never());
            _eventBus.Verify(m => m.Publish(It.IsAny<BatteryLowEvent>()), Times.Exactly(2));
        }
    }
}