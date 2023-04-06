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

            BoostCheckEnabled = properties.GetValue(GamingConstants.BoostCheckEnabled, true);
            BoostCheckMatched = game.BoostCheckMatched;

            RtpDisplayValues = GenerateRtpDisplayValues(GameDetail, DenominationValue);
        }

        public bool BoostCheckMatched { get; }

        public bool BoostCheckEnabled { get; }

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
                    : !(BoostCheckEnabled && BoostCheckMatched)
                        ? RtpVerifiedState.NotVerified
                        : RtpVerifiedState.Verified;

            var linkedProgressiveIncrementRtpState = !validationReport.IsValid
                ? RtpVerifiedState.NotAvailable
                : !jurisdictionalRtpRules.IncludeLinkProgressiveIncrementRtp
                    ? RtpVerifiedState.NotUsed
                    : !(BoostCheckEnabled && BoostCheckMatched)
                        ? RtpVerifiedState.NotVerified
                        : RtpVerifiedState.Verified;

            var rtpDisplay = CreateRtpDisplay(
                game,
                standaloneProgressiveResetRtpState,
                standaloneProgressiveIncrementRtpState,
                linkedProgressiveResetRtpState,
                linkedProgressiveIncrementRtpState);

            return rtpDisplay;
        }

        private GameRtpDisplay CreateRtpDisplay(
            IGameProfile game,
            RtpVerifiedState sapResetRtpState,
            RtpVerifiedState sapIncrementRtpState,
            RtpVerifiedState lpResetRtpState,
            RtpVerifiedState lpIncrementRtpState)
        {
            var rtp = _rtpService.GetTotalRtpBreakdown(game);

            // TODO: Handle legacy games
            var display = new GameRtpDisplay
            {
                HasExtendedRtpInformation = game.HasExtendedRtpInformation, BaseGameRtp = rtp.Base.ToString(),
                BaseGameRtpMin = rtp.Base.Minimum.ToRtpString(),
                BaseGameRtpMax = rtp.Base.Maximum.ToRtpString(),
                StandaloneProgressiveResetRtp = DisplayValue(rtp.StandaloneProgressiveReset.ToString(), sapResetRtpState),
                StandaloneProgressiveResetRtpMin = DisplayValue(rtp.StandaloneProgressiveReset.Minimum.ToRtpString(), sapResetRtpState),
                StandaloneProgressiveResetRtpMax = DisplayValue(rtp.StandaloneProgressiveReset.Maximum.ToRtpString(), sapResetRtpState),
                StandaloneProgressiveIncrementRtp = DisplayValue(rtp.StandaloneProgressiveIncrement.ToString(), sapIncrementRtpState),
                StandaloneProgressiveIncrementRtpMin = DisplayValue(rtp.StandaloneProgressiveIncrement.Minimum.ToRtpString(), sapIncrementRtpState),
                StandaloneProgressiveIncrementRtpMax = DisplayValue(rtp.StandaloneProgressiveIncrement.Maximum.ToRtpString(), sapIncrementRtpState),
                LinkedProgressiveResetRtp = DisplayValue(rtp.LinkedProgressiveReset.ToString(), lpResetRtpState),
                LinkedProgressiveResetRtpMin = DisplayValue(rtp.LinkedProgressiveReset.Minimum.ToRtpString(), lpResetRtpState),
                LinkedProgressiveResetRtpMax = DisplayValue(rtp.LinkedProgressiveReset.Maximum.ToRtpString(), lpResetRtpState),
                LinkedProgressiveIncrementRtp = DisplayValue(rtp.LinkedProgressiveIncrement.ToString(), lpIncrementRtpState),
                LinkedProgressiveIncrementRtpMin = DisplayValue(rtp.LinkedProgressiveIncrement.Minimum.ToRtpString(), lpIncrementRtpState),
                LinkedProgressiveIncrementRtpMax = DisplayValue(rtp.LinkedProgressiveIncrement.Maximum.ToRtpString(), lpIncrementRtpState),
                TotalJurisdictionalRtp = rtp.TotalRtp.ToString(),
                TotalJurisdictionalRtpMin = rtp.TotalRtp.Minimum.ToRtpString(),
                TotalJurisdictionalRtpMax = rtp.TotalRtp.Maximum.ToRtpString(),
                StandaloneProgressiveResetRtpState = sapResetRtpState,
                StandaloneProgressiveIncrementRtpState = sapIncrementRtpState,
                LinkedProgressiveResetRtpState = lpResetRtpState,
                LinkedProgressiveIncrementRtpState = lpIncrementRtpState
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