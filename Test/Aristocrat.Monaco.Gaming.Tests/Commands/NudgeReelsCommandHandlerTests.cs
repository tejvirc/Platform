namespace Aristocrat.Monaco.Gaming.Tests.Commands
{
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Aristocrat.Monaco.Hardware.Contracts.Reel.Capabilities;
    using Aristocrat.Monaco.Hardware.Contracts.Reel.ControlData;
    using Gaming.Commands;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     NudgeReelsCommandHandler unit tests
    /// </summary>
    [TestClass]
    public class NudgeReelsCommandHandlerTests
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
            _reelController.Setup(x => x.HasCapability<IReelSpinCapabilities>()).Returns(true);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void HandleTest()
        {
            var nudgeSpinData = new NudgeReelData[3];
            nudgeSpinData[0] = new NudgeReelData(1, SpinDirection.Forward, 50, 1, 10);
            nudgeSpinData[1] = new NudgeReelData(2, SpinDirection.Backwards, 100, 2, 20);
            nudgeSpinData[2] = new NudgeReelData(3, SpinDirection.Forward, 200, 3, 30);

            var command = new NudgeReels(nudgeSpinData);

            _reelController.Setup(r => r.GetCapability<IReelSpinCapabilities>().NudgeReels(command.NudgeSpinData)).Returns(Task.FromResult(true));

            var handler = Factory_CreateHandler();
            handler.Handle(command);

            _reelController.Verify(r => r.GetCapability<IReelSpinCapabilities>().NudgeReels(command.NudgeSpinData), Times.Once);
            Assert.AreEqual(command.Success, true);
        }

        private NudgeReelsCommandHandler Factory_CreateHandler()
        {
            return new NudgeReelsCommandHandler();
        }
    }
}