namespace Aristocrat.Monaco.G2S.Tests.Handlers.CommConfig
{
    using System;
    using System.Linq;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.CommConfig;
    using G2S.Handlers.CommConfig;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Constants = G2S.Constants;

    [TestClass]
    public class CommHostListCommandBuilderTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var builder = new CommHostListCommandBuilder(null);

            Assert.IsNull(builder);
        }

        [TestMethod]
        public void WhenBuildIncludeNoDevicesExpectCommHostItems()
        {
            var hostId = 1;

            var communicationsDeviceMock = CrateCommunicationsDevice();
            var egmMock = new Mock<IG2SEgm>();
            egmMock.Setup(m => m.GetDevice<ICommunicationsDevice>(hostId))
                .Returns(communicationsDeviceMock.Object);

            var hostControlMock = CreateHostControlMock(hostId);
            egmMock.Setup(m => m.Hosts).Returns(new[] { hostControlMock.Object });

            var builder = new CommHostListCommandBuilder(egmMock.Object);

            var command = new commHostList();

            var parameters = new CommHostListCommandBuilderParameters
            {
                HostIndexes = new[] { hostId },
                IncludeConfigDevices = false,
                IncludeGuestDevices = false,
                IncludeOwnerDevices = false
            };

            var commConfigDeviceMock = new Mock<ICommConfigDevice>();
            builder.Build(commConfigDeviceMock.Object, command, parameters);

            Assert.AreEqual(Constants.MaxHosts, command.commHostItem.Length);

            var hostItem = command.commHostItem.First(x => x.hostId == hostId);

            var hostControl = hostControlMock.Object;
            var communicationsDevice = communicationsDeviceMock.Object;

            Assert.AreEqual(hostControl.Id, hostItem.hostId);
            Assert.AreEqual(hostControl.Index, hostItem.hostIndex);
            Assert.AreEqual(hostControl.Address, hostItem.hostLocation);
            Assert.AreEqual(hostControl.Registered, hostItem.hostRegistered);
            Assert.AreEqual(communicationsDevice.UseDefaultConfig, hostItem.useDefaultConfig);
            Assert.AreEqual(communicationsDevice.RequiredForPlay, hostItem.requiredForPlay);
            Assert.AreEqual(communicationsDevice.TimeToLive, hostItem.timeToLive);
            Assert.AreEqual((int)communicationsDevice.NoResponseTimer.TotalMilliseconds, hostItem.noResponseTimer);
            Assert.AreEqual(communicationsDevice.AllowMulticast, hostItem.allowMulticast);
            Assert.AreEqual(true, hostItem.canModLocal);
            Assert.AreEqual(true, hostItem.canModRemote);
            Assert.AreEqual(communicationsDevice.DisplayFault, hostItem.displayCommFault);
        }

        [TestMethod]
        public void WhenBuildIncludeOwnerDeviceExpectSuccess()
        {
            var hostId = 1;

            var communicationsDeviceMock = CrateCommunicationsDevice();
            var egmMock = new Mock<IG2SEgm>();
            egmMock.Setup(m => m.GetDevice<ICommunicationsDevice>(hostId))
                .Returns(communicationsDeviceMock.Object);

            var hostControlMock = CreateHostControlMock(hostId);
            egmMock.Setup(m => m.Hosts).Returns(new[] { hostControlMock.Object });

            var deviceMock = CreateDeviceMock();
            deviceMock.Setup(m => m.Owner).Returns(hostId);
            egmMock.Setup(m => m.Devices).Returns(new[] { deviceMock.Object });

            var builder = new CommHostListCommandBuilder(egmMock.Object);

            var command = new commHostList();

            var parameters = new CommHostListCommandBuilderParameters
            {
                HostIndexes = new[] { hostId },
                IncludeConfigDevices = false,
                IncludeGuestDevices = false,
                IncludeOwnerDevices = true
            };

            var commConfigDeviceMock = new Mock<ICommConfigDevice>();
            builder.Build(commConfigDeviceMock.Object, command, parameters);

            var commHostItem = command.commHostItem.First(x => x.hostId == hostId);

            Assert.IsNull(commHostItem.guestDevice1);
            Assert.IsNull(commHostItem.configDevice1);

            var ownerDevice = commHostItem.ownedDevice1.First();

            var device = deviceMock.Object;
            Assert.AreEqual(ownerDevice.deviceId, device.Id);
            Assert.AreEqual(ownerDevice.deviceClass.TrimmedDeviceClass(), device.DeviceClass);
            Assert.AreEqual(ownerDevice.deviceActive, device.Active);
        }

        [TestMethod]
        public void WhenBuildIncludeGuestDevicesExpectSuccess()
        {
            var hostId = 1;

            var communicationsDeviceMock = CrateCommunicationsDevice();
            var egmMock = new Mock<IG2SEgm>();
            egmMock.Setup(m => m.GetDevice<ICommunicationsDevice>(hostId))
                .Returns(communicationsDeviceMock.Object);

            var hostControlMock = CreateHostControlMock(hostId);
            egmMock.Setup(m => m.Hosts).Returns(new[] { hostControlMock.Object });

            var deviceMock = CreateDeviceMock();
            deviceMock.Setup(m => m.Guests).Returns(new[] { hostId });
            egmMock.Setup(m => m.Devices).Returns(new[] { deviceMock.Object });

            var builder = new CommHostListCommandBuilder(egmMock.Object);

            var command = new commHostList();

            var parameters = new CommHostListCommandBuilderParameters
            {
                HostIndexes = new[] { hostId },
                IncludeConfigDevices = false,
                IncludeGuestDevices = true,
                IncludeOwnerDevices = false
            };

            var commConfigDeviceMock = new Mock<ICommConfigDevice>();
            builder.Build(commConfigDeviceMock.Object, command, parameters);

            var commHostItem = command.commHostItem.First(x => x.hostId == hostId);

            Assert.IsNull(commHostItem.ownedDevice1);
            Assert.IsNull(commHostItem.configDevice1);

            var guestDevice = commHostItem.guestDevice1.First();

            var device = deviceMock.Object;
            Assert.AreEqual(guestDevice.deviceId, device.Id);
            Assert.AreEqual(guestDevice.deviceClass.TrimmedDeviceClass(), device.DeviceClass);
            Assert.AreEqual(guestDevice.deviceActive, device.Active);
        }

        [TestMethod]
        public void WhenBuildIncludeConfiguratorDeviceExpectSuccess()
        {
            var hostId = 1;

            var communicationsDeviceMock = CrateCommunicationsDevice();
            var egmMock = new Mock<IG2SEgm>();
            egmMock.Setup(m => m.GetDevice<ICommunicationsDevice>(hostId))
                .Returns(communicationsDeviceMock.Object);

            var hostControlMock = CreateHostControlMock(hostId);
            egmMock.Setup(m => m.Hosts).Returns(new[] { hostControlMock.Object });

            var deviceMock = CreateDeviceMock();
            deviceMock.Setup(m => m.Configurator).Returns(hostId);
            egmMock.Setup(m => m.Devices).Returns(new[] { deviceMock.Object });

            var builder = new CommHostListCommandBuilder(egmMock.Object);

            var command = new commHostList();

            var parameters = new CommHostListCommandBuilderParameters
            {
                HostIndexes = new[] { hostId },
                IncludeConfigDevices = true,
                IncludeGuestDevices = false,
                IncludeOwnerDevices = false
            };

            var commConfigDeviceMock = new Mock<ICommConfigDevice>();
            builder.Build(commConfigDeviceMock.Object, command, parameters);

            var commHostItem = command.commHostItem.First(x => x.hostId == hostId);

            Assert.IsNull(commHostItem.ownedDevice1);
            Assert.IsNull(commHostItem.guestDevice1);

            var configDevice = commHostItem.configDevice1.First();

            var device = deviceMock.Object;
            Assert.AreEqual(configDevice.deviceId, device.Id);
            Assert.AreEqual(configDevice.deviceClass.TrimmedDeviceClass(), device.DeviceClass);
            Assert.AreEqual(configDevice.deviceActive, device.Active);
        }

        private Mock<IDevice> CreateDeviceMock()
        {
            var deviceMock = new Mock<IDevice>();

            deviceMock.SetupGet(m => m.Id).Returns(2);
            deviceMock.SetupGet(m => m.Active).Returns(true);
            deviceMock.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_download.TrimmedDeviceClass());

            return deviceMock;
        }

        private Mock<IHostControl> CreateHostControlMock(int hostId)
        {
            var hostControlMock = new Mock<IHostControl>();

            hostControlMock.SetupGet(m => m.Id).Returns(hostId);
            hostControlMock.SetupGet(m => m.Index).Returns(hostId);
            hostControlMock.SetupGet(m => m.Address).Returns(new Uri("http://localhost"));
            hostControlMock.SetupGet(m => m.Registered).Returns(true);

            return hostControlMock;
        }

        private Mock<ICommunicationsDevice> CrateCommunicationsDevice()
        {
            var communicationsDeviceMock = new Mock<ICommunicationsDevice>();

            communicationsDeviceMock.SetupGet(m => m.UseDefaultConfig).Returns(true);
            communicationsDeviceMock.SetupGet(m => m.RequiredForPlay).Returns(true);
            communicationsDeviceMock.SetupGet(m => m.TimeToLive).Returns(10000);
            communicationsDeviceMock.SetupGet(m => m.NoResponseTimer).Returns(TimeSpan.FromMilliseconds(20000));
            communicationsDeviceMock.SetupGet(m => m.AllowMulticast).Returns(true);
            communicationsDeviceMock.SetupGet(m => m.DisplayFault).Returns(true);

            return communicationsDeviceMock;
        }
    }
}