namespace Aristocrat.Monaco.Gaming.Tests.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GdkRuntime.V1;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Aristocrat.Monaco.Hardware.Contracts.Reel.Capabilities;
    using Gaming.Commands;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using Enum = System.Enum;

    /// <summary>
    ///     GetReelStateCommandHandler unit tests
    /// </summary>
    [TestClass]
    public class GetReelStateCommandHandlerTests
    {
        private Mock<IReelController> _reelController;
        private GetReelStateCommandHandler _target;

        [TestInitialize]
        public void TestInitialization()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _reelController = MoqServiceManager.CreateAndAddService<IReelController>(MockBehavior.Default);
            _target = new GetReelStateCommandHandler();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void InvalidReelState()
        {
            var maxvalue = Enum.GetValues(typeof(ReelLogicalState)).Cast<int>().Max();
            var states = new Dictionary<int, ReelLogicalState> { { 1, (ReelLogicalState)(maxvalue + 1) } };
            _reelController.Setup(x => x.ReelStates).Returns(states);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _target.Handle(new GetReelState()));
        }

        [DataRow(ReelLogicalState.Disconnected, ReelState.Disconnected)]
        [DataRow(ReelLogicalState.Homing, ReelState.SpinningForward, true)]
        [DataRow(ReelLogicalState.Spinning, ReelState.SpinningForward, true)]
        [DataRow(ReelLogicalState.Homing, ReelState.SpinningConstant, false)]
        [DataRow(ReelLogicalState.Spinning, ReelState.SpinningConstant, false)]
        [DataRow(ReelLogicalState.Stopping, ReelState.Stopping)]
        [DataRow(ReelLogicalState.Tilted, ReelState.Faulted)]
        [DataRow(ReelLogicalState.IdleUnknown, ReelState.Stopped)]
        [DataRow(ReelLogicalState.IdleAtStop, ReelState.Stopped)]
        [DataRow(ReelLogicalState.SpinningBackwards, ReelState.SpinningBackwards)]
        [DataRow(ReelLogicalState.SpinningForward, ReelState.SpinningForward)]
        [DataRow(ReelLogicalState.SpinningConstant, ReelState.SpinningConstant)]
        [DataRow(ReelLogicalState.Accelerating, ReelState.Accelerating)]
        [DataRow(ReelLogicalState.Decelerating, ReelState.Decelerating)]
        [DataTestMethod]
        public void HandleTest(ReelLogicalState logicalState, ReelState expectedState, bool hasSpinCapability = false)
        {
            const int reelCount = 6;
            const int reelStartIndex = 1;
            var states = Enumerable.Range(reelStartIndex, reelCount).ToDictionary(x => x, _ => logicalState);
            _reelController.Setup(x => x.ReelStates).Returns(states);
            _reelController.Setup(x => x.HasCapability<IReelSpinCapabilities>()).Returns(hasSpinCapability);

            var command = new GetReelState();
            _target.Handle(command);

            _reelController.Verify(r => r.ReelStates, Times.Once);
            Assert.AreEqual(states.Count, command.States.Count);
            foreach (var commandState in command.States.Select(x => x.Value))
            {
                Assert.AreEqual(expectedState, commandState);
            }
        }
    }
}