namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives;
    using Common;
    using Progressives;
    using Contracts;
    using Contracts.Models;
    using Contracts.Rtp;
    using Kernel;
    using ProgressiveRtp = Contracts.Progressives.ProgressiveRtp;

    public class RtpService : IRtpService, IService
    {
        private readonly IPropertiesManager _properties;
        private readonly IProgressiveConfigurationProvider _progressiveConfigProvider;
        private readonly Dictionary<GameType, RtpRules> _rules = new ();

        public RtpService(
            IPropertiesManager propertiesManager,
            IProgressiveConfigurationProvider progressiveConfigProvider)
        {
            _properties = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _progressiveConfigProvider = progressiveConfigProvider ??
                                         throw new ArgumentNullException(nameof(progressiveConfigProvider));
        }

        public string Name => GetType().ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(IRtpService) };

        public void Initialize()
        {
            LoadRtpCalculationRules();
        }

        public decimal GetAverageRtp(IGameDetail game) => GetAverageRtp(new[] { game });

        public decimal GetAverageRtp(IEnumerable<IGameDetail> games)
        {
            var rtpAveragesOfGames = games.Select(GetAverageRtpInternal);

            var averageOfTheAverages = rtpAveragesOfGames.Average();

            return averageOfTheAverages;

            decimal GetAverageRtpInternal(IGameDetail game)
            {
                var wagerCategoryRtpAverages = new List<decimal>();

                foreach (var wagerCategory in game.WagerCategories)
                {
                    var wagerCategoryTotalRtp = GetRtpBreakdownForWagerCategory(game, wagerCategory.Id).TotalRtp;

                    wagerCategoryRtpAverages.Add((wagerCategoryTotalRtp.Minimum + wagerCategoryTotalRtp.Maximum) / 2.0m);
                }

                return wagerCategoryRtpAverages.Average();
            }
        }

        public RtpRange GetTotalRtp(IGameDetail game) => GetTotalRtp(new[] { game });

        public RtpRange GetTotalRtp(IEnumerable<IGameDetail> games)
        {
            var totalRtp = RtpRange.Zero;

            foreach (var game in games)
            {
                RtpRange rtp;

                // Handle new RTP information (Approx. GDK 5.0 onward)
                if (game.HasExtendedRtpInformation)
                {
                    var totalGameRtp = new RtpRange();

                    rtp = totalGameRtp;
                    foreach (var wagerCategory in game.WagerCategories)
                    {
                        var breakdown = GetRtpBreakdownForWagerCategory(game, wagerCategory.Id);
                        rtp = rtp.GetTotalWith(breakdown.TotalRtp);
                    }
                }
                // Handle legacy RTP information. This block is for backwards compatibility only.
                // It includes all games up to GDK 5.0. Also included, is a handful of GDK 5 games
                // that still have the legacy RTP format. Those are handled here too.
                else 
                {
                    rtp = new RtpRange(game.MinimumPaybackPercent, game.MaximumPaybackPercent);
                }
                
                totalRtp = totalRtp.GetTotalWith(rtp);
            }

            return totalRtp;
        }

        public RtpBreakdown GetTotalRtpBreakdown(IGameDetail game)
        {
            RtpBreakdown result = null;
            bool first = true;
            foreach (var w in game.WagerCategories)
            {
                var breakdown = GetRtpBreakdownForWagerCategory(game, w.Id);
                if (first)
                {
                    first = false;
                    result = breakdown;
                    continue;
                }

                result = RtpBreakdown.GetTotal(result, breakdown);
            }

            return result;
        }

        public RtpBreakdown GetRtpBreakdownForWagerCategory(IGameDetail game, string wagerCategoryId)
        {
            // Gather information about any Standalone Progressives because it is used to populate RTP values in legacy games.
            var (sapRtpState, progressiveRtp) = GetSapInfoFromProgressiveProvider(game);

            // Build up a empty breakdown with error flags zeroed out.
            var wagerCategory = game.WagerCategories.Single(w => w.Id.Equals(wagerCategoryId, StringComparison.Ordinal));

            var rtpBreakdown = new RtpBreakdown
            {
                Base = new RtpRange(wagerCategory.MinBaseRtpPercent, wagerCategory.MaxBaseRtpPercent),
                FailureFlags = RtpValidationFailureFlags.None,
            };

            if (game.HasExtendedRtpInformation)
            {
                rtpBreakdown.StandaloneProgressiveIncrement = new RtpRange(wagerCategory.SapIncrementRtpPercent, wagerCategory.SapIncrementRtpPercent);
                rtpBreakdown.StandaloneProgressiveReset = new RtpRange(wagerCategory.MinSapStartupRtpPercent, wagerCategory.MaxSapStartupRtpPercent);
                rtpBreakdown.LinkedProgressiveIncrement = new RtpRange(wagerCategory.LinkIncrementRtpPercent, wagerCategory.LinkIncrementRtpPercent);
                rtpBreakdown.LinkedProgressiveReset = new RtpRange(wagerCategory.MinLinkStartupRtpPercent, wagerCategory.MaxLinkStartupRtpPercent);
            }
            else
            {
                // Populate game SAP RTP values from the progressive provider's RTP. This is the legacy way of doing it.
                // It's for backwards support. Games with the New RTP values include SAP RTP in the game config.
                rtpBreakdown.StandaloneProgressiveIncrement = progressiveRtp?.Increment ?? RtpRange.Zero;
                rtpBreakdown.StandaloneProgressiveReset = progressiveRtp?.Reset ?? RtpRange.Zero;
                rtpBreakdown.LinkedProgressiveIncrement = RtpRange.Zero;
                rtpBreakdown.LinkedProgressiveReset = RtpRange.Zero;
            }

            // Verify SAP and LP RTP.
            var isLpVerified = (game.LinkedProgressiveVerificationComplete
                ? game.LinkedProgressiveVerificationResult
                : false) ?? false;
            var isSapVerified = sapRtpState == RtpVerifiedState.Verified;
            
            // Value validations will set error flags on RtpBreakdown object and change invalid rtp values to zero.
            if (game.HasExtendedRtpInformation)
            {
                ValidateRtpRangeBoundaries(rtpBreakdown);

                ValidatePrecision(rtpBreakdown, GamingConstants.NumberOfDecimalPlacesForRtpCalculations);
            }

            ValidateJurisdictionalLimits(rtpBreakdown, game.GameType);

            ValidateJurisdictionalRtpRules(rtpBreakdown, game.GameType);
            
            // Build up and assign RTP Verification states to breakdown, for use with UI mainly.
            rtpBreakdown.ProgressiveVerificationState = game.HasExtendedRtpInformation
                ? GenerateRtpVerificationStates(game, rtpBreakdown.IsValid, isSapVerified, isLpVerified)
                : GenerateRtpVerificationStatesLegacy(game, sapRtpState);

            return rtpBreakdown;
        }

        public RtpRules GetJurisdictionalRtpRules(GameType gameType) => _rules[gameType];

        private (RtpVerifiedState, ProgressiveRtp) GetSapInfoFromProgressiveProvider(IGameDetail game)
        {
            var denom = game.ActiveDenominations.Any()
                ? game.ActiveDenominations.First()
                : game.SupportedDenominations.First();

            var betOption = game.GetBetOption(denom)?.Name;
            var (progressiveRtp, rtpState) =
                _progressiveConfigProvider.GetProgressivePackRtp(game.Id, denom, betOption);

            return (rtpState, progressiveRtp);
        }

        private ProgressiveRtpVerificationState GenerateRtpVerificationStates(
            IGameDetail game,
            bool isRtpValueValid,
            bool isSapVerified,
            bool isLpVerified)
        {
            var jurisdictionalRtpRules = _rules[game.GameType];

            // NOTE ->                                                           Can be read like this:
            var standaloneProgressiveResetRtpState = !isRtpValueValid            // if
                ? RtpVerifiedState.NotAvailable                                  // then
                : !jurisdictionalRtpRules.IncludeStandaloneProgressiveStartUpRtp // else if
                    ? RtpVerifiedState.NotUsed                                   // then
                    : !isSapVerified                                             // else if
                        ? RtpVerifiedState.NotAvailable                          // then
                        : RtpVerifiedState.Verified;                             // else

            var standaloneProgressiveIncrementRtpState = !isRtpValueValid
                ? RtpVerifiedState.NotAvailable
                : !jurisdictionalRtpRules.IncludeStandaloneProgressiveIncrementRtp
                    ? RtpVerifiedState.NotUsed
                    : !isSapVerified
                        ? RtpVerifiedState.NotAvailable
                        : RtpVerifiedState.Verified;

            var linkedProgressiveResetRtpState = !isRtpValueValid
                ? RtpVerifiedState.NotAvailable
                : !jurisdictionalRtpRules.IncludeLinkProgressiveStartUpRtp
                    ? RtpVerifiedState.NotUsed
                    : !isLpVerified
                        ? RtpVerifiedState.NotVerified
                        : RtpVerifiedState.Verified;

            var linkedProgressiveIncrementRtpState = !isRtpValueValid
                ? RtpVerifiedState.NotAvailable
                : !jurisdictionalRtpRules.IncludeLinkProgressiveIncrementRtp
                    ? RtpVerifiedState.NotUsed
                    : !isLpVerified
                        ? RtpVerifiedState.NotVerified
                        : RtpVerifiedState.Verified;

            var rtpVerificationStates = new ProgressiveRtpVerificationState
            {
                SapResetRtpState = standaloneProgressiveResetRtpState,
                SapIncrementRtpState = standaloneProgressiveIncrementRtpState,
                LpResetRtpState = linkedProgressiveResetRtpState,
                LpIncrementRtpState = linkedProgressiveIncrementRtpState
            };

            return rtpVerificationStates;
        }

        /// <summary>
        ///     This RTP Verification logic matches that of the legacy games. These games existed before the new RTP Reporting
        ///     feature which allowed games to offer more detailed RTP information. This methods logic is the exact same as it
        ///     was before the New RTP Reporting.
        /// </summary>
        /// <param name="game">The game to generate validation states for.</param>
        /// <param name="sapRtpState"></param>
        /// <returns>The RTP verification states for the given game</returns>
        private ProgressiveRtpVerificationState GenerateRtpVerificationStatesLegacy(
            IGameDetail game,
            RtpVerifiedState sapRtpState)
        {
            var jurisdictionalRtpRules = _rules[game.GameType];

            var rtpVerificationState = new ProgressiveRtpVerificationState
            {
                SapResetRtpState = sapRtpState,
                SapIncrementRtpState = !jurisdictionalRtpRules.IncludeStandaloneProgressiveIncrementRtp &&
                                       sapRtpState == RtpVerifiedState.Verified
                    ? RtpVerifiedState.NotUsed
                    : sapRtpState,
                LpResetRtpState = RtpVerifiedState.NotAvailable,
                LpIncrementRtpState = RtpVerifiedState.NotAvailable
            };

            return rtpVerificationState;
        }

        private void LoadRtpCalculationRules()
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
            _rules[GameType.Poker] = new RtpRules
            {
                MinimumRtp = _properties.GetValue(GamingConstants.PokerMinimumReturnToPlayer, decimal.MinValue),
                MaximumRtp = _properties.GetValue(GamingConstants.PokerMaximumReturnToPlayer, decimal.MaxValue),
                IncludeLinkProgressiveIncrementRtp = _properties.GetValue(GamingConstants.PokerIncludeLinkProgressiveIncrementRtp, false),
                IncludeLinkProgressiveStartUpRtp = _properties.GetValue(GamingConstants.PokerIncludeLinkProgressiveStartUpRtp, false),
                IncludeStandaloneProgressiveIncrementRtp = _properties.GetValue(GamingConstants.PokerIncludeStandaloneProgressiveIncrementRtp, true),
                IncludeStandaloneProgressiveStartUpRtp = _properties.GetValue(GamingConstants.PokerIncludeStandaloneProgressiveStartUpRtp, false)
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
            _rules[GameType.Blackjack] = new RtpRules
            {
                MinimumRtp = _properties.GetValue(GamingConstants.BlackjackMinimumReturnToPlayer, decimal.MinValue),
                MaximumRtp = _properties.GetValue(GamingConstants.BlackjackMaximumReturnToPlayer, decimal.MaxValue),
                IncludeLinkProgressiveIncrementRtp = _properties.GetValue(GamingConstants.BlackjackIncludeLinkProgressiveIncrementRtp, false),
                IncludeLinkProgressiveStartUpRtp = _properties.GetValue(GamingConstants.BlackjackIncludeLinkProgressiveStartUpRtp, false),
                IncludeStandaloneProgressiveIncrementRtp = _properties.GetValue(GamingConstants.BlackjackIncludeStandaloneProgressiveIncrementRtp, true),
                IncludeStandaloneProgressiveStartUpRtp = _properties.GetValue(GamingConstants.BlackjackIncludeStandaloneProgressiveStartUpRtp, false)
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

        private bool VerifyLinkedProgressiveRtp(IGameDetail game, RtpBreakdown rtpBreakdown)
        {
            // TODO: To be implemented soon by another team (Most likely James King's)

            return false;
        }

        /// <summary>
        ///     Excludes individual RTP contributions from total RTP calculation. These RTP exclusions
        ///     are configured in the jurisdictional configuration.
        /// </summary>
        /// <param name="breakdown">The RTP Breakdown</param>
        /// <param name="gameType">The GameType</param>
        private void ValidateJurisdictionalRtpRules(RtpBreakdown breakdown, GameType gameType)
        {
            var rtpRules = _rules[gameType];

            if (!rtpRules.IncludeStandaloneProgressiveStartUpRtp)
            {
                breakdown.StandaloneProgressiveReset = RtpRange.Zero;
            }

            if (!rtpRules.IncludeStandaloneProgressiveIncrementRtp)
            {
                breakdown.StandaloneProgressiveIncrement = RtpRange.Zero;
            }

            if (!rtpRules.IncludeLinkProgressiveStartUpRtp)
            {
                breakdown.LinkedProgressiveReset = RtpRange.Zero;
            }

            if (!rtpRules.IncludeLinkProgressiveIncrementRtp)
            {
                breakdown.StandaloneProgressiveReset = RtpRange.Zero;
            }
        }

        private void ValidateJurisdictionalLimits(RtpBreakdown rtpBreakdown, GameType gameType)
        {
            if (rtpBreakdown.TotalRtp.Minimum < _rules[gameType].MinimumRtp)
            {
                rtpBreakdown.FailureFlags |= RtpValidationFailureFlags.RtpExceedsJurisdictionalMinimum;
            }
            if (rtpBreakdown.TotalRtp.Maximum > _rules[gameType].MaximumRtp)
            {
                rtpBreakdown.FailureFlags |= RtpValidationFailureFlags.RtpExceedsJurisdictionalMaximum;
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

            rtpBreakdown.FailureFlags |= RtpValidationFailureFlags.InvalidRtpValue;
        }

        private void ValidatePrecision(RtpBreakdown rtpBreakdown, int numOfDecimalPlaces)
        {
            if (rtpBreakdown.Base.Minimum.CheckPrecision(numOfDecimalPlaces) &&
               rtpBreakdown.Base.Maximum.CheckPrecision(numOfDecimalPlaces) &&
               rtpBreakdown.StandaloneProgressiveReset.Minimum.CheckPrecision(numOfDecimalPlaces) &&
               rtpBreakdown.StandaloneProgressiveReset.Maximum.CheckPrecision(numOfDecimalPlaces) &&
               rtpBreakdown.StandaloneProgressiveIncrement.Minimum.CheckPrecision(numOfDecimalPlaces) &&
               rtpBreakdown.StandaloneProgressiveIncrement.Maximum.CheckPrecision(numOfDecimalPlaces) &&
               rtpBreakdown.LinkedProgressiveReset.Minimum.CheckPrecision(numOfDecimalPlaces) &&
               rtpBreakdown.LinkedProgressiveReset.Maximum.CheckPrecision(numOfDecimalPlaces) &&
               rtpBreakdown.LinkedProgressiveIncrement.Minimum.CheckPrecision(numOfDecimalPlaces) &&
               rtpBreakdown.LinkedProgressiveIncrement.Maximum.CheckPrecision(numOfDecimalPlaces))
            {
                return;
            }

            rtpBreakdown.FailureFlags |= RtpValidationFailureFlags.InsufficientRtpPrecision;
        }

        private static bool CheckRtpRange(RtpRange rtpRange)
        {
            return rtpRange.Maximum >= decimal.Zero &&
                   rtpRange.Maximum >= rtpRange.Minimum &&
                   rtpRange.Minimum >= decimal.Zero &&
                   rtpRange.Minimum <= rtpRange.Maximum;
        }
    }
}