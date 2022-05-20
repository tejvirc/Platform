namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    /// <inheritdoc />
    public class SendCumulativeProgressiveWinResponse : LongPollResponse
    {
        /// <summary>
        ///     Gets and sets the Meter Value.
        /// </summary>
        public ulong MeterValue { get; set; }
    }

    public class SendCumulativeProgressiveWinData : LongPollData
    {
        public int GameId { get; set; }

        public long AccountingDenom { get; set; }
    }
}