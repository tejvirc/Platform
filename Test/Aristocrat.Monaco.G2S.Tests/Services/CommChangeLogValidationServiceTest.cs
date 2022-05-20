namespace Aristocrat.Monaco.G2S.Tests.Services
{
    using System.Data.Entity;
    using Aristocrat.G2S;
    using Data.CommConfig;
    using G2S.Services;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;
    using Test.Common;

    [TestClass]
    public class CommChangeLogValidationServiceTest
    {
        private readonly Mock<ICommChangeLogRepository> _changeLogRepositoryMock = new Mock<ICommChangeLogRepository>();

        private readonly Mock<IMonacoContextFactory> _contextFactoryMock = new Mock<IMonacoContextFactory>();

        private readonly long transactionId = 2;

        [TestMethod]
        public void WhenConstructWithNullsExpectException()
        {
            ConstructorTest.TestConstructorNullChecks<CommChangeLogValidationService>();
        }

        [TestMethod]
        public void ReturnErrorCodeIfPendingChangeLogNotFoundTest()
        {
            var service = new CommChangeLogValidationService(
                _changeLogRepositoryMock.Object,
                _contextFactoryMock.Object);

            var errorCode = service.Validate(transactionId);

            Assert.IsTrue(errorCode == ErrorCode.G2S_CCX005);
        }

        [TestMethod]
        public void ReturnErrorCodeIfChangeLogNotFoundByGetByTransactionIdTest()
        {
            _changeLogRepositoryMock.Setup(x => x.GetByTransactionId(It.IsAny<DbContext>(), transactionId))
                .Returns(new CommChangeLog());

            var service = new CommChangeLogValidationService(
                _changeLogRepositoryMock.Object,
                _contextFactoryMock.Object);
            var errorCode = service.Validate(transactionId);

            _changeLogRepositoryMock.Verify(
                x => x.GetPendingByTransactionId(It.IsAny<DbContext>(), transactionId),
                Times.Once);

            Assert.IsTrue(errorCode == ErrorCode.G2S_CCX006);
        }

        [TestMethod]
        public void ReturnEmptyErrorCodeTest()
        {
            _changeLogRepositoryMock.Setup(
                    x => x.GetPendingByTransactionId(It.IsAny<DbContext>(), transactionId))
                .Returns(new CommChangeLog());

            _changeLogRepositoryMock.Setup(x => x.GetByTransactionId(It.IsAny<DbContext>(), transactionId))
                .Returns(new CommChangeLog());

            var service = new CommChangeLogValidationService(
                _changeLogRepositoryMock.Object,
                _contextFactoryMock.Object);
            var errorCode = service.Validate(transactionId);

            _changeLogRepositoryMock.Verify(
                x => x.GetPendingByTransactionId(It.IsAny<DbContext>(), transactionId),
                Times.Once);

            _changeLogRepositoryMock.Verify(
                x => x.GetByTransactionId(It.IsAny<DbContext>(), transactionId),
                Times.Once);

            Assert.IsTrue(errorCode == string.Empty);
        }
    }
}