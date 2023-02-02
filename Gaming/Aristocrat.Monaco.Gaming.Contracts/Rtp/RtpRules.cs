namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    /// <summary>
    /// </summary>
    /// <remarks>
    ///     See the <c>Gaming.config.xml</c> jurisdictional settings file for modifying these values
    /// </remarks>
    /// TODO Edit XML Comment Template for RtpRules
    public class RtpRules
    {
        /// <summary>
        ///     Gets or sets the minimum RTP.
        /// </summary>
        public decimal MinimumRtp { get; set; }

        /// <summary>
        ///     Gets or sets the maximum RTP.
        /// </summary>
        public decimal MaximumRtp { get; set; }

        /// <summary>
        ///     Flag indicating whether to include the Link Progressive Increment RTP.
        /// </summary>
        public bool IncludeLinkProgressiveIncrementRtp { get; set; }

        /// <summary>
        ///     Flag indicating whether to include Link Progressive Startup RTP.
        /// </summary>
        public bool IncludeLinkProgressiveStartUpRtp { get; set; }

        /// <summary>
        ///     Flag indicating whether to include Standalone Progressive Increment RTP.
        /// </summary>
        public bool IncludeStandaloneProgressiveIncrementRtp { get; set; }

        /// <summary>
        ///     Flag indicating whether to include Standalone Progressive Startup RTP.
        /// </summary>
        public bool IncludeStandaloneProgressiveStartUpRtp { get; set; }
    }
}