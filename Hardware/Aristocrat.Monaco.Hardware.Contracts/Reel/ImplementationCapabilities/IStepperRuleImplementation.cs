namespace Aristocrat.Monaco.Hardware.Contracts.Reel.ImplementationCapabilities
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using ControlData;
    using Events;

    /// <summary>
    ///     The reel controller stepper rule capability of an implementation
    /// </summary>
    public interface IStepperRuleImplementation : IReelImplementationCapability
    {
        /// <summary>
        ///     The event that occurs when the reel controller triggers a stepper rule (user event)
        /// </summary>
        event EventHandler<StepperRuleTriggeredEventArgs> StepperRuleTriggered;

        /// <summary>
        ///     Prepares a stepper rule
        /// </summary>
        /// <param name="ruleData">The stepper rule data.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>True if the call succeeds, otherwise false.</returns>
        Task<bool> PrepareStepperRule(StepperRuleData ruleData, CancellationToken token = default);
    }
}