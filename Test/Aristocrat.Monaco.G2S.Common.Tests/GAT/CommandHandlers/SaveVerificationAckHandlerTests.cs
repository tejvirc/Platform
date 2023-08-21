namespace Aristocrat.Monaco.G2S.Common.Tests.GAT.CommandHandlers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
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
    public class SaveVerificationAckHandlerTests
    {
        private readonly Mock<IGatComponentVerificationRepository> componentVerificationRepositoryMock =
            new Mock<IGatComponentVerificationRepository>();

        private readonly Mock<IMonacoContextFactory> contextFactory = new Mock<IMonacoContextFactory>();

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
        public void ThrowIfVerificationRequestRepositoryIsNull()
        {
            new SaveVerificationAckHandler(
                null,
                this.componentVerificationRepositoryMock.Object,
                this.contextFactory.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfComponentVerificationRepositoryIsNull()
        {
            new SaveVerificationAckHandler(
                this.verificationRequestRepositoryMock.Object,
                null,
                this.contextFactory.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfContextFactoryIsNull()
        {
            new SaveVerificationAckHandler(
                this.verificationRequestRepositoryMock.Object,
                this.componentVerificationRepositoryMock.Object,
                null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfSaveVerificationAckArgsIsNull()
        {
            var handler = new SaveVerificationAckHandler(
                this.verificationRequestRepositoryMock.Object,
                this.componentVerificationRepositoryMock.Object,
                this.contextFactory.Object);

            handler.Execute(null);
        }

        [TestMethod]
        public void ThrowIsSaveVerificationAckArgsInvalid()
        {
            AssertHelper.Throws<ArgumentOutOfRangeException>(
                () => new SaveVerificationAckArgs(
                    0,
                    1,
                    new List<ComponentVerification>
                    {
                        new ComponentVerification(
                            "c1",
                            true)
                    }));

            AssertHelper.Throws<ArgumentOutOfRangeException>(
                () => new SaveVerificationAckArgs(
                    1,
                    0,
                    new List<ComponentVerification>
                    {
                        new ComponentVerification(
                            "c1",
                            true)
                    }));

            AssertHelper.Throws<ArgumentNullException>(() => new SaveVerificationAckArgs(1, 1, null));

            AssertHelper.Throws<ArgumentException>(
                () => new SaveVerificationAckArgs(1, 1, new List<ComponentVerification>()));
        }

        [TestMethod]
        [ExpectedException(typeof(VerificationRequestNotFoundException))]
        public void ThrowIsVerificationRequestNotFoundTest()
        {
            this.verificationRequestRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<GatVerificationRequest, bool>>>()))
                .Returns(
                    (DbContext context, Expression<Func<GatVerificationRequest, bool>> predicate) =>
                        new List<GatVerificationRequest>().AsQueryable());

            var handler = new SaveVerificationAckHandler(
                this.verificationRequestRepositoryMock.Object,
                this.componentVerificationRepositoryMock.Object,
                this.contextFactory.Object);

            handler.Execute(this.CreateSaveVerificationAckArgs());
        }

        [TestMethod]
        public void UpdateEntityTest()
        {
            this.ConfigureVerificationRequestRepositoryMock();
            this.ConfigureComponentVerificationRepository("c1");

            var handler = new SaveVerificationAckHandler(
                this.verificationRequestRepositoryMock.Object,
                this.componentVerificationRepositoryMock.Object,
                this.contextFactory.Object);

            handler.Execute(this.CreateSaveVerificationAckArgs());

            this.componentVerificationRepositoryMock
                .Verify(
                    x =>
                        x.Update(
                            It.IsAny<DbContext>(),
                            It.Is<GatComponentVerification>(
                                entity =>
                                    (entity.ComponentId == "c1") &&
                                    (entity.State == ComponentVerificationState.Passed))));
        }

        [TestMethod]
        public void NotUpdateIfComponentIdUnknown()
        {
            this.ConfigureVerificationRequestRepositoryMock();
            this.ConfigureComponentVerificationRepository("c1");

            var handler = new SaveVerificationAckHandler(
                this.verificationRequestRepositoryMock.Object,
                this.componentVerificationRepositoryMock.Object,
                this.contextFactory.Object);

            handler.Execute(this.CreateSaveVerificationAckArgs());

            this.componentVerificationRepositoryMock
                .Verify(
                    x => x.Update(
                        It.IsAny<DbContext>(),
                        It.Is<GatComponentVerification>(entity => entity.ComponentId == "c2")),
                    Times.Never);
        }

        private void ConfigureComponentVerificationRepository(string componentId)
        {
            this.componentVerificationRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<GatComponentVerification, bool>>>()))
                .Returns(
                    new List<GatComponentVerification>
                    {
                        new GatComponentVerification
                        {
                            ComponentId = componentId
                        }
                    }.AsQueryable());
        }

        private void ConfigureVerificationRequestRepositoryMock()
        {
            this.verificationRequestRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<GatVerificationRequest, bool>>>()))
                .Returns(
                    (DbContext context, Expression<Func<GatVerificationRequest, bool>> predicate) =>
                        new List<GatVerificationRequest>
                        {
                            new GatVerificationRequest(1)
                        }.AsQueryable());
        }

        private SaveVerificationAckArgs CreateSaveVerificationAckArgs()
        {
            return new SaveVerificationAckArgs(
                1,
                1,
                new List<ComponentVerification>
                {
                    new ComponentVerification(
                        "c1",
                        true)
                });
        }
    }
}