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
    using G2S.Handlers;
    using G2S.Handlers.Cabinet;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetCabinetProfileTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetCabinetProfile(null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullBuilderExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new GetCabinetProfile(egm.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var builder = new Mock<ICommandBuilder<ICabinetDevice, cabinetProfile>>();

            var handler = new GetCabinetProfile(egm.Object, builder.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var builder = new Mock<ICommandBuilder<ICabinetDevice, cabinetProfile>>();
            var handler = new GetCabinetProfile(egm.Object, builder.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var builder = new Mock<ICommandBuilder<ICabinetDevice, cabinetProfile>>();
            var handler = new GetCabinetProfile(HandlerUtilities.CreateMockEgm<ICabinetDevice>(), builder.Object);

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<ICabinetDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var builder = new Mock<ICommandBuilder<ICabinetDevice, cabinetProfile>>();
            var handler = new GetCabinetProfile(HandlerUtilities.CreateMockEgm(device), builder.Object);

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

            var builder = new Mock<ICommandBuilder<ICabinetDevice, cabinetProfile>>();
            var handler = new GetCabinetProfile(egm, builder.Object);

            await VerificationTests.VerifyAllowsGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<ICabinetDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var builder = new Mock<ICommandBuilder<ICabinetDevice, cabinetProfile>>();
            var handler = new GetCabinetProfile(HandlerUtilities.CreateMockEgm(device), builder.Object);

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenHandleCommandExpectResponse()
        {
            var device = new Mock<ICabinetDevice>();

            var command = ClassCommandUtilities.CreateClassCommand<cabinet, getCabinetProfile>(
                TestConstants.HostId,
                TestConstants.EgmId);

            var builder = new Mock<ICommandBuilder<ICabinetDevice, cabinetProfile>>();
            var handler = new GetCabinetProfile(HandlerUtilities.CreateMockEgm(device), builder.Object);

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<cabinet, cabinetProfile>;

            Assert.IsNotNull(response);

            builder.Verify(b => b.Build(device.Object, It.IsAny<cabinetProfile>()));
        }
    }
}