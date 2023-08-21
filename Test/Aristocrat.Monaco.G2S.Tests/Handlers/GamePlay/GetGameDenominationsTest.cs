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
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetGameDenominationsTest
    {
        private readonly Mock<ICommandBuilder<IGamePlayDevice, gameDenomList>> _builderMock =
            new Mock<ICommandBuilder<IGamePlayDevice, gameDenomList>>();

        private readonly Mock<IG2SEgm> _egmMock = new Mock<IG2SEgm>();

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetGameDenominations(null, _builderMock.Object);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCommandBuilderExpectException()
        {
            var handler = new GetGameDenominations(_egmMock.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var handler = new GetGameDenominations(_egmMock.Object, _builderMock.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var handler = new GetGameDenominations(_egmMock.Object, _builderMock.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<IGamePlayDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var handler = new GetGameDenominations(HandlerUtilities.CreateMockEgm(device), _builderMock.Object);

            await VerificationTests.VerifyChecksForOwnerAndGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithGuestAccessExpectNoError()
        {
            const int otherHostId = 99;

            var device = new Mock<IGamePlayDevice>();

            device.SetupGet(d => d.Owner).Returns(otherHostId);
            device.SetupGet(d => d.Guests).Returns(new List<int> { TestConstants.HostId });

            var handler = new GetGameDenominations(HandlerUtilities.CreateMockEgm(device), _builderMock.Object);

            await VerificationTests.VerifyAllowsGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<IGamePlayDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var handler = new GetGameDenominations(HandlerUtilities.CreateMockEgm(device), _builderMock.Object);

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenHandleExpectResponse()
        {
            var deviceId = 1;

            var device = new Mock<IGamePlayDevice>();
            device.SetupGet(x => x.Id).Returns(deviceId);

            _egmMock.Setup(x => x.GetDevice<IGamePlayDevice>(deviceId)).Returns(device.Object);

            var handler = new GetGameDenominations(_egmMock.Object, _builderMock.Object);
            var command = ClassCommandUtilities.CreateClassCommand<gamePlay, getGameDenoms>(
                TestConstants.HostId,
                TestConstants.EgmId);

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<gamePlay, gameDenomList>;
            Assert.IsNotNull(response);

            _builderMock.Verify(
                x => x.Build(It.Is<IGamePlayDevice>(d => d.Id == deviceId), It.IsAny<gameDenomList>()),
                Times.Once);
        }
    }
}