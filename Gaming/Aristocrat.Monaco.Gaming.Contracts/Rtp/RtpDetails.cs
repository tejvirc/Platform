namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    /// <summary>
    ///     A fine-grain breakdown of RTP percentages which make up a total RTP percent range
    /// </summary>
    public class RtpDetails
    {
        /// <summary>
        ///     Gets the total RTP for base and feature games excluding all progressive elements.
        /// </summary>
        public RtpRange Base { get; set; }

        /// <summary>
        ///     Gets the total RTP coming from increment across all stand alone progressive levels.
        /// </summary>
        public decimal StandaloneProgressiveIncrement { get; set; }

        /// <summary>
        ///     Gets the total RTP coming from the start up amount (reset value) across all stand alone progressive levels.
        /// </summary>
        public RtpRange StandaloneProgressiveReset { get; set; }

        /// <summary>
        ///     Gets the total RTP coming from increment across all game driven linked progressive levels. This includes SSP and
        ///     MSP.
        /// </summary>
        public decimal LinkedProgressiveIncrement { get; set; }

        /// <summary>
        ///     Gets the total RTP coming from the start up amount (reset value) across all stand alone progressive levels.
        /// </summary>
        public RtpRange LinkedProgressiveReset { get; set; }

        /// <summary>
        ///     Gets the total RTP.
        /// </summary>
        public RtpRange TotalRtp => Base +
                                    StandaloneProgressiveIncrement +
                                    StandaloneProgressiveReset +
                                    LinkedProgressiveIncrement +
                                    LinkedProgressiveReset;

        /// <summary>
        ///     Implements the operator +.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns>The result of the operation.</returns>
        public static RtpDetails operator +(RtpDetails d1, RtpDetails d2)
        {
            return new RtpDetails
            {
                Base = d1.Base + d2.Base,
                StandaloneProgressiveReset = d1.StandaloneProgressiveReset + d2.StandaloneProgressiveReset,
                LinkedProgressiveReset = d1.LinkedProgressiveReset + d2.LinkedProgressiveReset,
                LinkedProgressiveIncrement = d1.LinkedProgressiveIncrement + d2.LinkedProgressiveIncrement,
                StandaloneProgressiveIncrement =
                    d1.StandaloneProgressiveIncrement + d2.StandaloneProgressiveIncrement
            };
        }
    }
}