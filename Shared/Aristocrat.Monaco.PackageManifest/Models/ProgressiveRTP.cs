namespace Aristocrat.Monaco.PackageManifest.Models
{
    /// <summary>
    ///     Defines progressive rtp values
    /// </summary>
    public class ProgressiveRtp
    { 
        /// <summary>
        ///     Gets or sets the Reset Rtp Minimum
        /// </summary>
        public decimal ResetRtpMin { get; set; }

        /// <summary>
        ///     Gets or sets the Reset Rtp Maximum
        /// </summary>
        public decimal ResetRtpMax { get; set; }

        /// <summary>
        ///     Gets or sets the Increment Rtp Minimum
        /// </summary>
        public decimal IncrementRtpMin { get; set; }

        /// <summary>
        ///     Gets or sets the Increment Rtp Maximum
        /// </summary>
        public decimal IncrementRtpMax { get; set; }

        /// <summary>
        ///     Gets or sets the Base+Reset RTP Minimum
        /// </summary>
        public decimal? BaseRtpAndResetRtpMin { get; set; }

        /// <summary>
        ///     Gets or sets the Base+Reset RTP Maximum 
        /// </summary>
        public decimal? BaseRtpAndResetRtpMax { get; set; }

        /// <summary>
        ///     Gets or sets the Base+Reset+Increment RTP Minimum 
        /// </summary>
        public decimal? BaseRtpAndResetRtpAndIncRtpMin { get; set; }

        /// <summary>
        ///     Gets or sets the Base+Reset+Increment RTP Maximum 
        /// </summary>
        public decimal? BaseRtpAndResetRtpAndIncRtpMax { get; set; }
    }
}
