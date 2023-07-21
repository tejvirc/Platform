namespace Aristocrat.Monaco.Gaming.Tests.Commands
{
    using System.Threading.Tasks;
    using Gaming.Commands;
    using Hardware.Contracts.Reel;
    using Hardware.Contracts.Reel.Capabilities;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class StartAnimationsCommandHandlerTests
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
        public void HandleTestForNoCapabilitiesShouldFail()
        {
            var reelController = MoqServiceManager.CreateAndAddService<IReelController>(MockBehavior.Default);
            reelController.Setup(x => x.HasCapability<IReelAnimationCapabilities>()).Returns(false);

            var command = new StartAnimations();
            var handler = Factory_CreateHandler();
            handler.Handle(command);

            Assert.AreEqual(command.Success, false);
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void HandleTest(bool capabilityResponse)
        {
            var reelController = MoqServiceManager.CreateAndAddService<IReelController>(MockBehavior.Default);
            reelController.Setup(x => x.HasCapability<IReelAnimationCapabilities>()).Returns(true);
            reelController.Setup(r => r.GetCapability<IReelAnimationCapabilities>().PlayAnimations(default)).Returns(Task.FromResult(capabilityResponse));
            
            var command = new StartAnimations();
            var handler = Factory_CreateHandler();
            handler.Handle(command);
            
            reelController.Verify(r => r.GetCapability<IReelAnimationCapabilities>().PlayAnimations(default), Times.Once);
            Assert.AreEqual(command.Success, capabilityResponse);
        }

        private StartAnimationsCommandHandler Factory_CreateHandler()
        {
            return new StartAnimationsCommandHandler();
        }
    }
}
