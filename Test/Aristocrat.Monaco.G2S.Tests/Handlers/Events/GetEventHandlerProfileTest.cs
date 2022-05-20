namespace Aristocrat.Monaco.G2S.Tests.Handlers.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers.Events;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetEventHandlerProfileTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetEventHandlerProfile(null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new GetEventHandlerProfile(egm.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new GetEventHandlerProfile(egm.Object);
            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var egm = EventsUtiliites.CreateMockEgm();
            var handler = new GetEventHandlerProfile(egm);
            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var egm = new Mock<IG2SEgm>();
            var evtDevice = new Mock<IEventHandlerDevice>();
            var queue = new Mock<ICommandQueue>();

            queue.SetupGet(q => q.TimeToLiveBehavior).Returns(TimeToLiveBehavior.Strict);
            evtDevice.SetupGet(evt => evt.Queue).Returns(queue.Object);
            evtDevice.SetupGet(evt => evt.Owner).Returns(TestConstants.HostId);
            egm.Setup(e => e.GetDevice<IEventHandlerDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(evtDevice.Object);

            var command =
                ClassCommandUtilities.CreateClassCommand<eventHandler, getEventHandlerProfile>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            var handler = new GetEventHandlerProfile(egm.Object);
            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenEmgNotReturnDeviceExpectNoResponse()
        {
            var egm = new Mock<IG2SEgm>();
            var handler = new GetEventHandlerProfile(egm.Object);

            var command =
                ClassCommandUtilities.CreateClassCommand<eventHandler, getEventHandlerProfile>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            await handler.Handle(command);

            Assert.AreEqual(command.Responses.Count(), 0);
        }

        [TestMethod]
        public async Task WhenHandleCommandExpectResponse()
        {
            var egm = new Mock<IG2SEgm>();
            var eventDevice = new Mock<IEventHandlerDevice>();

            var dateTime = DateTime.UtcNow;

            eventDevice.SetupGet(x => x.ConfigurationId).Returns(1);
            eventDevice.SetupGet(x => x.RestartStatus).Returns(true);
            eventDevice.SetupGet(x => x.UseDefaultConfig).Returns(true);
            eventDevice.SetupGet(x => x.RequiredForPlay).Returns(true);
            eventDevice.SetupGet(x => x.MinLogEntries).Returns(2);
            eventDevice.SetupGet(x => x.TimeToLive).Returns(3);
            eventDevice.SetupGet(x => x.QueueBehavior).Returns(t_queueBehaviors.G2S_overwrite);
            eventDevice.SetupGet(x => x.ConfigComplete).Returns(true);
            eventDevice.SetupGet(x => x.DisableBehavior).Returns(t_disableBehaviors.G2S_discard);
            eventDevice.Setup(x => x.GetAllForcedEventSub()).Returns(
                new List<forcedSubscription>
                {
                    new forcedSubscription
                    {
                        deviceId = 1
                    }
                });
            eventDevice.SetupGet(x => x.ConfigDateTime).Returns(dateTime);

            egm.Setup(e => e.GetDevice<IEventHandlerDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(eventDevice.Object);

            var command =
                ClassCommandUtilities.CreateClassCommand<eventHandler, getEventHandlerProfile>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            var handler = new GetEventHandlerProfile(egm.Object);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<eventHandler, eventHandlerProfile>;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.Command.configurationId, 1);
            Assert.IsTrue(response.Command.restartStatus);
            Assert.IsTrue(response.Command.useDefaultConfig);
            Assert.IsTrue(response.Command.requiredForPlay);
            Assert.AreEqual(response.Command.minLogEntries, 2);
            Assert.AreEqual(response.Command.timeToLive, 3);
            Assert.AreEqual(response.Command.queueBehavior, t_queueBehaviors.G2S_overwrite);
            Assert.AreEqual(response.Command.disableBehavior, t_disableBehaviors.G2S_discard);
            Assert.IsTrue(response.Command.configComplete);
            Assert.IsTrue(
                response.Command.forcedSubscription.Length == 1
                && response.Command.forcedSubscription.First().deviceId == 1);
            Assert.AreEqual(response.Command.configDateTime, dateTime);
            Assert.IsTrue(response.Command.configComplete);
        }
    }
}