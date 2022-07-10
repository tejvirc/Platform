namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Contracts;
    using Contracts.Models;
    using Contracts.Progressives;
    using Kernel;
    using Progressives;

    public class ReadOnlyGameConfiguration
    {
        private readonly IGameRtpService _gameRtpService;

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
            _gameRtpService = container.GetInstance<IGameRtpService>();

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

        public long MaximumWinAmount => GameDetail.MaximumWinAmount;

        public long TopAward => GameDetail.TopAward(GameDetail.Denominations.Single(x => x.Value == DenominationValue));

        public string BaseGameRTP { get; private set; }

        public string BaseGameRTPMin { get; private set; }

        public string BaseGameRTPMax { get; private set; }

        public string ProgressiveResetRTP { get; private set; }

        public string ProgressiveResetRTPMin { get; private set; }

        public string ProgressiveResetRTPMax { get; private set; }

        public string ProgressiveIncrementRTP { get; private set; }

        public string ProgressiveIncrementRTPMin { get; private set; }

        public string ProgressiveIncrementRTPMax { get; private set; }

        public bool ProgressiveSetupConfigured { get; }

        public string TotalJurisdictionalRTP { get; private set; }

        public string TotalJurisdictionalRTPMin { get; private set; }

        public string TotalJurisdictionalRTPMax { get; private set; }

        public RtpVerifiedState ProgressiveResetRTPState { get; private set; }

        public RtpVerifiedState ProgressiveIncrementRTPState { get; private set; }

        public string RuntimeVersion { get; }

        public GameType GameType => GameDetail.GameType;

        public string GameSubtype => GameDetail.GameSubtype;

        public long DenominationValue { get; }

        private void SetupProgressiveValues(IProgressiveConfigurationProvider progressiveConfigurationProvider, IGameDetail game, long denom)
        {
            var baseGameRtp = game.GetBaseGameRtpRange();

            var (progressiveRtp, rtpState) =
                progressiveConfigurationProvider.GetProgressivePackRtp(game.Id, denom, game.GetBetOption(denom)?.Name);

            var (totalJurisdictionRtp, _) = game.GetTotalJurisdictionRtpRange((progressiveRtp, rtpState));

            SetRtpInformation(
                baseGameRtp,
                progressiveRtp?.Reset,
                progressiveRtp?.Increment,
                totalJurisdictionRtp,
                rtpState);
        }

        private void SetRtpInformation(
            RtpRange baseGameRtp,
            RtpRange progressiveResetRtp,
            RtpRange progressiveIncrementRtp,
            RtpRange totalJurisdictionRtp,
            RtpVerifiedState rtpState)
        {
            BaseGameRTP = GameConfigHelper.GetRtpRangeString(baseGameRtp);
            BaseGameRTPMin = baseGameRtp.Minimum.GetRtpString();
            BaseGameRTPMax = baseGameRtp.Maximum.GetRtpString();

            ProgressiveResetRTP = progressiveResetRtp?.ToString();
            ProgressiveResetRTPMin = progressiveResetRtp?.Minimum.GetRtpString();
            ProgressiveResetRTPMax = progressiveResetRtp?.Maximum.GetRtpString();

            ProgressiveIncrementRTP = progressiveIncrementRtp?.ToString();
            ProgressiveIncrementRTPMin = progressiveIncrementRtp?.Minimum.GetRtpString();
            ProgressiveIncrementRTPMax = progressiveIncrementRtp?.Maximum.GetRtpString();

            TotalJurisdictionalRTP = totalJurisdictionRtp?.ToString();
            TotalJurisdictionalRTPMin = totalJurisdictionRtp?.Minimum.GetRtpString();
            TotalJurisdictionalRTPMax = totalJurisdictionRtp?.Maximum.GetRtpString();

            ProgressiveResetRTPState = rtpState;
            ProgressiveIncrementRTPState = !_gameRtpService.CanIncludeIncrementRtp(GameType) && rtpState == RtpVerifiedState.Verified
                ? RtpVerifiedState.NotUsed : rtpState;
        }
    }
}