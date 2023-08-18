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
    public class AllLightAnimationsClearedConsumerTests
    {
        private Mock<IReelService> _reelService;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Default);
        private AllLightAnimationsClearedConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.AddService(_eventBus);
            _reelService = MoqServiceManager.CreateAndAddService<IReelService>(MockBehavior.Default);

            _target = new AllLightAnimationsClearedConsumer(_reelService.Object);
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
            _target = new AllLightAnimationsClearedConsumer(null);
        }

        [TestMethod]
        public void ConsumeAllLightShowsClearedEvent()
        {
            _reelService.Setup(s => s.Connected).Returns(true);
            var notification = new AnimationUpdatedNotification
            {
                AnimationData = null,
                AnimationId = string.Empty,
                State = GDKAnimationState.AllAnimationsCleared
            };

            _target.Consume(new AllLightAnimationsClearedEvent());

            _reelService.Verify(s => s.NotifyAnimationUpdated(notification), Times.Once);
        }

        [TestMethod]
        public void ConsumeNotConnected()
        {
            _reelService.Setup(s => s.Connected).Returns(false);

            _target.Consume(new AllLightAnimationsClearedEvent());

            _reelService.Verify(s => s.NotifyAnimationUpdated(It.IsAny<AnimationUpdatedNotification>()), Times.Never);
        }
    }
}