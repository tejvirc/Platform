namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Capabilities
{
    using System.Threading;
    using System.Threading.Tasks;
    using ControlData;

    /// <summary>
    ///     The public interface for reel controller stepper rule capabilities
    /// </summary>
    public interface IStepperRuleCapabilities : IReelControllerCapability
    {
        /// <summary>
        ///     Prepares a stepper rule
        /// </summary>
        /// <param name="ruleData">The stepper rule data.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>True if the call succeeds, otherwise false.</returns>
        Task<bool> PrepareStepperRule(StepperRuleData ruleData, CancellationToken token = default);
    }
}
