namespace Aristocrat.Monaco.G2S.Tests.Handlers.Downloads
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.PackageManager;
    using Common.PackageManager.Storage;
    using G2S.Handlers;
    using G2S.Handlers.Downloads;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CancelScriptTest
    {
        private const int DeviceId = 1;
        private const int ScriptId = 2;

        private readonly Mock<ICommandBuilder<IDownloadDevice, scriptStatus>> _commandBuilderMock =
            new Mock<ICommandBuilder<IDownloadDevice, scriptStatus>>();

        private readonly Mock<IG2SEgm> _egmMock = new Mock<IG2SEgm>();
        private readonly Mock<IPackageManager> _packageManagerMock = new Mock<IPackageManager>();
        private readonly Mock<IScriptManager> _scriptManagerMock = new Mock<IScriptManager>();

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new CancelScript(null, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullPackageManagerExpectException()
        {
            var handler = new CancelScript(_egmMock.Object, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullScriptManagerExpectException()
        {
            var handler = new CancelScript(_egmMock.Object, _packageManagerMock.Object, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCommandBuilderExpectException()
        {
            var handler = new CancelScript(
                _egmMock.Object,
                _packageManagerMock.Object,
                _scriptManagerMock.Object,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var handler = new CancelScript(
                _egmMock.Object,
                _packageManagerMock.Object,
                _scriptManagerMock.Object,
                _commandBuilderMock.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var handler = new CancelScript(
                _egmMock.Object,
                _packageManagerMock.Object,
                _scriptManagerMock.Object,
                _commandBuilderMock.Object);
            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var egm = DownloadsUtilities.CreateMockEgm();
            var handler = new CancelScript(
                egm,
                _packageManagerMock.Object,
                _scriptManagerMock.Object,
                _commandBuilderMock.Object);
            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var evtDevice = new Mock<IDownloadDevice>();
            var queue = new Mock<ICommandQueue>();
            queue.SetupGet(q => q.TimeToLiveBehavior).Returns(TimeToLiveBehavior.Strict);
            evtDevice.SetupGet(comms => comms.Queue).Returns(queue.Object);
            evtDevice.SetupGet(evt => evt.Owner).Returns(TestConstants.HostId);
            _egmMock.Setup(e => e.GetDevice<IDownloadDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(evtDevice.Object);

            var handler = new CancelScript(
                _egmMock.Object,
                _packageManagerMock.Object,
                _scriptManagerMock.Object,
                _commandBuilderMock.Object);
            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenSessionTypeIsG2SNotificationExpectNoResult()
        {
            var handler = new CancelScript(
                _egmMock.Object,
                _packageManagerMock.Object,
                _scriptManagerMock.Object,
                _commandBuilderMock.Object);

            var command = CreateCommand();

            command.Class.sessionType = t_sessionTypes.G2S_notification;

            await handler.Handle(command);

            Assert.AreEqual(0, command.Responses.Count());
        }

        [TestMethod]
        public async Task WhenDeviceNotFoundExpectNoResult()
        {
            var handler = new CancelScript(
                _egmMock.Object,
                _packageManagerMock.Object,
                _scriptManagerMock.Object,
                _commandBuilderMock.Object);

            var command = CreateCommand();

            await handler.Handle(command);

            Assert.AreEqual(0, command.Responses.Count());
        }

        [TestMethod]
        public async Task WhenScriptLogNotFoundExpectError()
        {
            ConfigureEgm();

            var handler = new CancelScript(
                _egmMock.Object,
                _packageManagerMock.Object,
                _scriptManagerMock.Object,
                _commandBuilderMock.Object);

            var command = CreateCommand();

            await handler.Handle(command);

            Assert.IsTrue(command.Error.IsError);
            Assert.AreEqual(command.Error.Code, ErrorCode.G2S_DLX005);
        }

        [TestMethod]
        public async Task WhenScriptStatesIsNotPendingExpectError()
        {
            ConfigureEgm();

            ConfigurePackageManager(ScriptState.InProgress);

            var handler = new CancelScript(
                _egmMock.Object,
                _packageManagerMock.Object,
                _scriptManagerMock.Object,
                _commandBuilderMock.Object);

            var command = CreateCommand();

            await handler.Handle(command);

            Assert.IsTrue(command.Error.IsError);
            Assert.AreEqual(command.Error.Code, ErrorCode.G2S_DLX007);
        }

        [TestMethod]
        public async Task WhenHandleCommandExpectResponse()
        {
            ConfigureEgm();

            ConfigurePackageManager(ScriptState.PendingAuthorization);

            var handler = new CancelScript(
                _egmMock.Object,
                _packageManagerMock.Object,
                _scriptManagerMock.Object,
                _commandBuilderMock.Object);

            var command = CreateCommand();

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<download, scriptStatus>;

            Assert.IsNotNull(response);

            _scriptManagerMock.Verify(x => x.CancelScript(It.Is<Script>(s => s.ScriptId == ScriptId)), Times.Once);
        }

        private void ConfigurePackageManager(ScriptState state)
        {
            var scriptLog = new Script { ScriptId = ScriptId, State = state };

            _packageManagerMock.Setup(x => x.GetScript(ScriptId)).Returns(scriptLog);
        }

        private void ConfigureEgm()
        {
            var device = new Mock<IDownloadDevice>();
            device.SetupGet(x => x.Id).Returns(DeviceId);
            device.SetupGet(x => x.DeviceClass).Returns("download");

            _egmMock.Setup(x => x.GetDevice<IDownloadDevice>(DeviceId)).Returns(device.Object);
        }

        private ClassCommand<download, cancelScript> CreateCommand()
        {
            var command = ClassCommandUtilities.CreateClassCommand<download, cancelScript>(
                TestConstants.HostId,
                TestConstants.EgmId);

            command.Command.scriptId = ScriptId;
            command.IClass.deviceId = DeviceId;

            return command;
        }
    }
}