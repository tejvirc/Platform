namespace Aristocrat.Monaco.Asp.Tests.Client.Consumers
{
    using System;
    using Asp.Client.Consumers;
    using Asp.Client.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class SystemDisableAddedEventConsumerTests
    {
        private Mock<IGameStatusProvider> _gameStatusProvider;
        private SystemDisableAddedEventConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _gameStatusProvider = new Mock<IGameStatusProvider>(MockBehavior.Strict);
            _target = new SystemDisableAddedEventConsumer(_gameStatusProvider.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ConsumeTest()
        {

            _gameStatusProvider.Setup(
                x => x.HandleEvent(It.IsAny<SystemDisableAddedEvent>())).Verifiable();

            _target.Consume(new SystemDisableAddedEvent(SystemDisablePriority.Immediate, Guid.NewGuid(), "", false));

            _gameStatusProvider.Verify();
        }
    }
}
