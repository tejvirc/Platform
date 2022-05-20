namespace Aristocrat.Monaco.G2S.Tests.Handlers.Events
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers;
    using G2S.Handlers.Events;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetEventHandlerStatusTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetEventHandlerStatus(null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCommandBuilderException()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new GetEventHandlerStatus(egm.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var commandBuilder = new Mock<ICommandBuilder<IEventHandlerDevice, eventHandlerStatus>>();
            var handler = new GetEventHandlerStatus(egm.Object, commandBuilder.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var commandBuilder = new Mock<ICommandBuilder<IEventHandlerDevice, eventHandlerStatus>>();
            var handler = new GetEventHandlerStatus(egm.Object, commandBuilder.Object);
            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var egm = EventsUtiliites.CreateMockEgm();
            var commandBuilder = new Mock<ICommandBuilder<IEventHandlerDevice, eventHandlerStatus>>();
            var handler = new GetEventHandlerStatus(egm, commandBuilder.Object);
            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var evtDevice = new Mock<IEventHandlerDevice>();
            var egm = EventsUtiliites.CreateMockEgm(evtDevice);
            var commandBuilder = new Mock<ICommandBuilder<IEventHandlerDevice, eventHandlerStatus>>();
            evtDevice.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var evt = new eventHandler
            {
                deviceId = TestConstants.HostId,
                timeToLive = TestConstants.TimeToLive,
                dateTime = DateTime.UtcNow
            };
            ClassCommandUtilities.CreateClassCommand<eventHandler, getEventHandlerStatus>(
                evt,
                TestConstants.HostId,
                TestConstants.EgmId);

            var handler = new GetEventHandlerStatus(egm, commandBuilder.Object);
            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenEmgNotReturnDeviceExpectNoResponse()
        {
            var egm = new Mock<IG2SEgm>();
            var commandBuilder = new Mock<ICommandBuilder<IEventHandlerDevice, eventHandlerStatus>>();
            var handler = new GetEventHandlerStatus(egm.Object, commandBuilder.Object);

            var command =
                ClassCommandUtilities.CreateClassCommand<eventHandler, getEventHandlerStatus>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            await handler.Handle(command);

            Assert.AreEqual(command.Responses.Count(), 0);
        }

        [TestMethod]
        public async Task WhenHandleCommandExpectResponse()
        {
            var dateTime = DateTime.UtcNow;

            var egm = new Mock<IG2SEgm>();

            var device = new Mock<IEventHandlerDevice>();
            device.SetupGet(x => x.ConfigurationId).Returns(1);
            device.SetupGet(x => x.ConfigComplete).Returns(true);
            device.SetupGet(x => x.Enabled).Returns(true);
            device.SetupGet(x => x.HostEnabled).Returns(true);
            device.SetupGet(x => x.Overflow).Returns(true);
            device.SetupGet(x => x.ConfigDateTime).Returns(dateTime);

            egm.Setup(e => e.GetDevice<IEventHandlerDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);
            var commandBuilder = new Mock<ICommandBuilder<IEventHandlerDevice, eventHandlerStatus>>();

            var command =
                ClassCommandUtilities.CreateClassCommand<eventHandler, getEventHandlerStatus>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            var handler = new GetEventHandlerStatus(egm.Object, commandBuilder.Object);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<eventHandler, eventHandlerStatus>;

            Assert.IsNotNull(response);
            /* TODO: figure out how to Moq ICommandBuilder
            Assert.AreEqual(response.Command.configurationId, 1);
            Assert.IsTrue(response.Command.egmEnabled);
            Assert.IsTrue(response.Command.hostEnabled);
            Assert.IsTrue(response.Command.eventHandlerOverflow);
            Assert.IsTrue(response.Command.configComplete);
            Assert.AreEqual(response.Command.configDateTime, dateTime);
            Assert.IsTrue(response.Command.configDateTimeSpecified);*/
        }
    }
}