namespace Aristocrat.Monaco.G2S.Tests.Handlers.Downloads
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers;
    using G2S.Handlers.Downloads;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetScriptStatusTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetScriptStatus(null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullScriptCommandExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var handler = new GetScriptStatus(egm.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var scriptCommand = new Mock<ICommandBuilder<IDownloadDevice, scriptStatus>>();
            var handler = new GetScriptStatus(egm.Object, scriptCommand.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var scriptCommand = new Mock<ICommandBuilder<IDownloadDevice, scriptStatus>>();
            var handler = new GetScriptStatus(egm.Object, scriptCommand.Object);
            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var egm = DownloadsUtilities.CreateMockEgm();
            var scriptCommand = new Mock<ICommandBuilder<IDownloadDevice, scriptStatus>>();
            var handler = new GetScriptStatus(egm, scriptCommand.Object);
            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var egm = new Mock<IG2SEgm>();
            var evtDevice = new Mock<IDownloadDevice>();
            var queue = new Mock<ICommandQueue>();

            queue.SetupGet(q => q.TimeToLiveBehavior).Returns(TimeToLiveBehavior.Strict);
            evtDevice.SetupGet(comms => comms.Queue).Returns(queue.Object);
            evtDevice.SetupGet(evt => evt.Owner).Returns(TestConstants.HostId);
            egm.Setup(e => e.GetDevice<IDownloadDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(evtDevice.Object);

            var scriptCommand = new Mock<ICommandBuilder<IDownloadDevice, scriptStatus>>();
            var handler = new GetScriptStatus(egm.Object, scriptCommand.Object);

            await VerificationTests.VerifyCanSucceed(handler);
        }
    }
}