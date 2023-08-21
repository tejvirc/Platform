namespace Aristocrat.Monaco.G2S.Tests.Handlers.NoteAcceptor
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers;
    using G2S.Handlers.NoteAcceptor;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.SharedDevice;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SetNoteAcceptorStateTest
    {
        private Mock<ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus>> _commandBuilderMock;
        private Mock<IG2SEgm> _egmMock;

        [TestInitialize]
        public void Initialize()
        {
            _egmMock = new Mock<IG2SEgm>();
            _commandBuilderMock = new Mock<ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus>>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new SetNoteAcceptorState(null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCommandBuilderExpectException()
        {
            var handler = new SetNoteAcceptorState(_egmMock.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectSuccess()
        {
            var handler = new SetNoteAcceptorState(
                _egmMock.Object,
                _commandBuilderMock.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithOwnerOnlyExpectNoError()
        {
            var egm = HandlerUtilities.CreateMockEgm<INoteAcceptorDevice>();

            var handler = new SetNoteAcceptorState(
                egm,
                _commandBuilderMock.Object);

            await VerificationTests.VerifyChecksForOwner(handler);
        }

        [TestMethod]
        public async Task WhenHandleWithEnableCommandExpectResponse()
        {
            var noteAcceptorMock = new Mock<INoteAcceptor>();
            var deviceServiceMock = noteAcceptorMock.As<IDeviceService>();
            deviceServiceMock.SetupGet(m => m.ReasonDisabled).Returns(DisabledReasons.Backend);

            var noteAcceptorDeviceMock = new Mock<INoteAcceptorDevice>();
            noteAcceptorDeviceMock.SetupGet(d => d.DeviceClass).Returns("noteAcceptor");
            noteAcceptorDeviceMock.SetupGet(d => d.HostEnabled).Returns(false);
            var egm = HandlerUtilities.CreateMockEgm(noteAcceptorDeviceMock);

            var handler = new SetNoteAcceptorState(
                egm,
                _commandBuilderMock.Object);

            var command =
                ClassCommandUtilities.CreateClassCommand<noteAcceptor, setNoteAcceptorState>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            command.Command.enable = true;

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<noteAcceptor, noteAcceptorStatus>;

            Assert.IsNotNull(response);

            noteAcceptorDeviceMock.VerifySet(m => m.HostEnabled = true);
            _commandBuilderMock.Verify(m => m.Build(noteAcceptorDeviceMock.Object, It.IsAny<noteAcceptorStatus>()));
        }

        [TestMethod]
        public async Task WhenHandleWithDisableCommandExpectResponse()
        {
            var noteAcceptorMock = new Mock<INoteAcceptor>();
            var deviceServiceMock = noteAcceptorMock.As<IDeviceService>();
            deviceServiceMock.SetupGet(m => m.ReasonDisabled).Returns(DisabledReasons.Service);

            var noteAcceptorDeviceMock = new Mock<INoteAcceptorDevice>();
            noteAcceptorDeviceMock.SetupGet(d => d.DeviceClass).Returns("noteAcceptor");
            noteAcceptorDeviceMock.SetupGet(d => d.RequiredForPlay).Returns(true);
            noteAcceptorDeviceMock.SetupGet(d => d.HostEnabled).Returns(true);
            var egm = HandlerUtilities.CreateMockEgm(noteAcceptorDeviceMock);

            var handler = new SetNoteAcceptorState(egm, _commandBuilderMock.Object);

            var command =
                ClassCommandUtilities.CreateClassCommand<noteAcceptor, setNoteAcceptorState>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            command.Command.enable = false;

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<noteAcceptor, noteAcceptorStatus>;

            Assert.IsNotNull(response);

            noteAcceptorDeviceMock.VerifySet(m => m.HostEnabled = false);
            _commandBuilderMock.Verify(m => m.Build(noteAcceptorDeviceMock.Object, It.IsAny<noteAcceptorStatus>()));
        }

        [TestMethod]
        public async Task WhenHandleWithRedisableCommandExpectNoActionsAndResponse()
        {
            var noteAcceptorMock = new Mock<INoteAcceptor>();
            var deviceServiceMock = noteAcceptorMock.As<IDeviceService>();
            deviceServiceMock.SetupGet(m => m.ReasonDisabled).Returns(DisabledReasons.Backend);

            var noteAcceptorDeviceMock = new Mock<INoteAcceptorDevice>();
            var egm = HandlerUtilities.CreateMockEgm(noteAcceptorDeviceMock);

            var handler = new SetNoteAcceptorState(egm, _commandBuilderMock.Object);

            var command =
                ClassCommandUtilities.CreateClassCommand<noteAcceptor, setNoteAcceptorState>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            command.Command.enable = false;

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<noteAcceptor, noteAcceptorStatus>;

            Assert.IsNotNull(response);

            deviceServiceMock.Verify(m => m.Disable(It.IsAny<DisabledReasons>()), Times.Never);
        }
    }
}
