namespace Aristocrat.Monaco.Gaming.Tests.ConsumerTests
{
    using System;
    using Consumers;
    using Gaming.Runtime;
    using Hardware.Contracts.Reel.Events;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class ReelStoppingConsumerTests
    {
        private Mock<IReelService> _reelService;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Default);
        private ReelStoppingConsumer _target;

        [TestInitialize]
        public void TestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.AddService(_eventBus);
            _reelService = MoqServiceManager.CreateAndAddService<IReelService>(MockBehavior.Default);

            _target = new ReelStoppingConsumer(_reelService.Object);
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
            _target = new ReelStoppingConsumer(null);
        }

        [DataTestMethod]
        public void ConsumeEventTest()
        {
            const int reelId = 1;
            const int stopTime = 100;
            _reelService.Setup(s => s.Connected).Returns(true);

            _target.Consume(new ReelStoppingEvent(reelId, stopTime));

            _reelService.Verify(s => s.NotifyReelStopping(reelId, stopTime), Times.Once);
        }

        [TestMethod]
        public void ConsumeNotConnectedTest()
        {
            _reelService.Setup(s => s.Connected).Returns(false);

            _target.Consume(new ReelStoppingEvent(It.IsAny<int>(), It.IsAny<int>()));

            _reelService.Verify(s => s.NotifyReelStopping(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }
    }
}
