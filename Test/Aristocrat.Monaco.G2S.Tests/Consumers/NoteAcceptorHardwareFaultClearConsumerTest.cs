namespace Aristocrat.Monaco.G2S.Tests.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Consumers;
    using G2S.Handlers;
    using Hardware.Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class NoteAcceptorHardwareFaultClearConsumerTest
    {
        private Mock<ICabinetDevice> _cabinetDevice;

        private Mock<ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus>> _commandBuilderMock;
        private Mock<IG2SEgm> _egmMock;
        private Mock<IEventLift> _eventLiftMock;
        private Mock<INoteAcceptorDevice> _noteAcceptorDevice;
        private Mock<IDeviceRegistryService> _deviceRegistry;

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
            _deviceRegistry = new Mock<IDeviceRegistryService>();

            _noteAcceptorDevice.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_noteAcceptor);
            _cabinetDevice.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_cabinet);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenG2SEgmIsNullExpectException()
        {
            var consumer = new NoteAcceptorHardwareFaultClearConsumer(
                null,
                _commandBuilderMock.Object,
                _eventLiftMock.Object,
                _deviceRegistry.Object);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCommandBuilderIsNullExpectException()
        {
            var consumer = new NoteAcceptorHardwareFaultClearConsumer(
                _egmMock.Object,
                null,
                _eventLiftMock.Object,
                _deviceRegistry.Object);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEventLiftIsNullExpectException()
        {
            var consumer = new NoteAcceptorHardwareFaultClearConsumer(
                _egmMock.Object,
                _commandBuilderMock.Object,
                null,
                _deviceRegistry.Object);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenDeviceRegistryIsNullExpectException()
        {
            var consumer = new NoteAcceptorHardwareFaultClearConsumer(
                _egmMock.Object,
                _commandBuilderMock.Object,
                _eventLiftMock.Object,
                null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        public void WhenConsumeWithFaultClearNoDevice()
        {
            var consumer = CreateConsumer(false);
            consumer.Consume(new HardwareFaultClearEvent(NoteAcceptorFaultTypes.OpticalFault));

            _commandBuilderMock
                .Verify(m => m.Build(It.IsAny<INoteAcceptorDevice>(), It.IsAny<noteAcceptorStatus>()), Times.Never);
            _eventLiftMock
                .Verify(m => m.Report(It.IsAny<IDevice>(), It.IsAny<string>(), It.IsAny<deviceList1>()), Times.Never);
        }

        [TestMethod]
        public void WhenConsumeWithFaultClearNoActions()
        {
            var consumer = CreateConsumer();
            var noteAcceptor = new Mock<INoteAcceptor>();
            _deviceRegistry.Setup(r => r.GetDevice<INoteAcceptor>()).Returns(noteAcceptor.Object);
            noteAcceptor.Setup(n => n.Faults).Returns(NoteAcceptorFaultTypes.MechanicalFault);

            consumer.Consume(new HardwareFaultClearEvent(NoteAcceptorFaultTypes.OpticalFault));

            _commandBuilderMock
                .Verify(m => m.Build(It.IsAny<INoteAcceptorDevice>(), It.IsAny<noteAcceptorStatus>()), Times.Once);
            _eventLiftMock
                .Verify(m => m.Report(It.IsAny<IDevice>(), It.IsAny<string>(), It.IsAny<deviceList1>()), Times.Never);
        }

        [TestMethod]
        public void WhenConsumeWithFaultClearExpectSuccess()
        {
            var consumer = CreateConsumer();
            var noteAcceptor = new Mock<INoteAcceptor>();
            _deviceRegistry.Setup(r => r.GetDevice<INoteAcceptor>()).Returns(noteAcceptor.Object);
            noteAcceptor.Setup(n => n.Faults).Returns(NoteAcceptorFaultTypes.MechanicalFault);

            consumer.Consume(new HardwareFaultClearEvent(NoteAcceptorFaultTypes.StackerDisconnected));

            _commandBuilderMock
                .Verify(m => m.Build(_noteAcceptorDevice.Object, It.IsAny<noteAcceptorStatus>()), Times.Once);
            _eventLiftMock
                .Verify(
                    m => m.Report(_noteAcceptorDevice.Object, EventCode.G2S_NAE104, It.IsAny<deviceList1>(), It.IsAny<IEvent>()),
                    Times.Once);
        }

        [TestMethod]
        public void WhenConsumeWithFaultClearAllClear()
        {
            var consumer = CreateConsumer();
            var noteAcceptor = new Mock<INoteAcceptor>();
            _deviceRegistry.Setup(r => r.GetDevice<INoteAcceptor>()).Returns(noteAcceptor.Object);
            noteAcceptor.Setup(n => n.Faults).Returns(NoteAcceptorFaultTypes.None);

            consumer.Consume(new HardwareFaultClearEvent(NoteAcceptorFaultTypes.OpticalFault));

            _commandBuilderMock
                .Verify(m => m.Build(_noteAcceptorDevice.Object, It.IsAny<noteAcceptorStatus>()), Times.Once);
            _eventLiftMock
                .Verify(
                    m => m.Report(_noteAcceptorDevice.Object, EventCode.G2S_NAE099, It.IsAny<deviceList1>(), It.IsAny<IEvent>()),
                    Times.Once);
        }

        private NoteAcceptorHardwareFaultClearConsumer CreateConsumer(bool egmWithDevices = true)
        {
            if (egmWithDevices)
            {
                _egmMock.Setup(m => m.GetDevice<INoteAcceptorDevice>(It.IsAny<int>())).Returns(_noteAcceptorDevice.Object);
                _egmMock.Setup(m => m.GetDevice<ICabinetDevice>()).Returns(_cabinetDevice.Object);
            }

            return new NoteAcceptorHardwareFaultClearConsumer(
                _egmMock.Object,
                _commandBuilderMock.Object,
                _eventLiftMock.Object,
                _deviceRegistry.Object);
        }
    }
}