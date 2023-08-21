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
    using Common.GAT.Models;
    using G2S.Handlers.Gat;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetGatLogStatusTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetGatLogStatus(null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructNullGatServiceExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new GetGatLogStatus(egm.Object, null);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var gatService = new Mock<IGatService>();

            var handler = new GetGatLogStatus(egm.Object, gatService.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var gatService = new Mock<IGatService>();
            var handler = new GetGatLogStatus(egm.Object, gatService.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var gatService = new Mock<IGatService>();
            var handler = new GetGatLogStatus(HandlerUtilities.CreateMockEgm<IGatDevice>(), gatService.Object);

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<IGatDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var gatService = new Mock<IGatService>();
            var handler = new GetGatLogStatus(HandlerUtilities.CreateMockEgm(device), gatService.Object);

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
            var handler = new GetGatLogStatus(HandlerUtilities.CreateMockEgm(device), gatService.Object);

            await VerificationTests.VerifyAllowsGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<IGatDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var gatService = new Mock<IGatService>();
            var handler = new GetGatLogStatus(HandlerUtilities.CreateMockEgm(device), gatService.Object);

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenHandleExpectResponse()
        {
            const long lastSequence = 999;
            const int totalEntries = 7;

            var egm = new Mock<IG2SEgm>();
            var device = new Mock<IGatDevice>();

            egm.Setup(e => e.GetDevice<IGatDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);

            var command = ClassCommandUtilities.CreateClassCommand<gat, getGatLogStatus>(
                TestConstants.HostId,
                TestConstants.EgmId);

            var gatService = new Mock<IGatService>();

            gatService.Setup(x => x.GetLogStatus()).Returns(new GetLogStatusResult(lastSequence, totalEntries));

            var handler = new GetGatLogStatus(egm.Object, gatService.Object);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<gat, gatLogStatus>;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.Command.lastSequence, lastSequence);
            Assert.AreEqual(response.Command.totalEntries, totalEntries);
        }
    }
}