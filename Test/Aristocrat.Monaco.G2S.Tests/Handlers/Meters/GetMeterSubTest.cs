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
    public class GetMeterSubTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetMeterSub(null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullMetersSubscriptionManagerExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new GetMeterSub(egm.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var handler = new GetMeterSub(egm.Object, meterSubManager.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var handler = new GetMeterSub(egm.Object, meterSubManager.Object);
            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var queue = new Mock<ICommandQueue>();
            var device = new Mock<IMetersDevice>();
            queue.SetupGet(q => q.TimeToLiveBehavior).Returns(TimeToLiveBehavior.Strict);
            device.SetupGet(evt => evt.Queue).Returns(queue.Object);
            egm.Setup(e => e.GetDevice<IMetersDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);

            var handler = new GetMeterSub(egm.Object, meterSubManager.Object);
            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var egm = new Mock<IG2SEgm>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            var device = new Mock<IMetersDevice>();
            var queue = new Mock<ICommandQueue>();

            queue.SetupGet(q => q.TimeToLiveBehavior).Returns(TimeToLiveBehavior.Strict);
            device.SetupGet(comms => comms.Queue).Returns(queue.Object);
            device.SetupGet(evt => evt.Owner).Returns(TestConstants.HostId);
            egm.Setup(e => e.GetDevice<IMetersDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);

            var handler = new GetMeterSub(egm.Object, meterSubManager.Object);
            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenSessionTypeIsG2SNotificationExpectNoResult()
        {
            var egm = new Mock<IG2SEgm>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();

            var handler = new GetMeterSub(egm.Object, meterSubManager.Object);

            var command = CreateCommand();

            command.Class.sessionType = t_sessionTypes.G2S_notification;

            await handler.Handle(command);

            Assert.AreEqual(0, command.Responses.Count());

            meterSubManager.Verify(
                x => x.GetMeterSub(It.IsAny<int>(), It.IsAny<MetersSubscriptionType>()),
                Times.Never);
        }

        [TestMethod]
        public async Task WhenMeterSubNotExistExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();

            var handler = new GetMeterSub(egm.Object, meterSubManager.Object);

            var command = CreateCommand();
            command.Command.meterSubType = "G2S_subType";

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<meters, meterSubList>;

            Assert.IsNotNull(response);
            Assert.IsFalse(response.Command.onCoinDrop);
            Assert.IsFalse(response.Command.onDoorOpen);
            Assert.IsFalse(response.Command.onEOD);
            Assert.IsFalse(response.Command.onNoteDrop);
            Assert.AreEqual(response.Command.eodBase, 0);
            Assert.AreEqual(response.Command.periodicBase, 0);
            Assert.AreEqual(response.Command.periodicInterval, 90000);
            Assert.AreEqual(response.Command.meterSubType, command.Command.meterSubType);
        }

        [TestMethod]
        public async Task WhenMeterSubExistsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var meterSubManager = new Mock<IMetersSubscriptionManager>();
            meterSubManager.Setup(x => x.GetMeterSub(1, MetersSubscriptionType.Periodic))
                .Returns(
                    new List<MeterSubscription>
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
                    }.AsQueryable());

            var handler = new GetMeterSub(egm.Object, meterSubManager.Object);

            var command = CreateCommand();
            command.Command.meterSubType = "G2S_onPeriodic";

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<meters, meterSubList>;

            Assert.IsNotNull(response);
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
        }

        private ClassCommand<meters, getMeterSub> CreateCommand()
        {
            var command = ClassCommandUtilities.CreateClassCommand<meters, getMeterSub>(
                TestConstants.HostId,
                TestConstants.EgmId);

            return command;
        }
    }
}