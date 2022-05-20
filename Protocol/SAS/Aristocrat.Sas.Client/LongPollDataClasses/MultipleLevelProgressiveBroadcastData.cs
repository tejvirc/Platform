namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    using System.Collections.Generic;

    /// <summary>
    ///     Hold the Multiple value of GroupId, Level and LevelAmount to be configured.
    /// </summary>
    public class MultipleLevelProgressiveBroadcastData : LongPollData
    {
        /// <summary>
        ///     Gets or sets the GroupId(ProgressiveId).
        /// </summary>
        public int ProgId { get; set; }

        /// <summary>
        ///     Gets or Sets the Level Details (level and level Amount).
        /// </summary>
        public IReadOnlyDictionary<int, long> LevelInfo { get; set; }
    }
}