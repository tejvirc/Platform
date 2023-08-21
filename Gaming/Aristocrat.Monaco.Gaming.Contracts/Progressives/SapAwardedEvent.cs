namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    /// <summary>
    ///     Posted when a standalone progressive level has been awarded
    /// </summary>
    public class SapAwardedEvent : ProgressiveAwardedEvent
    {
        /// <inheritdoc />
        public SapAwardedEvent(long transactionId, long awardedAmount, string winText, PayMethod payMethod)
        : base(transactionId, awardedAmount, winText, payMethod)
        {
        }
    }
}