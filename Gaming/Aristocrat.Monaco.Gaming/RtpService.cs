namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Contracts.Models;
    using Contracts.Rtp;
    using Kernel;

    public class RtpService : IRtpService, IService
    {
        private readonly IPropertiesManager _properties;
        private readonly Dictionary<GameType, RtpRules> _rules = new();

        public RtpService(IPropertiesManager propertiesManager)
        {
            _properties = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        public string Name => GetType().ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(IRtpService) };

        public void Initialize()
        {
            LoadRtpRules();
        }

        public decimal GetAverageRtp(IEnumerable<IGameProfile> games)
        {
            var rtpValuesForGames = new List<decimal>();

            foreach (var game in games)
            {
                foreach (var wagerCategory in game.WagerCategories)
                {
                    var rtpBreakdown = CreateRtpBreakdown(game.GameType, wagerCategory);

                    var averageGameRtp = (rtpBreakdown.TotalRtp.Minimum + rtpBreakdown.TotalRtp.Maximum) / 2.0m;

                    rtpValuesForGames.Add(averageGameRtp);
                }
            }

            var averageRtp = rtpValuesForGames.Average(rtp => rtp);

            return averageRtp;
        }

        public RtpRange GetTotalRtp(IEnumerable<IGameProfile> games)
        {
            var totalRtp = new RtpRange();

            foreach (var game in games)
            {
                foreach (var wagerCategory in game.WagerCategories)
                {
                    var rtpBreakdown = CreateRtpBreakdown(game.GameType, wagerCategory);

                    totalRtp = totalRtp.TotalWith(rtpBreakdown.TotalRtp);
                }
            }

            return totalRtp;
        }

        public RtpBreakdown GetRtpBreakdown(string wagerCategoryId, IGameProfile game)
        {
            var wagerCategory = game.WagerCategories.FirstOrDefault(w => w.Id.Equals(wagerCategoryId))
                ?? throw new ArgumentException(nameof(wagerCategoryId), $"No WagerCategory exists with id={wagerCategoryId}");

            var breakdown = CreateRtpBreakdown(game.GameType, wagerCategory);

            ValidateRtp(breakdown, game.GameType);

            return breakdown;
        }

        public RtpValidationReport ValidateRtp(IEnumerable<IGameProfile> games)
        {
            var validationDataForReport = new List<(IGameProfile game, RtpValidation validation)>();

            foreach (var game in games)
            {
                var resultEntries = new List<(string wagerCategoryId, RtpValidationResult validationResult)>();

                foreach (var wagerCategory in game.WagerCategories)
                {
                    var breakdown = CreateRtpBreakdown(game.GameType, wagerCategory);

                    ValidateRtp(breakdown, game.GameType);

                    resultEntries.Add((wagerCategory.Id, breakdown.ValidationResult));
                }

                var validation = new RtpValidation
                {
                    Game = game,
                    IsValid = resultEntries.All(r => r.validationResult.IsValid),
                    ValidationResults = resultEntries
                };

                validationDataForReport.Add((game, validation));
            } // End foreach game

            var validationReport = new RtpValidationReport(validationDataForReport);

            return validationReport;
        }

        /// <summary>
        ///     Creates a custom RTP breakdown, based on jurisdictional RTP rules, which is used to seed the final RTP calculation.
        /// </summary>
        /// <param name="gameType">Type of the game.</param>
        /// <param name="wagerCategory">The wager category used to populate the <see cref="RtpBreakdown" />.</param>
        /// <returns>An breakdown of RTP ranges, used in RTP calculation.</returns>
        private RtpBreakdown CreateRtpBreakdown(GameType gameType, IWagerCategory wagerCategory)
        {
            return new RtpBreakdown
            {
                Base = new RtpRange(wagerCategory.MinBaseRtpPercent, wagerCategory.MaxBaseRtpPercent),
                StandaloneProgressiveIncrement = _rules[gameType].IncludeStandaloneProgressiveIncrementRtp
                    ? new RtpRange(wagerCategory.SapIncrementRtpPercent, wagerCategory.SapIncrementRtpPercent)
                    : RtpRange.Zero,
                StandaloneProgressiveReset = _rules[gameType].IncludeStandaloneProgressiveStartUpRtp
                    ? new RtpRange(wagerCategory.MinSapStartupRtpPercent, wagerCategory.MaxSapStartupRtpPercent)
                    : RtpRange.Zero,
                LinkedProgressiveIncrement = _rules[gameType].IncludeLinkProgressiveIncrementRtp
                    ? new RtpRange(wagerCategory.LinkIncrementRtpPercent, wagerCategory.LinkIncrementRtpPercent)
                    : RtpRange.Zero,
                LinkedProgressiveReset = _rules[gameType].IncludeLinkProgressiveStartUpRtp
                    ? new RtpRange(wagerCategory.MinLinkStartupRtpPercent, wagerCategory.MaxLinkStartupRtpPercent)
                    : RtpRange.Zero,
            };
        }

        private void LoadRtpRules()
        {
            _rules[GameType.Slot] = new RtpRules
            {
                MinimumRtp = _properties.GetValue(GamingConstants.SlotMinimumReturnToPlayer, decimal.MinValue),
                MaximumRtp = _properties.GetValue(GamingConstants.SlotMaximumReturnToPlayer, decimal.MaxValue),
                IncludeLinkProgressiveIncrementRtp = _properties.GetValue(GamingConstants.SlotsIncludeLinkProgressiveIncrementRtp, false),
                IncludeLinkProgressiveStartUpRtp = _properties.GetValue(GamingConstants.SlotsIncludeLinkProgressiveStartUpRtp, false),
                IncludeStandaloneProgressiveIncrementRtp = _properties.GetValue(GamingConstants.SlotsIncludeStandaloneProgressiveIncrementRtp, true),
                IncludeStandaloneProgressiveStartUpRtp = _properties.GetValue(GamingConstants.SlotsIncludeStandaloneProgressiveStartUpRtp, false)
            };

            _rules[GameType.Blackjack] = new RtpRules
            {
                MinimumRtp = _properties.GetValue(GamingConstants.BlackjackMinimumReturnToPlayer, decimal.MinValue),
                MaximumRtp = _properties.GetValue(GamingConstants.BlackjackMaximumReturnToPlayer, decimal.MaxValue),
                IncludeLinkProgressiveIncrementRtp = _properties.GetValue(GamingConstants.BlackjackIncludeLinkProgressiveIncrementRtp, false),
                IncludeLinkProgressiveStartUpRtp = _properties.GetValue(GamingConstants.BlackjackIncludeLinkProgressiveStartUpRtp, false),
                IncludeStandaloneProgressiveIncrementRtp = _properties.GetValue(GamingConstants.BlackjackIncludeStandaloneProgressiveIncrementRtp, true),
                IncludeStandaloneProgressiveStartUpRtp = _properties.GetValue(GamingConstants.BlackjackIncludeStandaloneProgressiveStartUpRtp, false)
            };

            _rules[GameType.Keno] = new RtpRules
            {
                MinimumRtp = _properties.GetValue(GamingConstants.KenoMinimumReturnToPlayer, decimal.MinValue),
                MaximumRtp = _properties.GetValue(GamingConstants.KenoMaximumReturnToPlayer, decimal.MaxValue),
                IncludeLinkProgressiveIncrementRtp = _properties.GetValue(GamingConstants.KenoIncludeLinkProgressiveIncrementRtp, false),
                IncludeLinkProgressiveStartUpRtp = _properties.GetValue(GamingConstants.KenoIncludeLinkProgressiveStartUpRtp, false),
                IncludeStandaloneProgressiveIncrementRtp = _properties.GetValue(GamingConstants.KenoIncludeStandaloneProgressiveIncrementRtp, true),
                IncludeStandaloneProgressiveStartUpRtp = _properties.GetValue(GamingConstants.KenoIncludeStandaloneProgressiveStartUpRtp, false)
            };

            _rules[GameType.Roulette] = new RtpRules
            {
                MinimumRtp = _properties.GetValue(GamingConstants.RouletteMinimumReturnToPlayer, decimal.MinValue),
                MaximumRtp = _properties.GetValue(GamingConstants.RouletteMaximumReturnToPlayer, decimal.MaxValue),
                IncludeLinkProgressiveIncrementRtp = _properties.GetValue(GamingConstants.RouletteIncludeLinkProgressiveIncrementRtp, false),
                IncludeLinkProgressiveStartUpRtp = _properties.GetValue(GamingConstants.RouletteIncludeLinkProgressiveStartUpRtp, false),
                IncludeStandaloneProgressiveIncrementRtp = _properties.GetValue(GamingConstants.RouletteIncludeStandaloneProgressiveIncrementRtp, true),
                IncludeStandaloneProgressiveStartUpRtp = _properties.GetValue(GamingConstants.RouletteIncludeStandaloneProgressiveStartUpRtp, false)
            };
            
            // For games that didn't specify their type, presume they are Slot.
            _rules[GameType.Undefined] = _rules[GameType.Slot];
        }

        private void ValidateRtp(RtpBreakdown breakdown, GameType gameType)
        {
            ValidateRtpRangeBoundaries(breakdown);

            ValidatePrecision(breakdown, GamingConstants.NumberOfDecimalPlacesForRtpDisplay);

            ValidateGameLimits(breakdown);

            ValidateJurisdictionalLimits(breakdown, gameType);

            ValidateMachineAndOrHostLimits(breakdown);
        }

        private void ValidateMachineAndOrHostLimits(RtpBreakdown rtpBreakdown)
        {
            // TODO: Implement if needed
        }

        private void ValidateGameLimits(RtpBreakdown rtpBreakdown)
        {
            // TODO: Implement if needed
        }

        private void ValidateJurisdictionalLimits(RtpBreakdown rtpBreakdown, GameType gameType)
        {
            if (rtpBreakdown.TotalRtp.Minimum < _rules[gameType].MinimumRtp)
            {
                rtpBreakdown.ValidationResult.FailureFlags |= RtpValidationFailureFlags.RtpExceedsJurisdictionalMinimum;
            }
            if (rtpBreakdown.TotalRtp.Maximum > _rules[gameType].MaximumRtp)
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
                CheckRtpRange(rtpBreakdown.LinkedProgressiveIncrement) &&
                CheckRtpRange(rtpBreakdown.TotalRtp))
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
            return rtpRange.Maximum is >= decimal.Zero and <= 100m &&
                   rtpRange.Maximum >= rtpRange.Minimum &&
                   rtpRange.Minimum is >= decimal.Zero and <= 100m &&
                   rtpRange.Minimum <= rtpRange.Maximum;
        }
    }
}