namespace Aristocrat.Monaco.G2S.Tests.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Consumers;
    using G2S.Handlers;
    using G2S.Meters;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class NoteAcceptorHardwareFaultConsumerTest
    {
        private Mock<ICabinetDevice> _cabinetDevice;

        private Mock<ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus>> _commandBuilderMock;
        private Mock<IG2SEgm> _egmMock;
        private Mock<IEventLift> _eventLiftMock;
        private Mock<INoteAcceptorDevice> _noteAcceptorDevice;
        private Mock<IMeterAggregator<INoteAcceptorDevice>> _metersAggregatorMock;
        private Mock<IMetersSubscriptionManager> _meterSubscription;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _egmMock = new Mock<IG2SEgm>();
            _commandBuilderMock = new Mock<ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus>>();
            _eventLiftMock = new Mock<IEventLift>();
            _noteAcceptorDevice = new Mock<INoteAcceptorDevice>();
            _cabinetDevice = new Mock<ICabinetDevice>();
            _metersAggregatorMock = new Mock<IMeterAggregator<INoteAcceptorDevice>>();
            _meterSubscription = new Mock<IMetersSubscriptionManager>();

            _noteAcceptorDevice.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_noteAcceptor);
            _cabinetDevice.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_cabinet);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenG2SEgmIsNullExpectException()
        {
            var consumer = new NoteAcceptorHardwareFaultConsumer(
                null,
                _commandBuilderMock.Object,
                _eventLiftMock.Object,
                _metersAggregatorMock.Object,
                _meterSubscription.Object);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCommandBuilderIsNullExpectException()
        {
            var consumer = new NoteAcceptorHardwareFaultConsumer(
                _egmMock.Object,
                null,
                _eventLiftMock.Object,
                _metersAggregatorMock.Object,
                _meterSubscription.Object);
            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEventLiftIsNullExpectException()
        {
            var consumer = new NoteAcceptorHardwareFaultConsumer(
                _egmMock.Object,
                _commandBuilderMock.Object,
                null,
                _metersAggregatorMock.Object,
                _meterSubscription.Object);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenMeterAggregatorIsNullExpectException()
        {
            var consumer = new NoteAcceptorHardwareFaultConsumer(
                _egmMock.Object,
                _commandBuilderMock.Object,
                _eventLiftMock.Object,
                null,
                _meterSubscription.Object);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenMetersSubscriptionIsNullExpectException()
        {
            var consumer = new NoteAcceptorHardwareFaultConsumer(
                _egmMock.Object,
                _commandBuilderMock.Object,
                _eventLiftMock.Object,
                _metersAggregatorMock.Object,
                null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        public void WhenConsumeWithHardwareFaultIsNoneExpectNoActions()
        {
            var consumer = CreateConsumer(true);

            consumer.Consume(new HardwareFaultEvent(NoteAcceptorFaultTypes.None));

            _commandBuilderMock
                .Verify(m => m.Build(It.IsAny<INoteAcceptorDevice>(), It.IsAny<noteAcceptorStatus>()), Times.Once);
            _eventLiftMock
                .Verify(m => m.Report(It.IsAny<IDevice>(), It.IsAny<string>(), It.IsAny<deviceList1>(), It.IsAny<meterList>(), It.IsAny<IEvent>()), Times.Never);
        }

        [TestMethod]
        public void WhenEgmHasNoNoteAcceptorDeviceExpectNoActions()
        {
            _egmMock.Setup(m => m.GetDevice<ICabinetDevice>()).Returns(_cabinetDevice.Object);
            var consumer = CreateConsumer();

            consumer.Consume(new HardwareFaultEvent(NoteAcceptorFaultTypes.MechanicalFault));

            _commandBuilderMock
                .Verify(m => m.Build(It.IsAny<INoteAcceptorDevice>(), It.IsAny<noteAcceptorStatus>()), Times.Never);
            _eventLiftMock
                .Verify(m => m.Report(It.IsAny<IDevice>(), It.IsAny<string>(), It.IsAny<deviceList1>(), It.IsAny<meterList>(), It.IsAny<IEvent>()), Times.Never);
        }

        [TestMethod]
        public void WhenConsumeWithHardwareFaultFirmwareFailure()
        {
            var consumer = CreateConsumer(true);

            consumer.Consume(new HardwareFaultEvent(NoteAcceptorFaultTypes.FirmwareFault));

            _commandBuilderMock
                .Verify(m => m.Build(It.IsAny<INoteAcceptorDevice>(), It.IsAny<noteAcceptorStatus>()), Times.Once);

            _eventLiftMock
                .Verify(
                    m => m.Report(_noteAcceptorDevice.Object, EventCode.G2S_NAE903, It.IsAny<deviceList1>(), It.IsAny<meterList>(), It.IsAny<IEvent>()),
                    Times.Once);
        }

        [TestMethod]
        public void WhenConsumeWithHardwareFaultMechanicalFailure()
        {
            var consumer = CreateConsumer(true);

            consumer.Consume(new HardwareFaultEvent(NoteAcceptorFaultTypes.MechanicalFault));

            _commandBuilderMock
                .Verify(m => m.Build(It.IsAny<INoteAcceptorDevice>(), It.IsAny<noteAcceptorStatus>()), Times.Once);

            _eventLiftMock
                .Verify(
                    m => m.Report(_noteAcceptorDevice.Object, EventCode.G2S_NAE904, It.IsAny<deviceList1>(), It.IsAny<meterList>(), It.IsAny<IEvent>()),
                    Times.Once);
        }

        [TestMethod]
        public void WhenConsumeWithHardwareFaultOpticalFailure()
        {
            var consumer = CreateConsumer(true);

            consumer.Consume(new HardwareFaultEvent(NoteAcceptorFaultTypes.OpticalFault));

            _commandBuilderMock
                .Verify(m => m.Build(It.IsAny<INoteAcceptorDevice>(), It.IsAny<noteAcceptorStatus>()), Times.Once);

            _eventLiftMock
                .Verify(
                    m => m.Report(_noteAcceptorDevice.Object, EventCode.G2S_NAE905, It.IsAny<deviceList1>(), It.IsAny<meterList>(), It.IsAny<IEvent>()),
                    Times.Once);
        }

        [TestMethod]
        public void WhenConsumeWithHardwareFaultComponentFailure()
        {
            var consumer = CreateConsumer(true);

            consumer.Consume(new HardwareFaultEvent(NoteAcceptorFaultTypes.ComponentFault));

            _commandBuilderMock
                .Verify(m => m.Build(It.IsAny<INoteAcceptorDevice>(), It.IsAny<noteAcceptorStatus>()), Times.Once);

            _eventLiftMock
                .Verify(
                    m => m.Report(_noteAcceptorDevice.Object, EventCode.G2S_NAE906, It.IsAny<deviceList1>(), It.IsAny<meterList>(), It.IsAny<IEvent>()),
                    Times.Once);
        }

        [TestMethod]
        public void WhenConsumeWithHardwareFaultNvmFailure()
        {
            var consumer = CreateConsumer(true);

            consumer.Consume(new HardwareFaultEvent(NoteAcceptorFaultTypes.NvmFault));

            _commandBuilderMock
                .Verify(m => m.Build(It.IsAny<INoteAcceptorDevice>(), It.IsAny<noteAcceptorStatus>()), Times.Once);

            _eventLiftMock
                .Verify(
                    m => m.Report(_noteAcceptorDevice.Object, EventCode.G2S_NAE907, It.IsAny<deviceList1>(), It.IsAny<meterList>(), It.IsAny<IEvent>()),
                    Times.Once);
        }

        [TestMethod]
        public void WhenConsumeWithHardwareFaultOtherFailure()
        {
            var consumer = CreateConsumer(true);

            consumer.Consume(new HardwareFaultEvent(NoteAcceptorFaultTypes.OtherFault));

            _commandBuilderMock
                .Verify(m => m.Build(It.IsAny<INoteAcceptorDevice>(), It.IsAny<noteAcceptorStatus>()), Times.Once);

            _eventLiftMock
                .Verify(
                    m => m.Report(_noteAcceptorDevice.Object, EventCode.G2S_NAE101, It.IsAny<deviceList1>(), It.IsAny<meterList>(), It.IsAny<IEvent>()),
                    Times.Once);
        }

        [TestMethod]
        public void WhenConsumeWithHardwareFaultStackerFull()
        {
            var consumer = CreateConsumer(true);

            consumer.Consume(new HardwareFaultEvent(NoteAcceptorFaultTypes.StackerFull));

            _commandBuilderMock
                .Verify(m => m.Build(It.IsAny<INoteAcceptorDevice>(), It.IsAny<noteAcceptorStatus>()), Times.Once);

            _eventLiftMock
                .Verify(
                    m => m.Report(_noteAcceptorDevice.Object, EventCode.G2S_NAE105, It.IsAny<deviceList1>(), It.IsAny<meterList>(), It.IsAny<IEvent>()),
                    Times.Once);
        }

        [TestMethod]
        public void WhenConsumeWithHardwareFaultStackerJammed()
        {
            var consumer = CreateConsumer(true);

            consumer.Consume(new HardwareFaultEvent(NoteAcceptorFaultTypes.StackerJammed));

            _commandBuilderMock
                .Verify(m => m.Build(It.IsAny<INoteAcceptorDevice>(), It.IsAny<noteAcceptorStatus>()), Times.Once);

            _eventLiftMock
                .Verify(
                    m => m.Report(_noteAcceptorDevice.Object, EventCode.G2S_NAE106, It.IsAny<deviceList1>(), It.IsAny<meterList>(), It.IsAny<IEvent>()),
                    Times.Once);
        }

        [TestMethod]
        public void WhenConsumeWithHardwareFaultStackerFault()
        {
            var consumer = CreateConsumer(true);

            consumer.Consume(new HardwareFaultEvent(NoteAcceptorFaultTypes.StackerFault));

            _commandBuilderMock
                .Verify(m => m.Build(It.IsAny<INoteAcceptorDevice>(), It.IsAny<noteAcceptorStatus>()), Times.Once);

            _eventLiftMock
                .Verify(
                    m => m.Report(_noteAcceptorDevice.Object, EventCode.G2S_NAE107, It.IsAny<deviceList1>(), It.IsAny<meterList>(), It.IsAny<IEvent>()),
                    Times.Once);
        }

        [TestMethod]
        public void WhenConsumeWithHardwareFaultOutOfCaseExpectNoActions()
        {
            var consumer = CreateConsumer(true);

            consumer.Consume(new HardwareFaultEvent((NoteAcceptorFaultTypes) 0x10000));

            _commandBuilderMock
                .Verify(m => m.Build(It.IsAny<INoteAcceptorDevice>(), It.IsAny<noteAcceptorStatus>()), Times.Once);

            _eventLiftMock
                .Verify(m => m.Report(It.IsAny<IDevice>(), It.IsAny<string>(), It.IsAny<deviceList1>(), It.IsAny<meterList>(), It.IsAny<IEvent>()), Times.Never);
        }

        private NoteAcceptorHardwareFaultConsumer CreateConsumer(bool egmWithDevices = false)
        {
            if (egmWithDevices)
            {
                _egmMock.Setup(m => m.GetDevice<INoteAcceptorDevice>()).Returns(_noteAcceptorDevice.Object);
                _egmMock.Setup(m => m.GetDevice<ICabinetDevice>()).Returns(_cabinetDevice.Object);
            }

            return new NoteAcceptorHardwareFaultConsumer(
                _egmMock.Object,
                _commandBuilderMock.Object,
                _eventLiftMock.Object,
                _metersAggregatorMock.Object,
                _meterSubscription.Object);
        }
    }
}