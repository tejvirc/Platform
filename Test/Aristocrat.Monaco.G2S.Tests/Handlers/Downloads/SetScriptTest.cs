namespace Aristocrat.Monaco.G2S.Tests.Handlers.Downloads
{
    using System;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.PackageManager;
    using G2S.Handlers;
    using G2S.Handlers.Downloads;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SetScriptTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new SetScript(null, null, null, null, null, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullPackageManagerExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var handler = new SetScript(egm.Object, null, null, null, null, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullScriptManagerExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var packageManager = new Mock<IPackageManager>();
            var handler = new SetScript(egm.Object, packageManager.Object, null, null, null, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEventLiftExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var packageManager = new Mock<IPackageManager>();
            var scriptManager = new Mock<IScriptManager>();
            var handler = new SetScript(
                egm.Object,
                packageManager.Object,
                scriptManager.Object,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullIdProviderExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var packageManager = new Mock<IPackageManager>();
            var scriptManager = new Mock<IScriptManager>();
            var eventLift = new Mock<IEventLift>();
            var handler = new SetScript(
                egm.Object,
                packageManager.Object,
                scriptManager.Object,
                eventLift.Object,
                null,
                null,
                null,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullScriptCommandExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var packageManager = new Mock<IPackageManager>();
            var scriptManager = new Mock<IScriptManager>();
            var eventLift = new Mock<IEventLift>();
            var idProvider = new Mock<IIdProvider>();
            var properties = new Mock<IPropertiesManager>();
            var handler = new SetScript(
                egm.Object,
                packageManager.Object,
                scriptManager.Object,
                eventLift.Object,
                idProvider.Object,
                properties.Object,
                null,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullScriptLogCommandExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var packageManager = new Mock<IPackageManager>();
            var scriptManager = new Mock<IScriptManager>();
            var eventLift = new Mock<IEventLift>();
            var idProvider = new Mock<IIdProvider>();
            var scriptBuilder = new Mock<ICommandBuilder<IDownloadDevice, scriptStatus>>();
            var properties = new Mock<IPropertiesManager>();
            var handler = new SetScript(
                egm.Object,
                packageManager.Object,
                scriptManager.Object,
                eventLift.Object,
                idProvider.Object,
                properties.Object,
                scriptBuilder.Object,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var packageManager = new Mock<IPackageManager>();
            var scriptManager = new Mock<IScriptManager>();
            var eventLift = new Mock<IEventLift>();
            var idProvider = new Mock<IIdProvider>();
            var properties = new Mock<IPropertiesManager>();
            var scriptBuilder = new Mock<ICommandBuilder<IDownloadDevice, scriptStatus>>();
            var scriptLogBuilder = new Mock<ICommandBuilder<IDownloadDevice, scriptLog>>();
            var handler = new SetScript(
                egm.Object,
                packageManager.Object,
                scriptManager.Object,
                eventLift.Object,
                idProvider.Object,
                properties.Object,
                scriptBuilder.Object,
                scriptLogBuilder.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var packageManager = new Mock<IPackageManager>();
            var scriptManager = new Mock<IScriptManager>();
            var eventLift = new Mock<IEventLift>();
            var idProvider = new Mock<IIdProvider>();
            var properties = new Mock<IPropertiesManager>();
            var scriptBuilder = new Mock<ICommandBuilder<IDownloadDevice, scriptStatus>>();
            var scriptLogBuilder = new Mock<ICommandBuilder<IDownloadDevice, scriptLog>>();
            var handler = new SetScript(
                egm.Object,
                packageManager.Object,
                scriptManager.Object,
                eventLift.Object,
                idProvider.Object,
                properties.Object,
                scriptBuilder.Object,
                scriptLogBuilder.Object);
            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var egm = DownloadsUtilities.CreateMockEgm();
            var packageManager = new Mock<IPackageManager>();
            var scriptManager = new Mock<IScriptManager>();
            var eventLift = new Mock<IEventLift>();
            var idProvider = new Mock<IIdProvider>();
            var properties = new Mock<IPropertiesManager>();
            var scriptBuilder = new Mock<ICommandBuilder<IDownloadDevice, scriptStatus>>();
            var scriptLogBuilder = new Mock<ICommandBuilder<IDownloadDevice, scriptLog>>();
            var handler = new SetScript(
                egm,
                packageManager.Object,
                scriptManager.Object,
                eventLift.Object,
                idProvider.Object,
                properties.Object,
                scriptBuilder.Object,
                scriptLogBuilder.Object);
            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var egm = new Mock<IG2SEgm>();
            var evtDevice = new Mock<IDownloadDevice>();
            var queue = new Mock<ICommandQueue>();
            var packageManager = new Mock<IPackageManager>();
            var scriptManager = new Mock<IScriptManager>();
            var eventLift = new Mock<IEventLift>();
            queue.SetupGet(q => q.TimeToLiveBehavior).Returns(TimeToLiveBehavior.Strict);
            evtDevice.SetupGet(comms => comms.Queue).Returns(queue.Object);
            evtDevice.SetupGet(evt => evt.Owner).Returns(TestConstants.HostId);
            evtDevice.SetupGet(evt => evt.DownloadEnabled).Returns(true);
            egm.Setup(e => e.GetDevice<IDownloadDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(evtDevice.Object);
            var idProvider = new Mock<IIdProvider>();
            var properties = new Mock<IPropertiesManager>();
            properties.Setup(m => m.GetProperty(ApplicationConstants.DeletePackageAfterInstall, It.IsAny<bool>())).Returns(false);
            var scriptBuilder = new Mock<ICommandBuilder<IDownloadDevice, scriptStatus>>();
            var scriptLogBuilder = new Mock<ICommandBuilder<IDownloadDevice, scriptLog>>();
            var handler = new SetScript(
                egm.Object,
                packageManager.Object,
                scriptManager.Object,
                eventLift.Object,
                idProvider.Object,
                properties.Object,
                scriptBuilder.Object,
                scriptLogBuilder.Object);
            await VerificationTests.VerifyCanSucceed(handler);
        }
    }
}