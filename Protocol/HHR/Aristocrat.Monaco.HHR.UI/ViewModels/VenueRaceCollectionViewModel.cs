namespace Aristocrat.Monaco.Hhr.UI.ViewModels
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
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
    using Kernel;
    using Localization.Properties;
    using log4net;
    using Models;
    using MVVM.ViewModel;
    using Storage.Helpers;

    public class VenueRaceCollectionViewModel : BaseViewModel, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly IPrizeInformationEntityHelper _entityHelper;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly IGamePlayEntityHelper _gamePlayEntityHelper;
        private new static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private CancellationTokenSource _cancelToken;
        private DateTime? _startTime;
        private int _remainingTime;
        private bool _isLaunched;
        private bool _isPaused;
        private readonly object _lock = new object();
        private bool _isAnimationVisible;
        private bool _raceStarted;
        private bool _disposed;

        public VenueRaceCollectionViewModel(
            IEventBus eventBus,
            IPrizeInformationEntityHelper entityHelper,
            ISystemDisableManager systemDisableManager,
            IGamePlayEntityHelper gamePlayEntityHelper,
            IPropertiesManager properties)
        {
            _entityHelper = entityHelper ?? throw new ArgumentNullException(nameof(entityHelper));
            _systemDisableManager = systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
            _gamePlayEntityHelper = gamePlayEntityHelper ?? throw new ArgumentNullException(nameof(gamePlayEntityHelper));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            var horseAnimationOff = properties.GetValue("horseAnimationOff", Constants.False).ToUpperInvariant();
            if (horseAnimationOff == Constants.False)
            {
                _eventBus.Subscribe<GamePlayStateInitializedEvent>(this, eventArgs => Handler(eventArgs.CurrentState));
                _eventBus.Subscribe<GamePlayStateChangedEvent>(this, eventArgs => Handler(eventArgs.CurrentState));

                _eventBus.Subscribe<GamePlayDisabledEvent>(this, GameDisabledEventHandler, _ => _isLaunched);
                _eventBus.Subscribe<GamePlayEnabledEvent>(this, GameEnabledEventHandler, _ => _isLaunched);

                _eventBus.Subscribe<OperatorMenuEnteredEvent>(this, Handler);
                _eventBus.Subscribe<OperatorMenuExitedEvent>(this, Handler);
            }


            if (int.TryParse(
                    properties.GetValue(
                        HHRPropertyNames.HorseResultsRunTime,
                        UiProperties.HorseResultsRunTimeMilliseconds.ToString()),
                    out int commandLineHorseResultsRunTime)
                && commandLineHorseResultsRunTime > 0)
            {
                UiProperties.HorseResultsRunTimeMilliseconds = commandLineHorseResultsRunTime;
            }

            InitializeModels();
        }

        private void InitializeModels()
        {
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

            for (int i = 0; i < 5; i++)
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

                for (int h = 0; h < HhrUiConstants.MaxNumberOfHorses; h++)
                {
                    venueRaceTracksModel1.RaceTrackModels.Add(Create(h));
                    venueRaceTracksModel2.RaceTrackModels.Add(Create(h));
                }
            }
        }

        private void Handler(OperatorMenuEnteredEvent theEvent)
        {
            if (!_isLaunched)
            {
                return;
            }

            Logger.Debug("Hiding horse animation");

            IsPaused = true;
            IsAnimationVisible = false;
        }

        private void Handler(OperatorMenuExitedEvent theEvent)
        {
            if (!_isLaunched)
            {
                return;
            }

            Logger.Debug("Unhiding horse animation");

            IsAnimationVisible = true;
        }

        private void Handler(PlayState playState)
        {
            /*
             * playState == PlayState.Initiated
             * this condition is added to handle the scenario when game ended before the animation
             * and this situation will come when we pass the following as command line argument
             * HorseResultsRunTime=10000 for testing purpose only to verify the animation timings
             * */

            if (playState == PlayState.GameEnded || playState == PlayState.Initiated)
            {
                _gamePlayEntityHelper.HorseAnimationFinished = false;

                return;
            }

            if (playState != PlayState.PrimaryGameStarted || _gamePlayEntityHelper.HorseAnimationFinished)
            {
                return;
            }

            _isLaunched = true;

            if (!_systemDisableManager.IsDisabled || _systemDisableManager.IsGamePlayAllowed())
            {
                ShowHorseAnimation();
            }
        }

        private void DelayedHideHorseAnimation(int delay)
        {
            _cancelToken = new CancellationTokenSource();
            _startTime = DateTime.Now;

            Task.Delay(delay, _cancelToken.Token)
                .ContinueWith(
                    t =>
                    {
                        if (_cancelToken.IsCancellationRequested)
                        {
                            return;
                        }

                        Logger.Debug("Removing horse animation");

                        RaceStarted = false;

                        _startTime = null;
                        _isLaunched = false;
                        _gamePlayEntityHelper.HorseAnimationFinished = true;

                        _cancelToken.Dispose();

                        RaisePropertyChanged(nameof(IsAnimationVisible));
                    }
                );
        }

        private void ShowHorseAnimation()
        {
            Logger.Debug("Showing horse animation");

            SetupRaces(_entityHelper.PrizeInformation.RaceInfo);

            _remainingTime = UiProperties.HorseResultsRunTimeMilliseconds;
            _gamePlayEntityHelper.HorseAnimationFinished = false;

            RaceStarted = true;

            IsAnimationVisible = true;

            RaisePropertyChanged(nameof(IsAnimationVisible));

            DelayedHideHorseAnimation(UiProperties.HorseResultsRunTimeMilliseconds);
        }

        private void GameDisabledEventHandler(GamePlayDisabledEvent theEvent)
        {
            IsPaused = true;

            if (_startTime == null || _cancelToken.IsCancellationRequested)
            {
                return;
            }

            var timeElapsed = (int)(DateTime.Now - (DateTime)_startTime).TotalMilliseconds;

            if (_remainingTime - timeElapsed > 0)
            {
                _remainingTime -= timeElapsed;
            }
            else
            {
                _remainingTime = 0;
            }

            _cancelToken.Cancel();
        }

        private void GameEnabledEventHandler(GamePlayEnabledEvent theEvent)
        {
            IsPaused = false;

            if (_startTime == null)
            {
                ShowHorseAnimation();
            }

            DelayedHideHorseAnimation(_remainingTime);
        }

        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                if (_isPaused == value)
                {
                    return;
                }

                _isPaused = value;
                RaisePropertyChanged(nameof(IsPaused));
            }
        }

        public int VenuesPerRow { get; } = 5;

        public ObservableCollection<VenueRaceTracksModel> RaceSet1Models { get; } =
            new ObservableCollection<VenueRaceTracksModel>();

        public ObservableCollection<VenueRaceTracksModel> RaceSet2Models { get; } =
            new ObservableCollection<VenueRaceTracksModel>();

        public int GetHorseNumberFromActual(char ch) =>  Convert.ToByte(ch.ToString(), 16);

        public int[] GetHorseActualArray(string horseActual)
        {
            List<int> horseActuals = new List<int>();
            foreach (var ch in horseActual)
            {
                horseActuals.Add(GetHorseNumberFromActual(ch));
            }

            return horseActuals.ToArray();
        }

        public void SetupRaces(CRaceInfo raceInfo)
        {
            Logger.Debug("Setup races");

            for (int i = 0; i < VenuesPerRow; i++)
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
            for (int w = 0; w < HhrUiConstants.MaxNumberOfHorses; w++)
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
        }

        public IEnumerable RaceSet1Title => Localizer.For(CultureFor.Player).GetString(ResourceKeys.HhrRaceSet1).ToList();

        public IEnumerable RaceSet2Title => Localizer.For(CultureFor.Player).GetString(ResourceKeys.HhrRaceSet2).ToList();

        public bool RaceStarted
        {
            get => _raceStarted;
            set
            {
                lock (_lock)
                {
                    foreach (var venue in RaceSet1Models)
                    {
                        venue.RaceStarted = value;
                    }

                    foreach (var venue in RaceSet2Models)
                    {
                        venue.RaceStarted = value;
                    }
                }

                SetProperty(ref _raceStarted, value, nameof(RaceStarted));

                RaisePropertyChanged(nameof(IsAnimationVisible));
            }
        }

        public bool IsAnimationVisible
        {
            get => _isAnimationVisible && RaceStarted;
            set => SetProperty(ref _isAnimationVisible, value, nameof(IsAnimationVisible));
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

                if (_cancelToken != null)
                {
                    _cancelToken.Dispose();
                }
            }

            _disposed = true;
        }
    }
}