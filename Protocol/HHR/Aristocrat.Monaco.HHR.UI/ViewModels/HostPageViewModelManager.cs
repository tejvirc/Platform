namespace Aristocrat.Monaco.Hhr.UI.ViewModels
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts;
    using Cabinet.Contracts;
    using Gaming.Contracts;
    using Kernel;
    using log4net;
    using Menu;
    using MVVM.ViewModel;
    using Services;
    using Views;
    using MVVM;
    using System.Windows;

    public class HostPageViewModelManager : BaseEntityViewModel, IMenuAccessService, IDisposable
    {
        private static readonly Guid RaceInfoTransactionRequestorId = new Guid("5297D108-9F34-4174-BE9F-716538519FBA");

        private readonly IEventBus _eventBus;
        private readonly IRuntimeFlagHandler _runtimeFlagHandler;
        private readonly IPropertiesManager _properties;

        private readonly ManualHandicapPageViewModel _manualHandicapPageViewModel;
        private readonly RaceStatsPageViewModel _raceStatsPageViewModel;
        private readonly WinningCombinationPageViewModel _winningCombinationPageViewModel;
        private readonly ManualHandicapHelpPageViewModel _manualHandicapHelpPageViewModel;
        private readonly PreviousRaceResultPageViewModel _previousRaceResultPageViewModel;
        private readonly CurrentProgressivePageViewModel _currentProgressivePageViewModel;
        private readonly HelpPageViewModel _helpPageViewModel;
        private readonly BetHelpPageViewModel _betHelpPageViewModel;
        private readonly ConcurrentDictionary<Command, IHhrMenuPageViewModel> _commandsToViewModelMap =
            new ConcurrentDictionary<Command, IHhrMenuPageViewModel>();

        private readonly HHRTimer _placardTimer;
        private readonly HHRTimer _overlayExpiryTimer;
        private const int PlacardTimerTickIntervalMilli = 1000;
        private const int OverlayIdleTimerIntervalMilliSeconds = 30000;
        private int _tickCount;
        private int _placardTimeOut;
        private Action _placardTimeOutAction;

        private bool _disposed;
        private IHhrHostPageView _hostView;
        private PlacardView _placardView;
        private IHhrMenuPageViewModel _selectedViewModel;
        private bool _initializing;
        private bool _commandRunning;
        private readonly IPlayerBank _bank;
        private ITransactionCoordinator _transactionCoordinator;
        private Guid _raceInfoTransactionId;
        protected new readonly ILog Logger;

        public HostPageViewModelManager(
            IEventBus eventBus,
            ManualHandicapPageViewModel manualHandicapPageViewModel,
            RaceStatsPageViewModel raceStatsPageViewModel,
            WinningCombinationPageViewModel winningCombinationPageViewModel,
            ManualHandicapHelpPageViewModel manualHandicapHelpPageViewModel,
            PreviousRaceResultPageViewModel previousRaceResultPageViewModel,
            CurrentProgressivePageViewModel currentProgressivePageViewModel,
            HelpPageViewModel helpPageViewModel,
            BetHelpPageViewModel betHelpPageViewModel,
            IPlayerBank bank,
            IRuntimeFlagHandler runtimeFlagHandler,
            IPropertiesManager properties,
            ITransactionCoordinator transactionCoordinator)
        {
            Logger = LogManager.GetLogger(GetType());
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _transactionCoordinator = transactionCoordinator ?? throw new ArgumentNullException(nameof(transactionCoordinator));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));

            _runtimeFlagHandler = runtimeFlagHandler ?? throw new ArgumentNullException(nameof(runtimeFlagHandler));

            _manualHandicapPageViewModel = manualHandicapPageViewModel ??
                                           throw new ArgumentNullException(nameof(manualHandicapPageViewModel));
            _raceStatsPageViewModel =
                raceStatsPageViewModel ?? throw new ArgumentNullException(nameof(raceStatsPageViewModel));
            _winningCombinationPageViewModel = winningCombinationPageViewModel ??
                                               throw new ArgumentNullException(nameof(winningCombinationPageViewModel));
            _manualHandicapHelpPageViewModel = manualHandicapHelpPageViewModel ??
                                               throw new ArgumentNullException(nameof(manualHandicapHelpPageViewModel));
            _previousRaceResultPageViewModel = previousRaceResultPageViewModel ??
                                               throw new ArgumentNullException(nameof(previousRaceResultPageViewModel));
            _currentProgressivePageViewModel = currentProgressivePageViewModel ??
                                               throw new ArgumentNullException(nameof(currentProgressivePageViewModel));
            _helpPageViewModel = helpPageViewModel ?? throw new ArgumentNullException(nameof(helpPageViewModel));
            _betHelpPageViewModel =
                betHelpPageViewModel ?? throw new ArgumentNullException(nameof(betHelpPageViewModel));

            _selectedViewModel = null;
            _hostView = null;

            _placardTimer = new HHRTimer(PlacardTimerTickIntervalMilli);
            _tickCount = 0;
            _placardTimeOut = 0;
            _placardTimer.Elapsed += (obj, args) => OnTimerElapsed();
            _commandRunning = false;

            _overlayExpiryTimer = new HHRTimer(OverlayIdleTimerIntervalMilliSeconds) { AutoReset = true, Enabled = false };
            _overlayExpiryTimer.Elapsed += (obj, args) =>
            {
                MvvmHelper.ExecuteOnUI(
                    CloseMenu);
            };

            eventBus.Subscribe<SystemDisableAddedEvent>(this, OnSystemDisableAdded);
            eventBus.Subscribe<GameConnectedEvent>(this, OnGameConnected);
            eventBus.Subscribe<GamePlayDisabledEvent>(this, OnGamePlayDisabled);
            eventBus.Subscribe<GamePlayEnabledEvent>(this, OnGamePlayEnabled);
            eventBus.Subscribe<GameDiagnosticsStartedEvent>(this, HandleEvent);

            InitializeCommandMapper();

            // If there was a power-cycle in the middle of manual handicap, we need to abandon that transaction
            _transactionCoordinator.AbandonTransactions(RaceInfoTransactionRequestorId);
        }

        private void OnGameConnected(GameConnectedEvent obj)
        {
            // If the host view is null, we aren't in the race menu, so reset the AwaitingPlayerSelection.
            // This is need primarily for when we are in the race menu and a power-cycle happens. On boot,
            // the VBD buttons will be disabled if we don't clear this
            if (_selectedViewModel is null)
            {
                Logger.Debug("Resetting AwaitingPlayerSelection flag");
                // Reset this flag in case a power-cycle occurred in the middle of an activity that had set this flag
                _properties.SetProperty(GamingConstants.AwaitingPlayerSelection, false);
                _runtimeFlagHandler.SetAwaitingPlayerSelection(false);
            }

            // If the host view is not null, then the operator menu was opened and then now closed, causing the
            // race menu to hide and show, and the runtime to close and be started again. Since we are in the race
            // race menu when this happens, we need to update runtime with the current value of the flag
            else
            {
                var awaitingPlayerSelection = _properties.GetValue(GamingConstants.AwaitingPlayerSelection, false);
                Logger.Debug($"Updated the AwaitingPlayerSelection as {awaitingPlayerSelection}");
                _runtimeFlagHandler.SetAwaitingPlayerSelection(awaitingPlayerSelection);
            }
        }

        public IHhrMenuPageViewModel SelectedViewModel => _selectedViewModel;
        private bool IsHandicapActive => _selectedViewModel == _manualHandicapPageViewModel ||
                                         _selectedViewModel == _raceStatsPageViewModel;

        private async Task SetSelectedViewModel(IHhrMenuPageViewModel viewModel, Command command)
        {
            Logger.Debug($"NewViewModel is {viewModel?.GetType()} and OldViewModel is {_selectedViewModel?.GetType()}");

            if (viewModel == _selectedViewModel)
                return;

            var oldViewModel = _selectedViewModel;


            if (viewModel != null)
            {
                await viewModel.Init(command);
            }

            _selectedViewModel = viewModel;

            RaisePropertyChanged(nameof(SelectedViewModel));

            oldViewModel?.Reset();
        }

        private void CloseViewModel()
        {
            _selectedViewModel?.Reset();

            _selectedViewModel = null;

            RaisePropertyChanged(nameof(SelectedViewModel));
        }

        /// <summary>
        ///     Dispose of managed objects used by this class
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Hide()
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    if (_hostView != null && _selectedViewModel != null && _hostView is UIElement element)
                    {
                        element.Visibility = Visibility.Hidden;
                    }
                });
        }

        public void Unhide()
        {
            var isDisabled = ServiceManager.GetInstance().GetService<ISystemDisableManager>().IsDisabled;
            Logger.Debug($"isDisabled {isDisabled} and handicap is {IsHandicapActive}");

            if (IsHandicapActive && isDisabled)
            {
                return;
            }

            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    if (_hostView != null && _hostView is UIElement element)
                    {
                        element.Visibility = Visibility.Visible;
                    }
                });
        }

        public async Task Show(Command command)
        {
            if (_initializing)
            {
                return;
            }

            try
            {
                // If the race info button is pressed and the menu is on the screen, close it.
                if (command == Command.PreviousResults && _selectedViewModel != null)
                {
                    // If the user is currently in the manual handicap view, we can't close.
                    if (IsHandicapActive)
                    {
                        return;
                    }

                    CloseMenu();
                }
                else
                {
                    _raceInfoTransactionId = _transactionCoordinator.RequestTransaction(RaceInfoTransactionRequestorId, 500, TransactionType.Write);
                    if (_raceInfoTransactionId == Guid.Empty)
                    {
                        return;
                    }

                    _overlayExpiryTimer.Start();
                    _initializing = true;

                    await SetSelectedViewModel(_commandsToViewModelMap[command], command);

                    _runtimeFlagHandler.SetAwaitingPlayerSelection(true);
                    _properties.SetProperty(GamingConstants.AwaitingPlayerSelection, true);

                    _eventBus.Publish(new OverlayMenuEnteredEvent());
                    _eventBus.Publish(new ViewInjectionEvent(_hostView, DisplayRole.Main, ViewAction.Add));
                }
            }
            finally
            {
                _initializing = false;
            }
        }

        public void SetView(IHhrHostPageView hhrHostPageView)
        {
            _hostView = hhrHostPageView;
        }

        private static long _lastBetUpDownTimeTicks;
        public const long BetUpDownDelayTimeTicks = TimeSpan.TicksPerMillisecond * 300;
        public static bool BetButtonDelayLimit()
        {
            long timeNow = DateTime.UtcNow.Ticks;
            if (timeNow - _lastBetUpDownTimeTicks > BetUpDownDelayTimeTicks)
            {
                ResetBetButtonDelayLimit(timeNow);
                return true;
            }

            return false;
        }

        // Public to make this testable, so we don't have to wait in unit tests.
        public static void ResetBetButtonDelayLimit(long timeNow)
        {
            _lastBetUpDownTimeTicks = timeNow;
        }

        private async void Handle(object sender, HHRCommandEventArgs commandEvent)
        {
            Logger.Debug($"_commandRunning is {_commandRunning} and command is {commandEvent.Command}");

            if (_commandRunning)
                return;

            try
            {
                _commandRunning = true;

                switch (commandEvent.Command)
                {
                    case Command.PreviousResults:
                    case Command.ManualHandicapHelp:
                    case Command.ExitHelp:
                    case Command.ManualHandicap:
                    case Command.RaceStats:
                    case Command.Back:
                    case Command.Help:
                    case Command.StatPageTimerExpire:
                    case Command.BetHelp:
                    case Command.CurrentProgressive:
                    case Command.CurrentProgressiveMoneyIn:
                    case Command.WinningCombination:
                        await SetSelectedViewModel(_commandsToViewModelMap[commandEvent.Command], commandEvent.Command);
                        break;
                    case Command.HandicapCompleted:
                    case Command.HandicapPageTimerExpire:
                        CloseServiceConfirmationVbdDialog();
                        CloseMenu();
                        break;
                    case Command.ReturnToGame:
                    case Command.PlayNow:
                        CloseMenu();
                        break;
                }
                RestartOverlayExpiryTimer();
            }
            finally
            {
                _commandRunning = false;
                Logger.Debug($"Set _commandRunning to False");
            }
        }

        private void CloseServiceConfirmationVbdDialog()
        {
            // We *must* complete manual handicap once we enter it, so we'll force the VBD
            // service confirmation dialog closed, so that GDK won't block our game start.
            _runtimeFlagHandler.SetDisplayingOverlay(false);
            _eventBus.Publish(new ShowServiceConfirmationEvent
            {
                Show = false
            });
        }

        private void CloseMenu()
        {
            _transactionCoordinator.ReleaseTransaction(_raceInfoTransactionId);

            if (_hostView == null || SelectedViewModel == null)
            {
                return;
            }
            _eventBus.Publish(new ViewInjectionEvent(_hostView, DisplayRole.Main, ViewAction.Remove));

            _overlayExpiryTimer.Stop();

            _eventBus.Publish(new OverlayMenuExitedEvent());
            
            //Notify the runtime that HHR Menu is not active any  more
            _runtimeFlagHandler.SetAwaitingPlayerSelection(false);
            _properties.SetProperty(GamingConstants.AwaitingPlayerSelection, false);
            CloseViewModel();
        }

        private void InitializeCommandMapper()
        {
            _commandsToViewModelMap.TryAdd(Command.Help, _helpPageViewModel);
            _commandsToViewModelMap.TryAdd(Command.BetHelp, _betHelpPageViewModel);
            _commandsToViewModelMap.TryAdd(Command.CurrentProgressive, _currentProgressivePageViewModel);
            _commandsToViewModelMap.TryAdd(Command.CurrentProgressiveMoneyIn, _currentProgressivePageViewModel);

            _commandsToViewModelMap.TryAdd(Command.PreviousResults, _previousRaceResultPageViewModel);
            _commandsToViewModelMap.TryAdd(Command.RaceStats, _raceStatsPageViewModel);
            _commandsToViewModelMap.TryAdd(Command.ManualHandicapHelp, _manualHandicapHelpPageViewModel);
            _commandsToViewModelMap.TryAdd(Command.ManualHandicap, _manualHandicapPageViewModel);
            _commandsToViewModelMap.TryAdd(Command.WinningCombination, _winningCombinationPageViewModel);

            _commandsToViewModelMap.TryAdd(Command.ExitHelp, _commandsToViewModelMap[Command.PreviousResults]);
            _commandsToViewModelMap.TryAdd(Command.Back, _commandsToViewModelMap[Command.ManualHandicap]);
            _commandsToViewModelMap.TryAdd(Command.StatPageTimerExpire, _commandsToViewModelMap[Command.ManualHandicap]);

            SubscribeToHandlers();
        }

        private void HandlePlacard(object sender, PlacardEventArgs args)
        {
            if (args.IsVisible)
            {
                ShowPlacard(args);
            }
            else
            {
                RemovePlacard();
            }
        }

        private void ShowPlacard(PlacardEventArgs args)
        {
            if (_placardTimer.Enabled)
            {
                return;
            }

            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    _placardView = new PlacardView(args.Placard);
                    _eventBus.Publish(new ViewInjectionEvent(_placardView, DisplayRole.Main, ViewAction.Add));
                });

            // The timeout would be zero for the CallAttendant placard, as it would be removed only after clearing the handpay lockup
            if (args.Timeout == 0)
            {
                return;
            }

            _placardTimeOut = args.Timeout;
            _placardTimeOutAction = args.ExitAction;
            _tickCount = 0;

            _placardTimer.Start();
        }

        private void RemovePlacard()
        {
            _eventBus.Publish(new ViewInjectionEvent(_placardView, DisplayRole.Main, ViewAction.Remove));
        }

        private void OnTimerElapsed()
        {
            _tickCount++;

            if (_tickCount < _placardTimeOut)
            {
                return;
            }

            RemovePlacard();

            _tickCount = 0;
            _placardTimer.Stop();
            _placardTimeOutAction?.Invoke();
        }

        private void SubscribeToHandlers()
        {
            foreach (var viewModel in _commandsToViewModelMap)
            {
                viewModel.Value.PlacardEvent -= HandlePlacard;
                viewModel.Value.HhrButtonClicked -= Handle;

                viewModel.Value.PlacardEvent += HandlePlacard;
                viewModel.Value.HhrButtonClicked += Handle;
            }
        }

        private void UnsubscribeToHandlers()
        {
            foreach (var viewModel in _commandsToViewModelMap)
            {
                viewModel.Value.HhrButtonClicked -= Handle;
                viewModel.Value.PlacardEvent -= HandlePlacard;
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _hostView = null;

                UnsubscribeToHandlers();

                _commandsToViewModelMap?.Clear();

                if (_selectedViewModel != null)
                {
                    _selectedViewModel.HhrButtonClicked -= Handle;
                    _selectedViewModel = null;

                    CloseViewModel();
                }

                _placardTimer.Dispose();
                _overlayExpiryTimer.Dispose();

                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void RestartOverlayExpiryTimer()
        {
            if (_selectedViewModel != null && !IsHandicapActive && _overlayExpiryTimer.Enabled)
            {
                _overlayExpiryTimer.Stop();
                _overlayExpiryTimer.Start();
            }
            else
            {
                _overlayExpiryTimer.Stop();
            }
        }

        private void OnSystemDisableAdded(SystemDisableAddedEvent evt)
        {

            if (!IsHandicapActive && EnableCashout(evt))
            {
                CloseMenu();
            }
        }

        private void OnGamePlayDisabled(GamePlayDisabledEvent evt)
        {
            /* Hide the menu if manual handicap is active to avoid the player
            leveraging the added time while the terminal is faulted. */
            if (IsHandicapActive)
            {
                Hide();
            }
        }

        private void OnGamePlayEnabled(GamePlayEnabledEvent evt)
        {
            /* GamePlayEnabledEvent is published before 3 sec of GameInitializationCompletedEvent
            and it is not good to show Manual Handicap screen on Game Loading page, Unhide
            would be called by consumer of GameInitializationCompletedEvent. */
            if ((IsHandicapActive && UiProperties.GameLoaded) || _selectedViewModel == null)
            {
                Unhide();
            }
        }

        private void HandleEvent(GameDiagnosticsStartedEvent replayStartedEvent)
        {
            MvvmHelper.ExecuteOnUI(CloseMenu);
        }

        private bool EnableCashout(SystemDisableAddedEvent evt)
        {
            return _bank.Balance != 0 &&
                   (evt.DisableId == ApplicationConstants.DisabledByHost0Key ||
                    evt.DisableId == ApplicationConstants.DisabledByHost1Key) ||
                   evt.DisableId == ApplicationConstants.Host0CommunicationsOfflineDisableKey ||
                   evt.DisableId == ApplicationConstants.Host1CommunicationsOfflineDisableKey;
        }

        /// <inheritdoc />
        ~HostPageViewModelManager()
        {
            Dispose(false);
        }
    }
}