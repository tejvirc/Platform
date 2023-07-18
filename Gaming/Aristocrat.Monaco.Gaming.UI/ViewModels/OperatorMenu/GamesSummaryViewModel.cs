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
    using Kernel.Contracts.Components;
    using Localization.Properties;
    using Models;

    public class GamesSummaryViewModel : OperatorMenuPageViewModelBase
    {
        private string _range;
        private int _enabledGamesCount;
        private readonly double _denomMultiplier;
        private readonly IGameProvider _gameProvider;
        private ObservableCollection<ReadOnlyGameConfiguration> _enabledGames;
        private string _maxBetLimit;
        private IReadOnlyCollection<IGameDetail> _enabledGamesList;
        private readonly List<IGameDetail> _filteredGamesList;
        private string _hashesComponentId;
        private const string HashesFileExtension = ".hashes";

        public GamesSummaryViewModel()
        {
            _enabledGamesList = new List<IGameDetail>();
            _filteredGamesList = new List<IGameDetail>();

            EnabledGames = new ObservableCollection<ReadOnlyGameConfiguration>();
            var container = ServiceManager.GetInstance().GetService<IContainerService>();
            if (container != null)
            {
                _gameProvider = container.Container.GetInstance<IGameProvider>();
            }

            _denomMultiplier = PropertiesManager.GetValue(ApplicationConstants.CurrencyMultiplierKey, 1d);
            ShowGameRtpAsRange = GetGlobalConfigSetting(OperatorMenuSetting.ShowGameRtpAsRange, true);
            ProgressiveRtpsVisible = Configuration.GetSetting(OperatorMenuSetting.ShowProgressiveRtps, true);

            var componentRegistry = ServiceManager.GetInstance().GetService<IComponentRegistry>();
            var components = componentRegistry.Components;

            var hashesFileName = components.SingleOrDefault(c => c.ComponentId.EndsWith(HashesFileExtension))?.ComponentId;
            if (!string.IsNullOrEmpty(hashesFileName))
            {
                HashesComponentId = hashesFileName.Substring(0, hashesFileName.LastIndexOf(HashesFileExtension[0]));
            }
        }

        public string Range
        {
            get => _range;
            set
            {
                _range = value;
                OnPropertyChanged(nameof(Range));
            }
        }

        public string HashesComponentId
        {
            get => _hashesComponentId;
            set
            {
                _hashesComponentId = value;
                OnPropertyChanged(nameof(HashesComponentId));
            }
        }

        public bool IsHashComponentVisible => !string.IsNullOrEmpty(HashesComponentId);

        public int EnabledGamesCount
        {
            get => _enabledGamesCount;
            set
            {
                _enabledGamesCount = value;
                OnPropertyChanged(nameof(EnabledGamesCount));
            }
        }

        public ObservableCollection<ReadOnlyGameConfiguration> EnabledGames
        {
            get => _enabledGames;
            set
            {
                _enabledGames = value;
                OnPropertyChanged(nameof(EnabledGames));
                EnabledGamesCount = _enabledGames.Count;
            }
        }

        public string MaxBetLimit
        {
            get => _maxBetLimit;
            set
            {
                _maxBetLimit = value;
                OnPropertyChanged(nameof(MaxBetLimit));
            }
        }

        public bool ProgressiveRtpsVisible { get; }

        public bool ShowGameRtpAsRange { get; }

        public bool ShowProgressiveRtpAsRange => ProgressiveRtpsVisible && ShowGameRtpAsRange;

        public bool ShowProgressiveRtpSeparately => ProgressiveRtpsVisible && !ShowGameRtpAsRange;

        protected override void OnLoaded()
        {
            var maxBetLimitDollars = ((long)PropertiesManager.GetProperty(
                AccountingConstants.MaxBetLimit,
                AccountingConstants.DefaultMaxBetLimit)).MillicentsToDollars();
            var maxBetLimitCents = maxBetLimitDollars.DollarsToCents();

            _enabledGamesList = _gameProvider.GetEnabledGames();
            _filteredGamesList.Clear();

            var denomMultiplier = (decimal)PropertiesManager.GetValue(ApplicationConstants.CurrencyMultiplierKey, 1d);
            _filteredGamesList.AddRange(
                _enabledGamesList.Where(
                    g => g.Denominations
                        .Select(d => (g.MaximumWagerCredits(d) * d.Value / denomMultiplier).DollarsToCents())
                        .Any(c => c <= maxBetLimitCents)));

            RefreshGames();

            if (_filteredGamesList.Any())
            {
                var minOverall = _filteredGamesList.Min(g => g.MinimumPaybackPercent);
                var maxOverall = _filteredGamesList.Max(g => g.MaximumPaybackPercent);
                Range = $"{minOverall:F3}% - {maxOverall:F3}%";
            }
            else
            {
                if (_enabledGamesList.Any())
                {
                    InputStatusText = string.Format(
                        CultureInfo.CurrentCulture,
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoEnabledGamesAtMaxBetLimit),
                        MaxBetLimit);
                }

                Range = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable);
            }
        }

        private void RefreshGames()
        {
            var configs = _filteredGamesList.SelectMany(
                g => g.ActiveDenominations,
                (game, denom) => new ReadOnlyGameConfiguration(game, denom, _denomMultiplier));

            EnabledGames = new ObservableCollection<ReadOnlyGameConfiguration>(configs);
        }

        protected override void OnOperatorCultureChanged(OperatorCultureChangedEvent evt)
        {
            RefreshGames();
            base.OnOperatorCultureChanged(evt);
        }
    }
}
