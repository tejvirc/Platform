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
    public class SetEventHandlerStateTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new SetEventHandlerState(null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmStateManagerExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var handler = new SetEventHandlerState(egm.Object, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCommandBuilderExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var egmStateManager = new Mock<IEgmStateManager>();
            var handler = new SetEventHandlerState(egm.Object, egmStateManager.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var egmStateManager = new Mock<IEgmStateManager>();
            var command = new Mock<ICommandBuilder<IEventHandlerDevice, eventHandlerStatus>>();
            var handler = new SetEventHandlerState(
                egm.Object,
                egmStateManager.Object,
                command.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var egmStateManager = new Mock<IEgmStateManager>();
            var command = new Mock<ICommandBuilder<IEventHandlerDevice, eventHandlerStatus>>();
            var handler = new SetEventHandlerState(
                egm.Object,
                egmStateManager.Object,
                command.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var egmStateManager = new Mock<IEgmStateManager>();
            var egm = EventsUtiliites.CreateMockEgm();
            var command = new Mock<ICommandBuilder<IEventHandlerDevice, eventHandlerStatus>>();
            var handler = new SetEventHandlerState(egm, egmStateManager.Object, command.Object);

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var evtDevice = new Mock<IEventHandlerDevice>();
            var egmStateManager = new Mock<IEgmStateManager>();
            var egm = EventsUtiliites.CreateMockEgm(evtDevice);
            evtDevice.SetupGet(d => d.Owner).Returns(TestConstants.HostId);
            var command = new Mock<ICommandBuilder<IEventHandlerDevice, eventHandlerStatus>>();
            var handler = new SetEventHandlerState(egm, egmStateManager.Object, command.Object);
            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenEmgNotReturnDeviceExpectNoResponse()
        {
            var egm = new Mock<IG2SEgm>();
            var egmStateManager = new Mock<IEgmStateManager>();
            var commandBuilder = new Mock<ICommandBuilder<IEventHandlerDevice, eventHandlerStatus>>();
            var handler = new SetEventHandlerState(
                egm.Object,
                egmStateManager.Object,
                commandBuilder.Object);

            var command =
                ClassCommandUtilities.CreateClassCommand<eventHandler, setEventHandlerState>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            await handler.Handle(command);

            Assert.AreEqual(command.Responses.Count(), 0);
        }

        [TestMethod]
        public async Task WhenCommandEnableOnHandleCommandExpectResponse()
        {
            var egm = new Mock<IG2SEgm>();
            var egmStateManager = new Mock<IEgmStateManager>();
            var device = new Mock<IEventHandlerDevice>();
            egm.Setup(e => e.GetDevice<IEventHandlerDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);
            device.SetupGet(x => x.ConfigComplete).Returns(true);
            device.SetupGet(x => x.Enabled).Returns(true);
            device.SetupGet(x => x.Overflow).Returns(true);
            device.SetupGet(x => x.HostEnabled).Returns(true);
            device.SetupGet(x => x.ConfigurationId).Returns(1);
            device.SetupGet(x => x.DeviceClass).Returns("EventHandler");
            var commandBuilder = new Mock<ICommandBuilder<IEventHandlerDevice, eventHandlerStatus>>();
            var command = ClassCommandUtilities.CreateClassCommand<eventHandler, setEventHandlerState>(
                TestConstants.HostId,
                TestConstants.EgmId);
            var handler = new SetEventHandlerState(
                egm.Object,
                egmStateManager.Object,
                commandBuilder.Object);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<eventHandler, eventHandlerStatus>;

            device.Verify(x => x.DeviceStateChanged(), Times.Once);

            Assert.IsNotNull(response);
            /* TODO: figure out how to Moq ICommandBuilder
            Assert.IsTrue(response.Command.configComplete);
            Assert.IsTrue(response.Command.egmEnabled);
            Assert.IsTrue(response.Command.eventHandlerOverflow);
            Assert.IsTrue(response.Command.hostEnabled);
            Assert.AreEqual(response.Command.configurationId, 1);
            */
        }

        [TestMethod]
        public async Task WhenCommandDisableOnHandleCommandExpectResponse()
        {
            var egm = new Mock<IG2SEgm>();
            var egmStateManager = new Mock<IEgmStateManager>();
            var device = new Mock<IEventHandlerDevice>();
            egm.Setup(e => e.GetDevice<IEventHandlerDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);
            var dateTime = DateTime.UtcNow;
            device.SetupGet(x => x.ConfigDateTime).Returns(dateTime);
            device.SetupGet(x => x.DeviceClass).Returns("EventHandler");
            var commandBuilder = new Mock<ICommandBuilder<IEventHandlerDevice, eventHandlerStatus>>();
            var command = ClassCommandUtilities.CreateClassCommand<eventHandler, setEventHandlerState>(
                TestConstants.HostId,
                TestConstants.EgmId);
            command.Command.enable = false;

            var handler = new SetEventHandlerState(
                egm.Object,
                egmStateManager.Object,
                commandBuilder.Object);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<eventHandler, eventHandlerStatus>;
            device.Verify(x => x.DeviceStateChanged(), Times.Once);
            Assert.IsNotNull(response);

            /*Assert.AreEqual(response.Command.configDateTime, dateTime);
            Assert.IsTrue(response.Command.configDateTimeSpecified);*/
        }
    }
}