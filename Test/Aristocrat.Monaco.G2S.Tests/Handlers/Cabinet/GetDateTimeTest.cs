namespace Aristocrat.Monaco.G2S.Tests.Handlers.Cabinet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers.Cabinet;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetDateTimeTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetDateTime(null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new GetDateTime(egm.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var handler = new GetDateTime(egm.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var handler = new GetDateTime(HandlerUtilities.CreateMockEgm<ICabinetDevice>());

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<ICabinetDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var handler = new GetDateTime(HandlerUtilities.CreateMockEgm(device));

            await VerificationTests.VerifyChecksForOwnerAndGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithGuestAccessExpectNoError()
        {
            const int otherHostId = 99;

            var device = new Mock<ICabinetDevice>();
            var egm = HandlerUtilities.CreateMockEgm(device);

            device.SetupGet(d => d.Owner).Returns(otherHostId);
            device.SetupGet(d => d.Guests).Returns(new List<int> { TestConstants.HostId });

            var handler = new GetDateTime(egm);

            await VerificationTests.VerifyAllowsGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<ICabinetDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var handler = new GetDateTime(HandlerUtilities.CreateMockEgm(device));

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenHandleCommandExpectTime()
        {
            var device = new Mock<ICabinetDevice>();

            var command = ClassCommandUtilities.CreateClassCommand<cabinet, getDateTime>(
                TestConstants.HostId,
                TestConstants.EgmId);

            var handler = new GetDateTime(HandlerUtilities.CreateMockEgm(device));

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<cabinet, cabinetDateTime>;

            Assert.IsNotNull(response);
        }
    }
}