namespace Aristocrat.Monaco.Asp.Tests.Client.Consumers
{
    using Asp.Client.Consumers;
    using Asp.Client.Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class PersistentStorageIntegrityCheckFailedEventConsumerTests
    {
        private Mock<ICurrentMachineModeStateManager>  _currentMachineModeStateManager;
        private PersistentStorageIntegrityCheckFailedEventConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _currentMachineModeStateManager = new Mock<ICurrentMachineModeStateManager>(MockBehavior.Strict);
            _target = new PersistentStorageIntegrityCheckFailedEventConsumer(_currentMachineModeStateManager.Object);
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
                x => x.HandleEvent(It.IsAny<PersistentStorageIntegrityCheckFailedEvent>())).Verifiable();

            _target.Consume(new PersistentStorageIntegrityCheckFailedEvent());

            _currentMachineModeStateManager.Verify();
        }
    }
}
