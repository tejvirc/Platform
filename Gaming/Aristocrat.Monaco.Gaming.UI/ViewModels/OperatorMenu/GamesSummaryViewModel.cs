﻿namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using Contracts.Rtp;
    using MVVM.Command;
    using Aristocrat.Monaco.Gaming.UI.Views.OperatorMenu;
    using Contracts.Progressives;
    using IDialogService = Application.Contracts.OperatorMenu.IDialogService;

    public class GamesSummaryViewModel : OperatorMenuPageViewModelBase
    {
        private readonly IGameProvider _gameProvider;
        private readonly IDialogService _dialogService;
        private string _range;
        private int _enabledGamesCount;
        private readonly double _denomMultiplier;
        private ObservableCollection<ReadOnlyGameConfiguration> _enabledGames;
        private string _maxBetLimit;
        private IReadOnlyCollection<IGameDetail> _enabledGamesList;
        private readonly List<IGameDetail> _filteredGamesList;
        private string _hashesComponentId;
        private const string HashesFileExtension = ".hashes";

        public GamesSummaryViewModel()
        {
            _dialogService = ServiceManager.GetInstance().GetService<IDialogService>();

            ShowRtpBreakdownDialogCommand = new ActionCommand<ReadOnlyGameConfiguration>(ShowRtpBreakdownDialog);

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
            if(!string.IsNullOrEmpty(hashesFileName))
            {
                HashesComponentId = hashesFileName.Substring(0, hashesFileName.LastIndexOf(HashesFileExtension[0]));
            }
        }

        public ActionCommand<ReadOnlyGameConfiguration> ShowRtpBreakdownDialogCommand { get; }

        public string Range
        {
            get => _range;
            set
            {
                _range = value;
                RaisePropertyChanged(nameof(Range));
            }
        }

        public string HashesComponentId
        {
            get => _hashesComponentId;
            set
            {
                _hashesComponentId = value;
                RaisePropertyChanged(nameof(HashesComponentId));
            }
        }

        public bool IsHashComponentVisible => !string.IsNullOrEmpty(HashesComponentId);

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

        public string MaxBetLimit
        {
            get => _maxBetLimit;
            set
            {
                _maxBetLimit = value;
                RaisePropertyChanged(nameof(MaxBetLimit));
            }
        }

        public bool ProgressiveRtpsVisible { get; }

        public bool ShowGameRtpAsRange { get; }

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

            var configs = _filteredGamesList.SelectMany(
                g => g.ActiveDenominations,
                (game, denom) => new ReadOnlyGameConfiguration(game, denom, _denomMultiplier));

            EnabledGames = new ObservableCollection<ReadOnlyGameConfiguration>(configs);

            if (_filteredGamesList.Any())
            {
                var rtpService = ServiceManager.GetInstance().GetService<IRtpService>();

                var totalRtp = rtpService.GetTotalRtp(_filteredGamesList);

                Range = $"{totalRtp.Minimum.ToRtpString()} - {totalRtp.Maximum.ToRtpString()}";
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

        private void ShowRtpBreakdownDialog(ReadOnlyGameConfiguration gameConfig)
        {
            var viewModel = new GameRtpBreakdownViewModel(gameConfig.RtpDisplayValues);

            _dialogService.ShowInfoDialog<GameRtpBreakdownView>(this, viewModel, string.Empty);
        }
    }
}
