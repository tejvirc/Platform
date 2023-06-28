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
        private Timer _ResponsibleGamingTimer;
        private bool _gameIsRunning;
        private bool _isTimeLimitDialogVisible;
        private bool disposedValue;

        public ResponsibleGamingOperations(IEventBus eventBus, RobotLogger logger, Automation automator, RobotController robotController)
        {
            _eventBus = eventBus;
            _logger = logger;
            _automator = automator;
            _robotController = robotController;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void Reset()
        {
            disposedValue = false;
        }

        public void Execute()
        {
            _logger.Info("Responsible Gaming Operations Has Been Initiated!", GetType().Name);
            SubscribeToEvents();

            if (IsRegularRobots())
            {
                return;
            }
            _ResponsibleGamingTimer = new Timer(
                                    (sender) =>
                                    {
                                        RequestResponsibleGaming();
                                    },
                                    null,
                                    _robotController.Config.Active.IntervalRgSet,
                                    _robotController.Config.Active.IntervalRgSet);
        }

        public void Halt()
        {
            _logger.Info("Halt Request is Received!", GetType().Name);
            _eventBus.UnsubscribeAll(this);
            _ResponsibleGamingTimer?.Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_ResponsibleGamingTimer is not null)
                    {
                        _ResponsibleGamingTimer.Dispose();
                    }
                    _ResponsibleGamingTimer = null;
                }
                disposedValue = true;
            }
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

        private bool IsRegularRobots()
        {
            return _robotController.InProgressRequests.Contains(RobotStateAndOperations.RegularMode);
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<TimeLimitDialogVisibleEvent>(
                 this,
                 evt =>
                 {
                     _logger.Info($"TimeLimitDialogVisibleEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);

                     _automator.DismissTimeLimitDialog();
                 });
            _eventBus.Subscribe<TimeLimitDialogHiddenEvent>(
                this,
                evt =>
                {
                    _logger.Info($"TimeLimitDialogHiddenEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
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
    }
}
