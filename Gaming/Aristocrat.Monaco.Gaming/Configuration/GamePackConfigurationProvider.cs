namespace Aristocrat.Monaco.Gaming.Configuration
{
    using Contracts;
    using Contracts.Configuration;
    using System.Linq;
    using Application.Contracts.Extensions;

    public class GamePackConfigurationProvider : IGamePackConfigurationProvider
    {
        private readonly IGameConfigurationProvider _configProvider;
        private readonly IGameProvider _gameProvider;

        public GamePackConfigurationProvider(IGameConfigurationProvider configProvider,
            IGameProvider gameProvider)
        {
            _configProvider = configProvider;
            _gameProvider = gameProvider;
        }

        public object GetDenomRestrictionsByGameId(int gameId)
        {
            var game = _gameProvider.GetGame(gameId);
            var restrictions = _configProvider.GetActive(game.ThemeId);

            if (restrictions is null)
            {
                return "";
            }

            var activeDenominations = _gameProvider.GetEnabledGames()
                .Where(g => g.ThemeId == game.ThemeId)
                .SelectMany(g => g.Denominations.Where(d => d.Active));

            return new
            {
                PackName = restrictions.Name,
                Denominations = activeDenominations.Select(
                    d => new
                    {
                        Denomination = d.Value.MillicentsToCents(),
                        Gamble = d.SecondaryAllowed,
                        LetItRide = d.LetItRideAllowed,
                        d.MinimumWagerCredits,
                        d.MaximumWagerCredits,
                        d.MaximumWagerOutsideCredits,
                        d.BetOption,
                        d.LineOption,
                        d.BonusBet
                    })
            };
        }
    }
}
