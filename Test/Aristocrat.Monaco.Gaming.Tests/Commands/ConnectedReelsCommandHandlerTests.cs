namespace Aristocrat.Monaco.Gaming.Tests.Commands
{
    using System.Collections.Generic;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Gaming.Commands;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     ConnectedReelsCommandHandler unit tests
    /// </summary>
    [TestClass]
    public class ConnectedReelsCommandHandlerTests
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
            var command = new ConnectedReels();
            var reelIds = new List<int> { 1, 2, 3 };

            _reelController.Setup(r => r.ConnectedReels).Returns(reelIds);

            var handler = Factory_CreateHandler();
            handler.Handle(command);

            _reelController.Verify(r => r.ConnectedReels, Times.Once);
            Assert.AreEqual(command.ReelIds.Count, reelIds.Count);
            for (int i = 0; i < command.ReelIds.Count; ++i)
            {
                Assert.AreEqual(command.ReelIds[i], reelIds[i]);
            }
        }

        private ConnectedReelsCommandHandler Factory_CreateHandler()
        {
            return new ConnectedReelsCommandHandler();
        }
    }
}