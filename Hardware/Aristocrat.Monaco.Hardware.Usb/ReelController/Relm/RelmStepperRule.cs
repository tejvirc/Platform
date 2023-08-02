namespace Aristocrat.Monaco.Hardware.Usb.ReelController.Relm
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Communicator;
    using Contracts.Reel.ControlData;
    using Contracts.Reel.Events;
    using Contracts.Reel.ImplementationCapabilities;

    internal class RelmStepperRule : IStepperRuleImplementation
    {
        private readonly IRelmCommunicator _communicator;

        public RelmStepperRule(IRelmCommunicator communicator)
        {
            _communicator = communicator;
        }

#pragma warning disable 67
        public event EventHandler<StepperRuleTriggeredEventArgs> StepperRuleTriggered;
#pragma warning restore 67

        public Task<bool> PrepareStepperRule(StepperRuleData ruleData, CancellationToken token = default)
        {
            return _communicator.PrepareStepperRule(ruleData, token);
        }
    }
}
