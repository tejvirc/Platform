namespace Aristocrat.Monaco.Gaming.Tests.ConsumerTests
{
    using System;
    using Consumers;
    using Gaming.Runtime;
    using Hardware.Contracts.Reel;
    using Hardware.Contracts.Reel.Events;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class ReelSynchronizationConsumerTests
    {
        private Mock<IReelService> _reelService;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Default);
        private ReelSynchronizationConsumer _target;

        [TestInitialize]
        public void TestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.AddService(_eventBus);
            _reelService = MoqServiceManager.CreateAndAddService<IReelService>(MockBehavior.Default);

            _target = new ReelSynchronizationConsumer(_reelService.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullReelServiceTest()
        {
            _target = new ReelSynchronizationConsumer(null);
        }

        [DataTestMethod]
        [DataRow(1, SynchronizeStatus.Complete)]
        [DataRow(2, SynchronizeStatus.Complete)]
        public void ConsumeEventTest(int reelId, SynchronizeStatus status)
        {
            _reelService.Setup(s => s.Connected).Returns(true);

            _target.Consume(new ReelSynchronizationEvent(reelId, status));

            _reelService.Verify(s => s.NotifyReelSynchronizationStatus(reelId, status), Times.Once);
        }

        [TestMethod]
        public void ConsumeNotConnectedTest()
        {
            _reelService.Setup(s => s.Connected).Returns(false);

            _target.Consume(new ReelSynchronizationEvent(It.IsAny<int>(), It.IsAny<SynchronizeStatus>()));

            _reelService.Verify(s => s.NotifyReelSynchronizationStatus(It.IsAny<int>(), It.IsAny<SynchronizeStatus>()), Times.Never);
        }
    }
}
