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
    using Data.Model;
    using G2S.Handlers.Events;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetSupportedEventsTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetSupportedEvents(null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var eventPersistenceManager = new Mock<IEventPersistenceManager>();
            var handler = new GetSupportedEvents(egm.Object, eventPersistenceManager.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var eventPersistenceManager = new Mock<IEventPersistenceManager>();
            var handler = new GetSupportedEvents(egm.Object, eventPersistenceManager.Object);
            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var egm = EventsUtiliites.CreateMockEgm();
            var eventPersistenceManager = new Mock<IEventPersistenceManager>();
            var handler = new GetSupportedEvents(egm, eventPersistenceManager.Object);
            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var evtDevice = new Mock<IEventHandlerDevice>();
            var egm = EventsUtiliites.CreateMockEgm(evtDevice);
            var eventPersistenceManager = new Mock<IEventPersistenceManager>();
            evtDevice.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var evt = new eventHandler
            {
                deviceId = TestConstants.HostId,
                timeToLive = TestConstants.TimeToLive,
                dateTime = DateTime.UtcNow
            };
            var command = ClassCommandUtilities.CreateClassCommand<eventHandler, getSupportedEvents>(
                evt,
                TestConstants.HostId,
                TestConstants.EgmId);

            var handler = new GetSupportedEvents(egm, eventPersistenceManager.Object);
            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenEmgNotReturnDeviceExpectNoResponse()
        {
            var egm = new Mock<IG2SEgm>();
            var eventPersistenceManager = new Mock<IEventPersistenceManager>();
            var handler = new GetSupportedEvents(egm.Object, eventPersistenceManager.Object);

            var command =
                ClassCommandUtilities.CreateClassCommand<eventHandler, getSupportedEvents>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            await handler.Handle(command);

            Assert.AreEqual(command.Responses.Count(), 0);
        }

        [TestMethod]
        public async Task WhenGetAllEventsOnHandleCommandExpectResponse()
        {
            var egm = new Mock<IG2SEgm>();
            var device = new Mock<IEventHandlerDevice>();

            egm.Setup(e => e.GetDevice<IEventHandlerDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);

            var eventPersistenceManager = new Mock<IEventPersistenceManager>();
            eventPersistenceManager.SetupGet(evt => evt.SupportedEvents)
                .Returns((IReadOnlyCollection<object>)GetSupportedEvents());

            var command =
                ClassCommandUtilities.CreateClassCommand<eventHandler, getSupportedEvents>(
                    TestConstants.HostId,
                    TestConstants.EgmId);
            command.Command.deviceId = -1;
            command.Command.deviceClass = "G2S_all";

            var handler = new GetSupportedEvents(egm.Object, eventPersistenceManager.Object);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<eventHandler, supportedEvents>;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.Command.supportedEvent.Length, 6);
            Assert.IsTrue(response.Command.supportedEvent.Select(x => x.deviceClass).Contains("G2S_cabinet"));
            Assert.IsTrue(response.Command.supportedEvent.Select(x => x.deviceClass).Contains("G2S_eventHandler"));
            Assert.IsTrue(response.Command.supportedEvent.Select(x => x.deviceClass).Contains("G2S_meters"));
        }

        [TestMethod]
        public async Task WhenFilterByDeviceClassOnHandleCommandExpectResponse()
        {
            var egm = new Mock<IG2SEgm>();
            var device = new Mock<IEventHandlerDevice>();

            egm.Setup(e => e.GetDevice<IEventHandlerDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);

            var eventPersistenceManager = new Mock<IEventPersistenceManager>();
            eventPersistenceManager.SetupGet(evt => evt.SupportedEvents)
                .Returns((IReadOnlyCollection<object>)GetSupportedEvents());

            var command =
                ClassCommandUtilities.CreateClassCommand<eventHandler, getSupportedEvents>(
                    TestConstants.HostId,
                    TestConstants.EgmId);
            command.Command.deviceId = -1;
            command.Command.deviceClass = "G2S_cabinet";

            var handler = new GetSupportedEvents(egm.Object, eventPersistenceManager.Object);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<eventHandler, supportedEvents>;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.Command.supportedEvent.Length, 2);
            Assert.IsTrue(response.Command.supportedEvent.Select(x => x.deviceClass).Contains("G2S_cabinet"));
            Assert.AreEqual(response.Command.supportedEvent.First().deviceId, 1);
            Assert.AreEqual(response.Command.supportedEvent.Last().deviceId, 2);
        }

        [TestMethod]
        public async Task WhenFilterByDeviceIdOnHandleCommandExpectResponse()
        {
            var egm = new Mock<IG2SEgm>();
            var device = new Mock<IEventHandlerDevice>();

            egm.Setup(e => e.GetDevice<IEventHandlerDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);
            var eventPersistenceManager = new Mock<IEventPersistenceManager>();
            eventPersistenceManager.SetupGet(evt => evt.SupportedEvents)
                .Returns((IReadOnlyCollection<object>)GetSupportedEvents());

            var command =
                ClassCommandUtilities.CreateClassCommand<eventHandler, getSupportedEvents>(
                    TestConstants.HostId,
                    TestConstants.EgmId);
            command.Command.deviceId = 1;
            command.Command.deviceClass = "G2S_all";

            var handler = new GetSupportedEvents(egm.Object, eventPersistenceManager.Object);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<eventHandler, supportedEvents>;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.Command.supportedEvent.Length, 1);
            Assert.IsTrue(response.Command.supportedEvent.Select(x => x.deviceClass).Contains("G2S_cabinet"));
            Assert.AreEqual(response.Command.supportedEvent.First().deviceId, 1);
        }

        [TestMethod]
        public async Task WhenFilterByDeviceIdAndDeviceClassOnHandleCommandExpectResponse()
        {
            var egm = new Mock<IG2SEgm>();
            var device = new Mock<IEventHandlerDevice>();
            egm.Setup(e => e.GetDevice<IEventHandlerDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);

            var eventPersistenceManager = new Mock<IEventPersistenceManager>();
            eventPersistenceManager.SetupGet(evt => evt.SupportedEvents)
                .Returns((IReadOnlyCollection<object>)GetSupportedEvents());

            var command =
                ClassCommandUtilities.CreateClassCommand<eventHandler, getSupportedEvents>(
                    TestConstants.HostId,
                    TestConstants.EgmId);
            command.Command.deviceId = 5;
            command.Command.deviceClass = "G2S_meters";

            var handler = new GetSupportedEvents(egm.Object, eventPersistenceManager.Object);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<eventHandler, supportedEvents>;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.Command.supportedEvent.Length, 1);
            Assert.IsTrue(response.Command.supportedEvent.Select(x => x.deviceClass).Contains("G2S_meters"));
            Assert.AreEqual(response.Command.supportedEvent.First().deviceId, 5);
        }

        private IEnumerable<SupportedEvent> GetSupportedEvents()
        {
            return new List<SupportedEvent>
            {
                new SupportedEvent
                {
                    DeviceId = 1,
                    DeviceClass = "G2S_cabinet",
                    EventCode = EventCode.G2S_APE001
                },
                new SupportedEvent
                {
                    DeviceId = 2,
                    DeviceClass = "G2S_cabinet",
                    EventCode = EventCode.G2S_APE001
                },
                new SupportedEvent
                {
                    DeviceId = 3,
                    DeviceClass = "G2S_eventHandler",
                    EventCode = EventCode.G2S_APE001
                },
                new SupportedEvent
                {
                    DeviceId = 4,
                    DeviceClass = "G2S_eventHandler",
                    EventCode = EventCode.G2S_APE001
                },
                new SupportedEvent
                {
                    DeviceId = 5,
                    DeviceClass = "G2S_meters",
                    EventCode = EventCode.G2S_APE001
                },
                new SupportedEvent
                {
                    DeviceId = 6,
                    DeviceClass = "G2S_meters",
                    EventCode = EventCode.G2S_APE001
                }
            };
        }
    }
}