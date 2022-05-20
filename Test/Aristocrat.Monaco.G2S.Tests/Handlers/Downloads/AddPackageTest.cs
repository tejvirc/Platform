namespace Aristocrat.Monaco.G2S.Tests.Handlers.Downloads
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.Monaco.Protocol.Common.Installer;
    using Common.PackageManager;
    using Common.PackageManager.CommandHandlers;
    using Common.PackageManager.Storage;
    using Data.Model;
    using G2S.Handlers;
    using G2S.Handlers.Downloads;
    using Kernel;
    using Kernel.Contracts.Components;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class AddPackageTest
    {
        private const int DeviceId = 1;

        private const string PackageId = "G2S_packageId";
        private readonly Mock<IG2SEgm> _egmMock = new Mock<IG2SEgm>();
        private readonly Mock<IEventBus> _eventBusMock = new Mock<IEventBus>();
        private readonly Mock<IEventLift> _eventLiftMock = new Mock<IEventLift>();
        private readonly Mock<IGatService> _gatServiceMock = new Mock<IGatService>();

        private readonly Mock<ICommandBuilder<IDownloadDevice, packageLog>> _logCommandBuilderMock =
            new Mock<ICommandBuilder<IDownloadDevice, packageLog>>();

        private readonly Mock<IPackageManager> _packageManagerMock = new Mock<IPackageManager>();

        private readonly Mock<ICommandBuilder<IDownloadDevice, packageStatus>> _statusCommandBuilderMock =
            new Mock<ICommandBuilder<IDownloadDevice, packageStatus>>();


        private readonly Mock<IIdProvider> _idProviderMock = new Mock<IIdProvider>();

        private readonly Mock<IPackageDownloadManager> _packageDownloadManagerMock = new Mock<IPackageDownloadManager>();
        private readonly Mock<IInstallerService> _installerServiceMock = new Mock<IInstallerService>();

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new AddPackage(null, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullPackageManagerExpectException()
        {
            var handler = new AddPackage(_egmMock.Object, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullPackageDownloadManagerException()
        {
            var handler = new AddPackage(_egmMock.Object, _packageManagerMock.Object, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullGatServiceExpectException()
        {
            var handler = new AddPackage(
                _egmMock.Object,
                _packageManagerMock.Object,
                _packageDownloadManagerMock.Object,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var handler = new AddPackage(
                _egmMock.Object,
                _packageManagerMock.Object,
                _packageDownloadManagerMock.Object,
                _statusCommandBuilderMock.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoEventHandlerDeviceExpectError()
        {
            var handler = new AddPackage(
                _egmMock.Object,
                _packageManagerMock.Object,
                _packageDownloadManagerMock.Object,
                _statusCommandBuilderMock.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var egm = DownloadsUtilities.CreateMockEgm();
            var handler = new AddPackage(
                egm,
                _packageManagerMock.Object,
                _packageDownloadManagerMock.Object,
                _statusCommandBuilderMock.Object);

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
            evtDevice.SetupGet(evt => evt.DownloadEnabled).Returns(true);
            egm.Setup(e => e.GetDevice<IDownloadDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(evtDevice.Object);

            var handler = new AddPackage(
                egm.Object,
                _packageManagerMock.Object,
                _packageDownloadManagerMock.Object,
                _statusCommandBuilderMock.Object);

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenSessionTypeIsG2SNotificationExpectNoResult()
        {
            var handler = CreateHandler();

            var command = CreateCommand();

            command.Class.sessionType = t_sessionTypes.G2S_notification;

            await handler.Handle(command);

            Assert.AreEqual(0, command.Responses.Count());
        }

        [TestMethod]
        public async Task WhenDeviceNotFoundExpectNoResult()
        {
            var handler = CreateHandler();

            var command = CreateCommand();

            await handler.Handle(command);

            Assert.AreEqual(0, command.Responses.Count());
        }

        [TestMethod]
        public async Task WhenPackageExistsAndNotTransferringExpectError()
        {
            ConfigureEgm();

            ConfigurePackageManager(hasPackage: true);

            var handler = CreateHandler();

            var command = CreateCommand();
            command.Command.pkgId = PackageId;

            await handler.Handle(command);

            Assert.IsTrue(command.Error.IsError);
            Assert.AreEqual(ErrorCode.G2S_DLX003, command.Error.Code);
        }

        [TestMethod]
        public async Task WhenPackageExistsAndTransferringExpectError()
        {
            ConfigureEgm();

            ConfigurePackageManager(hasPackage: true, isTransferring: true);

            var handler = CreateHandler();

            var command = CreateCommand();
            command.Command.pkgId = PackageId;

            await handler.Handle(command);

            Assert.IsTrue(command.Error.IsError);
            Assert.AreEqual(command.Error.Code, ErrorCode.G2S_DLX002);
        }

        [TestMethod]
        public void WhenTransferStateCompletedExpectSuccess()
        {
            var device = new Mock<IDownloadDevice>();
            device.SetupGet(x => x.DeviceClass).Returns("download");

            ConfigurePackageManager(hasPackage: true);

            PackageDownloadManager packageDownloadManager = new PackageDownloadManager(
                _egmMock.Object,
                _packageManagerMock.Object,
                _eventBusMock.Object,
                _gatServiceMock.Object,
                _eventLiftMock.Object,
                _statusCommandBuilderMock.Object,
                _idProviderMock.Object,
                _installerServiceMock.Object);

            var privateObject = new PrivateObject(packageDownloadManager);

            privateObject.Invoke(
                "UpdatePackageStatus",
                new PackageTransferEventArgs { PackageId = PackageId, TransferState = TransferState.Completed },
                device.Object);

            _eventBusMock.Verify(x => x.Publish(It.IsAny<PackageDownloadCompleteEvent>()), Times.Once);

            // This was supposed to run twice, but the AddPackageHandler was updated to only send one package status to the host
            device.Verify(
                x =>
                    x.SendStatus(It.IsAny<packageStatus>(), null),
                Times.Exactly(1));

            _packageManagerMock.Verify(
                x => x.UpdateTransferEntity(It.IsAny<TransferEntity>()),
                Times.Exactly(3));

            Assert.AreEqual(_packageManagerMock.Object.PackageTaskAbortTokens.Count, 0);

            _packageManagerMock.Verify(
                x => x.UpdatePackage(It.Is<PackageLog>(log => log.PackageId == PackageId)),
                Times.Exactly(2));

            _gatServiceMock.Verify(x => x.SaveComponent(It.Is<Component>(c => c.ComponentId == PackageId)));
        }

        [TestMethod]
        public void WhenTransferStateFailedExpectSuccess()
        {
            var device = new Mock<IDownloadDevice>();
            device.SetupGet(x => x.DeviceClass).Returns("download");

            _egmMock.Setup(egm => egm.GetDevice<IDownloadDevice>()).Returns(device.Object);

            ConfigurePackageManager(hasPackage: true);

            _packageManagerMock.Setup(x => x.DeletePackage(It.IsAny<DeletePackageArgs>())).Callback(
                (DeletePackageArgs args) => { args.DeletePackageCallback(args); }).Verifiable();

            PackageDownloadManager packageDownloadManager = new PackageDownloadManager(
                _egmMock.Object,
                _packageManagerMock.Object,
                _eventBusMock.Object,
                _gatServiceMock.Object,
                _eventLiftMock.Object,
                _statusCommandBuilderMock.Object,
                _idProviderMock.Object,
                _installerServiceMock.Object);

            var privateObject = new PrivateObject(packageDownloadManager);

            privateObject.Invoke(
                "UpdatePackageStatus",
                new PackageTransferEventArgs
                {
                    PackageId = PackageId,
                    TransferState = TransferState.Failed,
                    PackageState = PackageState.DeletePending
                },
                device.Object);

            _packageManagerMock.Verify(x => x.DeletePackage(It.Is<DeletePackageArgs>(p => p.PackageId == PackageId)));

            device.Verify(
                x =>
                    x.SendStatus(It.IsAny<packageStatus>(), null),
                Times.Exactly(2));

            _packageManagerMock.Verify(
                x => x.UpdateTransferEntity(It.IsAny<TransferEntity>()),
                Times.Exactly(1));

            _packageManagerMock.Verify(
                x =>
                    x.UpdatePackage(
                        It.Is<PackageLog>(log => (log.PackageId == PackageId) && (log.State == PackageState.Deleted))),
                Times.AtLeastOnce);
        }

        [TestMethod]
        public void WhenTransferStateInProgressExpectSuccess()
        {
            var device = new Mock<IDownloadDevice>();
            device.SetupGet(x => x.DeviceClass).Returns("download");

            ConfigurePackageManager(hasPackage: true);

            PackageDownloadManager packageDownloadManager = new PackageDownloadManager(
                _egmMock.Object,
                _packageManagerMock.Object,
                _eventBusMock.Object,
                _gatServiceMock.Object,
                _eventLiftMock.Object,
                _statusCommandBuilderMock.Object,
                _idProviderMock.Object,
                _installerServiceMock.Object);

            var privateObject = new PrivateObject(packageDownloadManager);

            privateObject.Invoke(
                "UpdatePackageStatus",
                new PackageTransferEventArgs
                {
                    PackageId = PackageId,
                    TransferState = TransferState.InProgress
                },
                device.Object);

            _packageManagerMock.Verify(
                x => x.UpdateTransferEntity(It.Is<TransferEntity>(t => t.PackageId == PackageId)),
                Times.Once);

            _packageManagerMock.Verify(
                x => x.UpdatePackage(It.Is<PackageLog>(log => log.PackageId == PackageId)),
                Times.Never);
        }

        [TestMethod]
        public void WhenTransferStatePendingExpectSuccess()
        {
            var device = new Mock<IDownloadDevice>();
            device.SetupGet(x => x.DeviceClass).Returns("download");

            ConfigurePackageManager(hasPackage: true, transferState: TransferState.Completed);

            PackageDownloadManager packageDownloadManager = new PackageDownloadManager(
                _egmMock.Object,
                _packageManagerMock.Object,
                _eventBusMock.Object,
                _gatServiceMock.Object,
                _eventLiftMock.Object,
                _statusCommandBuilderMock.Object,
                _idProviderMock.Object,
                _installerServiceMock.Object);

            var privateObject = new PrivateObject(packageDownloadManager);

            privateObject.Invoke(
                "UpdatePackageStatus",
                new PackageTransferEventArgs { PackageId = PackageId, TransferState = TransferState.Pending },
                device.Object);

            _packageManagerMock.Verify(
                x => x.UpdateTransferEntity(It.Is<TransferEntity>(t => t.PackageId == PackageId)),
                Times.Once);

            _packageManagerMock.Verify(
                x =>
                    x.UpdatePackage(
                        It.Is<PackageLog>(log => (log.PackageId == PackageId) && (log.State == PackageState.InUse))),
                Times.Never);
        }

        private void ConfigureEgm(bool downloadEnabled = true)
        {
            var minPackageListEntries = 3;

            var device = new Mock<IDownloadDevice>();
            device.SetupGet(x => x.DownloadEnabled).Returns(downloadEnabled);
            device.SetupGet(x => x.MinPackageListEntries).Returns(minPackageListEntries);
            _egmMock.Setup(x => x.GetDevice<IDownloadDevice>(DeviceId)).Returns(device.Object);
        }

        private void ConfigurePackageManager(
            int packageCount = 1,
            bool hasPackage = false,
            bool isTransferring = false,
            TransferState transferState = TransferState.InProgress)
        {
            _packageManagerMock.Setup(a => a.GetPackageLogEntity(It.IsAny<string>()))
                .Returns(new PackageLog() { PackageId = PackageId, TransactionId = 1 });
            _packageManagerMock.Setup(x => x.DeletePackage(It.IsAny<DeletePackageArgs>())).Callback(
                (DeletePackageArgs args) => { args.DeletePackageCallback(args); }).Verifiable();

            _packageManagerMock.Setup(x => x.ValidatePackage(It.IsAny<string>())).Returns(true);
            _packageManagerMock.SetupGet(x => x.PackageCount).Returns(packageCount);
            _packageManagerMock.SetupGet(x => x.ScriptEntityList).Returns(new List<Script>());
            _packageManagerMock.Setup(x => x.IsTransferring(PackageId)).Returns(isTransferring);
            _packageManagerMock.Setup(x => x.HasPackage(PackageId)).Returns(hasPackage);
            _packageManagerMock.Setup(x => x.GetTransferEntity(PackageId)).Returns(
                new TransferEntity
                {
                    PackageId = PackageId,
                    State = transferState
                });
            _packageManagerMock.Setup(x => x.GetPackageEntity(PackageId)).Returns(
                new Package
                {
                    State = isTransferring == false ? PackageState.InUse : PackageState.Pending,
                    PackageId = PackageId,
                    Id = 1
                });

            _packageManagerMock.SetupGet(x => x.PackageTaskAbortTokens)
                .Returns(new Dictionary<string, CancellationTokenSource> { { PackageId, null } });

            _packageManagerMock.Setup(x => x.ParseXml<packageTransferLog>(It.IsAny<string>()))
                .Returns(new packageTransferLog());
            _packageManagerMock.Setup(x => x.ParseXml<packageTransferStatus>(It.IsAny<string>()))
                .Returns(new packageTransferStatus());
        }

        private ClassCommand<download, addPackage> CreateCommand()
        {
            var command = ClassCommandUtilities.CreateClassCommand<download, addPackage>(
                TestConstants.HostId,
                TestConstants.EgmId);

            command.Command.pkgId = PackageId;
            command.Command.transferId = 1;
            command.Command.pkgSize = 1;
            command.Command.reasonCode = "1";
            command.Command.transferLocation = $"ftp://{PackageId}.zip";
            command.Command.transferParameters = string.Empty;

            return command;
        }

        private AddPackage CreateHandler()
        {
            return new AddPackage(
                _egmMock.Object,
                _packageManagerMock.Object,
                _packageDownloadManagerMock.Object,
                _statusCommandBuilderMock.Object);
        }
    }
}
