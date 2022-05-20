namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    using Kernel;

    /// <summary>
    ///     Base event structure for when a progressive level is awarded. Any implementor of this
    ///     event should post this event only when the award has been confirmed. The IProgressiveGameProvider
    ///     will handle this event and attempt to complete the associated transaction.
    /// </summary>
    public abstract class ProgressiveAwardedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes the ProgressivesAwardedEvent object via a derived class.
        /// </summary>
        /// <param name="transactionId">The id of the associated transaction</param>
        /// <param name="awardedAmount">The amount awarded with the transaction</param>
        /// <param name="winText">The win text</param>
        /// <param name="payMethod">The method of payment</param>
        protected ProgressiveAwardedEvent(
            long transactionId, long awardedAmount, string winText, PayMethod payMethod)
        {
            AwardedAmount = awardedAmount;
            TransactionId = transactionId;
            WinText = winText;
            PayMethod = payMethod;
        }

        /// <summary>
        ///     Gets the associated transaction id.
        /// </summary>
        public long TransactionId { get; }

        /// <summary>
        ///     Gets the amount awarded.
        /// </summary>
        public long AwardedAmount { get; }

        /// <summary>
        ///     Gets the win text
        /// </summary>
        public string WinText { get; }

        /// <summary>
        ///     Gets the payment method
        /// </summary>
        public PayMethod PayMethod { get; }
    }
}