namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    using Aristocrat.Monaco.PackageManifest.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     TODO: DELETE ME
    /// </summary>
    public class RtpReport
    {
        private const int RequiredLevelOfRtpPrecision = 5;
        private readonly RtpRules _rules;
        private readonly Dictionary<string, Dictionary<string, RtpBreakdown>> _rtpBreakdownsByVariationThenWagerCategory = new();

        // TODO: DELETE ME
        /// <summary>
        ///     Initializes a new instance of the <see cref="RtpReport" /> class.
        /// </summary>
        /// <param name="gameDetails">The games details.</param>
        /// <param name="rules">
        ///     The rules used to validate and filter out invalid RTP. It also includes rules for customizing
        ///     the RTP calculation
        /// </param>
        public RtpReport(IEnumerable<IGameDetail> gameDetails, RtpRules rules)
        {
            _rules = rules;
            _rtpBreakdownsByVariationThenWagerCategory = GenerateRtpBreakdowns(gameDetails);

            RunAllRtpValidations();
        }

        /// <summary>
        ///     Gets the Total RTP for the given game variation.
        /// </summary>
        /// <param name="variationId">The variation identifier.</param>
        /// <returns>The total RTP the variation</returns>
        public RtpRange GetTotalVariationRtp(string variationId)
        {
            if (!_rtpBreakdownsByVariationThenWagerCategory.TryGetValue(variationId, out var wagerCategoryRtpBreakdowns))
            {
                throw new Exception($"Could not find any RTP information for VariationId: \"{variationId}\"");
            }

            var totalRtpBreakdown = wagerCategoryRtpBreakdowns.Values
                .Select(breakdown => breakdown.TotalRtp)
                .Aggregate((r1, r2) => r1.TotalWith(r2));

            return totalRtpBreakdown;
        }

        /// <summary>
        ///     Gets the total RTP Range for the Game Theme.
        /// </summary>
        /// <returns>The RTP statistics for the Game Theme</returns>
        public RtpRange GetTotalRtp()
        {
            var variationRtpTotals =
                _rtpBreakdownsByVariationThenWagerCategory.Keys.Select(GetTotalVariationRtp);

            var rtpTotal = variationRtpTotals.Aggregate((r1, r2) => r1.TotalWith(r2));

            return rtpTotal;
        }

        /// <summary>
        ///     Gets the RTP Breakdown for the given game variation and wager category.
        /// </summary>
        /// <param name="variationId">The variation identifier.</param>
        /// <param name="wagerCategoryId">The wager category identifier.</param>
        /// <returns>The RTP Breakdown</returns>
        public RtpBreakdown GetRtpBreakdown(string variationId, string wagerCategoryId)
        {
            var rtpBreakdownsForVariation = GetRtpBreakdowns(variationId);

            var rtpBreakdownPair =
                rtpBreakdownsForVariation.FirstOrDefault(pair => pair.wagerCategoryId.Equals(wagerCategoryId));

            if (rtpBreakdownPair == default((string wagerCategoryId, RtpBreakdown rtpBreakdown)))
            {
                throw new Exception($"Could not find any RTP information for WagerCategoryId: \"{wagerCategoryId}\"");
            }

            return rtpBreakdownPair.rtpBreakdown;
        }

        /// <summary>
        ///     Gets a list of (WagerCategoryId, <see cref="RtpBreakdown" />) tuples. Each tuple consists of a
        ///     WagerCategory and its respective RTP Breakdown.
        /// </summary>
        /// <param name="variationId">The variation identifier.</param>
        /// <returns>A collection of (WagerCategoryId, <see cref="RtpBreakdown" />) tuples</returns>
        public IEnumerable<(string wagerCategoryId, RtpBreakdown rtpBreakdown)> GetRtpBreakdowns(string variationId)
        {
            if (!_rtpBreakdownsByVariationThenWagerCategory.TryGetValue(
                    variationId,
                    out var wagerCategoryRtpBreakdowns))
            {
                throw new Exception($"Could not find any RTP information for VariationId: \"{variationId}\"");
            }

            var rtpBreakdowns = wagerCategoryRtpBreakdowns.Select(pair => (pair.Key, pair.Value)).ToList();

            return rtpBreakdowns;
        }

        private Dictionary<string, Dictionary<string, RtpBreakdown>> GenerateRtpBreakdowns(IEnumerable<IGameDetail> games)
        {
            var rtpBreakdowns = new Dictionary<string, Dictionary<string, RtpBreakdown>>();

            foreach (var gameDetail in games)
            {
                var rtpBreakdownsByWagerCategoryId = gameDetail.WagerCategories
                    .ToDictionary(wagerCategory => wagerCategory.Id, ConvertToRtpBreakdown);

                rtpBreakdowns.Add(gameDetail.VariationId, rtpBreakdownsByWagerCategoryId);
            }

            return rtpBreakdowns;
        }

        private RtpBreakdown ConvertToRtpBreakdown(IWagerCategory wagerCategory)
        {
            return new RtpBreakdown
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
        }



        private void RunAllRtpValidations()
        {
            foreach (var gameVariantRtp in _rtpBreakdownsByVariationThenWagerCategory)
            {
                foreach (var wagerCategoryRtp in gameVariantRtp.Value)
                {
                    ValidateRtpRangeBoundaries(wagerCategoryRtp.Value);

                    ValidatePrecision(wagerCategoryRtp.Value, RequiredLevelOfRtpPrecision);

                    ValidateGameLimits();

                    ValidateJurisdictionalLimits(wagerCategoryRtp.Value);

                    ValidateMachineAndOrHostLimits();
                }
            }
        }

        private void ValidateMachineAndOrHostLimits()
        {
            // TODO: Implement if needed
            throw new NotImplementedException();
        }

        private void ValidateGameLimits()
        {
            // TODO: Implement if needed
            throw new NotImplementedException();
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

        private void ValidateRtpRangeBoundaries(RtpBreakdown rtpBreakdown)
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

        private void ValidatePrecision(RtpBreakdown rtpBreakdown, int numOfDecimalPlaces)
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