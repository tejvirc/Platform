namespace Aristocrat.Monaco.Gaming.Lobby.Services;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using Kernel;
using Models;

public class GameLoader : IGameLoader
{
    private readonly IPropertiesManager _properties;
    private readonly IGameOrderSettings _gameOrderSettings;

    public GameLoader(IPropertiesManager properties, IGameOrderSettings gameOrderSettings)
    {
        _properties = properties;
        _gameOrderSettings = gameOrderSettings;
    }

    public Task<IEnumerable<GameInfo>> LoadGames()
    {
        var games = _properties.GetValues<IGameDetail>(GamingConstants.Games).ToList();

        SetGameOrderFromConfig(games);

        var orderedGames = GetOrderedGames(games);

        return Task.FromResult(orderedGames);
    }

    private void SetGameOrderFromConfig(IEnumerable<IGameDetail> games)
    {
        var config = _properties.GetValue<LobbyConfiguration?>(GamingConstants.LobbyConfig, null);

        var distinctThemeGames = games
            .GroupBy(p => p.ThemeId)
            .Select(g => g.FirstOrDefault(e => e.Active))
            .ToList();

        _gameOrderSettings.SetGameOrderFromConfig(
            distinctThemeGames
                .Where(g => g != null)
                .Select(g => new GameInfo { InstallDateTime = g!.InstallDate, ThemeId = g.ThemeId })
                .Cast<IGameInfo>().ToList(),
            config?.DefaultGameDisplayOrderByThemeId.ToList());
    }

    private IEnumerable<GameInfo> GetOrderedGames(ICollection<IGameDetail> games)
    {
        var config = _properties.GetValue<LobbyConfiguration?>(GamingConstants.LobbyConfig, null);

        // ChooseGameOffsetY = UseSmallIcons ? 25.0 : 50.0;

        var gameCombos = (from game in games
                          from denom in game.ActiveDenominations
                          where game.Enabled
                          select new GameInfo
                          {
                              GameId = game.Id,
                              Name = game.ThemeName,
                              InstallDateTime = game.InstallDate,
                              DllPath = game.GameDll,
                              ImagePath = games.Count > 8 ? game.LocaleGraphics.Values.First().SmallIcon : game.LocaleGraphics.Values.First().LargeIcon,
                              // ImagePath = UseSmallIcons ? game.LocaleGraphics[ActiveLocaleCode].SmallIcon : game.LocaleGraphics[ActiveLocaleCode].LargeIcon,
                              // TopPickImagePath = UseSmallIcons ? game.LocaleGraphics[ActiveLocaleCode].SmallTopPickIcon : game.LocaleGraphics[ActiveLocaleCode].LargeTopPickIcon,
                              // TopAttractVideoPath = game.LocaleGraphics[ActiveLocaleCode].TopAttractVideo,
                              // TopperAttractVideoPath = game.LocaleGraphics[ActiveLocaleCode].TopperAttractVideo,
                              // BottomAttractVideoPath = game.LocaleGraphics[ActiveLocaleCode].BottomAttractVideo,
                              // LoadingScreenPath = game.LocaleGraphics[ActiveLocaleCode].LoadingScreen,
                              HasProgressiveOrBonusValue = !string.IsNullOrEmpty(game.DisplayMeterName),
                              // ProgressiveOrBonusValue = GetProgressiveOrBonusValue(game.Id, denom),
                              ProgressiveIndicator = ProgressiveLobbyIndicator.Disabled,
                              Denomination = denom,
                              BetOption = game.Denominations.Single(d => d.Value == denom).BetOption,
                              FilteredDenomination = config?.MinimumWagerCreditsAsFilter ?? false ? game.MinimumWagerCredits * denom : denom,
                              GameType = game.GameType,
                              GameSubtype = game.GameSubtype,
                              PlatinumSeries = false,
                              Enabled = game.Enabled,
                              AttractHighlightVideoPath = !string.IsNullOrEmpty(game.DisplayMeterName) ? config?.AttractVideoWithBonusFilename : config?.AttractVideoNoBonusFilename,
                              // UseSmallIcons = UseSmallIcons,
                              LocaleGraphics = game.LocaleGraphics,
                              ThemeId = game.ThemeId,
                              IsNew = GameIsNew(game.GameTags),
                              Category = game.Category,
                              SubCategory = game.SubCategory,
                              RequiresMechanicalReels = game.MechanicalReels > 0
                          }).ToList();

        return new ObservableCollection<GameInfo>(
            gameCombos.OrderBy(game => _gameOrderSettings.GetPositionPriority(game.ThemeId))
                .ThenBy(g => g.Denomination));
    }

    private static bool GameIsNew(IEnumerable<string>? gameTags)
    {
        return gameTags != null &&
               gameTags.Any(
                   t => string.Compare(GameTag.NewGame.ToString(), t, StringComparison.CurrentCultureIgnoreCase) == 0);
    }
}
