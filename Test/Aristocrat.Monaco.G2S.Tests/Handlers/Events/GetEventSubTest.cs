namespace Aristocrat.Monaco.G2S.Tests.Handlers.Events
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Serialization;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using G2S.Handlers.Events;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetEventSubTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetEventSub(null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new GetEventSub(egm.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new GetEventSub(egm.Object);
            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var egm = EventsUtiliites.CreateMockEgm();
            var handler = new GetEventSub(egm);
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
            ClassCommandUtilities.CreateClassCommand<eventHandler, getEventSub>(
                evt,
                TestConstants.HostId,
                TestConstants.EgmId);

            var handler = new GetEventSub(egm);
            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenEmgNotReturnDeviceExpectNoResponse()
        {
            var egm = new Mock<IG2SEgm>();
            var handler = new GetEventSub(egm.Object);

            var command =
                ClassCommandUtilities.CreateClassCommand<eventHandler, getEventSub>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            await handler.Handle(command);

            Assert.AreEqual(command.Responses.Count(), 0);
        }

        [TestMethod]
        public async Task WhenHandleCommandExpectResponse()
        {
            var command = ClassCommandUtilities.CreateClassCommand<eventHandler, getEventSub>(
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

            var egm = new Mock<IG2SEgm>();
            var device = new Mock<IEventHandlerDevice>();
            egm.Setup(e => e.GetDevice<IEventHandlerDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);
            device.Setup(x => x.GetAllEventSubscriptions())
                .Returns(
                    new List<object>
                    {
                        new EventSubscription
                        {
                            DeviceId = 1,
                            EventCode = "G2S_all",
                            SubType = EventSubscriptionType.Forced
                        },
                        new EventSubscription
                        {
                            DeviceId = 1,
                            EventCode = "G2S_APE001",
                            SubType = EventSubscriptionType.Forced,
                            DeviceClass = "G2S_cabinet"
                        },
                        new EventSubscription
                        {
                            DeviceId = 1,
                            EventCode = "G2S_all",
                            SubType = EventSubscriptionType.Host,
                            DeviceClass = "G2S_cabinet"
                        },
                        new EventSubscription
                        {
                            DeviceId = 1,
                            EventCode = "G2S_APE001",
                            SubType = EventSubscriptionType.Host,
                            DeviceClass = "G2S_cabinet"
                        }
                    });

            var handler = new GetEventSub(egm.Object);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<eventHandler, eventSubList>;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.Command.eventSubscription.Length, 2);
            Assert.IsTrue(response.Command.eventSubscription.First().deviceId == 1);
            Assert.IsTrue(response.Command.eventSubscription.Last().deviceId == 1);
        }
    }
}