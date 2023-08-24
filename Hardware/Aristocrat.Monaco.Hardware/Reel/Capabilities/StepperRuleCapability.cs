namespace Aristocrat.Monaco.Hardware.Reel.Capabilities
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Reel.Capabilities;
    using Contracts.Reel.ControlData;
    using Contracts.Reel.Events;
    using Contracts.Reel.ImplementationCapabilities;
    using Kernel;

    internal sealed class StepperRuleCapability : IStepperRuleCapabilities
    {
        private readonly IEventBus _eventBus;
        private readonly IStepperRuleImplementation _implementation;

        public StepperRuleCapability(IStepperRuleImplementation implementation, IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _implementation = implementation ?? throw new ArgumentNullException(nameof(implementation));
            
            _implementation.StepperRuleTriggered += HandleStepperRuleTriggered;
        }

        public void Dispose()
        {
            _implementation.StepperRuleTriggered -= HandleStepperRuleTriggered;
        }

        public Task<bool> PrepareStepperRule(StepperRuleData ruleData, CancellationToken token = default)
        {
            return _implementation.PrepareStepperRule(ruleData, token);
        }

        private void HandleStepperRuleTriggered(object sender, StepperRuleTriggeredEventArgs args)
        {
            _eventBus.Publish(new StepperRuleTriggeredEvent(args.ReelId, args.EventId));
        }
    }
}
