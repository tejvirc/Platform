namespace Aristocrat.Monaco.Hardware.Usb.ReelController.Relm
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Communicator;
    using Contracts.Reel.ControlData;
    using Contracts.Reel.Events;
    using Contracts.Reel.ImplementationCapabilities;

    internal sealed class RelmStepperRule : IStepperRuleImplementation
    {
        private readonly IRelmCommunicator _communicator;

        public RelmStepperRule(IRelmCommunicator communicator)
        {
            _communicator = communicator;
            _communicator.StepperRuleTriggered += HandleStepperRuleTriggered;
        }

        public event EventHandler<StepperRuleTriggeredEventArgs> StepperRuleTriggered;

        public void Dispose()
        {
            _communicator.StepperRuleTriggered -= HandleStepperRuleTriggered;
        }

        public Task<bool> PrepareStepperRule(StepperRuleData ruleData, CancellationToken token = default)
        {
            return _communicator.PrepareStepperRule(ruleData, token);
        }

        private void HandleStepperRuleTriggered(object sender, StepperRuleTriggeredEventArgs args)
        {
            StepperRuleTriggered?.Invoke(sender, args);
        }
    }
}
