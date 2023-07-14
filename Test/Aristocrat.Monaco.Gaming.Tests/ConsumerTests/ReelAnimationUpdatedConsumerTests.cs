namespace Aristocrat.Monaco.Gaming.Tests.ConsumerTests
{
    using System;
    using Consumers;
    using Gaming.Runtime;
    using GdkRuntime.V1;
    using Google.Protobuf.WellKnownTypes;
    using Hardware.Contracts.Reel.Events;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using AnimationState = Hardware.Contracts.Reel.AnimationState;
    using GDKAnimationState = GdkRuntime.V1.AnimationState;

    [TestClass]
    public class ReelAnimationUpdatedConsumerTests
    {
        private Mock<IReelService> _reelService;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Default);
        private ReelAnimationUpdatedConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.AddService(_eventBus);
            _reelService = MoqServiceManager.CreateAndAddService<IReelService>(MockBehavior.Default);

            _target = new ReelAnimationUpdatedConsumer(
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
            _target = new ReelAnimationUpdatedConsumer(null);
        }

        [TestMethod]
        [DataRow(AnimationState.Started, GDKAnimationState.AnimationPlaying)]
        [DataRow(AnimationState.Stopped, GDKAnimationState.AnimationStopped)]
        [DataRow(AnimationState.Prepared, GDKAnimationState.AnimationsPrepared)]
        public void Consume(AnimationState givenState, GDKAnimationState expectedState)
        {
            var animationUpdatedEvent = new ReelAnimationUpdatedEvent("animation", 0, givenState);
            _reelService.Setup(s => s.Connected).Returns(true);

            _target.Consume(animationUpdatedEvent);

            var notification = new AnimationUpdatedNotification
            {
                AnimationData = Any.Pack(new ReelAnimationData { ReelIndex = animationUpdatedEvent.ReelIndex }),
                AnimationId = animationUpdatedEvent.AnimationName,
                State = expectedState
            };

            _reelService.Verify(s => s.AnimationUpdated(notification), Times.Once);
        }

        [TestMethod]
        public void ConsumeNotConnected()
        {
            var animationUpdatedEvent = new ReelAnimationUpdatedEvent("animation", 0, AnimationState.Started);
            _reelService.Setup(s => s.Connected).Returns(false);

            _target.Consume(animationUpdatedEvent);

            _reelService.Verify(s => s.AnimationUpdated(It.IsAny<AnimationUpdatedNotification>()), Times.Never);
        }
    }
}