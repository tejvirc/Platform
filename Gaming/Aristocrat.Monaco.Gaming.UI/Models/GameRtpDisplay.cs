namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using Aristocrat.Monaco.Gaming.Contracts.Progressives;

    public class GameRtpDisplay
    {
        public bool HasExtendedRtpInformation { get; set; }

        public string BaseGameRtp { get; set; } 
 
        public string BaseGameRtpMin { get; set; } 
 
        public string BaseGameRtpMax { get; set; }

        public string StandaloneProgressiveResetRtp { get; set; } 
 
        public string StandaloneProgressiveResetRtpMin { get; set; } 
 
        public string StandaloneProgressiveResetRtpMax { get; set; }

        public string StandaloneProgressiveIncrementRtp { get; set; } 
 
        public string StandaloneProgressiveIncrementRtpMin { get; set; } 
 
        public string StandaloneProgressiveIncrementRtpMax { get; set; }

        public string LinkedProgressiveResetRtp { get; set; } 
 
        public string LinkedProgressiveResetRtpMin { get; set; } 
 
        public string LinkedProgressiveResetRtpMax { get; set; }

        public string LinkedProgressiveIncrementRtp { get; set; } 
 
        public string LinkedProgressiveIncrementRtpMin { get; set; } 
 
        public string LinkedProgressiveIncrementRtpMax { get; set; }

        public string TotalJurisdictionalRtp { get; set; }

        public string TotalJurisdictionalRtpMin { get; set; }

        public string TotalJurisdictionalRtpMax { get; set; }

        public RtpVerifiedState StandaloneProgressiveResetRtpState { get; set; }

        public RtpVerifiedState StandaloneProgressiveIncrementRtpState { get; set; }

        public RtpVerifiedState LinkedProgressiveResetRtpState { get; set; }

        public RtpVerifiedState LinkedProgressiveIncrementRtpState { get; set; }
    }
}