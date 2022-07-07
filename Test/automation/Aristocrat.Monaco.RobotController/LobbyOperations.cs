namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using System;
    using System.Threading;

    internal class LobbyOperations : IRobotOperations
    {
        private readonly IEventBus _eventBus;
        private readonly Automation _automator;
        private readonly RobotLogger _logger;
        private readonly StateChecker _sc;
        private readonly RobotController _robotController;
        private Timer _forceGameExitTimer;
        private bool _disposed;

        public LobbyOperations(IEventBus eventBus, RobotLogger logger, Automation automator, StateChecker sc, RobotController controller)
        {
            _sc = sc;
            _automator = automator;
            _logger = logger;
            _eventBus = eventBus;
            _robotController = controller;
        }

        ~LobbyOperations() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Reset()
        {
            _disposed = false;
        }

        public void Execute()
        {
            _logger.Info("LobbyOperations Has Been Initiated!", GetType().Name);
            SubscribeToEvents();
            _forceGameExitTimer = new Timer(
                               (sender) =>
                               {
                                   RequestForceGameExit();
                               },
                               null,
                               _robotController.Config.Active.IntervalLobby,
                               _robotController.Config.Active.IntervalLobby);
        }

        public void Halt()
        {
            _logger.Info("Halt Request is Received!", GetType().Name);
            _forceGameExitTimer?.Dispose();
            _eventBus.UnsubscribeAll(this);
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

        private void RequestForceGameExit()
        {
            if (!IsForceGameExitValid())
            {
                return;
            }
            _logger.Info("ForceGameExit Requested Received!", GetType().Name);
            _robotController.InProgressRequests.TryAdd(RobotStateAndOperations.LobbyOperation_ForceGameExit);
            _automator.ForceGameExit(Constants.GdkRuntimeHostName);
        }

        private bool IsForceGameExitValid()
        {
            var isLoadGameInProgress = _robotController.InProgressRequests.Contains(RobotStateAndOperations.GameOperation_LoadGame);
            var IsForceGameExitInProgress = _robotController.InProgressRequests.Contains(RobotStateAndOperations.LobbyOperation_ForceGameExit);
            var isAuditMenuInProgress = _robotController.InProgressRequests.Contains(RobotStateAndOperations.AuditMenuOperation);
            var isLockUpInProgress = _robotController.InProgressRequests.Contains(RobotStateAndOperations.LockUpOperation);
            var isOutOfOperatingHoursInProgress = _robotController.InProgressRequests.Contains(RobotStateAndOperations.OperatingHoursOperation);
            var isCashoutOperationInProgress = _robotController.InProgressRequests.Contains(RobotStateAndOperations.CashoutOperation);
            var GeneralRule = (_sc.IsGame && !_sc.IsGameLoading && _robotController.Config.Active.TestRecovery);
            return !isCashoutOperationInProgress && !isLoadGameInProgress && !IsForceGameExitInProgress && !isAuditMenuInProgress && !isLockUpInProgress && !isOutOfOperatingHoursInProgress && GeneralRule;
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<GameInitializationCompletedEvent>(
                this,
                _ =>
                {
                    ResetForceGameExitTimer();
                });
            _eventBus.Subscribe<GameRequestFailedEvent>(
                this,
                _ =>
                {
                    _robotController.InProgressRequests.TryRemove(RobotStateAndOperations.LobbyOperation_ForceGameExit);
                });
        }

        private void ResetForceGameExitTimer()
        {
            _forceGameExitTimer.Change(_robotController.Config.Active.IntervalLobby, _robotController.Config.Active.IntervalLobby);
        }
    }
}
