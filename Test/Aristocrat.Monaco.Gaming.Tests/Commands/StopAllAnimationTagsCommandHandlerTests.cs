namespace Aristocrat.Monaco.Gaming.Tests.Commands
{
    using Gaming.Commands;
    using Hardware.Contracts.Reel;
    using Hardware.Contracts.Reel.Capabilities;
    using Hardware.Contracts.Reel.ControlData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System.Threading.Tasks;
    using Test.Common;

    [TestClass]
    public class StopAllAnimationTagsCommandHandlerTests
    {
        private readonly AnimationFile _animationFile =
            new AnimationFile(string.Empty, AnimationType.GameStepperCurve, "Test1") { AnimationId = 1 };

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

            var command = new StopAllAnimationTags(_animationFile.FriendlyName);
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
            reelController.Setup(r => r.GetCapability<IReelAnimationCapabilities>().StopAllAnimationTags(It.IsAny<string>(), default)).Returns(Task.FromResult(capabilityResponse));

            var handler = Factory_CreateHandler();
            var command = new StopAllAnimationTags(_animationFile.FriendlyName);
            handler.Handle(command);

            reelController.Verify(r => r.GetCapability<IReelAnimationCapabilities>().StopAllAnimationTags(_animationFile.FriendlyName, default), Times.Once);
            Assert.AreEqual(command.Success, capabilityResponse);
        }

        private StopAllAnimationTagsCommandHandler Factory_CreateHandler()
        {
            return new StopAllAnimationTagsCommandHandler();
        }
    }
}
