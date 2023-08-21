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
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CreatePackageTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new CreatePackage(null, null, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullPackageManagerExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var handler = new CreatePackage(egm.Object, null, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEventLiftExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var packageManager = new Mock<IPackageManager>();
            var handler = new CreatePackage(egm.Object, packageManager.Object, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullIdProviderExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var packageManager = new Mock<IPackageManager>();
            var eventLift = new Mock<IEventLift>();
            var handler = new CreatePackage(egm.Object, packageManager.Object, eventLift.Object, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCommandBuilderExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var packageManager = new Mock<IPackageManager>();
            var eventLift = new Mock<IEventLift>();
            var idProvider = new Mock<IIdProvider>();
            var handler = new CreatePackage(
                egm.Object,
                packageManager.Object,
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
            var eventLift = new Mock<IEventLift>();
            var idProvider = new Mock<IIdProvider>();
            var commandBuilder = new Mock<ICommandBuilder<IDownloadDevice, packageStatus>>();
            var handler = new CreatePackage(
                egm.Object,
                packageManager.Object,
                eventLift.Object,
                idProvider.Object,
                commandBuilder.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var packageManager = new Mock<IPackageManager>();
            var eventLift = new Mock<IEventLift>();
            var idProvider = new Mock<IIdProvider>();
            var commandBuilder = new Mock<ICommandBuilder<IDownloadDevice, packageStatus>>();
            var handler = new CreatePackage(
                egm.Object,
                packageManager.Object,
                eventLift.Object,
                idProvider.Object,
                commandBuilder.Object);
            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var egm = DownloadsUtilities.CreateMockEgm();
            var packageManager = new Mock<IPackageManager>();
            var eventLift = new Mock<IEventLift>();
            var idProvider = new Mock<IIdProvider>();
            var commandBuilder = new Mock<ICommandBuilder<IDownloadDevice, packageStatus>>();

            var handler = new CreatePackage(
                egm,
                packageManager.Object,
                eventLift.Object,
                idProvider.Object,
                commandBuilder.Object);
            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var egm = new Mock<IG2SEgm>();
            var evtDevice = new Mock<IDownloadDevice>();
            var packageManager = new Mock<IPackageManager>();
            var queue = new Mock<ICommandQueue>();
            var eventLift = new Mock<IEventLift>();
            var idProvider = new Mock<IIdProvider>();
            queue.SetupGet(q => q.TimeToLiveBehavior).Returns(TimeToLiveBehavior.Strict);
            evtDevice.SetupGet(comms => comms.Queue).Returns(queue.Object);
            evtDevice.SetupGet(evt => evt.Owner).Returns(TestConstants.HostId);
            evtDevice.SetupGet(evt => evt.DownloadEnabled).Returns(true);
            egm.Setup(e => e.GetDevice<IDownloadDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(evtDevice.Object);

            var commandBuilder = new Mock<ICommandBuilder<IDownloadDevice, packageStatus>>();
            var handler = new CreatePackage(
                egm.Object,
                packageManager.Object,
                eventLift.Object,
                idProvider.Object,
                commandBuilder.Object);
            await VerificationTests.VerifyCanSucceed(handler);
        }
    }
}