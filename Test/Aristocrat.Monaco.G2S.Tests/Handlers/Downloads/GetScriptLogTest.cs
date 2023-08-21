namespace Aristocrat.Monaco.G2S.Tests.Handlers.Downloads
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.PackageManager.Storage;
    using G2S.Handlers;
    using G2S.Handlers.Downloads;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;

    [TestClass]
    public class GetScriptLogTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new GetScriptLog(null, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullScriptCommandExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new GetScriptLog(egm.Object, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullContextFactoryExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var scriptCommand = new Mock<ICommandBuilder<IDownloadDevice, scriptLog>>();
            var handler = new GetScriptLog(egm.Object, scriptCommand.Object, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullRepositoryExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var scriptCommand = new Mock<ICommandBuilder<IDownloadDevice, scriptLog>>();
            var contextFactory = new Mock<IMonacoContextFactory>();
            var handler = new GetScriptLog(egm.Object, scriptCommand.Object, contextFactory.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var scriptCommand = new Mock<ICommandBuilder<IDownloadDevice, scriptLog>>();
            var contextFactory = new Mock<IMonacoContextFactory>();
            var repo = new Mock<IScriptRepository>();
            var handler = new GetScriptLog(egm.Object, scriptCommand.Object, contextFactory.Object, repo.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var scriptCommand = new Mock<ICommandBuilder<IDownloadDevice, scriptLog>>();
            var contextFactory = new Mock<IMonacoContextFactory>();
            var repo = new Mock<IScriptRepository>();
            var handler = new GetScriptLog(egm.Object, scriptCommand.Object, contextFactory.Object, repo.Object);
            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var egm = DownloadsUtilities.CreateMockEgm();
            var scriptCommand = new Mock<ICommandBuilder<IDownloadDevice, scriptLog>>();
            var contextFactory = new Mock<IMonacoContextFactory>();
            var repo = new Mock<IScriptRepository>();
            var handler = new GetScriptLog(egm, scriptCommand.Object, contextFactory.Object, repo.Object);
            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var egm = new Mock<IG2SEgm>();
            var evtDevice = new Mock<IDownloadDevice>();
            var queue = new Mock<ICommandQueue>();
            var scriptCommand = new Mock<ICommandBuilder<IDownloadDevice, scriptLog>>();
            var contextFactory = new Mock<IMonacoContextFactory>();
            var repo = new Mock<IScriptRepository>();
            queue.SetupGet(q => q.TimeToLiveBehavior).Returns(TimeToLiveBehavior.Strict);
            evtDevice.SetupGet(comms => comms.Queue).Returns(queue.Object);
            evtDevice.SetupGet(evt => evt.Owner).Returns(TestConstants.HostId);
            egm.Setup(e => e.GetDevice<IDownloadDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(evtDevice.Object);

            var handler = new GetScriptLog(egm.Object, scriptCommand.Object, contextFactory.Object, repo.Object);
            await VerificationTests.VerifyCanSucceed(handler);
        }
    }
}