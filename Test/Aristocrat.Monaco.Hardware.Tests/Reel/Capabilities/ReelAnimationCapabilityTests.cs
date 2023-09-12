namespace Aristocrat.Monaco.Hardware.Tests.Reel.Capabilities
{
    using System;
    using Contracts.Reel;
    using Contracts.Reel.Events;
    using Contracts.Reel.ImplementationCapabilities;
    using Hardware.Reel.Capabilities;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class ReelAnimationCapabilityTests
    {
        private const int ReelId = 3;
        private const string AnimationName = "animation";
        private const string Tag = "ALL";
        private const AnimationQueueType QueueType = AnimationQueueType.PlayingQueue;
        private const AnimationPreparedStatus PreparedStatus = AnimationPreparedStatus.Prepared;

        private Mock<IEventBus> _eventBus;
        private readonly Mock<IAnimationImplementation> _implementation = new();

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            
            _ = new ReelAnimationCapability(_implementation.Object, _eventBus.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void HandleAllLightAnimationsClearedTest()
        {
            _implementation.Raise(x => x.AllLightAnimationsCleared += null, EventArgs.Empty);

            _eventBus.Verify(x => x.Publish(It.IsAny<AllLightAnimationsClearedEvent>()), Times.Once);
        }

        [TestMethod]
        public void HandleLightAnimationRemovedTest()
        {
            var actualArgs = new LightAnimationEventArgs(AnimationName, QueueType);

            _implementation.Raise(x => x.LightAnimationRemoved += null, actualArgs);

            _eventBus.Verify(x => x.Publish(It.Is<LightAnimationUpdatedEvent>(e =>
                e.State == AnimationState.Removed &&
                e.AnimationName == AnimationName &&
                string.IsNullOrEmpty(e.Tag) &&
                e.QueueType == QueueType &&
                e.PreparedStatus == AnimationPreparedStatus.Unknown
                )), Times.Once);
        }

        [TestMethod]
        public void HandleLightAnimationStartedTest()
        {
            var actualArgs = new LightAnimationEventArgs(AnimationName, Tag);

            _implementation.Raise(x => x.LightAnimationStarted += null, actualArgs);

            _eventBus.Verify(x => x.Publish(It.Is<LightAnimationUpdatedEvent>(e =>
                e.State == AnimationState.Started &&
                e.AnimationName == AnimationName &&
                e.Tag == Tag &&
                e.QueueType == AnimationQueueType.Unknown &&
                e.PreparedStatus == AnimationPreparedStatus.Unknown
            )), Times.Once);
        }

        [TestMethod]
        public void HandleLightAnimationStoppedTest()
        {
            var actualArgs = new LightAnimationEventArgs(AnimationName, Tag, QueueType);

            _implementation.Raise(x => x.LightAnimationStopped += null, actualArgs);

            _eventBus.Verify(x => x.Publish(It.Is<LightAnimationUpdatedEvent>(e =>
                e.State == AnimationState.Stopped &&
                e.AnimationName == AnimationName &&
                e.Tag == Tag &&
                e.QueueType == QueueType &&
                e.PreparedStatus == AnimationPreparedStatus.Unknown
            )), Times.Once);
        }

        [TestMethod]
        public void HandleLightAnimationPreparedTest()
        {
            var actualArgs = new LightAnimationEventArgs(AnimationName, Tag, PreparedStatus);

            _implementation.Raise(x => x.LightAnimationPrepared += null, actualArgs);

            _eventBus.Verify(x => x.Publish(It.Is<LightAnimationUpdatedEvent>(e =>
                e.State == AnimationState.Prepared &&
                e.AnimationName == AnimationName &&
                e.Tag == Tag &&
                e.QueueType == AnimationQueueType.Unknown &&
                e.PreparedStatus == PreparedStatus
            )), Times.Once);
        }

        [TestMethod]
        public void HandleReelAnimationStartedTest()
        {
            var actualArgs = new ReelAnimationEventArgs(ReelId, AnimationName);

            _implementation.Raise(x => x.ReelAnimationStarted += null, actualArgs);

            _eventBus.Verify(x => x.Publish(It.Is<ReelAnimationUpdatedEvent>(e =>
                e.State == AnimationState.Started &&
                e.ReelId == ReelId &&
                e.AnimationName == AnimationName &&
                e.PreparedStatus == AnimationPreparedStatus.Unknown
            )), Times.Once);
        }

        [TestMethod]
        public void HandleReelAnimationStoppedTest()
        {
            var actualArgs = new ReelAnimationEventArgs(ReelId, AnimationName);

            _implementation.Raise(x => x.ReelAnimationStopped += null, actualArgs);

            _eventBus.Verify(x => x.Publish(It.Is<ReelAnimationUpdatedEvent>(e =>
                e.State == AnimationState.Stopped &&
                e.ReelId == ReelId &&
                e.AnimationName == AnimationName &&
                e.PreparedStatus == AnimationPreparedStatus.Unknown
            )), Times.Once);
        }

        [TestMethod]
        public void HandleReelAnimationPreparedTest()
        {
            var actualArgs = new ReelAnimationEventArgs(AnimationName, PreparedStatus);

            _implementation.Raise(x => x.ReelAnimationPrepared += null, actualArgs);

            _eventBus.Verify(x => x.Publish(It.Is<ReelAnimationUpdatedEvent>(e =>
                e.State == AnimationState.Prepared &&
                e.ReelId == 0 &&
                e.AnimationName == AnimationName &&
                e.PreparedStatus == PreparedStatus
            )), Times.Once);
        }
    }
}
