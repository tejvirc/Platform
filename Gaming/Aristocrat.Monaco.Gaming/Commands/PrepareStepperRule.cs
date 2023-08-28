namespace Aristocrat.Monaco.Gaming.Commands
{
    using Hardware.Contracts.Reel.ControlData;

    /// <summary>
    ///     The <see cref="PrepareStepperRule"/> class.
    /// </summary>
    public class PrepareStepperRule
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PrepareStepperRule" /> class.
        /// </summary>
        /// <param name="stepperRuleData">The stepper rule data</param>
        public PrepareStepperRule(StepperRuleData stepperRuleData)
        {
            StepperRuleData = stepperRuleData;
        }

        /// <summary>
        ///     Gets the stepper rule data
        /// </summary>
        public StepperRuleData StepperRuleData { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not the stepper curves were prepared
        /// </summary>
        public bool Success { get; set; }
    }
}
