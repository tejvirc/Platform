namespace Aristocrat.Monaco.G2S.Tests.Handlers.Downloads
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.PackageManager;
    using Common.PackageManager.CommandHandlers;
    using Common.PackageManager.Storage;
    using Data.Model;
    using G2S.Handlers;
    using G2S.Handlers.Downloads;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class AbortPackageTransferTest
    {
        private readonly Mock<IG2SEgm> _egmMock = new Mock<IG2SEgm>();

        private readonly Mock<IEventLift> _eventLiftMock = new Mock<IEventLift>();

        private readonly Mock<IPackageManager> _packageManagerMock = new Mock<IPackageManager>();

        private readonly Mock<ICommandBuilder<IDownloadDevice, packageStatus>> _statusCommandBuilderMock =
            new Mock<ICommandBuilder<IDownloadDevice, packageStatus>>();

        private readonly int deviceId = 1;
        private readonly string packageId = "G2S_packageId";

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new AbortPackageTransfer(
                null,
                _packageManagerMock.Object,
                _eventLiftMock.Object,
                _statusCommandBuilderMock.Object);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullPackageManagerExpectException()
        {
            var handler = new AbortPackageTransfer(
                _egmMock.Object,
                null,
                _eventLiftMock.Object,
                _statusCommandBuilderMock.Object);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEventLiftExpectException()
        {
            var handler = new AbortPackageTransfer(
                _egmMock.Object,
                _packageManagerMock.Object,
                null,
                _statusCommandBuilderMock.Object);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCommandBuilderExpectException()
        {
            var handler = new AbortPackageTransfer(
                _egmMock.Object,
                _packageManagerMock.Object,
                _eventLiftMock.Object,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var handler = new AbortPackageTransfer(
                _egmMock.Object,
                _packageManagerMock.Object,
                _eventLiftMock.Object,
                _statusCommandBuilderMock.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoEventHandlerDeviceExpectError()
        {
            var handler = new AbortPackageTransfer(
                _egmMock.Object,
                _packageManagerMock.Object,
                _eventLiftMock.Object,
                _statusCommandBuilderMock.Object);
            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var egm = DownloadsUtilities.CreateMockEgm();

            var handler = new AbortPackageTransfer(
                egm,
                _packageManagerMock.Object,
                _eventLiftMock.Object,
                _statusCommandBuilderMock.Object);
            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var evtDevice = new Mock<IDownloadDevice>();
            var queue = new Mock<ICommandQueue>();

            queue.SetupGet(q => q.TimeToLiveBehavior).Returns(TimeToLiveBehavior.Strict);
            evtDevice.SetupGet(comms => comms.Queue).Returns(queue.Object);
            evtDevice.SetupGet(evt => evt.Owner).Returns(TestConstants.HostId);
            _egmMock.Setup(e => e.GetDevice<IDownloadDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(evtDevice.Object);

            var handler = new AbortPackageTransfer(
                _egmMock.Object,
                _packageManagerMock.Object,
                _eventLiftMock.Object,
                _statusCommandBuilderMock.Object);
            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenSessionTypeIsG2SNotificationExpectNoResult()
        {
            var handler = new AbortPackageTransfer(
                _egmMock.Object,
                _packageManagerMock.Object,
                _eventLiftMock.Object,
                _statusCommandBuilderMock.Object);

            var command = CreateCommand();
            command.Class.sessionType = t_sessionTypes.G2S_notification;

            await handler.Handle(command);

            Assert.AreEqual(0, command.Responses.Count());
        }

        [TestMethod]
        public async Task WhenDeviceNotFoundExpectNoResult()
        {
            var handler = new AbortPackageTransfer(
                _egmMock.Object,
                _packageManagerMock.Object,
                _eventLiftMock.Object,
                _statusCommandBuilderMock.Object);

            var command = CreateCommand();

            await handler.Handle(command);
        }

        [TestMethod]
        public async Task WhenPackageNotFoundExpectError()
        {
            this.ConfigureEgm();

            var handler = new AbortPackageTransfer(
                _egmMock.Object,
                _packageManagerMock.Object,
                _eventLiftMock.Object,
                _statusCommandBuilderMock.Object);

            var command = CreateCommand();

            await handler.Handle(command);

            Assert.IsTrue(command.Error.IsError);
            Assert.AreEqual(command.Error.Code, ErrorCode.G2S_DLX001);
        }

        [TestMethod]
        public async Task WhenPackageNotTransferringExpectError()
        {
            ConfigureEgm();

            _packageManagerMock.Setup(x => x.HasPackage(packageId)).Returns(true);

            var handler = new AbortPackageTransfer(
                _egmMock.Object,
                _packageManagerMock.Object,
                _eventLiftMock.Object,
                _statusCommandBuilderMock.Object);

            var command = CreateCommand();
            command.Command.pkgId = packageId;

            await handler.Handle(command);

            Assert.IsTrue(command.Error.IsError);
            Assert.AreEqual(command.Error.Code, "GTK_DLX001");
        }

        [TestMethod]
        public async Task WhenTransferTypeIsDownloadExpectResponse()
        {
            ConfigureEgm();

            var cancellationTokenSource = new CancellationTokenSource();

            Assert.IsFalse(cancellationTokenSource.IsCancellationRequested);

            ConfigurePackageManager(TransferType.Download, cancellationTokenSource);

            Assert.AreEqual(_packageManagerMock.Object.PackageTaskAbortTokens.Count, 1);
            Assert.IsTrue(_packageManagerMock.Object.PackageTaskAbortTokens.ContainsKey(packageId));

            var handler = new AbortPackageTransfer(
                _egmMock.Object,
                _packageManagerMock.Object,
                _eventLiftMock.Object,
                _statusCommandBuilderMock.Object);

            var command = CreateCommand();
            command.Command.pkgId = packageId;

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<download, packageStatus>;

            Assert.IsNotNull(response);

            Assert.IsTrue(cancellationTokenSource.IsCancellationRequested);
            Assert.AreEqual(response.Command.pkgId, packageId);
            Assert.AreEqual(response.Command.pkgState, t_pkgStates.G2S_pending);
            Assert.AreEqual(_packageManagerMock.Object.PackageTaskAbortTokens.Count, 0);

            _packageManagerMock.Verify(
                x => x.DeletePackage(It.Is<DeletePackageArgs>(p => p.PackageId == packageId)),
                Times.Once);
        }

        private void ConfigurePackageManager(TransferType transferType, CancellationTokenSource cancellationTokenSource)
        {
            var transferEntity = new TransferEntity { TransferType = transferType };

            var packageLog = new PackageLog
            {
                PackageId = packageId,
                TransactionId = 1,
                Id = 1,
                DeviceId = deviceId
            };

            var package = new Package
            {
                Id = 1
            };

            var packageTransferLog = new packageTransferLog();

            _packageManagerMock.Setup(x => x.HasPackage(packageId)).Returns(true);
            _packageManagerMock.Setup(x => x.IsTransferring(packageId)).Returns(true);
            _packageManagerMock.Setup(x => x.GetTransferEntity(packageId)).Returns(transferEntity);
            _packageManagerMock.Setup(x => x.GetPackageEntity(packageId)).Returns(package);
            _packageManagerMock.Setup(x => x.ParseXml<packageTransferLog>(It.IsAny<string>()))
                .Returns(packageTransferLog);
            _packageManagerMock.SetupGet(x => x.PackageTaskAbortTokens)
                .Returns(
                    new Dictionary<string, CancellationTokenSource>
                    {
                        {
                            packageId, cancellationTokenSource
                        }
                    });
        }

        private void ConfigureEgm()
        {
            var device = new Mock<IDownloadDevice>();
            device.SetupGet(x => x.DeviceClass).Returns("download");
            _egmMock.Setup(x => x.GetDevice<IDownloadDevice>(deviceId)).Returns(device.Object);
        }

        private ClassCommand<download, abortPackageTransfer> CreateCommand()
        {
            var command = ClassCommandUtilities.CreateClassCommand<download, abortPackageTransfer>(
                TestConstants.HostId,
                TestConstants.EgmId);

            return command;
        }
    }
}