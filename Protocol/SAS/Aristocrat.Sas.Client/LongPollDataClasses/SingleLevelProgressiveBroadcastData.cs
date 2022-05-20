namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    /// <summary>
    ///     Hold the single value of GroupId, Level and LevelAmount to be configured.
    /// </summary>
    public class SingleLevelProgressiveBroadcastData : LongPollData
    {
        /// <summary>
        ///     Gets or sets the progressive identifier.
        /// </summary>
        public int ProgId { get; set; }

        /// <summary>
        ///     Gets or sets the level identifier.
        /// </summary>
        public int LevelId { get; set; }

        /// <summary>
        ///     Gets or sets the level amount.
        /// </summary>
        public long LevelAmount { get; set; }
    }
}