namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using System;
    using Application.Contracts.Extensions;
    using Contracts;
    using Contracts.Models;
    using Contracts.Progressives;
    using Contracts.Rtp;
    using Kernel;
    using Progressives;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Application.Contracts.Localization;
    using Localization.Properties;
    using Monaco.UI.Common.Extensions;

    public class ReadOnlyGameConfiguration
    {
        private readonly double _denomMultiplier;
        private readonly IRtpService _rtpService;
        private readonly IProgressiveConfigurationProvider _progressiveConfigProvider;
        private readonly string _notAvailableLabel;
        private readonly string _notUsedLabel;
        private readonly string _notVerifiedLabel;

        public ReadOnlyGameConfiguration(
            IGameDetail game,
            long denom,
            double denomMultiplier,
            bool progressiveSetupConfigured = false)
        {
            GameDetail = game;
            DenominationValue = denom;
            _denomMultiplier = denomMultiplier;

            ProgressiveSetupConfigured = progressiveSetupConfigured;
            
            var container = ServiceManager.GetInstance().GetService<IContainerService>().Container;
            _rtpService = container.GetInstance<IRtpService>();
            _progressiveConfigProvider = container.GetInstance<IProgressiveConfigurationProvider>();
            var properties = container.GetInstance<IPropertiesManager>();

            _notAvailableLabel = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable);
            _notUsedLabel = RtpVerifiedState.NotUsed.GetDescription(typeof(RtpVerifiedState));
            _notVerifiedLabel = RtpVerifiedState.NotVerified.GetDescription(typeof(RtpVerifiedState));

            RuntimeVersion = Path.GetFileName(GameDetail.TargetRuntime);
            var denomination = GameDetail.Denominations.Single(d => d.Value == denom);
            var betOption = GameDetail.BetOptionList.FirstOrDefault(b => b.Name == denomination.BetOption);
            var lineOption = GameDetail.LineOptionList.FirstOrDefault(l => l.Name == denomination.LineOption);
            MaximumWagerCreditsValue = GameDetail.MaximumWagerCredits(betOption, lineOption);

            LinkedProgressiveRtpExists = game.HasExtendedRtpInformation &&
                                         (!_rtpService.GetTotalRtpBreakdown(game).LinkedProgressiveIncrement.Equals(RtpRange.Zero) ||
                                          !_rtpService.GetTotalRtpBreakdown(game).LinkedProgressiveReset.Equals(RtpRange.Zero));

            RtpDisplayValues = GenerateRtpDisplayValues(GameDetail, DenominationValue);
        }

        public bool LinkedProgressivesVerified => GameDetail.LinkedProgressiveVerified;

        public IGameDetail GameDetail { get; }

        public int Id => GameDetail.Id;

        public IEnumerable<string> AvailableDenominations =>
            GameDetail.SupportedDenominations.Select(d => $"{(d / _denomMultiplier).FormattedCurrencyString()}");

        public GameRtpDisplay RtpDisplayValues { get; }

        public string Denomination => $"{(DenominationValue / _denomMultiplier).FormattedCurrencyString()}";

        public string ThemeId => GameDetail.ThemeId;

        public string ThemeName => GameDetail.ThemeName;

        public string PaytableId => GameDetail.PaytableId;

        public string PaytableName => GameDetail.PaytableName;

        public string Version => GameDetail.Version;

        public long MaximumWagerCreditsValue { get; }

        public string MaximumWagerCredits =>
            $"{(MaximumWagerCreditsValue * DenominationValue / _denomMultiplier).FormattedCurrencyString()}";

        public long MaximumWinAmount => GameDetail.MaximumWinAmount; 

        public long TopAward => GameDetail.TopAward(GameDetail.Denominations.Single(x => x.Value == DenominationValue));

        public bool ProgressiveSetupConfigured { get; }

        public string RuntimeVersion { get; }

        public GameType GameType => GameDetail.GameType;

        public string GameSubtype => GameDetail.GameSubtype;

        public long DenominationValue { get; }

        public bool LinkedProgressiveRtpExists { get; }

        private GameRtpDisplay GenerateRtpDisplayValues(IGameDetail game, long denom)
        {
            var validationReport = _rtpService.ValidateGame(game);

            var betOption = game.GetBetOption(denom)?.Name;

            var (progressiveRtp, progressiveRtpState) = _progressiveConfigProvider.GetProgressivePackRtp(game.Id, denom, betOption);
            var hasSap = progressiveRtpState == RtpVerifiedState.Verified;

            var jurisdictionalRtpRules = _rtpService.GetJurisdictionalRtpRules(game.GameType);

            var standaloneProgressiveResetRtpState = !validationReport.IsValid
                ? RtpVerifiedState.NotAvailable
                : !jurisdictionalRtpRules.IncludeStandaloneProgressiveStartUpRtp
                    ? RtpVerifiedState.NotUsed
                    : !hasSap
                        ? RtpVerifiedState.NotAvailable
                        : RtpVerifiedState.Verified;

            var standaloneProgressiveIncrementRtpState = !validationReport.IsValid
                ? RtpVerifiedState.NotAvailable
                : !jurisdictionalRtpRules.IncludeStandaloneProgressiveIncrementRtp
                    ? RtpVerifiedState.NotUsed
                    : !hasSap
                        ? RtpVerifiedState.NotAvailable
                        : RtpVerifiedState.Verified;

            var linkedProgressiveResetRtpState = !validationReport.IsValid
                ? RtpVerifiedState.NotAvailable
                : !jurisdictionalRtpRules.IncludeLinkProgressiveStartUpRtp
                    ? RtpVerifiedState.NotUsed
                    : !LinkedProgressivesVerified
                        ? RtpVerifiedState.NotVerified
                        : RtpVerifiedState.Verified;

            var linkedProgressiveIncrementRtpState = !validationReport.IsValid
                ? RtpVerifiedState.NotAvailable
                : !jurisdictionalRtpRules.IncludeLinkProgressiveIncrementRtp
                    ? RtpVerifiedState.NotUsed
                    : !LinkedProgressivesVerified
                        ? RtpVerifiedState.NotVerified
                        : RtpVerifiedState.Verified;

            var progressiveRtpStateForGame = new ProgressiveRtpState
            {
                SapResetRtpState = standaloneProgressiveResetRtpState,
                SapIncrementRtpState = standaloneProgressiveIncrementRtpState,
                LpResetRtpState = linkedProgressiveResetRtpState,
                LpIncrementRtpState = linkedProgressiveIncrementRtpState
            };

            return game.HasExtendedRtpInformation
                ? CreateRtpDisplayFromExtendedRtpInfo(game, progressiveRtpStateForGame)
                : CreateRtpDisplayFromLegacyRtpInfo(game, progressiveRtp, progressiveRtpStateForGame);
        }

        private GameRtpDisplay CreateRtpDisplayFromExtendedRtpInfo( 
            IGameProfile game,
            ProgressiveRtpState rtpState) 
        { 
            var rtp = _rtpService.GetTotalRtpBreakdown(game); 
            
            var display = new GameRtpDisplay 
            { 
                HasExtendedRtpInformation = true, 
 
                BaseGameRtp = rtp.Base.ToString(), 
                BaseGameRtpMin = rtp.Base.Minimum.ToRtpString(), 
                BaseGameRtpMax = rtp.Base.Maximum.ToRtpString(), 
 
                StandaloneProgressiveResetRtp = DisplayValue(rtp.StandaloneProgressiveReset.ToString(), rtpState.SapResetRtpState), 
                StandaloneProgressiveResetRtpMin = DisplayValue(rtp.StandaloneProgressiveReset.Minimum.ToRtpString(), rtpState.SapResetRtpState), 
                StandaloneProgressiveResetRtpMax = DisplayValue(rtp.StandaloneProgressiveReset.Maximum.ToRtpString(), rtpState.SapResetRtpState), 
                StandaloneProgressiveIncrementRtp = DisplayValue(rtp.StandaloneProgressiveIncrement.ToString(), rtpState.SapIncrementRtpState), 
                StandaloneProgressiveIncrementRtpMin = DisplayValue(rtp.StandaloneProgressiveIncrement.Minimum.ToRtpString(), rtpState.SapIncrementRtpState), 
                StandaloneProgressiveIncrementRtpMax = DisplayValue(rtp.StandaloneProgressiveIncrement.Maximum.ToRtpString(), rtpState.SapIncrementRtpState), 
 
                LinkedProgressiveResetRtp = DisplayValue(rtp.LinkedProgressiveReset.ToString(), rtpState.LpResetRtpState), 
                LinkedProgressiveResetRtpMin = DisplayValue(rtp.LinkedProgressiveReset.Minimum.ToRtpString(), rtpState.LpResetRtpState), 
                LinkedProgressiveResetRtpMax = DisplayValue(rtp.LinkedProgressiveReset.Maximum.ToRtpString(), rtpState.LpResetRtpState), 
                LinkedProgressiveIncrementRtp = DisplayValue(rtp.LinkedProgressiveIncrement.ToString(), rtpState.LpIncrementRtpState),
                LinkedProgressiveIncrementRtpMin = DisplayValue(rtp.LinkedProgressiveIncrement.Minimum.ToRtpString(), rtpState.LpIncrementRtpState),
                LinkedProgressiveIncrementRtpMax = DisplayValue(rtp.LinkedProgressiveIncrement.Maximum.ToRtpString(), rtpState.LpIncrementRtpState),

                TotalRtp = rtp.TotalRtp.ToString(), 
                TotalRtpMin = rtp.TotalRtp.Minimum.ToRtpString(), 
                TotalRtpMax = rtp.TotalRtp.Maximum.ToRtpString(), 
 
                StandaloneProgressiveResetRtpState = rtpState.SapResetRtpState, 
                StandaloneProgressiveIncrementRtpState = rtpState.SapIncrementRtpState, 
                LinkedProgressiveResetRtpState = rtpState.LpResetRtpState, 
                LinkedProgressiveIncrementRtpState = rtpState.LpIncrementRtpState 
            }; 
 
            return display; 
        } 

        private GameRtpDisplay CreateRtpDisplayFromLegacyRtpInfo(
            IGameProfile game,
            ProgressiveRtp progressiveRtp,
            ProgressiveRtpState rtpState)
        {
            var baseGameRtp = new RtpRange(game.MinimumPaybackPercent, game.MaximumPaybackPercent);
            var sapResetRtp = progressiveRtp?.Reset;
            var sapIncrementRtp = progressiveRtp?.Increment;
            var totalRtp = baseGameRtp;

            if (sapResetRtp is not null)
            {
                totalRtp = totalRtp.TotalWith(sapResetRtp);
            }

            if (sapIncrementRtp is not null)
            {
                totalRtp = totalRtp.TotalWith(sapIncrementRtp);
            }

            var display = new GameRtpDisplay
            {
                HasExtendedRtpInformation = false,

                BaseGameRtp = baseGameRtp.ToString(),
                BaseGameRtpMin = game.MinimumPaybackPercent.ToRtpString(),
                BaseGameRtpMax = game.MaximumPaybackPercent.ToRtpString(),

                StandaloneProgressiveResetRtp = DisplayValue(sapResetRtp?.ToString(), rtpState.SapResetRtpState),
                StandaloneProgressiveResetRtpMin = DisplayValue(sapResetRtp?.Minimum.ToRtpString(), rtpState.SapResetRtpState),
                StandaloneProgressiveResetRtpMax = DisplayValue(sapResetRtp?.Maximum.ToRtpString(), rtpState.SapResetRtpState),
                StandaloneProgressiveIncrementRtp = DisplayValue(sapIncrementRtp?.ToString(), rtpState.SapIncrementRtpState),
                StandaloneProgressiveIncrementRtpMin = DisplayValue(sapIncrementRtp?.Minimum.ToRtpString(), rtpState.SapIncrementRtpState),
                StandaloneProgressiveIncrementRtpMax = DisplayValue(sapIncrementRtp?.Maximum.ToRtpString(), rtpState.SapIncrementRtpState),

                // Linked progressives not available in Monaco for legacy games
                LinkedProgressiveResetRtp = _notAvailableLabel,
                LinkedProgressiveResetRtpMin = _notAvailableLabel,
                LinkedProgressiveResetRtpMax = _notAvailableLabel,
                LinkedProgressiveIncrementRtp = _notAvailableLabel,
                LinkedProgressiveIncrementRtpMin = _notAvailableLabel,
                LinkedProgressiveIncrementRtpMax = _notAvailableLabel,

                TotalRtp = totalRtp.ToString(),
                TotalRtpMin = totalRtp.Minimum.ToRtpString(),
                TotalRtpMax = totalRtp.Maximum.ToRtpString(),

                StandaloneProgressiveResetRtpState = rtpState.SapResetRtpState,
                StandaloneProgressiveIncrementRtpState = rtpState.SapIncrementRtpState,
                LinkedProgressiveResetRtpState = RtpVerifiedState.NotAvailable,
                LinkedProgressiveIncrementRtpState = RtpVerifiedState.NotAvailable
            };

            return display;
        }

        private string DisplayValue(string value, RtpVerifiedState state)
        {
            return state switch
            {
                RtpVerifiedState.Verified => value,
                RtpVerifiedState.NotVerified => _notVerifiedLabel,
                RtpVerifiedState.NotUsed => _notUsedLabel,
                RtpVerifiedState.NotAvailable => _notAvailableLabel,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}