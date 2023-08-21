namespace Aristocrat.Monaco.Gaming.UI.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts.Progressives;

    /// <summary>
    ///     Progressive level settings.
    /// </summary>
    internal class ProgressiveLevelSettings
    {
        /// <summary>
        ///     Creates an instance of <see cref="ProgressiveLevelSettings"/>.
        /// </summary>
        /// <param name="paytableId">The paytable id this progressive setting is tied to.</param>
        /// <param name="themeId">The theme id this progressive setting is tied to.</param>
        /// <param name="level">The level to pass in.</param>
        public ProgressiveLevelSettings(string paytableId, string themeId, IViewableProgressiveLevel level)
            : this(level)
        {
            PaytableId = paytableId;
            ThemeId = themeId;
        }

        /// <summary>
        ///     Creates an instance of <see cref="ProgressiveLevelSettings"/>.
        /// </summary>
        /// <param name="level">The level to pass in.</param>
        public ProgressiveLevelSettings(IViewableProgressiveLevel level)
        {
            if (level is null)
            {
                throw new ArgumentNullException(nameof(level));
            }

            // TODO: We can trim this down further for our saved properties.
            ProgressivePackName = level.ProgressivePackName;
            ProgressivePackId = level.ProgressivePackId;
            ProgressiveId = level.ProgressiveId;
            Denomination = level.Denomination?.ToArray() ?? Enumerable.Empty<long>();
            BetOption = level.BetOption;
            Variation = level.Variation;
            ProgressivePackRtp = level.ProgressivePackRtp;
            LevelType = level.LevelType;
            FundingType = level.FundingType;
            LevelName = level.LevelName;
            IncrementRate = level.IncrementRate;
            HiddenIncrementRate = level.HiddenIncrementRate;
            Probability = level.Probability;
            MaximumValue = level.MaximumValue;
            ResetValue = level.ResetValue;
            LevelRtp = level.LevelRtp;
            LineGroup = level.LineGroup;
            AllowTruncation = level.AllowTruncation;
            Turnover = level.Turnover;
            TriggerControl = level.TriggerControl;
            AssignedProgressiveId = new AssignableProgressiveId(
                level.AssignedProgressiveId?.AssignedProgressiveType ?? AssignableProgressiveType.None,
                level.AssignedProgressiveId?.AssignedProgressiveKey ?? string.Empty);
        }

        public ProgressiveLevelSettings()
        {
        }

        /// <summary>
        ///     Gets or sets the paytable id.
        /// </summary>
        public string PaytableId { get; set; }

        /// <summary>
        ///     Gets or sets the theme id.
        /// </summary>
        public string ThemeId { get; set; }

        /// <summary>
        ///     Gets or sets the progressive pack name.
        /// </summary>
        public string ProgressivePackName { get; set; }

        /// <summary>
        ///     Gets or sets progressive pack id.
        /// </summary>
        public int ProgressivePackId { get; set; }

        /// <summary>
        ///     Gets or sets the progressive id.
        /// </summary>
        public int ProgressiveId { get; set; }

        /// <summary>
        ///     Gets or sets the denominations.
        /// </summary>
        public IEnumerable<long> Denomination { get; set; }

        /// <summary>
        ///     Gets or sets the bet option.
        /// </summary>
        public string BetOption { get; set; }

        /// <summary>
        ///     Gets or sets the variation.
        /// </summary>
        public string Variation { get; set; }

        /// <summary>
        ///     Gets or sets the progressive pack rtp.
        /// </summary>
        public ProgressiveRtp ProgressivePackRtp { get; set; }

        /// <summary>
        ///     Gets or sets the level type.
        /// </summary>
        public ProgressiveLevelType LevelType { get; set; }

        /// <summary>
        ///     Gets or sets the funding type.
        /// </summary>
        public SapFundingType FundingType { get; set; }

        /// <summary>
        ///     Gets or sets the level name.
        /// </summary>
        public string LevelName { get; set; }

        /// <summary>
        ///     Gets or sets the increment rate.
        /// </summary>
        public long IncrementRate { get; set; }

        /// <summary>
        ///     Gets or sets the hidden increment rate.
        /// </summary>
        public long HiddenIncrementRate { get; set; }

        /// <summary>
        ///     Gets or sets the probability.
        /// </summary>
        public long Probability { get; set; }

        /// <summary>
        ///     Gets or sets the maximum value.
        /// </summary>
        public long MaximumValue { get; set; }

        /// <summary>
        ///     Gets or sets the reset value.
        /// </summary>
        public long ResetValue { get; set; }

        /// <summary>
        ///     Gets or sets the level rtp.
        /// </summary>
        public long LevelRtp { get; set; }

        /// <summary>
        ///     Gets or sets the line group.
        /// </summary>
        public int LineGroup { get; set; }

        /// <summary>
        ///     Gets or sets the allow truncation.
        /// </summary>
        public bool AllowTruncation { get; set; }

        /// <summary>
        ///     Gets or sets the turn over.
        /// </summary>
        public long Turnover { get; set; }

        /// <summary>
        ///     Gets or sets the trigger control.
        /// </summary>
        public TriggerType TriggerControl { get; set; }

        /// <summary>
        ///     Gets or sets the assigned progressive id.
        /// </summary>
        public AssignableProgressiveId AssignedProgressiveId { get; set; }
    }
}
