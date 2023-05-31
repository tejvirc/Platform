namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts.Handpay;
    using Aristocrat.Monaco.Accounting.Contracts.Vouchers;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.Button;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    internal class BalanceOperations : IRobotOperations
    {
        private readonly IEventBus _eventBus;
        private readonly StateChecker _stateChecker;
        private readonly RobotLogger _logger;
        private readonly Automation _automator;
        private readonly RobotController _robotController;
        private readonly IBank _bank;
        private Timer _balanceCheckTimer;
        private bool _disposed;

        public BalanceOperations(IEventBus eventBus,IBank bank, RobotLogger logger, Automation automator, StateChecker sc, RobotController robotController)
        {
            _stateChecker = sc;
            _automator = automator;
            _logger = logger;
            _eventBus = eventBus;
            _robotController = robotController;
            _bank = bank;
        }

        ~BalanceOperations() => Dispose(false);

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
            _logger.Info("BalanceOperations Has Been Initiated!", GetType().Name);
            SubscribeToEvents();
            _balanceCheckTimer = new Timer(
                                (sender) =>
                                {
                                    EnsureEnoughCreditToPlay();
                                },
                                null,
                                _robotController.Config.Active.IntervalBalanceCheck,
                                _robotController.Config.Active.IntervalBalanceCheck);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                if (_balanceCheckTimer is not null)
                {
                    _balanceCheckTimer.Dispose();
                }
                _balanceCheckTimer = null;
                _eventBus.UnsubscribeAll(this);
            }
            _disposed = true;
        }

        //zhg: changed name
        private void EnsureEnoughCreditToPlay()
        {
            if (!IsValid())
            {
                return;
            }
            _logger.Info($"BalanceCheck Requested.", GetType().Name);
            CheckNegativeBalance(_bank, _logger);
            InsertCredit();
        }

        public void Halt()
        {
            _logger.Info("Halt Request is Received!", GetType().Name);
            _eventBus.UnsubscribeAll(this);
            _balanceCheckTimer?.Dispose();
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<BalanceCheckEvent>(this, HandleEvent);
            _eventBus.Subscribe<VoucherRejectedEvent>(this, evt =>
            {
                if (evt.Transaction.Exception.Equals((int)VoucherInExceptionCode.CreditInLimitExceeded) ||
                    evt.Transaction.Exception.Equals((int)VoucherInExceptionCode.LaundryLimitExceeded))
                {
                    _eventBus.Publish(new CashOutButtonPressedEvent());
                }
            });

            _eventBus.Subscribe<HandpayStartedEvent>(this, evt =>
            {
                if (evt.Handpay == HandpayType.GameWin ||
                     evt.Handpay == HandpayType.CancelCredit)
                {
                    _logger.Info("Keying off large win", GetType().Name);
                    Task.Delay(1000).ContinueWith(_ => _automator.JackpotKeyoff()).ContinueWith(_ => _eventBus.Publish(new DownEvent((int)ButtonLogicalId.Button30)));
                }
            });
            _eventBus.Subscribe<TransferOutCompletedEvent>(this, evt =>
            {
                _logger.Info("TransferOutCompletedEvent Got Triggered!", GetType().Name);
                EnsureEnoughCreditToPlay();
            });
        }

        private void HandleEvent(BalanceCheckEvent obj)
        {
            EnsureEnoughCreditToPlay();
        }

        private bool IsValid()
        {
            var isBlocked = _robotController.IsBlockedByOtherOperation(new List<RobotStateAndOperations>());
            return !isBlocked && _stateChecker.BalanceOperationValid;
        }

        private void InsertCredit()
        {
            var bankBalanceInDollars = CurrencyExtensions.MillicentsToDollars(_bank.QueryBalance());
            var minBalanceInDollars = CurrencyExtensions.CentsToMillicents(_robotController.Config.GetMinimumBalance());
            var enoughBlanace = !CurrencyExtensions.IsBelowMinimum(bankBalanceInDollars, minBalanceInDollars);
            var hasEdgeCase = _robotController.Config?.Active?.InsertCreditsDuringGameRound == true;
            //inserting credits can lead to race conditions that make the platform not update the runtime balance
            //we now support inserting credits during game round for some jurisdictions
            if (enoughBlanace || (!_stateChecker.IsIdle && !hasEdgeCase))
            {
                return;
            }
            _logger.Info($"Insufficient balance.", GetType().Name);
            _automator.InsertDollars(_robotController.Config.GetDollarsInserted());
        }

        private void CheckNegativeBalance(IBank _bank, RobotLogger Logger)
        {
            Logger.Info($"Platform balance: {_bank.QueryBalance()}", GetType().Name);
            if (_bank.QueryBalance() < 0)
            {
                Logger.Fatal("NEGATIVE BALANCE DETECTED", GetType().Name);
                throw new Exception($"NEGATIVE BALANCE DETECTED");
            }
        }
    }
}
