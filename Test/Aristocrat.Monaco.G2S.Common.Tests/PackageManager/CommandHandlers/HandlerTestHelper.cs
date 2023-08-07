using System.Data.Entity;
using System.IO;
using Aristocrat.Monaco.G2S.Data.Model;
using Moq;

namespace Aristocrat.Monaco.G2S.Common.Tests.PackageManager.CommandHandlers
{
    using System;
    using System.Data.Entity;
    using System.IO;
    using Application.Contracts;
    using Application.Contracts.Authentication;
    using Aristocrat.Monaco.Protocol.Common.Installer;
    using Common.PackageManager.Storage;
    using Common.Transfer;
    using G2S.Data.Model;
    using G2S.Data.Packages;
    using Gaming.Contracts;
    using Hardware.Contracts;
    using Kernel;
    using Monaco.Common.Storage;
    using Moq;
    using PackageManifest;
    using PackageManifest.Models;

    public class HandlerTestHelper
    {
        public Mock<IMonacoContextFactory> ContextFactoryMock { get; } = new Mock<IMonacoContextFactory>();

        public Mock<IPackageLogRepository> PackageLogRepositoryMock { get; } = new Mock<IPackageLogRepository>();

        public Mock<IPackageRepository> PackageRepositoryMock { get; } = new Mock<IPackageRepository>();

        public Mock<IModuleRepository> ModuleRepositoryMock { get; } = new Mock<IModuleRepository>();

        public Mock<ITransferRepository> TransferRepositoryMock { get; } = new Mock<ITransferRepository>();

        public Mock<IPackageErrorRepository> PackageErrorRepositoryMock { get; } = new Mock<IPackageErrorRepository>();

        public Mock<IPackageService> PackageServiceMock { get; } = new Mock<IPackageService>();

        public Mock<ITransferService> TransferServiceMock { get; } = new Mock<ITransferService>();

        public Mock<IFileSystemProvider> FileSystemProviderMock { get; } = new Mock<IFileSystemProvider>();

        public Mock<IManifest<Image>> ManifestServiceMock { get; } = new Mock<IManifest<Image>>();

        public Mock<IPathMapper> PathMapper { get; } = new Mock<IPathMapper>();

        public InstallerService InstallerService { get; private set; }
        public Mock<IIdProvider> IdProvider { get; } = new Mock<IIdProvider>();
        public Mock<IAuthenticationService> ComponentHash { get; } = new Mock<IAuthenticationService>();
        public Mock<Action<PackageLog, DbContext>> UpdatePackage { get; } = new Mock<Action<PackageLog, DbContext>>();

