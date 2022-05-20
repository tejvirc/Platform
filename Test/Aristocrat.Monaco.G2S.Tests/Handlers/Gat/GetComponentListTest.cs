namespace Aristocrat.Monaco.G2S.Tests.Handlers.Gat
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Contracts.Authentication;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.GAT.Models;
    using G2S.Handlers.Gat;
    using Kernel.Contracts.Components;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetComponentListTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetComponentList(null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructNullGatServiceExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new GetComponentList(egm.Object, null);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var gatService = new Mock<IGatService>();

            var handler = new GetComponentList(egm.Object, gatService.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var gatService = new Mock<IGatService>();
            var handler = new GetComponentList(egm.Object, gatService.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var gatService = new Mock<IGatService>();
            var handler = new GetComponentList(HandlerUtilities.CreateMockEgm<IGatDevice>(), gatService.Object);

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<IGatDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var gatService = new Mock<IGatService>();
            var handler = new GetComponentList(HandlerUtilities.CreateMockEgm(device), gatService.Object);

            await VerificationTests.VerifyChecksForOwnerAndGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithGuestAccessExpectNoError()
        {
            const int otherHostId = 99;

            var device = new Mock<IGatDevice>();

            device.SetupGet(d => d.Owner).Returns(otherHostId);
            device.SetupGet(d => d.Guests).Returns(new List<int> { TestConstants.HostId });

            var gatService = new Mock<IGatService>();
            var handler = new GetComponentList(HandlerUtilities.CreateMockEgm(device), gatService.Object);

            await VerificationTests.VerifyAllowsGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<IGatDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var gatService = new Mock<IGatService>();
            var handler = new GetComponentList(HandlerUtilities.CreateMockEgm(device), gatService.Object);

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenHandleExpectResponse()
        {
            const string componentDescription = "Test";
            const long componentSize = 1024;
            const ComponentType componentType = ComponentType.Software;

            var egm = new Mock<IG2SEgm>();
            var device = new Mock<IGatDevice>();

            device.SetupGet(comms => comms.DeviceClass).Returns(typeof(gat).Name);
            egm.Setup(e => e.GetDevice<IGatDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);
            egm.Setup(e => e.Devices).Returns(new List<IDevice> { device.Object });

            var command = ClassCommandUtilities.CreateClassCommand<gat, getComponentList>(
                TestConstants.HostId,
                TestConstants.EgmId);

            var gatService = new Mock<IGatService>();

            gatService.Setup(g => g.GetSupportedAlgorithms(It.IsAny<ComponentType>())).Returns(
                new List<IAlgorithm>()
                {
                    new Algorithm { Type = AlgorithmType.Sha1 }
                });

            gatService.Setup(s => s.GetComponentList()).Returns(
                new List<Component>
                {
                    new Component
                    {
                        ComponentId = Guid.NewGuid().ToString(),
                        Description = componentDescription,
                        Size = componentSize,
                        Type = componentType
                    }
                });

            var handler = new GetComponentList(egm.Object, gatService.Object);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<gat, componentList>;
            Assert.IsNotNull(response);

            var components = response.Command.component;
            Assert.IsNotNull(components);
            Assert.IsTrue(components.Length >= 1);

            var component = components.First();
            Assert.IsNotNull(component);
            Assert.IsTrue(component.algorithm.Length > 0);
        }
    }
}
