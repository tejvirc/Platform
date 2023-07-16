namespace Aristocrat.Monaco.Gaming.Tests.ConsumerTests
{
    using System;
    using Consumers;
    using GdkRuntime.V1;
    using Google.Protobuf.WellKnownTypes;
    using Hardware.Contracts.Reel.Events;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Gaming.Runtime;
    using Moq;
    using Test.Common;
    using AnimationState = Hardware.Contracts.Reel.AnimationState;
    using GDKAnimationState = GdkRuntime.V1.AnimationState;

    [TestClass]
    public class LightShowAnimationUpdatedConsumerTests
    {
        private Mock<IReelService> _reelService;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Default);
        private LightShowAnimationUpdatedConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.AddService(_eventBus);
            _reelService = MoqServiceManager.CreateAndAddService<IReelService>(MockBehavior.Default);

            _target = new LightShowAnimationUpdatedConsumer(_reelService.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void LightShowAnimationUpdatedConsumerNullReelService()
        {
            _target = new LightShowAnimationUpdatedConsumer(null);
        }

        [TestMethod]
        [DataRow(AnimationState.Started, GDKAnimationState.AnimationPlaying)]
        [DataRow(AnimationState.Stopped, GDKAnimationState.AnimationStopped)]
        [DataRow(AnimationState.Prepared, GDKAnimationState.AnimationsPrepared)]
        [DataRow(AnimationState.Removed, GDKAnimationState.AnimationRemoved)]
        public void ConsumeLightShowAnimationUpdatedEvent(AnimationState givenState, GDKAnimationState expectedState)
        {
            var animationUpdatedEvent = new LightShowAnimationUpdatedEvent("animation", "all", givenState);
            _reelService.Setup(s => s.Connected).Returns(true);

            _target.Consume(animationUpdatedEvent);

            var notification = new AnimationUpdatedNotification
            {
                AnimationData = Any.Pack(new LightshowAnimationData { Tag = animationUpdatedEvent.Tag }),
                AnimationId = animationUpdatedEvent.AnimationName,
                State = expectedState
            };

            _reelService.Verify(s => s.AnimationUpdated(notification), Times.Once);
        }

        [TestMethod]
        public void ConsumeNotConnected()
        {
            var animationUpdatedEvent = new LightShowAnimationUpdatedEvent("animation", "all", AnimationState.Started);
            _reelService.Setup(s => s.Connected).Returns(false);

            _target.Consume(animationUpdatedEvent);

            _reelService.Verify(s => s.AnimationUpdated(It.IsAny<AnimationUpdatedNotification>()), Times.Never);
        }
    }
}