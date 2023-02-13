namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    using Models;

    /// <summary>
    ///     Jurisdictional game RTP contribution rule set. Each <see cref="GameType"/> has its own rule set.
    /// </summary>
    /// <remarks>
    ///     See the <c>Gaming.config.xml</c> jurisdictional settings file for modifying these values
    /// </remarks>
    public class RtpRules
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RtpRules" /> class.
        /// </summary>
        /// <param name="minimumRtp">The minimum RTP.</param>
        /// <param name="maximumRtp">The maximum RTP.</param>
        /// <param name="includeLinkProgressiveIncrementRtp">Include link progressive increment contribution to RTP.</param>
        /// <param name="includeLinkProgressiveStartUpRtp">Include link progressive start-up/reset contribution to RTP.</param>
        /// <param name="includeStandaloneProgressiveIncrementRtp">Include standalone progressive increment contribution to RTP.</param>
        /// <param name="includeStandaloneProgressiveStartUpRtp">Include standalone progressive start-up/reset contribution to RTP.</param>
        public RtpRules(
            decimal minimumRtp,
            decimal maximumRtp,
            bool includeLinkProgressiveIncrementRtp,
            bool includeLinkProgressiveStartUpRtp,
            bool includeStandaloneProgressiveIncrementRtp,
            bool includeStandaloneProgressiveStartUpRtp)
        {
            MinimumRtp = minimumRtp;
            MaximumRtp = maximumRtp;
            IncludeLinkProgressiveIncrementRtp = includeLinkProgressiveIncrementRtp;
            IncludeLinkProgressiveStartUpRtp = includeLinkProgressiveStartUpRtp;
            IncludeStandaloneProgressiveIncrementRtp = includeStandaloneProgressiveIncrementRtp;
            IncludeStandaloneProgressiveStartUpRtp = includeStandaloneProgressiveStartUpRtp;
        }

        /// <summary>
        ///     Gets or sets the minimum RTP.
        /// </summary>
        public decimal MinimumRtp { get; }

        /// <summary>
        ///     Gets or sets the maximum RTP.
        /// </summary>
        public decimal MaximumRtp { get; }

        /// <summary>
        ///     Flag indicating whether to include the Link Progressive Increment RTP.
        /// </summary>
        public bool IncludeLinkProgressiveIncrementRtp { get; }

        /// <summary>
        ///     Flag indicating whether to include Link Progressive Startup RTP.
        /// </summary>
        public bool IncludeLinkProgressiveStartUpRtp { get; }

        /// <summary>
        ///     Flag indicating whether to include Standalone Progressive Increment RTP.
        /// </summary>
        public bool IncludeStandaloneProgressiveIncrementRtp { get; }

        /// <summary>
        ///     Flag indicating whether to include Standalone Progressive Startup RTP.
        /// </summary>
        public bool IncludeStandaloneProgressiveStartUpRtp { get; }
    }
}