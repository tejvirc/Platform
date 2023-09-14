namespace Aristocrat.Monaco.Hardware.Tests.Reel.Capabilities
{
    using Contracts.Reel;
    using Contracts.Reel.Events;
    using Contracts.Reel.ImplementationCapabilities;
    using Hardware.Reel.Capabilities;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class ReelSynchronizationCapabilityTests
    {
        private const int ReelId = 3;

        private Mock<IEventBus> _eventBus;
        private readonly Mock<ISynchronizationImplementation> _implementation = new();

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            
            _ = new ReelSynchronizationCapability(_implementation.Object, _eventBus.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void HandleSynchronizationStartedTest()
        {
            var actualArgs = new ReelSynchronizationEventArgs(ReelId);

            _implementation.Raise(x => x.SynchronizationStarted += null, actualArgs);

            _eventBus.Verify(x => x.Publish(It.Is<ReelSynchronizationEvent>(e => e.ReelId == ReelId && e.Status == SynchronizeStatus.Started)), Times.Once);
        }

        [TestMethod]
        public void HandleSynchronizationCompletedTest()
        {
            var actualArgs = new ReelSynchronizationEventArgs(ReelId);

            _implementation.Raise(x => x.SynchronizationCompleted += null, actualArgs);

            _eventBus.Verify(x => x.Publish(It.Is<ReelSynchronizationEvent>(e => e.ReelId == ReelId && e.Status == SynchronizeStatus.Complete)), Times.Once);
        }
    }
}
