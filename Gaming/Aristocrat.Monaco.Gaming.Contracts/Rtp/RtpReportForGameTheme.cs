namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     TODO: fill this in
    /// </summary>
    public class RtpReportForGameTheme
    {
        private readonly Dictionary<string, Dictionary<string, RtpDetails>> _rtpDetailsByVariationAndWagerCategory = new();

        /// <summary>
        ///     Initializes a new instance of the <see cref="RtpReportForGameTheme" /> class.
        /// </summary>
        /// <param name="gamesDetails">The games details.</param>
        public RtpReportForGameTheme(IEnumerable<IGameDetail> gamesDetails)
        {
            foreach (var gameVariant in gamesDetails)
            {
                var rtpDetailsForGameVariant = new Dictionary<string, RtpDetails>();

                foreach (var wagerCategory in gameVariant.WagerCategories)
                {
                    var rtpDetails = new RtpDetails
                    {
                        Base = new RtpRange(wagerCategory.MinBaseRtpPercent, wagerCategory.MaxBaseRtpPercent),
                        StandaloneProgressiveIncrement = wagerCategory.SapIncrementRtpPercent,
                        StandaloneProgressiveReset =
                            new RtpRange(
                                wagerCategory.MinSapStartupRtpPercent,
                                wagerCategory.MaxSapStartupRtpPercent),
                        LinkedProgressiveIncrement = wagerCategory.LinkIncrementRtpPercent,
                        LinkedProgressiveReset = new RtpRange(
                            wagerCategory.MinLinkStartupRtpPercent,
                            wagerCategory.MaxLinkStartupRtpPercent)
                    };

                    rtpDetailsForGameVariant.Add(gameVariant.VariationId, rtpDetails);
                }

                _rtpDetailsByVariationAndWagerCategory.Add(gameVariant.VariationId, rtpDetailsForGameVariant);
            }
        }

        /// <summary>
        ///     Gets the validation information for the for the RTP Ranges in the report.
        /// </summary>
        public RtpValidationInfo ValidationInfo { get; } = new();

        /// <summary>
        ///     Gets the Total RTP Details for the given variation.
        /// </summary>
        /// <param name="variationId">The variation identifier.</param>
        /// <returns>The total RTP statistics for the variation</returns>
        public RtpDetails GetTotalRtpDetailsForVariation(string variationId)
        {
            if (!_rtpDetailsByVariationAndWagerCategory.TryGetValue(variationId, out var wagerCategoryRtpDetails))
            {
                throw new Exception($"Could not find any RTP Details for VariationId: \"{variationId}\"");
            }

            var totalRtpDetails = wagerCategoryRtpDetails.Values.Aggregate((d1, d2) => d1 + d2);

            return totalRtpDetails;
        }

        /// <summary>
        ///     Gets the RTP Details for the given wager category.
        /// </summary>
        /// <param name="variationId">The variation identifier.</param>
        /// <param name="wagerCategoryId">The wager category identifier.</param>
        /// <returns>The RTP details for the wager category</returns>
        public RtpDetails GetRtpDetailsForWagerCategory(string variationId, string wagerCategoryId)
        {
            if (!_rtpDetailsByVariationAndWagerCategory.TryGetValue(variationId, out var wagerCategoryRtpDetails))
            {
                throw new Exception($"Could not find any RTP Details for VariationId: \"{variationId}\"");
            }

            if (!wagerCategoryRtpDetails.TryGetValue(wagerCategoryId, out var rtpDetails))
            {
                throw new Exception($"Could not find any RTP Details for WagerCategoryId: \"{wagerCategoryId}\"");
            }

            return rtpDetails;
        }

        /// <summary>
        ///     Gets the total RTP Range for the Game Theme.
        /// </summary>
        /// <returns>The RTP statistics for the Game Theme</returns>
        public RtpDetails GetTotalRtp()
        {
            var variationRtpDetailTotals =
                _rtpDetailsByVariationAndWagerCategory.Keys.Select(GetTotalRtpDetailsForVariation);

            var rtpDetailsTotal = variationRtpDetailTotals.Aggregate((d1, d2) => d1 + d2);

            return rtpDetailsTotal;
        }
    }
}