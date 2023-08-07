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
    public class PrepareStepperRuleCommandHandlerTests
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
            reelController.Setup(x => x.HasCapability<IStepperRuleCapabilities>()).Returns(false);

            var command = new PrepareStepperRule(new StepperRuleData());
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
            reelController.Setup(x => x.HasCapability<IStepperRuleCapabilities>()).Returns(true);
            reelController.Setup(r => r.GetCapability<IStepperRuleCapabilities>()
                .PrepareStepperRule(It.IsAny<StepperRuleData>(), default))
                .Returns(Task.FromResult(capabilityResponse));

            var stepperRuleData = new StepperRuleData();
            
            var command = new PrepareStepperRule(stepperRuleData);
            var handler = Factory_CreateHandler();
            handler.Handle(command);
            
            reelController.Verify(r => r.GetCapability<IStepperRuleCapabilities>().PrepareStepperRule(stepperRuleData, default), Times.Once);
            Assert.AreEqual(command.Success, capabilityResponse);
        }

        private PrepareStepperRuleCommandHandler Factory_CreateHandler()
        {
            return new PrepareStepperRuleCommandHandler();
        }
    }
}
