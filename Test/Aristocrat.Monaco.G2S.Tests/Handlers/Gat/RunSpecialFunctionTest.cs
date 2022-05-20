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
    public class RunSpecialFunctionTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new RunSpecialFunction(null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructNullGatServiceExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new RunSpecialFunction(egm.Object, null);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var gatService = new Mock<IGatService>();

            var handler = new RunSpecialFunction(egm.Object, gatService.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var gatService = new Mock<IGatService>();
            var handler = new RunSpecialFunction(egm.Object, gatService.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var gatService = new Mock<IGatService>();
            var handler = new RunSpecialFunction(HandlerUtilities.CreateMockEgm<IGatDevice>(), gatService.Object);

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<IGatDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var gatService = new Mock<IGatService>();
            var handler = new RunSpecialFunction(HandlerUtilities.CreateMockEgm(device), gatService.Object);

            await VerificationTests.VerifyChecksForOwner(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithGuestAccessExpectError()
        {
            const int otherHostId = 99;

            var device = new Mock<IGatDevice>();

            device.SetupGet(d => d.Owner).Returns(otherHostId);
            device.SetupGet(d => d.Guests).Returns(new List<int> { TestConstants.HostId });

            var gatService = new Mock<IGatService>();
            var handler = new RunSpecialFunction(HandlerUtilities.CreateMockEgm(device), gatService.Object);

            await VerificationTests.VerifyDeniesGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoFeatureExpectError()
        {
            var device = new Mock<IGatDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var gatService = new Mock<IGatService>();
            var handler = new RunSpecialFunction(HandlerUtilities.CreateMockEgm(device), gatService.Object);

            var command = ClassCommandUtilities.CreateClassCommand<gat, runSpecialFunction>(
                TestConstants.HostId,
                TestConstants.EgmId);
            command.Command.feature = "Test";

            var result = await handler.Verify(command);

            Assert.AreEqual(result.Code, ErrorCode.G2S_GAX002);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            const string feature = "Test";
            const string exec = "TestExec";

            var device = new Mock<IGatDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

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

            var handler = new RunSpecialFunction(HandlerUtilities.CreateMockEgm(device), gatService.Object);

            var command = ClassCommandUtilities.CreateClassCommand<gat, runSpecialFunction>(
                TestConstants.HostId,
                TestConstants.EgmId);
            command.Command.feature = feature;

            await VerificationTests.VerifyCanSucceed(handler, command);
        }

        [TestMethod]
        public async Task WhenHandleExpectResponse()
        {
            const string feature = "Test";
            const string exec = "TestExec";

            var egm = new Mock<IG2SEgm>();
            var device = new Mock<IGatDevice>();

            device.SetupGet(comms => comms.DeviceClass).Returns(typeof(gat).Name);
            egm.Setup(e => e.GetDevice<IGatDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);
            egm.Setup(e => e.Devices).Returns(new List<IDevice> { device.Object });

            var command = ClassCommandUtilities.CreateClassCommand<gat, runSpecialFunction>(
                TestConstants.HostId,
                TestConstants.EgmId);
            command.Command.feature = feature;

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

            var handler = new RunSpecialFunction(egm.Object, gatService.Object);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<gat, specialFunctionResult>;

            Assert.IsNotNull(response);
        }
    }
}