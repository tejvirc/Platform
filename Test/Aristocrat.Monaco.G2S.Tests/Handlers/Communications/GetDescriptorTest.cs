namespace Aristocrat.Monaco.G2S.Tests.Handlers.Communications
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers.Communications;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetDescriptorTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetDescriptor(null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var descriptorFactory = new Mock<IDeviceDescriptorFactory>();

            var handler = new GetDescriptor(egm.Object, descriptorFactory.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var descriptorFactory = new Mock<IDeviceDescriptorFactory>();

            var handler = new GetDescriptor(egm.Object, descriptorFactory.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var descriptorFactory = new Mock<IDeviceDescriptorFactory>();

            var handler = new GetDescriptor(HandlerUtilities.CreateMockEgm<ICommunicationsDevice>(), descriptorFactory.Object);

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var descriptorFactory = new Mock<IDeviceDescriptorFactory>();

            var device = new Mock<ICommunicationsDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var handler = new GetDescriptor(HandlerUtilities.CreateMockEgm(device), descriptorFactory.Object);

            await VerificationTests.VerifyChecksForOwnerAndGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithGuestAccessExpectNoError()
        {
            const int otherHostId = 99;

            var descriptorFactory = new Mock<IDeviceDescriptorFactory>();

            var device = new Mock<ICommunicationsDevice>();
            var egm = HandlerUtilities.CreateMockEgm(device);

            device.SetupGet(d => d.Owner).Returns(otherHostId);
            device.SetupGet(d => d.Guests).Returns(new List<int> { TestConstants.HostId });

            var handler = new GetDescriptor(egm, descriptorFactory.Object);

            await VerificationTests.VerifyAllowsGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var descriptorFactory = new Mock<IDeviceDescriptorFactory>();

            var device = new Mock<ICommunicationsDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var handler = new GetDescriptor(HandlerUtilities.CreateMockEgm(device), descriptorFactory.Object);

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenHandleCommandExpectResponse()
        {
            var egm = new Mock<IG2SEgm>();
            var device = new Mock<ICommunicationsDevice>();
            var descriptorFactory = new Mock<IDeviceDescriptorFactory>();

            device.SetupGet(comms => comms.DeviceClass).Returns(typeof(communications).Name);
            device.SetupGet(comms => comms.Id).Returns(1);
            device.SetupGet(comms => comms.Owner).Returns(TestConstants.HostId);
            device.SetupGet(comms => comms.Active).Returns(true);
            egm.Setup(e => e.GetDevice<ICommunicationsDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);
            egm.Setup(e => e.Devices).Returns(new List<IDevice> { device.Object });

            var command = ClassCommandUtilities.CreateClassCommand<communications, getDescriptor>(
                TestConstants.HostId,
                TestConstants.EgmId);
            command.Command.deviceClass = DeviceClass.G2S_all;
            command.Command.deviceId = DeviceId.All;
            command.Command.includeOwners = true;

            var handler = new GetDescriptor(egm.Object, descriptorFactory.Object);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<communications, descriptorList>;

            Assert.IsNotNull(response);
            Assert.IsTrue(response.Command.descriptor.Length > 0);
        }
    }
}