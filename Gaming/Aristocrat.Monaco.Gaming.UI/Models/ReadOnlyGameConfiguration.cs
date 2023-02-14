namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Contracts;
    using Contracts.Models;
    using Contracts.Progressives;
    using Contracts.Rtp;
    using Kernel;
    using Progressives;

    public class ReadOnlyGameConfiguration
    {
        private readonly IRtpService _rtpService;
        private readonly double _denomMultiplier;

        public ReadOnlyGameConfiguration(
            IGameDetail game,
            long denom,
            double denomMultiplier,
            bool progressiveSetupConfigured = false)
        {
            GameDetail = game;
            DenominationValue = denom;
            _denomMultiplier = denomMultiplier;

            var container = ServiceManager.GetInstance().GetService<IContainerService>().Container;
            _rtpService = container.GetInstance<IRtpService>();

            SetupProgressiveValues(container.GetInstance<IProgressiveConfigurationProvider>(), GameDetail, DenominationValue);

            RuntimeVersion = Path.GetFileName(GameDetail.TargetRuntime);
            var denomination = GameDetail.Denominations.Single(d => d.Value == denom);
            var betOption = GameDetail.BetOptionList.FirstOrDefault(b => b.Name == denomination.BetOption);
            var lineOption = GameDetail.LineOptionList.FirstOrDefault(l => l.Name == denomination.LineOption);
            MaximumWagerCreditsValue = GameDetail.MaximumWagerCredits(betOption, lineOption);

            ProgressiveSetupConfigured = progressiveSetupConfigured;
        }

        public IGameDetail GameDetail { get; }

        public int Id => GameDetail.Id;

        public long UniqueId => GameDetail.Denominations.Single(d => d.Value == DenominationValue).Id;

        public IEnumerable<string> AvailableDenominations =>
            GameDetail.SupportedDenominations.Select(d => $"{(d / _denomMultiplier).FormattedCurrencyString()}");

        public string Denomination => $"{(DenominationValue / _denomMultiplier).FormattedCurrencyString()}";

        public string ThemeId => GameDetail.ThemeId;

        public string ThemeName => GameDetail.ThemeName;

        public string PaytableId => GameDetail.PaytableId;

        public string PaytableName => GameDetail.PaytableName;

        public string Version => GameDetail.Version;

        public long MaximumWagerCreditsValue { get; }

        public string MaximumWagerCredits =>
            $"{(MaximumWagerCreditsValue * DenominationValue / _denomMultiplier).FormattedCurrencyString()}";

        public long TopAward => GameDetail.TopAward(GameDetail.Denominations.Single(x => x.Value == DenominationValue));

        public bool ProgressiveSetupConfigured { get; }

        public RtpVerifiedState StandaloneProgressiveResetRtpState { get; private set; }

        public RtpVerifiedState StandaloneProgressiveIncrementRtpState { get; private set; }

        public RtpVerifiedState LinkedProgressiveResetRtpState { get; private set; }

        public RtpVerifiedState LinkedProgressiveIncrementRtpState { get; private set; }

        public string RuntimeVersion { get; }

        public GameType GameType => GameDetail.GameType;

        public string GameSubtype => GameDetail.GameSubtype;

        public long DenominationValue { get; }

        private void SetupProgressiveValues(IProgressiveConfigurationProvider progressiveConfigurationProvider, IGameDetail game, long denom)
        {
            var totalRtpBreakdown = _rtpService.GetTotalRtpBreakdown(game);

            // TODO: handle bad RTP state enum here, not in GetRtp method.
            var (progressiveRtp, rtpState) =
                progressiveConfigurationProvider.GetProgressivePackRtp(game.Id, denom, game.GetBetOption(denom)?.Name);

            var progressiveResetRtp = totalRtpBreakdown.StandaloneProgressiveReset.TotalWith(totalRtpBreakdown.LinkedProgressiveReset);
            var progressiveIncrementRtp = totalRtpBreakdown.StandaloneProgressiveIncrement.TotalWith(totalRtpBreakdown.LinkedProgressiveIncrement);

            //var rules = _rtpService.GetJurisdictionalRtpRules(GameType);

            //var progressiveIncrement = rules.IncludeLinkProgressiveIncrementRtp ||
            //                           rules.IncludeStandaloneProgressiveIncrementRtp;

            //ProgressiveIncrementRTPState = (!progressiveIncrement && rtpState == RtpVerifiedState.Verified)
            //    ? RtpVerifiedState.NotUsed
            //    : rtpState;
        }
    }
}