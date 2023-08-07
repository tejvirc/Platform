namespace Aristocrat.Monaco.G2S.Common.Tests.PackageManager.CommandHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using Common.PackageManager.CommandHandlers;
    using Common.PackageManager.Storage;
    using G2S.Data.Model;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Exceptions;
    using Monaco.Common.Storage;
    using Moq;
    using Test.Common;

    [TestClass]
    public class GetPackageStatusHandlerTests
    {
        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConsturctWithNullPakcageRepositoryExepectException()
        {
            var helper = new HandlerTestHelper();

            var handler = new GetPackageStatusHandler(
                null,
                helper.TransferRepositoryMock.Object,
                helper.PackageErrorRepositoryMock.Object,
                helper.ContextFactoryMock.Object);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConsturctWithNullTransferRepositoryExepectException()
        {
            var helper = new HandlerTestHelper();

            var handler = new GetPackageStatusHandler(
                helper.PackageRepositoryMock.Object,
                null,
                helper.PackageErrorRepositoryMock.Object,
                helper.ContextFactoryMock.Object);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConsturctWithNullPackageErrorRepositoryExepectException()
        {
            var helper = new HandlerTestHelper();

            var handler = new GetPackageStatusHandler(
                helper.PackageRepositoryMock.Object,
                helper.TransferRepositoryMock.Object,
                null,
                helper.ContextFactoryMock.Object);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConsturctWithNullContextFactoryExepectException()
        {
            var helper = new HandlerTestHelper();

            var handler = new GetPackageStatusHandler(
                helper.PackageRepositoryMock.Object,
                helper.TransferRepositoryMock.Object,
                helper.PackageErrorRepositoryMock.Object,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(CommandException))]
        public void PackageDoesNotExistTest()
        {
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();

            var handler = this.GetPackageStatusHandlerInstance(helper);

            handler.Execute("Test");
        }

        [TestMethod]
        public void PackageExistTest()
        {
            var package = new Package() { State = PackageState.Available };
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();
            helper.PackageRepositoryMock
                .Setup(x => x.GetPackageByPackageId(It.IsAny<DbContext>(), It.IsAny<String>())).Returns(package);

            var handler = this.GetPackageStatusHandlerInstance(helper);
            var result = handler.Execute("Test");

            helper.PackageRepositoryMock
                .Verify(x => x.GetPackageByPackageId(It.IsAny<DbContext>(), It.IsAny<String>()), Times.Once);
            Assert.AreEqual(result.PackageState, package.State);
            Assert.IsNull(result.PackageError);
        }

        [TestMethod]
        public void NotInProgressTransferEntityNotAvailableAsResult()
        {
            var transfer = new TransferEntity() { State = TransferState.Completed };
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();
            helper.PackageRepositoryMock
                .Setup(x => x.GetPackageByPackageId(It.IsAny<DbContext>(), It.IsAny<String>())).Returns(new Package());
            helper.TransferRepositoryMock
                .Setup(x => x.GetByPackageId(It.IsAny<DbContext>(), It.IsAny<String>())).Returns(transfer);

            var handler = this.GetPackageStatusHandlerInstance(helper);
            var result = handler.Execute("Test");

            Assert.IsNull(result.Transfer);
        }

        [TestMethod]
        public void InProgressTransferEntityAvailableAsResult()
        {
            var transfer = new TransferEntity() { State = TransferState.InProgress };
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();
            helper.PackageRepositoryMock
                .Setup(x => x.GetPackageByPackageId(It.IsAny<DbContext>(), It.IsAny<String>())).Returns(new Package());
            helper.TransferRepositoryMock
                .Setup(x => x.GetByPackageId(It.IsAny<DbContext>(), It.IsAny<String>())).Returns(transfer);

            var handler = this.GetPackageStatusHandlerInstance(helper);
            var result = handler.Execute("Test");

            Assert.IsNotNull(result.Transfer);
            Assert.AreEqual(result.Transfer.State, transfer.State);
        }

        [TestMethod]
        public void PackageErrorsAvailableInResultsTest()
        {
            var package = new Package() { State = PackageState.Error };
            var packageErrors = new List<PackageError>();
            packageErrors.Add(new PackageError() { ErrorCode = 1 });
            packageErrors.Add(new PackageError() { ErrorCode = 2 });

            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();
            helper.PackageRepositoryMock
                .Setup(x => x.GetPackageByPackageId(It.IsAny<DbContext>(), It.IsAny<String>())).Returns(package);
            helper.PackageErrorRepositoryMock
                .Setup(x => x.GetByPackageId(It.IsAny<DbContext>(), It.IsAny<String>())).Returns(packageErrors);

            var handler = this.GetPackageStatusHandlerInstance(helper);
            var result = handler.Execute("Test");

            helper.PackageErrorRepositoryMock
                .Verify(x => x.GetByPackageId(It.IsAny<DbContext>(), It.IsAny<String>()), Times.Once);
            Assert.AreEqual(result.PackageError.ErrorCode, packageErrors[0].ErrorCode);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPackageIdParameterTest()
        {
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();

            var handler = this.GetPackageStatusHandlerInstance(helper);

            handler.Execute(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmptyPackageIdParameterTest()
        {
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();

            var handler = this.GetPackageStatusHandlerInstance(helper);

            handler.Execute(string.Empty);
        }

        private GetPackageStatusHandler GetPackageStatusHandlerInstance(HandlerTestHelper testHelper)
        {
            IPackageRepository packageRepository = testHelper.PackageRepositoryMock.Object;
            ITransferRepository transferRepository = testHelper.TransferRepositoryMock.Object;
            IPackageErrorRepository packageErrorRepository = testHelper.PackageErrorRepositoryMock.Object;
            IMonacoContextFactory contextFactory = testHelper.ContextFactoryMock.Object;

            return new GetPackageStatusHandler(
                packageRepository,
                transferRepository,
                packageErrorRepository,
                contextFactory);
        }
    }
}