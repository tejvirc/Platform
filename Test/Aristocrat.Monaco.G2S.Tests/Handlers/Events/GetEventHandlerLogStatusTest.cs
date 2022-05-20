namespace Aristocrat.Monaco.G2S.Tests.Handlers.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
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
    public class GetEventHandlerLogStatusTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetEventHandlerLogStatus(null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var contextFactory = new Mock<IMonacoContextFactory>();
            var repo = new Mock<IEventHandlerLogRepository>();
            var handler = new GetEventHandlerLogStatus(egm.Object, repo.Object, contextFactory.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var contextFactory = new Mock<IMonacoContextFactory>();
            var repo = new Mock<IEventHandlerLogRepository>();
            var handler = new GetEventHandlerLogStatus(egm.Object, repo.Object, contextFactory.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var egm = EventsUtiliites.CreateMockEgm();
            var contextFactory = new Mock<IMonacoContextFactory>();
            var repo = new Mock<IEventHandlerLogRepository>();
            var handler = new GetEventHandlerLogStatus(egm, repo.Object, contextFactory.Object);

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var evtDevice = new Mock<IEventHandlerDevice>();
            var egm = EventsUtiliites.CreateMockEgm(evtDevice);

            evtDevice.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var evt = new eventHandler
            {
                deviceId = TestConstants.HostId,
                timeToLive = TestConstants.TimeToLive,
                dateTime = DateTime.UtcNow
            };
            var command = ClassCommandUtilities.CreateClassCommand<eventHandler, getEventHandlerLogStatus>(
                evt,
                TestConstants.HostId,
                TestConstants.EgmId);

            var contextFactory = new Mock<IMonacoContextFactory>();
            var repo = new Mock<IEventHandlerLogRepository>();
            var handler = new GetEventHandlerLogStatus(egm, repo.Object, contextFactory.Object);

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenNullEventLogsHandleCommandExpectResponse()
        {
            var egm = new Mock<IG2SEgm>();
            var eventDevice = new Mock<IEventHandlerDevice>();
            var queue = new Mock<ICommandQueue>();
            var contextFactory = EventsUtiliites.CreateMonacoContextFactory();
            var repo = new Mock<IEventHandlerLogRepository>();

            repo.Setup(a => a.Count(contextFactory.Create())).Returns(0);
            repo.Setup(a => a.GetMaxLastSequence<EventHandlerLog>(contextFactory.Create())).Returns(0);

            queue.SetupGet(q => q.TimeToLiveBehavior).Returns(TimeToLiveBehavior.Strict);
            eventDevice.SetupGet(evt => evt.Queue).Returns(queue.Object);
            eventDevice.SetupGet(evt => evt.Owner).Returns(TestConstants.HostId);
            egm.Setup(e => e.GetDevice<IEventHandlerDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(eventDevice.Object);

            var command =
                ClassCommandUtilities.CreateClassCommand<eventHandler, getEventHandlerLogStatus>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            var handler = new GetEventHandlerLogStatus(egm.Object, repo.Object, contextFactory);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<eventHandler, eventHandlerLogStatus>;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.Command.lastSequence, 0);
            Assert.AreEqual(response.Command.totalEntries, 0);
        }

        [TestMethod]
        public async Task WhenNotNullEventLogsHandleCommandExpectResponse()
        {
            var egm = new Mock<IG2SEgm>();
            var eventDevice = new Mock<IEventHandlerDevice>();
            var contextFactory = EventsUtiliites.CreateMonacoContextFactory();
            var repo = new Mock<IEventHandlerLogRepository>();

            repo.Setup(a => a.Count(contextFactory.Create(), It.IsAny<Expression<Func<EventHandlerLog, bool>>>())).Returns(1);
            repo.Setup(a => a.GetMaxLastSequence<EventHandlerLog>(contextFactory.Create(), It.IsAny<Expression<Func<EventHandlerLog, bool>>>())).Returns(1);

            var queue = new Mock<ICommandQueue>();

            queue.SetupGet(q => q.TimeToLiveBehavior).Returns(TimeToLiveBehavior.Strict);
            eventDevice.SetupGet(evt => evt.Queue).Returns(queue.Object);
            eventDevice.SetupGet(evt => evt.Owner).Returns(TestConstants.HostId);
            egm.Setup(e => e.GetDevice<IEventHandlerDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(eventDevice.Object);

            var command =
                ClassCommandUtilities.CreateClassCommand<eventHandler, getEventHandlerLogStatus>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            var handler = new GetEventHandlerLogStatus(egm.Object, repo.Object, contextFactory);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<eventHandler, eventHandlerLogStatus>;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.Command.lastSequence, 1);
            Assert.AreEqual(response.Command.totalEntries, 1);
        }
    }
}