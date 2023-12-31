﻿namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Application.UI.MeterPage;
    using Aristocrat.Monaco.Gaming.Contracts.Models;
    using Contracts;
    using Contracts.Meters;
    using Kernel;
    using MVVM;

    public class GameStatisticsViewModel : OperatorMenuDiagnosticPageViewModelBase
    {
        private Dictionary<string, string> _gambleMeters;
        private readonly Dictionary<string, InGameMeter> _featureMeters;

        private bool _baseStatsCollapsed;
        private bool _featureStatsCollapsed;
        private string _selectedGame;
        private readonly IEnumerable<IGameDetail> _games;

        public GameStatisticsViewModel()
        {
            _games = PropertiesManager.GetValues<IGameDetail>(GamingConstants.AllGames).Where(x => x.Features != null && x.Features.Any() && x.Features.All(e=> e.Enable));
            _featureMeters = new Dictionary<string, InGameMeter>();
            LoadMeters();
        }

        public bool GameSelectionEnabled => Games.Count > 1;

        public bool BaseStatsCollapsed
        {
            get => _baseStatsCollapsed;

            set
            {
                if (_baseStatsCollapsed != value)
                {
                    _baseStatsCollapsed = value;
                    RaisePropertyChanged(nameof(BaseStatsCollapsed));
                }
            }
        }

        public bool FeatureStatsCollapsed
        {
            get => _featureStatsCollapsed;

            set
            {
                if (_featureStatsCollapsed != value)
                {
                    _featureStatsCollapsed = value;
                    RaisePropertyChanged(nameof(FeatureStatsCollapsed));
                }
            }
        }

        public ObservableCollection<DisplayMeter> GameGambleMeters { get; } = new ObservableCollection<DisplayMeter>();

        public ObservableCollection<InGameMeter> GameFeatureMeters { get; } = new ObservableCollection<InGameMeter>();

        protected override void OnLoaded()
        {
            base.OnLoaded();

            BaseStatsCollapsed = true;  // if we implement this change to false and bind up - view contains the stub
            FeatureStatsCollapsed = !_games.Any();
            InitializeMeters();
            SelectedGame = Games?.FirstOrDefault();
        }

        private void LoadMeters()
        {
            _gambleMeters = new Dictionary<string, string>
            {
                { GamingMeters.PrimaryWonAmount, "Primary Game Amount Won" },
                { GamingMeters.SecondaryWageredAmount, "Double Up Amount Wagered" },
                { GamingMeters.SecondaryWonAmount, "Double Up Amount Won" },
                { GamingMeters.SecondaryPlayedCount, "Double Up Games Played" },
                { GamingMeters.SecondaryTiedAndWonCount, "Double Up Games Won" }
            };
        }

        private void InitializeMeters()
        {
            // This will occur each time a different game is selected
            var meterManager = ServiceManager.GetInstance().GetService<IGameMeterManager>();

            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    foreach (var meter in GameGambleMeters)
                    {
                        meter.Dispose();
                    }

                    GameGambleMeters.Clear();

                    var localizer = Localizer.For(CultureFor.Operator);

                    foreach (var meterNode in _gambleMeters)
                    {
                        var label = meterNode.Key + "Label";

                        string meterDisplayName = localizer.GetString(label, _ => meterDisplayName = meterNode.Value);

                        try
                        {
                            var meter = meterManager.GetMeter(meterNode.Key);
                            GameGambleMeters.Add(new DisplayMeter(meterDisplayName ?? meterNode.Value, meter, true));
                        }
                        catch (MeterNotFoundException)
                        {

                            GameGambleMeters.Add(new DisplayMeter(meterDisplayName ?? meterNode.Value, null, true));

                            Logger.ErrorFormat("Meter not found: {0}", meterNode.Key);
                        }
                    }

                });
        }

        public ObservableCollection<string> Games => new ObservableCollection<string>(_games.Select(x => x.ThemeName).Distinct());

        public string SelectedGame
        {
            get => _selectedGame;
            set
            {
                _selectedGame = value;
                RaisePropertyChanged(nameof(SelectedGame));
                InitializeGameFeatureMeters(value);
            }
        }

        private void InitializeGameFeatureMeters(string game)
        {
            if (string.IsNullOrEmpty(game))
                return;

            _featureMeters.Clear();

            _games?.FirstOrDefault(x => x.ThemeName == game)
                ?.Features.FirstOrDefault()?.StatInfo.ToList().ForEach(x => {
                _featureMeters[x.Name] = new InGameMeter { MeterName = x.DisplayName };
            });


            _featureMeters.ToList().ForEach(i => _featureMeters[i.Key].Value = 0);
            GameFeatureMeters.Clear();

            var gameStorage = ServiceManager.GetInstance().TryGetService<IContainerService>().Container.GetInstance<IGameStorage>();

            _games?.Where(x => x.ThemeName == game).ToList().ForEach(g =>
            {
                g.SupportedDenominations.ToList().ForEach(denom => {
                    gameStorage.GetValues<InGameMeter>(g.Id, denom, GamingConstants.InGameMeters).ToList().ForEach(meter =>
                    {
                        if (_featureMeters.ContainsKey(meter.MeterName))
                            _featureMeters[meter.MeterName].Value += meter.Value;

                    });
                });
            });

            _featureMeters.ToList()
                .ForEach(
                    i => GameFeatureMeters.Add(new InGameMeter { MeterName = _featureMeters[i.Key].MeterName, Value = _featureMeters[i.Key].Value }));
        }

        protected override void DisposeInternal()
        {
            _gambleMeters.Clear();
            GameGambleMeters.Clear();

            _featureMeters.Clear();
            GameFeatureMeters.Clear();

            Games.Clear();

            base.DisposeInternal();
        }
    }
}