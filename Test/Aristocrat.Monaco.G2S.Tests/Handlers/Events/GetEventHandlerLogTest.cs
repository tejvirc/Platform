namespace Aristocrat.Monaco.G2S.Tests.Handlers.Events
{
    using System;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using System.Xml.Serialization;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Data.EventHandler;
    using Data.Model;
    using G2S.Handlers.Events;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;

    [TestClass]
    public class GetEventHandlerLogTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetEventHandlerLog(null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullContextExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new GetEventHandlerLog(egm.Object, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullRepositoryExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var context = new Mock<IMonacoContextFactory>();

            var handler = new GetEventHandlerLog(egm.Object, context.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var context = new Mock<IMonacoContextFactory>();
            var repo = new Mock<IEventHandlerLogRepository>();

            var handler = new GetEventHandlerLog(egm.Object, context.Object, repo.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var context = new Mock<IMonacoContextFactory>();
            var repo = new Mock<IEventHandlerLogRepository>();
            var handler = new GetEventHandlerLog(egm.Object, context.Object, repo.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var egm = EventsUtiliites.CreateMockEgm();
            var context = new Mock<IMonacoContextFactory>();
            var repo = new Mock<IEventHandlerLogRepository>();

            var handler = new GetEventHandlerLog(egm, context.Object, repo.Object);
            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var evtDevice = new Mock<IEventHandlerDevice>();
            var egm = EventsUtiliites.CreateMockEgm(evtDevice);
            var context = new Mock<IMonacoContextFactory>();
            var repo = new Mock<IEventHandlerLogRepository>();
            evtDevice.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var evt = new eventHandler
            {
                deviceId = TestConstants.HostId,
                timeToLive = TestConstants.TimeToLive,
                dateTime = DateTime.UtcNow
            };
            var command = ClassCommandUtilities.CreateClassCommand<eventHandler, getEventHandlerLog>(
                evt,
                TestConstants.HostId,
                TestConstants.EgmId);

            var handler = new GetEventHandlerLog(egm, context.Object, repo.Object);
            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenEmgNotReturnDeviceExpectNoResponse()
        {
            var egm = new Mock<IG2SEgm>();
            var context = new Mock<IMonacoContextFactory>();
            var repo = new Mock<IEventHandlerLogRepository>();
            var handler = new GetEventHandlerLog(egm.Object, context.Object, repo.Object);

            var command = ClassCommandUtilities.CreateClassCommand<eventHandler, getEventHandlerLog>(
                TestConstants.HostId,
                TestConstants.EgmId);

            await handler.Handle(command);

            Assert.AreEqual(command.Responses.Count(), 0);
        }

        [TestMethod]
        public async Task WhenHandleCommandWithZeroLastSequenceExpectResponse()
        {
            var eventDevice = new Mock<IEventHandlerDevice>();
            var egm = EventsUtiliites.CreateMockEgm(eventDevice);

            var command = ClassCommandUtilities.CreateClassCommand<eventHandler, getEventHandlerLog>(
                TestConstants.HostId,
                TestConstants.EgmId);
            var context = new Mock<IMonacoContextFactory>();
            var repo = new Mock<IEventHandlerLogRepository>();

            var logs = (new List<EventHandlerLog>()
            {
                new EventHandlerLog
                {
                    Id = 2,
                    DeviceClass = "G2S_download",
                    DeviceId = 1,
                    EventCode = "G2S_DLX003",
                    EventDateTime = DateTime.UtcNow,
                    EventAck = false,
                    EventId = 1,
                    TransactionId = 1
                },
            }).AsQueryable();

            repo.Setup(s => s.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<EventHandlerLog, bool>>>())).Returns(logs);

            var handler = new GetEventHandlerLog(egm, context.Object, repo.Object);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<eventHandler, eventHandlerLogList>;

            Assert.IsNotNull(response);

            Assert.AreEqual(response.Command.eventHandlerLog.Length, 1);
        }

        [TestMethod]
        public async Task WhenHandleCommandWithLastSequenceMoreZeroAndZeroTotalEntriesExpectResponse()
        {
            var eventDevice = new Mock<IEventHandlerDevice>();
            var egm = EventsUtiliites.CreateMockEgm(eventDevice);
            var context = new Mock<IMonacoContextFactory>();
            var repo = new Mock<IEventHandlerLogRepository>();
            var command = ClassCommandUtilities.CreateClassCommand<eventHandler, getEventHandlerLog>(
                TestConstants.HostId,
                TestConstants.EgmId);
            command.Command.lastSequence = 2;

            var logs = (new List<EventHandlerLog>()
            {
                new EventHandlerLog
                {
                    Id = 2,
                    DeviceClass = "G2S_download",
                    DeviceId = 1,
                    EventCode = "G2S_DLX003",
                    EventDateTime = DateTime.UtcNow,
                    EventAck = false,
                    EventId = 1,
                    TransactionId = 1
                },
                new EventHandlerLog
                {
                    Id = 3,
                    DeviceClass = "G2S_download",
                    DeviceId = 1,
                    EventCode = "G2S_DLX003",
                    EventDateTime = DateTime.UtcNow,
                    EventAck = false,
                    EventId = 1,
                    TransactionId = 1
                },
            }).AsQueryable();

            repo.Setup(s => s.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<EventHandlerLog, bool>>>())).Returns(logs);

            var handler = new GetEventHandlerLog(egm, context.Object, repo.Object);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<eventHandler, eventHandlerLogList>;

            Assert.IsNotNull(response);

            Assert.AreEqual(response.Command.eventHandlerLog.Length, 1);
            Assert.AreEqual(response.Command.eventHandlerLog[0].logSequence, 2);
        }
    }
}