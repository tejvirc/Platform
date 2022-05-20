namespace Aristocrat.Monaco.G2S.Common.Tests.GAT.CommandHandlers
{
    using System;
    using System.Data.Entity;
    using Common.GAT.CommandHandlers;
    using Common.GAT.Storage;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;

    [TestClass]
    public class GetSpecialFunctionsHandlerTests
    {
        private readonly Mock<IMonacoContextFactory> contextFactoryMock = new Mock<IMonacoContextFactory>();

        private readonly Mock<IGatSpecialFunctionRepository> gatSpecialFunctionRepositoryMock =
            new Mock<IGatSpecialFunctionRepository>();

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfGatSpecialFunctionRepositoryIsNull()
        {
            new GetSpecialFunctionsHandler(null, this.contextFactoryMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfContextFactoryIsNull()
        {
            new GetSpecialFunctionsHandler(
                this.gatSpecialFunctionRepositoryMock.Object,
                null);
        }

        [TestMethod]
        public void SelectGatSpecialFunctionsTest()
        {
            var handler = new GetSpecialFunctionsHandler(
                this.gatSpecialFunctionRepositoryMock.Object,
                this.contextFactoryMock.Object);

            handler.Execute();

            this.gatSpecialFunctionRepositoryMock.Verify(
                x => x.GetAll(It.IsAny<DbContext>()));
        }
    }
}