namespace Aristocrat.Monaco.Gaming.Tests.Commands
{
    using System.Collections.Generic;
    using Aristocrat.GdkRuntime.V1;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Gaming.Commands;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     GetReelStateCommandHandler unit tests
    /// </summary>
    [TestClass]
    public class GetReelStateCommandHandlerTests
    {
        private Mock<IReelController> _reelController;

        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestInitialization()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _reelController = MoqServiceManager.CreateAndAddService<IReelController>(MockBehavior.Default);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void HandleTest()
        {
            var command = new GetReelState();
            IReadOnlyDictionary<int, ReelLogicalState> states = new Dictionary<int, ReelLogicalState>()
            {
                { 1, ReelLogicalState.Disconnected },
                { 2, ReelLogicalState.IdleUnknown },
                { 3, ReelLogicalState.IdleAtStop },
                { 4, ReelLogicalState.Spinning },
                { 5, ReelLogicalState.Stopping },
                { 6, ReelLogicalState.Tilted }
            };

            _reelController.Setup(r => r.ReelStates).Returns(states);

            var handler = Factory_CreateHandler();
            handler.Handle(command);

            _reelController.Verify(r => r.ReelStates, Times.Once);
            Assert.AreEqual(command.States.Count, states.Count);
            Assert.AreEqual(command.States[1], ReelState.Disconnected);
            Assert.AreEqual(command.States[2], ReelState.Stopped);
            Assert.AreEqual(command.States[3], ReelState.Stopped);
            Assert.AreEqual(command.States[4], ReelState.SpinningForward);
            Assert.AreEqual(command.States[5], ReelState.Stopping);
            Assert.AreEqual(command.States[6], ReelState.Faulted);
        }

        private GetReelStateCommandHandler Factory_CreateHandler()
        {
            return new GetReelStateCommandHandler();
        }
    }
}