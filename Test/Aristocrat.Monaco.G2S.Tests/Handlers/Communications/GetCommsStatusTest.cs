namespace Aristocrat.Monaco.G2S.Tests.Handlers.Communications
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers;
    using G2S.Handlers.Communications;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetCommsStatusTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetCommsStatus(null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCommandBuilderExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new GetCommsStatus(egm.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var command = new Mock<ICommandBuilder<ICommunicationsDevice, commsStatus>>();

            var handler = new GetCommsStatus(egm.Object, command.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var command = new Mock<ICommandBuilder<ICommunicationsDevice, commsStatus>>();
            var handler = new GetCommsStatus(egm.Object, command.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var command = new Mock<ICommandBuilder<ICommunicationsDevice, commsStatus>>();

            var handler = new GetCommsStatus(HandlerUtilities.CreateMockEgm<ICommunicationsDevice>(), command.Object);

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<ICommunicationsDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var command = new Mock<ICommandBuilder<ICommunicationsDevice, commsStatus>>();
            var handler = new GetCommsStatus(HandlerUtilities.CreateMockEgm(device), command.Object);

            await VerificationTests.VerifyChecksForOwnerAndGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithGuestAccessExpectNoError()
        {
            const int otherHostId = 99;

            var device = new Mock<ICommunicationsDevice>();
            var egm = HandlerUtilities.CreateMockEgm(device);

            device.SetupGet(d => d.Owner).Returns(otherHostId);
            device.SetupGet(d => d.Guests).Returns(new List<int> { TestConstants.HostId });

            var command = new Mock<ICommandBuilder<ICommunicationsDevice, commsStatus>>();
            var handler = new GetCommsStatus(egm, command.Object);

            await VerificationTests.VerifyAllowsGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<ICommunicationsDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var command = new Mock<ICommandBuilder<ICommunicationsDevice, commsStatus>>();
            var handler = new GetCommsStatus(HandlerUtilities.CreateMockEgm(device), command.Object);

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenHandleCommandExpectResponse()
        {
            var egm = new Mock<IG2SEgm>();
            var device = new Mock<ICommunicationsDevice>();

            device.SetupGet(comms => comms.TransportState).Returns(t_transportStates.G2S_transportUp);
            egm.Setup(e => e.GetDevice<ICommunicationsDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);

            var command = ClassCommandUtilities.CreateClassCommand<communications, getCommsStatus>(
                TestConstants.HostId,
                TestConstants.EgmId);

            var builder = new Mock<ICommandBuilder<ICommunicationsDevice, commsStatus>>();
            var handler = new GetCommsStatus(egm.Object, builder.Object);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<communications, commsStatus>;

            Assert.IsNotNull(response);

            builder.Verify(b => b.Build(device.Object, response.Command));
        }
    }
}