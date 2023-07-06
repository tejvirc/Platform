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
    public class PrepareStepperCurvesCommandHandlerTests
    {
        private const string AnimationName = "TestAnimation";

        private readonly ReelCurveData[] _multipleCurveData =
        {
            new(1, AnimationName),
            new(2, AnimationName),
            new(3, AnimationName)
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

            var command = new PrepareStepperCurves(_multipleCurveData);
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
            reelController.Setup(r => r.GetCapability<IReelAnimationCapabilities>().PrepareAnimations(It.IsAny<ReelCurveData[]>(), default)).Returns(Task.FromResult(capabilityResponse));
            
            var command = new PrepareStepperCurves(_multipleCurveData);
            var handler = Factory_CreateHandler();
            handler.Handle(command);
            
            reelController.Verify(r => r.GetCapability<IReelAnimationCapabilities>().PrepareAnimations(command.ReelCurveData, default), Times.Once);
            Assert.AreEqual(command.Success, capabilityResponse);
        }

        private PrepareStepperCurvesCommandHandler Factory_CreateHandler()
        {
            return new PrepareStepperCurvesCommandHandler();
        }
    }
}
