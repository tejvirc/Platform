namespace Aristocrat.Monaco.Gaming.Contracts.Central
{
    /// <summary>
    ///     The <see cref="OutcomeRequestedEvent" /> is posted when a central outcome is requested
    /// </summary>
    public class OutcomeRequestedEvent : BaseOutcomeEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OutcomeRequestedEvent" /> class.
        /// </summary>
        /// <param name="transaction">The central transaction</param>
        public OutcomeRequestedEvent(CentralTransaction transaction)
            : base(transaction)
        {
        }
    }
}