namespace Aristocrat.Monaco.G2S.Tests.Handlers.CoinAcceptor
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers;
    using G2S.Handlers.CoinAcceptor;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SetCoinAcceptorStateTest
    {
        private Mock<ICommandBuilder<ICoinAcceptor, coinAcceptorStatus>> _commandBuilderMock;
        private Mock<IG2SEgm> _egmMock;

        [TestInitialize]
        public void Initialiaze()
        {
            _egmMock = new Mock<IG2SEgm>();
            _commandBuilderMock = new Mock<ICommandBuilder<ICoinAcceptor, coinAcceptorStatus>>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new SetCoinAcceptorState(null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCommandBuilderExpectException()
        {
            var handler = new SetCoinAcceptorState(_egmMock.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithOwnerOnlyExpectNoError()
        {
            var device = new Mock<ICoinAcceptor>();
            var handler = CreateHander(device);

            await VerificationTests.VerifyChecksForOwner(handler);
        }

        [TestMethod]
        public async Task WhenHandlerWithEnableCommandExpectSuccess()
        {
            var deviceMock = new Mock<ICoinAcceptor>();
            deviceMock.SetupGet(m => m.HostEnabled).Returns(false);
            var handler = CreateHander(deviceMock.SetupAllProperties());
            var command = CreateCommand();

            command.Command.enable = true;

            await handler.Handle(command);

            _commandBuilderMock.Verify(m => m.Build(deviceMock.Object, It.IsAny<coinAcceptorStatus>()));
        }

        [TestMethod]
        public async Task WhenHandlerWithDisableCommandExpectSuccess()
        {
            var deviceMock = new Mock<ICoinAcceptor>();
            deviceMock.SetupGet(m => m.HostEnabled).Returns(true);
            var handler = CreateHander(deviceMock);
            var command = CreateCommand();

            command.Command.enable = false;

            await handler.Handle(command);

            _commandBuilderMock.Verify(m => m.Build(deviceMock.Object, It.IsAny<coinAcceptorStatus>()));
        }

        [TestMethod]
        public async Task WhenHandlerWithHostEnabledEqualsEnableCommandExpectNoStateChange()
        {
            var deviceMock = new Mock<ICoinAcceptor>();
            deviceMock.SetupGet(m => m.HostEnabled).Returns(true);
            var handler = CreateHander(deviceMock);
            var command = CreateCommand();

            command.Command.enable = true;

            await handler.Handle(command);

            _commandBuilderMock.Verify(m => m.Build(deviceMock.Object, It.IsAny<coinAcceptorStatus>()), Times.Once);
        }

        private SetCoinAcceptorState CreateHander(Mock<ICoinAcceptor> deviceMock = null)
        {
            return new SetCoinAcceptorState(
                deviceMock == null ? _egmMock.Object : HandlerUtilities.CreateMockEgm(deviceMock),
                _commandBuilderMock.Object);
        }

        private ClassCommand<coinAcceptor, setCoinAcceptorState> CreateCommand()
        {
            return ClassCommandUtilities.CreateClassCommand<coinAcceptor, setCoinAcceptorState>(
                TestConstants.HostId,
                TestConstants.EgmId);
        }
    }
}