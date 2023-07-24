namespace Aristocrat.Monaco.Gaming.Tests.Commands
{
    using System.Threading.Tasks;
    using Gaming.Commands;
    using Hardware.Contracts.Reel;
    using Hardware.Contracts.Reel.Capabilities;
    using Hardware.Contracts.Reel.ControlData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class PrepareStopReelsCommandHandlerTests
    {
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
            var reelController = MoqServiceManager.CreateAndAddService<IReelController>(MockBehavior.Default);
            reelController.Setup(x => x.HasCapability<IReelAnimationCapabilities>()).Returns(true);

            var reelStopData = new ReelStopData[3];
            reelStopData[0] = new ReelStopData(1, 100, 50);
            reelStopData[1] = new ReelStopData(2, 100, 100);
            reelStopData[2] = new ReelStopData(3, 100, 200);

            var command = new PrepareStopReels(reelStopData);

            reelController.Setup(r => r.GetCapability<IReelAnimationCapabilities>().PrepareStopReels(command.ReelStopData, default)).Returns(Task.FromResult(true));

            var handler = Factory_CreateHandler();
            handler.Handle(command);

            reelController.Verify(r => r.GetCapability<IReelAnimationCapabilities>().PrepareStopReels(command.ReelStopData, default), Times.Once);
            Assert.AreEqual(command.Success, true);
        }

        private PrepareStopReelsCommandHandler Factory_CreateHandler()
        {
            return new PrepareStopReelsCommandHandler();
        }
    }
}
