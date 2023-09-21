namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts.HandCount;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Kernel;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    internal class CashoutOperations : IRobotOperations
    {
        private readonly IEventBus _eventBus;
        private readonly RobotLogger _logger;
        private readonly StateChecker _stateChecker;
        private readonly RobotController _robotController;
        private Timer _actionCashoutTimer;
        private bool _disposed;

        private readonly int CashoutDialogDismiss = (int)TimeSpan.FromSeconds(3).TotalMilliseconds;

        public CashoutOperations(IEventBus eventBus, RobotLogger logger, StateChecker sc, RobotController robotController)
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
            if (_robotController.Config.Active.IntervalCashOut == 0)
            {
                _logger.Info("CashoutOperations is Disabled!", GetType().Name);
                return;
            }

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
            _eventBus.UnsubscribeAll(this);
            _actionCashoutTimer?.Dispose();
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

            CashoutBannerSupport();
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

        private void CashoutBannerSupport()
        {
            _eventBus.Subscribe<CashoutAmountAuthorizationRequestedEvent>(this,
                evt =>
                {
                    _robotController.BlockOtherOperations(RobotStateAndOperations.CashoutBannerSupport);
                    Task.Delay(CashoutDialogDismiss).ContinueWith(task =>
                    {
                        var cashOut = GetRandomBoolean();

                        _eventBus.Publish(new CashoutAmountAuthorizationReceivedEvent(cashOut));
                        _robotController.UnBlockOtherOperations(RobotStateAndOperations.CashoutBannerSupport);
                    });
                });
        }

        private bool GetRandomBoolean() => new Random((int)DateTime.Now.Ticks).Next() % 2 != 0;
    }
}
