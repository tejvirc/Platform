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
    public class UploadPackageTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new UploadPackage(null, null, null, null, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullPackageManagerExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var handler = new UploadPackage(egm.Object, null, null, null, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEventBusExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var packageManager = new Mock<IPackageManager>();
            var handler = new UploadPackage(egm.Object, packageManager.Object, null, null, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullGatServiceExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var packageManager = new Mock<IPackageManager>();
            var eventBus = new Mock<IEventBus>();
            var handler = new UploadPackage(
                egm.Object,
                packageManager.Object,
                eventBus.Object,
                null,
                null,
                null,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEventLiftExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var packageManager = new Mock<IPackageManager>();
            var eventBus = new Mock<IEventBus>();
            var gatService = new Mock<IGatService>();
            var handler = new UploadPackage(
                egm.Object,
                packageManager.Object,
                eventBus.Object,
                gatService.Object,
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
            var eventBus = new Mock<IEventBus>();
            var gatService = new Mock<IGatService>();
            var eventLift = new Mock<IEventLift>();
            var handler = new UploadPackage(
                egm.Object,
                packageManager.Object,
                eventBus.Object,
                gatService.Object,
                eventLift.Object,
                null,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCommandBuilderExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var packageManager = new Mock<IPackageManager>();
            var eventBus = new Mock<IEventBus>();
            var gatService = new Mock<IGatService>();
            var eventLift = new Mock<IEventLift>();
            var idProvider = new Mock<IIdProvider>();
            var handler = new UploadPackage(
                egm.Object,
                packageManager.Object,
                eventBus.Object,
                gatService.Object,
                eventLift.Object,
                idProvider.Object,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var packageManager = new Mock<IPackageManager>();
            var eventBus = new Mock<IEventBus>();
            var gatService = new Mock<IGatService>();
            var eventLift = new Mock<IEventLift>();
            var idProvider = new Mock<IIdProvider>();
            var statusCommand = new Mock<ICommandBuilder<IDownloadDevice, packageStatus>>();

            var handler = new UploadPackage(
                egm.Object,
                packageManager.Object,
                eventBus.Object,
                gatService.Object,
                eventLift.Object,
                idProvider.Object,
                statusCommand.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var packageManager = new Mock<IPackageManager>();
            var eventBus = new Mock<IEventBus>();
            var gatService = new Mock<IGatService>();
            var eventLift = new Mock<IEventLift>();
            var idProvider = new Mock<IIdProvider>();
            var statusCommand = new Mock<ICommandBuilder<IDownloadDevice, packageStatus>>();

            var handler = new UploadPackage(
                egm.Object,
                packageManager.Object,
                eventBus.Object,
                gatService.Object,
                eventLift.Object,
                idProvider.Object,
                statusCommand.Object);
            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var egm = DownloadsUtilities.CreateMockEgm();
            var packageManager = new Mock<IPackageManager>();
            var eventBus = new Mock<IEventBus>();
            var gatService = new Mock<IGatService>();
            var eventLift = new Mock<IEventLift>();
            var idProvider = new Mock<IIdProvider>();
            var statusCommand = new Mock<ICommandBuilder<IDownloadDevice, packageStatus>>();

            var handler = new UploadPackage(
                egm,
                packageManager.Object,
                eventBus.Object,
                gatService.Object,
                eventLift.Object,
                idProvider.Object,
                statusCommand.Object);
            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var egm = new Mock<IG2SEgm>();
            var evtDevice = new Mock<IDownloadDevice>();
            var packageManager = new Mock<IPackageManager>();
            var queue = new Mock<ICommandQueue>();
            var eventBus = new Mock<IEventBus>();
            var gatService = new Mock<IGatService>();
            var eventLift = new Mock<IEventLift>();
            var idProvider = new Mock<IIdProvider>();
            var statusCommand = new Mock<ICommandBuilder<IDownloadDevice, packageStatus>>();

            queue.SetupGet(q => q.TimeToLiveBehavior).Returns(TimeToLiveBehavior.Strict);
            evtDevice.SetupGet(comms => comms.Queue).Returns(queue.Object);
            evtDevice.SetupGet(evt => evt.Owner).Returns(TestConstants.HostId);
            evtDevice.SetupGet(evt => evt.UploadEnabled).Returns(true);
            egm.Setup(e => e.GetDevice<IDownloadDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(evtDevice.Object);

            packageManager.Setup(p => p.HasPackage(It.IsAny<string>())).Returns(true);

            var handler = new UploadPackage(
                egm.Object,
                packageManager.Object,
                eventBus.Object,
                gatService.Object,
                eventLift.Object,
                idProvider.Object,
                statusCommand.Object);
            await VerificationTests.VerifyCanSucceed(handler);
        }
    }
}