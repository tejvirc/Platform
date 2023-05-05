namespace Aristocrat.Monaco.G2S.Tests.Handlers.Progressive
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.Monaco.G2S.Handlers.Progressive;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ProgressiveHostInfoHandlerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new ProgressiveHostInfoHandler(null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullProgressivesExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new ProgressiveHostInfoHandler(egm.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var progressives = new Mock<IProgressiveLevelProvider>();

            var handler = new ProgressiveHostInfoHandler(egm.Object, progressives.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var progressives = new Mock<IProgressiveLevelProvider>();

            var handler = new ProgressiveHostInfoHandler(egm.Object, progressives.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var progressives = new Mock<IProgressiveLevelProvider>();

            var handler = new ProgressiveHostInfoHandler(HandlerUtilities.CreateMockEgm<IProgressiveDevice>(), progressives.Object);

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<IProgressiveDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);
            var progressives = new Mock<IProgressiveLevelProvider>();

            var handler = new ProgressiveHostInfoHandler(HandlerUtilities.CreateMockEgm(device), progressives.Object);

            await VerificationTests.VerifyChecksForOwnerAndGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithGuestAccessExpectNoError()
        {
            const int otherHostId = 99;

            var device = new Mock<IProgressiveDevice>();
            device.SetupGet(d => d.Owner).Returns(otherHostId);
            device.SetupGet(d => d.Guests).Returns(new List<int> { TestConstants.HostId });
            var progressives = new Mock<IProgressiveLevelProvider>();

            var handler = new ProgressiveHostInfoHandler(HandlerUtilities.CreateMockEgm(device), progressives.Object);

            await VerificationTests.VerifyAllowsGuests(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<IProgressiveDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);
            var progressives = new Mock<IProgressiveLevelProvider>();

            var handler = new ProgressiveHostInfoHandler(HandlerUtilities.CreateMockEgm(device), progressives.Object);

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public void WhenHandlerHandledExpectCompletedTask()
        {
            var deviceMock = new Mock<IProgressiveDevice>();

            var egm = HandlerUtilities.CreateMockEgm(deviceMock);
            var progressives = new Mock<IProgressiveLevelProvider>();
            progressives.Setup(p => p.GetProgressiveLevels()).Returns(new List<ProgressiveLevel> { CreateEmptyProgressiveLevel() });

            var handler = new ProgressiveHostInfoHandler(egm, progressives.Object);

            var command = ClassCommandUtilities.CreateClassCommand<progressive, progressiveHostInfo>(TestConstants.HostId, TestConstants.EgmId);

            command.Command.progressiveLevel = new progressiveLevel[] {createEmptyG2SProgLevel()};

            Task result = handler.Handle(command);

            Assert.AreEqual(true, result.IsCompleted && !result.IsFaulted);
        }

        private progressiveLevel createEmptyG2SProgLevel()
        {
            progressiveLevel level = new progressiveLevel();
            level.incrementRate = 0;
            level.levelId = 0;
            level.mustBeWonByHigh = 0L;
            level.mustBeWonByLow = 0L;
            level.progId = 0;
            rangeHigh h = new rangeHigh();
            h.denomId = 0;
            h.numberOfCredits = 1;
            h.odds = 0;
            level.rangeHigh = h;
            rangeLow l = new rangeLow();
            l.denomId = 0;
            l.numberOfCredits = 1;
            l.odds = 0;
            level.rangeLow = l;
            level.resetValue = 0;

            return level;
        }

        private ProgressiveLevel CreateEmptyProgressiveLevel()
        {
            ProgressiveLevel l = new ProgressiveLevel();
            l.DeviceId = 0;
            return l;
        }
    }
}
