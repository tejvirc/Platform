namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Test.Automation;
    using Aristocrat.Monaco.Kernel;
    using System;
    using System.Threading;
    using Aristocrat.Monaco.Application.Contracts.OperatorMenu;

    internal class AuditMenuOperations : IRobotOperations
    {
        private readonly IEventBus _eventBus;
        private readonly Automation _automator;
        private readonly RobotLogger _logger;
        private readonly StateChecker _sc;
        private readonly RobotController _robotController;
        private Timer _loadAuditMenuTimer;
        private Timer _exitAuditMenuTimer;
        private bool _disposed;

        public AuditMenuOperations(IEventBus eventBus, RobotLogger logger, Automation automator, StateChecker sc, RobotController robotController)
        {
            _sc = sc;
            _automator = automator;
            _logger = logger;
            _eventBus = eventBus;
            _robotController = robotController;
        }

        ~AuditMenuOperations() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Reset()
        {
            _disposed = false;
            _automator.ExitAuditMenu();
            _robotController.InProgressRequests.TryRemove(RobotStateAndOperations.AuditMenuOperation);
        }

        public void Execute()
        {
            _logger.Info("AuditMenuOperations Has Been Initiated!", GetType().Name);
            SubscribeToEvents();
            if (_robotController.Config.Active.IntervalLoadAuditMenu == 0)
            {
                return;
            }
            _loadAuditMenuTimer = new Timer(
                (sender) =>
                {
                    RequestAuditMenu();
                },
                null,
                _robotController.Config.Active.IntervalLoadAuditMenu,
                _robotController.Config.Active.IntervalLoadAuditMenu);
        }

        public void Halt()
        {
            _logger.Info("Halt Request is Received!", GetType().Name);
            _automator.ExitAuditMenu();
            _loadAuditMenuTimer?.Dispose();
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
                if (_loadAuditMenuTimer is not null)
                {
                    _loadAuditMenuTimer.Dispose();
                }
                _loadAuditMenuTimer = null;
                if (_exitAuditMenuTimer is not null)
                {
                    _exitAuditMenuTimer.Dispose();
                }
                _exitAuditMenuTimer = null;
                _eventBus.UnsubscribeAll(this);
            }
            _disposed = true;
        }

        private void RequestAuditMenu()
        {
            if (!IsValid())
            {
                return;
            }
            _logger.Info("Requesting Audit Menu", GetType().Name);
            _automator.LoadAuditMenu();
            _exitAuditMenuTimer = new Timer(
                (sender) =>
                {
                    _automator.ExitAuditMenu();
                    _exitAuditMenuTimer.Dispose();
                },
                null,
                Constants.AuditMenuDuration,
                Timeout.Infinite);
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<OperatorMenuEnteredEvent>(
                this,
                _ =>
                {
                    _robotController.InProgressRequests.TryAdd(RobotStateAndOperations.AuditMenuOperation);
                });

            _eventBus.Subscribe<OperatorMenuExitedEvent>(
                this,
                _ =>
                {
                    _robotController.InProgressRequests.TryRemove(RobotStateAndOperations.AuditMenuOperation);
                });
        }

        private bool IsValid()
        {
            var IsForceGameExitInProgress = _robotController.InProgressRequests.Contains(RobotStateAndOperations.LobbyOperation_ForceGameExit);
            var isCashoutOperationInProgress = _robotController.InProgressRequests.Contains(RobotStateAndOperations.CashoutOperation);
            var isLoadGameInProgress = _robotController.InProgressRequests.Contains(RobotStateAndOperations.GameOperation_LoadGame);
            var isAuditMenuInProgress = _robotController.InProgressRequests.Contains(RobotStateAndOperations.AuditMenuOperation);
            var isLockUpInProgress = _robotController.InProgressRequests.Contains(RobotStateAndOperations.LockUpOperation);
            var isOutOfOperatingHoursInProgress = _robotController.InProgressRequests.Contains(RobotStateAndOperations.OperatingHoursOperation);
            return !IsForceGameExitInProgress && !isCashoutOperationInProgress && !isLoadGameInProgress && !isAuditMenuInProgress && !isLockUpInProgress && !isOutOfOperatingHoursInProgress && _sc.AuditMenuOperationValid;
        }
    }
}
