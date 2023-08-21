namespace Aristocrat.Monaco.G2S.Common.Tests.GAT.CommandHandlers
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Common.GAT.CommandHandlers;
    using Common.GAT.Storage;
    using Common.GAT.Validators;
    using FluentValidation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;

    [TestClass]
    public class SaveSpecialFunctionHandlerTests
    {
        private readonly Mock<IMonacoContextFactory> contextFactoryMock = new Mock<IMonacoContextFactory>();

        private readonly IValidator<GatSpecialFunction> gatSpecialFunctionEntityValidator =
            new GatSpecialFunctionEntityValidator();

        private readonly Mock<IGatSpecialFunctionRepository> gatSpecialFunctionRepositoryMock =
            new Mock<IGatSpecialFunctionRepository>();

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfGatSpecialFunctionRepositoryIsNull()
        {
            new SaveSpecialFunctionHandler(null, contextFactoryMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfContextFactoryIsNull()
        {
            new SaveSpecialFunctionHandler(gatSpecialFunctionRepositoryMock.Object, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfGatSpecialFunctionEntityIsNull()
        {
            var handler = new SaveSpecialFunctionHandler(
                this.gatSpecialFunctionRepositoryMock.Object,
                this.contextFactoryMock.Object);

            handler.Execute(null);
        }

        [TestMethod]
        //[Order(0)]
        public void SpecialFunctionEntityInvalidTest()
        {
            var handler = new SaveSpecialFunctionHandler(
                this.gatSpecialFunctionRepositoryMock.Object,
                this.contextFactoryMock.Object);

            handler.Execute(
                new GatSpecialFunction
                {
                    Feature = string.Empty,
                    GatExec = string.Empty
                });

            this.gatSpecialFunctionRepositoryMock.Verify(
                x => x.Add(It.IsAny<DbContext>(), It.IsAny<GatSpecialFunction>()),
                Times.Never);
            this.gatSpecialFunctionRepositoryMock.Verify(
                x => x.Update(It.IsAny<DbContext>(), It.IsAny<GatSpecialFunction>()),
                Times.Never);
        }

        [TestMethod]
        public void AddSpecialFunctionEntityTest()
        {
            var handler = new SaveSpecialFunctionHandler(
                this.gatSpecialFunctionRepositoryMock.Object,
                this.contextFactoryMock.Object);

            var gatSpecialFunctionEntity = new GatSpecialFunction
            {
                Feature = "Feature",
                GatExec = string.Empty
            };

            handler.Execute(gatSpecialFunctionEntity);

            this.gatSpecialFunctionRepositoryMock.Verify(x => x.Add(It.IsAny<DbContext>(), gatSpecialFunctionEntity));
        }

        [TestMethod]
        public void UpdateSpecialFunctionEntityTest()
        {
            var handler = new SaveSpecialFunctionHandler(
                this.gatSpecialFunctionRepositoryMock.Object,
                this.contextFactoryMock.Object);

            var gatSpecialFunctionEntity = new GatSpecialFunction(1)
            {
                Feature = "Feature",
                GatExec = string.Empty
            };

            handler.Execute(gatSpecialFunctionEntity);

            this.gatSpecialFunctionRepositoryMock.Verify(
                x => x.Update(It.IsAny<DbContext>(), gatSpecialFunctionEntity));
        }
    }
}