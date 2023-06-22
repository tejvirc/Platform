namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Kernel;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal class CashoutOperations : IRobotOperations
    {
        private readonly IEventBus _eventBus;
        private readonly RobotLogger _logger;
        private readonly LobbyStateChecker _stateChecker;
        private readonly RobotController _robotController;
        private Timer _actionCashoutTimer;
        private bool _disposed;

        public CashoutOperations(IEventBus eventBus, RobotLogger logger, LobbyStateChecker sc, RobotController robotController)
        {
            _stateChecker = sc;
            _logger = logger;
            _eventBus = eventBus;
            _robotController = robotController;
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
                                _robotController.Config.ActiveGameMode.IntervalCashOut,
                                _robotController.Config.ActiveGameMode.IntervalCashOut);
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
            var isBlocked = _robotController.IsBlockedByOtherOperation(new List<RobotStateAndOperations>());
            return !isBlocked && _stateChecker.CashoutOperationValid && !_stateChecker.IsDisabled;
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<CashoutRequestEvent>(this, HandleEvent);
            _eventBus.Subscribe<TransferOutFailedEvent>(this,
                 _ =>
                 {
                     _robotController.UnBlockOtherOperations(RobotStateAndOperations.CashoutOperation);
                    _logger.Info($"TransferOutFailedEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                 });
            _eventBus.Subscribe<TransferOutCompletedEvent>(this,
                _ =>
                {
                    _robotController.UnBlockOtherOperations(RobotStateAndOperations.CashoutOperation);
                    _logger.Info($"TransferOutCompletedEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                });
            _eventBus.Subscribe<CashOutAbortedEvent>(this,
                _ =>
                {
                    _robotController.UnBlockOtherOperations(RobotStateAndOperations.CashoutOperation);
                    _logger.Info($"CashOutAbortedEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                });
            _eventBus.Subscribe<CashOutStartedEvent>(this,
                _ =>
                {
                    _robotController.BlockOtherOperations(RobotStateAndOperations.CashoutOperation);
                    _logger.Info($"CashOutStartedEvent Got Triggered! Game: [{_robotController.Config.CurrentGame}]", GetType().Name);
                });
        }

        private void RequestCashOut()
        {
            if (!IsValid())
            {
                return;
            }
            _logger.Info("Requesting Cashout", GetType().Name);
            _eventBus.Publish(new CashOutButtonPressedEvent());
        }
    }
}
