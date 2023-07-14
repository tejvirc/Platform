namespace Aristocrat.Monaco.Gaming.Tests.ConsumerTests
{
    using System;
    using Consumers;
    using GdkRuntime.V1;
    using Hardware.Contracts.Reel.Events;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Gaming.Runtime;
    using Moq;
    using Test.Common;
    using GDKAnimationState = GdkRuntime.V1.AnimationState;

    [TestClass]
    public class AllLightShowsClearedConsumerTests
    {
        private Mock<IReelService> _reelService;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Default);
        private AllLightShowsClearedConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.AddService(_eventBus);
            _reelService = MoqServiceManager.CreateAndAddService<IReelService>(MockBehavior.Default);

            _target = new AllLightShowsClearedConsumer(_reelService.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void AllLightShowsClearedConsumerNullReelService()
        {
            _target = new AllLightShowsClearedConsumer(null);
        }

        [TestMethod]
        public void ConsumeAllLightShowsClearedEvent()
        {
            var allLightShowsClearedEvent = new AllLightShowsClearedEvent();
            _reelService.Setup(s => s.Connected).Returns(true);

            _target.Consume(allLightShowsClearedEvent);

            var notification = new AnimationUpdatedNotification
            {
                AnimationData = null,
                AnimationId = string.Empty,
                State = GDKAnimationState.AllAnimationsCleared
            };

            _reelService.Verify(s => s.AnimationUpdated(notification), Times.Once);
        }

        [TestMethod]
        public void ConsumeNotConnected()
        {
            var animationUpdatedEvent = new AllLightShowsClearedEvent();
            _reelService.Setup(s => s.Connected).Returns(false);

            _target.Consume(animationUpdatedEvent);

            _reelService.Verify(s => s.AnimationUpdated(It.IsAny<AnimationUpdatedNotification>()), Times.Never);
        }
    }
}