        public void ConfigureAndRegisterMocks()
        {
            UpdatePackage.Setup(a => a(It.IsAny<PackageLog>(), It.IsAny<DbContext>())).Callback(() =>
            {
                PackageLogRepositoryMock.Object.Update(null, null);
                PackageRepositoryMock.Object.Update(null, null);
            });

            PackageLogRepositoryMock
                .Setup(x => x.GetLastPackageLogeByPackageId(It.IsAny<DbContext>(), It.IsAny<string>())).Verifiable();
            PackageLogRepositoryMock.Setup(x => x.Add(It.IsAny<DbContext>(), It.IsAny<PackageLog>()))
                .Returns((DbContext context, PackageLog entity) => entity)
                .Verifiable();

            PackageRepositoryMock
                .Setup(x => x.GetPackageByPackageId(It.IsAny<DbContext>(), It.IsAny<string>())).Verifiable();
            PackageRepositoryMock.Setup(x => x.Add(It.IsAny<DbContext>(), It.IsAny<Package>()))
                .Returns((DbContext context, Package entity) => entity)
                .Verifiable();
            PackageRepositoryMock.Setup(x => x.Update(It.IsAny<DbContext>(), It.IsAny<Package>())).Verifiable();

            ModuleRepositoryMock.Setup(x => x.GetModuleByModuleId(It.IsAny<DbContext>(), It.IsAny<string>()))
                .Verifiable();
            ModuleRepositoryMock.Setup(x => x.Add(It.IsAny<DbContext>(), It.IsAny<Module>()))
                .Returns((Module entity) => entity)
                .Verifiable();
            ModuleRepositoryMock.Setup(x => x.Delete(It.IsAny<DbContext>(), It.IsAny<Module>())).Verifiable();
            ModuleRepositoryMock.Setup(x => x.Update(It.IsAny<DbContext>(), It.IsAny<Module>())).Verifiable();

            TransferRepositoryMock.Setup(x => x.Add(It.IsAny<DbContext>(), It.IsAny<TransferEntity>()))
                .Returns((DbContext context, TransferEntity entity) => entity)
                .Verifiable();
            TransferRepositoryMock.Setup(x => x.Update(It.IsAny<DbContext>(), It.IsAny<TransferEntity>())).Verifiable();

            FileSystemProviderMock.Setup(x => x.GetFileReadStream(It.IsAny<string>()))
                .Returns((string path) => new MemoryStream())
                .Verifiable();
            FileSystemProviderMock.Setup(x => x.GetFileWriteStream(It.IsAny<string>()))
                .Returns((string path) => new MemoryStream())
                .Verifiable();
            FileSystemProviderMock.Setup(x => x.SearchFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(
                    (string path, string pattern) =>
                        new[] { Path.Combine(Directory.GetCurrentDirectory(), "test.manifest") })
                .Verifiable();
            FileSystemProviderMock.Setup(x => x.CreateFolder(It.IsAny<string>()))
                .Returns((string path) => new DirectoryInfo(Directory.GetCurrentDirectory()))
                .Verifiable();
            FileSystemProviderMock.Setup(x => x.CreateFile(It.IsAny<string>()))
                .Returns(
                    (string path) =>
                        new FileInfo(new DirectoryInfo(Directory.GetCurrentDirectory()).GetFiles()[0].FullName))
                .Verifiable();
            FileSystemProviderMock.Setup(x => x.DeleteFolder(It.IsAny<string>())).Verifiable();
            FileSystemProviderMock.Setup(x => x.DeleteFile(It.IsAny<string>())).Verifiable();

            ManifestServiceMock.Setup(x => x.Read(It.IsAny<string>()))
                .Returns(
                    (string file) => new Image(
                        "Test",
                        "game",
                        string.Empty,
                        0,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        new string[] { }))
                .Verifiable();

            PackageServiceMock.Setup(x => x.Pack(It.IsAny<ArchiveFormat>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .Verifiable();
            PackageServiceMock.Setup(x => x.Unpack(It.IsAny<ArchiveFormat>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .Verifiable();

            PathMapper.Setup(x => x.GetDirectory(It.IsAny<string>()))
                .Returns((string path) => new DirectoryInfo(Directory.GetCurrentDirectory()))
                .Verifiable();

            var osInstaller = new Mock<IOSInstaller>();
            var gameInstaller = new Mock<IGameInstaller>();
            var printerInstaller = new Mock<IPrinterFirmwareInstaller>();
            var noteAcceptorInstaller = new Mock<INoteAcceptorFirmwareInstaller>();
            var softwareInstaller = new Mock<ISoftwareInstaller>();
            var installerFactory = new InstallerFactory
            {
                { @"winUpdate", osInstaller.Object },
                { @"game", gameInstaller.Object },
                { @"printer", printerInstaller.Object },
                { @"noteacceptor", noteAcceptorInstaller.Object },
                { @"platform", softwareInstaller.Object },
                { @"runtime", softwareInstaller.Object }
            };

            InstallerService = new InstallerService(
                installerFactory,
                PathMapper.Object,
                ManifestServiceMock.Object,
                FileSystemProviderMock.Object,
                PackageServiceMock.Object);

            osInstaller.Setup(x => x.Uninstall(It.IsAny<string>())).Returns(true);
            gameInstaller.Setup(x => x.Uninstall(It.IsAny<string>())).Returns(true);
            printerInstaller.Setup(x => x.Uninstall(It.IsAny<string>())).Returns(true);
            noteAcceptorInstaller.Setup(x => x.Uninstall(It.IsAny<string>())).Returns(true);
            softwareInstaller.Setup(x => x.Uninstall(It.IsAny<string>())).Returns(true);
        }
    }
}
