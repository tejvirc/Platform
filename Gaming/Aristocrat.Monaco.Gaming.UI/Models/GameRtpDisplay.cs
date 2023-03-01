namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using Aristocrat.Monaco.Gaming.Contracts.Progressives;

    public class GameRtpDisplay
    {
        public bool HasExtendedRtpInformation { get; init; }

        public string BaseGameRtp { get; init; } 
 
        public string BaseGameRtpMin { get; init; } 
 
        public string BaseGameRtpMax { get; init; }

        public string StandaloneProgressiveResetRtp { get; init; } 
 
        public string StandaloneProgressiveResetRtpMin { get; init; } 
 
        public string StandaloneProgressiveResetRtpMax { get; init; }

        public string StandaloneProgressiveIncrementRtp { get; init; } 
 
        public string StandaloneProgressiveIncrementRtpMin { get; init; } 
 
        public string StandaloneProgressiveIncrementRtpMax { get; init; }

        public string LinkedProgressiveResetRtp { get; init; } 
 
        public string LinkedProgressiveResetRtpMin { get; init; } 
 
        public string LinkedProgressiveResetRtpMax { get; init; }

        public string LinkedProgressiveIncrementRtp { get; init; } 
 
        public string LinkedProgressiveIncrementRtpMin { get; init; } 
 
        public string LinkedProgressiveIncrementRtpMax { get; init; }

        public string TotalJurisdictionalRtp { get; init; }

        public string TotalJurisdictionalRtpMin { get; init; }

        public string TotalJurisdictionalRtpMax { get; init; }

        public RtpVerifiedState StandaloneProgressiveResetRtpState { get; init; }

        public RtpVerifiedState StandaloneProgressiveIncrementRtpState { get; init; }

        public RtpVerifiedState LinkedProgressiveResetRtpState { get; init; }

        public RtpVerifiedState LinkedProgressiveIncrementRtpState { get; init; }
    }
}