namespace Aristocrat.Monaco.G2S.Tests.Handlers.Downloads
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Serialization;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.PackageManager;
    using Common.PackageManager.CommandHandlers;
    using Common.PackageManager.Storage;
    using G2S.Handlers;
    using G2S.Handlers.Downloads;
    using Data.Model;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class DeletePackageTest
    {
        private const int DeviceId = 1;
        private const string PackageId = "G2S_packageId";
        private const string TransferStatus = "transfer-status";
        private const ScriptState ScriptStatus = ScriptState.PendingAuthorization;
        private readonly Mock<IG2SEgm> _egmMock = new Mock<IG2SEgm>();
        private readonly Mock<IEventLift> _eventLiftMock = new Mock<IEventLift>();
        private readonly Mock<IGatService> _gatServiceMock = new Mock<IGatService>();
        private readonly Mock<IIdProvider> _idProvider = new Mock<IIdProvider>();
        private readonly Mock<IPackageManager> _packageManagerMock = new Mock<IPackageManager>();
        private readonly Mock<IScriptManager> _scriptManagerMock = new Mock<IScriptManager>();

        private readonly Mock<ICommandBuilder<IDownloadDevice, packageStatus>> _statusCommandBuilderMock =
            new Mock<ICommandBuilder<IDownloadDevice, packageStatus>>();

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var handler = new DeletePackage(null, null, null, null, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullPackageManagerExpectException()
        {
            var handler = new DeletePackage(_egmMock.Object, null, null, null, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullScriptManagerExpectException()
        {
            var handler = new DeletePackage(
                _egmMock.Object,
                _packageManagerMock.Object,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullGatServiceExpectException()
        {
            var handler = new DeletePackage(
                _egmMock.Object,
                _packageManagerMock.Object,
                _scriptManagerMock.Object,
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
            var handler = new DeletePackage(
                _egmMock.Object,
                _packageManagerMock.Object,
                _scriptManagerMock.Object,
                _gatServiceMock.Object,
                null,
                null,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullIdProviderExpectException()
        {
            var handler = new DeletePackage(
                _egmMock.Object,
                _packageManagerMock.Object,
                _scriptManagerMock.Object,
                _gatServiceMock.Object,
                _eventLiftMock.Object,
                null,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCommandBuilderExpectException()
        {
            var handler = new DeletePackage(
                _egmMock.Object,
                _packageManagerMock.Object,
                _scriptManagerMock.Object,
                _gatServiceMock.Object,
                _eventLiftMock.Object,
                _idProvider.Object,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var handler = new DeletePackage(
                _egmMock.Object,
                _packageManagerMock.Object,
                _scriptManagerMock.Object,
                _gatServiceMock.Object,
                _eventLiftMock.Object,
                _idProvider.Object,
                _statusCommandBuilderMock.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var handler = new DeletePackage(
                _egmMock.Object,
                _packageManagerMock.Object,
                _scriptManagerMock.Object,
                _gatServiceMock.Object,
                _eventLiftMock.Object,
                _idProvider.Object,
                _statusCommandBuilderMock.Object);
            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var egm = DownloadsUtilities.CreateMockEgm();
            var handler = new DeletePackage(
                egm,
                _packageManagerMock.Object,
                _scriptManagerMock.Object,
                _gatServiceMock.Object,
                _eventLiftMock.Object,
                _idProvider.Object,
                _statusCommandBuilderMock.Object);
            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<IDownloadDevice>();
            var queue = new Mock<ICommandQueue>();

            queue.SetupGet(q => q.TimeToLiveBehavior).Returns(TimeToLiveBehavior.Strict);
            device.SetupGet(comms => comms.Queue).Returns(queue.Object);
            device.SetupGet(evt => evt.Owner).Returns(TestConstants.HostId);
            device.SetupGet(evt => evt.DownloadEnabled).Returns(true);
            _egmMock.Setup(e => e.GetDevice<IDownloadDevice>(It.Is<int>(id => id == TestConstants.HostId)))
                .Returns(device.Object);

            var handler = new DeletePackage(
                _egmMock.Object,
                _packageManagerMock.Object,
                _scriptManagerMock.Object,
                _gatServiceMock.Object,
                _eventLiftMock.Object,
                _idProvider.Object,
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
        public async Task WhenPackageAndTransferEntityNotExistExpectError()
        {
            ConfigureEgm();

            var handler = CreateHandler();

            var command = CreateCommand();

            await handler.Handle(command);

            Assert.IsTrue(command.Error.IsError);
            Assert.AreEqual(command.Error.Code, ErrorCode.G2S_DLX001);
        }

        [TestMethod]
        public async Task WhenPackageWasDeletedExpectError()
        {
            _packageManagerMock.Setup(x => x.GetPackageEntity(PackageId))
                .Returns(new Package { State = PackageState.Deleted });

            ConfigureEgm();

            var handler = CreateHandler();

            var command = CreateCommand();

            await handler.Handle(command);

            Assert.IsTrue(command.Error.IsError);
            Assert.AreEqual(command.Error.Code, ErrorCode.G2S_DLX001);
        }

        [TestMethod]
        public async Task WhenPackageInUseExpectError()
        {
            _packageManagerMock.Setup(x => x.GetPackageEntity(PackageId))
                .Returns(new Package { State = PackageState.InUse });
            _packageManagerMock.Setup(x => x.GetTransferEntity(PackageId))
                .Returns(new TransferEntity());

            ConfigureEgm();

            var handler = CreateHandler();

            var command = CreateCommand();

            await handler.Handle(command);

            Assert.IsTrue(command.Error.IsError);
            Assert.AreEqual(command.Error.Code, ErrorCode.G2S_DLX003);
        }

        [TestMethod]
        public async Task WhenPackageNotFoundExpectResponse()
        {
            ConfigurePackageManager();

            var cancellationToken = new CancellationTokenSource();
            Assert.IsFalse(cancellationToken.IsCancellationRequested);
            _packageManagerMock.SetupGet(x => x.PackageTaskAbortTokens)
                .Returns(
                    new Dictionary<string, CancellationTokenSource> { { PackageId, cancellationToken } });

            _packageManagerMock.Setup(x => x.ParseXml<packageTransferStatus>(TransferStatus))
                .Returns(new packageTransferStatus());

            _packageManagerMock.SetupGet(x => x.ScriptEntityList).Returns(new List<Script>());

            ConfigureEgm();

            var handler = CreateHandler();

            var command = CreateCommand();

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<download, packageStatus>;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.Command.pkgId, PackageId);
        }

        private void ConfigurePackageManager()
        {
            _packageManagerMock.Setup(a => a.GetPackageLogEntity(It.IsAny<string>()))
                .Returns(new PackageLog() { PackageId = PackageId });
            _packageManagerMock.Setup(a => a.GetPackageEntity(It.IsAny<string>()))
                .Returns(new Package() { PackageId = PackageId });
            _packageManagerMock.SetupGet(a => a.PackageTaskAbortTokens)
                .Returns(new Dictionary<string, CancellationTokenSource>());
            _packageManagerMock.Setup(x => x.DeletePackage(It.IsAny<DeletePackageArgs>())).Callback(
                (DeletePackageArgs args) => { args.DeletePackageCallback(args); }).Verifiable();

            _packageManagerMock.Setup(x => x.GetTransferEntity(PackageId))
                .Returns(new TransferEntity { PackageId = PackageId });

            _packageManagerMock.Setup(x => x.ParseXml<commandStatusList>(It.IsAny<string>()))
                .Returns(
                    new commandStatusList
                    {
                        Items =
                            new[]
                            {
                                new packageCmdStatus
                                {
                                    pkgId =
                                        PackageId
                                }
                            }
                    });

            _packageManagerMock.Setup(x => x.ScriptEntityList).Returns(
                new List<Script>
                {
                    new Script
                    {
                        State = ScriptStatus,
                        CommandData = GetCommandStatusList()
                    }
                });
        }

        private string GetCommandStatusList()
        {
            var result = new commandStatusList
            {
                Items =
                    new[]
                    {
                        new packageCmdStatus
                        {
                            pkgId =
                                PackageId
                        }
                    }
            };

            using (var stringwriter = new StringWriter())
            {
                var serializer = new XmlSerializer(typeof(commandStatusList));
                serializer.Serialize(stringwriter, result);
                return stringwriter.ToString();
            }
        }

        private void ConfigureEgm()
        {
            var device = new Mock<IDownloadDevice>();
            device.SetupGet(x => x.Id).Returns(DeviceId);
            device.SetupGet(x => x.DeviceClass).Returns("download");

            _egmMock.Setup(x => x.GetDevice<IDownloadDevice>(DeviceId)).Returns(device.Object);
        }

        private ClassCommand<download, deletePackage> CreateCommand()
        {
            var command = ClassCommandUtilities.CreateClassCommand<download, deletePackage>(
                TestConstants.HostId,
                TestConstants.EgmId);

            command.Command.pkgId = PackageId;

            return command;
        }

        private DeletePackage CreateHandler()
        {
            return new DeletePackage(
                _egmMock.Object,
                _packageManagerMock.Object,
                _scriptManagerMock.Object,
                _gatServiceMock.Object,
                _eventLiftMock.Object,
                _idProvider.Object,
                _statusCommandBuilderMock.Object);
        }
    }
}
