namespace Aristocrat.Monaco.G2S.Tests.Handlers.Meters
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
    using G2S.Handlers.Meters;
    using G2S.Meters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using DeviceClass = Aristocrat.G2S.DeviceClass;

    [TestClass]
    public class SetMeterSubTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new SetMeterSub(null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullMeterSubscriptionManagerExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new SetMeterSub(egm.Object, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEventLiftExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var handler = new SetMeterSub(egm.Object, meterSubManager.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var eventLift = new Mock<IEventLift>();
            var handler = new SetMeterSub(egm.Object, meterSubManager.Object, eventLift.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();

            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var eventLift = new Mock<IEventLift>();
            var handler = new SetMeterSub(egm.Object, meterSubManager.Object, eventLift.Object);
            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var queue = new Mock<ICommandQueue>();
            var eventLift = new Mock<IEventLift>();
            var device = new Mock<IMetersDevice>();
            queue.SetupGet(q => q.TimeToLiveBehavior).Returns(TimeToLiveBehavior.Strict);
            device.SetupGet(evt => evt.Queue).Returns(queue.Object);
            egm.Setup(e => e.GetDevice<IMetersDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);

            var handler = new SetMeterSub(egm.Object, meterSubManager.Object, eventLift.Object);
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

            var handler = new SetMeterSub(egm.Object, meterSubManager.Object, eventLift.Object);
            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenSessionTypeIsG2SNotificationExpectNoResult()
        {
            var egm = new Mock<IG2SEgm>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var eventLift = new Mock<IEventLift>();
            var handler = new SetMeterSub(egm.Object, meterSubManager.Object, eventLift.Object);

            var command = CreateCommand();

            command.Class.sessionType = t_sessionTypes.G2S_notification;

            await handler.Handle(command);

            Assert.AreEqual(0, command.Responses.Count());

            meterSubManager.Verify(
                x =>
                    x.SetMetersSubscription(
                        It.IsAny<int>(),
                        It.IsAny<MetersSubscriptionType>(),
                        It.IsAny<IList<MeterSubscription>>()),
                Times.Never);
        }

        [TestMethod]
        public async Task WhenMeterSubExistsExpectSuccess()
        {
            var device = new Mock<IMetersDevice>();
            device.SetupGet(d => d.DeviceClass).Returns("meters");
            var command = ClassCommandUtilities.CreateClassCommand<meters, setMeterSub>(
                TestConstants.HostId,
                TestConstants.EgmId);
            var eventLift = new Mock<IEventLift>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();

            meterSubManager.Setup(x => x.GetMeterSub(1, MetersSubscriptionType.EndOfDay))
                .Returns(CreateMeterSubscriptions().AsQueryable());

            var handler = new SetMeterSub(
                HandlerUtilities.CreateMockEgm(device),
                meterSubManager.Object,
                eventLift.Object);

            command.Command.meterSubType = MeterSubscriptionType.EndOfDay;
            FillMeters(command);

            await handler.Handle(command);

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
            Assert.AreEqual(response.Command.meterSubType, command.Command.meterSubType);

            Assert.AreEqual(response.Command.getCurrencyMeters.Length, 1);
            Assert.IsTrue(
                response.Command.getCurrencyMeters.First().deviceClass == DeviceClass.G2S_cabinet
                && response.Command.getCurrencyMeters.First().deviceId == 1);

            Assert.AreEqual(response.Command.getDeviceMeters.Length, 1);
            Assert.IsTrue(
                response.Command.getDeviceMeters.First().deviceClass == DeviceClass.G2S_coinAcceptor
                && response.Command.getDeviceMeters.First().deviceId == 2);

            Assert.AreEqual(response.Command.getGameDenomMeters.Length, 1);
            Assert.IsTrue(
                response.Command.getGameDenomMeters.First().deviceClass == DeviceClass.G2S_eventHandler
                && response.Command.getGameDenomMeters.First().deviceId == 3);

            Assert.AreEqual(response.Command.getWagerMeters.Length, 1);
            Assert.IsTrue(
                response.Command.getWagerMeters.First().deviceClass == DeviceClass.G2S_hopper
                && response.Command.getWagerMeters.First().deviceId == 4);

            meterSubManager.Verify(
                x =>
                    x.SetMetersSubscription(
                        1,
                        MetersSubscriptionType.EndOfDay,
                        It.Is<IList<MeterSubscription>>(
                            l => l.Select(item => item.ClassName).Contains(DeviceClass.G2S_eventHandler)
                                 && l.Select(item => item.ClassName).Contains(DeviceClass.G2S_hopper)
                                 && l.Select(item => item.ClassName).Contains(DeviceClass.G2S_gat)
                                 && l.Select(item => item.ClassName).Contains(DeviceClass.G2S_progressive)
                        )));
        }

        [TestMethod]
        public async Task WhenMeterSubNotExistExpectSuccess()
        {
            var device = new Mock<IMetersDevice>();
            device.SetupGet(d => d.DeviceClass).Returns("meters");
            var command = ClassCommandUtilities.CreateClassCommand<meters, setMeterSub>(
                TestConstants.HostId,
                TestConstants.EgmId);
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var eventLift = new Mock<IEventLift>();
            var handler = new SetMeterSub(
                HandlerUtilities.CreateMockEgm(device),
                meterSubManager.Object,
                eventLift.Object);
            command.Command.meterSubType = "G2S_onPeriodic";

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<meters, meterSubList>;

            Assert.IsNotNull(response);

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

            meterSubManager.Verify(
                x =>
                    x.SetMetersSubscription(
                        1,
                        MetersSubscriptionType.Periodic,
                        It.Is<IList<MeterSubscription>>(l => !l.Any())));
        }

        private IEnumerable<MeterSubscription> CreateMeterSubscriptions()
        {
            return new List<MeterSubscription>
            {
                new MeterSubscription
                {
                    OnCoinDrop = true,
                    OnDoorOpen = true,
                    OnEndOfDay = true,
                    OnNoteDrop = true,
                    Base = 11,
                    PeriodInterval = 77777,
                    MeterType = MeterType.Currency,
                    DeviceId = 1,
                    ClassName = DeviceClass.G2S_cabinet,
                    MeterDefinition = true,
                    SubType = MetersSubscriptionType.Periodic
                },
                new MeterSubscription
                {
                    OnCoinDrop = true,
                    OnDoorOpen = true,
                    OnEndOfDay = true,
                    OnNoteDrop = true,
                    Base = 22,
                    PeriodInterval = 88888,
                    MeterType = MeterType.Device,
                    DeviceId = 2,
                    ClassName = DeviceClass.G2S_coinAcceptor,
                    MeterDefinition = true
                },
                new MeterSubscription
                {
                    OnCoinDrop = true,
                    OnDoorOpen = true,
                    OnEndOfDay = true,
                    OnNoteDrop = true,
                    Base = 33,
                    PeriodInterval = 88888,
                    MeterType = MeterType.Game,
                    DeviceId = 3,
                    ClassName = DeviceClass.G2S_eventHandler,
                    MeterDefinition = true
                },
                new MeterSubscription
                {
                    OnCoinDrop = true,
                    OnDoorOpen = true,
                    OnEndOfDay = true,
                    OnNoteDrop = true,
                    Base = 44,
                    PeriodInterval = 88888,
                    MeterType = MeterType.Wage,
                    DeviceId = 4,
                    ClassName = DeviceClass.G2S_hopper,
                    MeterDefinition = true
                }
            };
        }

        private void FillMeters(ClassCommand<meters, setMeterSub> command)
        {
            command.Command.getCurrencyMeters = new[]
            {
                new getCurrencyMeters
                {
                    deviceId = 1,
                    deviceClass =
                        DeviceClass.G2S_eventHandler
                }
            };

            command.Command.getGameDenomMeters = new[]
            {
                new getGameDenomMeters
                {
                    deviceId = 2,
                    deviceClass = DeviceClass.G2S_hopper
                }
            };

            command.Command.getDeviceMeters = new[]
            {
                new getDeviceMeters
                {
                    deviceId = 3,
                    deviceClass = DeviceClass.G2S_gat
                }
            };

            command.Command.getWagerMeters = new[]
            {
                new getWagerMeters
                {
                    deviceId = 4,
                    deviceClass = DeviceClass.G2S_progressive
                }
            };
        }

        private ClassCommand<meters, setMeterSub> CreateCommand()
        {
            var command = ClassCommandUtilities.CreateClassCommand<meters, setMeterSub>(
                TestConstants.HostId,
                TestConstants.EgmId);

            return command;
        }
    }
}