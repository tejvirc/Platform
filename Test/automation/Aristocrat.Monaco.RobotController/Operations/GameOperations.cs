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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.RobotController.Services;

    internal class GameOperations : IRobotOperations
    {
        private readonly IEventBus _eventBus;
        private readonly Automation _automator;
        private readonly IPropertiesManager _propertyManager;
        private readonly RobotLogger _logger;
        private readonly LobbyStateChecker _lobbyStateChecker;
        private readonly RobotController _robotController;
        private readonly IGameService _gameService;
        private readonly GamingService _robotGamingService;
        private readonly RobotService _robotService;
        private readonly StatusManager _statusManager;

        private Timer _loadGameTimer;
        private Timer _RgTimer;
        private bool _disposed;

        public GameOperations(IEventBus eventBus, RobotLogger logger, Automation automator,
            LobbyStateChecker sc, IPropertiesManager pm, RobotController robotController,
            IGameService gameService, GamingService gamingService, RobotService robotService,
            StatusManager statusManager)
        {
            _lobbyStateChecker = sc;
            _automator = automator;
            _logger = logger;
            _eventBus = eventBus;
            _propertyManager = pm;
            _robotController = robotController;
            _gameService = gameService;
            _robotGamingService = gamingService;
            _robotService = robotService;
            _statusManager = statusManager;
        }

        ~GameOperations() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Execute()
        {
            _logger.Info("GameOperations Has Been Initiated!", GetType().Name);
            SubscribeToEvents();

            if (_robotService.IsRegularRobots())
            {
                return;
            }

            _loadGameTimer = new Timer(
                               (sender) =>
                               {
                                   _robotGamingService.RequestGameLoad();
                               },
                               null,
                               _robotController.Config.ActiveGameMode.IntervalLoadGame,
                               _robotController.Config.ActiveGameMode.IntervalLoadGame);
            _RgTimer = new Timer(
                               (sender) =>
                               {
                                   RequestRg();
                               },
                               null,
                               _robotController.Config.ActiveGameMode.IntervalRgSet,
                               _robotController.Config.ActiveGameMode.IntervalRgSet);

            _robotGamingService.LoadGameWithDelay(Constants.loadGameDelayDuration);
        }

        public void Reset()
        {
            _disposed = false;
            _statusManager.SanityCounter = 0;

            _statusManager.IsLoadGameInProgress = false;
            _statusManager.IsGameRunning = _gameService.Running;
            _statusManager.GoingNextGame = false;
        }

        public void Halt()
        {
            _logger.Info("Halt Request is Received!", GetType().Name);
            Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_loadGameTimer is not null)
                {
                    _loadGameTimer.Dispose();
                }
                _loadGameTimer = null;
                if (_RgTimer is not null)
                {
                    _RgTimer.Dispose();
                }
                _RgTimer = null;

                _eventBus.UnsubscribeAll(this);
            }
            _disposed = true;
        }

        private void RequestRg()
        {
            if (!_statusManager.IsGameRunning)
            {
                return;
            }
            _logger.Info($"Performing Responsible Gaming Request Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
            _automator.SetResponsibleGamingTimeElapsed(_robotController.Config.GetTimeElapsedOverride());
            if (_robotController.Config.GetSessionCountOverride() != 0)
            {
                _automator.SetRgSessionCountOverride(_robotController.Config.GetSessionCountOverride());
            }
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<TimeLimitDialogVisibleEvent>(
                this,
                evt =>
                {
                    _logger.Info($"TimeLimitDialogVisibleEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                    _statusManager.IsTimeLimitDialogVisible = true;
                    if (evt.IsLastPrompt)
                    {
                        _statusManager.ExitToLobbyWhenGameIdle = !_robotService.IsRegularRobots();
                    }
                });

            _eventBus.Subscribe<TimeLimitDialogHiddenEvent>(
                this,
                evt =>
                {
                    _logger.Info($"TimeLimitDialogHiddenEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                    _statusManager.IsTimeLimitDialogVisible = false;
                });

            _eventBus.Subscribe<GameRequestFailedEvent>(
                this,
                _ =>
                {
                    _logger.Error($"GameRequestFailedEvent Got Triggered!  Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                    _statusManager.IsLoadGameInProgress = false;
                    if (!_lobbyStateChecker.IsAllowSingleGameAutoLaunch)
                    {
                        _robotGamingService.RequestGameLoad();
                    }
                });
            _eventBus.Subscribe<GameInitializationCompletedEvent>(
                this,
                _ =>
                {
                    _logger.Info($"GameInitializationCompletedEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                    _statusManager.IsGameRunning = true;
                    _statusManager.SanityCounter = 0;
                    _statusManager.IsLoadGameInProgress = false;
                    _robotGamingService.BalanceCheckWithDelay(Constants.BalanceCheckDelayDuration);
                });

            _eventBus.Subscribe<GamePlayRequestFailedEvent>(
                this,
                _ =>
                {
                    _logger.Info($"Keying off GamePlayRequestFailed! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                    ToggleJackpotKey(Constants.ToggleJackpotKeyDuration);
                });

            _eventBus.Subscribe<UnexpectedOrNoResponseEvent>(
                this,
                _ =>
                {
                    _logger.Info($"Keying off UnexpectedOrNoResponseEvent Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                    ToggleJackpotKey(Constants.ToggleJackpotKeyLongerDuration);
                });

            _eventBus.Subscribe<GameFatalErrorEvent>(
                 this,
                 _ =>
                 {
                     _logger.Error($"GameFatalErrorEvent Got Triggered! There is an issue with [{_robotController.Config.CurrentGame}]", GetType().Name);
                     _robotController.Enabled = false;
                 });
            _eventBus.Subscribe<GamePlayStateChangedEvent>(
                 this,
                 _ =>
                 {
                     _logger.Info($"GamePlayStateChangedEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                     _robotController.IdleDuration = 0;
                     _statusManager.SanityCounter = 0;
                 });
            _eventBus.Subscribe<HandpayStartedEvent>(this, evt =>
            {
                if (evt.Handpay == HandpayType.GameWin ||
                     evt.Handpay == HandpayType.CancelCredit)
                {
                    _logger.Info($"Keying off large win Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                    ToggleJackpotKey(Constants.ToggleJackpotKeyDuration);
                }
                else
                {
                    _logger.Info($"Skip toggling jackpot key since evt.Handpay = [{evt.Handpay}] is not valid!", GetType().Name);
                }
            });
            _eventBus.Subscribe<SystemEnabledEvent>(
                this,
                _ =>
                {
                    _logger.Info($"SystemEnabledEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                    _robotGamingService.LoadGameWithDelay(Constants.loadGameDelayDuration);
                });
            InitGameProcessHungEvent();
        }

        private void InitGameProcessHungEvent()
        {
            // If the runtime process hangs, and the setting to not kill it is active, then stop the robot. 
            // This will allow someone to attach a debugger to investigate.
            var doNotKillRuntime = _propertyManager.GetValue("doNotKillRuntime", Common.Constants.False).ToUpperInvariant();
            if (doNotKillRuntime == Common.Constants.True)
            {
                _eventBus.Subscribe<GameProcessHungEvent>(this, _ =>
                {
                    _robotController.Enabled = false;
                });
            };
        }

        private void ToggleJackpotKey(int waitDuration)
        {
            _logger.Info($"ToggleJackpotKey Request Is Received! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
            Task.Delay(waitDuration).ContinueWith(_ => _automator.JackpotKeyoff()).ContinueWith(_ => _eventBus.Publish(new DownEvent((int)ButtonLogicalId.Button30)));
        }









    }
}
