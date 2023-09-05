namespace Aristocrat.Monaco.Gaming.Tests.ConsumerTests
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Gaming.Runtime;
    using Consumers;
    using Hardware.Contracts.Reel;
    using Hardware.Contracts.Reel.Events;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class ReelStoppedConsumerTests
    {
        private Mock<IReelService> _reelService;
        private readonly Mock<IEventBus> _eventBus = new Mock<IEventBus>(MockBehavior.Default);
        private ReelStoppedConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.AddService(_eventBus);
            _reelService = MoqServiceManager.CreateAndAddService<IReelService>(MockBehavior.Default);

            _target = new ReelStoppedConsumer(
                _reelService.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullReelService()
        {
            _target = new ReelStoppedConsumer(null);
        }

        [TestMethod]
        public void Consume()
        {
            const int stopStep = 180;
            var reelStoppedEvent = new ReelStoppedEvent(10, stopStep, false);
            _reelService.Setup(s => s.Connected).Returns(true);

            _target.Consume(reelStoppedEvent);

            var reelState = new Dictionary<int, ReelLogicalState>
            {
                { reelStoppedEvent.ReelId, ReelLogicalState.IdleAtStop }
            };

            _reelService.Verify(s => s.UpdateReelState(reelState, stopStep), Times.Once);
        }

        [TestMethod]
        public void ConsumeHomeReelStop()
        {
            const int stopStep = 180;
            var reelStoppedEvent = new ReelStoppedEvent(10, stopStep, true);
            _reelService.Setup(s => s.Connected).Returns(true);

            _target.Consume(reelStoppedEvent);

            _reelService.Verify(s => s.UpdateReelState(It.IsAny<Dictionary<int, ReelLogicalState>>(), stopStep), Times.Never);
        }

        [TestMethod]
        public void ConsumeNotConnected()
        {
            var reelStoppedEvent = new ReelStoppedEvent(10, 180, true);
            _reelService.Setup(s => s.Connected).Returns(false);

            _target.Consume(reelStoppedEvent);

            var reelState = new Dictionary<int, ReelLogicalState>
            {
                { reelStoppedEvent.ReelId, ReelLogicalState.IdleAtStop }
            };

            _reelService.Verify(s => s.UpdateReelState(It.IsAny<Dictionary<int, ReelLogicalState>>(), It.IsAny<int>()), Times.Never);
        }
    }
}