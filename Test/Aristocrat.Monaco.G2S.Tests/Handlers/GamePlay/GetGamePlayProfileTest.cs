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
    using G2S.Handlers.GamePlay;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetGamePlayProfileTest
    {
        private readonly Mock<IG2SEgm> _egm = new Mock<IG2SEgm>();
        private readonly Mock<IGameHistory> _history = new Mock<IGameHistory>();
        private readonly Mock<IGameProvider> _provider = new Mock<IGameProvider>();
        private readonly Mock<IProtocolLinkedProgressiveAdapter> _progressives = new Mock<IProtocolLinkedProgressiveAdapter>();

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetGamePlayProfile(null, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullGameProviderExpectException()
        {
            var handler = new GetGamePlayProfile(_egm.Object, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullGameHistoryExpectException()
        {
            var handler = new GetGamePlayProfile(_egm.Object, _provider.Object, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var handler = new GetGamePlayProfile(_egm.Object, _provider.Object, _history.Object, _progressives.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var handler = new GetGamePlayProfile(_egm.Object, _provider.Object, _history.Object, _progressives.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<IGamePlayDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var handler = new GetGamePlayProfile(
                HandlerUtilities.CreateMockEgm(device),
                _provider.Object,
                _history.Object,
                _progressives.Object);

            await VerificationTests.VerifyChecksForOwnerAndGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithGuestAccessExpectNoError()
        {
            const int otherHostId = 99;

            var device = new Mock<IGamePlayDevice>();

            device.SetupGet(d => d.Owner).Returns(otherHostId);
            device.SetupGet(d => d.Guests).Returns(new List<int> { TestConstants.HostId });

            var handler = new GetGamePlayProfile(
                HandlerUtilities.CreateMockEgm(device),
                _provider.Object,
                _history.Object,
                _progressives.Object);

            await VerificationTests.VerifyAllowsGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<IGamePlayDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var handler = new GetGamePlayProfile(
                HandlerUtilities.CreateMockEgm(device),
                _provider.Object,
                _history.Object,
                _progressives.Object);

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenHandleExpectResponse()
        {
            const int deviceId = 1;

            var device = new Mock<IGamePlayDevice>();
            device.SetupGet(x => x.Id).Returns(deviceId);

            _egm.Setup(x => x.GetDevice<IGamePlayDevice>(deviceId)).Returns(device.Object);

            var game = new Mock<IGameDetail>();
            game.SetupGet(m => m.ThemeId).Returns("ATI_Test");
            game.SetupGet(m => m.PaytableId).Returns("ATI_VAR001");
            game.Setup(m => m.MaximumWagerCredits).Returns(5);
            _provider.Setup(m => m.GetGame(deviceId)).Returns(game.Object);

            _history.Setup(m => m.MaxEntries).Returns(500);

            var handler = new GetGamePlayProfile(_egm.Object, _provider.Object, _history.Object, _progressives.Object);
            var command = ClassCommandUtilities.CreateClassCommand<gamePlay, getGamePlayProfile>(
                TestConstants.HostId,
                TestConstants.EgmId);

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<gamePlay, gamePlayProfile>;
            Assert.IsNotNull(response);
        }
    }
}
