namespace Aristocrat.Monaco.Application.Tests.Monitors
{
    using System;
    using Application.Monitors;
    using Contracts;
    using Contracts.Drm;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SmartCardMonitorTest
    {
        private Mock<IEventBus> _bus;
        private Mock<IDigitalRights> _digitalRights;
        private Mock<IMeter> _errocCountMeter;
        private Mock<IMeterManager> _meters;

        [TestInitialize]
        public void Initialize()
        {
            _bus = new Mock<IEventBus>();
            _meters = new Mock<IMeterManager>();
            _digitalRights = new Mock<IDigitalRights>();
            _errocCountMeter = new Mock<IMeter>();

            _meters.Setup(m => m.GetMeter(ApplicationMeters.SmartCardErrorCount)).Returns(_errocCountMeter.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEventBusExpectException()
        {
            var target = new SmartCardMonitor(null, null, null);

            Assert.IsNull(target);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullMeterMangerExpectException()
        {
            var target = new SmartCardMonitor(_bus.Object, null, null);

            Assert.IsNull(target);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullDigitalRightExpectException()
        {
            var target = new SmartCardMonitor(_bus.Object, _meters.Object, null);

            Assert.IsNull(target);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var target = new SmartCardMonitor(_bus.Object, _meters.Object, _digitalRights.Object);

            _bus.Verify(m => m.Subscribe(target, It.IsAny<Action<SoftwareProtectionModuleDisconnectedEvent>>()));
            _bus.Verify(m => m.Subscribe(target, It.IsAny<Action<SoftwareProtectionModuleErrorEvent>>()));

            _meters.Verify(m => m.GetMeter(ApplicationMeters.SmartCardErrorCount), Times.Never);
        }

        [TestMethod]
        public void WhenDigitalRightsDisabledExpectMeterIncrement()
        {
            _digitalRights.SetupGet(m => m.Disabled).Returns(true);

            var _ = new SmartCardMonitor(_bus.Object, _meters.Object, _digitalRights.Object);

            _errocCountMeter.Verify(m => m.Increment(1), Times.Once);
        }

        [TestMethod]
        public void WhenPostModuleDisconnectExpectMeterIncrement()
        {
            TestEvent(new SoftwareProtectionModuleDisconnectedEvent("test"));
        }

        [TestMethod]
        public void WhenPostModuleErrorExpectMeterIncrement()
        {
            TestEvent(new SoftwareProtectionModuleErrorEvent("test"));
        }

        private void TestEvent<T>(T @event)
            where T : IEvent
        {
            Action<T> callback = null;

            _bus.Setup(
                    m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<T>>()))
                .Callback<object, Action<T>>((o, c) => callback = c);

            var _ = new SmartCardMonitor(_bus.Object, _meters.Object, _digitalRights.Object);

            Assert.IsNotNull(callback);

            callback(@event);

            _errocCountMeter.Verify(m => m.Increment(1), Times.Once);
        }
    }
}