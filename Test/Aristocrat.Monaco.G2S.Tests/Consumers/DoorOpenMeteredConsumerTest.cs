namespace Aristocrat.Monaco.G2S.Tests.Consumers
{
    using System;
    using System.Linq;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Consumers;
    using G2S.Handlers;
    using G2S.Meters;
    using Hardware.Contracts.Door;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class DoorOpenMeteredConsumerTest
    {
        [TestInitialize]
        public void TestInitialization()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEgmIsNullExpectException()
        {
            var consumer = new DoorOpenMeteredConsumer(null, null, null, null, null, null, null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCabinetStatusCommandBuilderIsNullExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var consumer = new DoorOpenMeteredConsumer(egm.Object, null, null, null, null, null, null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenNoteAcceptorStatusBuilderIsNullExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var cabinetStatus = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();

            var consumer = new DoorOpenMeteredConsumer(egm.Object, cabinetStatus.Object, null, null, null, null, null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCabinetMeterAggregatorIsNullExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var cabinetStatus = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var noteAcceptorStatus = new Mock<ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus>>();

            var consumer = new DoorOpenMeteredConsumer(
                egm.Object,
                cabinetStatus.Object,
                noteAcceptorStatus.Object,
                null,
                null,
                null,
                null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenNoteAcceptorMeterAggregatorIsNullExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var cabinetStatus = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var noteAcceptorStatus = new Mock<ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus>>();
            var cabinetMeters = new Mock<IMeterAggregator<ICabinetDevice>>();

            var consumer = new DoorOpenMeteredConsumer(
                egm.Object,
                cabinetStatus.Object,
                noteAcceptorStatus.Object,
                cabinetMeters.Object,
                null,
                null,
                null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEventLiftIsNullExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var cabinetStatus = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var noteAcceptorStatus = new Mock<ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus>>();
            var cabinetMeters = new Mock<IMeterAggregator<ICabinetDevice>>();
            var noteAcceptorMeters = new Mock<IMeterAggregator<INoteAcceptorDevice>>();

            var consumer = new DoorOpenMeteredConsumer(
                egm.Object,
                cabinetStatus.Object,
                noteAcceptorStatus.Object,
                cabinetMeters.Object,
                noteAcceptorMeters.Object,
                null,
                null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenMeterSubscriptionManagerIsNullExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var cabinetStatus = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var noteAcceptorStatus = new Mock<ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus>>();
            var cabinetMeters = new Mock<IMeterAggregator<ICabinetDevice>>();
            var noteAcceptorMeters = new Mock<IMeterAggregator<INoteAcceptorDevice>>();
            var lift = new Mock<IEventLift>();

            var consumer = new DoorOpenMeteredConsumer(
                egm.Object,
                cabinetStatus.Object,
                noteAcceptorStatus.Object,
                cabinetMeters.Object,
                noteAcceptorMeters.Object,
                lift.Object,
                null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var cabinetStatus = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var noteAcceptorStatus = new Mock<ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus>>();
            var cabinetMeters = new Mock<IMeterAggregator<ICabinetDevice>>();
            var noteAcceptorMeters = new Mock<IMeterAggregator<INoteAcceptorDevice>>();
            var lift = new Mock<IEventLift>();
            var metersSubscriptionManager = new Mock<IMetersSubscriptionManager>();

            var consumer = new DoorOpenMeteredConsumer(
                egm.Object,
                cabinetStatus.Object,
                noteAcceptorStatus.Object,
                cabinetMeters.Object,
                noteAcceptorMeters.Object,
                lift.Object,
                metersSubscriptionManager.Object);

            Assert.IsNotNull(consumer);
        }

        [TestMethod]
        public void WhenConsumeWithUnknownIdExpectNoEvent()
        {
            var egm = new Mock<IG2SEgm>();
            var cabinetStatus = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var noteAcceptorStatus = new Mock<ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus>>();
            var cabinetMeters = new Mock<IMeterAggregator<ICabinetDevice>>();
            var noteAcceptorMeters = new Mock<IMeterAggregator<INoteAcceptorDevice>>();
            var lift = new Mock<IEventLift>();
            var metersSubscriptionManager = new Mock<IMetersSubscriptionManager>();

            var device = new Mock<ICabinetDevice>();

            egm.Setup(e => e.GetDevice<ICabinetDevice>()).Returns(device.Object);
            device.SetupGet(d => d.DeviceClass).Returns("cabinet");

            var consumer = new DoorOpenMeteredConsumer(
                egm.Object,
                cabinetStatus.Object,
                noteAcceptorStatus.Object,
                cabinetMeters.Object,
                noteAcceptorMeters.Object,
                lift.Object,
                metersSubscriptionManager.Object);

            consumer.Consume(new DoorOpenMeteredEvent());

            lift.Verify(
                l => l.Report(
                    It.IsAny<IDevice>(),
                    It.IsAny<string>(),
                    It.IsAny<deviceList1>()),
                Times.Never);
        }

        [TestMethod]
        public void WhenConsumeWithLogicDoorExpectEvent()
        {
            TestCabinetEventConsumer(DoorLogicalId.Logic, EventCode.G2S_CBE303);
        }

        [TestMethod]
        public void WhenConsumeWithTopBoxExpectEvent()
        {
            TestCabinetEventConsumer(DoorLogicalId.TopBox, EventCode.G2S_CBE305);
        }

        [TestMethod]
        public void WhenConsumeWithMainDoorExpectEvent()
        {
            TestCabinetEventConsumer(DoorLogicalId.Main, EventCode.G2S_CBE307);
        }

        [TestMethod]
        public void WhenConsumeWithCashBoxExpectEvent()
        {
            var egm = new Mock<IG2SEgm>();
            var cabinetStatus = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var noteAcceptorStatus = new Mock<ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus>>();
            var cabinetMeters = new Mock<IMeterAggregator<ICabinetDevice>>();
            var noteAcceptorMeters = new Mock<IMeterAggregator<INoteAcceptorDevice>>();
            var lift = new Mock<IEventLift>();
            var metersSubscriptionManager = new Mock<IMetersSubscriptionManager>();
            var device = new Mock<INoteAcceptorDevice>();

            egm.Setup(e => e.GetDevice<INoteAcceptorDevice>()).Returns(device.Object);
            device.SetupGet(d => d.DeviceClass).Returns("noteAcceptor");
            noteAcceptorMeters.Setup(m => m.GetMeters(device.Object, It.IsAny<string>()))
                .Returns(Enumerable.Empty<meterInfo>());

            var consumer = new DoorOpenMeteredConsumer(
                egm.Object,
                cabinetStatus.Object,
                noteAcceptorStatus.Object,
                cabinetMeters.Object,
                noteAcceptorMeters.Object,
                lift.Object,
                metersSubscriptionManager.Object);

            consumer.Consume(new DoorOpenMeteredEvent((int)DoorLogicalId.CashBox, false, false, string.Empty));

            lift.Verify(
                l => l.Report(
                    It.Is<IDevice>(d => d == device.Object),
                    It.Is<string>(e => e == EventCode.G2S_NAE112),
                    It.IsAny<deviceList1>(),
                    It.IsAny<meterList>()));
        }

        private static void TestCabinetEventConsumer(DoorLogicalId id, string eventCode)
        {
            var egm = new Mock<IG2SEgm>();
            var cabinetStatus = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var noteAcceptorStatus = new Mock<ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus>>();
            var cabinetMeters = new Mock<IMeterAggregator<ICabinetDevice>>();
            var noteAcceptorMeters = new Mock<IMeterAggregator<INoteAcceptorDevice>>();
            var lift = new Mock<IEventLift>();
            var metersSubscriptionManager = new Mock<IMetersSubscriptionManager>();
            var device = new Mock<ICabinetDevice>();

            egm.Setup(e => e.GetDevice<ICabinetDevice>()).Returns(device.Object);
            device.SetupGet(d => d.DeviceClass).Returns("cabinet");
            cabinetMeters.Setup(m => m.GetMeters(device.Object, It.IsAny<string>()))
                .Returns(Enumerable.Empty<meterInfo>());

            var consumer = new DoorOpenMeteredConsumer(
                egm.Object,
                cabinetStatus.Object,
                noteAcceptorStatus.Object,
                cabinetMeters.Object,
                noteAcceptorMeters.Object,
                lift.Object,
                metersSubscriptionManager.Object);

            consumer.Consume(new DoorOpenMeteredEvent((int)id, false, false, string.Empty));

            lift.Verify(
                l => l.Report(
                    It.Is<IDevice>(d => d == device.Object),
                    It.Is<string>(e => e == eventCode),
                    It.IsAny<deviceList1>(),
                    It.IsAny<meterList>()));
        }
    }
}
