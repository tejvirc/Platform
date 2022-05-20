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
    using G2S.Handlers.Communications;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SetKeepAliveTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new SetKeepAlive(null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new SetKeepAlive(egm.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var handler = new SetKeepAlive(egm.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var handler = new SetKeepAlive(HandlerUtilities.CreateMockEgm<ICommunicationsDevice>());

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<ICommunicationsDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var handler = new SetKeepAlive(HandlerUtilities.CreateMockEgm(device));

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

            var handler = new SetKeepAlive(egm);

            await VerificationTests.VerifyDeniesGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidIntervalExpectError()
        {
            const int invalidInterval = 4999; // It's in milliseconds

            var device = new Mock<ICommunicationsDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var handler = new SetKeepAlive(HandlerUtilities.CreateMockEgm(device));

            var command = ClassCommandUtilities.CreateClassCommand<communications, setKeepAlive>(
                TestConstants.HostId,
                TestConstants.EgmId);
            command.Command.interval = invalidInterval;

            var result = await handler.Verify(command);

            Assert.AreEqual(result.Code, ErrorCode.G2S_CMX001);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<ICommunicationsDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var handler = new SetKeepAlive(HandlerUtilities.CreateMockEgm(device));

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenHandleCommandExpectResponse()
        {
            const int testInterval = 30001;

            var egm = new Mock<IG2SEgm>();
            var commsDevice = new Mock<ICommunicationsDevice>();

            commsDevice.SetupGet(comms => comms.TransportState).Returns(t_transportStates.G2S_transportUp);
            egm.Setup(e => e.GetDevice<ICommunicationsDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(commsDevice.Object);

            var command = ClassCommandUtilities.CreateClassCommand<communications, setKeepAlive>(
                TestConstants.HostId,
                TestConstants.EgmId);
            command.Command.interval = testInterval;

            var handler = new SetKeepAlive(egm.Object);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<communications, setKeepAliveAck>;

            Assert.IsNotNull(response);

            commsDevice.Verify(d => d.SetKeepAlive(It.Is<int>(i => i == testInterval)));
        }
    }
}