namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    using System;

    /// <summary>
    ///     TODO: Should be a readonly model because it's generated at runtime, based on formula configuration for jurisdiction
    /// </summary>
    public class GameRtpReport
    {
        public RtpValidationStatus ValidationStatus { get; }

        public void GetRtpStatsForWagerCategory(string wagerCategoryId)
        {
            throw new NotImplementedException();
        }


        public void GetRtpStatsForVariation()
        {
            throw new NotImplementedException();
        }

        public void GetRtpStatsForGame()
        {
            throw new NotImplementedException();
        }

        public RtpBreakdown GetRtpStatsForPaytable()
        {
            throw new NotImplementedException();
        }
    }
}