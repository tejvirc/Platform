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
    public class SetEventSubTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new SetEventSub(null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEventLiftExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var handler = new SetEventSub(egm.Object, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var eventLift = new Mock<IEventLift>();
            var eventPersistenceManager = new Mock<IEventPersistenceManager>();

            var handler = new SetEventSub(egm.Object, eventLift.Object, eventPersistenceManager.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var eventLift = new Mock<IEventLift>();
            var eventPersistenceManager = new Mock<IEventPersistenceManager>();
            
            var handler = new SetEventSub(egm.Object, eventLift.Object, eventPersistenceManager.Object);
            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var egm = EventsUtiliites.CreateMockEgm();
            var eventLift = new Mock<IEventLift>();
            var eventPersistenceManager = new Mock<IEventPersistenceManager>();

            var handler = new SetEventSub(egm, eventLift.Object, eventPersistenceManager.Object);
            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var egm = new Mock<IG2SEgm>();
            var evtDevice = new Mock<IEventHandlerDevice>();
            var queue = new Mock<ICommandQueue>();
            var eventLift = new Mock<IEventLift>();

            var eventPersistenceManager = new Mock<IEventPersistenceManager>();

            queue.SetupGet(q => q.TimeToLiveBehavior).Returns(TimeToLiveBehavior.Strict);
            evtDevice.SetupGet(evt => evt.Queue).Returns(queue.Object);
            evtDevice.SetupGet(evt => evt.Owner).Returns(TestConstants.HostId);
            egm.Setup(e => e.GetDevice<IEventHandlerDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(evtDevice.Object);

            var handler = new SetEventSub(egm.Object, eventLift.Object, eventPersistenceManager.Object);
            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenEmgNotReturnDeviceExpectNoResponse()
        {
            var egm = new Mock<IG2SEgm>();
            var eventLift = new Mock<IEventLift>();
            var eventPersistenceManager = new Mock<IEventPersistenceManager>();

            var handler = new SetEventSub(egm.Object, eventLift.Object, eventPersistenceManager.Object);

            var command =
                ClassCommandUtilities.CreateClassCommand<eventHandler, setEventSub>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            await handler.Handle(command);

            Assert.AreEqual(command.Responses.Count(), 0);
        }

        [TestMethod]
        public async Task WhenHandleCommandExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var device = new Mock<IEventHandlerDevice>();
            var eventPersistenceManager = new Mock<IEventPersistenceManager>();

            eventPersistenceManager.SetupGet(x => x.SupportedEvents).Returns(
                new List<SupportedEvent>
                {
                    new SupportedEvent
                    {
                        DeviceClass = "G2S_cabinet"
                    }
                });

            egm.Setup(e => e.GetDevice<IEventHandlerDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);
            var eventLift = new Mock<IEventLift>();
            var command = ClassCommandUtilities.CreateClassCommand<eventHandler, setEventSub>(
                TestConstants.HostId,
                TestConstants.EgmId);
            command.Command.eventHostSubscription = new List<eventHostSubscription>
            {
                new eventHostSubscription
                {
                    deviceClass = "G2S_eventHandler"
                }
            }.ToArray();

            var handler = new SetEventSub(egm.Object, eventLift.Object, eventPersistenceManager.Object);
            await handler.Handle(command);

            Assert.IsTrue(command.Error.IsError);
            Assert.AreEqual(command.Error.Code, ErrorCode.G2S_EHX001);
        }

        [TestMethod]
        public async Task WhenHandleCommandExpectErrorCaseTwo()
        {
            var egm = new Mock<IG2SEgm>();
            var device = new Mock<IEventHandlerDevice>();
            var eventPersistenceManager = new Mock<IEventPersistenceManager>();

            eventPersistenceManager.SetupGet(x => x.SupportedEvents).Returns(
                new List<SupportedEvent>
                {
                    new SupportedEvent
                    {
                        DeviceClass = "G2S_cabinet"
                    }
                });

            egm.Setup(e => e.GetDevice<IEventHandlerDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);
            var eventLift = new Mock<IEventLift>();
            var command = ClassCommandUtilities.CreateClassCommand<eventHandler, setEventSub>(
                TestConstants.HostId,
                TestConstants.EgmId);
            command.Command.eventHostSubscription = new List<eventHostSubscription>().ToArray();

            var handler = new SetEventSub(egm.Object, eventLift.Object, eventPersistenceManager.Object);
            await handler.Handle(command);

            Assert.IsTrue(command.Error.IsError);
            Assert.AreEqual(command.Error.Code, ErrorCode.G2S_EHX003);
        }
    }
}