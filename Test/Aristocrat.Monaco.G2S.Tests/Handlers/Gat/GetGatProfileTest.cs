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
    using G2S.Handlers.Gat;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetGatProfileTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetGatProfile(null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new GetGatProfile(egm.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var handler = new GetGatProfile(egm.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var handler = new GetGatProfile(HandlerUtilities.CreateMockEgm<IGatDevice>());

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<IGatDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var handler = new GetGatProfile(HandlerUtilities.CreateMockEgm(device));

            await VerificationTests.VerifyChecksForOwnerAndGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithGuestAccessExpectNoError()
        {
            const int otherHostId = 99;

            var device = new Mock<IGatDevice>();

            device.SetupGet(d => d.Owner).Returns(otherHostId);
            device.SetupGet(d => d.Guests).Returns(new List<int> { TestConstants.HostId });

            var handler = new GetGatProfile(HandlerUtilities.CreateMockEgm(device));

            await VerificationTests.VerifyAllowsGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<IGatDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var handler = new GetGatProfile(HandlerUtilities.CreateMockEgm(device));

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenHandleExpectResponse()
        {
            var egm = new Mock<IG2SEgm>();
            var device = new Mock<IGatDevice>();

            device.SetupGet(x => x.ConfigurationId).Returns(1);
            device.SetupGet(x => x.ConfigComplete).Returns(true);
            device.SetupGet(x => x.SpecialFunctions).Returns(t_g2sBoolean.G2S_true);
            device.SetupGet(x => x.IdReaderId).Returns(2);
            device.SetupGet(x => x.MinLogEntries).Returns(3);
            device.SetupGet(x => x.MinQueuedComps).Returns(4);
            device.SetupGet(x => x.TimeToLive).Returns(5);
            device.SetupGet(x => x.UseDefaultConfig).Returns(true);
            device.SetupGet(x => x.ConfigDateTime).Returns(DateTime.UtcNow);

            egm.Setup(e => e.GetDevice<IGatDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);

            var command = ClassCommandUtilities.CreateClassCommand<gat, getGatProfile>(
                TestConstants.HostId,
                TestConstants.EgmId);

            var handler = new GetGatProfile(egm.Object);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<gat, gatProfile>;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.Command.configurationId, 1);
            Assert.IsTrue(response.Command.configComplete);
            Assert.AreEqual(response.Command.specialFunctions, t_g2sBoolean.G2S_true);
            Assert.AreEqual(response.Command.idReaderId, 2);
            Assert.AreEqual(response.Command.minLogEntries, 3);
            Assert.AreEqual(response.Command.minQueuedComps, 4);
            Assert.AreEqual(response.Command.timeToLive, 5);
            Assert.IsTrue(response.Command.configComplete);
            Assert.IsTrue(response.Command.configDateTime != default(DateTime));
            Assert.IsTrue(response.Command.configDateTimeSpecified);
        }
    }
}