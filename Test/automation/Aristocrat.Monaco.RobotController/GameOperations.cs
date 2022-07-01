namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Accounting.Contracts.Handpay;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Monaco.Hardware.Contracts.Button;
    using Aristocrat.Monaco.Hhr.Events;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal class GameOperations : IRobotOperations, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly Configuration _config;
        private readonly Automation _automator;
        private readonly IPropertiesManager _pm;
        private readonly RobotLogger _logger;
        private readonly StateChecker _sc;
        private readonly RobotController _robotController;
        private Timer _LoadGameTimer;
        private Timer _RgTimer;
        private bool _disposed;
        private bool _isTimeLimitDialogVisible;
        private int sanityCounter;
        private int unexpectedExitCounter;
        public GameOperations(IEventBus eventBus, RobotLogger logger, Automation automator, Configuration config, StateChecker sc, IPropertiesManager pm, RobotController robotController)
        {
            _config = config;
            _sc = sc;
            _automator = automator;
            _logger = logger;
            _eventBus = eventBus;
            _pm = pm;
            _robotController = robotController;
        }
        ~GameOperations() => Dispose(false);
        public void Execute()
        {
            SubscribeToEvents();
            _LoadGameTimer = new Timer(
                               (sender) =>
                               {
                                   HandleGameRequest();
                               },
                               null,
                               _config.Active.IntervalLoadGame,
                               _config.Active.IntervalLoadGame);
            if (_config.Active.IntervalRgSet == 0) { return; }
            _RgTimer = new Timer(
                               (sender) =>
                               {
                                   HandleRgRequest();
                               },
                               null,
                               _config.Active.IntervalRgSet,
                               _config.Active.IntervalRgSet);
        }
        private void HandleRgRequest()
        {
            _logger.Info("Performing Responsible Gaming Request", GetType().Name);
            _automator.SetResponsibleGamingTimeElapsed(_config.GetTimeElapsedOverride());
            if (_config.GetSessionCountOverride() != 0)
            {
                _automator.SetRgSessionCountOverride(_config.GetSessionCountOverride());
            }
        }
        private void HandleGameRequest()
        {
            if (!IsValid())
            {
                _logger.Error("Game Request Validation Failed", GetType().Name);
                return;
            }
            _logger.Info("Game Requested Received!", GetType().Name);
            if (_sc.IsGame)
            {
                _automator.EnableExitToLobby(true);
            }
            else
            {
                LoadGame();
            }
        }
        private void LoadGame()
        {
            if (!IsTimeLimitDialogInProgress() && CheckSanity())
            {
                DismissTimeLimitDialog();
                RequestGameLoad();
            }
        }
        private void LoadGameWithDelay(int milliseconds)
        {
            _logger.Info("LoadGameWithDelay Request is Received!", GetType().Name);
            Task.Delay(milliseconds).ContinueWith(
                _ =>
                {
                    LoadGame();
                });
        }
        public void Halt()
        {
            _logger.Info("Halt Request is Received!", GetType().Name);
            _automator.EnableExitToLobby(true);
            _automator.RequestGameExit();
            _automator.EnableExitToLobby(false);
            _LoadGameTimer?.Dispose();
            _RgTimer?.Dispose();
            sanityCounter = 0;
            _eventBus.UnsubscribeAll(this);
        }
        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<GameLoadRequestEvent>(this, HandleEvent);
            _eventBus.Subscribe<TimeLimitDialogVisibleEvent>(
                 this,
                 evt =>
                 {
                     _logger.Info("TimeLimitDialogVisibleEvent Got Triggered!", GetType().Name);
                     _isTimeLimitDialogVisible = true;
                     if (evt.IsLastPrompt)
                     {
                         _automator.EnableCashOut(true);
                     }
                 });
            _eventBus.Subscribe<TimeLimitDialogHiddenEvent>(
                this,
                evt =>
                {
                    _logger.Info("TimeLimitDialogHiddenEvent Got Triggered!", GetType().Name);
                    _isTimeLimitDialogVisible = false;
                });
            _eventBus.Subscribe<GameRequestFailedEvent>(
                this,
                _ =>
                {
                    _logger.Info("GameRequestFailedEvent Got Triggered!", GetType().Name);
                    sanityCounter++;
                    if (!_sc.IsAllowSingleGameAutoLaunch)
                    {
                        SelectNextGame(true);
                        HandleGameRequest();
                    }
                });
            _eventBus.Subscribe<GameInitializationCompletedEvent>(
                this,
                _ =>
                {
                    _logger.Info("GameInitializationCompletedEvent Got Triggered!", GetType().Name);
                    BalanceCheck();
                    ResetTimer();
                    sanityCounter = 0;
                    unexpectedExitCounter = 0;
                });

            _eventBus.Subscribe<GamePlayRequestFailedEvent>(
                this,
                _ =>
                {
                    _logger.Info("Keying off GamePlayRequestFailed", GetType().Name);
                    ToggleJackpotKey(1000);
                });
            _eventBus.Subscribe<UnexpectedOrNoResponseEvent>(
                this,
                _ =>
                {
                    _logger.Info("Keying off UnexpectedOrNoResponseEvent", GetType().Name);
                    ToggleJackpotKey(10000);
                });
            _eventBus.Subscribe<GameIdleEvent>(
                 this,
                 _ =>
                 {
                     _logger.Info("GameIdleEvent Got Triggered!", GetType().Name);
                     BalanceCheckWithDelay(3000);
                     HandleExitToLobbyRequest();
                 });
            _eventBus.Subscribe<GameProcessExitedEvent>(
                 this,
                 evt =>
                 {
                     bool goToNextGame = false;
                     if (evt.Unexpected)
                     {
                         _logger.Info("GameProcessExitedEvent-Unexpected Got Triggered!", GetType().Name);
                         unexpectedExitCounter++;
                         goToNextGame = false;
                         _automator.EnableExitToLobby(true);
                     }
                     else
                     {
                         _logger.Info("GameProcessExitedEvent-Normal Got Triggered!", GetType().Name);
                         goToNextGame = true;
                         _automator.EnableExitToLobby(false);
                     }
                     SelectNextGame(goToNextGame);
                     LoadGameWithDelay(1000);
                 });
            _eventBus.Subscribe<GameFatalErrorEvent>(
                 this,
                 _ =>
                 {
                     _logger.Info("GameFatalErrorEvent Got Triggered!", GetType().Name);
                     AnotherChance();
                 });
            _eventBus.Subscribe<GamePlayStateChangedEvent>(
                 this,
                 _ =>
                 {
                     _logger.Info("GamePlayStateChangedEvent Got Triggered!", GetType().Name);
                     _robotController.IdleDuration = 0;
                 });
            _eventBus.Subscribe<HandpayStartedEvent>(this, evt =>
            {
                if (evt.Handpay == HandpayType.GameWin ||
                     evt.Handpay == HandpayType.CancelCredit)
                {
                    _logger.Info("Keying off large win", GetType().Name);
                    ToggleJackpotKey(1000);
                }
            });
            InitGameProcessHungEvent();
        }
        private void InitGameProcessHungEvent()
        {
            // If the runtime process hangs, and the setting to not kill it is active, then stop the robot. 
            // This will allow someone to attach a debugger to investigate.
            var doNotKillRuntime = _pm.GetValue("doNotKillRuntime", Common.Constants.False).ToUpperInvariant();
            if (doNotKillRuntime == Common.Constants.True)
            {
                _eventBus.Subscribe<GameProcessHungEvent>(this, _ => { _robotController.Enabled = false; });
            };
        }
        private void AnotherChance()
        {
            _logger.Info("AnotherChance Request Is Received!", GetType().Name);
            sanityCounter ++;
            _automator.EnableExitToLobby(false);
            SelectNextGame(true);
            LoadGame();
        }
        private void ResetTimer()
        {
            _logger.Info("ResetTimer Request Is Received!", GetType().Name);
            _LoadGameTimer.Change(_config.Active.IntervalLoadGame, _config.Active.IntervalLoadGame);
        }
        private void SelectNextGame(bool goToNextGame)
        {
            if (!goToNextGame) { return; }
            _logger.Info("SelectNextGame Request Is Received!", GetType().Name);
            _config.SelectNextGame();
        }
        private void HandleExitToLobbyRequest()
        {
            if ((bool)_pm.GetProperty(Constants.HandleExitToLobby, false))
            {
                _logger.Info("ExitToLobby Request Is Received!", GetType().Name);
                _automator.RequestGameExit();
            }
        }
        private void BalanceCheckWithDelay(int milliseconds)
        {
            _logger.Info("BalanceCheckWithDelay Request Is Received!", GetType().Name);
            Task.Delay(milliseconds).ContinueWith(_ => BalanceCheck());
        }
        private void BalanceCheck()
        {
            _eventBus.Publish(new BalanceCheckEvent());
        }
        private void ToggleJackpotKey(int waitDuration)
        {
            _logger.Info("ToggleJackpotKey Request Is Received!", GetType().Name);
            Task.Delay(waitDuration).ContinueWith(_ => _automator.JackpotKeyoff()).ContinueWith(_ => _eventBus.Publish(new DownEvent((int)ButtonLogicalId.Button30)));
        }
        private void HandleEvent(GameLoadRequestEvent evt)
        {
            HandleGameRequest();
        }
        private void DismissTimeLimitDialog()
        {
            _automator.DismissTimeLimitDialog(_isTimeLimitDialogVisible);
        }
        private bool IsTimeLimitDialogInProgress()
        {
            var timeLimitDialogVisible = _pm.GetValue(LobbyConstants.LobbyIsTimeLimitDlgVisible, false);
            var timeLimitDialogPending = _pm.GetValue(LobbyConstants.LobbyShowTimeLimitDlgPending, false);
            return timeLimitDialogVisible && timeLimitDialogPending;
        }
        private bool IsValid()
        {
            return _sc.IsChooser || _sc.IsGame;
        }
        private void RequestGameLoad()
        {
            var games = _pm.GetValues<IGameDetail>(GamingConstants.Games).ToList();
            var gameInfo = games.FirstOrDefault(g => g.ThemeName == _config.CurrentGame && g.Enabled);
            if (gameInfo != null)
            {
                var denom = gameInfo.Denominations.First(d => d.Active == true).Value;
                _logger.Info($"Requesting game {gameInfo.ThemeName} with denom {denom} be loaded.", GetType().Name);
                _automator.RequestGameLoad(gameInfo.Id, denom);
            }
            else
            {
                _logger.Error($"Did not find game, {_config.CurrentGame}", GetType().Name);
                AnotherChance();
            }
        }
        private bool CheckSanity()
        {
            if (unexpectedExitCounter > 1)
            {
                unexpectedExitCounter = 0;
                AnotherChance();
            }
            if (sanityCounter < 10) { return true; }
            _robotController.Enabled = false;
            return false;
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
                _LoadGameTimer?.Dispose();
                _RgTimer?.Dispose();
                _eventBus.UnsubscribeAll(this);
            }
            _disposed = true;
        }
    }
}
