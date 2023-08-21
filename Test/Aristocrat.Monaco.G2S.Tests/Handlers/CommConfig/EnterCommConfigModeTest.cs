namespace Aristocrat.Monaco.G2S.Tests.Handlers.CommConfig
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using G2S.Handlers;
    using G2S.Handlers.CommConfig;
    using G2S.Services;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using DeviceClass = Aristocrat.G2S.DeviceClass;

    [TestClass]
    public class EnterCommConfigModeTest
    {
        private Mock<ICommandBuilder<ICommConfigDevice, commConfigModeStatus>> _commandBuilderMock;

        private Mock<IDisableConditionSaga> _configurationModeSagaMock;
        private Mock<IG2SEgm> _egmMock;

        private Mock<IEventLift> _eventLiftMock;

        [TestInitialize]
        public void Initialize()
        {
            _egmMock = new Mock<IG2SEgm>();
            _configurationModeSagaMock = new Mock<IDisableConditionSaga>();
            _commandBuilderMock = new Mock<ICommandBuilder<ICommConfigDevice, commConfigModeStatus>>();
            _eventLiftMock = new Mock<IEventLift>();
        }

        [TestMethod]
        public void WhenConstructWithNullsExpectException()
        {
            ConstructorTest.TestConstructorNullChecks<EnterCommConfigMode>();
        }

        [TestMethod]
        public async Task WhenVerifyWithOwnerDeviceExpectSuccess()
        {
            var egm = HandlerUtilities.CreateMockEgm<ICommConfigDevice>();
            var handler = CreateHandler(egm);

            await VerificationTests.VerifyChecksForOwner(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithDisableConditionNoneExpectError()
        {
            var device = CreateCommConfigDeviceMock();
            var egm = HandlerUtilities.CreateMockEgm(device);
            var handler = CreateHandler(egm);

            var command = CreateCommand();

            command.Command.disableCondition = DisableCondition.None.ToG2SString();

            var error = await handler.Verify(command);

            Assert.AreEqual(error.Code, ErrorCode.G2S_CCX018);
        }

        [TestMethod]
        public async Task WhenVerifyWithDisableAndDisableConditionZeroCreditsExpectError()
        {
            var device = CreateCommConfigDeviceMock();
            var egm = HandlerUtilities.CreateMockEgm(device);
            var handler = CreateHandler(egm);

            var command = CreateCommand();

            command.Command.enable = false;
            command.Command.disableCondition = DisableCondition.ZeroCredits.ToG2SString();

            var error = await handler.Verify(command);

            Assert.AreEqual(error.Code, ErrorCode.G2S_CCX018);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoErrorExpectNull()
        {
            var deviceMock = CreateCommConfigDeviceMock();
            var egm = HandlerUtilities.CreateMockEgm(deviceMock);
            var handler = CreateHandler(egm);

            var command = CreateCommand();

            command.Command.enable = true;
            command.Command.disableCondition = DisableCondition.ZeroCredits.ToG2SString();

            var error = await handler.Verify(command);

            Assert.IsNull(error);
        }

        [TestMethod]
        public async Task WhenHandleWithCommandEnableEqualConfigurationModeSagaEnabledExpectResponse()
        {
            _configurationModeSagaMock.Setup(m => m.Enabled(It.IsAny<IDevice>())).Returns(true);

            var deviceMock = CreateCommConfigDeviceMock();
            var egm = HandlerUtilities.CreateMockEgm(deviceMock);
            var handler = CreateHandler(egm);

            var command = CreateCommand();
            command.Command.enable = true;

            await handler.Handle(command);

            _commandBuilderMock.Verify(m => m.Build(deviceMock.Object, It.IsAny<commConfigModeStatus>()));

            Assert.AreEqual(1, command.Responses.Count());
        }

        [TestMethod]
        public async Task WhenHandleWithCommandDisableWithSuccessExpectResponse()
        {
            _configurationModeSagaMock.Setup(m => m.Enabled(It.IsAny<IDevice>())).Returns(true);

            var deviceMock = CreateCommConfigDeviceMock();

            var command = CreateCommand();
            command.Command.enable = false;

            _configurationModeSagaMock
                .Setup(
                    m => m.Exit(
                        deviceMock.Object,
                        It.IsAny<DisableCondition>(),
                        It.IsAny<TimeSpan>(),
                        It.IsAny<Action<bool>>()))
                .Callback<IDevice, DisableCondition, TimeSpan, Action<bool>>(
                    (arg1, arg2, arg3, onEnable) => onEnable(true));

            var egm = HandlerUtilities.CreateMockEgm(deviceMock);
            var handler = CreateHandler(egm);

            await handler.Handle(command);

            _eventLiftMock.Verify(m => m.Report(deviceMock.Object, EventCode.G2S_CCE008, It.IsAny<deviceList1>()));
            _eventLiftMock.Verify(m => m.Report(deviceMock.Object, EventCode.G2S_CCE102, It.IsAny<deviceList1>()));
            _commandBuilderMock.Verify(m => m.Build(deviceMock.Object, It.IsAny<commConfigModeStatus>()));

            Assert.AreEqual(1, command.Responses.Count());
        }

        [TestMethod]
        public async Task WhenHandleWithCommandDisableWithFailExpectNoResponse()
        {
            _configurationModeSagaMock.Setup(m => m.Enabled(It.IsAny<IDevice>())).Returns(true);

            var deviceMock = CreateCommConfigDeviceMock();

            var command = CreateCommand();
            command.Command.enable = false;

            _configurationModeSagaMock
                .Setup(
                    m => m.Exit(
                        deviceMock.Object,
                        It.IsAny<DisableCondition>(),
                        It.IsAny<TimeSpan>(),
                        It.IsAny<Action<bool>>()))
                .Callback<IDevice, DisableCondition, TimeSpan, Action<bool>>(
                    (arg1, arg2, arg3, onEnable) => onEnable(false));

            var egm = HandlerUtilities.CreateMockEgm(deviceMock);
            var handler = CreateHandler(egm);

            await handler.Handle(command);

            _eventLiftMock
                .Verify(m => m.Report(It.IsAny<IDevice>(), It.IsAny<string>(), It.IsAny<deviceList1>()), Times.Never);
            _commandBuilderMock
                .Verify(m => m.Build(It.IsAny<ICommConfigDevice>(), It.IsAny<commConfigModeStatus>()), Times.Never);

            Assert.AreEqual(0, command.Responses.Count());
        }

        private EnterCommConfigMode CreateHandler(IG2SEgm egm = null)
        {
            return new EnterCommConfigMode(
                egm ?? _egmMock.Object,
                _configurationModeSagaMock.Object,
                _commandBuilderMock.Object,
                _eventLiftMock.Object);
        }

        private ClassCommand<commConfig, enterCommConfigMode> CreateCommand()
        {
            return ClassCommandUtilities.CreateClassCommand<commConfig, enterCommConfigMode>(
                TestConstants.HostId,
                TestConstants.EgmId);
        }

        private Mock<ICommConfigDevice> CreateCommConfigDeviceMock()
        {
            var device = new Mock<ICommConfigDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);
            device.SetupGet(d => d.Id).Returns(TestConstants.HostId);
            device.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_commConfig);

            return device;
        }
    }
}
