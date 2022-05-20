namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    using System.Collections.Generic;

    /// <summary>
    ///     Hold the extended progressive multiple value of ProgressiveId, Level, ProgressiveAmount, BaseAmount, ContributionRate  to be configured.
    /// </summary>
    public class ExtendedProgressiveBroadcastData : LongPollData
    {
        /// <summary>
        ///     Gets or sets the progressive group id.
        /// </summary>
        public int ProgId { get; set; }

        /// <summary>
        ///     Gets or sets the Level data (level and broadcast ProgressiveLevelData).
        /// </summary>
        public IReadOnlyDictionary<int, ExtendedLevelData> LevelInfo { get; set; }
    }

    /// <summary>
    ///     Broadcast progressive level information filled in LP7AExtendedProgressiveBroadcastParser
    /// </summary>
    public class ExtendedLevelData
    {
        /// <summary>
        ///     Gets or sets the amount.
        /// </summary>
        /// <value>The amount.</value>
        public long Amount { get; set; }

        /// <summary>
        ///     Gets or sets the reset value.
        /// </summary>
        /// <value>The reset value.</value>
        public long ResetValue { get; set; }

        /// <summary>
        ///     Gets or sets the Contribution Rate
        /// </summary>
        public int ContributionRate { get; set; }
    }
}