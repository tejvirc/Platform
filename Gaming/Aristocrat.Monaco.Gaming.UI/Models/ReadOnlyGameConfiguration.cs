namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using System.IO;
    using System.Linq;
    using Application.Contracts.Localization;
    using System;
    using Application.Contracts.Extensions;
    using Contracts;
    using Contracts.Models;
    using Contracts.Progressives;
    using Contracts.Rtp;
    using Localization.Properties;
    using System.Collections.Generic;
    using Common;

    public class ReadOnlyGameConfiguration
    {
        private readonly double _denomMultiplier;
        private readonly string _notAvailableLabel = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable);
        private readonly string _notUsedLabel = RtpVerifiedState.NotUsed.GetDescription(typeof(RtpVerifiedState));
        private readonly string _notVerifiedLabel = RtpVerifiedState.NotVerified.GetDescription(typeof(RtpVerifiedState));

        public ReadOnlyGameConfiguration(
            RtpBreakdown rtpBreakdown,
            IGameDetail game,
            long denom,
            double denomMultiplier,
            bool progressiveSetupConfigured = false)
        {
            GameDetail = game;
            DenominationValue = denom;
            _denomMultiplier = denomMultiplier;

            ProgressiveSetupConfigured = progressiveSetupConfigured;

            RuntimeVersion = Path.GetFileName(GameDetail.TargetRuntime);
            var denomination = GameDetail.Denominations.Single(d => d.Value == denom);
            var betOption = GameDetail.BetOptionList.FirstOrDefault(b => b.Name == denomination.BetOption);
            var lineOption = GameDetail.LineOptionList.FirstOrDefault(l => l.Name == denomination.LineOption);
            MaximumWagerCreditsValue = GameDetail.MaximumWagerCredits(betOption, lineOption);

            RtpDisplayValues = CreateRtpDisplay(rtpBreakdown);
        }

        public IGameDetail GameDetail { get; }

        public int Id => GameDetail.Id;
        
        public long UniqueId => GameDetail.Denominations.Single(d => d.Value == DenominationValue).Id;
        
        public IEnumerable<string> AvailableDenominations =>
            GameDetail.SupportedDenominations.Select(d => $"{(d / _denomMultiplier).FormattedCurrencyString()}");

        public GameRtpDisplay RtpDisplayValues { get; }

        public double Denomination => DenominationValue / _denomMultiplier;

        public string ThemeId => GameDetail.ThemeId;

        public string ThemeName => GameDetail.ThemeName;

        public string PaytableId => GameDetail.PaytableId;

        public string PaytableName => GameDetail.PaytableName;

        public string Version => GameDetail.Version;

        public long MaximumWagerCreditsValue { get; }

        public double MaximumWagerCredits => MaximumWagerCreditsValue * DenominationValue / _denomMultiplier;

        public long MaximumWinAmount => GameDetail.MaximumWinAmount; 

        public long TopAward => GameDetail.TopAward(GameDetail.Denominations.Single(x => x.Value == DenominationValue));

        public bool ProgressiveSetupConfigured { get; }

        public string RuntimeVersion { get; }

        public GameType GameType => GameDetail.GameType;

        public string GameSubtype => GameDetail.GameSubtype;

        public long DenominationValue { get; }

        private GameRtpDisplay CreateRtpDisplay(RtpBreakdown rtp)
        {
            var rtpStates = rtp.ProgressiveVerificationState;

            var rtpDisplay = new GameRtpDisplay 
            { 
                HasExtendedRtpInformation = GameDetail.HasExtendedRtpInformation,

                // Base RTP
                BaseGameRtp = rtp.Base.ToString(),

                BaseGameRtpMin = rtp.Base.Minimum.ToRtpString(),

                BaseGameRtpMax = rtp.Base.Maximum.ToRtpString(),

                // Standalone Progressive RTP
                StandaloneProgressiveResetRtp = GetRtpText(
                    rtp.StandaloneProgressiveReset.ToString(),
                    rtp.ProgressiveVerificationState.SapResetRtpState),

                StandaloneProgressiveResetRtpMin = GetRtpText(
                    rtp.StandaloneProgressiveReset.Minimum.ToRtpString(),
                    rtp.ProgressiveVerificationState.SapResetRtpState), 

                StandaloneProgressiveResetRtpMax = GetRtpText(
                    rtp.StandaloneProgressiveReset.Maximum.ToRtpString(),
                    rtpStates.SapResetRtpState), 

                StandaloneProgressiveIncrementRtp = GetRtpText(
                    rtp.StandaloneProgressiveIncrement.ToString(),
                    rtpStates.SapIncrementRtpState), 

                StandaloneProgressiveIncrementRtpMin = GetRtpText(
                    rtp.StandaloneProgressiveIncrement.Minimum.ToRtpString(),
                    rtpStates.SapIncrementRtpState), 

                StandaloneProgressiveIncrementRtpMax = GetRtpText(
                    rtp.StandaloneProgressiveIncrement.Maximum.ToRtpString(),
                    rtpStates.SapIncrementRtpState),

                // Linked Progressive RTP
                LinkedProgressiveResetRtp = GetRtpText(rtp.LinkedProgressiveReset.ToString(), rtpStates.LpResetRtpState),

                LinkedProgressiveResetRtpMin = GetRtpText(rtp.LinkedProgressiveReset.Minimum.ToRtpString(), rtpStates.LpResetRtpState),
                
                LinkedProgressiveResetRtpMax = GetRtpText(rtp.LinkedProgressiveReset.Maximum.ToRtpString(), rtpStates.LpResetRtpState),
                
                LinkedProgressiveIncrementRtp = GetRtpText(rtp.LinkedProgressiveIncrement.ToString(), rtpStates.LpIncrementRtpState),

                LinkedProgressiveIncrementRtpMin = GetRtpText(rtp.LinkedProgressiveIncrement.Minimum.ToRtpString(), rtpStates.LpIncrementRtpState),

                LinkedProgressiveIncrementRtpMax = GetRtpText(rtp.LinkedProgressiveIncrement.Maximum.ToRtpString(), rtpStates.LpIncrementRtpState),

                // Total
                TotalRtp = GameDetail.HasExtendedRtpInformation ? rtp.TotalRtp.ToString() : rtp.Base.ToString(),

                TotalRtpMin = GameDetail.HasExtendedRtpInformation ? rtp.TotalRtp.Minimum.ToRtpString() : rtp.Base.Minimum.ToString(),

                TotalRtpMax = GameDetail.HasExtendedRtpInformation ? rtp.TotalRtp.Maximum.ToRtpString() : rtp.Base.Maximum.ToString(), 

                // RTP Progressive States
                StandaloneProgressiveResetRtpState = rtpStates.SapResetRtpState,

                StandaloneProgressiveIncrementRtpState = rtpStates.SapIncrementRtpState,

                LinkedProgressiveResetRtpState = rtpStates.LpResetRtpState,

                LinkedProgressiveIncrementRtpState = rtpStates.LpIncrementRtpState 
            }; 
 
            return rtpDisplay; 
        }

        private string GetRtpText(string rtpText, RtpVerifiedState rtpState)
        {
            return rtpState switch
            {
                RtpVerifiedState.Verified => rtpText,
                RtpVerifiedState.NotVerified => _notVerifiedLabel,
                RtpVerifiedState.NotUsed => _notUsedLabel,
                RtpVerifiedState.NotAvailable => _notAvailableLabel,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static string GetRtpRangeString(RtpRange rtpRange, RtpVerifiedState state)
        {
            switch (state)
            {
                case RtpVerifiedState.NotAvailable:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable);
                case RtpVerifiedState.NotUsed:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotUsed);
                case RtpVerifiedState.NotVerified:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotVerified);
                default:
                    return rtpRange.ToString();
            }
        }

        private static string GetRtpString(decimal? rtp, RtpVerifiedState state)
        {
            switch (state)
            {
                case RtpVerifiedState.NotAvailable:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable);
                case RtpVerifiedState.NotUsed:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotUsed);
                case RtpVerifiedState.NotVerified:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotVerified);
                default:
                    return rtp?.ToRtpString() ?? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable);
            }
        }
    }
}