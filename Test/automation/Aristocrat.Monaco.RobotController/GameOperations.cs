namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Monaco.Hardware.Contracts.Button;
    using Aristocrat.Monaco.Hhr.Events;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using log4net;
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
        private readonly ILog _logger;
        private readonly StateChecker _sc;
        private readonly Func<long> _idleDuration;
        private Action _idleDurationReset;
        private Timer _LoadGameTimer;
        private Timer _RgTimer;
        private bool _disposed;
        private bool _isTimeLimitDialogVisible;
        private int sanityCounter;
        private static GameOperations instance = null;
        private static readonly object padlock = new object();
        public static GameOperations Instantiate(RobotInfo robotInfo)
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new GameOperations(robotInfo);
                }
                return instance;
            }
        }
        private GameOperations(RobotInfo robotInfo)
        {
            _config = robotInfo.Config;
            _sc = robotInfo.StateChecker;
            _automator = robotInfo.Automator;
            _logger = robotInfo.Logger;
            _eventBus = robotInfo.EventBus;
            _pm = robotInfo.PropertiesManager;
            _idleDurationReset = robotInfo.IdleDurationReset;
            _idleDuration = robotInfo.IdleDuration;
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
                //Todo: Log Something
                return;
            }
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
            Task.Delay(milliseconds).ContinueWith(
                _ =>
                {
                    LoadGame();
                });
        }

        public void Halt()
        {
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
                    _isTimeLimitDialogVisible = false;
                });
            _eventBus.Subscribe<GameRequestFailedEvent>(
                this,
                _ =>
                {
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
                    BalanceCheck();
                    ResetTimer();
                    sanityCounter = 0;
                });

            _eventBus.Subscribe<GamePlayRequestFailedEvent>(
                this,
                _ =>
                {
                    _logger.Info("Keying off GamePlayRequestFailed");
                    ToggleJackpotKey(1000);
                });
            _eventBus.Subscribe<UnexpectedOrNoResponseEvent>(
                this,
                _ =>
                {
                    _logger.Info("Keying off UnexpectedOrNoResponseEvent");
                    ToggleJackpotKey(10000);
                });
            _eventBus.Subscribe<GameIdleEvent>(
                 this,
                 _ =>
                 {
                     BalanceCheckWithDelay();
                     HandleExitToLobbyRequest();
                 });
            _eventBus.Subscribe<GameProcessExitedEvent>(
                 this,
                 evt =>
                 {
                     bool goToNextGame = false;
                     //log
                     if (evt.Unexpected)
                     {
                         sanityCounter ++;
                         goToNextGame = false;
                         _automator.EnableExitToLobby(true);
                     }
                     else
                     {
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
                     AnotherChance();
                     //log
                 });
            _eventBus.Subscribe<GamePlayStateChangedEvent>(
                 this,
                 _ =>
                 {
                     //log
                     _idleDurationReset();
                 });
        }

        private void AnotherChance()
        {
            sanityCounter += 2;
            _automator.EnableExitToLobby(false);
            SelectNextGame(true);
            RequestGameLoad();
        }

        private void ResetTimer()
        {
            _LoadGameTimer.Change(_config.Active.IntervalLoadGame, _config.Active.IntervalLoadGame);
        }

        private void SelectNextGame(bool goToNextGame)
        {
            if (!goToNextGame) { return; }
            _config.SelectNextGame();
        }
        private void HandleExitToLobbyRequest()
        {
            if ((bool)_pm.GetProperty(Constants.HandleExitToLobby, false))
            {
                _automator.RequestGameExit();
            }
        }
        private void BalanceCheckWithDelay()
        {
            if (_idleDuration() > 3000)
            {
                BalanceCheck();
            }
        }
        private void BalanceCheck()
        {
            _eventBus.Publish(new BalanceCheckEvent());
        }
        private void ToggleJackpotKey(int waitDuration)
        {
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
                _logger.Info($"Requesting game {gameInfo.ThemeName} with denom {denom} be loaded.");
                _automator.RequestGameLoad(gameInfo.Id, denom);
            }
            else
            {
                _logger.Info($"Did not find game, {_config.CurrentGame}");
                AnotherChance();
            }
        }
        private bool CheckSanity()
        {
            if (sanityCounter > 1)
            {
                AnotherChance();
            }
            if (sanityCounter < 10) { return true; }
            Halt();
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
