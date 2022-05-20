namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.OperatorMenu;
    using Contracts;
    using Kernel;
    using Localization.Properties;
    using Models;

    public class GameConfigurationViewModel : OperatorMenuPageViewModelBase
    {
        private const string ShowMaxBetLimitSetting = "ShowMaxBetLimit";
        private string _maxBetLimit;
        private string _range;
        private int _enabledGamesCount;

        private readonly double _denomMultiplier;
        private readonly IGameProvider _gameProvider;

        private ObservableCollection<ReadOnlyGameConfiguration> _enabledGames;
        private ReadOnlyGameConfiguration _selectedGame;

        private List<IGameDetail> _enabledGamesList;
        private readonly List<IGameDetail> _filteredGamesList;

        public GameConfigurationViewModel()
        {
            _enabledGamesList = new List<IGameDetail>();
            _filteredGamesList = new List<IGameDetail>();

            EnabledGames = new ObservableCollection<ReadOnlyGameConfiguration>();

            ProgressiveRTPsVisible = GetConfigSetting(OperatorMenuSetting.ShowProgressiveRtps, false);

            var container = ServiceManager.GetInstance().GetService<IContainerService>();
            if (container != null)
            {
                _gameProvider = container.Container.GetInstance<IGameProvider>();
            }

            _denomMultiplier = PropertiesManager.GetValue(ApplicationConstants.CurrencyMultiplierKey, 1d);

            InputStatusText = string.Empty;
            ShowMaxBetLimit = GetConfigSetting(ShowMaxBetLimitSetting, true);
        }

        public bool ProgressiveRTPsVisible { get; }

        public string MaxBetLimit
        {
            get => _maxBetLimit;
            set
            {
                _maxBetLimit = value;
                RaisePropertyChanged(nameof(MaxBetLimit));
            }
        }

        public string Range
        {
            get => _range;
            set
            {
                _range = value;
                RaisePropertyChanged(nameof(Range));
            }
        }

        public int EnabledGamesCount
        {
            get => _enabledGamesCount;
            set
            {
                _enabledGamesCount = value;
                RaisePropertyChanged(nameof(EnabledGamesCount));
            }
        }

        public ObservableCollection<ReadOnlyGameConfiguration> EnabledGames
        {
            get => _enabledGames;
            set
            {
                _enabledGames = value;
                RaisePropertyChanged(nameof(EnabledGames));
                EnabledGamesCount = _enabledGames.Count;
            }
        }

        public ReadOnlyGameConfiguration SelectedGame
        {
            get => _selectedGame;
            set
            {
                _selectedGame = value;
                RaisePropertyChanged(nameof(SelectedGame));
            }
        }

        public bool ShowMaxBetLimit { get; set; }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            var maxBetLimitDollars = ((long)PropertiesManager.GetProperty(
                AccountingConstants.MaxBetLimit,
                AccountingConstants.DefaultMaxBetLimit)).MillicentsToDollars();
            var maxBetLimitCents = maxBetLimitDollars.DollarsToCents();
            MaxBetLimit = maxBetLimitDollars == long.MaxValue.MillicentsToDollars()
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoLimit)
                : maxBetLimitDollars.FormattedCurrencyString();

            _enabledGamesList.Clear();
            _enabledGamesList = _gameProvider.GetEnabledGames().ToList();
            _filteredGamesList.Clear();

            var denomMultiplier = (decimal)PropertiesManager.GetValue(ApplicationConstants.CurrencyMultiplierKey, 1d);
            _filteredGamesList.AddRange(
                _enabledGamesList.Where(
                    g => g.Denominations
                        .Select(d => (g.MaximumWagerCredits(d) * d.Value / denomMultiplier).DollarsToCents())
                        .Any(c => c <= maxBetLimitCents)));

            var configs = _filteredGamesList.SelectMany(
                g => g.ActiveDenominations,
                (game, denom) => new ReadOnlyGameConfiguration(game, denom, _denomMultiplier));

            EnabledGames = new ObservableCollection<ReadOnlyGameConfiguration>(configs);

            if (_filteredGamesList.Any())
            {
                var minOverall = _filteredGamesList.Min(g => g.MinimumPaybackPercent);
                var maxOverall = _filteredGamesList.Max(g => g.MaximumPaybackPercent);
                Range = $"{minOverall:F3}% - {maxOverall:F3}%";
            }
            else
            {
                if (InputEnabled && _enabledGamesList.Any())
                {
                    InputStatusText = string.Format(
                        CultureInfo.CurrentCulture,
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoEnabledGamesAtMaxBetLimit),
                        MaxBetLimit);
                    InputEnabled = false;
                }

                Range = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable);
            }
        }
    }
}
