namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    /// <summary>
    ///     Contains the verification state for Games progressive RTP contributions.
    /// </summary>
    public class ProgressiveRtpVerificationState
    {
        /// <summary>
        ///     Gets or sets the verification state of the Stand Alone Progressive Increment RTP.
        /// </summary>
        public RtpVerifiedState SapResetRtpState { get; set; }

        /// <summary>
        ///     Gets or sets the verification state of the Stand Alone Progressive Increment RTP.
        /// </summary>
        public RtpVerifiedState SapIncrementRtpState { get; set; }

        /// <summary>
        ///     Gets or sets the verification state of the Linked Progressive Reset RTP.
        /// </summary>
        public RtpVerifiedState LpResetRtpState { get; set; }

        /// <summary>
        ///     Gets or sets the verification state of the Linked Progressive Increment RTP.
        /// </summary>
        public RtpVerifiedState LpIncrementRtpState { get; set; }
    }
}