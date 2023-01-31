namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    using System;

    /// <summary>
    ///     TODO: Should be a readonly model because it's generated at runtime, based on formula configuration for jurisdiction
    ///
    ///     Machine RTP > Game Type RTP > Game Theme RTP > Variation RTP > Wager Category RTP
    /// </summary>
    public class GameThemeRtpReport
    {
        /// <summary>
        /// Gets the validation information.
        /// </summary>
        public RtpValidationInfo ValidationInfo { get; }

        /// <summary>
        /// Gets the RTP stats by wager category.
        /// </summary>
        /// <param name="wagerCategoryId">The wager category identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        /// TODO Edit XML Comment Template for GetRtpStatsByWagerCategory
        public RtpStats GetRtpStatsByWagerCategory(string wagerCategoryId)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Gets the RTP stats by variation.
        /// </summary>
        /// <param name="variationId">The variation identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public RtpStats GetRtpStatsByVariation(string variationId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the total RTP Range for the Game Theme.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        /// TODO Edit XML Comment Template for GetTotalRtp
        public RtpStats GetTotalRtp()
        {
            throw new NotImplementedException();
        }
    }
}