namespace Aristocrat.Monaco.Gaming.Tests.Commands
{
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
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
            var reelSpeedData = new ReelSpeedData[3];
            reelSpeedData[0] = new ReelSpeedData(1, 10);
            reelSpeedData[1] = new ReelSpeedData(2, 20);
            reelSpeedData[2] = new ReelSpeedData(3, 30);

            var command = new UpdateReelsSpeed(reelSpeedData);

            _reelController.Setup(r => r.SetReelSpeed(command.SpeedData)).Returns(Task.FromResult(true));

            var handler = Factory_CreateHandler();
            handler.Handle(command);

            _reelController.Verify(r => r.SetReelSpeed(command.SpeedData), Times.Once);
            Assert.AreEqual(command.Success, true);
        }

        private UpdateReelsSpeedCommandHandler Factory_CreateHandler()
        {
            return new UpdateReelsSpeedCommandHandler();
        }
    }
}