namespace Aristocrat.Monaco.G2S.Tests.Handlers.GamePlay
{
    using System;
    using System.Collections.Generic;
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
    public class SetActiveDenominationsTest
    {
        private readonly Mock<ICommandBuilder<IGamePlayDevice, gameDenomList>> _denomListBuilder =
            new Mock<ICommandBuilder<IGamePlayDevice, gameDenomList>>();

        private readonly Mock<IDisableConditionSaga> _disableCondition = new Mock<IDisableConditionSaga>();
        private readonly Mock<IG2SEgm> _egm = new Mock<IG2SEgm>();
        private readonly Mock<IEventLift> _eventLift = new Mock<IEventLift>();

        private readonly Mock<ICommandBuilder<IGamePlayDevice, gamePlayStatus>> _gamePlayStatusBuilder =
            new Mock<ICommandBuilder<IGamePlayDevice, gamePlayStatus>>();

        private readonly Mock<IGameProvider> _gameProvider = new Mock<IGameProvider>();
        private readonly Mock<IPropertiesManager> _properties = new Mock<IPropertiesManager>();

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new SetActiveDenominations(null, null, null, null, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullGameProviderExpectException()
        {
            var handler = new SetActiveDenominations(
                _egm.Object,
                _disableCondition.Object,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCommandBuilderExpectException()
        {
            var handler = new SetActiveDenominations(
                _egm.Object,
                _disableCondition.Object,
                _gameProvider.Object,
                null,
                null,
                null,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var handler = new SetActiveDenominations(
                _egm.Object,
                _disableCondition.Object,
                _gameProvider.Object,
                _denomListBuilder.Object,
                _gamePlayStatusBuilder.Object,
                _properties.Object,
                _eventLift.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var handler = new SetActiveDenominations(
                _egm.Object,
                _disableCondition.Object,
                _gameProvider.Object,
                _denomListBuilder.Object,
                _gamePlayStatusBuilder.Object,
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

            var handler = new SetActiveDenominations(
                HandlerUtilities.CreateMockEgm(device),
                _disableCondition.Object,
                _gameProvider.Object,
                _denomListBuilder.Object,
                _gamePlayStatusBuilder.Object,
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

            var handler = new SetActiveDenominations(
                HandlerUtilities.CreateMockEgm(device),
                _disableCondition.Object,
                _gameProvider.Object,
                _denomListBuilder.Object,
                _gamePlayStatusBuilder.Object,
                _properties.Object,
                _eventLift.Object);

            await VerificationTests.VerifyReturnsError(handler, ErrorCode.G2S_GPX005);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidDenomsExpectError()
        {
            const int deviceId = 1;

            var device = new Mock<IGamePlayDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var command = ClassCommandUtilities.CreateClassCommand<gamePlay, setActiveDenoms>(
                TestConstants.HostId,
                TestConstants.EgmId);
            command.IClass.deviceId = deviceId;
            command.Command.Items = new object[]
                { new gameRange { denomMin = 5, denomMax = 10, denomInterval = 5 }, new activeDenom { denomId = 1 } };

            var game = new Mock<IGameDetail>();
            game.SetupGet(m => m.SupportedDenominations).Returns(new List<long> { 1, 5 });
            _gameProvider.Setup(m => m.GetGame(deviceId)).Returns(game.Object);
            _gameProvider.Setup(m => m.ValidateConfiguration(It.IsAny<IGameDetail>(), It.IsAny<IEnumerable<long>>())).Returns(true);

            var handler = new SetActiveDenominations(
                HandlerUtilities.CreateMockEgm(device),
                _disableCondition.Object,
                _gameProvider.Object,
                _denomListBuilder.Object,
                _gamePlayStatusBuilder.Object,
                _properties.Object,
                _eventLift.Object);

            await VerificationTests.VerifyReturnsError(handler, command, ErrorCode.G2S_GPX001);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            const int deviceId = 1;

            var device = new Mock<IGamePlayDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var command = ClassCommandUtilities.CreateClassCommand<gamePlay, setActiveDenoms>(
                TestConstants.HostId,
                TestConstants.EgmId);
            command.IClass.deviceId = deviceId;
            command.Command.Items = new object[]
                { new gameRange { denomMin = 5, denomMax = 10, denomInterval = 5 }, new activeDenom { denomId = 1 } };

            var game = new Mock<IGameDetail>();
            game.SetupGet(m => m.SupportedDenominations).Returns(new List<long> { 1, 5, 10 });
            _gameProvider.Setup(m => m.GetGame(deviceId)).Returns(game.Object);
            _gameProvider.Setup(m => m.ValidateConfiguration(It.IsAny<IGameDetail>(), It.IsAny<IEnumerable<long>>())).Returns(true);

            var handler = new SetActiveDenominations(
                HandlerUtilities.CreateMockEgm(device),
                _disableCondition.Object,
                _gameProvider.Object,
                _denomListBuilder.Object,
                _gamePlayStatusBuilder.Object,
                _properties.Object,
                _eventLift.Object);

            await VerificationTests.VerifyCanSucceed(handler, command);
        }

        [TestMethod]
        public async Task WhenHandleExpectResponse()
        {
            const int deviceId = 1;

            var device = new Mock<IGamePlayDevice>();
            device.SetupGet(m => m.DeviceClass).Returns("G2S_gamePlay");
            device.SetupGet(x => x.Id).Returns(deviceId);

            _egm.Setup(x => x.GetDevice<IGamePlayDevice>(deviceId)).Returns(device.Object);

            var game = new Mock<IGameDetail>();
            game.SetupGet(m => m.SupportedDenominations).Returns(new List<long> { 1, 5 });
            _gameProvider.Setup(m => m.GetGame(deviceId)).Returns(game.Object);
            _gameProvider.Setup(m => m.ValidateConfiguration(game.Object, It.IsAny<IEnumerable<long>>())).Returns(true);

            _properties.Setup(m => m.GetProperty(GamingConstants.StateChangeOverride, DisableStrategy.None))
                .Returns(DisableStrategy.None);

            var handler = new SetActiveDenominations(
                _egm.Object,
                _disableCondition.Object,
                _gameProvider.Object,
                _denomListBuilder.Object,
                _gamePlayStatusBuilder.Object,
                _properties.Object,
                _eventLift.Object);

            var command = ClassCommandUtilities.CreateClassCommand<gamePlay, setActiveDenoms>(
                TestConstants.HostId,
                TestConstants.EgmId);
            command.Command.applyCondition = "G2S_immediate";
            command.Command.Items = new object[]
                { new gameRange { denomMin = 1, denomMax = 3, denomInterval = 2 }, new activeDenom { denomId = 5 } };

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<gamePlay, gameDenomList>;
            Assert.IsNotNull(response);

            _gameProvider.Verify(
                x =>
                    x.SetActiveDenominations(
                        deviceId,
                        It.Is<IEnumerable<long>>(
                            list => (list.Count() == 3) && list.Contains(1) && list.Contains(3) && list.Contains(5))),
                Times.Once);

            _eventLift.Verify(e => e.Report(device.Object, EventCode.G2S_GPE201, It.IsAny<deviceList1>()));
            _eventLift.Verify(e => e.Report(device.Object, EventCode.G2S_GPE005, It.IsAny<deviceList1>()));

            _denomListBuilder.Verify(
                x => x.Build(It.Is<IGamePlayDevice>(d => d.Id == deviceId), It.IsAny<gameDenomList>()),
                Times.Once);
        }
    }
}