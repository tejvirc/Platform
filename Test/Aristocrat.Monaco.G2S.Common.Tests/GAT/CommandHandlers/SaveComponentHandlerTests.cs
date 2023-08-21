namespace Aristocrat.Monaco.G2S.Common.Tests.GAT.CommandHandlers
{
    using System;
    using System.Collections.Generic;
    using Common.GAT.CommandHandlers;
    using Kernel.Contracts.Components;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class SaveComponentHandlerTests
    {
        private readonly Mock<IComponentRegistry> _componentRepositoryMock = new Mock<IComponentRegistry>();

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfComponentRepositoryIsNull()
        {
            new SaveComponentHandler(null);
        }

        [TestMethod]
        public void AddComponentTest()
        {
            _componentRepositoryMock.SetupGet(x => x.Components).Returns(new List<Component>());

            var handler = new SaveComponentHandler(_componentRepositoryMock.Object);

            var component = new Component
            {
                ComponentId = "comp_1",
                FileSystemType = FileSystemType.Directory,
                Path = @"a:\fdd\s~~`!@#$%^&(){}[]+- u\"
            };

            handler.Execute(component);

            _componentRepositoryMock.Verify(x => x.Register(component, false));
        }
    }
}