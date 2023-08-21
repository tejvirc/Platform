namespace Aristocrat.Monaco.Gaming.Contracts.Central
{
    /// <summary>
    ///     The <see cref="OutcomeFailedEvent" /> is posted when a central outcome is requested
    /// </summary>
    public class OutcomeFailedEvent : BaseOutcomeEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OutcomeFailedEvent" /> class.
        /// </summary>
        /// <param name="transaction">The central transaction</param>
        public OutcomeFailedEvent(CentralTransaction transaction)
            : base(transaction)
        {
        }
    }
}