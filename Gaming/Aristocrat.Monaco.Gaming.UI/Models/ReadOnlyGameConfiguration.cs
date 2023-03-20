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

            RuntimeVersion = Path.GetFileName(GameDetail.TargetRuntime);
            var denomination = GameDetail.Denominations.Single(d => d.Value == denom);
            var betOption = GameDetail.BetOptionList.FirstOrDefault(b => b.Name == denomination.BetOption);
            var lineOption = GameDetail.LineOptionList.FirstOrDefault(l => l.Name == denomination.LineOption);
            MaximumWagerCreditsValue = GameDetail.MaximumWagerCredits(betOption, lineOption);

            RtpDisplayValues = SetRtpDisplayValues(GameDetail, DenominationValue);
        }

        public IGameDetail GameDetail { get; }

        public int Id => GameDetail.Id;

        public long UniqueId => GameDetail.Denominations.Single(d => d.Value == DenominationValue).Id;

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

        private GameRtpDisplay SetRtpDisplayValues(IGameDetail game, long denom)
        {
            var validationReport = _rtpService.ValidateGame(game);

            var betOption = game.GetBetOption(denom)?.Name;

            var (progressiveRtp, progressiveRtpState) = _progressiveConfigProvider.GetProgressivePackRtp(game.Id, denom, betOption);
            var hasSap = progressiveRtpState == RtpVerifiedState.Verified;

            var standaloneProgressiveResetRtpState = !validationReport.IsValid
                ? RtpVerifiedState.NotAvailable
                : !_rtpService.GetJurisdictionalRtpRules(game.GameType).IncludeStandaloneProgressiveStartUpRtp
                    ? RtpVerifiedState.NotUsed
                    : !hasSap
                        ? RtpVerifiedState.NotAvailable
                        : RtpVerifiedState.Verified;

            var standaloneProgressiveIncrementRtpState = !validationReport.IsValid
                ? RtpVerifiedState.NotAvailable
                : !_rtpService.GetJurisdictionalRtpRules(game.GameType).IncludeStandaloneProgressiveIncrementRtp
                    ? RtpVerifiedState.NotUsed
                    : !hasSap
                        ? RtpVerifiedState.NotAvailable
                        : RtpVerifiedState.Verified;

            var notAvailableLocalized = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable);

            GameRtpDisplay rtpDisplay;

            if (game.HasExtendedRtpInformation)
            {
                var breakdown = _rtpService.GetTotalRtpBreakdown(game);

                rtpDisplay = new GameRtpDisplay
                {
                    HasExtendedRtpInformation = true,

                    StandaloneProgressiveResetRtpState = standaloneProgressiveResetRtpState,
                    StandaloneProgressiveIncrementRtpState = standaloneProgressiveIncrementRtpState,

                    LinkedProgressiveResetRtpState = RtpVerifiedState.NotAvailable,
                    BaseGameRtpMin = breakdown.Base.Minimum.ToRtpString(),
                    BaseGameRtpMax = breakdown.Base.Maximum.ToRtpString(),

                    // Standalone Progressive Reset
                    StandaloneProgressiveResetRtp = standaloneProgressiveResetRtpState switch
                    {
                        RtpVerifiedState.Verified => breakdown.StandaloneProgressiveReset.ToString(),
                        RtpVerifiedState.NotUsed => RtpVerifiedState.NotUsed.GetDescription(typeof(RtpVerifiedState)),
                        RtpVerifiedState.NotAvailable => notAvailableLocalized,
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    StandaloneProgressiveResetRtpMin = standaloneProgressiveResetRtpState switch
                    {
                        RtpVerifiedState.Verified => breakdown.StandaloneProgressiveReset.Minimum.ToRtpString(),
                        RtpVerifiedState.NotUsed => RtpVerifiedState.NotUsed.GetDescription(typeof(RtpVerifiedState)),
                        RtpVerifiedState.NotAvailable => notAvailableLocalized,
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    StandaloneProgressiveResetRtpMax = standaloneProgressiveResetRtpState switch
                    {
                        RtpVerifiedState.Verified => breakdown.StandaloneProgressiveReset.Maximum.ToRtpString(),
                        RtpVerifiedState.NotUsed => RtpVerifiedState.NotUsed.GetDescription(typeof(RtpVerifiedState)),
                        RtpVerifiedState.NotAvailable => notAvailableLocalized,
                        _ => throw new ArgumentOutOfRangeException()
                    },

                    // Standalone Progressive Increment
                    StandaloneProgressiveIncrementRtp = standaloneProgressiveIncrementRtpState switch
                    {
                        RtpVerifiedState.Verified => breakdown.StandaloneProgressiveIncrement.ToString(),
                        RtpVerifiedState.NotUsed => RtpVerifiedState.NotUsed.GetDescription(typeof(RtpVerifiedState)),
                        RtpVerifiedState.NotAvailable => notAvailableLocalized,
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    StandaloneProgressiveIncrementRtpMin = standaloneProgressiveIncrementRtpState switch
                    {
                        RtpVerifiedState.Verified => breakdown.StandaloneProgressiveIncrement.Minimum.ToRtpString(),
                        RtpVerifiedState.NotUsed => RtpVerifiedState.NotUsed.GetDescription(typeof(RtpVerifiedState)),
                        RtpVerifiedState.NotAvailable => notAvailableLocalized,
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    StandaloneProgressiveIncrementRtpMax = standaloneProgressiveIncrementRtpState switch
                    {
                        RtpVerifiedState.Verified => breakdown.StandaloneProgressiveIncrement.Maximum.ToRtpString(),
                        RtpVerifiedState.NotUsed => RtpVerifiedState.NotUsed.GetDescription(typeof(RtpVerifiedState)),
                        RtpVerifiedState.NotAvailable => notAvailableLocalized,
                        _ => throw new ArgumentOutOfRangeException()
                    },

                    // Linked Progressive Reset
                    LinkedProgressiveResetRtp = breakdown.LinkedProgressiveReset.ToString(),
                    LinkedProgressiveResetRtpMin = breakdown.LinkedProgressiveReset.Minimum.ToRtpString(),
                    LinkedProgressiveResetRtpMax = breakdown.LinkedProgressiveReset.Maximum.ToRtpString(),

                    // Linked Progressive Increment
                    LinkedProgressiveIncrementRtp = breakdown.LinkedProgressiveIncrement.ToString(),
                    LinkedProgressiveIncrementRtpMin = breakdown.LinkedProgressiveIncrement.Minimum.ToRtpString(),
                    LinkedProgressiveIncrementRtpMax = breakdown.LinkedProgressiveIncrement.Maximum.ToRtpString(),

                    TotalJurisdictionalRtp = breakdown.TotalRtp.ToString(),
                    TotalJurisdictionalRtpMin = breakdown.TotalRtp.Minimum.ToRtpString(),
                    TotalJurisdictionalRtpMax = breakdown.TotalRtp.Maximum.ToRtpString(),
                };
            }
            else
            {
                var totalRtp = _rtpService.GetTotalRtp(game);

                rtpDisplay = new GameRtpDisplay
                {
                    HasExtendedRtpInformation = false,

                    BaseGameRtp = totalRtp.ToString(),

                    TotalJurisdictionalRtp = totalRtp.ToString(),
                    TotalJurisdictionalRtpMin = totalRtp.Minimum.ToRtpString(),
                    TotalJurisdictionalRtpMax = totalRtp.Maximum.ToRtpString(),

                    StandaloneProgressiveResetRtpState = RtpVerifiedState.NotAvailable,
                    StandaloneProgressiveIncrementRtpState = RtpVerifiedState.NotAvailable,
                    LinkedProgressiveResetRtpState = RtpVerifiedState.NotAvailable,
                    LinkedProgressiveIncrementRtpState = RtpVerifiedState.NotAvailable,
                };
            }

            return rtpDisplay;
        }
    }
}