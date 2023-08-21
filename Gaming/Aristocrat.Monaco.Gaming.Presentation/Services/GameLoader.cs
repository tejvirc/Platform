namespace Aristocrat.Monaco.Gaming.Presentation.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Application.Contracts.Extensions;
using Common;
using Contracts;
using Contracts.Models;
using Extensions.Fluxor;
using Kernel;
using Microsoft.Extensions.Logging;
using Progressives;
using UI.Models;
using static Store.Chooser.ChooserSelectors;
using static Store.Translate.TranslateSelectors;

public sealed class GameLoader : IGameLoader, IDisposable
{
    private readonly ILogger<GameLoader> _logger;
    private readonly IStoreSelector _selector;
    private readonly IPropertiesManager _properties;
    private readonly IGameStorage _gameStorage;
    private readonly LobbyConfiguration _configuration;
    private readonly IGameOrderSettings _gameOrderSettings;
    private readonly IProgressiveConfigurationProvider _progressiveProvider;

    private readonly SubscriptionList _subscriptions = new();

    private string? _activeLocaleCode;

    public GameLoader(
        ILogger<GameLoader> logger,
        IStoreSelector selector,
        LobbyConfiguration configuration,
        IPropertiesManager properties,
        IGameStorage gameStorage,
        IGameOrderSettings gameOrderSettings,
        IProgressiveConfigurationProvider progressiveProvider)
    {
        _logger = logger;
        _selector = selector;
        _properties = properties;
        _gameStorage = gameStorage;
        _configuration = configuration;
        _gameOrderSettings = gameOrderSettings;
        _progressiveProvider = progressiveProvider;

        _subscriptions += selector.Select(SelectActiveLocale).Subscribe(code => _activeLocaleCode = code);
    }

    public async Task<IEnumerable<GameInfo>> LoadGames()
    {
        if (string.IsNullOrWhiteSpace(_activeLocaleCode))
        {
            return Enumerable.Empty<GameInfo>();
        }

        var games = _properties.GetValues<IGameDetail>(GamingConstants.Games).ToList();

        // Do not crash if game manifest does not provide the metadata for the expected locales.
        // This will just render bad data.
        foreach (var game in games)
        {
            if (!game.LocaleGraphics.ContainsKey(_activeLocaleCode))
            {
                game.LocaleGraphics.Add(_activeLocaleCode, new LocaleGameGraphics());
            }
        }

        SetGameOrderFromConfig(games);

        return await GetOrderedGames(games, _activeLocaleCode);
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
    }

    private void SetGameOrderFromConfig(IEnumerable<IGameDetail> games)
    {
        var distinctThemeGames = games.GroupBy(p => p.ThemeId).Select(g => g.FirstOrDefault(e => e.Active)).ToList();

        var lightningLinkEnabled = distinctThemeGames.Any(g => g.EgmEnabled && g.Enabled && g.Category == GameCategory.LightningLink);

        var lightningLinkOrder = lightningLinkEnabled
            ? _configuration.DefaultGameOrderLightningLinkEnabled
            : _configuration.DefaultGameOrderLightningLinkDisabled;

        var defaultList = lightningLinkOrder ?? _configuration.DefaultGameDisplayOrderByThemeId;


        _gameOrderSettings.SetAttractOrderFromConfig(distinctThemeGames.Select(g => new GameInfo { InstallDateTime = g.InstallDate, ThemeId = g.ThemeId } as IGameInfo).ToList(),
            defaultList);
        _gameOrderSettings.SetIconOrderFromConfig(distinctThemeGames.Select(g => new GameInfo { InstallDateTime = g.InstallDate, ThemeId = g.ThemeId } as IGameInfo).ToList(),
            _configuration.DefaultGameDisplayOrderByThemeId);
    }

