namespace Aristocrat.Monaco.G2S.Tests.Handlers.GamePlay
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers;
    using G2S.Handlers.GamePlay;
    using G2S.Services;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SetGamePlayStateTest
    {
        private readonly Mock<ICommandBuilder<IGamePlayDevice, gamePlayStatus>> _builder =
            new Mock<ICommandBuilder<IGamePlayDevice, gamePlayStatus>>();

        private readonly Mock<IDisableConditionSaga> _disableCondition = new Mock<IDisableConditionSaga>();

        private readonly Mock<IG2SEgm> _egm = new Mock<IG2SEgm>();
        private readonly Mock<IEventLift> _eventLift = new Mock<IEventLift>();
        private readonly Mock<IGameProvider> _gameProvider = new Mock<IGameProvider>();
        private readonly Mock<IPropertiesManager> _properties = new Mock<IPropertiesManager>();

        [TestInitialize]
        public void Initialize()
        {
            _properties.Setup(m => m.GetProperty(GamingConstants.StateChangeOverride, It.IsAny<DisableStrategy>()))
                .Returns(DisableStrategy.None);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new SetGamePlayState(null, null, null, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullGameProviderExpectException()
        {
            var handler = new SetGamePlayState(_egm.Object, null, null, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCommandBuilderExpectException()
        {
            var handler = new SetGamePlayState(
                _egm.Object,
                _disableCondition.Object,
                _gameProvider.Object,
                null,
                null,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmStateManagerExpectException()
        {
            var handler = new SetGamePlayState(
                _egm.Object,
                _disableCondition.Object,
                _gameProvider.Object,
                _builder.Object,
                null,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEventLiftExpectException()
        {
            var handler = new SetGamePlayState(
                _egm.Object,
                _disableCondition.Object,
                _gameProvider.Object,
                _builder.Object,
                null,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var handler = new SetGamePlayState(
                _egm.Object,
                _disableCondition.Object,
                _gameProvider.Object,
                _builder.Object,
                _properties.Object,
                _eventLift.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<IGamePlayDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var handler = new SetGamePlayState(
                HandlerUtilities.CreateMockEgm(device),
                _disableCondition.Object,
                _gameProvider.Object,
                _builder.Object,
                _properties.Object,
                _eventLift.Object);

            await VerificationTests.VerifyChecksForOwner(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithSetViaAccessConfigExpectError()
        {
            var device = new Mock<IGamePlayDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);
            device.SetupGet(d => d.SetViaAccessConfig).Returns(true);

            var handler = new SetGamePlayState(
                HandlerUtilities.CreateMockEgm(device),
                _disableCondition.Object,
                _gameProvider.Object,
                _builder.Object,
                _properties.Object,
                _eventLift.Object);

            await VerificationTests.VerifyReturnsError(handler, ErrorCode.G2S_GPX005);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            const int deviceId = 1;

            var device = new Mock<IGamePlayDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var game = new Mock<IGameDetail>();
            _gameProvider.Setup(m => m.GetGame(deviceId)).Returns(game.Object);
            _gameProvider.Setup(m => m.ValidateConfiguration(It.IsAny<IGameDetail>())).Returns(true);

            var command = CreateCommand();
            command.IClass.deviceId = deviceId;

            var handler = new SetGamePlayState(
                HandlerUtilities.CreateMockEgm(device),
                _disableCondition.Object,
                _gameProvider.Object,
                _builder.Object,
                _properties.Object,
                _eventLift.Object);

            await VerificationTests.VerifyCanSucceed(handler, command);
        }

        [TestMethod]
        public async Task WhenCommandEnableHandleExpectResponse()
        {
            const int deviceId = 1;

            var device = new Mock<IGamePlayDevice>();
            device.SetupGet(m => m.DeviceClass).Returns("G2S_gamePlay");
            device.SetupGet(x => x.Id).Returns(deviceId);
            device.SetupGet(x => x.HostEnabled).Returns(false);

            var game = new Mock<IGameDetail>();
            game.SetupGet(m => m.Status).Returns(GameStatus.DisabledByBackend);
            _gameProvider.Setup(m => m.GetGame(deviceId)).Returns(game.Object);

            var handler = new SetGamePlayState(
                HandlerUtilities.CreateMockEgm(device),
                _disableCondition.Object,
                _gameProvider.Object,
                _builder.Object,
                _properties.Object,
                _eventLift.Object);

            var command = CreateCommand();
            command.Command.applyCondition = "G2S_immediate";
            command.Command.enable = true;

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<gamePlay, gamePlayStatus>;
            Assert.IsNotNull(response);

            device.VerifySet(m => m.HostEnabled = true);
        }

        [TestMethod]
        public async Task WhenCommandDisableHandleExpectResponse()
        {
            const int deviceId = 1;

            var device = new Mock<IGamePlayDevice>();
            device.SetupGet(m => m.DeviceClass).Returns("G2S_gamePlay");
            device.SetupGet(x => x.Id).Returns(deviceId);
            device.SetupGet(x => x.RequiredForPlay).Returns(true);
            device.SetupGet(x => x.HostEnabled).Returns(true);

            var game = new Mock<IGameDetail>();
            game.SetupGet(m => m.Status).Returns(GameStatus.None);
            _gameProvider.Setup(m => m.GetGame(deviceId)).Returns(game.Object);

            var handler = new SetGamePlayState(
                HandlerUtilities.CreateMockEgm(device),
                _disableCondition.Object,
                _gameProvider.Object,
                _builder.Object,
                _properties.Object,
                _eventLift.Object);

            var command = CreateCommand();
            command.Command.applyCondition = "G2S_immediate";
            command.Command.enable = false;

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<gamePlay, gamePlayStatus>;
            Assert.IsNotNull(response);

            device.VerifySet(m => m.HostEnabled = false);
        }

        private ClassCommand<gamePlay, setGamePlayState> CreateCommand()
        {
            return ClassCommandUtilities.CreateClassCommand<gamePlay, setGamePlayState>(
                TestConstants.HostId,
                TestConstants.EgmId);
        }
    }
}
