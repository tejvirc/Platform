namespace Aristocrat.Monaco.Gaming.Contracts.Progressives.SharedSap
{
    /// <summary>
    ///     Posted when a hit shared sap level is awarded
    /// </summary>
    public class SharedSapAwardedEvent : ProgressiveAwardedEvent
    {
        /// <inheritdoc />
        public SharedSapAwardedEvent(long transactionId, long awardAmount, string winText, PayMethod payMethod)
            : base(transactionId, awardAmount, winText, payMethod)
        {
        }
    }
}