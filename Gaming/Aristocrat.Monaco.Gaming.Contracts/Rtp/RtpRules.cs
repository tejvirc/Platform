namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    using Models;

    /// <summary>
    ///     Jurisdictional game RTP contribution rule set. Each <see cref="GameType"/> has its own rule set.
    /// </summary>
    /// <remarks>
    ///     See the <c>Gaming.config.xml</c> jurisdictional settings file for modifying these values.
    /// </remarks>
    public class RtpRules
    {
        /// <summary>
        ///     Gets or sets the minimum RTP.
        /// </summary>
        public decimal MinimumRtp { get; init; }

        /// <summary>
        ///     Gets or sets the maximum RTP.
        /// </summary>
        public decimal MaximumRtp { get; init; }

        /// <summary>
        ///     Flag indicating whether to include the Link Progressive Increment RTP.
        /// </summary>
        public bool IncludeLinkProgressiveIncrementRtp { get; init; }

        /// <summary>
        ///     Flag indicating whether to include Link Progressive Startup RTP.
        /// </summary>
        public bool IncludeLinkProgressiveStartUpRtp { get; init; }

        /// <summary>
        ///     Flag indicating whether to include Standalone Progressive Increment RTP.
        /// </summary>
        public bool IncludeStandaloneProgressiveIncrementRtp { get; init; }

        /// <summary>
        ///     Flag indicating whether to include Standalone Progressive Startup RTP.
        /// </summary>
        public bool IncludeStandaloneProgressiveStartUpRtp { get; init; }
    }
}