    private async Task<IEnumerable<GameInfo>> GetOrderedGames(IReadOnlyCollection<IGameDetail> games, string activeLocalCode)
    {
        var useSmallIcons = await _selector.Select(SelectUseSmallIcons).FirstAsync();

        var gameCombos = (from game in games
            from denom in game.ActiveDenominations
            where game.Enabled
            select new GameInfo
            {
                GameId = game.Id,
                Name = game.ThemeName,
                InstallDateTime = game.InstallDate,
                DllPath = game.GameDll,
                ImagePath =
                    useSmallIcons
                        ? game.LocaleGraphics[activeLocalCode].SmallIcon
                        : game.LocaleGraphics[activeLocalCode].LargeIcon,
                TopPickImagePath =
                    useSmallIcons
                        ? game.LocaleGraphics[activeLocalCode].SmallTopPickIcon
                        : game.LocaleGraphics[activeLocalCode].LargeTopPickIcon,
                TopAttractVideoPath = game.LocaleGraphics[activeLocalCode].TopAttractVideo,
                TopperAttractVideoPath = game.LocaleGraphics[activeLocalCode].TopperAttractVideo,
                BottomAttractVideoPath = game.LocaleGraphics[activeLocalCode].BottomAttractVideo,
                LoadingScreenPath = game.LocaleGraphics[activeLocalCode].LoadingScreen,
                ProgressiveOrBonusValue =
                    GetProgressiveOrBonusValue(
                        game.Id,
                        denom,
                        game.Denominations.Single(d => d.Value == denom).BetOption),
                ProgressiveIndicator = ProgressiveLobbyIndicator.Disabled,
                Denomination = denom,
                BetOption = game.Denominations.Single(d => d.Value == denom).BetOption,
                FilteredDenomination =
                    _configuration.MinimumWagerCreditsAsFilter ? game.MinimumWagerCredits * denom : denom,
                GameType = game.GameType,
                GameSubtype = game.GameSubtype,
                PlatinumSeries = false,
                Enabled = game.Enabled,
                AttractHighlightVideoPath =
                    !string.IsNullOrEmpty(game.DisplayMeterName)
                        ? _configuration.AttractVideoWithBonusFilename
                        : _configuration.AttractVideoNoBonusFilename,
                UseSmallIcons = useSmallIcons,
                LocaleGraphics = game.LocaleGraphics,
                ThemeId = game.ThemeId,
                IsNew = GameIsNew(game.GameTags),
                Category = game.Category,
                SubCategory = game.SubCategory,
                RequiresMechanicalReels = game.MechanicalReels > 0
            }).ToList();

        return gameCombos.OrderBy(game => _gameOrderSettings.GetIconPositionPriority(game.ThemeId))
                .ThenBy(g => g.Denomination);
    }

    private static bool GameIsNew(IEnumerable<string>? gameTags)
    {
        return gameTags != null &&
               gameTags.Any(
                   t => string.Compare(GameTag.NewGame.ToString(), t, StringComparison.CurrentCultureIgnoreCase) == 0);
    }

    private string GetProgressiveOrBonusValue(int gameId, long denomId, string? betOption = null)
    {
        _logger.LogDebug("GetProgressiveOrBonusValue(gameId={GameId}, denomId={DenomId}, betOption={BetOption}", gameId, denomId, betOption);

        var game = _properties.GetValues<IGameDetail>(GamingConstants.Games).SingleOrDefault(g => g.Id == gameId);
        if (!string.IsNullOrEmpty(game?.DisplayMeterName))
        {
            var currentValue = game.InitialValue;

            var meter =
                _gameStorage.GetValues<InGameMeter>(gameId, denomId, GamingConstants.InGameMeters)
                    .FirstOrDefault(m => m.MeterName == game.DisplayMeterName);
            if (meter != null)
            {
                currentValue = meter.Value;
            }

            _logger.LogDebug("DisplayMeterName={DisplayMeterName}, JackpotValue={JackpotValue}", game.DisplayMeterName, currentValue);
            return (currentValue / CurrencyExtensions.CurrencyMinorUnitsPerMajorUnit).FormattedCurrencyString();
        }

        var levels = _progressiveProvider.ViewProgressiveLevels(gameId, denomId).ToList();
        if (levels.Any() && !string.IsNullOrWhiteSpace(betOption))
        {
            var match = levels.FirstOrDefault(
                p => string.IsNullOrEmpty(p.BetOption) || p.BetOption == betOption);

            if (match != null)
            {
                _logger.LogDebug("Found {LevelCount} levels, returning first JackpotValue={JackpotValue}", levels.Count, match.CurrentValue);
                return match.CurrentValue.MillicentsToDollarsNoFraction().FormattedCurrencyString();
            }
        }

        _logger.LogDebug("Returning empty progressive value");
        return string.Empty;
    }
}
