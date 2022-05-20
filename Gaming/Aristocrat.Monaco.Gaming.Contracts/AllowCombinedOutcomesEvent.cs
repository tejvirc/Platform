namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;

    /// <summary>
    ///     The AllowCombinedOutcomesEvent is posted when the platform updates the AllowCombinedOutcomes RuntimeCondition.
    /// </summary>
    public class AllowCombinedOutcomesEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AllowCombinedOutcomesEvent" /> class.
        /// </summary>
        /// <param name="allowCombineOutcomes">Allow Combined Outcomes.</param>
        public AllowCombinedOutcomesEvent(bool allowCombineOutcomes)
        {
            AllowCombineOutcomes = allowCombineOutcomes;
        }

        /// <summary>
        ///     Gets the AllowCombineOutcome parameter
        /// </summary>
        public bool AllowCombineOutcomes { get; }
    }
}
