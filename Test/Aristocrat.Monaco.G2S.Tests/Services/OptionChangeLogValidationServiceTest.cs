namespace Aristocrat.Monaco.G2S.Tests.Services
{
    using System.Data.Entity;
    using Aristocrat.G2S;
    using Data.OptionConfig;
    using G2S.Services;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;
    using Test.Common;

    [TestClass]
    public class OptionChangeLogValidationServiceTest
    {
        private readonly Mock<IOptionChangeLogRepository> _changeLogRepositoryMock =
            new Mock<IOptionChangeLogRepository>();

        private readonly Mock<IMonacoContextFactory> _contextFactoryMock =
            new Mock<IMonacoContextFactory>();

        private readonly long transactionId = 2;

        [TestMethod]
        public void WhenConstructWithNullsExpectException()
        {
            ConstructorTest.TestConstructorNullChecks<OptionChangeLogValidationService>();
        }

        [TestMethod]
        public void WhenPendingChangeLogNotFoundExpectError()
        {
            _changeLogRepositoryMock.Setup(x => x.GetByTransactionId(It.IsAny<DbContext>(), transactionId))
                .Returns(new OptionChangeLog());

            var service = new OptionChangeLogValidationService(
                _changeLogRepositoryMock.Object,
                _contextFactoryMock.Object);

            var errorCode = service.Validate(transactionId);

            Assert.IsTrue(errorCode == ErrorCode.G2S_OCX006);
        }

        [TestMethod]
        public void WhenChangeLogNotFoundByGetByTransactionIdExpectError()
        {
            var service = new OptionChangeLogValidationService(
                _changeLogRepositoryMock.Object,
                _contextFactoryMock.Object);
            var errorCode = service.Validate(transactionId);

            _changeLogRepositoryMock.Verify(
                x => x.GetByTransactionId(It.IsAny<DbContext>(), transactionId),
                Times.Once);

            Assert.IsTrue(errorCode == ErrorCode.G2S_OCX005);
        }

        [TestMethod]
        public void WhenFoundExpectSuccess()
        {
            _changeLogRepositoryMock.Setup(x => x.GetPendingByTransactionId(It.IsAny<DbContext>(), transactionId))
                .Returns(new OptionChangeLog());

            _changeLogRepositoryMock.Setup(x => x.GetByTransactionId(It.IsAny<DbContext>(), transactionId))
                .Returns(new OptionChangeLog());

            var service = new OptionChangeLogValidationService(
                _changeLogRepositoryMock.Object,
                _contextFactoryMock.Object);
            var errorCode = service.Validate(transactionId);

            _changeLogRepositoryMock.Verify(
                x => x.GetByTransactionId(It.IsAny<DbContext>(), transactionId),
                Times.Once);
            _changeLogRepositoryMock.Verify(
                x => x.GetPendingByTransactionId(It.IsAny<DbContext>(), transactionId),
                Times.Once);

            Assert.IsTrue(errorCode == string.Empty);
        }
    }
}