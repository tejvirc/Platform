namespace Aristocrat.Monaco.PackageManifest.Models
{
    using Ati;
    using System.Collections.Generic;

    /// <summary>
    ///     Defines a progressive configuration object
    /// </summary>
    public class ProgressiveDetail
    {
        /// <summary>
        ///     Gets or sets the progressive ID (ie index) from inside a progressive pack
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Gets or sets the progressive pack name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the progressive pack Id
        /// </summary>
        public int  PackId { get; set; }

        /// <summary>
        ///     Gets or sets the progressive level pack
        /// </summary>
        public string LevelPack { get; set; }

        /// <summary>
        ///     Gets or sets the denomination
        /// </summary>
        public IEnumerable<string> Denomination { get; set; }

        /// <summary>
        ///     Gets or sets the variation
        /// </summary>
        public string Variation { get; set; }

        /// <summary>
        ///     Gets or sets the levels
        /// </summary>
        public IEnumerable<LevelDetail> Levels { get; set; }

        /// <summary>
        ///     Gets or Sets the use levels from specific level pack
        /// </summary>
        public IEnumerable<string> UseLevels { get; set; }

        /// <summary>
        ///     Gets or sets the RTP
        /// </summary>
        public ProgressiveRtp ReturnToPlayer { get; set; }

        /// <summary>
        ///     Gets or Sets Progressive type
        /// </summary>
        public progressiveType ProgressiveType { get; set; }

        /// <summary>
        ///     Gets or sets the funding type
        /// </summary>
        public sapFundingType FundingType { get; set; }

        /// <summary>
        ///     Gets or Sets turnover value
        /// </summary>
        public long Turnover { get; set; }

        /// <summary>
        ///     Placeholder for future support when the game defines progressives that contribute to all (applicable) denominations
        /// </summary>
        public bool ScaledByDenomination { get; } = true;

        /// <summary>
        ///     Gets or sets the pool creation type {Default, All, Max}. Case-sensitive
        /// </summary>
        public poolCreationType CreationType { get; set; }
    }
}