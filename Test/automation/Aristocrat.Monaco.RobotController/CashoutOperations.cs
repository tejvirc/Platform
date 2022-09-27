namespace Aristocrat.Monaco.RobotController
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Operations;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;

    internal sealed class CashoutOperations : IRobotOperations
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

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_actionCashoutTimer != null)
            {
                _actionCashoutTimer.Dispose();
                _actionCashoutTimer = null;
            }

            _eventBus.UnsubscribeAll(this);
            _disposed = true;
        }

        public void Reset()
        {
        }

        public void Execute()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(CashoutOperations));
            }

            _logger.Info("CashoutOperations Has Been Initiated!", GetType().Name);
            SubscribeToEvents();
            _actionCashoutTimer = new Timer(
                                _ => RequestCashOut(),
                                null,
                                _robotController.Config.Active.IntervalCashOut,
                                _robotController.Config.Active.IntervalCashOut);
        }

        public void Halt()
        {
            _logger.Info("Halt Request is Received!", GetType().Name);
            _eventBus.UnsubscribeAll(this);
            _actionCashoutTimer?.Halt();
        }

        private void HandleEvent(CashoutRequestEvent obj)
        {
            RequestCashOut();
        }

        private bool IsValid()
        {
            var isBlocked = _robotController.IsBlockedByOtherOperation(new List<RobotStateAndOperations>());
            return !isBlocked && _sc.CashoutOperationValid;
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
