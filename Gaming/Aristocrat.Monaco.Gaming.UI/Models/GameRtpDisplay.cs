namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using Contracts.Progressives;

    public class GameRtpDisplay
    {
        /// <summary>
        ///     Gets or sets a value indicating whether RTP contribution breakdown values are present.
        /// </summary>
        public bool HasExtendedRtpInformation { get; set; }

        /// <summary>
        ///     Gets or sets the display value for base game RTP.
        /// </summary>
        public string BaseGameRtp { get; set; }

        /// <summary>
        ///     Gets or sets the display value for base game RTP minimum.
        /// </summary>
        public string BaseGameRtpMin { get; set; }

        /// <summary>
        ///     Gets or sets the display value for base game RTP maximum.
        /// </summary>
        public string BaseGameRtpMax { get; set; }

        /// <summary>
        ///     Gets or sets the display value for Standalone Progressive Reset RTP.
        /// </summary>
        public string StandaloneProgressiveResetRtp { get; set; }

        /// <summary>
        ///     Gets or sets the display value for Standalone Progressive Reset RTP minimum.
        /// </summary>
        public string StandaloneProgressiveResetRtpMin { get; set; }

        /// <summary>
        ///     Gets or sets the display value for Standalone Progressive Reset RTP maximum.
        /// </summary>
        public string StandaloneProgressiveResetRtpMax { get; set; }

        /// <summary>
        ///     Gets or sets the display value for Standalone Progressive Increment RTP.
        /// </summary>
        public string StandaloneProgressiveIncrementRtp { get; set; }

        /// <summary>
        ///     Gets or sets the display value for Standalone Progressive Increment RTP minimum.
        /// </summary>
        public string StandaloneProgressiveIncrementRtpMin { get; set; }

        /// <summary>
        ///     Gets or sets the display value for Standalone Progressive Increment RTP maximum.
        /// </summary>
        public string StandaloneProgressiveIncrementRtpMax { get; set; }

        /// <summary>
        ///     Gets or sets the display value for Linked Progressive Reset RTP.
        /// </summary>
        public string LinkedProgressiveResetRtp { get; set; }

        /// <summary>
        ///     Gets or sets the display value for Linked Progressive Reset RTP minimum.
        /// </summary>
        public string LinkedProgressiveResetRtpMin { get; set; }

        /// <summary>
        ///     Gets or sets the display value for Linked Progressive Reset RTP maximum.
        /// </summary>
        public string LinkedProgressiveResetRtpMax { get; set; }

        /// <summary>
        ///     Gets or sets the display value for Linked Progressive Increment RTP.
        /// </summary>
        public string LinkedProgressiveIncrementRtp { get; set; }

        /// <summary>
        ///     Gets or sets the display value for Linked Progressive Increment RTP minimum.
        /// </summary>
        public string LinkedProgressiveIncrementRtpMin { get; set; }

        /// <summary>
        ///     Gets or sets the display value for Linked Progressive Increment RTP maximum.
        /// </summary>
        public string LinkedProgressiveIncrementRtpMax { get; set; }

        /// <summary>
        ///     Gets or sets the display value for Total RTP.
        /// </summary>
        public string TotalRtp { get; set; }

        /// <summary>
        ///     Gets or sets the display value for Total RTP minimum.
        /// </summary>
        public string TotalRtpMin { get; set; }

        /// <summary>
        ///     Gets or sets the display value for Total RTP maximum.
        /// </summary>
        public string TotalRtpMax { get; set; }

        /// <summary>
        ///     Gets or sets the state of the Standalone Progressive Reset RTP.
        /// </summary>
        public RtpVerifiedState StandaloneProgressiveResetRtpState { get; set; }

        /// <summary>
        ///     Gets or sets the state of the Standalone Progressive Increment RTP.
        /// </summary>
        public RtpVerifiedState StandaloneProgressiveIncrementRtpState { get; set; }

        /// <summary>
        ///     Gets or sets the state of the Linked Progressive Reset RTP.
        /// </summary>
        public RtpVerifiedState LinkedProgressiveResetRtpState { get; set; }

        /// <summary>
        ///     Gets or sets the state of the Linked Progressive Increment RTP.
        /// </summary>

        public RtpVerifiedState LinkedProgressiveIncrementRtpState { get; set; }
    }
}