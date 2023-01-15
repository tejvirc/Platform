namespace Aristocrat.Monaco.Hhr.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Accounting.Contracts.Handpay;
    using Application.Contracts.Localization;
    using Client.Messages;
    using Client.Data;
    using Controls;
    using Events;
    using Gaming.Contracts;
    using Hardware.Contracts.Button;
    using Hhr.Services;
    using Hhr.Events;
    using Kernel;
    using Kernel.Contracts.MessageDisplay;
    using Localization.Properties;
    using Menu;
    using Models;
    using MVVM.Command;
    using Storage.Helpers;
    using Command = Menu.Command;

    public class ManualHandicapPageViewModel : HhrMenuPageViewModelBase
    {
        private readonly IPropertiesManager _properties;
        private readonly IEventBus _eventBus;
        private readonly ISystemDisableManager _systemDisable;
        private readonly IPrizeDeterminationService _prizeDeterminationService;
        private readonly IManualHandicapEntityHelper _manualHandicapEntityHelper;
        private readonly IGameProvider _gameProvider;

        private readonly Dictionary<int, RaceInfo> _racesToHandicap = new Dictionary<int, RaceInfo>();
        private readonly HHRTimer _manualHandicapTimer;

        private bool _gameStarted;
        private bool _raceSelectionCompleted;
        private int _currentRaceIndex;
        private int _tickCount;
        private bool _disposed;

        public ManualHandicapPageViewModel(
            IPropertiesManager properties,
            IEventBus eventBus,
            ISystemDisableManager systemDisable,
            IPrizeDeterminationService prizeDeterminationService,
            IManualHandicapEntityHelper manualHandicapEntityHelper,
            IGameProvider gameProvider)
        {
            _properties = properties
                ?? throw new ArgumentNullException(nameof(properties));
            _eventBus = eventBus
                ?? throw new ArgumentNullException(nameof(eventBus));
            _systemDisable = systemDisable
                ?? throw new ArgumentNullException(nameof(systemDisable));
            _prizeDeterminationService = prizeDeterminationService
                ?? throw new ArgumentNullException(nameof(prizeDeterminationService));
            _manualHandicapEntityHelper = manualHandicapEntityHelper
                ?? throw new ArgumentNullException(nameof(manualHandicapEntityHelper));
            _gameProvider = gameProvider
                ?? throw new ArgumentNullException(nameof(gameProvider));

            HorseNumberClicked = new ActionCommand<object>(OnHorseNumberClicked);

            _tickCount = 0;
            _manualHandicapTimer = new HHRTimer(1000);
            _manualHandicapTimer.Elapsed += (_, _) => OnTickElapsed();

            _eventBus.Subscribe<ManualHandicapAbortedEvent>(this, Handle);
            _eventBus.Subscribe<StartQuickPickEvent>(this, Handle);
        }

        public int TotalRaces => _racesToHandicap?.Count ?? 0;

        public ObservableCollection<HorsePositionModel> CurrentHorsePicks =>
            _racesToHandicap?.Count > 0
                ? _racesToHandicap[CurrentRaceIndex].HorsePositionPicks
                : null;

        public ObservableCollection<HorseModel> CurrentHorseNumbers =>
            _racesToHandicap?.Count > 0
                ? _racesToHandicap?[CurrentRaceIndex].Horses
                : null;

        public string HhrHorseTitle => Localizer.GetString(ResourceKeys.HhrHorseTitle, CultureProviderType.Player);

        public string HhrManualHandicapDisclaimerMessage => Localizer.GetString(ResourceKeys.HhrManualHandicapDisclaimerMessage, CultureProviderType.Player);

        public string HhrYourPicksTitle => Localizer.GetString(ResourceKeys.HhrYourPicksTitle, CultureProviderType.Player);

        public ICommand HorseNumberClicked { get; }

        public bool IsCurrentRaceSelectionCompleted => IsRaceSelectionCompleted(_racesToHandicap[CurrentRaceIndex]);

        public bool RaceSelectionCompleted
        {
            get => _raceSelectionCompleted;
            set
            {
                if (value != _raceSelectionCompleted)
                {
                    _raceSelectionCompleted = value;
                    RaisePropertyChanged(nameof(RaceSelectionCompleted));
                }
            }
        }

        public int RemainingRacesToHandicap => TotalRaces - CurrentRaceIndex;

        public string HhrRemainingRacesToHandicapTitle => Localizer.GetString(
            ResourceKeys.HhrRemainingRacesToHandicapTitle,
            CultureProviderType.Player);

        private HhrPageCommand RaceCommand { get; set; }

        private HhrPageCommand NextRaceCommand { get; set; }

        private HhrPageCommand RaceStatsCommand { get; set; }

        public int CurrentRaceIndex
        {
            get => _currentRaceIndex;
            set
            {
                _currentRaceIndex = value;
                UiProperties.CurrentRaceIndex = _currentRaceIndex;
            }
        }

        private readonly CRaceInfo _emptyRaceInfo = new CRaceInfo
        {
            RaceData = new[]
            {
                new CRaceData { Runners = 8 },
                new CRaceData { Runners = 8 },
                new CRaceData { Runners = 8 },
                new CRaceData { Runners = 8 },
                new CRaceData { Runners = 8 },
                new CRaceData { Runners = 8 },
                new CRaceData { Runners = 8 },
                new CRaceData { Runners = 8 },
                new CRaceData { Runners = 8 },
                new CRaceData { Runners = 8 },
            }
        };

        public override Task Init(Command executeCommand)
        {
            Commands.Add(new HhrPageCommand(PageCommandHandler, true, IsQuickPick ? Command.QuickPick : Command.AutoPick));

            Commands.Add(new HhrPageCommand(PageCommandHandler, true, Command.Reset));

            RaceStatsCommand = new HhrPageCommand(PageCommandHandler, true, Command.RaceStats);
            Commands.Add(RaceStatsCommand);

            NextRaceCommand = new HhrPageCommand(PageCommandHandler, false, Command.NextRace);
            Commands.Add(NextRaceCommand);

            RaceCommand = new HhrPageCommand(PageCommandHandler, false, Command.Race);
            Commands.Add(RaceCommand);

            // Manual handicap has just started, setup the display
            if (executeCommand == Command.ManualHandicap)
            {
                // Ensure all events and properties are reset before staring a new manual handicap
                ManualHandicapRemainingTime = ClientProperties.ManualHandicapTimeOut;

                Cleanup();

                // Game Play would be allowed if got the Normal lockup in Manual Handicapping page 
                _properties.SetProperty(GamingConstants.AdditionalInfoGameInProgress, true);

                _eventBus.Subscribe<GameEndedEvent>(this, Handle);
                _eventBus.Subscribe<HandpayCompletedEvent>(this, Handle);
                _eventBus.Subscribe<AwaitingPlayerSelectionChangedEvent>(this, Handle);

                Util.LoadImageResources();

                CurrentRaceIndex = 0;
                _raceSelectionCompleted = false;

                SetData(_emptyRaceInfo, true);

                _tickCount = 0;
                ConfigureTimer(false);

                LoadRaceInfo();
            }
            else
            {
                HandleReturningFromRaceStatsPage();
                ConfigureTimer(true);
            }

            UpdateView();

            return Task.CompletedTask;
        }

        private async void LoadRaceInfo()
        {
            var gameId = _properties.GetValue(GamingConstants.SelectedGameId, 0);
            var referenceId = Convert.ToUInt32(_gameProvider.GetGame(gameId)?.ReferenceId);

            _systemDisable.Disable(
                HhrUiConstants.WaitingForRaceInfoGuid,
                SystemDisablePriority.Immediate,
                ResourceKeys.WaitingForRaceInfo,
                CultureProviderType.Player);

            var numberOfCredits = _properties.GetValue(GamingConstants.SelectedBetCredits, (long)0);
            var result = await _prizeDeterminationService.RequestRaceInfo(referenceId, (uint)numberOfCredits);

            if (result != null && result.Value.RaceData.Length > 0)
            {
                SetData(result.Value);
            }

            _systemDisable.Enable(HhrUiConstants.WaitingForRaceInfoGuid);

            _manualHandicapTimer.Start();
            TimerInfo.IsEnabled = true;

            UpdateView();
        }

        public void Handle(ManualHandicapAbortedEvent evt)
        {
            // If we couldn't get the data from the server, return to previous screen (Previous Results page)
            OnHhrButtonClicked(Command.PreviousResults);
            Cleanup();
        }

        private void ConfigureTimer(bool startAndShow)
        {
            TimerInfo = new TimerInfo
            {
                TimerElapsedCommand = new ActionCommand<object>(OnTimerElapsed),
                Timeout = ManualHandicapRemainingTime,
                IsVisible = true,
                IsQuickPickTextVisible = ClientProperties.ManualHandicapMode == HhrConstants.QuickPickMode,
                IsAutoPickTextVisible = ClientProperties.ManualHandicapMode != HhrConstants.QuickPickMode,
                IsEnabled = startAndShow
            };
        }

        private void HandleReturningFromRaceStatsPage()
        {
            // Rules are you can only look at the Race Stats page once per race, so hide the button
            RaceStatsCommand.Visibility = false;

            // Any picks must be cleared upon returning from the Race Stats page
            ClearRaceSelection(true);
        }

        public bool AreAllRaceSelectionsCompleted()
        {
            foreach (var race in _racesToHandicap)
            {
                if (!IsRaceSelectionCompleted(race.Value))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsRaceSelectionCompleted(RaceInfo race)
        {
            // If there are no more horse numbers, then the selection is complete. Note there can 
            // be more or less than 8 horses to pick from, but only 8 positions to place them into
            var filledPositions =
                race.HorsePositionPicks
                    .Where(h => h.HorseInfo != null)
                    .ToList();

            if (filledPositions.Count == race.Horses.Count)
            {
                return true;
            }
            else if (filledPositions.Count == HhrUiConstants.NumberOfHorsePickPositions)
            {
                return true;
            }

            return false;
        }

        public bool IsQuickPick => ClientProperties.ManualHandicapMode == HhrConstants.QuickPickMode;

        public void PageCommandHandler(object obj)
        {
            switch ((Command)obj)
            {
                case Command.QuickPick:
                case Command.AutoPick:
                    {
                        ClearRaceSelection(false);

                        if (IsQuickPick)
                        {
                            QuickPickRaceSelections();
                        }
                        else
                        {
                            _prizeDeterminationService.ClearManualHandicapData();
                        }

                        UpdateView();
                        HandicapCompletedAndExiting();

                        break;
                    }

                case Command.Reset:
                    {
                        ClearRaceSelection(true);

                        UpdateView();

                        break;
                    }

                case Command.RaceStats:
                    {
                        OnHhrButtonClicked(Command.RaceStats);

                        break;
                    }

                case Command.NextRace:
                    {
                        if (CurrentRaceIndex == TotalRaces - 1)
                        {
                            return;
                        }

                        if (IsCurrentRaceSelectionCompleted)
                        {
                            CurrentRaceIndex++;

                            // The next race button has been pressed, ensure the Race Stats button is visible
                            RaceStatsCommand.Visibility = true;
                        }

                        UpdateRaceButtonsVisibility();

                        UpdateView();

                        break;
                    }

                case Command.Race:
                    {
                        HandicapCompletedAndExiting();

                        break;
                    }
            }
        }

        private void HandicapCompletedAndExiting()
        {
            Logger.Debug("Exiting");

            _tickCount = 0;
            _manualHandicapTimer?.Stop();
            ManualHandicapRemainingTime = 0;

            _prizeDeterminationService.SetHandicapPicks(GetPersistedPicks());

            _manualHandicapEntityHelper.IsCompleted = true;

            OnHhrButtonClicked(Command.HandicapCompleted);
        }

        private void Handle(AwaitingPlayerSelectionChangedEvent evt)
        {
            Logger.Debug($"Received AwaitingPlayerSelectionChangedEvent with value {evt.AwaitingPlayerSelection} ");
            if (evt.AwaitingPlayerSelection)
            {
                return;
            }

            _eventBus.Unsubscribe<AwaitingPlayerSelectionChangedEvent>(this);
            _eventBus.Publish(new DownEvent((int)ButtonLogicalId.Play));

            _gameStarted = false;
            _eventBus.Subscribe<PrimaryGameEscrowEvent>(this, e =>
            {
                _gameStarted = true;
                _eventBus.Unsubscribe<PrimaryGameEscrowEvent>(this);
            });

            _eventBus.Subscribe<GamePlayEnabledEvent>(this, e =>
            {
                Logger.Debug($"GameStarted is {_gameStarted}");
                if (!_gameStarted)
                {
                    _eventBus.Publish(new DownEvent((int)ButtonLogicalId.Play));
                }
            });
        }

        public ReadOnlyCollection<string> GetPersistedPicks()
        {
            var handicappedRaces = new List<string>();
            foreach (var entry in _racesToHandicap)
            {
                var race = entry.Value;

                var raceString = "";
                foreach (var horsePick in race.HorsePositionPicks)
                {
                    if (horsePick.HorseNumber != 0)
                    {
                        raceString += horsePick.HorseNumber.ToString("X");
                    }
                }

                handicappedRaces.Add(raceString);
            }

            return new ReadOnlyCollection<string>(handicappedRaces);
        }

        public void SetData(CRaceInfo raceInfo, bool empty = false)
        {
            _racesToHandicap.Clear();

            for (var i = 0; i < raceInfo.RaceData.Length; i++)
            {
                var horses = SetupHorseNumbers(raceInfo.RaceData[i].Runners, empty);
                var myRaceInfo = new RaceInfo { HorsePositionPicks = SetupPickPositions(), Horses = horses };

                _racesToHandicap.Add(i, myRaceInfo);
            }

            UpdateView();
        }

        private void UpdateRaceButtonsVisibility()
        {
            // If all picks are chosen for all races, enable the Race button
            if (AreAllRaceSelectionsCompleted())
            {
                NextRaceCommand.Visibility = false;
                RaceCommand.Visibility = true;
            }

            // If the current race pick selection is complete, show the next race button
            else if (IsCurrentRaceSelectionCompleted)
            {
                NextRaceCommand.Visibility = true;
                RaceCommand.Visibility = false;
            }

            // Otherwise, both buttons should be invisible
            else
            {
                NextRaceCommand.Visibility = false;
                RaceCommand.Visibility = false;
            }
        }

        public void OnHorseNumberClicked(object obj)
        {
            if (obj is ManualHandicapHorseNumber hnc)
            {
                //Find empty horse position
                var emptyHorsePosition = CurrentHorsePicks.FirstOrDefault(h => h.HorseInfo == null);
                if (emptyHorsePosition != null)
                {
                    var expectedHorseWinPosition = emptyHorsePosition.HorsePosition;
                    var chosenHorseNum = hnc.HorseNumber;

                    emptyHorsePosition.HorseInfo = new HorseModel(
                        chosenHorseNum,
                        expectedHorseWinPosition,
                        true);

                    var horseNumber = CurrentHorseNumbers.FirstOrDefault(h => h.Number == chosenHorseNum);

                    if (horseNumber != null)
                    {
                        horseNumber.RacePosition = 0; // Hide Horse number as it's selected now
                    }

                    UpdateRaceButtonsVisibility();

                    UpdateView();
                }
            }
        }

        private void ClearRaceSelection(bool currentRaceOnly)
        {
            if (currentRaceOnly)
            {
                ResetRace(_racesToHandicap[CurrentRaceIndex]);
            }
            else
            {
                foreach (var race in _racesToHandicap)
                {
                    ResetRace(race.Value);
                }

                CurrentRaceIndex = 0;
            }

            RaceSelectionCompleted = false;
        }

        private void ResetRace(RaceInfo race)
        {
            // Reset Horse numbers
            foreach (var horseNumber in race.Horses)
            {
                horseNumber.RacePosition = horseNumber.Number;
            }

            // Reset Horse position selections
            foreach (var raceSelection in race.HorsePositionPicks)
            {
                raceSelection.HorseInfo = null;
            }

            UpdateRaceButtonsVisibility();
        }

        private void QuickPickRaceSelections()
        {
            CurrentRaceIndex = 0;
            for (; CurrentRaceIndex < TotalRaces; ++CurrentRaceIndex)
            {
                var shuffledHorses = CurrentHorseNumbers.ToList();
                shuffledHorses.Shuffle();

                var placementsAvailable = Math.Min(CurrentHorsePicks.Count, CurrentHorseNumbers.Count);

                for (var i = 0; i < placementsAvailable; ++i)
                {
                    CurrentHorsePicks[i].HorseInfo = shuffledHorses[i];
                }
            }

            CurrentRaceIndex = 0;
            RaceSelectionCompleted = true;
        }

        private void UpdateView()
        {
            RaisePropertyChanged(nameof(TimerInfo));
            RaisePropertyChanged(nameof(CurrentRaceIndex));
            RaisePropertyChanged(nameof(CurrentHorsePicks));
            RaisePropertyChanged(nameof(CurrentHorseNumbers));
            RaisePropertyChanged(nameof(RemainingRacesToHandicap));
        }

        private void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e != null)
            {
                RaisePropertyChanged(e.PropertyName);
            }
        }

        private ObservableCollection<HorseModel> SetupHorseNumbers(uint totalHorses, bool empty = false)
        {
            var horseList = new ObservableCollection<HorseModel>();

            var numHorses =
                totalHorses > HhrUiConstants.MaxNumberOfHorses
                    ? HhrUiConstants.MaxNumberOfHorses
                    : totalHorses;

            for (var i = 1; i <= numHorses; i++)
            {
                var horseIdx = empty ? 0 : i;

                // This will list the horse numbers in order from 1 to numHorses
                var horse = new HorseModel(horseIdx, i, true);
                horse.PropertyChanged += HandlePropertyChanged;
                horseList.Add(horse);
            }

            return horseList;
        }

        private int ManualHandicapRemainingTime
        {
            get => UiProperties.ManualHandicapRemainingTime;
            set => UiProperties.ManualHandicapRemainingTime = value;
        }

        private ObservableCollection<HorsePositionModel> SetupPickPositions()
        {
            var horseList = new ObservableCollection<HorsePositionModel>();

            for (var i = 1; i <= HhrUiConstants.NumberOfHorsePickPositions; i++)
            {
                var horsePos = new HorsePositionModel(i);
                horsePos.PropertyChanged += HandlePropertyChanged;
                horseList.Add(horsePos);
            }

            return horseList;
        }

        private void OnTickElapsed()
        {
            _tickCount++;
            if (_tickCount >= ClientProperties.ManualHandicapTimeOut)
            {
                _tickCount = 0;
                ManualHandicapRemainingTime = 0;
                _manualHandicapTimer.Stop();
                return;
            }
            ManualHandicapRemainingTime = ClientProperties.ManualHandicapTimeOut - _tickCount;
        }

        private void Handle(GameEndedEvent evt)
        {
            Cleanup();
        }

        private void Handle(HandpayCompletedEvent evt)
        {
            Cleanup();
        }

        private void OnTimerElapsed(object obj)
        {
            OnHandlePlacard(
                new PlacardEventArgs(
                    IsQuickPick ? Placard.TimerExpireQuick : Placard.TimerExpireAuto,
                    true,
                    HhrUiConstants.TimerExpirePlacardTimeout,
                    () => PageCommandHandler(Command.QuickPick)));
        }

        public void Handle(StartQuickPickEvent evt)
        {
            PageCommandHandler(Command.QuickPick);
        }

        private void Cleanup()
        {
            Logger.Debug("Cleanup");

            _manualHandicapEntityHelper.IsCompleted = false;
            _gameStarted = false;

            _properties.SetProperty(GamingConstants.HandpayPresentationOverride, false);
            _properties.SetProperty(GamingConstants.AdditionalInfoGameInProgress, false);

            _eventBus.Unsubscribe<GameEndedEvent>(this);
            _eventBus.Unsubscribe<HandpayCompletedEvent>(this);
            _eventBus.Unsubscribe<AwaitingPlayerSelectionChangedEvent>(this);
            _eventBus.Unsubscribe<GamePlayEnabledEvent>(this);
            _eventBus.Unsubscribe<PrimaryGameEscrowEvent>(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
                _manualHandicapTimer.Dispose();
            }

            _disposed = true;

            base.Dispose(disposing);
        }
    }
}