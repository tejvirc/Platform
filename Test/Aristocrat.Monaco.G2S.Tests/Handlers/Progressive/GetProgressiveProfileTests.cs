namespace Aristocrat.Monaco.G2S.Tests.Handlers.Progressive
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
    using G2S.Handlers.Progressive;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetProgressiveProfileTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetProgressiveProfile(null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullProgressiveProfileExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new GetProgressiveProfile(egm.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var builder = new Mock<ICommandBuilder<IProgressiveDevice, progressiveProfile>>();

            var handler = new GetProgressiveProfile(egm.Object, builder.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var builder = new Mock<ICommandBuilder<IProgressiveDevice, progressiveProfile>>();
            var handler = new GetProgressiveProfile(egm.Object, builder.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var builder = new Mock<ICommandBuilder<IProgressiveDevice, progressiveProfile>>();
            var handler = new GetProgressiveProfile(
                HandlerUtilities.CreateMockEgm<IProgressiveDevice>(),
                builder.Object);

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<IProgressiveDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var builder = new Mock<ICommandBuilder<IProgressiveDevice, progressiveProfile>>();
            var handler = new GetProgressiveProfile(HandlerUtilities.CreateMockEgm(device), builder.Object);

            await VerificationTests.VerifyChecksForOwnerAndGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithGuestAccessExpectNoError()
        {
            const int otherHostId = 99;

            var device = new Mock<IProgressiveDevice>();
            var egm = HandlerUtilities.CreateMockEgm(device);

            device.SetupGet(d => d.Owner).Returns(otherHostId);
            device.SetupGet(d => d.Guests).Returns(new List<int> { TestConstants.HostId });

            var builder = new Mock<ICommandBuilder<IProgressiveDevice, progressiveProfile>>();
            var handler = new GetProgressiveProfile(egm, builder.Object);

            await VerificationTests.VerifyAllowsGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<IProgressiveDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var builder = new Mock<ICommandBuilder<IProgressiveDevice, progressiveProfile>>();
            var handler = new GetProgressiveProfile(HandlerUtilities.CreateMockEgm(device), builder.Object);

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenHandleCommandExpectResponse()
        {
            var device = new Mock<IProgressiveDevice>();

            var command = ClassCommandUtilities.CreateClassCommand<progressive, getProgressiveProfile>(
                TestConstants.HostId,
                TestConstants.EgmId);

            var builder = new Mock<ICommandBuilder<IProgressiveDevice, progressiveProfile>>();
            var handler = new GetProgressiveProfile(HandlerUtilities.CreateMockEgm(device), builder.Object);

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<progressive, progressiveProfile>;

            Assert.IsNotNull(response);

            builder.Verify(b => b.Build(device.Object, It.IsAny<progressiveProfile>()));
        }
    }
}