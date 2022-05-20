namespace Aristocrat.Monaco.G2S.Common.Tests.GAT.CommandHandlers
{
    using System;
    using Common.GAT.CommandHandlers;
    using Kernel.Contracts.Components;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetComponentsHandlerTests
    {
        private readonly Mock<IComponentRegistry> _componentRepositoryMock = new Mock<IComponentRegistry>();

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfIComponentRepositoryIsNull()
        {
            new GetComponentsHandler(null);
        }

        [TestMethod]
        public void SelectComponentsTest()
        {
            var handler = new GetComponentsHandler(
                _componentRepositoryMock.Object);

            handler.Execute();

            _componentRepositoryMock.Verify(x => x.Components);
        }
    }
}