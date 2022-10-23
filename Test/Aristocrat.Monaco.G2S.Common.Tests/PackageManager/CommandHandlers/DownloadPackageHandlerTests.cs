namespace Aristocrat.Monaco.G2S.Common.Tests.PackageManager.CommandHandlers
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using System.IO;
    using System.Threading;
    using Application.Contracts;
    using Common.PackageManager.CommandHandlers;
    using Common.PackageManager.Storage;
    using Common.Transfer;
    using Data.Model;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Exceptions;
    using Monaco.Common.Storage;
    using Moq;
    using PackageManifest;
    using PackageManifest.Models;
    using Aristocrat.Monaco.Protocol.Common.Storage;

    [TestClass]
    public class DownloadPackageHandlerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullTransferRepositoryExpectException()
        {
            var helper = new HandlerTestHelper();

            var handler = new DownloadPackageHandler(
                helper.ContextFactoryMock.Object,
                null,
                helper.TransferServiceMock.Object,
                helper.PackageErrorRepositoryMock.Object,
                helper.ComponentHash.Object,
                helper.PathMapper.Object,
                helper.FileSystemProviderMock.Object,
                helper.UpdatePackage.Object);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullTransferServiceExpectException()
        {
            var helper = new HandlerTestHelper();

            var handler = new DownloadPackageHandler(
                helper.ContextFactoryMock.Object,
                helper.TransferRepositoryMock.Object,
                null,
                helper.PackageErrorRepositoryMock.Object,
                helper.ComponentHash.Object,
                helper.PathMapper.Object,
                helper.FileSystemProviderMock.Object,
                helper.UpdatePackage.Object);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenExecuteWithNullTransferPackageArgsExpectException()
        {
            var handler = GetDownloadPackageHandler(new HandlerTestHelper());

            handler.Execute(null);
        }

        [TestMethod]
        public void TestDownloadPackageSteps()
        {
            var packageLog = new PackageLog() { PackageId = "Test" };
            var helper = new HandlerTestHelper();

            helper.ConfigureAndRegisterMocks();


            helper.TransferRepositoryMock.Setup(x => x.GetByPackageId(It.IsAny<DbContext>(), It.IsAny<string>()))
                .Returns(new TransferEntity { TransferType = TransferType.Download });

            var downloadHandler = this.GetDownloadPackageHandler(helper);

            downloadHandler.Execute(
                new TransferPackageArgs(
                    "Test",
                    new TransferEntity(),
                    args =>
                    {
                        // DO NOTHING
                    },
                    CancellationToken.None,
                    packageLog));

            helper.TransferRepositoryMock
                .Verify(x => x.Update(It.IsAny<DbContext>(), It.IsAny<TransferEntity>()), Times.Once);
            helper.TransferServiceMock
                .Verify(
                    x => x.Download(
                        It.IsAny<String>(),
                        It.IsAny<String>(),
                        It.IsAny<String>(),
                        It.IsAny<Stream>(),
                        CancellationToken.None),
                    Times.Once);
            helper.PackageLogRepositoryMock.Verify(
                x => x.Update(It.IsAny<DbContext>(), It.IsAny<PackageLog>()),
                Times.Exactly(1));
            helper.PackageRepositoryMock.Verify(
                x => x.Update(It.IsAny<DbContext>(), It.IsAny<Package>()),
                Times.Exactly(1));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPackageIdParameterTest()
        {
            var packageLog = new PackageLog() { PackageId = "Test" };
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();

            var handler = this.GetDownloadPackageHandler(helper);

            handler.Execute(
                new TransferPackageArgs(
                    null,
                    new TransferEntity(),
                    args =>
                    {
                        // DO NOTHING
                    },
                    CancellationToken.None,
                    packageLog));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmptyPackageIdParameterTest()
        {
            var packageLog = new PackageLog() { PackageId = "Test" };
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();

            var handler = this.GetDownloadPackageHandler(helper);

            handler.Execute(
                new TransferPackageArgs(
                    String.Empty,
                    new TransferEntity(),
                    args =>
                    {
                        // DO NOTHING
                    },
                    CancellationToken.None,
                    packageLog));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTransferEntityParameterTest()
        {
            var packageLog = new PackageLog() { PackageId = "Test" };
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();

            var handler = this.GetDownloadPackageHandler(helper);

            handler.Execute(
                new TransferPackageArgs(
                    "Test",
                    null,
                    args =>
                    {
                        // DO NOTHING
                    },
                    CancellationToken.None,
                    packageLog));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullCallbackParameterTest()
        {
            var packageLog = new PackageLog() { PackageId = "Test" };
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();

            var handler = this.GetDownloadPackageHandler(helper);

            handler.Execute(new TransferPackageArgs("Test", new TransferEntity(), null, CancellationToken.None, packageLog));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void WhenExecuteWithUpdateExceptionExpectHandleException()
        {
            var packageLog = new PackageLog() { PackageId = "Test" };
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();

            var dbContextOptions = new Mock<DbContextOptions>();

            helper.ContextFactoryMock.Setup(x => x.CreateDbContext()).Returns(new MonacoContext("test"));
            helper.PackageRepositoryMock
                .Setup(x => x.GetPackageByPackageId(It.IsAny<DbContext>(), It.IsAny<string>()))
                .Returns((Package)null)
                .Verifiable();

            helper.TransferRepositoryMock.Setup(x => x.GetByPackageId(It.IsAny<DbContext>(), It.IsAny<string>()))
                .Returns(new TransferEntity { TransferType = TransferType.Download });

            helper.TransferRepositoryMock
                .Setup(m => m.Update(It.IsAny<DbContext>(), It.IsAny<TransferEntity>()))
                .Throws<Exception>();

            var handler = GetDownloadPackageHandler(helper);

            handler.Execute(
                new TransferPackageArgs(
                    "Test",
                    new TransferEntity(),
                    args => { },
                    CancellationToken.None,
                    packageLog));
        }

        private DownloadPackageHandler GetDownloadPackageHandler(HandlerTestHelper testHelper)
        {
            ITransferRepository transferRepository = testHelper.TransferRepositoryMock.Object;
            ITransferService transferService = testHelper.TransferServiceMock.Object;
            IPackageErrorRepository packageErrorRepository = testHelper.PackageErrorRepositoryMock.Object;

            return new DownloadPackageHandler(
                testHelper.ContextFactoryMock.Object,
                transferRepository,
                transferService,
                packageErrorRepository,
                testHelper.ComponentHash.Object,
                testHelper.PathMapper.Object,
                testHelper.FileSystemProviderMock.Object,
                testHelper.UpdatePackage.Object);
        }
    }
}
