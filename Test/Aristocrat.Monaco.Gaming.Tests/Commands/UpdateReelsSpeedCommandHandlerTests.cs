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
    ///     SpinReelsCommandHandler unit tests
    /// </summary>
    [TestClass]
    public class UpdateReelsSpeedCommandHandlerTests
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestInitialization()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void NullControllerIsHandledTest()
        {
            Factory_CreateHandler();
        }

        [TestMethod]
        public void HandleTest()
        {
            Mock<IReelController> reelController = MoqServiceManager.CreateAndAddService<IReelController>(MockBehavior.Default);
            reelController.Setup(x => x.HasCapability<IReelSpinCapabilities>()).Returns(true);

            var reelSpeedData = new ReelSpeedData[3];
            reelSpeedData[0] = new ReelSpeedData(1, 10);
            reelSpeedData[1] = new ReelSpeedData(2, 20);
            reelSpeedData[2] = new ReelSpeedData(3, 30);

            var command = new UpdateReelsSpeed(reelSpeedData);

            reelController.Setup(r => r.GetCapability<IReelSpinCapabilities>().SetReelSpeed(command.SpeedData)).Returns(Task.FromResult(true));

            var handler = Factory_CreateHandler();
            handler.Handle(command);

            reelController.Verify(r => r.GetCapability<IReelSpinCapabilities>().SetReelSpeed(command.SpeedData), Times.Once);
            Assert.AreEqual(command.Success, true);
        }

        private UpdateReelsSpeedCommandHandler Factory_CreateHandler()
        {
            return new UpdateReelsSpeedCommandHandler();
        }
    }
}