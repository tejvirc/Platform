namespace Aristocrat.Monaco.Hardware.Tests.Reel.Capabilities
{
    using Contracts.Reel.Events;
    using Contracts.Reel.ImplementationCapabilities;
    using Hardware.Reel.Capabilities;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class StepperRuleCapabilityTests
    {
        private Mock<IEventBus> _eventBus;
        private readonly Mock<IStepperRuleImplementation> _implementation = new();

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            
            _ = new StepperRuleCapability(_implementation.Object, _eventBus.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void HandleStepperRuleTriggeredTest()
        {
            const int reelId = 3;
            const int eventId = 100;

            var actualArgs = new StepperRuleTriggeredEventArgs(reelId, eventId);

            _implementation.Raise(x => x.StepperRuleTriggered += null, actualArgs);

            _eventBus.Verify(x => x.Publish(It.Is<StepperRuleTriggeredEvent>(e => e.ReelId == reelId && e.EventId == eventId)), Times.Once);
        }
    }
}
