namespace Aristocrat.Monaco.Gaming.Tests.ConsumerTests
{
    using System;
    using Consumers;
    using Gaming.Runtime;
    using GdkRuntime.V1;
    using Google.Protobuf.WellKnownTypes;
    using Hardware.Contracts.Reel;
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
        private const string AnimationName = "animation";

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
        [DataRow(AnimationState.Started, GDKAnimationState.AnimationPlaying, 1)]
        [DataRow(AnimationState.Stopped, GDKAnimationState.AnimationStopped, 2)]
        public void ConsumeStartStop(AnimationState givenState, GDKAnimationState expectedState, int reelId)
        {
            var animationUpdatedEvent = new ReelAnimationUpdatedEvent(reelId, AnimationName, givenState);
            _reelService.Setup(s => s.Connected).Returns(true);

            _target.Consume(animationUpdatedEvent);

            var expectedNotification = new AnimationUpdatedNotification
            {
                AnimationData = Any.Pack(new ReelAnimationData { ReelIndex = (uint)reelId }),
                AnimationId = AnimationName,
                State = expectedState
            };

            _reelService.Verify(s => s.NotifyAnimationUpdated(expectedNotification), Times.Once);
        }

        [TestMethod]
        public void ConsumePrepared()
        {
            var animationUpdatedEvent = new ReelAnimationUpdatedEvent(AnimationName, AnimationPreparedStatus.Prepared);
            _reelService.Setup(s => s.Connected).Returns(true);

            _target.Consume(animationUpdatedEvent);

            var expectedNotification = new AnimationUpdatedNotification
            {
                AnimationData = Any.Pack(new ReelAnimationData { ReelIndex = 0 }),
                AnimationId = AnimationName,
                State = GDKAnimationState.AnimationsPrepared
            };

            _reelService.Verify(s => s.NotifyAnimationUpdated(expectedNotification), Times.Once);
        }

        [TestMethod]
        public void ConsumeNotConnected()
        {
            var animationUpdatedEvent = new ReelAnimationUpdatedEvent(0, AnimationName, AnimationState.Started);
            _reelService.Setup(s => s.Connected).Returns(false);

            _target.Consume(animationUpdatedEvent);

            _reelService.Verify(s => s.NotifyAnimationUpdated(It.IsAny<AnimationUpdatedNotification>()), Times.Never);
        }
    }
}