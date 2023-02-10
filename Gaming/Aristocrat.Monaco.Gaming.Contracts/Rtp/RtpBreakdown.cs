namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    /// <summary>
    ///     A fine-grain breakdown of RTP percentages for a WagerCategory
    /// </summary>
    public class RtpBreakdown
    {
        /// <summary>
        ///     Gets the total RTP for base and feature games excluding all progressive elements.
        /// </summary>
        public RtpRange Base { get; set; }

        /// <summary>
        ///     Gets the total RTP coming from increment across all stand alone progressive levels.
        /// </summary>
        public RtpRange StandaloneProgressiveIncrement { get; set; }

        /// <summary>
        ///     Gets the total RTP coming from the start up amount (reset value) across all stand alone progressive levels.
        /// </summary>
        public RtpRange StandaloneProgressiveReset { get; set; }

        /// <summary>
        ///     Gets the total RTP coming from increment across all game driven linked progressive levels. This includes SSP and
        ///     MSP.
        /// </summary>
        public RtpRange LinkedProgressiveIncrement { get; set; }

        /// <summary>
        ///     Gets the total RTP coming from the start up amount (reset value) across all stand alone progressive levels.
        /// </summary>
        public RtpRange LinkedProgressiveReset { get; set; }

        /// <summary>
        ///     Gets the total RTP.
        /// </summary>
        public RtpRange TotalRtp => Base + StandaloneProgressiveIncrement + StandaloneProgressiveReset + LinkedProgressiveIncrement + LinkedProgressiveReset;

        /// <summary>
        ///     The results of the RTP validation
        /// </summary>
        public RtpValidationResult ValidationResult { get; set; } = new();

        /// <summary>
        ///     Converts this model to string.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"{nameof(TotalRtp)}: {TotalRtp}, {nameof(ValidationResult)}: {ValidationResult}";
        }
    }
}