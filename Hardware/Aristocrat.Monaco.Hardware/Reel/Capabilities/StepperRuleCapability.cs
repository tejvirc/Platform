namespace Aristocrat.Monaco.Hardware.Reel.Capabilities
{
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Reel.Capabilities;
    using Contracts.Reel.ControlData;
    using Contracts.Reel.ImplementationCapabilities;

    internal class StepperRuleCapability : IStepperRuleCapabilities
    {
        private readonly IStepperRuleImplementation _implementation;
        private readonly ReelControllerStateManager _stateManager;

        public StepperRuleCapability(IStepperRuleImplementation implementation, ReelControllerStateManager stateManager)
        {
            _implementation = implementation;
            _stateManager = stateManager;
        }

        public Task<bool> PrepareStepperRule(StepperRuleData ruleData, CancellationToken token = default)
        {
            return _implementation.PrepareStepperRule(ruleData, token);
        }
    }
}
