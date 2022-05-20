namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    /// <summary>
    ///     When a progressive level is triggered, the result of the trigger will be returned.
    /// </summary>
    public class ProgressiveTriggerResult
    {
        /// <summary>
        ///     Gets or sets the game id of the trigger result
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        ///     Gets or sets the denom of the trigger result
        /// </summary>
        public long Denom { get; set; }

        /// <summary>
        ///     Gets or sets the transaction id of the trigger result
        /// </summary>
        public long TransactionId { get; set; }

        /// <summary>
        ///     Gets or sets the progressive pack name for the trigger result
        /// </summary>
        public string ProgressivePackName { get; set; }

        /// <summary>
        ///     Gets or sets the level id for the trigger result
        /// </summary>
        public int LevelId { get; set; }
    }
}