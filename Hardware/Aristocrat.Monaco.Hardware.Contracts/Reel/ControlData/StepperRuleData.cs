namespace Aristocrat.Monaco.Hardware.Contracts.Reel.ControlData
{
    /// <summary>
    ///     The stepper rule data class.
    /// </summary>
    public class StepperRuleData
    {
        /// <summary>
        ///     Get or sets the reel index.
        /// </summary>
        public byte ReelIndex { get; set; }
        
        /// <summary>
        ///     Get or sets the rule type.
        /// </summary>
        public StepperRuleType RuleType { get; set; }
        
        /// <summary>
        ///     Get or sets the step to follow.
        /// </summary>
        public byte StepToFollow { get; set; }
        
        /// <summary>
        ///     Get or sets the reference step.
        /// </summary>
        public byte ReferenceStep { get; set; }
        
        /// <summary>
        ///     Get or sets the cycle.
        /// </summary>
        public short Cycle { get; set; }
        
        /// <summary>
        ///     Get or sets the delta.
        /// </summary>
        public byte Delta { get; set; }
        
        /// <summary>
        ///     Get or sets the event id.
        /// </summary>
        public int EventId { get; set; }
    }
}
