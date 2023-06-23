namespace Aristocrat.Monaco.RobotController
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Threading;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Monaco.RobotController.Services;

    internal class ForceExitOperations : IRobotOperations
    {
        private readonly RobotLogger _logger;
        private readonly LobbyStateChecker _lobbyStateChecker;
        private Timer _forceGameExitTimer;
        private readonly RobotController _robotController;
        private readonly Automation _automator;
        private readonly IEventBus _eventBus;

        private readonly GamingService _robotGamingService;
        private readonly RobotService _robotService;
        private readonly StatusManager _statusManager;
        private readonly IGameService _platformGameService;

        public ForceExitOperations(IEventBus eventBus, RobotLogger logger, Automation automator,
            LobbyStateChecker stateChecker, IPropertiesManager pm, RobotController robotController,
            GamingService gamingService, RobotService robotService,
            IGameService gameService, StatusManager robotStatusManager)
        {
            _logger = logger;
            _lobbyStateChecker = stateChecker;
            _robotController = robotController;
            _automator = automator;
            _eventBus = eventBus;
            _robotGamingService = gamingService;
            _robotService = robotService;
            _statusManager = robotStatusManager;
            _platformGameService = gameService;
        }

        public void Execute()
        {
            _logger.Info("ForceExitSimulationOperations Has Been Initiated!", nameof(ForceExitOperations));

            SubscribeEvents();

            if (_robotService.IsRegularRobots())
            {
                return;
            }

            _forceGameExitTimer = new Timer(
                               (sender) =>
                               {
                                   if (!_robotGamingService.CanKillGame())
                                   {
                                       return;
                                   }

                                   _robotGamingService.KillGame();
                               },
                               null,
                               _robotController.Config.ActiveGameMode.IntervalLobby,
                               _robotController.Config.ActiveGameMode.IntervalLobby);
        }

        public void Halt()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_forceGameExitTimer != null)
            {
                _forceGameExitTimer.Dispose();
            }

            _eventBus.UnsubscribeAll(this);
        }

        public void Reset()
        {
            _statusManager.ExitToLobbyWhenGameIdle = false;
            _statusManager.IsGameRunning = _platformGameService.Running;
            _statusManager.GoingNextGame = false;
        }

        public void SubscribeEvents()
        {
            _eventBus.Subscribe<GameProcessExitedEvent>(
                 this,
                 evt =>
                 {
                     _statusManager.IsGameRunning = false;
                     _robotController.UnBlockOtherOperations(RobotStateAndOperations.GameExiting);
                     if (evt.Unexpected)
                     {
                         if (!_statusManager.IsGameExitInProgress)
                         {
                             _logger.Error($"GameProcessExitedEvent-Unexpected Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                             _robotController.Enabled = false;
                             return;
                         }
                         _logger.Info($"GameProcessExitedEvent-Unexpected-ForceGameExit Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                         _statusManager.IsGameExitInProgress = false;

                         _statusManager.GoingNextGame = false;
                         _statusManager.ExitToLobbyWhenGameIdle = !_robotService.IsRegularRobots();
                     }
                     else
                     {
                         _logger.Info($"GameProcessExitedEvent-Normal Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);

                         _statusManager.GoingNextGame = !_robotService.IsRegularRobots();
                     }
                     _robotGamingService.LoadGameWithDelay(Constants.loadGameDelayDuration);
                 });

            _eventBus.Subscribe<GameIdleEvent>(
                 this,
                 _ =>
                 {
                     _logger.Info($"GameIdleEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                     _robotGamingService.BalanceCheckWithDelay(Constants.BalanceCheckDelayDuration);

                     HandleExitToLobbyRequest();
                 });

        }

        private bool IsExitToLobbyWhenIdleValid()
        {
            return _statusManager.IsGameRunning
                    && (_lobbyStateChecker.IsIdle || _lobbyStateChecker.IsPresentationIdle)
                    && _statusManager.ExitToLobbyWhenGameIdle
                    && !_statusManager.IsGameExitInProgress;
        }

        private void HandleExitToLobbyRequest()
        {
            if (_statusManager.ExitToLobbyWhenGameIdle)
            {
                _robotController.BlockOtherOperations(RobotStateAndOperations.GameExiting);
                if (!IsExitToLobbyWhenIdleValid())
                {
                    _robotController.UnBlockOtherOperations(RobotStateAndOperations.GameExiting);
                    return;
                }
                _logger.Info($"ExitToLobby Request Is Received! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                _automator.RequestGameExit();
                _statusManager.IsGameExitInProgress = true;
                _statusManager.ExitToLobbyWhenGameIdle = false;
                //GameProcessExitedEvent gets trigered
            }
        }
    }
}
