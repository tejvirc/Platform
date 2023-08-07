namespace Aristocrat.Monaco.Gaming.UI.PlayerInfoDisplay
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Contracts.PlayerInfoDisplay;
    using log4net;
    using Models;

    /// <inheritdoc cref="IGameResourcesModelProvider" />
    public sealed class GameResourcesModelProvider : IGameResourcesModelProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IGameProvider _gameProvider;
        private readonly IPlayerInfoDisplayFeatureProvider _playerInfoDisplayFeatureProvider;

        public GameResourcesModelProvider(
            IGameProvider gameProvider
            , IPlayerInfoDisplayFeatureProvider playerInfoDisplayFeatureProvider)
        {
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _playerInfoDisplayFeatureProvider = playerInfoDisplayFeatureProvider ?? throw new ArgumentNullException(nameof(playerInfoDisplayFeatureProvider));
        }

        /// <inheritdoc />
        public IPlayInfoDisplayResourcesModel Find(int gameId)
        {
            var game = _gameProvider.GetGame(gameId);
            var model = new PlayInfoDisplayResourcesModel();
            if (game == null)
            {
                Logger.Error($"No game found for id = {gameId}.");
            }
            else
            {
                Logger.Debug($"Game {game.Id} is selected");

                var local = _playerInfoDisplayFeatureProvider.ActiveLocaleCode;
                if (game.LocaleGraphics == null || !game.LocaleGraphics.ContainsKey(local))
                {
                    return model;
                }

                var graphics = game.LocaleGraphics[local];

                var backgrounds = graphics.PlayerInfoDisplayResources
                    .Where(x => new HashSet<string> { GameAssetTags.PlayerInformationDisplayMenuTag, GameAssetTags.BackgroundTag, GameAssetTags.ScreenTag }.IsSubsetOf(x.Tags))
                    .ToArray();

                var buttons = graphics.PlayerInfoDisplayResources
                    .Where(x => new HashSet<string> { GameAssetTags.PlayerInformationDisplayMenuTag, GameAssetTags.ButtonTag }.IsSubsetOf(x.Tags))
                    .ToArray();
                model.ScreenBackgrounds = backgrounds;
                model.Buttons = buttons;
            }

            return model;
        }
    }
}