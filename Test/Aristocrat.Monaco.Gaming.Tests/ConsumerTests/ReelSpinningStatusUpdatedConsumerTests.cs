namespace Aristocrat.Monaco.Gaming.Tests.ConsumerTests
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Consumers;
    using Gaming.Runtime;
    using Hardware.Contracts.Reel.Events;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class ReelSpinningStatusUpdatedConsumerTests
    {
        private Mock<IReelService> _reelService;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Default);
        private ReelSpinningStatusUpdatedConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.AddService(_eventBus);
            _reelService = MoqServiceManager.CreateAndAddService<IReelService>(MockBehavior.Default);

            _target = new ReelSpinningStatusUpdatedConsumer(_reelService.Object);
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
            _target = new ReelSpinningStatusUpdatedConsumer(null);
        }

        [TestMethod]
        [DataRow(SpinVelocity.Constant, ReelLogicalState.SpinningConstant)]
        [DataRow(SpinVelocity.Accelerating, ReelLogicalState.Accelerating)]
        [DataRow(SpinVelocity.Decelerating, ReelLogicalState.Decelerating)]
        public void Consume(SpinVelocity givenState, ReelLogicalState expectedState)
        {
            var reelSpinStatusEvent = new ReelSpinningStatusUpdatedEvent(0, givenState);
            _reelService.Setup(s => s.Connected).Returns(true);

            _target.Consume(reelSpinStatusEvent);

            var reelState = new Dictionary<int, ReelLogicalState>
            {
                { 0, expectedState}
            };

            _reelService.Verify(s => s.UpdateReelState(reelState), Times.Once);
        }

        [TestMethod]
        public void ConsumeSpinStatusIsNone()
        {
            var reelSpinStatusEvent = new ReelSpinningStatusUpdatedEvent(0, SpinVelocity.None);
            _reelService.Setup(s => s.Connected).Returns(true);

            _target.Consume(reelSpinStatusEvent);

            _reelService.Verify(s => s.UpdateReelState(It.IsAny<Dictionary<int, ReelLogicalState>>()), Times.Never);
        }

        [TestMethod]
        public void ConsumeNotConnected()
        {
            var reelSpinStatusEvent = new ReelSpinningStatusUpdatedEvent(0, SpinVelocity.Constant);
            _reelService.Setup(s => s.Connected).Returns(false);

            _target.Consume(reelSpinStatusEvent);

            _reelService.Verify(s => s.UpdateReelState(It.IsAny<Dictionary<int, ReelLogicalState>>()), Times.Never);
        }
    }
}