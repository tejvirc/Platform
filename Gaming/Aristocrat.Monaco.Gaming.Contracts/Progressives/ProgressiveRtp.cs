namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    using System;
    using Rtp;

    /// <summary>
    ///     Definition of a Progressive rtp
    /// </summary>
    [Serializable]
    public class ProgressiveRtp
    {
        /// <summary>
        ///     Gets or sets the reset rtp
        /// </summary>
        public RtpRange Reset { get; set; }

        /// <summary>
        ///     Gets or sets the increment rtp
        /// </summary>
        public RtpRange Increment { get; set; }

        /// <summary>
        ///     Gets or sets the base and reset rtp
        /// </summary>
        public RtpRange BaseAndReset { get; set; }

        /// <summary>
        ///     Gets or sets the base and reset and increment rtp
        /// </summary>
        public RtpRange BaseAndResetAndIncrement { get; set; }
    }
}
