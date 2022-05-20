namespace Aristocrat.Monaco.G2S.Tests.Handlers.Chooser
{
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers.Chooser;
    using Gaming.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [TestClass]
    public class GetChooserProfileTest
    {
        private IG2SEgm _mockEgm;
        private Mock<IGameProvider> _mockProvider;
        private Mock<IGameOrderSettings> _mockOrder;

        [TestInitialize]
        public void Initialize()
        {
            _mockEgm = HandlerUtilities.CreateMockEgm<IChooserDevice>();
            _mockProvider = new Mock<IGameProvider>();
            _mockOrder = new Mock<IGameOrderSettings>();

            _mockProvider.Setup(m => m.GetGames()).Returns(new List<IGameDetail>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetChooserProfile(null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullProviderExpectException()
        {
            var handler = new GetChooserProfile(_mockEgm, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullOrderSettingsExpectException()
        {
            var handler = new GetChooserProfile(_mockEgm, _mockProvider.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var handler = new GetChooserProfile(_mockEgm, _mockProvider.Object, _mockOrder.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithOwnerAndGuestExpectSuccess()
        {
            var handler = new GetChooserProfile(_mockEgm, _mockProvider.Object, _mockOrder.Object);

            await VerificationTests.VerifyChecksForOwnerAndGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var handler = new GetChooserProfile(egm.Object, _mockProvider.Object, _mockOrder.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var handler = new GetChooserProfile(_mockEgm, _mockProvider.Object, _mockOrder.Object);

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<IChooserDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var handler = new GetChooserProfile(HandlerUtilities.CreateMockEgm(device), _mockProvider.Object, _mockOrder.Object);

            await VerificationTests.VerifyChecksForOwnerAndGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithGuestAccessExpectNoError()
        {
            const int otherHostId = 99;

            var device = new Mock<IChooserDevice>();
            device.SetupGet(d => d.Owner).Returns(otherHostId);
            device.SetupGet(d => d.Guests).Returns(new List<int> { TestConstants.HostId });

            var egm = HandlerUtilities.CreateMockEgm(device);

            var handler = new GetChooserProfile(egm, _mockProvider.Object, _mockOrder.Object);

            await VerificationTests.VerifyAllowsGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<IChooserDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var handler = new GetChooserProfile(HandlerUtilities.CreateMockEgm(device), _mockProvider.Object, _mockOrder.Object);

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenHandleCommandExpectChooserProfile()
        {
            var device = new Mock<IChooserDevice>();

            var command = ClassCommandUtilities.CreateClassCommand<chooser, getChooserProfile>(
                TestConstants.HostId,
                TestConstants.EgmId);

            var egm = new Mock<IG2SEgm>();
            egm.Setup(e => e.GetDevice<IChooserDevice>(command.IClass.deviceId)).Returns(device.Object);

            var handler = new GetChooserProfile(egm.Object, _mockProvider.Object, _mockOrder.Object);

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<chooser, chooserProfile>;

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Command.configurationId);
            Assert.IsNotNull(response.Command.configDateTime);
            Assert.IsNotNull(response.Command.configComplete);
            Assert.IsNotNull(response.Command.useDefaultConfig);
            Assert.IsNotNull(response.Command.gameComboData);
        }
    }
}
