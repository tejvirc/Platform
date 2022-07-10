namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Aristocrat.PackageManifest.Extension.v100;
    //using Aristocrat.PackageManifest.Extension.v100;
    using Contracts;
    using Contracts.Models;
    using Kernel;
    using log4net;
    using PackageManifest.Models;

    public class GameRtpService : IGameRtpService, IService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly IPropertiesManager _properties;
        private readonly Dictionary<GameType,
            (bool includeSapIncr, bool includeLinkIncr, decimal minPayback, decimal maxPayback)> _rtpRules = new();

        public GameRtpService(IPropertiesManager properties)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public string Name => GetType().ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(IGameRtpService) };

        public void Initialize()
        {
            _rtpRules[GameType.Slot] = (
                _properties.GetValue(GamingConstants.SlotsIncludeStandaloneProgressiveIncrementRtp, false),
                _properties.GetValue(GamingConstants.SlotsIncludeLinkProgressiveIncrementRtp, false),
                _properties.GetValue(GamingConstants.SlotMinimumReturnToPlayer, decimal.MinValue),
                _properties.GetValue(GamingConstants.SlotMaximumReturnToPlayer, decimal.MaxValue));
            _rtpRules[GameType.Poker] = (
                _properties.GetValue(GamingConstants.PokerIncludeStandaloneProgressiveIncrementRtp, false),
                _properties.GetValue(GamingConstants.PokerIncludeLinkProgressiveIncrementRtp, false),
                _properties.GetValue(GamingConstants.PokerMinimumReturnToPlayer, decimal.MinValue),
                _properties.GetValue(GamingConstants.PokerMaximumReturnToPlayer, decimal.MaxValue));
            _rtpRules[GameType.Keno] = (
                _properties.GetValue(GamingConstants.KenoIncludeStandaloneProgressiveIncrementRtp, false),
                _properties.GetValue(GamingConstants.KenoIncludeLinkProgressiveIncrementRtp, false),
                _properties.GetValue(GamingConstants.KenoMinimumReturnToPlayer, decimal.MinValue),
                _properties.GetValue(GamingConstants.KenoMaximumReturnToPlayer, decimal.MaxValue));
            _rtpRules[GameType.Blackjack] = (
                _properties.GetValue(GamingConstants.BlackjackIncludeStandaloneProgressiveIncrementRtp, false),
                _properties.GetValue(GamingConstants.BlackjackIncludeLinkProgressiveIncrementRtp, false),
                _properties.GetValue(GamingConstants.BlackjackMinimumReturnToPlayer, decimal.MinValue),
                _properties.GetValue(GamingConstants.BlackjackMaximumReturnToPlayer, decimal.MaxValue));
            _rtpRules[GameType.Roulette] = (
                _properties.GetValue(GamingConstants.RouletteIncludeStandaloneProgressiveIncrementRtp, false),
                _properties.GetValue(GamingConstants.RouletteIncludeLinkProgressiveIncrementRtp, false),
                _properties.GetValue(GamingConstants.RouletteMinimumReturnToPlayer, decimal.MinValue),
                _properties.GetValue(GamingConstants.RouletteMaximumReturnToPlayer, decimal.MaxValue));
        }

        public bool CanIncludeIncrementRtp(GameType type)
        {
            return CanIncludeSapIncrementRtp(type) ||
                   CanIncludeLinkProgressiveIncrementRtp(type);
        }

        public bool CanIncludeSapIncrementRtp(GameType type)
        {
            return _rtpRules[type].includeSapIncr;
        }

        public bool CanIncludeLinkProgressiveIncrementRtp(GameType type)
        {
            return _rtpRules[type].includeLinkIncr;
        }

        public RtpRange GetTotalRtp(GameAttributes game, IReadOnlyCollection<ProgressiveDetail> progressiveDetails)
        {
            var gameType = ToGameType(game.GameType);
            var includeIncrement = CanIncludeIncrementRtp(gameType);

            var categories = game.WagerCategories.ToList();
            var minBaseRtp = categories.Select(w => w.MinBaseRtpPercent).Min();
            var maxBaseRtp = categories.Select(w => w.MaxBaseRtpPercent).Max();
            var minProgStartupRtp = categories.Select(w => w.MinSapStartupRtpPercent)
                .Union(categories.Select(w => w.MinLinkStartupRtpPercent)).Min();
            var maxProgStartupRtp = categories.Select(w => w.MaxSapStartupRtpPercent)
                .Union(categories.Select(w => w.MaxLinkStartupRtpPercent)).Max();
            var progIncrementRtps = categories.Select(w => w.SapIncrementRtpPercent)
                .Union(categories.Select(w => w.LinkIncrementRtpPercent)).ToList();
            var minProgIncrementRtp = includeIncrement ? progIncrementRtps.Min() : 0;
            var maxProgIncrementRtp = includeIncrement ? progIncrementRtps.Max() : 0;
            Logger.Debug($"minBase={minBaseRtp}% maxBase={maxBaseRtp}% minProgStart={minProgStartupRtp}% maxProgStart={maxProgStartupRtp}% minProgIncr={minProgIncrementRtp}% maxProgIncr={maxProgIncrementRtp}%");

            var totalRtpMin = minBaseRtp + minProgStartupRtp + minProgIncrementRtp;
            var totalRtpMax = maxBaseRtp + maxProgStartupRtp + maxProgIncrementRtp;
            Logger.Debug($"minTotal={totalRtpMin}% maxTotal={totalRtpMax}%");

            // old-style game manifest?
            if (minBaseRtp == 0)
            {
                var returnToPlayer = progressiveDetails?.FirstOrDefault()?.ReturnToPlayer;

                totalRtpMin = (includeIncrement
                    ? returnToPlayer?.BaseRtpAndResetRtpAndIncRtpMin
                    : returnToPlayer?.BaseRtpAndResetRtpMin) ?? game.MinPaybackPercent;
                totalRtpMax = (includeIncrement
                    ? returnToPlayer?.BaseRtpAndResetRtpAndIncRtpMax
                    : returnToPlayer?.BaseRtpAndResetRtpMax) ?? game.MaxPaybackPercent;
                Logger.Debug($"Alt minTotal={totalRtpMin}% maxTotal={totalRtpMax}%");
            }

            return new RtpRange(totalRtpMin, totalRtpMax);
        }


        public bool IsValidRtp(GameType gameType, RtpRange rtpRange)
        {
            return rtpRange.Maximum >= rtpRange.Minimum
                   && rtpRange.Minimum >= _rtpRules[gameType].minPayback
                   && rtpRange.Maximum <= _rtpRules[gameType].maxPayback;
        }

        public GameType ToGameType(t_gameType type)
        {
            // Default to slot for games that are not tagged
            switch (type)
            {
                case t_gameType.Blackjack:
                    return GameType.Blackjack;
                case t_gameType.Poker:
                    return GameType.Poker;
                case t_gameType.Keno:
                    return GameType.Keno;
                case t_gameType.Roulette:
                    return GameType.Roulette;
                default:
                    return GameType.Slot;
            }
        }
    }
}
