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

    internal class ForceExitSimulationOperations : IRobotOperations
    {
        private readonly RobotLogger _logger;
        private readonly StateChecker _stateChecker;
        private Timer _forceGameExitTimer;
        private readonly RobotController _robotController;
        private readonly Automation _automator;
        private readonly IEventBus _eventBus;
        //private readonly IGameService _gamingService;
        private readonly GamingService _robotGamingService;
        private readonly RobotService _robotService;
        private readonly RobotRunStatus _robotRunStatus;

        private bool _gotoOtherGameWhenIdle;
        private bool _isGameRunning;

        public ForceExitSimulationOperations(IEventBus eventBus, RobotLogger logger, Automation automator,
            StateChecker stateChecker, IPropertiesManager pm, RobotController robotController,
            GamingService gamingService, RobotService robotService,
            IGameService gameService, RobotRunStatus robotStatus)
        {
            _logger = logger;
            _stateChecker = stateChecker;
            _robotController = robotController;
            _automator = automator;
            _eventBus = eventBus;
            _robotGamingService = gamingService;
            _robotService = robotService;
            _robotRunStatus = robotStatus;
        }

        public void Execute()
        {
            _logger.Info("ForceExitSimulationOperations Has Been Initiated!", nameof(ForceExitSimulationOperations));

            _forceGameExitTimer = new Timer(
                               (sender) =>
                               {
                                   _robotGamingService.KillGame();
                               },
                               null,
                               _robotController.Config.Active.IntervalLobby,
                               _robotController.Config.Active.IntervalLobby);
        }

        public void Halt()
        {

        }

        public void Dispose()
        {

        }

        public void Reset()
        {
            _gotoOtherGameWhenIdle = false;
        }


        public void SubscribeEvents()
        {
            _eventBus.Subscribe<GameProcessExitedEvent>(
                 this,
                 evt =>
                 {
                     _isGameRunning = false;
                     _robotController.UnBlockOtherOperations(RobotStateAndOperations.GameExiting);
                     if (evt.Unexpected)
                     {
                         if (!_isRobotGameExitInProgress)
                         {
                             _logger.Error($"GameProcessExitedEvent-Unexpected Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                             _robotController.Enabled = false;
                             return;
                         }
                         _logger.Info($"GameProcessExitedEvent-Unexpected-ForceGameExit Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                         _isRobotGameExitInProgress = false;
                         _goToNextGame = false;
                         _gotoOtherGameWhenIdle = !_robotService.IsRegularRobots();
                     }
                     else
                     {
                         _logger.Info($"GameProcessExitedEvent-Normal Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                         _goToNextGame = !_robotService.IsRegularRobots();
                     }
                     _robotGamingService.LoadGameWithDelay(Constants.loadGameDelayDuration);
                 });

            _eventBus.Subscribe<TimeLimitDialogVisibleEvent>(
     this,
     evt =>
     {
         _logger.Info($"TimeLimitDialogVisibleEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
         _robotGamingService.IsTimeLimitDialogVisible = true;
         if (evt.IsLastPrompt)
         {
             _gotoOtherGameWhenIdle = !_robotService.IsRegularRobots();
         }
     });

            _eventBus.Subscribe<TimeLimitDialogHiddenEvent>(
                this,
                evt =>
                {
                    _logger.Info($"TimeLimitDialogHiddenEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                    _robotGamingService.IsTimeLimitDialogVisible = false;
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
            return _isGameRunning
                    && (_stateChecker.IsIdle || _stateChecker.IsPresentationIdle)
                    && _gotoOtherGameWhenIdle
                    && !_robotRunStatus.IsGameExitInProgress;
        }

        private void HandleExitToLobbyRequest()
        {
            //if (_gotoOtherGameWhenIdle)
            //{
                _robotController.BlockOtherOperations(RobotStateAndOperations.GameExiting);
                if (!IsExitToLobbyWhenIdleValid())
                {
                    _robotController.UnBlockOtherOperations(RobotStateAndOperations.GameExiting);
                    return;
                }
                _logger.Info($"ExitToLobby Request Is Received! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                _automator.RequestGameExit();
                _robotRunStatus.IsGameExitInProgress = true;
               // _gotoOtherGameWhenIdle = false;
                //GameProcessExitedEvent gets trigered
            }
        //}
    }
}
