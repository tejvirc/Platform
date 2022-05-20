namespace Aristocrat.Monaco.G2S.Common.Tests.PackageManager.CommandHandlers
{
    using System;
    using System.Data.Entity;
    using System.IO;
    using System.Threading;
    using Common.PackageManager.CommandHandlers;
    using Common.PackageManager.Storage;
    using Data.Model;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Exceptions;
    using Moq;
    using Test.Common;

    [TestClass]
    public class RetryTransferHandlerTests
    {
        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
        }

        [TestMethod]
        public void RetryUploadTest()
        {
            var packageLog = new PackageLog() { PackageId = "Test" };
            var helper = new HandlerTestHelper();

            helper.ConfigureAndRegisterMocks();
            helper.PackageRepositoryMock.Setup(x => x.GetPackageByPackageId(It.IsAny<DbContext>(), It.IsAny<string>()))
                .Returns(new Package());
            helper.TransferRepositoryMock.Setup(x => x.GetByPackageId(It.IsAny<DbContext>(), It.IsAny<string>()))
                .Returns(new TransferEntity { TransferType = TransferType.Upload });

            var handler = GetRetryTransferHandler(helper);

            handler.Execute(
                new BaseTransferPackageArgs(
                    "Test",
                    args =>
                    {
                        // DO NOTHING
                    },
                    CancellationToken.None,
                    packageLog));

            helper.PackageRepositoryMock.Verify(
                x => x.GetPackageByPackageId(It.IsAny<DbContext>(), It.IsAny<string>()),
                Times.AtLeastOnce);
            helper.PackageRepositoryMock.Verify(x => x.Update(It.IsAny<DbContext>(), It.IsAny<Package>()), Times.Once);
            helper.TransferRepositoryMock.Verify(
                x => x.Add(It.IsAny<DbContext>(), It.IsAny<TransferEntity>()),
                Times.Once);
            helper.TransferServiceMock.Verify(
                x =>
                    x.Upload(
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
        public void RetryDownloadTest()
        {
            var packageLog = new PackageLog() { PackageId = "Test" };
            var helper = new HandlerTestHelper();

            helper.ConfigureAndRegisterMocks();

            helper.PackageLogRepositoryMock.Setup(x => x.GetLastPackageLogeByPackageId(It.IsAny<DbContext>(), It.IsAny<string>()))
                .Returns(new PackageLog { State = PackageState.Pending });
            helper.PackageRepositoryMock.Setup(x => x.GetPackageByPackageId(It.IsAny<DbContext>(), It.IsAny<string>()))
                .Returns(new Package { State = PackageState.Pending });
            helper.TransferRepositoryMock.Setup(x => x.GetByPackageId(It.IsAny<DbContext>(), It.IsAny<string>()))
                .Returns(new TransferEntity { TransferType = TransferType.Download });

            helper.TransferRepositoryMock.Setup(x => x.GetByPackageId(It.IsAny<DbContext>(), It.IsAny<string>()))
                .Returns(new TransferEntity { TransferType = TransferType.Download });

            var handler = GetRetryTransferHandler(helper);

            handler.Execute(
                new BaseTransferPackageArgs(
                    "Test",
                    args =>
                    {
                        // DO NOTHING
                    },
                    CancellationToken.None,
                    packageLog));

            helper.PackageRepositoryMock.Verify(
                x => x.GetPackageByPackageId(It.IsAny<DbContext>(), It.IsAny<string>()),
                Times.AtLeastOnce);
            helper.TransferRepositoryMock.Verify(
                x => x.Update(It.IsAny<DbContext>(), It.IsAny<TransferEntity>()),
                Times.Once);
            helper.TransferServiceMock.Verify(
                x =>
                    x.Download(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
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
        [ExpectedException(typeof(CommandException))]
        public void NoTransferEntityTest()
        {
            var packageLog = new PackageLog() { PackageId = "Test" };
            var helper = new HandlerTestHelper();

            helper.ConfigureAndRegisterMocks();
            helper.PackageRepositoryMock
                .Setup(x => x.GetPackageByPackageId(It.IsAny<DbContext>(), It.IsAny<string>())).Returns(new Package());

            var handler = GetRetryTransferHandler(helper);
            handler.Execute(
                new BaseTransferPackageArgs(
                    "Test",
                    args =>
                    {
                        // DO NOTHING
                    },
                    CancellationToken.None,
                    packageLog));
        }

        [TestMethod]
        [ExpectedException(typeof(CommandException))]
        public void NoPackageEntityTest()
        {
            var packageLog = new PackageLog() { PackageId = "Test" };
            var helper = new HandlerTestHelper();

            helper.ConfigureAndRegisterMocks();
            helper.TransferRepositoryMock
                .Setup(x => x.GetByPackageId(It.IsAny<DbContext>(), It.IsAny<string>()))
                .Returns(new TransferEntity { TransferType = TransferType.Upload });

            var handler = GetRetryTransferHandler(helper);

            handler.Execute(
                new BaseTransferPackageArgs(
                    "Test",
                    args =>
                    {
                        // DO NOTHING
                    },
                    CancellationToken.None,
                    packageLog));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPackageIdParameterTest()
        {
            var packageLog = new PackageLog() { PackageId = "Test" };
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();

            var handler = GetRetryTransferHandler(helper);

            handler.Execute(
                new BaseTransferPackageArgs(
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

            var handler = GetRetryTransferHandler(helper);

            handler.Execute(new BaseTransferPackageArgs("Test", null, CancellationToken.None, packageLog));
        }

        private RetryTransferHandler GetRetryTransferHandler(HandlerTestHelper testHelper)
        {
            var packageRepository = testHelper.PackageRepositoryMock.Object;
            var transferRepository = testHelper.TransferRepositoryMock.Object;
            var transferService = testHelper.TransferServiceMock.Object;
            var packageErrorRepository = testHelper.PackageErrorRepositoryMock.Object;
            return new RetryTransferHandler(
                testHelper.ContextFactoryMock.Object,
                packageRepository,
                transferRepository,
                transferService,
                packageErrorRepository,
                testHelper.ComponentHash.Object,
                testHelper.InstallerService,
                testHelper.PathMapper.Object,
                testHelper.FileSystemProviderMock.Object,
                testHelper.UpdatePackage.Object);
        }
    }
}