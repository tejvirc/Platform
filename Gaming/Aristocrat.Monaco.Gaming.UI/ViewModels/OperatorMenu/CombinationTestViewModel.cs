namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Contracts.Configuration;
    using Contracts;
    using Contracts.Models;
    using Diagnostics;
    using Kernel;
    using Localization.Properties;
    using Models;
    using Newtonsoft.Json;

    public class CombinationTestViewModel : OperatorMenuDiagnosticPageViewModelBase
    {
        private readonly IGameDiagnostics _diagnostics;
        private readonly IGameProvider _gameProvider;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IGamePackConfigurationProvider _gamePackConfigProvider;
        private ObservableCollection<GameComboInfo> _games;
        private GameComboInfo _selectedGame;

        public CombinationTestViewModel()
        {
            var container = ServiceManager.GetInstance()
                .GetService<IContainerService>()?.Container;

            if (container != null)
            {
                _gameProvider = container.GetInstance<IGameProvider>();
                _diagnostics = container.GetInstance<IGameDiagnostics>();
                _configurationProvider = container.GetInstance<IConfigurationProvider>();
                _gamePackConfigProvider = container.GetInstance<IGamePackConfigurationProvider>();
            }

            ComboTestCommand = new RelayCommand<object>(_ => LaunchCombinationTest(), _ => SelectedGame != null && InputEnabled);

            Games = new ObservableCollection<GameComboInfo>();
        }

        public IRelayCommand ComboTestCommand { get; }

        public GameComboInfo SelectedGame
        {
            get => _selectedGame;
            set
            {
                if (_selectedGame != value)
                {
                    _selectedGame = value;
                    OnPropertyChanged(nameof(SelectedGame));
                    ComboTestCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<GameComboInfo> Games
        {
            get => _games;
            set
            {
                _games = value;
                OnPropertyChanged(nameof(Games));
            }
        }

        protected override void OnInputStatusChanged()
        {
            ComboTestCommand.NotifyCanExecuteChanged();
        }

        protected override void OnInputEnabledChanged()
        {
            ComboTestCommand.NotifyCanExecuteChanged();
        }

        protected override void InitializeData()
        {
            Games = new ObservableCollection<GameComboInfo>(
                CreateGameComboList().OrderBy(m => m.UniqueId)
            );

            SelectedGame = null;
        }

        private void LaunchCombinationTest()
        {
            if (SelectedGame is null)
            {
                return;
            }

            PreventOperatorMenuExit();

            CombinationTestContext context;

            var configs = _configurationProvider
                .GetByThemeId(SelectedGame.ThemeId);

            if (configs is not null && configs.Any())
            {
                var gamePackRestrictions = _gamePackConfigProvider
                    .GetDenomRestrictionsByGameId(SelectedGame.Id);

                var json = JsonConvert.SerializeObject(gamePackRestrictions, Formatting.None);

                context = new CombinationTestContext(
                    new Dictionary<string, string>
                    {
                        { "/Runtime/Multigame&ActivePack", json }
                    }
                );
            }
            else
            {
                context = new CombinationTestContext();
            }

            _diagnostics.Start(
                SelectedGame.Id,
                SelectedGame.Denomination.DollarsToMillicents(),
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CombinationTestTitle),
                context,
                true);
        }

        private IEnumerable<GameComboInfo> CreateGameComboList()
        {
            var groupedByTheme = _gameProvider.GetGames()
                .Where(g => g.GameType == GameType.Slot)
                .GroupBy(g => g.ThemeId)
                .ToDictionary(k => k.Key, v => v.ToArray());

            foreach (var kvp in groupedByTheme)
            {
                var (themeId, games) = (kvp.Key, kvp.Value);

                var configs = _configurationProvider
                    .GetByThemeId(themeId)
                    .ToArray();

                if (!configs.Any())
                {
                    var combos = games.SelectMany(
                        g => g.Denominations,
                        (g, d) => MapGameCombo(g, d, $"Variation {g.VariationId}")
                    );

                    foreach (var combo in combos)
                    {
                        yield return combo;
                    }

                    continue;
                }

                var configMaps = configs.SelectMany(
                    c => c.RestrictionDetails.Mapping,
                    (c, m) => (c, m)
                );

                foreach (var (config, map) in configMaps)
                {
                    var game = games.FirstOrDefault(
                        g => g.VariationId == map.VariationId
                    );

                    var denom = game?.Denominations.FirstOrDefault(
                        d => d.Value == map.Denomination
                    );

                    if (game is not null && denom is not null)
                    {
                        yield return MapGameCombo(game, denom, config.Name);
                    }
                }
            }
        }

        private static GameComboInfo MapGameCombo(
            IGameProfile game,
            IDenomination denom,
            string variation)
        {
            return new GameComboInfo
            {
                Id = game.Id,
                UniqueId = denom.Id,
                ThemeId = game.ThemeId,
                PaytableId = game.PaytableId,
                Denomination = denom.Value.MillicentsToDollars(),
                Name = game.ThemeName,
                Variation = variation
            };
        }
    }
}
