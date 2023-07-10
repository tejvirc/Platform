namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using System;
    using System.Threading;

    internal class ResponsibleGamingOperations : IRobotOperations
    {
        private readonly IEventBus _eventBus;
        private readonly RobotLogger _logger;
        private readonly Automation _automator;
        private readonly RobotController _robotController;
        private readonly IGameService _gameService;
        private Timer _ResponsibleGamingTimer;
        private bool _gameIsRunning;
        private bool _isTimeLimitDialogVisible;
        private bool _disposed;

        public ResponsibleGamingOperations(IEventBus eventBus, RobotLogger logger, Automation automator, RobotController robotController, IGameService gameService)
        {
            _eventBus = eventBus;
            _logger = logger;
            _automator = automator;
            _robotController = robotController;
            _gameService = gameService;
        }

        ~ResponsibleGamingOperations() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        public void Execute()
        {
            SubscribeToEvents();

            if (_robotController.Config.Active.IntervalRgSet == 0)
            {
                _logger.Info("Responsible Gaming Operations Has NOT Been Initiated!", GetType().Name);
                return;
            }

            _logger.Info("Responsible Gaming Operations Has Been Initiated!", GetType().Name);

            _ResponsibleGamingTimer = new Timer(
                                    (sender) =>
                                    {
                                        RequestResponsibleGaming();
                                    },
                                    null,
                                    _robotController.Config.Active.IntervalRgSet,
                                    _robotController.Config.Active.IntervalRgSet);
        }

        public void Reset()
        {
            _disposed = false;
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
                if (_ResponsibleGamingTimer is not null)
                {
                    _ResponsibleGamingTimer.Dispose();
                }
                _ResponsibleGamingTimer = null;
                _eventBus.UnsubscribeAll(this);
            }
            _disposed = true;
        }

        private void RequestResponsibleGaming()
        {
            if (!_gameIsRunning)
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
                     _isTimeLimitDialogVisible = true;
                     DismissTimeLimitDialog();
                 });

            _eventBus.Subscribe<TimeLimitDialogHiddenEvent>(
                this,
                evt =>
                {
                    _logger.Info($"TimeLimitDialogHiddenEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                    _isTimeLimitDialogVisible = false;
                });

            _eventBus.Subscribe<GameInitializationCompletedEvent>(
                this,
                _ =>
                {
                    _logger.Info($"GameInitializationCompletedEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                    _gameIsRunning = true;
                });
            _eventBus.Subscribe<GameProcessExitedEvent>(
                 this,
                 evt =>
                 {
                     _logger.Info($"GameProcessExitedEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                     _gameIsRunning = false;
                 });
        }

        private void DismissTimeLimitDialog()
        {
            _automator.DismissTimeLimitDialog(_isTimeLimitDialogVisible);
        }
    }
}