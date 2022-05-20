namespace Aristocrat.Monaco.G2S.Tests
{
    using System;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Common.Events;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class TransportStateObserverTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEventBusIsNullExpectException()
        {
            var observer = new TransportStateObserver(null);

            Assert.IsNull(observer);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            var eventBus = new Mock<IEventBus>();

            var observer = new TransportStateObserver(eventBus.Object);

            Assert.IsNotNull(observer);
        }

        [TestMethod]
        public void WhenNotifyHostUnreachableExpectEvent()
        {
            const int deviceId = 1;

            var eventBus = new Mock<IEventBus>();

            var observer = new TransportStateObserver(eventBus.Object);

            var device = new Mock<ICommunicationsDevice>();
            device.SetupGet(d => d.Id).Returns(deviceId);

            observer.Notify(device.Object, t_transportStates.G2S_hostUnreachable);

            eventBus.Verify(m => m.Publish(It.IsAny<HostUnreachableEvent>()));
        }

        [TestMethod]
        public void WhenNotifyTransportDownExpectEvent()
        {
            const int deviceId = 1;

            var eventBus = new Mock<IEventBus>();

            var observer = new TransportStateObserver(eventBus.Object);

            var device = new Mock<ICommunicationsDevice>();
            device.SetupGet(d => d.Id).Returns(deviceId);

            observer.Notify(device.Object, t_transportStates.G2S_transportDown);

            eventBus.Verify(m => m.Publish(It.IsAny<TransportDownEvent>()));
        }

        [TestMethod]
        public void WhenNotifyTransportUpExpectEvent()
        {
            const int deviceId = 1;

            var eventBus = new Mock<IEventBus>();

            var observer = new TransportStateObserver(eventBus.Object);

            var device = new Mock<ICommunicationsDevice>();
            device.SetupGet(d => d.Id).Returns(deviceId);

            observer.Notify(device.Object, t_transportStates.G2S_transportUp);

            eventBus.Verify(m => m.Publish(It.IsAny<TransportUpEvent>()));
        }
    }
}