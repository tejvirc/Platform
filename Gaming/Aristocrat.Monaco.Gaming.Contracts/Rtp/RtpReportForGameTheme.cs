namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     TODO: fill this in
    ///     TODO: Validate the validations
    /// </summary>
    public class RtpReportForGameTheme
    {
        private const int RequiredLevelOfRtpPrecision = 5;
        private readonly IList<IGameDetail> _gamesDetails;
        private readonly RtpRules _rules;
        private readonly Dictionary<string, Dictionary<string, RtpBreakdown>> _rtpBreakdownsByVariationAndWagerCategory = new();

        /// <summary>
        ///     Initializes a new instance of the <see cref="RtpReportForGameTheme" /> class.
        /// </summary>
        /// <param name="gamesDetails">The games details.</param>
        /// <param name="rules">
        ///     The rules used to validate and filter out invalid RTP. It also includes rules for customizing
        ///     the RTP calculation
        /// </param>
        public RtpReportForGameTheme(IEnumerable<IGameDetail> gamesDetails, RtpRules rules)
        {
            _gamesDetails = gamesDetails.ToList();
            _rules = rules;

            SetupRtpBreakdowns();

            RunAllRtpValidations();
        }

        /// <summary>
        ///     Gets the Total RTP breakdown for the given variation.
        /// </summary>
        /// <param name="variationId">The variation identifier.</param>
        /// <returns>The total RTP statistics for the variation</returns>
        public RtpBreakdown GetTotalRtpBreakdownForVariation(string variationId)
        {
            if (!_rtpBreakdownsByVariationAndWagerCategory.TryGetValue(variationId, out var wagerCategoryRtpBreakdowns))
            {
                throw new Exception($"Could not find any RTP Details for VariationId: \"{variationId}\"");
            }

            var totalRtpBreakdown = wagerCategoryRtpBreakdowns.Values.Aggregate((d1, d2) => d1 + d2);

            return totalRtpBreakdown;
        }

        /// <summary>
        ///     Gets the RTP Breakdown for the given wager category.
        /// </summary>
        /// <param name="variationId">The variation identifier.</param>
        /// <param name="wagerCategoryId">The wager category identifier.</param>
        /// <returns>The RTP Breakdown for the wager category</returns>
        public RtpBreakdown GetRtpBreakdownForWagerCategory(string variationId, string wagerCategoryId)
        {
            if (!_rtpBreakdownsByVariationAndWagerCategory.TryGetValue(variationId, out var wagerCategoryRtpDetails))
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
        public RtpBreakdown GetTotalRtp()
        {
            var variationRtpDetailTotals =
                _rtpBreakdownsByVariationAndWagerCategory.Keys.Select(GetTotalRtpBreakdownForVariation);

            var rtpDetailsTotal = variationRtpDetailTotals.Aggregate((d1, d2) => d1 + d2);

            return rtpDetailsTotal;
        }

        private void SetupRtpBreakdowns()
        {
            foreach (var gameVariant in _gamesDetails)
            {
                var rtpBreakdownsForGameVariant = new Dictionary<string, RtpBreakdown>();

                foreach (var wagerCategory in gameVariant.WagerCategories)
                {
                    var rtpBreakdown = new RtpBreakdown
                    {
                        Base = new RtpRange(wagerCategory.MinBaseRtpPercent, wagerCategory.MaxBaseRtpPercent),
                        StandaloneProgressiveIncrement = _rules.IncludeStandaloneProgressiveIncrementRtp
                            ? new RtpRange(wagerCategory.SapIncrementRtpPercent, wagerCategory.SapIncrementRtpPercent)
                            : RtpRange.Zero,
                        StandaloneProgressiveReset = _rules.IncludeStandaloneProgressiveStartUpRtp
                            ? new RtpRange(wagerCategory.MinSapStartupRtpPercent, wagerCategory.MaxSapStartupRtpPercent)
                            : RtpRange.Zero,
                        LinkedProgressiveIncrement = _rules.IncludeLinkProgressiveIncrementRtp
                            ? new RtpRange(wagerCategory.LinkIncrementRtpPercent, wagerCategory.LinkIncrementRtpPercent)
                            : RtpRange.Zero,
                        LinkedProgressiveReset = _rules.IncludeLinkProgressiveStartUpRtp
                            ? new RtpRange(wagerCategory.MinLinkStartupRtpPercent, wagerCategory.MaxLinkStartupRtpPercent)
                            : RtpRange.Zero,
                    };

                    rtpBreakdownsForGameVariant.Add(gameVariant.VariationId, rtpBreakdown);
                }

                _rtpBreakdownsByVariationAndWagerCategory.Add(gameVariant.VariationId, rtpBreakdownsForGameVariant);
            }
        }

        private void RunAllRtpValidations()
        {
            foreach (var gameVariantRtp in _rtpBreakdownsByVariationAndWagerCategory)
            {
                foreach (var wagerCategoryRtp in gameVariantRtp.Value)
                {
                    ValidateRtpValues(wagerCategoryRtp.Value);

                    ValidatePrecision(wagerCategoryRtp.Value, RequiredLevelOfRtpPrecision);

                    ValidateJurisdictionalLimits(wagerCategoryRtp.Value);
                }
            }
        }

        private void ValidateJurisdictionalLimits(RtpBreakdown rtpBreakdown)
        {
            if (rtpBreakdown.TotalRtp.Minimum < _rules.MinimumRtp)
            {
                rtpBreakdown.ValidationResult.FailureFlags |= RtpValidationFailureFlags.RtpExceedsJurisdictionalMinimum;
            }
            if (rtpBreakdown.TotalRtp.Maximum > _rules.MaximumRtp)
            {
                rtpBreakdown.ValidationResult.FailureFlags |= RtpValidationFailureFlags.RtpExceedsJurisdictionalMaximum;
            }
        }

        private static void ValidateRtpValues(RtpBreakdown rtpBreakdown)
        {
            if (CheckRtpRange(rtpBreakdown.Base) &&
                CheckRtpRange(rtpBreakdown.StandaloneProgressiveReset) &&
                CheckRtpRange(rtpBreakdown.StandaloneProgressiveIncrement) &&
                CheckRtpRange(rtpBreakdown.LinkedProgressiveReset) &&
                CheckRtpRange(rtpBreakdown.LinkedProgressiveIncrement))
            {
                return;
            }

            rtpBreakdown.ValidationResult.FailureFlags |= RtpValidationFailureFlags.InvalidRtpValue;
        }

        private static void ValidatePrecision(RtpBreakdown rtpBreakdown, int numOfDecimalPlaces)
        {
            if (CheckPrecision(rtpBreakdown.Base.Minimum, numOfDecimalPlaces) &&
                CheckPrecision(rtpBreakdown.Base.Maximum, numOfDecimalPlaces) &&
                CheckPrecision(rtpBreakdown.StandaloneProgressiveReset.Minimum, numOfDecimalPlaces) &&
                CheckPrecision(rtpBreakdown.StandaloneProgressiveReset.Maximum, numOfDecimalPlaces) &&
                CheckPrecision(rtpBreakdown.StandaloneProgressiveIncrement.Minimum, numOfDecimalPlaces) &&
                CheckPrecision(rtpBreakdown.StandaloneProgressiveIncrement.Maximum, numOfDecimalPlaces) &&
                CheckPrecision(rtpBreakdown.LinkedProgressiveReset.Minimum, numOfDecimalPlaces) &&
                CheckPrecision(rtpBreakdown.LinkedProgressiveReset.Maximum, numOfDecimalPlaces) &&
                CheckPrecision(rtpBreakdown.LinkedProgressiveIncrement.Minimum, numOfDecimalPlaces) &&
                CheckPrecision(rtpBreakdown.LinkedProgressiveIncrement.Maximum, numOfDecimalPlaces))
            {
                return;
            }

            rtpBreakdown.ValidationResult.FailureFlags |= RtpValidationFailureFlags.InsufficientRtpPrecision;
        }

        private static bool CheckPrecision(decimal value, int numOfDecimalPlaces)
        {
            // https://stackoverflow.com/questions/6092243/c-sharp-check-if-a-decimal-has-more-than-3-decimal-places
            return decimal.Round(value, numOfDecimalPlaces) == value;
        }

        private static bool CheckRtpRange(RtpRange rtpRange)
        {
            return rtpRange.Maximum >= rtpRange.Minimum &&
                   rtpRange.Minimum <= rtpRange.Maximum &&
                   rtpRange.Maximum <= 100.0m &&
                   rtpRange.Minimum >= 0.0m;
        }
    }
}