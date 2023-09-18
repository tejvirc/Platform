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
        private readonly Automation _automator;
        private readonly IEventBus _eventBus;
        private readonly RobotController _robotController;
        private readonly RobotLogger _logger;
        private bool disposedValue;
        private GameOperations _gameOperation;
        private Timer _ResponsibleGamingTimer;

        public ResponsibleGamingOperations(IEventBus eventBus, RobotLogger logger, Automation automator, RobotController robotController, GameOperations gameOperation)
        {
            _gameOperation = gameOperation;
            _eventBus = eventBus;
            _logger = logger;
            _automator = automator;
            _robotController = robotController;
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
                _logger.Info("Regular Robot is running, skipping exectuion of RequestResponsibleGaming.", GetType().Name);
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

        private bool IsRegularRobots()
        {
            return _robotController.InProgressRequests.Contains(RobotStateAndOperations.RegularMode);
        }

        private void RequestResponsibleGaming()
        {
            if (!_gameOperation.GameIsRunning)
            {
                _logger.Info($"_gameOperation.GameIsRunning was false skipping SetResponsibleGamingTimeElapsed call, Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
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
                     _automator.DismissTimeLimitDialog(true);
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
                    _gameOperation.GameIsRunning = true;
                });
        }
    }
}