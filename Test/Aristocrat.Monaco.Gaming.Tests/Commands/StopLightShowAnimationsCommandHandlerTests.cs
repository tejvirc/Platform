namespace Aristocrat.Monaco.Gaming.Tests.Commands
{
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Gaming.Commands;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Aristocrat.Monaco.Hardware.Contracts.Reel.Capabilities;
    using Aristocrat.Monaco.Hardware.Contracts.Reel.ControlData;
    using Aristocrat.Monaco.Test.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class StopLightShowAnimationsCommandHandlerTests
    {
        private readonly LightShowData[] _lightShowData = new[] {
            new LightShowData("Animation1", "Tag1"),
            new LightShowData("Animation2", "Tag2")
        };

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

            var command = new StopLightShowAnimations(_lightShowData);
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
            reelController.Setup(r => r.GetCapability<IReelAnimationCapabilities>().StopLightShowAnimations(It.IsAny<LightShowData[]>(), default)).Returns(Task.FromResult(capabilityResponse));

            var handler = Factory_CreateHandler();
            var command = new StopLightShowAnimations(_lightShowData);
            handler.Handle(command);

            reelController.Verify(r => r.GetCapability<IReelAnimationCapabilities>().StopLightShowAnimations(_lightShowData, default), Times.Once);
            Assert.AreEqual(command.Success, capabilityResponse);
        }

        private StopLightShowAnimationsCommandHandler Factory_CreateHandler()
        {
            return new StopLightShowAnimationsCommandHandler();
        }
    }
}
