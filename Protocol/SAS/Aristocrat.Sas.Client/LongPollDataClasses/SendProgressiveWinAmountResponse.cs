namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    /// <summary>
    ///     Holds the data for progressive win
    /// </summary>
    public class SendProgressiveWinAmountResponse : LongPollResponse
    {
        /// <summary>
        ///     Gets or sets the win amount
        /// </summary>
        public long WinAmount { get; set; }

        /// <summary>
        ///     Gets or sets the GroupId of the progressive
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        ///     Gets or sets the progressive level
        /// </summary>
        public int LevelId { get; set; }
    }
}