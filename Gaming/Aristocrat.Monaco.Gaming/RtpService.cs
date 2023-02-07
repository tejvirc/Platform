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
        private readonly IGameProvider _gameProvider;
        private readonly IPropertiesManager _properties;
        private readonly Dictionary<GameType, RtpRules> _rtpRules = new();

        public RtpService(IGameProvider gameProvider, IPropertiesManager propertiesManager)
        {
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _properties = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        public string Name => GetType().ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(IRtpService) };

        public void Initialize()
        {
            LoadRtpRules();
        }

        public RtpReport GetRtpReport(params IGameDetail[] gameVariations)
        {
            throw new NotImplementedException();
        }

        public RtpReport GetRtpReportForGameTheme(string gameThemeId) 
        {
            var gamesForTheme = _gameProvider.GetAllGames()
                .Where(game => game.ThemeId.Equals(gameThemeId)).ToList();

            var gameType = gamesForTheme.First().GameType;

            var report = new RtpReport(gamesForTheme, _rtpRules[gameType]);

            return report;
        }

        public RtpReport GetRtpReportForVariation(string gameThemeId, string variationId)
        {
            throw new NotImplementedException();
        }

        public RtpReport GetRtpReportForWagerCategory(string gameThemeId, string variationId, string wagerCategoryId)
        {
            throw new NotImplementedException();
        }

        private void LoadRtpRules()
        {
            _rtpRules[GameType.Slot] = new RtpRules
            {
                MinimumRtp = _properties.GetValue(GamingConstants.SlotMinimumReturnToPlayer, decimal.MinValue),
                MaximumRtp = _properties.GetValue(GamingConstants.SlotMaximumReturnToPlayer, decimal.MaxValue),
                IncludeLinkProgressiveIncrementRtp = _properties.GetValue(GamingConstants.SlotsIncludeLinkProgressiveIncrementRtp, false),
                IncludeLinkProgressiveStartUpRtp = _properties.GetValue(GamingConstants.SlotsIncludeLinkProgressiveStartUpRtp, false),
                IncludeStandaloneProgressiveIncrementRtp = _properties.GetValue(GamingConstants.SlotsIncludeStandaloneProgressiveIncrementRtp, true),
                IncludeStandaloneProgressiveStartUpRtp = _properties.GetValue(GamingConstants.SlotsIncludeStandaloneProgressiveStartUpRtp, false)
            };

            _rtpRules[GameType.Blackjack] = new RtpRules
            {
                MinimumRtp = _properties.GetValue(GamingConstants.BlackjackMinimumReturnToPlayer, decimal.MinValue),
                MaximumRtp = _properties.GetValue(GamingConstants.BlackjackMaximumReturnToPlayer, decimal.MaxValue),
                IncludeLinkProgressiveIncrementRtp = _properties.GetValue(GamingConstants.BlackjackIncludeLinkProgressiveIncrementRtp, false),
                IncludeLinkProgressiveStartUpRtp = _properties.GetValue(GamingConstants.BlackjackIncludeLinkProgressiveStartUpRtp, false),
                IncludeStandaloneProgressiveIncrementRtp = _properties.GetValue(GamingConstants.BlackjackIncludeStandaloneProgressiveIncrementRtp, true),
                IncludeStandaloneProgressiveStartUpRtp = _properties.GetValue(GamingConstants.BlackjackIncludeStandaloneProgressiveStartUpRtp, false)
            };

            _rtpRules[GameType.Keno] = new RtpRules
            {
                MinimumRtp = _properties.GetValue(GamingConstants.KenoMinimumReturnToPlayer, decimal.MinValue),
                MaximumRtp = _properties.GetValue(GamingConstants.KenoMaximumReturnToPlayer, decimal.MaxValue),
                IncludeLinkProgressiveIncrementRtp = _properties.GetValue(GamingConstants.KenoIncludeLinkProgressiveIncrementRtp, false),
                IncludeLinkProgressiveStartUpRtp = _properties.GetValue(GamingConstants.KenoIncludeLinkProgressiveStartUpRtp, false),
                IncludeStandaloneProgressiveIncrementRtp = _properties.GetValue(GamingConstants.KenoIncludeStandaloneProgressiveIncrementRtp, true),
                IncludeStandaloneProgressiveStartUpRtp = _properties.GetValue(GamingConstants.KenoIncludeStandaloneProgressiveStartUpRtp, false)
            };

            _rtpRules[GameType.Roulette] = new RtpRules
            {
                MinimumRtp = _properties.GetValue(GamingConstants.RouletteMinimumReturnToPlayer, decimal.MinValue),
                MaximumRtp = _properties.GetValue(GamingConstants.RouletteMaximumReturnToPlayer, decimal.MaxValue),
                IncludeLinkProgressiveIncrementRtp = _properties.GetValue(GamingConstants.RouletteIncludeLinkProgressiveIncrementRtp, false),
                IncludeLinkProgressiveStartUpRtp = _properties.GetValue(GamingConstants.RouletteIncludeLinkProgressiveStartUpRtp, false),
                IncludeStandaloneProgressiveIncrementRtp = _properties.GetValue(GamingConstants.RouletteIncludeStandaloneProgressiveIncrementRtp, true),
                IncludeStandaloneProgressiveStartUpRtp = _properties.GetValue(GamingConstants.RouletteIncludeStandaloneProgressiveStartUpRtp, false)
            };
            
            // For games that didn't specify their type, presume they are Slot.
            _rtpRules[GameType.Undefined] = _rtpRules[GameType.Slot];
        }
    }
}