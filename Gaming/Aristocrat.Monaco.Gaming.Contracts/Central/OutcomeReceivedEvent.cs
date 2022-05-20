namespace Aristocrat.Monaco.Gaming.Contracts.Central
{
    /// <summary>
    ///     The <see cref="OutcomeReceivedEvent" /> is posted when a central outcome is requested
    /// </summary>
    public class OutcomeReceivedEvent : BaseOutcomeEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OutcomeReceivedEvent" /> class.
        /// </summary>
        /// <param name="transaction">The central transaction</param>
        public OutcomeReceivedEvent(CentralTransaction transaction)
            : base(transaction)
        {
        }
    }
}