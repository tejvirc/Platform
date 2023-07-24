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
    public class SetBrightnessCommandHandlerTests
    {
        private readonly uint validBrightness = 75;
        private readonly uint invalidBrightness = 175;

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
            reelController.Setup(x => x.HasCapability<IReelBrightnessCapabilities>()).Returns(false);

            var command = new SetBrightness(validBrightness);
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
            reelController.Setup(x => x.HasCapability<IReelBrightnessCapabilities>()).Returns(true);
            reelController.Setup(r => r.GetCapability<IReelBrightnessCapabilities>().DefaultReelBrightness).Returns(100);
            reelController.Setup(r => r.GetCapability<IReelBrightnessCapabilities>().SetBrightness(It.IsAny<int>())).Returns(Task.FromResult(capabilityResponse));
            
            var command = new SetBrightness(validBrightness);
            var handler = Factory_CreateHandler();
            handler.Handle(command);
            
            reelController.Verify(r => r.GetCapability<IReelBrightnessCapabilities>().SetBrightness((int)validBrightness), Times.Once);
            Assert.AreEqual(command.Success, capabilityResponse);
        }

        [DataTestMethod]
        public void HandleTestWithInvalidBrightness()
        {
            var reelController = MoqServiceManager.CreateAndAddService<IReelController>(MockBehavior.Default);
            reelController.Setup(x => x.HasCapability<IReelBrightnessCapabilities>()).Returns(true);
            reelController.Setup(r => r.GetCapability<IReelBrightnessCapabilities>().DefaultReelBrightness).Returns(100);
            reelController.Setup(r => r.GetCapability<IReelBrightnessCapabilities>().SetBrightness((int)invalidBrightness)).Returns(Task.FromResult(false));
            
            var command = new SetBrightness(invalidBrightness);
            var handler = Factory_CreateHandler();
            handler.Handle(command);
            
            reelController.Verify(r => r.GetCapability<IReelBrightnessCapabilities>().SetBrightness((int)invalidBrightness), Times.Once);
            Assert.AreEqual(command.Success, false);
        }

        private SetBrightnessCommandHandler Factory_CreateHandler()
        {
            return new SetBrightnessCommandHandler();
        }
    }
}
