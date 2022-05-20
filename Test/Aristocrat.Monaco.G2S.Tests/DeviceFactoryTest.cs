namespace Aristocrat.Monaco.G2S.Tests
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Data.Profile;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class DeviceFactoryTest
    {
        private const int HostId = 7;

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEgmIsNullExpectException()
        {
            var factory = new DeviceFactory(null, null);

            Assert.IsNull(factory);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenProfileServiceIsNullExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var factory = new DeviceFactory(
                egm.Object,
                null);

            Assert.IsNull(factory);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var profiles = new Mock<IProfileService>();

            var factory = new DeviceFactory(
                egm.Object,
                profiles.Object);

            Assert.IsNotNull(factory);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCreateWithNullHostExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var profiles = new Mock<IProfileService>();

            var factory = new DeviceFactory(
                egm.Object,
                profiles.Object);

            factory.Create(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCreateWithNullCallbackExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var profiles = new Mock<IProfileService>();

            var factory = new DeviceFactory(
                egm.Object,
                profiles.Object);

            var host = new Mock<IHost>();

            factory.Create(host.Object, null);
        }

        [TestMethod]
        public void WhenCreateDeviceWithNoOwnerExpectOwnedAndRegistered()
        {
            var egm = new Mock<IG2SEgm>();
            var profiles = new Mock<IProfileService>();
            var deviceObserver = new Mock<IDeviceObserver>();
            var egmStateObserver = new Mock<IEgmStateObserver>();

            var factory = new DeviceFactory(
                egm.Object,
                profiles.Object);

            var host = new Mock<IHostControl>();
            host.SetupGet(h => h.Id).Returns(HostId);

            egm.Setup(e => e.Hosts).Returns(new List<IHostControl> { host.Object });

            var device = new CabinetDevice(deviceObserver.Object, egmStateObserver.Object);

            var result = factory.Create(host.Object, () => device);

            Assert.IsNotNull(result);
            Assert.AreEqual(device, result);
            Assert.AreEqual(device.Owner, HostId);

            egm.Verify(e => e.AddDevice(It.Is<IDevice>(d => d == device)));
            profiles.Verify(p => p.Save(It.Is<IDevice>(d => d == device)));
        }

        [TestMethod]
        public void WhenCreateExistingDeviceExpectPopulated()
        {
            var egm = new Mock<IG2SEgm>();
            var profiles = new Mock<IProfileService>();
            var deviceObserver = new Mock<IDeviceObserver>();
            var egmStateObserver = new Mock<IEgmStateObserver>();

            var factory = new DeviceFactory(
                egm.Object,
                profiles.Object);

            var host = new Mock<IHost>();
            host.SetupGet(h => h.Id).Returns(HostId);

            var device = new CabinetDevice(deviceObserver.Object, egmStateObserver.Object);

            profiles.Setup(p => p.Exists(It.Is<IDevice>(d => d == device))).Returns(true);

            var result = factory.Create(host.Object, () => device);

            Assert.IsNotNull(result);
            Assert.AreEqual(device, result);

            egm.Verify(e => e.AddDevice(It.Is<IDevice>(d => d == device)));
            profiles.Verify(p => p.Populate(It.Is<IDevice>(d => d == device)));
        }
    }
}