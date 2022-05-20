namespace Aristocrat.Monaco.G2S.Common.Tests.PackageManager.CommandHandlers
{
    using System;
    using System.Data.Entity;
        using Common.PackageManager.CommandHandlers;
    using Common.PackageManager.Storage;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Monaco.Protocol.Common.Installer;
    using Moq;
    using PackageManifest;
    using PackageManifest.Models;
    using G2S.Data.Model;

    [TestClass]
    public class CreatePackageHandlerTests
    {
        [TestMethod]
        public void NotExistPackageWasNotDeleted()
        {
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();

            var handler = this.GetCreatePackageHandler(helper);

            handler.Execute(new CreatePackageArgs(this.GetPackageEntity(), this.GetModuleEntity(), true, ".zip"));

            helper.FileSystemProviderMock.Verify(x => x.DeleteFile(It.IsAny<String>()), Times.Never);
        }

        [TestMethod]
        public void TestDefaultStepsInCreatePackageHandler()
        {
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();

            var handler = this.GetCreatePackageHandler(helper);

            handler.Execute(new CreatePackageArgs(this.GetPackageEntity(), this.GetModuleEntity(), false, ".zip"));

            helper.PathMapper.Verify(x => x.GetDirectory(It.IsAny<String>()), Times.AtLeastOnce);
            helper.FileSystemProviderMock.Verify(x => x.GetFileWriteStream(It.IsAny<String>()), Times.Once);
            helper.PackageRepositoryMock.Verify(x => x.Update(It.IsAny<DbContext>(), It.IsAny<Package>()), Times.Once);
            helper.FileSystemProviderMock.Verify(x => x.DeleteFile(It.IsAny<String>()), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullCreatePackageArgsTest()
        {
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();

            var handler = this.GetCreatePackageHandler(helper);

            handler.Execute(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPackageEntityParameterTest()
        {
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();

            var handler = this.GetCreatePackageHandler(helper);

            handler.Execute(new CreatePackageArgs(null, null, true, ".zip"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullModuleEntityParameterTest()
        {
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();

            var handler = this.GetCreatePackageHandler(helper);

            handler.Execute(new CreatePackageArgs(this.GetPackageEntity(), null, true, ".zip"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullFormatParameterTest()
        {
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();

            var handler = this.GetCreatePackageHandler(helper);

            handler.Execute(new CreatePackageArgs(this.GetPackageEntity(), this.GetModuleEntity(), true, null));
        }

        private Module GetModuleEntity()
        {
            return new Module()
            {
                PackageId = Guid.NewGuid().ToString("N"),
                ModuleId = Guid.NewGuid().ToString("N")
            };
        }

        private PackageLog GetPackageEntity()
        {
            return new PackageLog()
            {
                PackageId = Guid.NewGuid().ToString("N")
            };
        }

        private CreatePackageHandler GetCreatePackageHandler(HandlerTestHelper testHelper)
        {
            IPackageErrorRepository packageErrorRepository = testHelper.PackageErrorRepositoryMock.Object;

            return new CreatePackageHandler(
                testHelper.ContextFactoryMock.Object,
                packageErrorRepository,
                testHelper.InstallerService,
                testHelper.UpdatePackage.Object);
        }

        private Mock<CreatePackageHandler> GetMockForCreatePackageHandler(HandlerTestHelper testHelper)
        {
            IMonacoContextFactory contextFactory = testHelper.ContextFactoryMock.Object;
            IPackageRepository packageRepository = testHelper.PackageRepositoryMock.Object;
            IManifest<Image> manifestService = testHelper.ManifestServiceMock.Object;
            IPathMapper pathMapper = testHelper.PathMapper.Object;
            IFileSystemProvider fileSystemProvider = testHelper.FileSystemProviderMock.Object;
            IPackageErrorRepository packageErrorRepository = testHelper.PackageErrorRepositoryMock.Object;

            return new Mock<CreatePackageHandler>(
                contextFactory,
                packageRepository,
                manifestService,
                pathMapper,
                fileSystemProvider,
                packageErrorRepository);
        }
    }
}
