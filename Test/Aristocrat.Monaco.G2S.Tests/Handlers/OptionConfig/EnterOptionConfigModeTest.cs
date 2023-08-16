namespace Aristocrat.Monaco.G2S.Tests.Handlers.OptionConfig
{
    using Aristocrat.Monaco.Test.Common.UnitTesting;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using G2S.Handlers;
    using G2S.Handlers.OptionConfig;
    using G2S.Services;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    [TestClass]
    public class EnterOptionConfigModeTest
    {
        private const int DeviceId = 1;
        private const string DisableText = "command-disable-text";

        private readonly TimeSpan _sessionTimeout = new TimeSpan(1, 2, 3);

        private Mock<ICommandBuilder<IOptionConfigDevice, optionConfigModeStatus>> _commandBuilderMock;

        private Mock<IDisableConditionSaga> _configurationModeSagaMock;
        private Mock<IG2SEgm> _egmMock;

        private Mock<IEventLift> _eventLiftMock;

        [TestInitialize]
        public void Initialize()
        {
            _egmMock = new Mock<IG2SEgm>();
            _commandBuilderMock = new Mock<ICommandBuilder<IOptionConfigDevice, optionConfigModeStatus>>();
            _eventLiftMock = new Mock<IEventLift>();
            _configurationModeSagaMock = new Mock<IDisableConditionSaga>();
        }

        [TestMethod]
        public void WhenConstructWithNullsExpectException()
        {
            ConstructorTest.TestConstructorNullChecks<EnterOptionConfigMode>();
        }

        [TestMethod]
        public async Task WhenVerifyWithOwnerOnlyExpectNoError()
        {
            var egm = HandlerUtilities.CreateMockEgm<IOptionConfigDevice>();

            var handler = CreateHandler(egm);

            await VerificationTests.VerifyChecksForOwner(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithDisableConditionIsNoneExpectError()
        {
            var egm = HandlerUtilities.CreateMockEgm<IOptionConfigDevice>();

            var handler = CreateHandler(egm);

            var command = CreateCommand();

            command.Command.disableCondition = DisableCondition.None.ToG2SString();

            var error = await handler.Verify(command);

            Assert.AreEqual(ErrorCode.G2S_OCX017, error.Code);
        }

        [TestMethod]
        public async Task WhenVerifyWithEnableAndZeroCreditsConditionCommandExpectError()
        {
            var egm = HandlerUtilities.CreateMockEgm<IOptionConfigDevice>();

            var handler = CreateHandler(egm);

            var command = CreateCommand();

            command.Command.disableCondition = DisableCondition.ZeroCredits.ToG2SString();
            command.Command.enable = false;

            var error = await handler.Verify(command);

            Assert.AreEqual(ErrorCode.G2S_OCX017, error.Code);
        }

        [TestMethod]
        public async Task WhenCanNotApplyModeRequestExpectNoResponse()
        {
            var deviceMock = new Mock<IOptionConfigDevice>();
            var egm = HandlerUtilities.CreateMockEgm(deviceMock);

            var command = CreateCommand();

            var handler = CreateHandler(egm);

            await handler.Handle(command);

            Assert.AreEqual(0, command.Responses.Count());
        }

        [TestMethod]
        public async Task WhenCommandEnableEqualConfModeEnabledExpectResponse()
        {
            ConfigureEgm();

            var command = CreateCommand();
            _configurationModeSagaMock.Setup(m => m.Enabled(It.IsAny<IDevice>())).Returns(true);

            var handler = CreateHandler();

            await handler.Handle(command);

            var response = command.Responses.First() as ClassCommand<optionConfig, optionConfigModeStatus>;
            Assert.IsNotNull(response);

            _commandBuilderMock.Verify(
                x => x.Build(It.Is<IOptionConfigDevice>(d => d.Id == DeviceId), It.IsAny<optionConfigModeStatus>()),
                Times.Once);
        }

        [TestMethod]
        public async Task WhenCallExitFromConfModeExpectResponse()
        {
            ConfigureEgm();

            var command = CreateCommand();
            command.Command.enable = false;
            _configurationModeSagaMock.Setup(m => m.Enabled(It.IsAny<IDevice>())).Returns(true);

            var handler = CreateHandler();

            await handler.Handle(command);

            _configurationModeSagaMock.Verify(
                x => x.Exit(
                    It.Is<IOptionConfigDevice>(d => d.Id == DeviceId),
                    DisableCondition.Immediate,
                    _sessionTimeout,
                    It.IsAny<Action<bool>>()),
                Times.Once);
        }

        [TestMethod]
        public async Task WhenCallEnterFromConfModeExpectResponse()
        {
            ConfigureEgm();

            var command = CreateCommand();
            command.Command.enable = true;
            command.IClass.timeToLive = (int)_sessionTimeout.TotalMilliseconds;
            _configurationModeSagaMock.Setup(m => m.Enabled(It.IsAny<IDevice>())).Returns(false);

            var handler = CreateHandler();

            await handler.Handle(command);

            _configurationModeSagaMock.Verify(
                x => x.Enter(
                    It.Is<IOptionConfigDevice>(d => d.Id == DeviceId),
                    DisableCondition.Idle,
                    _sessionTimeout,
                    It.Is<Func<string>>(m => m.Invoke() == DisableText),
                    true,
                    It.IsAny<Action<bool>>()),
                Times.Once);
        }

        [TestMethod]
        public void WhenConfigurationModeEnterSuccess()
        {
            var handler = CreateHandler();

            var commandQueueMock = new Mock<ICommandQueue>();

            var deviceMock = new Mock<IOptionConfigDevice>();
            deviceMock.SetupGet(x => x.DeviceClass).Returns("optionConfig");
            deviceMock.SetupGet(x => x.Id).Returns(DeviceId);
            deviceMock.SetupGet(x => x.Queue).Returns(commandQueueMock.Object);

            var command = CreateCommand();

            var privateObject = new PrivateObject(handler);

            privateObject.Invoke("ChangeConfigModeCallback", command, true, deviceMock.Object, true);

            _commandBuilderMock.Verify(
                x => x.Build(It.Is<IOptionConfigDevice>(d => d.Id == DeviceId), It.IsAny<optionConfigModeStatus>()),
                Times.Once);

            VerifyEventLift(EventCode.G2S_OCE007);
            VerifyEventLift(EventCode.G2S_OCE101);

            commandQueueMock.Verify(x => x.SendResponse(It.IsAny<ClassCommand<optionConfig, enterOptionConfigMode>>()));
        }

        [TestMethod]
        public void WhenConfigurationModeExitSuccess()
        {
            var handler = CreateHandler();

            var commandQueueMock = new Mock<ICommandQueue>();

            var deviceMock = new Mock<IOptionConfigDevice>();
            deviceMock.SetupGet(x => x.DeviceClass).Returns("optionConfig");
            deviceMock.SetupGet(x => x.Id).Returns(DeviceId);
            deviceMock.SetupGet(x => x.Queue).Returns(commandQueueMock.Object);

            var command = CreateCommand();

            var privateObject = new PrivateObject(handler);

            privateObject.Invoke("ChangeConfigModeCallback", command, true, deviceMock.Object, false);

            _commandBuilderMock.Verify(
                x => x.Build(It.Is<IOptionConfigDevice>(d => d.Id == DeviceId), It.IsAny<optionConfigModeStatus>()),
                Times.Once);

            VerifyEventLift(EventCode.G2S_OCE008);
            VerifyEventLift(EventCode.G2S_OCE102);

            commandQueueMock.Verify(x => x.SendResponse(It.IsAny<ClassCommand<optionConfig, enterOptionConfigMode>>()));
        }

        [TestMethod]
        public void WhenChangeConfigModeFailure()
        {
            var handler = CreateHandler();

            var commandQueueMock = new Mock<ICommandQueue>();

            var deviceMock = new Mock<IOptionConfigDevice>();
            deviceMock.SetupGet(x => x.DeviceClass).Returns("optionConfig");
            deviceMock.SetupGet(x => x.Id).Returns(DeviceId);
            deviceMock.SetupGet(x => x.Queue).Returns(commandQueueMock.Object);

            var command = CreateCommand();

            var privateObject = new PrivateObject(handler);

            privateObject.Invoke("ChangeConfigModeCallback", command, false, deviceMock.Object, true);

            Assert.IsTrue(command.Error.IsError);
            Assert.AreEqual(command.Error.Code, ErrorCode.G2S_APX011);

            commandQueueMock.Verify(x => x.SendResponse(It.IsAny<ClassCommand<optionConfig, enterOptionConfigMode>>()));
        }

        private void ConfigureEgm()
        {
            var commandQueueMock = new Mock<ICommandQueue>();
            commandQueueMock.SetupGet(x => x.SessionTimeout).Returns(_sessionTimeout);

            var deviceMock = new Mock<IOptionConfigDevice>();
            deviceMock.SetupGet(x => x.Id).Returns(DeviceId);
            deviceMock.SetupGet(x => x.Queue).Returns(commandQueueMock.Object);
            _egmMock.Setup(x => x.GetDevice<IOptionConfigDevice>(DeviceId)).Returns(deviceMock.Object);
        }

        private void VerifyEventLift(string eventCode)
        {
            _eventLiftMock.Verify(
                x =>
                    x.Report(
                        It.Is<IOptionConfigDevice>(d => d.Id == DeviceId),
                        eventCode,
                        It.Is<deviceList1>(
                            dl =>
                                dl.statusInfo.Length == 1 && dl.statusInfo.First().deviceId == DeviceId
                                                          && dl.statusInfo.First().deviceClass == "G2S_optionConfig")),
                Times.Once);
        }

        private EnterOptionConfigMode CreateHandler(IG2SEgm egm = null)
        {
            var handler = new EnterOptionConfigMode(
                egm ?? _egmMock.Object,
                _configurationModeSagaMock.Object,
                _commandBuilderMock.Object,
                _eventLiftMock.Object);

            return handler;
        }

        private ClassCommand<optionConfig, enterOptionConfigMode> CreateCommand()
        {
            var command =
                ClassCommandUtilities.CreateClassCommand<optionConfig, enterOptionConfigMode>(
                    TestConstants.HostId,
                    TestConstants.EgmId);
            command.Command.disableText = DisableText;
            command.Received = DateTime.UtcNow;
            command.IClass.timeToLive = (int)_sessionTimeout.TotalMilliseconds;

            return command;
        }
    }
}
