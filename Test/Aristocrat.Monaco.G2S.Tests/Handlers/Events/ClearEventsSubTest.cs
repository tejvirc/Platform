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
    public class ClearEventsSubTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new ClearEventSub(null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEventLiftExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var handler = new ClearEventSub(egm.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var eventLift = new Mock<IEventLift>();
            var handler = new ClearEventSub(egm.Object, eventLift.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var eventLift = new Mock<IEventLift>();
            var handler = new ClearEventSub(egm.Object, eventLift.Object);
            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var egm = EventsUtiliites.CreateMockEgm();
            var eventLift = new Mock<IEventLift>();
            var handler = new ClearEventSub(egm, eventLift.Object);
            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var egm = new Mock<IG2SEgm>();
            var evtDevice = new Mock<IEventHandlerDevice>();
            var queue = new Mock<ICommandQueue>();
            var eventLift = new Mock<IEventLift>();
            queue.SetupGet(q => q.TimeToLiveBehavior).Returns(TimeToLiveBehavior.Strict);
            evtDevice.SetupGet(comms => comms.Queue).Returns(queue.Object);
            evtDevice.SetupGet(evt => evt.Owner).Returns(TestConstants.HostId);
            egm.Setup(e => e.GetDevice<IEventHandlerDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(evtDevice.Object);

            var handler = new ClearEventSub(egm.Object, eventLift.Object);
            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenEmgNotReturnDeviceExpectNoResponse()
        {
            var egm = new Mock<IG2SEgm>();
            var eventLift = new Mock<IEventLift>();
            var handler = new ClearEventSub(egm.Object, eventLift.Object);

            var command = CreateCommand();

            await handler.Handle(command);

            Assert.AreEqual(command.Responses.Count(), 0);
        }

        [TestMethod]
        public async Task WhenHandleCommandNormalExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var eventLift = new Mock<IEventLift>();
            var device = new Mock<IEventHandlerDevice>();
            device.Setup(x => x.GetAllRegisteredEventSub()).Returns(
                new List<eventHostSubscription>
                {
                    new eventHostSubscription(),
                    new eventHostSubscription
                    {
                        deviceClass = "G2S_cabinet"
                    },
                    new eventHostSubscription
                    {
                        deviceClass = "G2S_cabinet",
                        deviceId = 1,
                        eventCode = "G2S_all"
                    },
                    new eventHostSubscription
                    {
                        deviceClass = "G2S_cabinet",
                        deviceId = 1,
                        eventCode = "G2S_APE001"
                    }
                });
            egm.Setup(e => e.GetDevice<IEventHandlerDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);

            var handler = new ClearEventSub(egm.Object, eventLift.Object);

            var command = CreateCommand();

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<eventHandler, clearEventSubAck>;

            Assert.IsNotNull(response);

            Assert.IsTrue(response.Command.listStateDateTime > (new DateTime()));

            device.Verify(
                x =>
                    x.RemoveRegisteredEventSubscriptions(
                        It.Is<IEnumerable<eventHostSubscription>>(
                            sub => sub.Any())),
                Times.Once);
        }

        private ClassCommand<eventHandler, clearEventSub> CreateCommand()
        {
            var command = ClassCommandUtilities.CreateClassCommand<eventHandler, clearEventSub>(
                TestConstants.HostId,
                TestConstants.EgmId);

            command.Command.eventSelect = new List<eventSelect>
            {
                new eventSelect
                {
                    deviceClass = "G2S_cabinet",
                    deviceId = 1,
                    eventCode = "G2S_APE001"
                }
            }.ToArray();

            return command;
        }
    }
}