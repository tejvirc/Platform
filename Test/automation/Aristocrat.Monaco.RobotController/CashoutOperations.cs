namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Operations;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using System;
    using System.Threading;

    internal class CashoutOperations : IRobotOperations
    {
        private readonly IEventBus _eventBus;
        private readonly RobotLogger _logger;
        private readonly StateChecker _sc;
        private readonly RobotController _robotController;
        private readonly Automation _automator;
        private Timer _actionCashoutTimer;
        private bool _disposed;

        public CashoutOperations(IEventBus eventBus, RobotLogger logger, StateChecker sc, RobotController robotController, Automation automator)
        {
            _sc = sc;
            _logger = logger;
            _eventBus = eventBus;
            _robotController = robotController;
            _automator = automator;
        }

        ~CashoutOperations() => Dispose(false);

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
            _logger.Info("CashoutOperations Has Been Initiated!", GetType().Name);
            SubscribeToEvents();
            _actionCashoutTimer = new Timer(
                                (sender) =>
                                {
                                    RequestCashOut();
                                },
                                null,
                                _robotController.Config.Active.IntervalCashOut,
                                _robotController.Config.Active.IntervalCashOut);
        }

        public void Halt()
        {
            _logger.Info("Halt Request is Received!", GetType().Name);
            _actionCashoutTimer?.Dispose();
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
                if (_actionCashoutTimer is not null)
                {
                    _actionCashoutTimer.Dispose();
                }
                _actionCashoutTimer = null;
                _eventBus.UnsubscribeAll(this);
            }
            _disposed = true;
        }

        private void HandleEvent(CashoutRequestEvent obj)
        {
            RequestCashOut();
        }

        private bool IsValid()
        {
            var IsForceGameExitInProgress = _robotController.InProgressRequests.Contains(RobotStateAndOperations.LobbyOperation_ForceGameExit);
            var isCashoutOperationInProgress = _robotController.InProgressRequests.Contains(RobotStateAndOperations.CashoutOperation);
            var isLoadGameInProgress = _robotController.InProgressRequests.Contains(RobotStateAndOperations.GameOperation_LoadGame);
            var isAuditMenuInProgress = _robotController.InProgressRequests.Contains(RobotStateAndOperations.AuditMenuOperation);
            var isLockUpInProgress = _robotController.InProgressRequests.Contains(RobotStateAndOperations.LockUpOperation);
            var isOutOfOperatingHoursInProgress = _robotController.InProgressRequests.Contains(RobotStateAndOperations.OperatingHoursOperation);
            return !IsForceGameExitInProgress && !isLoadGameInProgress && !isAuditMenuInProgress && !isLockUpInProgress && !isOutOfOperatingHoursInProgress && isCashoutOperationInProgress && _sc.CashoutOperationValid;
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<CashoutRequestEvent>(this, HandleEvent);
            _eventBus.Subscribe<TransferOutFailedEvent>(this,
                 _ =>
                 {
                     _robotController.InProgressRequests.TryRemove(RobotStateAndOperations.CashoutOperation);
                 });
            _eventBus.Subscribe<TransferOutCompletedEvent>(this,
                _ =>
                {
                    _robotController.InProgressRequests.TryRemove(RobotStateAndOperations.CashoutOperation);
                });
            _eventBus.Subscribe<CashOutAbortedEvent>(this,
                _ =>
                {
                    _robotController.InProgressRequests.TryRemove(RobotStateAndOperations.CashoutOperation);
                });
            _eventBus.Subscribe<CashOutStartedEvent>(this,
                _ =>
                {
                    _robotController.InProgressRequests.TryAdd(RobotStateAndOperations.CashoutOperation);
                });
            _eventBus.Subscribe<GameInitializationCompletedEvent>(
                 this,
                 evt =>
                 {
                     _automator.EnableCashOut(false);
                 });
            _eventBus.Subscribe<SystemDisableAddedEvent>(
                 this,
                 _ =>
                 {
                     _automator.EnableCashOut(false);
                 });
            _eventBus.Subscribe<OperatingHoursExpiredEvent>(
                this,
                _ =>
                {
                    _automator.EnableCashOut(false);
                });
        }

        private void RequestCashOut()
        {
            if (!IsValid())
            {
                return;
            }
            _logger.Info("Requesting Cashout", GetType().Name);
            _automator.EnableCashOut(true);
            _eventBus.Publish(new CashOutButtonPressedEvent());
        }
    }
}
