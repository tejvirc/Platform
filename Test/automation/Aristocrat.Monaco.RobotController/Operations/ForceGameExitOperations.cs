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

    internal class ForceGameExitOperations : IRobotOperations
    {
        private readonly IEventBus _eventBus;
        private readonly Automation _automator;
        private readonly IPropertiesManager _propertyManager;
        private readonly RobotLogger _logger;
        private readonly StateChecker _stateChecker;
        private readonly RobotController _robotController;
        private readonly IGameService _gameService;
        private Timer _forceGameExitTimer;
        private bool _disposed;
        private bool _exitWhenIdle;
        private bool _forceGameExitIsInProgress;
        private bool _gameIsRunning;

        public ForceGameExitOperations(IEventBus eventBus, RobotLogger logger, Automation automator, StateChecker sc, IPropertiesManager pm, RobotController robotController, IGameService gameService)
        {
            _stateChecker = sc;
            _automator = automator;
            _logger = logger;
            _eventBus = eventBus;
            _propertyManager = pm;
            _robotController = robotController;
            _gameService = gameService;
        }

        ~ForceGameExitOperations() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Execute()
        {

            if (_robotController.Config.Active.IntervalLobby == 0 || !_robotController.Config.Active.TestRecovery)
            {
                _logger.Info("ForceGameExitOperations Has Not Been Initiated!", GetType().Name);

                return;
            }

            SubscribeToEvents();

            _forceGameExitTimer = new Timer(
                               (sender) =>
                               {
                                   RequestForceExitToLobby();
                               },
                               null,
                               _robotController.Config.Active.IntervalLobby,
                               _robotController.Config.Active.IntervalLobby);
        }

        public void Reset()
        {
            _disposed = false;
            _exitWhenIdle = false;
            _gameIsRunning = _gameService.Running;
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
                if (_forceGameExitTimer is not null)
                {
                    _forceGameExitTimer.Dispose();
                }
                _forceGameExitTimer = null;
                _eventBus.UnsubscribeAll(this);
            }
            _disposed = true;
        }

        private void RequestForceExitToLobby(bool skipTestRecovery = false)
        {
            if (!IsRequestForceExitToLobbyValid(skipTestRecovery))
            {
                return;
            }
            _logger.Info("ForceGameExit Requested Received!", GetType().Name);
            _forceGameExitIsInProgress = true;
            _exitWhenIdle = false;
            _automator.ForceGameExit(Constants.GdkRuntimeHostName);
            _robotController.BlockOtherOperations(RobotStateAndOperations.GameExiting);
        }

        private bool IsRequestForceExitToLobbyValid(bool skipTestRecovery)
        {
            var isBlocked = _robotController.IsBlockedByOtherOperation(new List<RobotStateAndOperations>());
            var isGeneralRule = (_gameIsRunning && !_stateChecker.IsGameLoading && !_forceGameExitIsInProgress && !_exitWhenIdle && (_robotController.Config.Active.TestRecovery || skipTestRecovery));
            return !isBlocked && isGeneralRule;
        }


        private void RequestGame(bool selectNextGame = false)
        {
            _eventBus.Publish(new GameLoadRequestEvent() { SelectNextGame = selectNextGame} );
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<GameInitializationCompletedEvent>(
                this,
                _ =>
                {
                    _logger.Info($"GameInitializationCompletedEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                    _gameIsRunning = true;
                    BalanceCheckWithDelay(Constants.BalanceCheckDelayDuration);
                });

            _eventBus.Subscribe<GameIdleEvent>(
                 this,
                 _ =>
                 {
                     _logger.Info($"GameIdleEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                     BalanceCheckWithDelay(Constants.BalanceCheckDelayDuration);
                     HandleExitToLobbyRequest();
                 });
            _eventBus.Subscribe<GameProcessExitedEvent>(
                 this,
                 evt =>
                 {
                     _gameIsRunning = false;
                     if (evt.Unexpected)
                     {
                         if (!_forceGameExitIsInProgress)
                         {
                             _logger.Error($"GameProcessExitedEvent-Unexpected Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                             _robotController.Enabled = false;
                             return;
                         }
                         _logger.Info($"GameProcessExitedEvent-Unexpected-ForceGameExit Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                         _forceGameExitIsInProgress = false;
                         _exitWhenIdle = true;
                     }
                 });
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

        private void HandleExitToLobbyRequest()
        {
            if (_exitWhenIdle)
            {
                _robotController.BlockOtherOperations(RobotStateAndOperations.GameExiting);
                if (!IsExitToLobbyWhenIdleValid())
                {
                    _robotController.UnBlockOtherOperations(RobotStateAndOperations.GameExiting);
                    return;
                }
                _logger.Info($"ExitToLobby Request Is Received! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                _automator.RequestGameExit();
                _exitWhenIdle = false;
                RequestGame(false);
                //GameProcessExitedEvent gets trigered
            }
        }

        private bool IsExitToLobbyWhenIdleValid()
        {
            return _gameIsRunning && (_stateChecker.IsIdle || _stateChecker.IsPresentationIdle) && _exitWhenIdle && !_forceGameExitIsInProgress;
        }
    }
}