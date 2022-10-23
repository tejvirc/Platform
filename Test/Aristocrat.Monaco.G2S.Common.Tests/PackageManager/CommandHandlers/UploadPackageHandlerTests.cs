namespace Aristocrat.Monaco.G2S.Common.Tests.PackageManager.CommandHandlers
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using System.IO;
    using System.Threading;
    using Common.PackageManager.CommandHandlers;
    using Common.PackageManager.Storage;
    using Common.Transfer;
    using Data.Model;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Exceptions;
    using Monaco.Common.Storage;
    using Moq;
    using Test.Common;

    [TestClass]
    public class UploadPackageHandlerTests
    {
        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
        }

        [TestMethod]
        public void TestUploadPackageSteps()
        {
            var packageLog = new PackageLog() { PackageId = "Test" };

            var helper = new HandlerTestHelper();

            helper.ConfigureAndRegisterMocks();
            helper.PackageRepositoryMock
                .Setup(x => x.GetPackageByPackageId(It.IsAny<DbContext>(), It.IsAny<string>()))
                .Returns((DbContext context, string packageId) => { return new Package(); }).Verifiable();

            helper.PackageLogRepositoryMock
                .Setup(x => x.GetLastPackageLogeByPackageId(It.IsAny<DbContext>(), It.IsAny<string>()))
                .Returns((DbContext context, string packageId) => { return packageLog; });

            var downloadHandler = this.GetUploadPackageHandler(helper);

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

            helper.PackageRepositoryMock.Verify(
                x => x.GetPackageByPackageId(It.IsAny<DbContext>(), It.IsAny<string>()),
                Times.Once);
            helper.PackageRepositoryMock.Verify(x => x.Update(It.IsAny<DbContext>(), It.IsAny<Package>()), Times.Once);
            helper.TransferRepositoryMock.Verify(
                x => x.Add(It.IsAny<DbContext>(), It.IsAny<TransferEntity>()),
                Times.Once);
            helper.TransferServiceMock.Verify(
                x => x.Upload(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Stream>(),
                    CancellationToken.None),
                Times.Once);
            helper.TransferRepositoryMock.Verify(
                x => x.Update(It.IsAny<DbContext>(), It.IsAny<TransferEntity>()),
                Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPackageIdParameterTest()
        {
            var packageLog = new PackageLog() { PackageId = "Test" };
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();

            var handler = this.GetUploadPackageHandler(helper);

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

            var handler = this.GetUploadPackageHandler(helper);

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

            var handler = this.GetUploadPackageHandler(helper);

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

            var handler = this.GetUploadPackageHandler(helper);

            handler.Execute(new TransferPackageArgs("Test", new TransferEntity(), null, CancellationToken.None, packageLog));
        }

        [TestMethod]
        [ExpectedException(typeof(CommandException))]
        public void PackageDoesNotExistTest()
        {
            var packageLog = new PackageLog() { PackageId = "Test" };
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();
            helper.PackageRepositoryMock
                .Setup(x => x.GetPackageByPackageId(It.IsAny<DbContext>(), It.IsAny<String>()))
                .Returns((DbContext context, String packageId) => { return null; }).Verifiable();

            var handler = this.GetUploadPackageHandler(helper);

            handler.Execute(
                new TransferPackageArgs(
                    "Test",
                    new TransferEntity(),
                    args =>
                    {
                        // DO NOTHING
                    },
                    CancellationToken.None,
                    packageLog));
        }

        private UploadPackageHandler GetUploadPackageHandler(HandlerTestHelper testHelper)
        {
            IPackageRepository packageRepository = testHelper.PackageRepositoryMock.Object;
            ITransferRepository transferRepository = testHelper.TransferRepositoryMock.Object;
            ITransferService transferService = testHelper.TransferServiceMock.Object;
            IPackageErrorRepository packageErrorRepository = testHelper.PackageErrorRepositoryMock.Object;

            return new UploadPackageHandler(
                testHelper.ContextFactoryMock.Object,
                packageRepository,
                transferRepository,
                transferService,
                packageErrorRepository,
                testHelper.InstallerService,
                testHelper.FileSystemProviderMock.Object);
        }
    }
}