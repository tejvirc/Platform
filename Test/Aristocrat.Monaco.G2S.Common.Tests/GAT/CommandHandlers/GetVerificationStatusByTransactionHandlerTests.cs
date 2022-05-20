namespace Aristocrat.Monaco.G2S.Common.Tests.GAT.CommandHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using Common.GAT.CommandHandlers;
    using Common.GAT.Exceptions;
    using Common.GAT.Storage;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;
    using Test.Common;

    [TestClass]
    public class GetVerificationStatusByTransactionHandlerTests
    {
        private readonly Mock<IGatComponentVerificationRepository> componentVerificationRepositoryMock =
            new Mock<IGatComponentVerificationRepository>();

        private readonly Mock<IMonacoContextFactory> contextFactoryMock =
            new Mock<IMonacoContextFactory>();

        private readonly Mock<IGatVerificationRequestRepository> verificationRequestRepositoryMock =
            new Mock<IGatVerificationRequestRepository>();

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfComponentVerificationRequestRepositoryIsNull()
        {
            new GetVerificationStatusByTransactionHandler(
                null,
                this.componentVerificationRepositoryMock.Object,
                this.contextFactoryMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfComponentVerificationRepositoryIsNull()
        {
            new GetVerificationStatusByTransactionHandler(
                this.verificationRequestRepositoryMock.Object,
                null,
                this.contextFactoryMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfContextFactoryIsNull()
        {
            new GetVerificationStatusByTransactionHandler(
                this.verificationRequestRepositoryMock.Object,
                this.componentVerificationRepositoryMock.Object,
                null);
        }

        [TestMethod]
        [ExpectedException(typeof(VerificationRequestNotFoundException))]
        public void ThrowIfComponentVerificationRequestNotFound()
        {
            this.verificationRequestRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<GatVerificationRequest, bool>>>()))
                .Returns(
                    (DbContext context, Expression<Func<GatVerificationRequest, bool>> predicate) =>
                        new List<GatVerificationRequest>().AsQueryable());

            var handler = new GetVerificationStatusByTransactionHandler(
                this.verificationRequestRepositoryMock.Object,
                this.componentVerificationRepositoryMock.Object,
                this.contextFactoryMock.Object);

            handler.Execute(new GetVerificationStatusByTransactionArgs(1, 1));
        }

        [TestMethod]
        [ExpectedException(typeof(TransactionIdNotCorrespondingVerificationIdException))]
        public void ThrowIfTransactionIdNotCorrespondingVerificationId()
        {
            this.verificationRequestRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<GatVerificationRequest, bool>>>()))
                .Returns(
                    (DbContext context, Expression<Func<GatVerificationRequest, bool>> predicate) =>
                        new List<GatVerificationRequest>
                        {
                            new GatVerificationRequest
                            {
                                VerificationId = 1,
                                TransactionId = 2
                            }
                        }.AsQueryable());

            var handler = new GetVerificationStatusByTransactionHandler(
                this.verificationRequestRepositoryMock.Object,
                this.componentVerificationRepositoryMock.Object,
                this.contextFactoryMock.Object);

            handler.Execute(new GetVerificationStatusByTransactionArgs(1, 1));
        }

        [TestMethod]
        public void ComponentVerificationRequestCompletedTest()
        {
            this.verificationRequestRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<GatVerificationRequest, bool>>>()))
                .Returns(
                    (DbContext context, Expression<Func<GatVerificationRequest, bool>> predicate) =>
                        new List<GatVerificationRequest>
                        {
                            new GatVerificationRequest(1)
                            {
                                Completed = true,
                                VerificationId = 1,
                                TransactionId = 2
                            }
                        }.AsQueryable());
            var handler = new GetVerificationStatusByTransactionHandler(
                this.verificationRequestRepositoryMock.Object,
                this.componentVerificationRepositoryMock.Object,
                this.contextFactoryMock.Object);

            var result = handler.Execute(new GetVerificationStatusByTransactionArgs(2, 1));

            Assert.IsTrue(result.VerificationStatus == null && result.ComponentVerificationResults != null);
        }

        [TestMethod]
        public void ComponentVerificationRequestNotCompletedTest()
        {
            this.verificationRequestRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<GatVerificationRequest, bool>>>()))
                .Returns(
                    (DbContext context, Expression<Func<GatVerificationRequest, bool>> predicate) =>
                        new List<GatVerificationRequest>
                        {
                            new GatVerificationRequest(1)
                            {
                                Completed = false,
                                VerificationId = 1,
                                TransactionId = 2
                            }
                        }.AsQueryable());

            var handler = new GetVerificationStatusByTransactionHandler(
                this.verificationRequestRepositoryMock.Object,
                this.componentVerificationRepositoryMock.Object,
                this.contextFactoryMock.Object);

            var result = handler.Execute(new GetVerificationStatusByTransactionArgs(2, 1));

            Assert.IsTrue(result.VerificationStatus != null && result.ComponentVerificationResults == null);
        }
    }
}