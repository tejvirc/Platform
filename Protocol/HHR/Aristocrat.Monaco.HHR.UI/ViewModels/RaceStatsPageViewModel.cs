namespace Aristocrat.Monaco.Hhr.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Events;
    using Client.Data;
    using Client.Messages;
    using Gaming.Contracts;
    using Hhr.Services;
    using Kernel;
    using Menu;
    using Models;
    using Command = Menu.Command;
    using CommunityToolkit.Mvvm.Input;

    public class RaceStatsPageViewModel : HhrMenuPageViewModelBase
    {
        private readonly IPropertiesManager _properties;
        private readonly IPrizeDeterminationService _prizeDeterminationService;
        private readonly IEventBus _eventBus;

        private readonly IList<IList<RaceStatsChartModel>> _raceStatsCharts;

        public RaceStatsPageViewModel(
            IPropertiesManager properties,
            IPrizeDeterminationService prizeDeterminationService,
            IEventBus eventBus)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _prizeDeterminationService = prizeDeterminationService ?? throw new ArgumentNullException(nameof(prizeDeterminationService));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _raceStatsCharts = new List<IList<RaceStatsChartModel>>();
        }

        private async Task LoadRaceInfo()
        {
            var gameId = (uint)_properties.GetValue(GamingConstants.SelectedGameId, 0);

            var result = await _prizeDeterminationService.RequestRaceInfo(gameId, 0);

            if (result != null && result.Value.RaceData?.Length > 0)
            {
                SetData(result.Value);

                UpdateView();
            }
        }

        public override async Task Init(Command command)
        {
            Commands.Add(new HhrPageCommand(BackHandler, true, Command.Back));

            ConfigureTimer();

            await LoadRaceInfo();
        }

        public override void Reset()
        {
            base.Reset();
            _raceStatsCharts.Clear();
            UpdateView();
        }

        private void UpdateView()
        {
            OnPropertyChanged(nameof(RaceStatsCharts));
        }

        private void BackHandler(object obj)
        {
            OnHhrButtonClicked(Command.Back);
        }

        private void OnTimerElapsed(object obj)
        {
            if (UiProperties.ManualHandicapRemainingTime <= 0)
            {
                OnHandlePlacard(
                    new PlacardEventArgs(
                        ClientProperties.ManualHandicapMode == HhrConstants.QuickPickMode ?
                            Placard.TimerExpireQuick : Placard.TimerExpireAuto,
                        true,
                        HhrUiConstants.TimerExpirePlacardTimeout,
                        StartQuickPickAndReturnToGame));
            }
            else
            {
                OnHhrButtonClicked(Command.StatPageTimerExpire);
            }
        }

        private void StartQuickPickAndReturnToGame()
        {
            _eventBus.Publish(new StartQuickPickEvent());
            OnHhrButtonClicked(Command.ReturnToGame);
        }

        public IList<RaceStatsChartModel> RaceStatsCharts =>
            _raceStatsCharts?.Count > 0
                ? _raceStatsCharts[UiProperties.CurrentRaceIndex]
                : null;

        public void SetData(CRaceInfo raceInfo)
        {
            for (var raceIndex = 0; raceIndex < raceInfo.RaceData.Length; raceIndex++)
            {
                IList<RaceStatsChartModel> raceStatsChart = new List<RaceStatsChartModel>();
                for (int statChartIndex = 0; statChartIndex < raceInfo.RaceData[raceIndex].Racestat.Length; statChartIndex++)
                {
                    List<WinningOddsModel> winningOddsModel = new List<WinningOddsModel>();
                    for (int horseIndex = 0; horseIndex < raceInfo.RaceData[raceIndex].Runners; horseIndex++)
                    {
                        winningOddsModel.Add(new WinningOddsModel(horseIndex + 1, (int)raceInfo.RaceData[raceIndex].Racestat[statChartIndex].StatNum[horseIndex]));
                    }
                    raceStatsChart.Add(new RaceStatsChartModel(winningOddsModel, raceInfo.RaceData[raceIndex].Racestat[statChartIndex].StatStr));
                }
                _raceStatsCharts.Add(raceStatsChart);
            }
        }

        private void ConfigureTimer()
        {
            TimerInfo = new TimerInfo
            {
                Timeout = Math.Min(UiProperties.ManualHandicapRemainingTime, ClientProperties.RaceStatTimeOut),
                TimerElapsedCommand = new RelayCommand<object>(OnTimerElapsed),
                IsVisible = true,
                IsQuickPickTextVisible = false,
                IsAutoPickTextVisible = false,
                IsEnabled = true
            };
        }
    }
}
