namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    /// <summary>
    ///     Progressive Meters
    /// </summary>
    public static class ProgressiveMeters
    {
        /// <summary>
        ///     Bulk amount to contribute to respective progressive level
        /// </summary>
        public const string ProgressiveLevelBulkContribution = "ProgressiveLevel.BulkContribution";

        /// <summary>
        ///     Progressive Win Occurrence
        /// </summary>
        public const string ProgressiveLevelWinOccurrence = "ProgressiveLevel.WinOccurrence";

        /// <summary>
        ///     Progressive win accumulation (total accumulated value of hits for each level)
        /// </summary>
        public const string ProgressiveLevelWinAccumulation = "ProgressiveLevel.WinAccumulation";

        /// <summary>
        ///     Progressive hidden total (total accumulated value of hits for each level)
        /// </summary>
        public const string ProgressiveLevelHiddenTotal = "ProgressiveLevel.HiddenTotal";

        /// <summary>
        ///     Progressive bulk total (total accumulated value of hits for each level)
        /// </summary>
        public const string ProgressiveLevelBulkTotal = "ProgressiveLevel.BulkTotal";

        /// <summary>
        ///     Shared Level Win Occurrence
        /// </summary>
        public const string SharedLevelWinOccurrence = "SharedLevel.WinOccurrence";

        /// <summary>
        ///     Shared Level win accumulation (total accumulated value of hits for each level)
        /// </summary>
        public const string SharedLevelWinAccumulation = "SharedLevel.WinAccumulation";

        /// <summary>
        ///     The total accumulated bulk contribution amount to the linked level
        /// </summary>
        public const string LinkedProgressiveBulkContribution = "LinkedProgressive.BulkContribution";

        /// <summary>
        ///     linked Progressive Win Occurrence
        /// </summary>
        public const string LinkedProgressiveWinOccurrence = "LinkedProgressive.WinOccurrence";

        /// <summary>
        ///     linked Progressive Win Accumulation
        /// </summary>
        public const string LinkedProgressiveWinAccumulation = "LinkedProgressive.WinAccumulation";

        /// <summary>
        ///     The wagered amount per linked progressive level
        /// </summary>
        public const string LinkedProgressiveWageredAmount = "LinkedProgressive.WageredAmount";

        /// <summary>
        ///     The wagered amount per linked progressive level when using an ante bet
        /// </summary>
        public const string LinkedProgressiveWageredAmountWithAnte = "LinkedProgressive.WageredAmountWithAnte";

        /// <summary>
        ///     The total accumulated plays per linked progressive level
        /// </summary>
        public const string LinkedProgressivePlayedCount = "LinkedProgressive.PlayedCount";

        /// <summary>
        ///   	Pack name of what the progressive level belongs to. Used to identify DisplayMeter node.
        /// </summary>
        public const string ProgressivePackNameDisplayMeter = "ProgressivePackName";

        /// <summary>
        ///   	Name of a progressive level (eg. Major, Minor). Used to identify DisplayMeter node.
        /// </summary>
        public const string LevelNameDisplayMeter = "LevelName";

        /// <summary>
        ///   	Current value of a progressive level. Used to identify DisplayMeter node.
        /// </summary>
        public const string CurrentValueDisplayMeter = "CurrentValue";

        /// <summary>
        ///   	Overflow value of a progressive level. Used to identify DisplayMeter node.
        /// </summary>
        public const string OverflowDisplayMeter = "Overflow";

        /// <summary>
        ///     DisplayMeter key for accumulated overflow values over the life of the machine.
        ///     This will not be reset by a progressive win.
        /// </summary>
        public const string OverflowTotalDisplayMeter = "OverflowTotal";

        /// <summary>
        ///   	Startup value of a progressive level. Used to identify DisplayMeter node. Also known as ResetValue in ProgressiveLevel class.
        /// </summary>
        public const string StartupDisplayMeter = "Startup";

        /// <summary>
        ///   	Ceiling value of a progressive level. Used to identify DisplayMeter node.
        /// </summary>
        public const string CeilingDisplayMeter = "Ceiling";

        /// <summary>
        ///   	Increment rate value of a progressive level. Used to identify DisplayMeter node.
        /// </summary>
        public const string IncrementDisplayMeter = "Increment";

        /// <summary>
        ///   	Hidden increment rate value of a progressive level. Used to identify DisplayMeter node.
        /// </summary>
        public const string HiddenIncrementDisplayMeter = @"HiddenIncrement";

        /// <summary>
        ///     DisplayMeter key for the total value of the hidden pool which will be added after JP hit and reset
        /// </summary>
        public const string HiddenValueDisplayMeter = "HiddenValue";
        
        /// <summary>
        ///   	Initial value of a progressive level. Used to identify DisplayMeter node.
        /// </summary>
        public const string InitialValueDisplayMeter = @"InitialValue";

        /// <summary>
        ///   	Wager bet levels for a progressive level. Used to identify DisplayMeter node.
        /// </summary>
        public const string WagerBetLevelsDisplayMeter = @"WagerBetLevels";

        /// <summary>
        ///     The wagered amount per level
        /// </summary>
        public const string WageredAmount = "WageredAmount";

        /// <summary>
        ///     The total accumulated plays per level
        /// </summary>
        public const string PlayedCount = "PlayedCount";
    }
}