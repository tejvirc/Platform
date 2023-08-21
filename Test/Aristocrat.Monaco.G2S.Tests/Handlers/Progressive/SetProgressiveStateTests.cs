namespace Aristocrat.Monaco.G2S.Tests.Handlers.Progressive
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers;
    using G2S.Handlers.Progressive;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SetProgressiveStateTests
    {
        private Mock<ICommandBuilder<IProgressiveDevice, progressiveStatus>> _commandBuilder;
        private Mock<IG2SEgm> _egm;

        [TestInitialize]
        public void Initialize()
        {
            _egm = new Mock<IG2SEgm>();
            _commandBuilder = new Mock<ICommandBuilder<IProgressiveDevice, progressiveStatus>>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new SetProgressiveState(null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCommandBuilderExpectException()
        {
            var handler = new SetProgressiveState(_egm.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyOwnerExpectSuccess()
        {
            var egm = HandlerUtilities.CreateMockEgm<IProgressiveDevice>();
            var handler = CreateHandler(egm);

            await VerificationTests.VerifyChecksForOwner(handler);
        }

        // TODO: Uncomment this test when g2s is fixed
        //[TestMethod]
        //public async Task WhenHandleCommandWithEnableProgressiveStateExpectSuccess()
        //{
        //    var deviceMock = new Mock<IProgressiveDevice>();
        //    deviceMock.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_progressive);
        //    var egm = HandlerUtilities.CreateMockEgm(deviceMock);
        //    var handler = CreateHandler(egm);

        //    var command = CreateCommand();
        //    command.Command.enable = true;

        //    await handler.Handle(command);

        //    deviceMock.VerifySet(d => d.DisableText = command.Command.disableText);
        //    deviceMock.VerifySet(d => d.HostEnabled = true);

        //    _eventLift
        //        .Verify(m => m.Report(deviceMock.Object, EventCode.G2S_PGE004, It.IsAny<deviceList1>()), Times.Once);
        //}

        [TestMethod]
        public async Task WhenHandleCommandWithDisableProgressiveStateExpectSuccess()
        {
            var deviceMock = new Mock<IProgressiveDevice>();
            deviceMock.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_progressive);
            deviceMock.SetupGet(m => m.HostEnabled).Returns(true);
            deviceMock.SetupGet(m => m.RequiredForPlay).Returns(true);

            var egm = HandlerUtilities.CreateMockEgm(deviceMock);
            var handler = CreateHandler(egm);

            var command = CreateCommand();
            command.Command.enable = false;

            command.Command.disableText = "disable_text";

            await handler.Handle(command);

            deviceMock.VerifySet(d => d.DisableText = command.Command.disableText);
            deviceMock.VerifySet(d => d.HostEnabled = false);
        }

        [TestMethod]
        public async Task WhenHandleCommandWithRepeatProgressiveStateRequestExpectNoActions()
        {
            var deviceMock = new Mock<IProgressiveDevice>();
            deviceMock.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_progressive);
            deviceMock.SetupGet(m => m.HostEnabled).Returns(true);

            var egm = HandlerUtilities.CreateMockEgm(deviceMock);
            var handler = CreateHandler(egm);

            var command = CreateCommand();
            command.Command.enable = true;

            await handler.Handle(command);

            deviceMock.VerifySet(d => d.HostEnabled = false, Times.Never);
            deviceMock.VerifySet(d => d.HostEnabled = true, Times.Never);
        }

        private SetProgressiveState CreateHandler(IG2SEgm egm = null)
        {
            var handler = new SetProgressiveState(
                egm ?? _egm.Object,
                _commandBuilder.Object
            );

            return handler;
        }

        private ClassCommand<progressive, setProgressiveState> CreateCommand()
        {
            var command = ClassCommandUtilities.CreateClassCommand<progressive, setProgressiveState>(
                TestConstants.HostId,
                TestConstants.EgmId);

            return command;
        }
    }
}