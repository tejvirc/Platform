namespace Aristocrat.Monaco.Hhr.UI.ViewModels
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Client.Data;
    using Common;
    using Gaming.Contracts;
    using Hhr.Events;
    using Kernel;
    using Localization.Properties;
    using Models;
    using MVVM.ViewModel;
    using Storage.Helpers;
    using log4net;

    public class VenueRaceCollectionViewModel : BaseViewModel, IDisposable
    {
        private const string RaceFinishedName = "RaceFinished";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly IPrizeInformationEntityHelper _prizeEntityHelper;
        private readonly IGamePlayState _gamePlayState;
        private readonly IGamePlayEntityHelper _gamePlayEntity;

        private bool _isPaused;
        private bool _isAnimationVisible;
        private bool _raceStarted;
        private bool _disposed;
        private bool _willRecover;

        public VenueRaceCollectionViewModel(
            IEventBus eventBus,
            IPrizeInformationEntityHelper prizeEntityHelper,
            IPropertiesManager properties,
            IGamePlayState gamePlayState,
            IGamePlayEntityHelper gamePlayEntity)
        {
            _prizeEntityHelper = prizeEntityHelper
                ?? throw new ArgumentNullException(nameof(prizeEntityHelper));
            _eventBus = eventBus
                ?? throw new ArgumentNullException(nameof(eventBus));
            _gamePlayState = gamePlayState
                ?? throw new ArgumentNullException(nameof(gamePlayState));
            _gamePlayEntity = gamePlayEntity
                ?? throw new ArgumentNullException(nameof(gamePlayEntity));

            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            var horseAnimationOff = properties.GetValue("horseAnimationOff", Constants.False).ToUpperInvariant();
            if (horseAnimationOff == Constants.False)
            {
                _eventBus.Subscribe<GamePlayDisabledEvent>(this, GameDisabledEventHandler);
                _eventBus.Subscribe<GamePlayEnabledEvent>(this, GameEnabledEventHandler);
                _eventBus.Subscribe<OperatorMenuEnteredEvent>(this, OperatorMenuEnteredEventHandler);
                _eventBus.Subscribe<GameInitializationCompletedEvent>(this, GameInitializationCompletedEventHandler);
                _eventBus.Subscribe<RecoveryStartedEvent>(this, RecoveryStartedEventHandler);
                // Setup and show the animation as soon as we get the needed data from the server
                _eventBus.Subscribe<PrizeInformationEvent>(this, PrizeInformationEventHandler);
            }

            if (int.TryParse(
                    properties.GetValue(
                        HHRPropertyNames.HorseResultsRunTime,
                        UiProperties.HorseResultsRunTimeMilliseconds.ToString()),
                    out var commandLineHorseResultsRunTime)
                && commandLineHorseResultsRunTime > 0)
            {
                UiProperties.HorseResultsRunTimeMilliseconds = commandLineHorseResultsRunTime;
            }

            InitializeModels();
        }

        public CRaceInfo? CurrentRaceInfo => _prizeEntityHelper.PrizeInformation?.RaceInfo;

        private void InitializeModels()
        {
            Logger.Debug("Initializing models");

            RaceTrackEntryModel Create(int h)
            {
                return new RaceTrackEntryModel
                {
                    Position = h + 1,
                    FinishPosition = 0,
                    RaceStarted = false,
                    Visibility = Visibility.Hidden
                };
            }

            for (var i = 0; i < VenuesPerRow; i++)
            {
                var venueRaceTracksModel1 = new VenueRaceTracksModel
                {
                    RaceTrackModels = new ObservableCollection<RaceTrackEntryModel>()
                };

                var venueRaceTracksModel2 = new VenueRaceTracksModel
                {
                    RaceTrackModels = new ObservableCollection<RaceTrackEntryModel>()
                };

                RaceSet1Models.Add(venueRaceTracksModel1);
                RaceSet2Models.Add(venueRaceTracksModel2);

                for (var h = 0; h < HhrUiConstants.MaxNumberOfHorses; h++)
                {
                    venueRaceTracksModel1.RaceTrackModels.Add(Create(h));
                    venueRaceTracksModel2.RaceTrackModels.Add(Create(h));
                }
            }

            for (var i = 0; i < VenuesPerRow; i++)
            {
                RaceSet1Models[i].RaceFinishedEventHandler += (_, _) =>
                {
                    VenueRaceTracksModelOnPropertyChanged(new PropertyChangedEventArgs(RaceFinishedName));
                };

                RaceSet2Models[i].RaceFinishedEventHandler += (_, _) =>
                {
                    VenueRaceTracksModelOnPropertyChanged(new PropertyChangedEventArgs(RaceFinishedName));
                };
            }
        }

        private void VenueRaceTracksModelOnPropertyChanged(PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case RaceFinishedName:
                {
                    if (IsAnimationVisible || RaceStarted)
                    {
                        var allRacesFinished =
                            RaceSet1Models.All(r => r.RaceFinished) &&
                            RaceSet2Models.All(r => r.RaceFinished);

                        if (allRacesFinished)
                        {
                            Logger.Debug("All race animations are complete, hiding window");
                            RaceStarted = false;
                            IsAnimationVisible = false;
                            // Pause to ensure the GIFs aren't running when the window is hidden
                            IsPaused = true;
                        }
                    }
                    break;
                }
            }
        }

        private Task PrizeInformationEventHandler(PrizeInformationEvent evt, CancellationToken ct)
        {
            Logger.Debug($"PrizeInformationEvent RaceStarted: {RaceStarted}, willRecover: {_willRecover}");
            return ShowHorseAnimation(evt.PrizeInformation.RaceInfo);
        }

        private async Task OperatorMenuEnteredEventHandler(OperatorMenuEnteredEvent theEvent, CancellationToken token)
        {
            Logger.Debug($"OperatorMenuEnteredEvent RaceStarted: {RaceStarted}, willRecover: {_willRecover}");

            await ExecuteOnUI(() =>
            {
                if (!RaceStarted)
                {
                    return;
                }

                Logger.Debug("Hiding horse animation");
                IsPaused = true;
                IsAnimationVisible = false;
                _willRecover = true;
            });
        }

        private void RecoveryStartedEventHandler(RecoveryStartedEvent evt)
        {
            _willRecover = true;
            Logger.Debug($"RecoveryStartedEvent RaceStarted: {RaceStarted}, willRecover: {_willRecover}");
        }

        private async Task GameInitializationCompletedEventHandler(GameInitializationCompletedEvent evt, CancellationToken token)
        {
            Logger.Debug($"GameInitializationCompletedEvent RaceStarted: {RaceStarted}, willRecover: {_willRecover}");

            await ExecuteOnUI(() =>
            {
                // If we aren't recovering, then do not show or continue the races on account of receiving
                // the GameInitializationCompletedEvent event
                if (!_willRecover)
                {
                    return;
                }

                // If the race was started, then we are coming out of the audit menu, so continue the races
                if (RaceStarted)
                {
                    ContinueRaces();
                }
                // If race hasn't started, then we are loading from a power-cycle, so show the entire race again
                else
                {
                    // First though, let's check if we had the results for the game already. If not, then
                    // it will arrive shortly as a PrizeInformationEvent, and we don't want to show the
                    // current PrizeInformation because that's the horses from the previous game.
                    if (CurrentRaceInfo.HasValue)
                    {
                        var gamePlayPending = _gamePlayEntity.GamePlayRequest != null;
                        var raceStartPending = _gamePlayEntity.RaceStartRequest != null;

                        if (!gamePlayPending && !raceStartPending)
                        {
                            Task.Run(() => ShowHorseAnimation(CurrentRaceInfo.Value), token);
                        }
                    }
                }
            });

            _willRecover = false;
        }

        private async Task GameDisabledEventHandler(GamePlayDisabledEvent evt, CancellationToken token)
        {
            Logger.Debug($"GameDisabledEventHandler RaceStarted: {RaceStarted}, willRecover: {_willRecover}");

            await ExecuteOnUI(() =>
            {
                // Pause any currently running horses, but don't clean them up.
                IsPaused = true;
            });
        }

        private async Task GameEnabledEventHandler(GamePlayEnabledEvent evt, CancellationToken token)
        {
            Logger.Debug($"GameEnabledEventHandler RaceStarted: {RaceStarted}, willRecover: {_willRecover}");

            await ExecuteOnUI(() =>
            {
                if (_willRecover || !RaceStarted)
                {
                    // Do not resume the horses yet, once the game is loaded another event will let them
                    // reappear and continue on. This is to prevent any glitches that can happen by
                    // letting them continue while the game is loading for recovery
                    return;
                }

                ContinueRaces();
            });
        }

        private static async Task ExecuteOnUI(Action action)
        {
            var dispatcher = Application.Current.Dispatcher;
            if (dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                await dispatcher.BeginInvoke(action);
            }
        }

        private async Task ShowHorseAnimation(CRaceInfo raceInfo)
        {
            Logger.Debug("Setup and show horse animation");

            await ExecuteOnUI(() =>
            {
                // Stop and clean up any currently running horses.
                IsPaused = false;
                RaceStarted = false;

                SetupRaces(raceInfo);

                RaceStarted = true;
                IsAnimationVisible = true;

                // If the game was started, and then a lockup created before the races started, pause
                if (!_gamePlayState.Enabled)
                {
                    Logger.Debug("System is disabled, pausing horses right after start");
                    IsPaused = true;
                }
            });
        }

        private void ContinueRaces()
        {
            Logger.Debug("Continuing races");

            IsPaused = false;
            if (RaceStarted)
            {
                RaceStarted = true;
                IsAnimationVisible = true;
            }
        }

        public bool IsPaused
        {
            get => _isPaused;
            set => SetProperty(ref _isPaused, value);
        }

        public int VenuesPerRow { get; } = 5;

        public ObservableCollection<VenueRaceTracksModel> RaceSet1Models { get; } = new();

        public ObservableCollection<VenueRaceTracksModel> RaceSet2Models { get; } = new();

        public int GetHorseNumberFromActual(char ch) => Convert.ToByte(ch.ToString(), 16);

        public int[] GetHorseActualArray(string horseActual)
        {
            return horseActual.Select(GetHorseNumberFromActual).ToArray();
        }

        public void SetupRaces(CRaceInfo raceInfo)
        {
            for (var i = 0; i < VenuesPerRow; i++)
            {
                BuildVenueRaceTracksModel(
                    RaceSet1Models[i],
                    raceInfo.RaceData[i].TrackDescription,
                    GetHorseActualArray(raceInfo.RaceData[i].HorseActual));

                BuildVenueRaceTracksModel(
                    RaceSet2Models[i],
                    raceInfo.RaceData[i + VenuesPerRow].TrackDescription,
                    GetHorseActualArray(raceInfo.RaceData[i + VenuesPerRow].HorseActual));
            }
        }

        public void BuildVenueRaceTracksModel(VenueRaceTracksModel venueRaceTrackModel, string venueName, int[] horseActual)
        {
            for (var w = 0; w < HhrUiConstants.MaxNumberOfHorses; w++)
            {
                if (w < horseActual.Length)
                {
                    venueRaceTrackModel.RaceTrackModels[w].FinishPosition = horseActual.ToList().IndexOf(w + 1) + 1;
                    venueRaceTrackModel.RaceTrackModels[w].RaceStarted = false;
                    venueRaceTrackModel.RaceTrackModels[w].Visibility = Visibility.Visible;
                }
                else
                {
                    venueRaceTrackModel.RaceTrackModels[w].FinishPosition = 0;
                    venueRaceTrackModel.RaceTrackModels[w].RaceStarted = false;
                    venueRaceTrackModel.RaceTrackModels[w].Visibility = Visibility.Hidden;
                }
            }

            venueRaceTrackModel.VenueName = venueName;
            venueRaceTrackModel.RaceStarted = false;
            venueRaceTrackModel.RaceFinished = true;
        }

        public IEnumerable RaceSet1Title => Localizer.For(CultureFor.Player).GetString(ResourceKeys.HhrRaceSet1).ToList();

        public IEnumerable RaceSet2Title => Localizer.For(CultureFor.Player).GetString(ResourceKeys.HhrRaceSet2).ToList();

        public bool RaceStarted
        {
            get => _raceStarted;
            set
            {
                foreach (var venue in RaceSet1Models)
                {
                    venue.RaceStarted = value;
                }

                foreach (var venue in RaceSet2Models)
                {
                    venue.RaceStarted = value;
                }

                _raceStarted = value;
                RaisePropertyChanged(nameof(RaceStarted), nameof(IsAnimationVisible));
            }
        }

        public bool IsAnimationVisible
        {
            get => _isAnimationVisible && RaceStarted;
            set
            {
                Logger.Debug($"Set IsAnimationVisible: {value}");
                _isAnimationVisible = value;
                RaisePropertyChanged(nameof(IsAnimationVisible), nameof(RaceStarted));
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }
    }
}