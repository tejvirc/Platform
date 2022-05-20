namespace Aristocrat.Monaco.G2S.Tests.Handlers.Gat
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.GAT.Storage;
    using G2S.Handlers.Gat;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetSpecialFunctionsTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetSpecialFunctions(null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructNullGatServiceExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new GetSpecialFunctions(egm.Object, null);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var gatService = new Mock<IGatService>();
            var handler = new GetSpecialFunctions(egm.Object, gatService.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var gatService = new Mock<IGatService>();
            var handler = new GetSpecialFunctions(egm.Object, gatService.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var gatService = new Mock<IGatService>();
            var handler = new GetSpecialFunctions(HandlerUtilities.CreateMockEgm<IGatDevice>(), gatService.Object);

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<IGatDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var gatService = new Mock<IGatService>();
            var handler = new GetSpecialFunctions(HandlerUtilities.CreateMockEgm(device), gatService.Object);

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
            var handler = new GetSpecialFunctions(HandlerUtilities.CreateMockEgm(device), gatService.Object);

            await VerificationTests.VerifyAllowsGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<IGatDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var gatService = new Mock<IGatService>();
            var handler = new GetSpecialFunctions(HandlerUtilities.CreateMockEgm(device), gatService.Object);

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenHandleExpectResponse()
        {
            const string feature = "Test";
            const string exec = "notepad.exe";

            var egm = new Mock<IG2SEgm>();
            var device = new Mock<IGatDevice>();
            var queue = new Mock<ICommandQueue>();

            queue.SetupGet(q => q.TimeToLiveBehavior).Returns(TimeToLiveBehavior.Strict);
            device.SetupGet(comms => comms.Queue).Returns(queue.Object);
            device.SetupGet(comms => comms.DeviceClass).Returns(typeof(gat).Name);
            egm.Setup(e => e.GetDevice<IGatDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);
            egm.Setup(e => e.Devices).Returns(new List<IDevice> { device.Object });

            var command = ClassCommandUtilities.CreateClassCommand<gat, getSpecialFunctions>(
                TestConstants.HostId,
                TestConstants.EgmId);

            var gatService = new Mock<IGatService>();

            gatService.Setup(s => s.GetSpecialFunctions()).Returns(
                new List<GatSpecialFunction>
                {
                    new GatSpecialFunction
                    {
                        Feature = feature,
                        GatExec = exec
                    }
                });

            var handler = new GetSpecialFunctions(egm.Object, gatService.Object);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<gat, specialFunctions>;
            Assert.IsNotNull(response);

            var functions = response.Command.function;
            Assert.IsNotNull(functions);
            Assert.IsTrue(functions.Length >= 1);
        }
    }
}