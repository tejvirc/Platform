namespace Aristocrat.Monaco.Asp.Tests.Client.Consumers
{
    using Asp.Client.Consumers;
    using Asp.Client.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Application.Contracts;
    using Test.Common;

    [TestClass]
    public class SystemDisabledByOperatorEventConsumerTests
    {
        private Mock<ICurrentMachineModeStateManager>  _currentMachineModeStateManager;
        private SystemDisabledByOperatorEventConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _currentMachineModeStateManager = new Mock<ICurrentMachineModeStateManager>(MockBehavior.Strict);
            _target = new SystemDisabledByOperatorEventConsumer(_currentMachineModeStateManager.Object);
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

            _currentMachineModeStateManager.Setup(
                x => x.HandleEvent(It.IsAny<SystemDisabledByOperatorEvent>())).Verifiable();

            _target.Consume(new SystemDisabledByOperatorEvent());

            _currentMachineModeStateManager.Verify();
        }
    }
}
