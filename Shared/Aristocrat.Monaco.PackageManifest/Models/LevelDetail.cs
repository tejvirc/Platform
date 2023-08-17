namespace Aristocrat.Monaco.PackageManifest.Models
{
    using System.Collections.Generic;
    using Ati;

    /// <summary>
    ///     Defines a progressive level
    /// </summary>
    public class LevelDetail
    {
        /// <summary>
        ///     Gets or sets the level Id
        /// </summary>
        public int LevelId { get; set; }

        /// <summary>
        ///     Gets or sets the level name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the increment rate
        /// </summary>
        public decimal IncrementRate { get; set; }

        /// <summary>
        ///     Gets or sets the hidden increment rate
        /// </summary>
        public decimal HiddenIncrementRate { get; set; }

        /// <summary>
        ///     Gets or sets the probability.
        /// </summary>
        /// <value>The probability.</value>
        public decimal Probability { get; set; }

        /// <summary>
        ///     Gets or sets the maximum value (Ceiling)
        /// </summary>
        public ProgressiveValue MaximumValue { get; set; }

        /// <summary>
        ///     Gets or sets the startup value
        /// </summary>
        public ProgressiveValue StartupValue { get; set; }

        /// <summary>
        ///     Gets or sets the allowTruncation value indicating whether it is a platinum jackpot level (allowTruncation)
        /// </summary>
        public bool AllowTruncation { get; set; }

        /// <summary>
        ///     Gets or sets the bonus values (Bonuses)
        /// </summary>
        public Dictionary<string, long> BonusValues { get; set; }

        /// <summary>
        ///     Gets or sets Level Type value
        /// </summary>
        public LevelType LevelType { get; set; }

        /// <summary>
        ///     Gets or sets SAP funding type
        /// </summary>
        public sapFundingType SapFundingType { get; set; }

        /// <summary>
        ///     Gets or sets flavor type
        /// </summary>
        public flavorType FlavorType { get; set; }

        /// <summary>
        ///     Gets or sets the selection values of the level.
        /// </summary>
        public LevelSelectType[] Selections { get; set; }

        /// <summary>
        ///     Gets or sets the type of the progressive.
        /// </summary>
        /// <value>The type of the progressive.</value>
        public progressiveType ProgressiveType { get; set; }

        /// <summary>
        ///     Gets or sets the type of the trigger.
        /// </summary>
        /// <value>The type of the trigger.</value>
        public triggerType Trigger { get; set; }

        /// <summary>
        ///     Gets or set the line group
        /// </summary>
        public int LineGroup { get; set; }

        /// <summary>
        ///     Gets or sets the RTP for the level
        /// </summary>
        public decimal Rtp { get; set; }
    }
}