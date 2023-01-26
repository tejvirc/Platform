namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    public class RtpBreakdown
    {
        public RtpBreakdown(
            RtpRange standaloneProgressiveIncrement,
            RtpRange linkedProgressiveIncrement,
            RtpRange standaloneProgressiveReset,
            RtpRange standaloneProgressiveBase,
            RtpRange linkedProgressiveBase,
            RtpRange linkedProgressiveReset,
            RtpRange @base)
        {
            StandaloneProgressiveIncrement = standaloneProgressiveIncrement;
            LinkedProgressiveIncrement = linkedProgressiveIncrement;
            StandaloneProgressiveReset = standaloneProgressiveReset;
            StandaloneProgressiveBase = standaloneProgressiveBase;
            LinkedProgressiveBase = linkedProgressiveBase;
            LinkedProgressiveReset = linkedProgressiveReset;
            Base = @base;
        }

        public RtpRange Base { get; }

        public RtpRange StandaloneProgressiveBase { get; }

        public RtpRange StandaloneProgressiveIncrement { get; }

        public RtpRange StandaloneProgressiveReset { get; }

        public RtpRange LinkedProgressiveBase { get; }

        public RtpRange LinkedProgressiveIncrement { get; }

        public RtpRange LinkedProgressiveReset { get; }

        public RtpRange TotalRtp => Base +
                                    StandaloneProgressiveBase +
                                    StandaloneProgressiveIncrement +
                                    StandaloneProgressiveReset +
                                    LinkedProgressiveBase +
                                    LinkedProgressiveIncrement +
                                    LinkedProgressiveReset;
    }
}