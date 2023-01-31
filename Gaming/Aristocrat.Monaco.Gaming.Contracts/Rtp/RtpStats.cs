namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    /// <summary>
    ///     A fine-grain breakdown of RTP percentages which make up a total RTP percent
    /// </summary>
    public class RtpStats
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RtpStats"/> class.
        /// </summary>
        /// <param name="standaloneProgressiveIncrement">The standalone progressive increment.</param>
        /// <param name="linkedProgressiveIncrement">The linked progressive increment.</param>
        /// <param name="standaloneProgressiveReset">The standalone progressive reset.</param>
        /// <param name="linkedProgressiveReset">The linked progressive reset.</param>
        /// <param name="base">The base.</param>
        public RtpStats(
            RtpRange standaloneProgressiveIncrement,
            RtpRange linkedProgressiveIncrement,
            RtpRange standaloneProgressiveReset,
            //RtpRange standaloneProgressiveBase,
            //RtpRange linkedProgressiveBase,
            RtpRange linkedProgressiveReset,
            RtpRange @base)
        {
            StandaloneProgressiveIncrement = standaloneProgressiveIncrement;
            LinkedProgressiveIncrement = linkedProgressiveIncrement;
            StandaloneProgressiveReset = standaloneProgressiveReset;
            //StandaloneProgressiveBase = standaloneProgressiveBase;
            //LinkedProgressiveBase = linkedProgressiveBase;
            LinkedProgressiveReset = linkedProgressiveReset;
            Base = @base;
        }

        // TODO: Do we need these? If so update total RTP Prop
        //public RtpRange StandaloneProgressiveBase { get; }
        //public RtpRange LinkedProgressiveBase { get; }

        /// <summary>
        ///     Gets the total RTP for base and feature games excluding all progressive elements.
        /// </summary>
        public RtpRange Base { get; }

        /// <summary>
        ///     Gets the total RTP coming from increment across all stand alone progressive levels.
        /// </summary>
        public RtpRange StandaloneProgressiveIncrement { get; }

        /// <summary>
        ///     Gets the total RTP coming from the start up amount (reset value) across all stand alone progressive levels. 
        /// </summary>
        public RtpRange StandaloneProgressiveReset { get; }

        /// <summary>
        /// Gets the total RTP coming from increment across all game driven linked progressive levels. This includes SSP and MSP.
        /// </summary>
        public RtpRange LinkedProgressiveIncrement { get; }

        /// <summary>
        ///     Gets the total RTP coming from the start up amount (reset value) across all stand alone progressive levels. 
        /// </summary>
        public RtpRange LinkedProgressiveReset { get; }

        /// <summary>
        /// Gets the total RTP.
        /// </summary>
        public RtpRange TotalRtp => Base +
                                    StandaloneProgressiveIncrement +
                                    StandaloneProgressiveReset +
                                    LinkedProgressiveIncrement +
                                    LinkedProgressiveReset;
    }
}