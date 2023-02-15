﻿namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    using System.Linq;

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
        public RtpRange TotalRtp => Base + StandaloneProgressiveIncrement + StandaloneProgressiveReset +
                                    LinkedProgressiveIncrement + LinkedProgressiveReset;

        /// <summary>
        ///     The results of the RTP validation
        /// </summary>
        public RtpValidationResult ValidationResult { get; set; } = new();

        /// <summary>
        ///     Totals together the given RTP breakdowns.
        /// </summary>
        /// <param name="rtpBreakdowns">The RTP breakdowns.</param>
        public static RtpBreakdown Total(params RtpBreakdown[] rtpBreakdowns)
        {
            return rtpBreakdowns.Aggregate((r1, r2) => r1.TotalWith(r2));
        }

        /// <summary>
        ///     Converts this model to string.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"{nameof(TotalRtp)}: {TotalRtp}, IsValid: {ValidationResult.IsValid}";
        }

        /// <summary>
        ///     Totals together the current RTP Breakdown values with the RTP Breakdown given.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>A new <see cref="RtpBreakdown"/> that is a total of the two.</returns>
        public RtpBreakdown TotalWith(RtpBreakdown other)
        {
            var breakdownTotal = new RtpBreakdown
            {
                Base = Base.TotalWith(other.Base),
                StandaloneProgressiveIncrement =
                    StandaloneProgressiveIncrement.TotalWith(other.StandaloneProgressiveIncrement),
                StandaloneProgressiveReset = StandaloneProgressiveReset.TotalWith(other.StandaloneProgressiveReset),
                LinkedProgressiveIncrement = LinkedProgressiveIncrement.TotalWith(other.LinkedProgressiveIncrement),
                LinkedProgressiveReset = LinkedProgressiveReset.TotalWith(other.LinkedProgressiveReset)
            };
            return breakdownTotal;
        }
    }
}