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
    public class SpinReelsCommandHandlerTests
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
            var reelSpinData = new ReelSpinData[3];
            reelSpinData[0] = new ReelSpinData(1, SpinDirection.Forward, 1, 50);
            reelSpinData[1] = new ReelSpinData(2, SpinDirection.Backwards, 2, 100);
            reelSpinData[2] = new ReelSpinData(3, SpinDirection.Forward, 3, 200);

            var command = new SpinReels(reelSpinData);

            _reelController.Setup(r => r.SpinReels(command.SpinData)).Returns(Task.FromResult(true));

            var handler = Factory_CreateHandler();
            handler.Handle(command);

            _reelController.Verify(r => r.SpinReels(command.SpinData), Times.Once);
            Assert.AreEqual(command.Success, true);
        }

        private SpinReelsCommandHandler Factory_CreateHandler()
        {
            return new SpinReelsCommandHandler();
        }
    }
}