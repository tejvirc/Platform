namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    using System.Linq;
    using Progressives;

    /// <summary>
    ///     A fine-grain breakdown of RTP percentages for a WagerCategory
    /// </summary>
    /// 
    public class RtpBreakdown
    {
        /// <summary>
        ///     Gets the total RTP.
        /// </summary>
        public RtpRange TotalRtp => Base + StandaloneProgressiveIncrement + StandaloneProgressiveReset +
                                    LinkedProgressiveIncrement + LinkedProgressiveReset;

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
        ///     Gets a model which contains the associated verification states for the RTP breakdown values (contributions).
        ///     This is needed when displaying RTP breakdown in the UI, because the verification state of each breakdown value must
        ///     also be taken into account in the view logic.
        /// </summary>
        public ProgressiveRtpVerificationState ProgressiveVerificationState { get; set; }

        /// <summary>
        ///     Is <c>true</c> if all rtp values are both verified and valid; otherwise, is <c>false</c> if any errors exist.
        /// </summary>
        public bool IsValid => FailureFlags == RtpValidationFailureFlags.None;

        /// <summary>
        ///     Gets or sets the validation failure flags.
        /// </summary>
        public RtpValidationFailureFlags FailureFlags { get; set; }

        /// <summary>
        ///     Totals together the given RTP breakdowns. During the aggregation, since there is no meaningful way of aggregating
        ///     <see cref="ProgressiveVerificationState" />, it is set to null on the final output. All failure flags from the set
        ///     of <see cref="RtpBreakdown" />s are merged into the
        ///     <see cref="FailureFlags" /> property of the final output.
        /// </summary>
        /// <param name="rtpBreakdowns">The RTP breakdowns.</param>
        public static RtpBreakdown GetTotal(params RtpBreakdown[] rtpBreakdowns)
        {
            var total = rtpBreakdowns.Aggregate(
                (r1, r2) =>
                    new RtpBreakdown
                    {
                        Base = r1.Base.GetTotalWith(r2.Base),
                        StandaloneProgressiveIncrement =
                            r1.StandaloneProgressiveIncrement.GetTotalWith(r2.StandaloneProgressiveIncrement),
                        StandaloneProgressiveReset =
                            r1.StandaloneProgressiveReset.GetTotalWith(r2.StandaloneProgressiveReset),
                        LinkedProgressiveIncrement =
                            r1.LinkedProgressiveIncrement.GetTotalWith(r2.LinkedProgressiveIncrement),
                        LinkedProgressiveReset = r1.LinkedProgressiveReset.GetTotalWith(r2.LinkedProgressiveReset),
                        FailureFlags = r1.FailureFlags | r2.FailureFlags,
                        ProgressiveVerificationState = null
                    });

            return total;
        }

        /// <summary>
        ///     Converts this model to string.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"{nameof(TotalRtp)}: {TotalRtp}, IsValid: {IsValid}";
        }
    }
}