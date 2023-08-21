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
    using G2S.Handlers;
    using G2S.Handlers.Communications;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SetCommsStateTest
    {
        private const string DisableMessage = @"Test Disable Comms Device";

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new SetCommsState(null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCommandBuilderExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new SetCommsState(egm.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var builder = new Mock<ICommandBuilder<ICommunicationsDevice, commsStatus>>();

            var handler = new SetCommsState(egm.Object, builder.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var builder = new Mock<ICommandBuilder<ICommunicationsDevice, commsStatus>>();

            var handler = new SetCommsState(egm.Object, builder.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var builder = new Mock<ICommandBuilder<ICommunicationsDevice, commsStatus>>();

            var handler = new SetCommsState(HandlerUtilities.CreateMockEgm<ICommunicationsDevice>(), builder.Object);

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<ICommunicationsDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var builder = new Mock<ICommandBuilder<ICommunicationsDevice, commsStatus>>();
            var handler = new SetCommsState(HandlerUtilities.CreateMockEgm(device), builder.Object);

            await VerificationTests.VerifyChecksForOwner(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithGuestAccessExpectError()
        {
            const int otherHostId = 99;

            var device = new Mock<ICommunicationsDevice>();
            var egm = HandlerUtilities.CreateMockEgm(device);

            device.SetupGet(d => d.Owner).Returns(otherHostId);
            device.SetupGet(d => d.Guests).Returns(new List<int> { TestConstants.HostId });

            var builder = new Mock<ICommandBuilder<ICommunicationsDevice, commsStatus>>();
            var handler = new SetCommsState(egm, builder.Object);

            await VerificationTests.VerifyDeniesGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<ICommunicationsDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var builder = new Mock<ICommandBuilder<ICommunicationsDevice, commsStatus>>();
            var handler = new SetCommsState(HandlerUtilities.CreateMockEgm(device), builder.Object);

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenHandleDisableExpectDisabled()
        {
            var egm = new Mock<IG2SEgm>();
            var commsDevice = new Mock<ICommunicationsDevice>();

            commsDevice.SetupGet(comms => comms.TransportState).Returns(t_transportStates.G2S_transportUp);
            commsDevice.SetupGet(comms => comms.RequiredForPlay).Returns(true);
            egm.Setup(e => e.GetDevice<ICommunicationsDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(commsDevice.Object);

            var command = ClassCommandUtilities.CreateClassCommand<communications, setCommsState>(
                TestConstants.HostId,
                TestConstants.EgmId);
            command.Command.disableText = string.Empty;

            var builder = new Mock<ICommandBuilder<ICommunicationsDevice, commsStatus>>();
            var handler = new SetCommsState(egm.Object, builder.Object);

            // Need to disable first to generate the message
            command.Command.enable = false;
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<communications, commsStatus>;

            Assert.IsNotNull(response);

            commsDevice.Verify(
                comms =>
                    comms.TriggerStateChange(
                        It.Is<CommunicationTrigger>(t => t == CommunicationTrigger.HostDisabled),
                        string.Empty));

            builder.Verify(b => b.Build(commsDevice.Object, response.Command));
        }

        [TestMethod]
        public async Task WhenHandleEnableExpectEnabled()
        {
            var egm = new Mock<IG2SEgm>();
            var commsDevice = new Mock<ICommunicationsDevice>();

            commsDevice.SetupGet(comms => comms.TransportState).Returns(t_transportStates.G2S_transportUp);
            commsDevice.SetupGet(comms => comms.RequiredForPlay).Returns(true);
            egm.Setup(e => e.GetDevice<ICommunicationsDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(commsDevice.Object);

            var command = ClassCommandUtilities.CreateClassCommand<communications, setCommsState>(
                TestConstants.HostId,
                TestConstants.EgmId);
            command.Command.disableText = DisableMessage;

            var builder = new Mock<ICommandBuilder<ICommunicationsDevice, commsStatus>>();
            var handler = new SetCommsState(egm.Object, builder.Object);

            // Need to disable first to generate the message
            command.Command.enable = false;
            await handler.Handle(command);

            command.Command.enable = true;
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<communications, commsStatus>;

            Assert.IsNotNull(response);

            commsDevice.Verify(
                comms =>
                    comms.TriggerStateChange(
                        It.Is<CommunicationTrigger>(t => t == CommunicationTrigger.HostEnabled),
                        DisableMessage));

            builder.Verify(b => b.Build(commsDevice.Object, response.Command));
        }
    }
}