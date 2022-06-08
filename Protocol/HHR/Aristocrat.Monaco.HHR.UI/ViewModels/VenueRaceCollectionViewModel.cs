namespace Aristocrat.Monaco.Hhr.UI.ViewModels
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
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
    using log4net;
    using Models;
    using MVVM;
    using MVVM.ViewModel;
    using Storage.Helpers;

    public class VenueRaceCollectionViewModel : BaseViewModel, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly IPrizeInformationEntityHelper _prizeEntityHelper;
        private readonly IRuntimeFlagHandler _runtimeFlagHandler;
        private readonly ManualResetEvent _horsesShowing = new ManualResetEvent(true);
        private const string RaceFinishedName = "RaceFinished";

        private new static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private CancellationTokenSource _cancelToken;
        private DateTime _startTime = DateTime.MinValue;
        private int _remainingTime;
        private bool _isPaused;
        private bool _isAnimationVisible;
        private bool _raceStarted;
        private bool _disposed;
        private bool _willRecover;

        public VenueRaceCollectionViewModel(
            IEventBus eventBus,
            IPrizeInformationEntityHelper entityHelper,
            IPropertiesManager properties,
            IRuntimeFlagHandler runtimeFlagHandler)
        {
            _prizeEntityHelper = entityHelper ?? throw new ArgumentNullException(nameof(entityHelper));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _runtimeFlagHandler = runtimeFlagHandler ?? throw new ArgumentNullException(nameof(runtimeFlagHandler));

            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            var horseAnimationOff = properties.GetValue("horseAnimationOff", Constants.False).ToUpperInvariant();
            if (horseAnimationOff == Constants.False)
            {
                _eventBus.Subscribe<PrimaryGameEndedEvent>(this, Handler);
                _eventBus.Subscribe<GamePlayDisabledEvent>(this, GameDisabledEventHandler);
                _eventBus.Subscribe<GamePlayEnabledEvent>(this, GameEnabledEventHandler);
                _eventBus.Subscribe<OperatorMenuEnteredEvent>(this, Handler);
                _eventBus.Subscribe<GameInitializationCompletedEvent>(this, Handler);
                // Setup and show the animation as soon as we get the needed data from the server
                _eventBus.Subscribe<PrizeInformationEvent>(
                    this,
                    evt => ShowHorseAnimation(evt.PrizeInformation.RaceInfo));
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

        public CRaceInfo GetRaceInfo => _prizeEntityHelper.PrizeInformation.RaceInfo;

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

            for (int i = 0; i < 5; i++)
            {
                RaceSet1Models[i].RaceFinishedEventHandler += (sender, args) =>
                {
                    VenueRaceTracksModelOnPropertyChanged(new PropertyChangedEventArgs(RaceFinishedName));
                };

                RaceSet2Models[i].RaceFinishedEventHandler += (sender, args) =>
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
                        Logger.Debug($"VenueRaceTracksModelOnPropertyChanged Property: {e.PropertyName}");

                        if (IsAnimationVisible || RaceStarted)
                        {
                            var allRacesFinished =
                                RaceSet1Models.All(r => r.RaceFinished) &&
                                RaceSet2Models.All(r => r.RaceFinished);

                            if (allRacesFinished)
                            {
                                Logger.Debug($"All race animations are complete, hiding window");
                                RaceStarted = false;
                                IsAnimationVisible = false;
                                // Pause to ensure the GIFs aren't running when the window is hidden
                                IsPaused = true;
                            }
                            else
                            {
                                for (int i = 0; i < 5; i++)
                                {
                                    Logger.Debug(
                                        $"RaceSet1Models[{i}].RaceFinished: {RaceSet1Models[i].RaceFinished}, RaceSet2Models[{i}].RaceFinished: {RaceSet2Models[i].RaceFinished}");
                                }
                            }
                        }
                        else
                        {
                            Logger.Debug($"IsAnimationVisible: {IsAnimationVisible}, RaceStarted: {RaceStarted}");
                        }
                        break;
                    }
            }
        }

        private void Handler(OperatorMenuEnteredEvent theEvent)
        {
            Logger.Debug("OperatorMenuEnteredEvent");

            MvvmHelper.ExecuteOnUI(() =>
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

        private void Handler(PrimaryGameEndedEvent evt)
        {
            // If the game has ended and the horses are still running, stop them now. With
            // this commented out, the horses will ALWAYS run to their completion, and if the
            // AwaitingPlayerSelection flag is being used, will prevent another game from
            // starting until they have done so.
            //StopHorseAnimation();
        }

        private void Handler(GameInitializationCompletedEvent evt)
        {
            Logger.Debug($"GameInitializationCompletedEvent RaceStarted: {RaceStarted}, willRecover: {_willRecover}");

            MvvmHelper.ExecuteOnUI(() =>
            {
                if (!RaceStarted || !_willRecover)
                {
                    // If we aren't in recovery, or the races have started return now, another event handler
                    // will handle it
                    return;
                }

                ContinueRaces();
            });
        }

        private void ShowHorseAnimation(CRaceInfo raceInfo)
        {
            Logger.Debug("Setup and show horse animation");

            MvvmHelper.ExecuteOnUI(() =>
            {
                // Stop and clean up any currently running horses.
                IsPaused = false;
                _willRecover = false;
            });

            StopHorseAnimation();

            MvvmHelper.ExecuteOnUI(() =>
            {
                SetupRaces(raceInfo);

                RaceStarted = true;
                IsAnimationVisible = true;
                DelayedHideHorseAnimation(UiProperties.HorseResultsRunTimeMilliseconds).FireAndForget();
            });
        }

        private void StopHorseAnimation()
        {
            Logger.Debug("Stopping horse animation");

            _cancelToken?.Cancel();
            _horsesShowing.WaitOne();
        }

        private async Task DelayedHideHorseAnimation(int delay)
        {

            using (var source = new CancellationTokenSource())
            {
                _cancelToken = source;
                _horsesShowing.Reset();
                _startTime = DateTime.Now;
                _remainingTime = delay;

                try
                {
                    await Task.Delay(delay, source.Token);
                }
                catch (TaskCanceledException)
                {
                    // Do nothing. Continue to clean up or pause.
                }

                MvvmHelper.ExecuteOnUI(() =>
                {
                    if (IsPaused)
                    {
                        // TODO: Just hide and stop rather than clean up. This appears to work as is?
                        Logger.Debug("Pausing horse animation");
                    }
                    else
                    {
                        Logger.Debug("Removing horse animation");
                        _remainingTime = 0;
                        _willRecover = false;
                    }
                });

                _horsesShowing.Set();
                _cancelToken = null;
            }
        }

        private void GameDisabledEventHandler(GamePlayDisabledEvent evt)
        {
            Logger.Debug($"GameDisabledEventHandler");

            MvvmHelper.ExecuteOnUI(() =>
            {
                // If no animation is running, then don't do anything.
                if (_cancelToken == null)
                {
                    return;
                }

                // Pause any currently running horses, but don't clean them up.
                IsPaused = true;
            });

            StopHorseAnimation();

            // Calculate the remaining time the horses will need when restarted.
            var timeElapsed = (int)(DateTime.Now - _startTime).TotalMilliseconds;
            _remainingTime -= timeElapsed;
            if (_remainingTime < 0)
            {
                _remainingTime = 0;
            }
        }

        private void GameEnabledEventHandler(GamePlayEnabledEvent evt)
        {
            Logger.Debug($"GameEnabledEventHandler willRecover: {_willRecover}");

            MvvmHelper.ExecuteOnUI(() =>
            {
                if (_willRecover)
                {
                    // Do not resume the horses yet, once the game is loaded another event will let them
                    // reappear and continue on. This is to prevent any glitches that can happen by
                    // letting them continue while the game is loading for recovery
                    return;
                }

                ContinueRaces();
            });
        }

        private void ContinueRaces()
        {
            Logger.Debug("Continuing races");

            IsPaused = false;
            if (RaceStarted && _remainingTime > 0)
            {
                RaceStarted = true;
                IsAnimationVisible = true;
                DelayedHideHorseAnimation(_remainingTime).FireAndForget();
            }
        }

        public bool IsPaused
        {
            get => _isPaused;
            set => SetProperty(ref _isPaused, value);
        }

        public int VenuesPerRow { get; } = 5;

        public ObservableCollection<VenueRaceTracksModel> RaceSet1Models { get; } =
            new ObservableCollection<VenueRaceTracksModel>();

        public ObservableCollection<VenueRaceTracksModel> RaceSet2Models { get; } =
            new ObservableCollection<VenueRaceTracksModel>();

        public int GetHorseNumberFromActual(char ch) => Convert.ToByte(ch.ToString(), 16);

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
            venueRaceTrackModel.RaceFinished = true;
        }

        public IEnumerable RaceSet1Title => Localizer.For(CultureFor.Player).GetString(ResourceKeys.HhrRaceSet1).ToList();

        public IEnumerable RaceSet2Title => Localizer.For(CultureFor.Player).GetString(ResourceKeys.HhrRaceSet2).ToList();

        public bool RaceStarted
        {
            get
            {
                Logger.Debug($"Get RaceStarted: {_raceStarted}");
                return _raceStarted;
            }
            set
            {
                Logger.Debug($"Set RaceStarted: {value}");
                _runtimeFlagHandler.SetAwaitingPlayerSelection(value);

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
            get
            {
                Logger.Debug(
                    $"Get IsAnimationVisible: _isAnimationVisible: {_isAnimationVisible} && RaceStarted: {RaceStarted}, {_isAnimationVisible && RaceStarted}");
                return _isAnimationVisible && RaceStarted;
            }
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
                StopHorseAnimation();

                if (_cancelToken != null)
                {
                    _cancelToken.Dispose();
                }

                _horsesShowing.Dispose();
            }

            _disposed = true;
        }
    }
}