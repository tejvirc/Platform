namespace Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked
{
    /// <summary>
    ///     This event is posted when a linked progressive level has been awarded
    /// </summary>
    public class LinkedProgressiveAwardedEvent : ProgressiveAwardedEvent
    {
        ///<inheritdoc />
        public LinkedProgressiveAwardedEvent(long transactionId, long awardAmount, string winText, PayMethod payMethod)
            : base(transactionId, awardAmount, winText, payMethod)
        {
        }
    }
}