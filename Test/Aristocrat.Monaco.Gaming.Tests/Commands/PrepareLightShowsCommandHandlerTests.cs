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
    public class PrepareLightShowsCommandHandlerTests
    {
        private const string AnimationName = "TestAnimation";
        private const string Tag = "ALL";
        private readonly LightShowData[] _multipleLightShowData =
        {
            new(1, AnimationName, Tag,ReelConstants.RepeatForever, -1),
            new(2, AnimationName, Tag,ReelConstants.RepeatOnce, -1),
            new(3, AnimationName, Tag, 1, -1)
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

            var command = new PrepareLightShows(_multipleLightShowData);
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
            reelController.Setup(r => r.GetCapability<IReelAnimationCapabilities>().PrepareAnimations(It.IsAny<LightShowData[]>(), default)).Returns(Task.FromResult(capabilityResponse));
            
            var command = new PrepareLightShows(_multipleLightShowData);
            var handler = Factory_CreateHandler();
            handler.Handle(command);
            
            reelController.Verify(r => r.GetCapability<IReelAnimationCapabilities>().PrepareAnimations(command.LightShowData, default), Times.Once);
            Assert.AreEqual(command.Success, capabilityResponse);
        }

        private PrepareLightShowCommandHandler Factory_CreateHandler()
        {
            return new PrepareLightShowCommandHandler();
        }
    }
}
