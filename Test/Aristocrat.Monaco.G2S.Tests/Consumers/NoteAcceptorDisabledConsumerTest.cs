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
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using IDevice = Aristocrat.G2S.Client.Devices.IDevice;

    [TestClass]
    public class NoteAcceptorDisabledConsumerTest
    {
        private Mock<ICabinetDevice> _cabinetDevice;

        private Mock<ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus>> _commandBuilderMock;
        private Mock<IG2SEgm> _egmMock;
        private Mock<IDeviceRegistryService> _deviceRegistry;
        private Mock<IEventLift> _eventLiftMock;

        private Mock<INoteAcceptorDevice> _noteAcceptorDevice;

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

            _deviceRegistry.Setup(m => m.GetDevice<INoteAcceptor>()).Returns(new Mock<INoteAcceptor>().Object);

            _noteAcceptorDevice.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_noteAcceptor);
            _cabinetDevice.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_cabinet);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenG2SEgmIsNullExpectException()
        {
            var consumer = new NoteAcceptorDisabledConsumer(null, null, null, null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCommandBuilderIsNullExpectException()
        {
            var consumer = new NoteAcceptorDisabledConsumer(_egmMock.Object, null, null, null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenDeviceRegistryIsNullExpectException()
        {
            var consumer = new NoteAcceptorDisabledConsumer(_egmMock.Object, _commandBuilderMock.Object, null, null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEventLiftIsNullExpectException()
        {
            var consumer = new NoteAcceptorDisabledConsumer(_egmMock.Object, _commandBuilderMock.Object, _deviceRegistry.Object, null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        public void WhenEgmHasNoNoteAcceptorDeviceExpectNoActions()
        {
            var consumer = CreateConsumer(false);

            consumer.Consume(new DisabledEvent(DisabledReasons.Device));

            _commandBuilderMock
                .Verify(m => m.Build(It.IsAny<INoteAcceptorDevice>(), It.IsAny<noteAcceptorStatus>()), Times.Never);
            _eventLiftMock
                .Verify(m => m.Report(It.IsAny<IDevice>(), It.IsAny<string>(), It.IsAny<deviceList1>()), Times.Never);
        }

        [TestMethod]
        public void WhenConsumeWithCommonEnabledReasonExpectSuccess()
        {
            var consumer = CreateConsumer();

            _noteAcceptorDevice.SetupGet(m => m.Enabled).Returns(true);

            consumer.Consume(new DisabledEvent(DisabledReasons.Device));

            _noteAcceptorDevice.VerifySet(m => m.Enabled = false);
            _commandBuilderMock
                .Verify(m => m.Build(_noteAcceptorDevice.Object, It.IsAny<noteAcceptorStatus>()), Times.Once);
            _eventLiftMock
                .Verify(
                    m => m.Report(_noteAcceptorDevice.Object, EventCode.G2S_NAE001, It.IsAny<deviceList1>()),
                    Times.Once);
        }

        private NoteAcceptorDisabledConsumer CreateConsumer(bool egmWithDevices = true)
        {
            if (egmWithDevices)
            {
                _egmMock.Setup(m => m.GetDevice<INoteAcceptorDevice>()).Returns(_noteAcceptorDevice.Object);
                _egmMock.Setup(m => m.GetDevice<ICabinetDevice>()).Returns(_cabinetDevice.Object);
            }

            return new NoteAcceptorDisabledConsumer(
                _egmMock.Object,
                _commandBuilderMock.Object,
                _deviceRegistry.Object,
                _eventLiftMock.Object);
        }
    }
}
