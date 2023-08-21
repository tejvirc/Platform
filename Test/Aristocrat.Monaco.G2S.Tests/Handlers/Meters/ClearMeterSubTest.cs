namespace Aristocrat.Monaco.G2S.Tests.Handlers.Meters
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using G2S.Handlers.Meters;
    using G2S.Meters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ClearMeterSubTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new ClearMeterSub(null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullMetersSubscripionManagerExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var handler = new ClearMeterSub(egm.Object, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEventLiftExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var handler = new ClearMeterSub(egm.Object, meterSubManager.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var eventLift = new Mock<IEventLift>();
            var handler = new ClearMeterSub(egm.Object, meterSubManager.Object, eventLift.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var eventLift = new Mock<IEventLift>();
            var handler = new ClearMeterSub(egm.Object, meterSubManager.Object, eventLift.Object);
            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var queue = new Mock<ICommandQueue>();
            var device = new Mock<IMetersDevice>();
            var eventLift = new Mock<IEventLift>();
            queue.SetupGet(q => q.TimeToLiveBehavior).Returns(TimeToLiveBehavior.Strict);
            device.SetupGet(evt => evt.Queue).Returns(queue.Object);
            egm.Setup(e => e.GetDevice<IMetersDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);

            var handler = new ClearMeterSub(egm.Object, meterSubManager.Object, eventLift.Object);
            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var egm = new Mock<IG2SEgm>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var device = new Mock<IMetersDevice>();
            var queue = new Mock<ICommandQueue>();
            var eventLift = new Mock<IEventLift>();
            queue.SetupGet(q => q.TimeToLiveBehavior).Returns(TimeToLiveBehavior.Strict);
            device.SetupGet(comms => comms.Queue).Returns(queue.Object);
            device.SetupGet(evt => evt.Owner).Returns(TestConstants.HostId);
            egm.Setup(e => e.GetDevice<IMetersDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);

            var handler = new ClearMeterSub(egm.Object, meterSubManager.Object, eventLift.Object);
            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenSessionTypeIsG2SNotificationExpectNoResult()
        {
            var egm = new Mock<IG2SEgm>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var eventLift = new Mock<IEventLift>();
            var handler = new ClearMeterSub(egm.Object, meterSubManager.Object, eventLift.Object);

            var command = CreateCommand();

            command.Class.sessionType = t_sessionTypes.G2S_notification;

            await handler.Handle(command);

            Assert.AreEqual(0, command.Responses.Count());
        }

        [TestMethod]
        public async Task WhenEgmNotReturnDeviceExpectNoResult()
        {
            var egm = new Mock<IG2SEgm>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var eventLift = new Mock<IEventLift>();
            var handler = new ClearMeterSub(egm.Object, meterSubManager.Object, eventLift.Object);

            var command = CreateCommand();

            await handler.Handle(command);

            Assert.AreEqual(0, command.Responses.Count());
        }

        [TestMethod]
        public async Task WhenMeterExistsExpectResponse()
        {
            var egm = new Mock<IG2SEgm>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var eventLift = new Mock<IEventLift>();
            meterSubManager.Setup(x => x.ClearSubscriptions(1, MetersSubscriptionType.Periodic))
                .Returns(
                    new MeterSubscription
                    {
                        OnCoinDrop = true,
                        OnDoorOpen = true,
                        OnEndOfDay = true,
                        OnNoteDrop = true,
                        Base = 11,
                        SubType = MetersSubscriptionType.Periodic,
                        PeriodInterval = 77777
                    });

            var device = new Mock<IMetersDevice>();
            egm.Setup(e => e.GetDevice<IMetersDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);

            var handler = new ClearMeterSub(egm.Object, meterSubManager.Object, eventLift.Object);

            var command = CreateCommand();
            command.Command.meterSubType = "G2S_onPeriodic";

            await handler.Handle(command);

            meterSubManager.Verify(x => x.ClearSubscriptions(1, MetersSubscriptionType.Periodic), Times.Once);

            var response = command.Responses.FirstOrDefault() as ClassCommand<meters, meterSubList>;

            Assert.IsNotNull(response);
            Assert.IsTrue(response.Command.listStateDateTime != default(DateTime));
            Assert.IsTrue(response.Command.listStateDateTimeSpecified);
            Assert.AreEqual(response.Command.meterSubType, command.Command.meterSubType);
            Assert.IsTrue(response.Command.onCoinDrop);
            Assert.IsTrue(response.Command.onDoorOpen);
            Assert.IsTrue(response.Command.onEOD);
            Assert.IsTrue(response.Command.onNoteDrop);
            Assert.AreEqual(response.Command.eodBase, 0);
            Assert.AreEqual(response.Command.periodicBase, 11);
            Assert.AreEqual(response.Command.periodicInterval, 77777);
        }

        [TestMethod]
        public async Task WhenMeterNotExistExpectResponse()
        {
            var egm = new Mock<IG2SEgm>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();

            meterSubManager.Setup(x => x.ClearSubscriptions(1, MetersSubscriptionType.Periodic));
            var eventLift = new Mock<IEventLift>();
            var device = new Mock<IMetersDevice>();
            egm.Setup(e => e.GetDevice<IMetersDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);

            var handler = new ClearMeterSub(egm.Object, meterSubManager.Object, eventLift.Object);

            var command = CreateCommand();
            command.Command.meterSubType = "G2S_onPeriodic";

            await handler.Handle(command);

            meterSubManager.Verify(x => x.ClearSubscriptions(1, MetersSubscriptionType.Periodic), Times.Once);

            var response = command.Responses.FirstOrDefault() as ClassCommand<meters, meterSubList>;

            Assert.IsNotNull(response);
            Assert.IsTrue(response.Command.listStateDateTime != default(DateTime));
            Assert.IsTrue(response.Command.listStateDateTimeSpecified);
            Assert.AreEqual(response.Command.meterSubType, command.Command.meterSubType);

            Assert.IsFalse(response.Command.onCoinDrop);
            Assert.IsFalse(response.Command.onDoorOpen);
            Assert.IsFalse(response.Command.onEOD);
            Assert.IsFalse(response.Command.onNoteDrop);
            Assert.AreEqual(response.Command.eodBase, 0);
            Assert.AreEqual(response.Command.periodicBase, 0);
            Assert.AreEqual(response.Command.periodicInterval, 90000);
        }

        private ClassCommand<meters, clearMeterSub> CreateCommand()
        {
            var command = ClassCommandUtilities.CreateClassCommand<meters, clearMeterSub>(
                TestConstants.HostId,
                TestConstants.EgmId);

            return command;
        }
    }
}