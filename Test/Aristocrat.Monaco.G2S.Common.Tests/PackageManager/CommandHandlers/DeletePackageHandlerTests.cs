namespace Aristocrat.Monaco.G2S.Common.Tests.PackageManager.CommandHandlers
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Common.PackageManager.CommandHandlers;
    using Common.PackageManager.Storage;
    using G2S.Data.Model;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Exceptions;
    using Monaco.Common.Storage;
    using Moq;

    [TestClass]
    public class DeletePackageHandlerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullPackageRepositoryExpectException()
        {
            var helper = new HandlerTestHelper();

            var handler = new DeletePackageHandler(
                helper.ContextFactoryMock.Object,
                helper.PackageErrorRepositoryMock.Object,
                helper.PackageLogRepositoryMock.Object,
                null,
                helper.InstallerService,
                helper.UpdatePackage.Object);

            Assert.IsNull(handler);
        }
        
        [TestMethod]
        [ExpectedException(typeof(CommandException))]
        public void PackageInUseTest()
        {
            var package = new Package() { PackageId = "Test", State = PackageState.InUse };
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();
            helper.PackageRepositoryMock
                .Setup(x => x.GetPackageByPackageId(It.IsAny<DbContext>(), It.IsAny<String>())).Returns(package);

            var handler = this.GetDeletePackageHandler(helper);
            handler.Execute(
                new DeletePackageArgs(
                    package.PackageId,
                    a => { }));
        }
        
        [TestMethod]
        public void DeletePendingTest()
        {
            var transfer = new TransferEntity() { State = TransferState.InProgress };
            var package = new Package { PackageId = "Test" };
            var packageLog = new PackageLog { PackageId = "Test" };
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();
            helper.UpdatePackage.Setup(a => a(packageLog, It.IsAny<DbContext>()))
                .Callback(() =>
                {
                    package.State = packageLog.State;
                });

            helper.PackageLogRepositoryMock
                .Setup(x => x.GetLastPackageLogeByPackageId(It.IsAny<DbContext>(), It.IsAny<String>())).Returns(packageLog);
            helper.PackageRepositoryMock
                .Setup(x => x.GetPackageByPackageId(It.IsAny<DbContext>(), It.IsAny<String>())).Returns(package);
            helper.TransferRepositoryMock
                .Setup(x => x.GetByPackageId(It.IsAny<DbContext>(), It.IsAny<String>())).Returns(transfer);
            helper
                .TransferRepositoryMock.Setup(x => x.Update(It.IsAny<DbContext>(), It.IsAny<TransferEntity>()))
                .Callback(
                    (DbContext context, TransferEntity entity) => { transfer.State = entity.State; });

            var handler = this.GetDeletePackageHandler(helper);
            handler.Execute(
                new DeletePackageArgs(
                    package.PackageId,
                    a => { }));
        }

        [TestMethod]
        public void DeletedTest()
        {
            var package = new Package { PackageId = "Test" };
            var packageLog = new PackageLog { PackageId = "Test" };
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();
            helper.UpdatePackage.Setup(a => a(packageLog, It.IsAny<DbContext>()))
                .Callback(() =>
                {
                    package.State = packageLog.State;
                });

        helper.PackageLogRepositoryMock
                .Setup(x => x.GetLastPackageLogeByPackageId(It.IsAny<DbContext>(), It.IsAny<String>())).Returns(packageLog);
            helper.PackageRepositoryMock
                .Setup(x => x.GetPackageByPackageId(It.IsAny<DbContext>(), It.IsAny<String>())).Returns(package);
            helper.PackageRepositoryMock.Setup(x => x.Update(It.IsAny<DbContext>(), It.IsAny<Package>())).Callback(
                (DbContext context, Package entity) => { package.State = entity.State; });

            var handler = this.GetDeletePackageHandler(helper);
            handler.Execute(
                new DeletePackageArgs(
                    package.PackageId,
                    a => { }));

            Assert.AreEqual(package.State, PackageState.Deleted);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPackageIdParameterTest()
        {
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();

            var handler = this.GetDeletePackageHandler(helper);

            handler.Execute(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmptyPackageIdParameterTest()
        {
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();

            var handler = this.GetDeletePackageHandler(helper);
            var package = new Package();
            handler.Execute(
                new DeletePackageArgs(
                    package.PackageId,
                    a => { }));
        }

        private DeletePackageHandler GetDeletePackageHandler(HandlerTestHelper testHelper)
        {
            IPathMapper pathMapper = testHelper.PathMapper.Object;
            IFileSystemProvider fileSystemProvider = testHelper.FileSystemProviderMock.Object;
            IPackageErrorRepository packageErrorRepository = testHelper.PackageErrorRepositoryMock.Object;

            return new DeletePackageHandler(
                testHelper.ContextFactoryMock.Object,
                packageErrorRepository,
                testHelper.PackageLogRepositoryMock.Object,
                testHelper.PackageRepositoryMock.Object,
                testHelper.InstallerService,
                testHelper.UpdatePackage.Object);
        }
    }
}