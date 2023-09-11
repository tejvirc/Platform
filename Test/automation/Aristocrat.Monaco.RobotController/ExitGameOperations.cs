namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal class ExitGameOperations : IRobotOperations
    {
        protected readonly Automation _automator;
        protected readonly IEventBus _eventBus;
        protected readonly IGameService _gameService;
        protected readonly RobotController _robotController;
        protected readonly RobotLogger _logger;
        protected readonly StateChecker _stateChecker;
        private bool disposedValue;
        private Timer _ExitGameTimer;
        private GameOperations _gameOperation;


        public ExitGameOperations(IEventBus eventBus, RobotLogger logger, Automation automator, StateChecker sc, IPropertiesManager pm, RobotController robotController, IGameService gameService, GameOperations gameOperation)
        {
            _gameOperation = gameOperation;
            _logger = logger;
            _eventBus = eventBus;
            _automator = automator;
            _stateChecker = sc;
            _robotController = robotController;
            _gameService = gameService;
        }

        ~ExitGameOperations() => Dispose(false);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_ExitGameTimer is not null)
                    {
                        _ExitGameTimer.Dispose();
                    }
                    _ExitGameTimer = null;
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void Reset()
        {
            disposedValue = false;
            _gameOperation.SanityCounter = 0;
            _gameOperation.GoToNextGame = false;
        }

        public void Execute()
        {
           
            if (_robotController.Config.Active.IntervalLobby == 0)
            {
                _logger.Info("ExitGameOperations is Disabled!", GetType().Name);
                return;
            }

            _logger.Info("ExitGameOperations Has Been Initiated!", GetType().Name);

            SubscribeToEvents();

            _ExitGameTimer = new Timer(
                               (sender) =>
                               {
                                   RequestExitGame();
                               },
                               null,
                               _robotController.Config.Active.IntervalLobby,
                               _robotController.Config.Active.IntervalLobby);
        }

        public void Halt()
        {
            _logger.Info("Halt Request is Received!", GetType().Name);
            _eventBus.UnsubscribeAll(this);
            _ExitGameTimer?.Dispose();
        }

        private void ExitGame()
        {
            if (_stateChecker.IsIdle)
            {
                _logger.Info($"Game is in idle state, calling RequestGameExit.", GetType().Name);
                _automator.RequestGameExit();
            }
            else if (_robotController.Config.Active.TestRecovery)
            {
                _logger.Info($"_robotController.Config.Active.TestRecovery was set to true, requesting ForceGameExit.", GetType().Name);
                _automator.ForceGameExit(Constants.GdkRuntimeHostName);
            }
            else
            {
                _logger.Info("_robotController.Config.Active.TestRecovery was set to false!", GetType().Name);
            }
        }

        private void IsRegularRobotExited()
        {
            if (_gameOperation.IsRegularRobots())
            {
                _logger.Error($"Regular Robot Exited the Game, Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                _robotController.Enabled = false;
            }
        }

        private bool IsRequestExitValid()
        {
            var isBlocked = _robotController.IsBlockedByOtherOperation(new List<RobotStateAndOperations>() { });
            var isGeneralRule = _gameOperation.GameIsRunning && _stateChecker.IsGame;
            return !isBlocked && isGeneralRule;
        }

        private void RequestExitGame()
        {
            if (!IsRequestExitValid())
            {
                _logger.Info($"RequestExitGame was not valid!", GetType().Name);
                _robotController.UnBlockOtherOperations(RobotStateAndOperations.GameOperation_ExitGame);
                return;
            }
            _robotController.BlockOtherOperations(RobotStateAndOperations.GameOperation_ExitGame);
            _logger.Info("RequestExitGame Request Received!", GetType().Name);
            ExitGame();
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<GameProcessExitedEvent>(
                 this,
                 evt =>
                 {
                     _logger.Info($"GameProcessExitedEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                     IsRegularRobotExited();
                     _gameOperation.GameIsRunning = false;
                     if (evt.Unexpected)
                     {
                         _logger.Info($"GameProcessExitedEvent-Unexpected Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                         _gameOperation.GoToNextGame = false;
                     }
                     _robotController.UnBlockOtherOperations(RobotStateAndOperations.GameOperation_ExitGame);
                     _gameOperation.LoadGameWithDelay(Constants.loadGameDelayDuration);
                 });

            _eventBus.Subscribe<GameExitedNormalEvent>(
                 this,
                 evt =>
                 {
                     _logger.Info($"GameProcessExitedEvent-Normal Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                     IsRegularRobotExited();
                     _gameOperation.GoToNextGame = true;
                     _robotController.UnBlockOtherOperations(RobotStateAndOperations.GameOperation_ExitGame);
                 });

            _eventBus.Subscribe<GameShutdownStartedEvent>(
                this,
                evt =>
                {
                    _logger.Info($"GameShutdownStartedEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                    _robotController.BlockOtherOperations(RobotStateAndOperations.GameOperation_ExitGame);
                });
        }
    }
}
