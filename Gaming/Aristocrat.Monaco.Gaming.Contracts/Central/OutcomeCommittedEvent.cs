namespace Aristocrat.Monaco.Gaming.Contracts.Central
{
    /// <summary>
    ///     The <see cref="OutcomeCommittedEvent" /> is posted when a central outcome is requested
    /// </summary>
    public class OutcomeCommittedEvent : BaseOutcomeEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OutcomeCommittedEvent" /> class.
        /// </summary>
        /// <param name="transaction">The central transaction</param>
        public OutcomeCommittedEvent(CentralTransaction transaction)
            : base(transaction)
        {
        }
    }
}