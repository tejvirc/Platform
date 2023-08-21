namespace Aristocrat.Monaco.G2S.Tests.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Consumers;
    using G2S.Handlers;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using IDevice = Aristocrat.G2S.Client.Devices.IDevice;

    [TestClass]
    public class NoteAcceptorEnabledConsumerTest
    {
        private Mock<ICabinetDevice> _cabinetDevice;

        private Mock<ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus>> _commandBuilderMock;
        private Mock<IG2SEgm> _egmMock;

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

            _noteAcceptorDevice.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_noteAcceptor);
            _cabinetDevice.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_cabinet);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenG2SEgmIsNullExpectException()
        {
            var consumer = new NoteAcceptorEnabledConsumer(
                null,
                _commandBuilderMock.Object,
                _eventLiftMock.Object);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCommandBuilderIsNullExpectException()
        {
            var consumer = new NoteAcceptorEnabledConsumer(
                _egmMock.Object,
                null,
                _eventLiftMock.Object);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEventLiftIsNullExpectException()
        {
            var consumer = new NoteAcceptorEnabledConsumer(
                _egmMock.Object,
                _commandBuilderMock.Object,
                null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        public void WhenEgmHasNoNoteAcceptorDeviceExpectNoActions()
        {
            _egmMock.Setup(m => m.GetDevice<ICabinetDevice>()).Returns(_cabinetDevice.Object);
            var consumer = CreateConsumer(false);

            consumer.Consume(new EnabledEvent(EnabledReasons.Reset));

            _commandBuilderMock
                .Verify(m => m.Build(It.IsAny<INoteAcceptorDevice>(), It.IsAny<noteAcceptorStatus>()), Times.Never);
            _eventLiftMock
                .Verify(m => m.Report(It.IsAny<IDevice>(), It.IsAny<string>(), It.IsAny<deviceList1>()), Times.Never);
        }

        [TestMethod]
        public void WhenConsumeWithEnabledReasonsIsResetExpectCommandBuildTwice()
        {
            var consumer = CreateConsumer();
            consumer.Consume(new EnabledEvent(EnabledReasons.Reset));

            _commandBuilderMock
                .Verify(m => m.Build(_noteAcceptorDevice.Object, It.IsAny<noteAcceptorStatus>()), Times.Exactly(1));
            _eventLiftMock
                .Verify(
                    m => m.Report(_noteAcceptorDevice.Object, EventCode.G2S_NAE002, It.IsAny<deviceList1>(), It.IsAny<IEvent>()),
                    Times.Once);
        }

        [TestMethod]
        public void WhenConsumeWithCommonEnabledReasonExpectSuccess()
        {
            var consumer = CreateConsumer();

            _noteAcceptorDevice.SetupGet(m => m.Enabled).Returns(false);

            consumer.Consume(new EnabledEvent(EnabledReasons.Service));

            _noteAcceptorDevice.VerifySet(m => m.Enabled = true);
            _commandBuilderMock
                .Verify(m => m.Build(_noteAcceptorDevice.Object, It.IsAny<noteAcceptorStatus>()), Times.Once);
            _eventLiftMock
                .Verify(
                    m => m.Report(_noteAcceptorDevice.Object, EventCode.G2S_NAE002, It.IsAny<deviceList1>(), It.IsAny<IEvent>()),
                    Times.Once);
        }

        private NoteAcceptorEnabledConsumer CreateConsumer(bool egmWithDevices = true)
        {
            if (egmWithDevices)
            {
                _egmMock.Setup(m => m.GetDevice<INoteAcceptorDevice>()).Returns(_noteAcceptorDevice.Object);
                _egmMock.Setup(m => m.GetDevice<ICabinetDevice>()).Returns(_cabinetDevice.Object);
            }

            return new NoteAcceptorEnabledConsumer(
                _egmMock.Object,
                _commandBuilderMock.Object,
                _eventLiftMock.Object);
        }
    }
}