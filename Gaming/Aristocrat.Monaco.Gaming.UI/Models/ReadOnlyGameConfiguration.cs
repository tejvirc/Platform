namespace Aristocrat.Monaco.Gaming.UI.Models
{
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

    public class ReadOnlyGameConfiguration
    {
        private readonly double _denomMultiplier;
        private readonly IRtpService _rtpService;
        private readonly IProgressiveConfigurationProvider _progressiveProvider;

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
            _progressiveProvider = container.GetInstance<IProgressiveConfigurationProvider>();

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

            // TODO: SAP & LP WIP
            //var (progressiveRtp, rtpState) = _progressiveProvider.GetProgressivePackRtp(game.Id, denom, game.GetBetOption(denom)?.Name);

            GameRtpDisplay rtpDisplay;

            if (game.HasExtendedRtpInformation)
            {
                var breakdown = _rtpService.GetTotalRtpBreakdown(game);

                //var sapResetRtpState = 
                //var sapIncrementRtpState = 
                //var lpResetRtpState =
                //var lpIncrementRtpState =

                rtpDisplay = new GameRtpDisplay
                {
                    HasExtendedRtpInformation = true,

                    BaseGameRtp = breakdown.Base.ToString(),
                    BaseGameRtpMin = breakdown.Base.Minimum.ToRtpString(),
                    BaseGameRtpMax = breakdown.Base.Maximum.ToRtpString(),

                    StandaloneProgressiveResetRtp = breakdown.StandaloneProgressiveReset.ToString(),
                    StandaloneProgressiveResetRtpMin = breakdown.StandaloneProgressiveReset.Minimum.ToRtpString(),
                    StandaloneProgressiveResetRtpMax = breakdown.StandaloneProgressiveReset.Maximum.ToRtpString(),

                    StandaloneProgressiveIncrementRtp = breakdown.StandaloneProgressiveIncrement.ToString(),
                    StandaloneProgressiveIncrementRtpMin = breakdown.StandaloneProgressiveIncrement.Minimum.ToRtpString(),
                    StandaloneProgressiveIncrementRtpMax = breakdown.StandaloneProgressiveIncrement.Maximum.ToRtpString(),

                    LinkedProgressiveResetRtp = breakdown.LinkedProgressiveReset.ToString(),
                    LinkedProgressiveResetRtpMin = breakdown.LinkedProgressiveReset.Minimum.ToRtpString(),
                    LinkedProgressiveResetRtpMax = breakdown.LinkedProgressiveReset.Maximum.ToRtpString(),

                    LinkedProgressiveIncrementRtp = breakdown.LinkedProgressiveIncrement.ToString(),
                    LinkedProgressiveIncrementRtpMin = breakdown.LinkedProgressiveIncrement.Minimum.ToRtpString(),
                    LinkedProgressiveIncrementRtpMax = breakdown.LinkedProgressiveIncrement.Maximum.ToRtpString(),

                    TotalJurisdictionalRtp = breakdown.TotalRtp.ToString(),
                    TotalJurisdictionalRtpMin = breakdown.TotalRtp.Minimum.ToRtpString(),
                    TotalJurisdictionalRtpMax = breakdown.TotalRtp.Maximum.ToRtpString(),

                    StandaloneProgressiveResetRtpState = validationReport.IsValid ? RtpVerifiedState.Verified : RtpVerifiedState.NotUsed,
                    StandaloneProgressiveIncrementRtpState = validationReport.IsValid ? RtpVerifiedState.Verified : RtpVerifiedState.NotUsed,
                    LinkedProgressiveResetRtpState = validationReport.IsValid ? RtpVerifiedState.Verified : RtpVerifiedState.NotUsed,
                    LinkedProgressiveIncrementRtpState = validationReport.IsValid ? RtpVerifiedState.Verified : RtpVerifiedState.NotUsed,
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

            //var sapResetState = !rules.IncludeStandaloneProgressiveStartUpRtp && rtpState == RtpVerifiedState.Verified
            //    ? RtpVerifiedState.NotUsed
            //    : rtpState;

            //var sapIncrementState = !rules.IncludeStandaloneProgressiveIncrementRtp && rtpState == RtpVerifiedState.Verified
            //    ? RtpVerifiedState.NotUsed
            //    : rtpState;

            //var lpResetState = !rules.IncludeLinkProgressiveStartUpRtp && rtpState == RtpVerifiedState.Verified
            //    ? RtpVerifiedState.NotUsed
            //    : rtpState;

            //var lpIncrementState = !rules.IncludeLinkProgressiveIncrementRtp && rtpState == RtpVerifiedState.Verified
            //    ? RtpVerifiedState.NotUsed
            //    : rtpState;


            return rtpDisplay;
        }
    }
}